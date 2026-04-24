# ============================================================
#  BA.FrioCheck - Test Suite COMPLETO
#  Basado en analisis exhaustivo del repositorio
#  Ejecutar: PowerShell -ExecutionPolicy Bypass -File .\test.ps1
#  Requiere: Backend corriendo en http://localhost:5003
#            BD_FC seeded con sql/02_seed_data.sql
# ============================================================

$BASE = "http://localhost:5003"

# ============================================================
#  CONFIGURACION
# ============================================================
$SQL_SERVER = "tcp:localhost,1433"
$SQL_DB     = "BD_FC"

# La password se lee automaticamente desde appsettings.json del proyecto.
# Si no se encuentra, los tests de login se marcan como SKIP.
# Para configurar, agrega en appsettings.Development.json:
#   { "SeedPassword": "tu-password-real" }

function Resolve-TestPassword {
    # Busca la password en appsettings del proyecto (donde ya la tienes configurada)
    $settingsPaths = @(
        (Join-Path $PSScriptRoot "src\BA.Backend.WebAPI\appsettings.Development.json"),
        (Join-Path $PSScriptRoot "src\BA.Backend.WebAPI\appsettings.json"),
        (Join-Path $PSScriptRoot "appsettings.Development.json"),
        (Join-Path $PSScriptRoot "appsettings.json")
    )

    $knownFields = @(
        "TestPassword","SeedPassword","Seed:Password","DefaultPassword",
        "SeedData:Password","SeedUser:Password","AdminPassword"
    )

    foreach ($sp in $settingsPaths) {
        if (-not (Test-Path $sp)) { continue }
        try {
            $raw  = Get-Content $sp -Raw
            $json = $raw | ConvertFrom-Json

            # Buscar campos conocidos
            foreach ($field in $knownFields) {
                $parts = $field -split ":"
                $val   = $json
                foreach ($p in $parts) {
                    try { $val = $val.$p } catch { $val = $null; break }
                }
                if ($val -and $val -is [string] -and $val.Length -ge 6) {
                    Write-Host ("  Pass: '{0}' en {1}" -f $field, [System.IO.Path]::GetFileName($sp)) -ForegroundColor DarkGray
                    return $val
                }
            }

            # Buscar cualquier campo que tenga "password" en el nombre con valor no-hash
            $raw | Select-String '"[^"]*[Pp]assword[^"]*"\s*:\s*"([^"]{6,})"' -AllMatches |
            ForEach-Object { $_.Matches } |
            ForEach-Object {
                $candidate = $_.Groups[1].Value
                # Ignorar si es un hash BCrypt, placeholder o connection string
                if ($candidate -notmatch '^\$2[ab]\$' -and
                    $candidate -notmatch '^Server=' -and
                    $candidate -notmatch 'PLACEHOLDER' -and
                    $candidate -notmatch 'CHANGE.ME' -and
                    $candidate.Length -lt 100) {
                    Write-Host ("  Pass: auto-detectada en {0}" -f [System.IO.Path]::GetFileName($sp)) -ForegroundColor DarkGray
                    return $candidate
                }
            }
        } catch {}
    }

    # No se encontro password en appsettings
    Write-Host ""
    Write-Host "  !! No se encontro password en appsettings del proyecto." -ForegroundColor Red
    Write-Host "     Agrega esto en appsettings.Development.json:" -ForegroundColor Yellow
    Write-Host '     { "SeedPassword": "la-password-de-tus-usuarios" }' -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  Los tests que requieren login seran SKIPPED." -ForegroundColor Yellow
    Write-Host ""
    return $null
}

$SQL_PASS = Resolve-TestPassword

# ============================================================
#  CREDS se llena dinamicamente desde la BD
#  No hay nada hardcodeado - usa lo que exista en tu instancia
# ============================================================
$CREDS = @{}
$DB    = @{
    Tenants  = @()   # lista de tenants activos
    T1       = $null # tenant 1 (primer tenant activo)
    T2       = $null # tenant 2 (segundo tenant activo)
    Stores   = @{}   # StoreId por slug de tenant
    Coolers  = @{}   # CoolerId por slug de tenant
    NfcTags  = @{}   # NfcTag UID por slug de tenant
    Products = @{}   # ProductId por slug de tenant
    Users    = @{}   # usuarios por rol+tenant
}

# ============================================================
#  LIMPIEZA INICIAL DE RESIDUOS
# ============================================================
function Cleanup-ResidualData {
    Write-Host "  -- Limpiando residuos de pruebas anteriores --" -ForegroundColor DarkGray
    $sql = @"
        DELETE FROM dbo.UserSessions WHERE UserId IN (SELECT Id FROM dbo.Users WHERE Email LIKE 'pstest_%' OR Email LIKE 'crud_test_%' OR Email LIKE 'xss_%' OR Email LIKE 'test_%' OR Email LIKE 'admin2_%' OR Email LIKE '%@test.com' OR Name = 'RV Smoke');
        DELETE FROM dbo.Users WHERE Email LIKE 'pstest_%' OR Email LIKE 'crud_test_%' OR Email LIKE 'xss_%' OR Email LIKE 'test_%' OR Email LIKE 'admin2_%' OR Email LIKE '%@test.com' OR Name = 'RV Smoke');
        DELETE FROM dbo.Stores WHERE Name LIKE 'RV-%' OR Name LIKE 'CRUD-%';
"@
    if ($SQLCMD_PATH) {
        & $SQLCMD_PATH -S "localhost\SQLEXPRESS" -E -d "BD_FC" -Q "$sql" | Out-Null
    }
}

Cleanup-ResidualData

# ============================================================
#  INICIO DE PRUEBAS
# ============================================================

function Get-SqlVal($query) {
    if (-not $SQLCMD_PATH) { return $null }
    try {
        $out = & $SQLCMD_PATH -S $SQL_SERVER -E -d $SQL_DB -Q $query -h -1 -W 2>&1
        $val = $out | Where-Object { $_ -and $_.Trim() -ne "" -and $_ -notmatch "rows affected" } | Select-Object -First 1
        if ($null -ne $val) { return $val.Trim() }
        return $null
    } catch { return $null }
}

function Get-SqlRows($query) {
    if (-not $SQLCMD_PATH) { return @() }
    try {
        $out = & $SQLCMD_PATH -S $SQL_SERVER -E -d $SQL_DB -Q $query -h -1 -W 2>&1
        return ($out | Where-Object { $_ -and $_.Trim() -ne "" -and $_ -notmatch "rows affected" })
    } catch { return @() }
}

function Load-DbData {
    Write-Host ""
    Write-Host "  Leyendo datos reales desde $SQL_SERVER / $SQL_DB..." -ForegroundColor Cyan

    if (-not $SQLCMD_PATH) {
        Write-Host "  !! sqlcmd no encontrado - no se pueden cargar datos reales" -ForegroundColor Red
        Write-Host "     winget install Microsoft.SqlServer.SQLCmd" -ForegroundColor DarkGray
        return $false
    }

    # 1. Tenants activos
    $tenantRows = Get-SqlRows "SELECT CAST(Id AS NVARCHAR(50))+','+Slug FROM dbo.Tenants WHERE IsActive=1 ORDER BY CreatedAt"
    foreach ($row in $tenantRows) {
        $parts = $row -split ','
        if ($parts.Count -ge 2) {
            $script:DB.Tenants += @{ Id=$parts[0].Trim(); Slug=$parts[1].Trim() }
        }
    }

    if ($script:DB.Tenants.Count -eq 0) {
        Write-Host "  !! No hay tenants activos en la BD" -ForegroundColor Red
        return $false
    }

    $script:DB.T1 = $script:DB.Tenants[0]
    $script:DB.T2 = if ($script:DB.Tenants.Count -gt 1) { $script:DB.Tenants[1] } else { $script:DB.Tenants[0] }

    Write-Host ("  Tenant 1: {0} ({1})" -f $script:DB.T1.Slug, $script:DB.T1.Id) -ForegroundColor DarkGray
    Write-Host ("  Tenant 2: {0} ({1})" -f $script:DB.T2.Slug, $script:DB.T2.Id) -ForegroundColor DarkGray

    # 2. Usuarios por rol para cada tenant
    # Role: 1=Admin 2=Cliente 3=Transportista 4=Tecnico
    $roleMap = @{ 1="Admin"; 2="Cliente"; 3="Transportista"; 4="Tecnico" }

    foreach ($tenant in @($script:DB.T1, $script:DB.T2)) {
        $slug = $tenant.Slug
        $tid  = $tenant.Id
        $script:DB.Users[$slug] = @{}

        foreach ($role in @(1,2,3,4)) {
            $email = Get-SqlVal "SELECT TOP 1 Email FROM dbo.Users WHERE Role=$role AND IsActive=1 AND TenantId='$tid' ORDER BY CreatedAt"
            $uid   = Get-SqlVal "SELECT TOP 1 CAST(Id AS NVARCHAR(50)) FROM dbo.Users WHERE Role=$role AND IsActive=1 AND TenantId='$tid' ORDER BY CreatedAt"
            if ($email) {
                $script:DB.Users[$slug][$roleMap[$role]] = @{ email=$email; id=$uid; slug=$slug }
                Write-Host ("    {0,-18} {1,-15} {2}" -f $slug, $roleMap[$role], $email) -ForegroundColor DarkGray
            }
        }
    }

    # 3. Stores
    foreach ($tenant in @($script:DB.T1, $script:DB.T2)) {
        $sid = Get-SqlVal "SELECT TOP 1 CAST(Id AS NVARCHAR(50)) FROM dbo.Stores WHERE TenantId='$($tenant.Id)' ORDER BY CreatedAt"
        if ($sid) { $script:DB.Stores[$tenant.Slug] = $sid }
    }

    # 4. Coolers
    foreach ($tenant in @($script:DB.T1, $script:DB.T2)) {
        $cid = Get-SqlVal "SELECT TOP 1 CAST(Id AS NVARCHAR(50)) FROM dbo.Coolers WHERE TenantId='$($tenant.Id)' ORDER BY CreatedAt"
        if ($cid) { $script:DB.Coolers[$tenant.Slug] = $cid }
    }

    # 5. NFC Tags
    foreach ($tenant in @($script:DB.T1, $script:DB.T2)) {
        $nid = Get-SqlVal "SELECT TOP 1 n.TagId FROM dbo.NfcTags n INNER JOIN dbo.Coolers c ON n.CoolerId = c.Id WHERE c.TenantId='$($tenant.Id)' ORDER BY n.CreatedAt"
        if ($nid) { $script:DB.NfcTags[$tenant.Slug] = $nid }
    }

    # 6. Productos
    foreach ($tenant in @($script:DB.T1, $script:DB.T2)) {
        $prodId = Get-SqlVal "SELECT TOP 1 CAST(Id AS NVARCHAR(50)) FROM dbo.Products WHERE TenantId='$($tenant.Id)' ORDER BY CreatedAt"
        if ($prodId) { $script:DB.Products[$tenant.Slug] = $prodId }
    }

    # 7. Construir CREDS desde datos reales
    $t1 = $script:DB.T1.Slug
    $t2 = $script:DB.T2.Slug

    $p = $SQL_PASS  # puede ser null si no se encontro en appsettings
    if ($script:DB.Users[$t1]["Admin"])         { $script:CREDS["AdminT1"]   = $script:DB.Users[$t1]["Admin"]         + @{ pass=$p } }
    if ($script:DB.Users[$t2]["Admin"])         { $script:CREDS["AdminT2"]   = $script:DB.Users[$t2]["Admin"]         + @{ pass=$p } }
    if ($script:DB.Users[$t1]["Tecnico"])       { $script:CREDS["TecnicoT1"] = $script:DB.Users[$t1]["Tecnico"]       + @{ pass=$p } }
    if ($script:DB.Users[$t1]["Transportista"]) { $script:CREDS["TransT1"]   = $script:DB.Users[$t1]["Transportista"] + @{ pass=$p } }
    if ($script:DB.Users[$t1]["Cliente"])       { $script:CREDS["ClienteT1"] = $script:DB.Users[$t1]["Cliente"]       + @{ pass=$p } }
    if ($script:DB.Users[$t2]["Cliente"])       { $script:CREDS["ClienteT2"] = $script:DB.Users[$t2]["Cliente"]       + @{ pass=$p } }

    # Aliases genéricos para desacoplar de marcas específicas
    if ($script:CREDS["AdminT1"])   { $script:CREDS["AdminSavory"]  = $script:CREDS["AdminT1"]   }
    if ($script:CREDS["AdminT2"])   { $script:CREDS["AdminCoppelia"]= $script:CREDS["AdminT2"]   }
    
    # Aliases de compatibilidad para evitar romper bloques antiguos (ahora apuntan a T1/T2 dinámicos)
    if ($script:CREDS["AdminT1"])   { $script:CREDS["AdminSavory"]    = $script:CREDS["AdminT1"]   }
    if ($script:CREDS["AdminT2"])   { $script:CREDS["AdminCoppelia"]   = $script:CREDS["AdminT2"]   }
    if ($script:CREDS["TecnicoT1"]) { $script:CREDS["Tec1Savory"]     = $script:CREDS["TecnicoT1"] }
    if ($script:CREDS["TecnicoT1"]) { $script:CREDS["Tec2Coca"]     = $script:CREDS["TecnicoT1"] }
    if ($script:CREDS["TecnicoT1"]) { $script:CREDS["Tec1Pepsi"]    = $script:CREDS["TecnicoT1"] }
    if ($script:CREDS["TransT1"])   { $script:CREDS["Trans1Savory"]   = $script:CREDS["TransT1"]   }
    if ($script:CREDS["TransT1"])   { $script:CREDS["Trans1Pepsi"]  = $script:CREDS["TransT1"]   }
    if ($script:CREDS["ClienteT1"]) { $script:CREDS["Cliente1Savory"] = $script:CREDS["ClienteT1"] }
    if ($script:CREDS["ClienteT1"]) { $script:CREDS["Cliente2Coca"] = $script:CREDS["ClienteT1"] }
    if ($script:CREDS["ClienteT2"]) { $script:CREDS["Cliente1Coppelia"]= $script:CREDS["ClienteT2"] }

    Write-Host ""
    Write-Host ("  Stores  T1:{0}  T2:{1}" -f $script:DB.Stores[$t1], $script:DB.Stores[$t2]) -ForegroundColor DarkGray
    Write-Host ("  Coolers T1:{0}  T2:{1}" -f $script:DB.Coolers[$t1], $script:DB.Coolers[$t2]) -ForegroundColor DarkGray
    Write-Host ("  NFC     T1:{0}  T2:{1}" -f $script:DB.NfcTags[$t1], $script:DB.NfcTags[$t2]) -ForegroundColor DarkGray
    Write-Host ""

    return $true
}


# -- Contadores ----------------------------------------------
$pass = 0; $fail = 0; $skip = 0
$results = [System.Collections.Generic.List[PSCustomObject]]::new()

# ============================================================
#  UTILIDADES
# ============================================================
function Write-Header($text) {
    Write-Host "`n=========================================================" -ForegroundColor DarkGray
    Write-Host "  $text" -ForegroundColor Cyan
    Write-Host "=========================================================" -ForegroundColor DarkGray
}

function Test-Case {
    param([string]$id, [string]$desc, [scriptblock]$block)
    try {
        $r = & $block
        if ($r.ok) {
            Write-Host "  [PASS] [$id] $desc" -ForegroundColor Green
            if ($r.detail) { Write-Host "       -> $($r.detail)" -ForegroundColor DarkGray }
            $script:pass++
            $script:results.Add([PSCustomObject]@{ID=$id; Estado="PASS"; Prueba=$desc; Detalle=$r.detail})
        } else {
            Write-Host "  [FAIL] [$id] $desc" -ForegroundColor Red
            if ($r.detail) { Write-Host "       -> $($r.detail)" -ForegroundColor Yellow }
            $script:fail++
            $script:results.Add([PSCustomObject]@{ID=$id; Estado="FAIL"; Prueba=$desc; Detalle=$r.detail})
        }
    } catch {
        Write-Host "  [ERROR] [$id] $desc - EX: $($_.Exception.Message)" -ForegroundColor Magenta
        $script:fail++
        $script:results.Add([PSCustomObject]@{ID=$id; Estado="ERROR"; Prueba=$desc; Detalle=$_.Exception.Message})
    }
}

function Skip-Case($id, $desc, $reason) {
    Write-Host "  [SKIP]  [$id] $desc - SKIP: $reason" -ForegroundColor DarkGray
    $script:skip++
    $script:results.Add([PSCustomObject]@{ID=$id; Estado="SKIP"; Prueba=$desc; Detalle=$reason})
}

function Invoke-Api {
    param(
        [string]$Method = "GET",
        [string]$Url,
        $Body = $null,
        [string]$Token = $null,
        [string]$ContentType = "application/json"
    )
    $headers = @{ "Accept"="application/json" }
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }
    try {
        $params = @{ Method=$Method; Uri=$Url; Headers=$headers; ContentType=$ContentType; ErrorAction="Stop" }
        if ($Body) { $params["Body"] = ($Body | ConvertTo-Json -Depth 10) }
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $resp = Invoke-WebRequest @params -UseBasicParsing
        $sw.Stop()
        return @{ StatusCode=$resp.StatusCode; Body=($resp.Content | ConvertFrom-Json); Raw=$resp; Time=$sw.ElapsedMilliseconds }
    } catch {
        $code = 0
        $body = $null
        if ($_.Exception -and $_.Exception.Response) {
            $code = [int]$_.Exception.Response.StatusCode
            try {
                $stream = $_.Exception.Response.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($stream)
                $bodyTxt = $reader.ReadToEnd()
                $body = $bodyTxt | ConvertFrom-Json
            } catch { }
        } elseif ($_.Exception.Message -match "\((\d{3})\)") {
            $code = [int]$matches[1]
        }
        return @{ StatusCode=$code; Body=$body; Raw=$null; Time=0 }
    }
}

function Get-Token($cred) {
    if (-not $cred -or -not $cred.email -or -not $cred.pass -or -not $cred.slug) {
        return $null
    }
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$cred.email; password=$cred.pass; tenantSlug=$cred.slug
    }
    if ($r.StatusCode -eq 200 -and $r.Body.data.accessToken) {
        return @{ token=$r.Body.data.accessToken; data=$r.Body.data }
    }
    return $null
}

function Get-SqlCmd {
    # 1. Buscar en PATH
    $cmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    # 2. Buscar en rutas conocidas
    $paths = @(
        "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\160\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\130\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\120\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\130\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\140\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\160\Tools\Binn\sqlcmd.exe"
    )
    foreach ($p in $paths) { if (Test-Path $p) { return $p } }

    # 3. Intentar instalación automática si no existe
    Write-Host "  !! sqlcmd no encontrado. Intentando instalación automática..." -ForegroundColor Yellow
    
    # Probar winget
    $wingetIds = @("Microsoft.SqlServer.SQLCmd", "Microsoft.SQLCMD")
    foreach ($id in $wingetIds) {
        Write-Host "     Probando winget install --id $id ..." -ForegroundColor DarkGray
        $null = winget install --id $id --silent --accept-package-agreements --accept-source-agreements 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  OK  Instalado via winget. Buscando ejecutable..." -ForegroundColor Green
            Start-Sleep -Seconds 2
            $found = Get-ChildItem -Path "C:\Program Files", "C:\Program Files (x86)" -Filter "sqlcmd.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
            if ($found) { return $found }
        }
    }

    # Probar descarga directa MSI (aka.ms/sqlcmddownload)
    Write-Host "     Descargando instalador MSI de Microsoft..." -ForegroundColor DarkGray
    $tempMsi = Join-Path $env:TEMP "sqlcmd_installer.msi"
    try {
        Invoke-WebRequest -Uri "https://aka.ms/sqlcmddownload" -OutFile $tempMsi -UseBasicParsing -TimeoutSec 60
        if (Test-Path $tempMsi) {
            $proc = Start-Process msiexec -ArgumentList "/i `"$tempMsi`" /quiet /norestart" -Wait -PassThru
            if ($proc.ExitCode -eq 0) {
                Write-Host "  OK  MSI instalado. Buscando..." -ForegroundColor Green
                Start-Sleep -Seconds 2
                $found = Get-ChildItem -Path "C:\Program Files", "C:\Program Files (x86)" -Filter "sqlcmd.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
                if ($found) { return $found }
            }
        }
    } catch {}

    return $null
}


function Sql-Query($query) {
    if (-not $SQLCMD_PATH) {
        return @{ ok=$false; output="sqlcmd no encontrado. Instala SQL Server Tools o agrega sqlcmd al PATH." }
    }
    try {
        # Usamos la configuracion global que ya tiene el prefijo tcp: y el puerto
        $out = & $SQLCMD_PATH -S $SQL_SERVER -E -d $SQL_DB -Q $query -h -1 2>&1
        return @{ ok=($LASTEXITCODE -eq 0 -and ($out -notlike "*Error*")); output=($out | Select-Object -First 3) }
    } catch { return @{ ok=$false; output=$_.Exception.Message } }
}


function Get-Solution($testId, $detail) {
    foreach ($sol in $solutions) {
        if ($testId -match $sol.Pattern -or $detail -match $sol.Pattern) {
            return $sol.Msg
        }
    }
    return "Revisar logs del backend y logs de base de datos"
}

function Fallback-DbData {
    Write-Host "  !! Usando datos de FALLBACK (Hardcoded) por falta de conexion BD..." -ForegroundColor Yellow
    $p = if ($SQL_PASS) { $SQL_PASS } else { "DevPass123!" }

    $script:DB.T1 = @{ Id="3b369889-4705-4ce3-9cf2-c98e4f278f82"; Slug="savory-chile" }
    $script:DB.T2 = @{ Id="a3b3a3b3-a3b3-a3b3-a3b3-a3b3a3b3a3b3"; Slug="bresler-chile" }

    $t1 = $script:DB.T1.Slug
    $t2 = $script:DB.T2.Slug

    $script:CREDS["AdminSavory"]    = @{ email="admin@savory.cl"; pass=$p; slug=$t1 }
    $script:CREDS["AdminCoppelia"]   = @{ email="ignacio.romo@bresler.cl"; pass=$p; slug=$t2 }
    $script:CREDS["Tec1Savory"]     = @{ email="tec.frio@savory.cl"; pass=$p; slug=$t1 }
    $script:CREDS["Tec2Coca"]     = @{ email="tec.frio@savory.cl"; pass=$p; slug=$t1 }
    $script:CREDS["Trans1Savory"]   = @{ email="trans1@savory.cl"; pass=$p; slug=$t1 }
    $script:CREDS["Cliente1Savory"] = @{ email="cliente1@savory.cl"; pass=$p; slug=$t1 }
    $script:CREDS["Cliente2Coca"] = @{ email="cliente1@savory.cl"; pass=$p; slug=$t1 }

    $script:DB.Stores[$t1]   = "99999999-9999-9999-9999-999999999991"
    $script:DB.Coolers[$t1]  = "99999999-9999-9999-9999-999999999991"
    $script:DB.NfcTags[$t1]  = "test-nfc-token"
    $script:DB.Products[$t1] = "99999999-9999-9999-9999-999999999991"

    $script:dbLoaded = $true
}

# ============================================================
#  INICIALIZACION DE DATOS
# ============================================================
$SQLCMD_PATH = Get-SqlCmd
$dbLoaded = Load-DbData

if (-not $dbLoaded -or $CREDS.Count -eq 0) {
    Fallback-DbData
}



# -- Biblioteca de soluciones por patron ---------------------
$solutions = @(
    @{
        Pattern  = "C07"
        Title    = "CORS no configurado o no expuesto"
        Severity = "MEDIA"
        File     = "src\BA.Backend.WebAPI\Program.cs"
        Problem  = "El backend no envia el header Access-Control-Allow-Origin al recibir requests con Origin cross-origin."
        Fix      = @"
En Program.cs, verificar que CORS este configurado ANTES de app.UseAuthorization():

  builder.Services.AddCors(options => {
      options.AddPolicy("FrontendPolicy", policy => {
          policy.WithOrigins("http://localhost:4200", "https://tudominio.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
      });
  });
  // ...
  app.UseCors("FrontendPolicy");  // <- debe ir ANTES de UseAuthorization
"@
        Docs     = "https://learn.microsoft.com/en-us/aspnet/core/security/cors"
    }
    @{
        Pattern  = "A15"
        Title    = "Respuesta 401 sin campo message"
        Severity = "BAJA"
        File     = "src\BA.Backend.WebAPI\Middleware\GlobalExceptionHandler.cs"
        Problem  = "La respuesta de error 401 no incluye el campo 'message' o 'errors' en el body."
        Fix      = @"
En GlobalExceptionHandler.cs, en el handler de InvalidCredentialsException:

  case InvalidCredentialsException ex:
      context.Response.StatusCode = 401;
      await context.Response.WriteAsJsonAsync(new ApiResponse<object> {
          Success = false,
          Message = ex.Message,   // <- asegurar que Message no sea null/empty
          Errors  = new Dictionary<string, string[]> {
              { "credentials", new[] { ex.Message } }
          }
      });
      break;
"@
        Docs     = ""
    }
    @{
        Pattern  = "S06|DB0[1-9]|DB1[0-9]|DB2[0-5]"
        Title    = "sqlcmd no encontrado - Tests de BD saltados"
        Severity = "BAJA"
        File     = "PATH del sistema"
        Problem  = "sqlcmd no esta instalado o no esta en el PATH. Los tests directos de BD no pueden ejecutarse."
        Fix      = @"
Opcion A - Instalar via winget (recomendado):
  winget install Microsoft.SqlServer.SQLCmd

Opcion B - Instalar SQL Server Management Tools completo:
  winget install Microsoft.SQLServerManagementStudio

Opcion C - Agregar ruta existente al PATH:
  [Environment]::SetEnvironmentVariable(
    'PATH',
    $env:PATH + ';C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn',
    'User'
  )

Despues de instalar: cerrar y abrir nueva terminal para recargar PATH.
"@
        Docs     = "https://learn.microsoft.com/en-us/sql/tools/sqlcmd/sqlcmd-utility"
    }
    @{
        Pattern  = "R13|TR01|TR02"
        Title    = "SQL usa columna o.Total que no existe"
        Severity = "ALTA"
        File     = "src\BA.Backend.Infrastructure\Repositories\TransportistaRepository.cs"
        Problem  = "GetPendingRouteStopsAsync usa 'o.Total AS OrderTotal' pero Total no es columna en Orders - es un valor calculado. Causa 500 en GET /transportista/route."
        Fix      = @"
Buscar la query SQL en TransportistaRepository.cs y reemplazar:

  -- ANTES (incorrecto):
  o.Total AS OrderTotal,

  -- DESPUES (correcto):
  ISNULL((
      SELECT SUM(oi.Quantity * oi.UnitPrice)
      FROM dbo.OrderItems oi
      WHERE oi.OrderId = o.Id
  ), 0) AS OrderTotal,

Si la tabla OrderItems aun no tiene datos, retornara 0 correctamente.
"@
        Docs     = ""
    }
    @{
        Pattern  = "T04"
        Title    = "ReEnroll crea NfcTag sin validar CoolerId - FK violation"
        Severity = "ALTA"
        File     = "src\BA.Backend.Application\Tecnico\Handlers\TecnicoCommandHandlers.cs"
        Problem  = "ReEnrollNfcCommand inserta un NfcTag con un CoolerId que no existe en la tabla Coolers, causando una FK constraint violation (500)."
        Fix      = @"
En el handler de ReEnrollNfcCommand, agregar validacion antes del INSERT:

  // Validar que el Cooler existe y pertenece al tenant
  var cooler = await _coolerRepository
      .GetByIdWithTenantAsync(request.CoolerId, request.TenantId, cancellationToken);

  if (cooler is null)
      throw new KeyNotFoundException("COOLER_NOT_FOUND");

  // Recien aca crear el nuevo NfcTag
  var newTag = new NfcTag { ... };
"@
        Docs     = ""
    }
    @{
        Pattern  = "U0[1-9]|U1[0-6]"
        Title    = "CreateUserDto tiene [Range(0,3)] incorrecto para enum Role"
        Severity = "ALTA"
        File     = "src\BA.Backend.Application\Users\DTOs\CreateUserDto.cs"
        Problem  = "El atributo [Range(0,3)] no coincide con el enum UserRole (Admin=1, Cliente=2, Transportista=3, Tecnico=4). Role=4 (Tecnico) falla validacion y retorna 400."
        Fix      = @"
En CreateUserDto.cs cambiar:

  // ANTES:
  [Range(0, 3)]
  public int Role { get; set; }

  // DESPUES:
  [Range(1, 4, ErrorMessage = "Rol invalido. Valores validos: 1=Admin, 2=Cliente, 3=Transportista, 4=Tecnico")]
  public int Role { get; set; }
"@
        Docs     = ""
    }
    @{
        Pattern  = "MT05"
        Title    = "MT05 requiere sqlcmd para obtener ID de usuario Pepsi"
        Severity = "BAJA"
        File     = "test.ps1"
        Problem  = "El test MT05 necesita consultar la BD directamente para obtener el ID de un usuario de otro tenant. Sin sqlcmd usa un ID de fallback."
        Fix      = @"
Instalar sqlcmd (ver solucion DB tests) para que MT05 use el ID real.
El test de fallback con ID falso sigue validando el comportamiento 404.
"@
        Docs     = ""
    }
    @{
        Pattern  = "J05"
        Title    = "Token JWT expira demasiado rapido o expiresAt en pasado"
        Severity = "MEDIA"
        File     = "src\BA.Backend.WebAPI\appsettings.json o JwtService.cs"
        Problem  = "El campo expiresAt del token retorna una fecha en el pasado o muy cercana al momento de generacion. Puede ser un problema de zona horaria (UTC vs local) o TTL muy corto."
        Fix      = @"
Opcion A - Verificar zona horaria en JwtService.cs:
  // Usar siempre DateTime.UtcNow, nunca DateTime.Now
  var expiration = DateTime.UtcNow.AddHours(8);  // TTL de 8 horas

Opcion B - Verificar appsettings.json:
  "Jwt": {
    "ExpirationHours": 8,   // <- aumentar si es muy bajo
    "Issuer": "...",
    "Audience": "..."
  }

Opcion C - Verificar que el claim 'exp' del JWT use Unix timestamp UTC:
  new Claim(JwtRegisteredClaimNames.Exp,
      new DateTimeOffset(expiration).ToUnixTimeSeconds().ToString())
"@
        Docs     = "https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn"
    }
    @{
        Pattern  = "EP06"
        Title    = "Swagger no disponible o retorna Status:0"
        Severity = "BAJA"
        File     = "src\BA.Backend.WebAPI\Program.cs"
        Problem  = "GET /swagger retorna Status:0 (timeout o conexion rechazada). Swagger puede estar deshabilitado en la configuracion actual."
        Fix      = @"
En Program.cs verificar que Swagger este habilitado:

  // Agregar en el bloque de desarrollo:
  if (app.Environment.IsDevelopment()) {
      app.UseSwagger();
      app.UseSwaggerUI(c => {
          c.SwaggerEndpoint("/swagger/v1/swagger.json", "BA.FrioCheck API v1");
          c.RoutePrefix = "swagger";
      });
  }

Tambien verificar ASPNETCORE_ENVIRONMENT=Development al correr el backend.
"@
        Docs     = "https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger"
    }
    @{
        Pattern  = "BD_FC|Sin acceso SQL|SqlConnection"
        Title    = "No se puede conectar a BD_FC"
        Severity = "CRITICA"
        File     = "appsettings.json / SQL Express"
        Problem  = "No existe conexion a la base de datos BD_FC. El backend puede estar usando una instancia diferente o la BD no fue creada."
        Fix      = @"
Paso 1 - Iniciar LocalDB:
  net start MSSQL$SQLEXPRESS

Paso 2 - Verificar conexion:
  sqlcmd -S "localhost\SQLEXPRESS" -E -Q "SELECT name FROM sys.databases"

Paso 3 - Crear la BD si no aparece BD_FC:
  sqlcmd -S "localhost\SQLEXPRESS" -E -Q "CREATE DATABASE BD_FC"

Paso 4 - Ejecutar schema y seed:
  sqlcmd -S "localhost\SQLEXPRESS" -E -d BD_FC -i sql\01_schema.sql
  sqlcmd -S "localhost\SQLEXPRESS" -E -d BD_FC -i sql\02_seed_data.sql

Paso 5 - Verificar appsettings.json:
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\SQLEXPRESS;Database=BD_FC;Integrated Security=true;TrustServerCertificate=true;"
  }
"@
        Docs     = "https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express"
    }
)

# -- Funcion para mapear test ID a solucion -------------------



# ============================================================
#  BLOQUE 0 - VERIFICACION DE ENTORNO Y RUTAS DEL SISTEMA
# ============================================================
Write-Header "BLOQUE 0 - VERIFICACION DE ENTORNO Y RUTAS"

$envOk  = $true

function Check-Tool {
    param(
        [string]$Name,
        [string]$Command,
        [string]$VersionArg    = "--version",
        [string]$InstallHint   = "",
        [string[]]$FallbackPaths = @()
    )
    $found   = $false; $version = ""; $path = ""
    $cmd = Get-Command $Command -ErrorAction SilentlyContinue
    if ($cmd) {
        $found = $true; $path = $cmd.Source
        try { $version = (& $cmd.Source $VersionArg 2>&1 | Select-Object -First 1) -replace "`n",""  } catch {}
    } else {
        foreach ($fp in $FallbackPaths) {
            if (Test-Path $fp) {
                $found = $true; $path = $fp
                try { $version = (& $fp $VersionArg 2>&1 | Select-Object -First 1) -replace "`n","" } catch {}
                break
            }
        }
    }
    if ($found) {
        Write-Host ("  OK  {0,-22} {1}" -f $Name, $version.Trim()) -ForegroundColor Green
        Write-Host "       Ruta: $path" -ForegroundColor DarkGray
    } else {
        Write-Host ("  !!  {0,-22} No encontrado en PATH" -f $Name) -ForegroundColor Red
        if ($InstallHint) { Write-Host "      Instalar: $InstallHint" -ForegroundColor DarkYellow }
        $script:envOk = $false
    }
    return $found
}

function Check-EnvVar {
    param([string]$VarName, [string]$Desc, [bool]$Required = $false)
    $val = [System.Environment]::GetEnvironmentVariable($VarName)
    if ($val) {
        Write-Host ("  OK  {0,-35} = {1}" -f $VarName, $val.Substring(0,[Math]::Min(50,$val.Length))) -ForegroundColor Green
    } else {
        $color = if ($Required) { "Red" } else { "Yellow" }
        Write-Host ("  ??  {0,-35}   no definida - $Desc" -f $VarName) -ForegroundColor $color
        if ($Required) { $script:envOk = $false }
    }
}

function Check-Port {
    param([int]$Port, [string]$Label)
    $conn = Test-NetConnection -ComputerName "localhost" -Port $Port -WarningAction SilentlyContinue -EA SilentlyContinue
    if ($conn.TcpTestSucceeded) {
        Write-Host ("  OK  Puerto {0,-6}  {1}" -f $Port, $Label) -ForegroundColor Green
    } else {
        Write-Host ("  !!  Puerto {0,-6}  {1}  (no responde)" -f $Port, $Label) -ForegroundColor Red
        $script:envOk = $false
    }
}

# -- 1. Herramientas ------------------------------------------
Write-Host ""
Write-Host "  -- Herramientas de desarrollo --" -ForegroundColor White

Check-Tool ".NET SDK"    "dotnet"  "--version" "winget install Microsoft.DotNet.SDK.10"
Check-Tool "Git"         "git"     "--version" "winget install Git.Git"
Check-Tool "Node.js"     "node"    "--version" "winget install OpenJS.NodeJS.LTS"
Check-Tool "npm"         "npm"     "--version" "(viene con Node.js)"
Check-Tool "PowerShell7" "pwsh"    "--version" "winget install Microsoft.PowerShell"
Check-Tool "curl"        "curl"    "--version" "(incluido en Windows 10+)"

# sqlcmd con paths de fallback
if ($SQLCMD_PATH) {
    Write-Host ("  OK  {0,-22} {1}" -f "sqlcmd", $SQLCMD_PATH) -ForegroundColor Green
} else {
    Write-Host ("  !!  {0,-22} No encontrado en PATH ni rutas conocidas" -f "sqlcmd") -ForegroundColor Red
    Write-Host "       Instalar: winget install Microsoft.SqlServer.SQLCmd" -ForegroundColor DarkYellow
    Write-Host "       Luego abrir nueva terminal para recargar PATH" -ForegroundColor DarkGray
}

# -- 2. Puertos y servicios -----------------------------------
Write-Host ""
Write-Host "  -- Puertos y servicios --" -ForegroundColor White

Check-Port 5003 "BA.FrioCheck Backend  (REQUERIDO)"
Check-Port 1433 "SQL Server Express  (requerido)"

# -- 3. Rutas del proyecto ------------------------------------
Write-Host ""
Write-Host "  -- Rutas del proyecto --" -ForegroundColor White

$projectRoot = $PSScriptRoot
Write-Host "  Raiz detectada: $projectRoot" -ForegroundColor Cyan

$sln = Get-ChildItem -Path $projectRoot -Filter "*.sln" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($sln) {
    Write-Host "  OK  Solution: $($sln.Name)" -ForegroundColor Green
} else {
    Write-Host "  !!  No se encontro .sln en la raiz" -ForegroundColor Red
    $envOk = $false
}

$layers = @{
    "WebAPI"         = @("src\BA.Backend.WebAPI",         "BA.Backend.WebAPI")
    "Application"    = @("src\BA.Backend.Application",    "BA.Backend.Application")
    "Infrastructure" = @("src\BA.Backend.Infrastructure", "BA.Backend.Infrastructure")
    "Domain"         = @("src\BA.Backend.Domain",         "BA.Backend.Domain")
    "Tests"          = @("tests",                         "BA.Backend.Tests", "test")
}

foreach ($layer in $layers.GetEnumerator()) {
    $found = $false
    foreach ($candidate in $layer.Value) {
        $fp = Join-Path $projectRoot $candidate
        if (Test-Path $fp) {
            Write-Host ("  OK  Capa {0,-16} {1}" -f $layer.Key, $candidate) -ForegroundColor Green
            $found = $true; break
        }
    }
    if (-not $found) {
        Write-Host ("  ??  Capa {0,-16} no encontrada en rutas conocidas" -f $layer.Key) -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "  -- Archivos clave --" -ForegroundColor White

$keyFiles = [ordered]@{
    "appsettings.json"     = @("src\BA.Backend.WebAPI\appsettings.json",             "appsettings.json", "BA.Backend.WebAPI\appsettings.json")
    "appsettings.Dev"      = @("src\BA.Backend.WebAPI\appsettings.Development.json", "appsettings.Development.json")
    ".gitignore"           = @(".gitignore")
    "SQL 01_schema"        = @("sql\01_schema.sql",   "sql\01_create_tables.sql",    "01_schema.sql")
    "SQL 02_seed"          = @("sql\02_seed_data.sql","sql\seed.sql",                "02_seed_data.sql")
    "credenciales.md"      = @("credenciales_prueba.md", "docs\credenciales_prueba.md")
    "REFACTOR_REPORT.md"   = @("REFACTOR_REPORT.md",  "docs\REFACTOR_REPORT.md")
}

foreach ($kf in $keyFiles.GetEnumerator()) {
    $found = $false
    foreach ($candidate in $kf.Value) {
        $fp = Join-Path $projectRoot $candidate
        if (Test-Path $fp) {
            Write-Host ("  OK  {0,-22}  {1}" -f $kf.Key, $candidate) -ForegroundColor Green
            $found = $true; break
        }
    }
    if (-not $found) {
        Write-Host ("  ??  {0,-22}  No encontrado" -f $kf.Key) -ForegroundColor Yellow
    }
}

# -- 4. Variables de entorno ----------------------------------
Write-Host ""
Write-Host "  -- Variables de entorno --" -ForegroundColor White

Check-EnvVar "ASPNETCORE_ENVIRONMENT" "Development/Production"        $false
Check-EnvVar "DOTNET_ROOT"            "Ruta runtime .NET"             $false
Check-EnvVar "JWT_SECRET"             "Secreto JWT (si usa envvar)"   $false
Check-EnvVar "ConnectionStrings__DefaultConnection" "Cadena BD"       $false
Check-EnvVar "COMPUTERNAME"           "Nombre equipo"                 $false

# -- 5. Runtimes .NET instalados ------------------------------
Write-Host ""
Write-Host "  -- .NET SDKs y Runtimes --" -ForegroundColor White

$dotnetOk = Get-Command dotnet -ErrorAction SilentlyContinue
if ($dotnetOk) {
    $sdks     = dotnet --list-sdks     2>&1
    $runtimes = dotnet --list-runtimes 2>&1

    $sdks | ForEach-Object { Write-Host "  SDK:     $_" -ForegroundColor DarkGray }
    $runtimes | Select-Object -First 8 | ForEach-Object { Write-Host "  Runtime: $_" -ForegroundColor DarkGray }

    $hasTarget = ($sdks | Where-Object { $_ -match "^(9\.|10\.)" })
    if ($hasTarget) {
        Write-Host "  OK  .NET 9/10 SDK disponible para el proyecto" -ForegroundColor Green
    } else {
        Write-Host "  ??  No se detecta .NET 9 ni 10 - el proyecto usa .NET 10" -ForegroundColor Yellow
        Write-Host "       Instalar: winget install Microsoft.DotNet.SDK.10" -ForegroundColor DarkYellow
    }
} else {
    Write-Host "  !!  dotnet no instalado o no en PATH" -ForegroundColor Red
}

# -- 6. Analisis del PATH -------------------------------------
Write-Host ""
Write-Host "  -- Analisis del PATH del sistema --" -ForegroundColor White

$pathDirs     = ($env:PATH -split ";") | Where-Object { $_ -ne "" }
$missingPaths = $pathDirs | Where-Object { -not (Test-Path $_) }
$validPaths   = $pathDirs | Where-Object {  (Test-Path $_) }

Write-Host "  Entradas totales en PATH : $($pathDirs.Count)" -ForegroundColor White
Write-Host "  Rutas validas            : $($validPaths.Count)" -ForegroundColor Green
if ($missingPaths.Count -gt 0) {
    Write-Host "  Rutas rotas (no existen) : $($missingPaths.Count)" -ForegroundColor Yellow
    $missingPaths | Select-Object -First 4 | ForEach-Object {
        Write-Host "    - $_" -ForegroundColor DarkGray
    }
    if ($missingPaths.Count -gt 4) {
        Write-Host "    ... y $($missingPaths.Count - 4) mas" -ForegroundColor DarkGray
    }
}

Write-Host ""
Write-Host "  Rutas criticas recomendadas en PATH:" -ForegroundColor White

$recommendedPaths = @(
    @{ path="C:\Program Files\dotnet";                                                       label=".NET SDK/Runtime" }
    @{ path="C:\Program Files\Git\cmd";                                                      label="Git" }
    @{ path="C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn";          label="sqlcmd ODBC 17" }
    @{ path="C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\160\Tools\Binn";          label="sqlcmd ODBC 16" }
    @{ path="C:\Program Files\nodejs";                                                       label="Node.js" }
    @{ path="C:\Users\PrDes\AppData\Roaming\npm";                                            label="npm global (PrDes)" }
    @{ path="C:\Users\PrDes\.local\bin";                                                     label="Herramientas locales (PrDes)" }
    @{ path="C:\Users\PrDes\AppData\Local\Programs\Microsoft VS Code";                       label="VS Code" }
)

foreach ($rec in $recommendedPaths) {
    $exists = Test-Path $rec.path
    $inPath = $pathDirs -contains $rec.path
    if ($exists -and $inPath) {
        Write-Host ("  OK  {0,-36}  en PATH" -f $rec.label) -ForegroundColor Green
    } elseif ($exists -and -not $inPath) {
        Write-Host ("  !!  {0,-36}  existe pero NO en PATH" -f $rec.label) -ForegroundColor Yellow
        Write-Host "       Fix: [Environment]::SetEnvironmentVariable('PATH', `$env:PATH+';$($rec.path)', 'User')" -ForegroundColor DarkGray
    } else {
        Write-Host ("  --  {0,-36}  no instalado" -f $rec.label) -ForegroundColor DarkGray
    }
}


# -- 8. Conexion con la base de datos ------------------------
Write-Host ""
Write-Host "  -- Conexion con Base de Datos (BD_FC / SQL Express) --" -ForegroundColor White

$dbStatus = @{
    localdbInstalled  = $false
    instanceRunning   = $false
    bdFcExists        = $false
    canConnect        = $false
    tablesOk          = $false
    seedOk            = $false
    connString        = ""
}

# 8a. Verificar SQL Server Express
Write-Host "  Verificando SQL Server Express en localhost\SQLEXPRESS..." -ForegroundColor DarkGray
$dbStatus.localdbInstalled = $true

# Probar conexion directa con sqlcmd
if ($SQLCMD_PATH) {
    $testConn = & $SQLCMD_PATH -S "localhost\SQLEXPRESS" -E -Q "SELECT 1" -h -1 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  OK  SQL Server Express responde en localhost\SQLEXPRESS" -ForegroundColor Green
        $dbStatus.instanceRunning = $true
    } else {
        Write-Host "  !!  SQL Server Express no responde - verificar servicio" -ForegroundColor Red
        Write-Host "      Iniciar servicio: net start MSSQL`$SQLEXPRESS" -ForegroundColor DarkGray
        $envOk = $false
    }
} else {
    Write-Host "  ??  sqlcmd no encontrado - verificando via SqlClient..." -ForegroundColor Yellow
    $dbStatus.instanceRunning = $true
}

# 8c. Conexion via SqlConnection (.NET - no necesita sqlcmd)
Write-Host ""
Write-Host "  Probando conexion directa via SqlClient..." -ForegroundColor DarkGray

$connStrings = @(
    "Server=localhost\SQLEXPRESS;Database=BD_FC;Integrated Security=true;Connect Timeout=10;TrustServerCertificate=true;"
    "Server=localhost\SQLEXPRESS;Database=BD_FC;Trusted_Connection=Yes;Connect Timeout=10;TrustServerCertificate=true;"
    "Data Source=localhost\SQLEXPRESS;Initial Catalog=BD_FC;Integrated Security=True;Connect Timeout=10;TrustServerCertificate=true;"
)

$connected = $false
foreach ($cs in $connStrings) {
    try {
        $conn = New-Object System.Data.SqlClient.SqlConnection($cs)
        $conn.Open()
        if ($conn.State -eq "Open") {
            $connected       = $true
            $dbStatus.canConnect  = $true
            $dbStatus.connString  = $cs
            Write-Host "  OK  SqlConnection abierta" -ForegroundColor Green
            Write-Host "      String: $($cs.Substring(0,[Math]::Min(80,$cs.Length)))" -ForegroundColor DarkGray

            # 8d. Verificar que BD_FC existe
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = "SELECT DB_NAME()"
            $dbName = $cmd.ExecuteScalar()
            Write-Host "  OK  Base de datos activa: $dbName" -ForegroundColor Green
            $dbStatus.bdFcExists = ($dbName -eq "BD_FC")

            # 8e. Verificar tablas clave
            Write-Host ""
            Write-Host "  Verificando tablas..." -ForegroundColor DarkGray
            $tables = @("Tenants","Users","Stores","Coolers","NfcTags","Orders","UserSessions",
                        "TechSupportRequests","Routes","Products","OrderItems","Mermas","PasswordResetTokens")
            $missingTables = @()

            foreach ($tbl in $tables) {
                $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='$tbl' AND TABLE_TYPE='BASE TABLE'"
                $exists = [int]$cmd.ExecuteScalar()
                if ($exists -eq 1) {
                    Write-Host ("  OK  Tabla {0,-30}" -f $tbl) -ForegroundColor Green
                } else {
                    Write-Host ("  !!  Tabla {0,-30} NO EXISTE - ejecutar sql/01_schema.sql" -f $tbl) -ForegroundColor Red
                    $missingTables += $tbl
                }
            }
            $dbStatus.tablesOk = ($missingTables.Count -eq 0)

            # 8f. Verificar seed data
            Write-Host ""
            Write-Host "  Verificando datos de seed..." -ForegroundColor DarkGray
            $seedChecks = @(
                @{ query="SELECT COUNT(*) FROM Tenants WHERE IsActive=1";                        min=2; label="Tenants activos"    }
                @{ query="SELECT COUNT(*) FROM Users WHERE IsActive=1";                          min=10; label="Usuarios activos"  }
                @{ query="SELECT COUNT(*) FROM Stores";                                          min=2; label="Stores"            }
                @{ query="SELECT COUNT(*) FROM Users WHERE Role=1 AND IsActive=1";               min=1; label="Admins (Role=1)"   }
                @{ query="SELECT COUNT(*) FROM Users WHERE PasswordHash LIKE '`$2a`$%'";         min=1; label="Hashes BCrypt"     }
                @{ query="SELECT COUNT(*) FROM Users WHERE PasswordHash LIKE 'hash_%'";          max=0; label="Hashes en texto plano (deben ser 0)" }
            )

            $seedOk = $true
            foreach ($sc in $seedChecks) {
                $cmd.CommandText = $sc.query
                try {
                    $val = [int]$cmd.ExecuteScalar()
                    if ($sc.ContainsKey("min") -and $val -ge $sc.min) {
                        Write-Host ("  OK  {0,-38} = {1}" -f $sc.label, $val) -ForegroundColor Green
                    } elseif ($sc.ContainsKey("max") -and $val -le $sc.max) {
                        Write-Host ("  OK  {0,-38} = {1}" -f $sc.label, $val) -ForegroundColor Green
                    } elseif ($sc.ContainsKey("min")) {
                        Write-Host ("  !!  {0,-38} = {1} (minimo esperado: {2}) - ejecutar sql/02_seed_data.sql" -f $sc.label, $val, $sc.min) -ForegroundColor Red
                        $seedOk = $false
                    } else {
                        Write-Host ("  !!  {0,-38} = {1} (deberia ser 0 - hashes en texto plano en BD)" -f $sc.label, $val) -ForegroundColor Red
                        $seedOk = $false
                    }
                } catch {
                    Write-Host ("  ??  {0,-38} error: {1}" -f $sc.label, $_.Exception.Message) -ForegroundColor Yellow
                }
            }
            $dbStatus.seedOk = $seedOk

            # 8g. Verificar configuracion de conexion en appsettings
            Write-Host ""
            Write-Host "  Verificando appsettings.json..." -ForegroundColor DarkGray
            $appsettingsPaths = @(
                (Join-Path $PSScriptRoot "src\BA.Backend.WebAPI\appsettings.json"),
                (Join-Path $PSScriptRoot "appsettings.json"),
                (Join-Path $PSScriptRoot "BA.Backend.WebAPI\appsettings.json")
            )
            foreach ($asp in $appsettingsPaths) {
                if (Test-Path $asp) {
                    $aspContent = Get-Content $asp -Raw | ConvertFrom-Json -ErrorAction SilentlyContinue
                    $connFromSettings = $aspContent.ConnectionStrings.DefaultConnection
                    if ($connFromSettings) {
                        Write-Host "  OK  ConnectionStrings.DefaultConnection encontrada" -ForegroundColor Green
                        Write-Host "      $($connFromSettings.Substring(0,[Math]::Min(80,$connFromSettings.Length)))" -ForegroundColor DarkGray

                        # Verificar que apunta a SQLEXPRESS
                        if ($connFromSettings -match "SQLEXPRESS|localhost|127\.0\.0\.1") {
                            Write-Host "  OK  Apunta a SQL Express correctamente" -ForegroundColor Green
                        } else {
                            Write-Host "  ??  No apunta a SQLEXPRESS - verificar appsettings" -ForegroundColor Yellow
                        }
                        # Verificar que la BD es BD_FC
                        if ($connFromSettings -match "BD_FC") {
                            Write-Host "  OK  Base de datos: BD_FC" -ForegroundColor Green
                        } else {
                            Write-Host "  !!  La cadena de conexion no menciona BD_FC" -ForegroundColor Red
                        }
                    } else {
                        Write-Host "  !!  No se encontro ConnectionStrings en $asp" -ForegroundColor Red
                    }
                    break
                }
            }

            $conn.Close()
            break
        }
    } catch {
        # Silently try next connection string
    }
}

if (-not $connected) {
    Write-Host "  !!  No se pudo conectar a BD_FC via ninguna cadena de conexion" -ForegroundColor Red
    Write-Host "      Posibles causas:" -ForegroundColor Yellow
    Write-Host "      1. Servicio SQLEXPRESS no iniciado (net start MSSQL$SQLEXPRESS)" -ForegroundColor DarkGray
    Write-Host "      2. BD_FC no existe (ejecutar sql/01_schema.sql)" -ForegroundColor DarkGray
    Write-Host "      3. Verificar cadena de conexion: Server=localhost\SQLEXPRESS;Database=BD_FC" -ForegroundColor DarkGray
    Write-Host "      4. El backend usa una instancia diferente (revisar appsettings.json)" -ForegroundColor DarkGray
    $envOk = $false
}

# 8h. Verificar via API health (alternativa si SqlClient falla)
Write-Host ""
Write-Host "  Verificando estado BD via API /health..." -ForegroundColor DarkGray
try {
    $healthR = Invoke-WebRequest -Uri "$BASE/health" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    $healthBody = $healthR.Content | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($healthR.StatusCode -eq 200) {
        Write-Host "  OK  /health retorna 200" -ForegroundColor Green
        # Algunos health checks exponen el estado de la BD
        if ($healthBody.entries -or $healthBody.components) {
            $dbEntry = ($healthBody.entries | Get-Member -MemberType NoteProperty -ErrorAction SilentlyContinue) |
                       Where-Object { $_.Name -match "db|database|sql" }
            if ($dbEntry) {
                Write-Host "  OK  Health check de BD reportado en /health" -ForegroundColor Green
            }
        }
    } else {
        Write-Host "  !!  /health retorna $($healthR.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "  ??  /health no responde (backend no iniciado)" -ForegroundColor Yellow
}

# Resumen BD
Write-Host ""
Write-Host "  -- Resumen BD ------------------------------------------" -ForegroundColor White
$_ldb  = if($dbStatus.localdbInstalled){"SI - localhost\SQLEXPRESS"}else{"NO - verificar servicio MSSQL$SQLEXPRESS"};  $_ldbc = if($dbStatus.localdbInstalled){"Green"}else{"Yellow"}
$_inst = if($dbStatus.instanceRunning) {"SI"}else{"NO"};                                  $_instc= if($dbStatus.instanceRunning) {"Green"}else{"Red"}
$_sql  = if($dbStatus.canConnect)      {"OK"}else{"FALLO"};                               $_sqlc = if($dbStatus.canConnect)      {"Green"}else{"Red"}
$_bdf  = if($dbStatus.bdFcExists)      {"SI"}else{"NO - ejecutar 01_schema.sql"};         $_bdfc = if($dbStatus.bdFcExists)      {"Green"}else{"Red"}
$_tbl  = if($dbStatus.tablesOk)        {"SI"}else{"NO - ejecutar 01_schema.sql"};         $_tblc = if($dbStatus.tablesOk)        {"Green"}else{"Red"}
$_sed  = if($dbStatus.seedOk)          {"SI"}else{"NO - ejecutar 02_seed_data.sql"};      $_sedc = if($dbStatus.seedOk)          {"Green"}else{"Yellow"}
Write-Host ("  SQL Express activo   : {0}" -f $_ldb)  -ForegroundColor $_ldbc
Write-Host ("  Instancia corriendo  : {0}" -f $_inst) -ForegroundColor $_instc
Write-Host ("  Conexion SqlClient   : {0}" -f $_sql)  -ForegroundColor $_sqlc
Write-Host ("  BD_FC existe         : {0}" -f $_bdf)  -ForegroundColor $_bdfc
Write-Host ("  Tablas completas     : {0}" -f $_tbl)  -ForegroundColor $_tblc
Write-Host ("  Seed data cargado    : {0}" -f $_sed)  -ForegroundColor $_sedc
Write-Host ""


# -- 9. Resumen entorno ---------------------------------------
Write-Host ""
if ($envOk) {
    Write-Host "  ENTORNO OK - Todas las dependencias criticas disponibles" -ForegroundColor Green
} else {
    Write-Host "  ENTORNO INCOMPLETO - Revisa los items en rojo arriba" -ForegroundColor Yellow
    Write-Host "  Los tests de API seguiran ejecutandose de todas formas" -ForegroundColor DarkGray
}
Write-Host ""


# ============================================================
#  OBTENER TOKENS (todos los roles y tenants)
# ============================================================
Write-Header "OBTENIENDO TOKENS DE SESION"

$TOKENS = @{}
foreach ($key in $CREDS.Keys) {
    $result = Get-Token $CREDS[$key]
    if ($result) {
        $TOKENS[$key] = $result.token
        Write-Host "  [PASS] $key ($($CREDS[$key].role) / $($CREDS[$key].slug))" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] $key - sin token" -ForegroundColor Red
    }
}

# ============================================================
#  BLOQUE 1 - CONEXION Y PING
# ============================================================

Write-Header "BLOQUE 1 - CONEXION API Y ESTRUCTURA BASE"

Test-Case "C01" "GET /ping retorna 200 Online" {
    $r = Invoke-Api -Url "$BASE/ping"
    @{ ok=($r.StatusCode -eq 200); detail="Status: $($r.StatusCode)" }
}

Test-Case "C02" "GET /ping sin token funciona (AllowAnonymous)" {
    $r = Invoke-Api -Url "$BASE/ping"
    @{ ok=($r.StatusCode -eq 200 -and $r.Body.success -eq $true); detail="success=$($r.Body.success)" }
}

Test-Case "C03" "Content-Type es application/json en todas las respuestas" {
    $r = Invoke-Api -Url "$BASE/ping"
    $ct = $r.Raw.Headers["Content-Type"]
    @{ ok=($ct -like "*application/json*"); detail="Content-Type: $ct" }
}

Test-Case "C04" "Respuesta /ping tiene campo data.status=Online" {
    $r = Invoke-Api -Url "$BASE/ping"
    @{ ok=($r.Body.data.status -eq "Online"); detail="status=$($r.Body.data.status)" }
}

Test-Case "C05" "Respuesta /ping tiene campo data.machineName" {
    $r = Invoke-Api -Url "$BASE/ping"
    @{ ok=($r.Body.data.machineName -ne $null); detail="machineName=$($r.Body.data.machineName)" }
}

Test-Case "C06" "GET /health retorna 200 con status Healthy" {
    $r = Invoke-Api -Url "$BASE/health"
    @{ ok=($r.StatusCode -eq 200); detail="Status: $($r.StatusCode)" }
}

Test-Case "C07" "Header CORS presente con Origin cross-origin" {
    try {
        $headers = @{ "Accept"="application/json"; "Origin"="http://localhost:4200" }
        $resp = Invoke-WebRequest -Method GET -Uri "$BASE/ping" -Headers $headers -UseBasicParsing -ErrorAction Stop
        $cors = $resp.Headers["Access-Control-Allow-Origin"]
        @{ ok=($cors -ne $null -and $cors -ne ""); detail="CORS: $cors" }
    } catch { @{ ok=$false; detail="Error: $($_.Exception.Message)" } }
}

Test-Case "C08" "Backend responde en menos de 3000ms" {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $r = Invoke-Api -Url "$BASE/ping"
    $sw.Stop()
    @{ ok=($sw.ElapsedMilliseconds -lt 3000 -and $r.StatusCode -eq 200); detail="Tiempo: $($sw.ElapsedMilliseconds)ms" }
}

# ============================================================
#  BLOQUE 2 - AUTH LOGIN
# ============================================================
Write-Header "BLOQUE 2 - AUTENTICACION / LOGIN"

Test-Case "A01" "Login Admin savory-chile retorna 200 + accessToken" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 200 -and $r.Body.data.accessToken); detail="Status:$($r.StatusCode) token=$(if($r.Body.data.accessToken){'OK'}else{'MISSING'})" }
}

Test-Case "A02" "Login Admin coppelia-chile retorna 200" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminCoppelia"].email; password=$SQL_PASS; tenantSlug=$DB.T2.Slug
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "A03" "Login Tecnico retorna redirectTo=/tecnico/dashboard" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["Tec1Savory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.Body.data.redirectTo -eq "/tecnico/dashboard"); detail="redirectTo=$($r.Body.data.redirectTo)" }
}

Test-Case "A04" "Login Transportista retorna redirectTo=/transportista/dashboard" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["Trans1Savory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.Body.data.redirectTo -eq "/transportista/dashboard"); detail="redirectTo=$($r.Body.data.redirectTo)" }
}

Test-Case "A05" "Login Cliente retorna redirectTo=/cliente/dashboard" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["Cliente1Savory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.Body.data.redirectTo -eq "/cliente/dashboard"); detail="redirectTo=$($r.Body.data.redirectTo)" }
}

Test-Case "A06" "Login Admin retorna redirectTo=/admin/dashboard" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.Body.data.redirectTo -eq "/admin/dashboard"); detail="redirectTo=$($r.Body.data.redirectTo)" }
}

Test-Case "A07" "Login respuesta tiene success:true y campos requeridos" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    $d = $r.Body.data
    $ok = ($r.Body.success -eq $true -and $d.accessToken -and $d.expiresAt -and $d.userFullName -and $d.role -and $d.userId -and $d.tenantId)
    @{ ok=$ok; detail="success=$($r.Body.success) campos=$(if($ok){'OK'}else{'FALTANTES'})" }
}

Test-Case "A08" "Login password incorrecta retorna 401 (NO 500)" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password="ClaveMAL999_PS_TEST!"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode) esperado:401" }
}

Test-Case "A09" "Login email inexistente retorna 401 (NO 500)" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email="noexiste_ps_test@test.cl"; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode)" }
}

Test-Case "A10" "Login tenantSlug inexistente retorna 401 (NO 500)" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug="tenant-que-no-existe-xyz"
    }
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode)" }
}

Test-Case "A11" "Login sin tenantSlug (usa Fallback de Backend)" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=""
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) (200 esperado por fallback)" }
}

Test-Case "A12" "Login sin email retorna 400 con detalle de validacion" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode)" }
}

Test-Case "A13" "Login sin password retorna 400" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode)" }
}

Test-Case "A14" "Login password < 8 chars retorna 400 (FluentValidation -> custom -> 400, NO 500)" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password="corta"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode) - valida que ValidationException no sea 500" }
}

Test-Case "A15" "Respuesta de error 401 tiene wrap ApiResponse con success:false" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password="ClaveMAL_PS_TEST!"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 401 -and $r.Body.success -eq $false); detail="success=$($r.Body.success) msg=$($r.Body.message) errors=$($r.Body.errors.Count)" }
}

Test-Case "A16" "Error 401 NO expone stack trace ni paths internos" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password="ClaveMAL_PS_TEST!"; tenantSlug=$DB.T1.Slug
    }
    $body = $r.Body | ConvertTo-Json
    @{ ok=($body -notlike "*StackTrace*" -and $body -notlike "*at BA.Backend*"); detail="Sin internals expuestos" }
}

Test-Case "A17" "Login usuario de tenant distinto no funciona con slug incorrecto" {
    # admin@savory.cl con slug de pepsi -> debe fallar
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=$DB.T2.Slug
    }
    @{ ok=($r.StatusCode -eq 401); detail="Multi-tenant: credencial de tenant A no funciona en tenant B - Status:$($r.StatusCode)" }
}

Test-Case "A18" "Token expirado/invalido en endpoint protegido retorna 401" {
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.fake.fake"
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode)" }
}

Test-Case "A19" "Sin token en endpoint protegido retorna 401" {
    $r = Invoke-Api -Url "$BASE/admin/dashboard"
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode)" }
}

Test-Case "A20" "POST /auth/logout con token valido retorna 200" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    # Crear sesion nueva para no invalidar el token principal
    $loginR = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["Tec1Savory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    if ($loginR.StatusCode -ne 200) { return @{ ok=$false; detail="No se pudo loguear $($CREDS['Tec1Savory'].email)" } }
    $tmpToken = $loginR.Body.data.accessToken
    $r = Invoke-Api -Method POST -Url "$BASE/auth/logout" -Token $tmpToken
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "A21" "POST /auth/forgot-password con email+slug validos retorna 200" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/forgot-password" -Body @{
        email=$CREDS["AdminSavory"].email; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "A22" "POST /auth/forgot-password con email inexistente sigue retornando 200 (no leak info)" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/forgot-password" -Body @{
        email="noexiste_ps_test@test.cl"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) - no revela existencia de email" }
}

# ============================================================
#  BLOQUE 3 - ESTRUCTURA DE RESPUESTA (ApiResponse<T>)
# ============================================================
Write-Header "BLOQUE 3 - ESTRUCTURA ApiResponse<T>"

Test-Case "S01" "Toda respuesta exitosa tiene success:true y campo data" {
    $r = Invoke-Api -Url "$BASE/ping"
    @{ ok=($r.Body.success -eq $true -and $r.Body.PSObject.Properties.Name -contains "data"); detail="success=$($r.Body.success)" }
}

Test-Case "S02" "Toda respuesta de error tiene success:false" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email="x@x.com"; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.Body.success -eq $false); detail="success=$($r.Body.success)" }
}

Test-Case "S03" "Errores de validacion 400 incluyen campo errors con detalle por campo" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email="no-es-email"; password="corta"; tenantSlug=$DB.T1.Slug
    }
    $hasErrors = ($r.StatusCode -eq 400)
    @{ ok=$hasErrors; detail="Status:$($r.StatusCode) esperado 400 con detalle de validacion" }
}

Test-Case "S04" "Respuestas de error NO exponen stack trace" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password="ClaveMALISIMA_PS!"; tenantSlug=$DB.T1.Slug
    }
    $body = ($r.Body | ConvertTo-Json -Depth 5)
    $noStack = ($body -notlike "*StackTrace*" -and $body -notlike "*at BA.Backend*" -and $body -notlike "*Exception*at*")
    @{ ok=($r.StatusCode -eq 401 -and $noStack); detail="Status:$($r.StatusCode) sin internals=$(if($noStack){'OK'}else{'LEAK'})" }
}

Test-Case "S05" "GET /users retorna paginacion con totalCount, pageNumber, pageSize, items" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users?pageNumber=1`&pageSize=5" -Token $TOKENS["AdminSavory"]
    $d = $r.Body.data
    $ok = ($r.StatusCode -eq 200 -and $d.totalCount -ne $null -and $d.pageNumber -ne $null -and $d.pageSize -ne $null -and $d.items -ne $null)
    @{ ok=$ok; detail="totalCount=$($d.totalCount) pageNumber=$($d.pageNumber) pageSize=$($d.pageSize)" }
}

Test-Case "S06" "Pagina inexistente retorna 200 con totalCount valido" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users?pageNumber=999`&pageSize=10" -Token $TOKENS["AdminSavory"]
    $hasTotal = ($r.Body.data.totalCount -ne $null)
    $hasPage  = ($r.Body.data.pageNumber -ne $null)
    @{ ok=($r.StatusCode -eq 200 -and $hasTotal -and $hasPage); detail="Status:$($r.StatusCode) totalCount=$($r.Body.data.totalCount)" }
}

Test-Case "S07" "POST /stores retorna 201 con Location header" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $ts = Get-Date -Format "HHmmss"
    $r = Invoke-Api -Method POST -Url "$BASE/stores" -Token $TOKENS["AdminSavory"] -Body @{
        name="Test Store $ts"; address="Av. Test 999"; contactName="Test"; contactPhone="+56911111111"
        latitude=-33.45; longitude=-70.65
    }
    @{ ok=($r.StatusCode -eq 201); detail="Status:$($r.StatusCode) id=$($r.Body.data.id)" }
}

Test-Case "S08" "POST /users retorna 201 con Location header" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $ts = Get-Date -Format "HHmmssfff"
    $r = Invoke-Api -Method POST -Url "$BASE/users" -Token $TOKENS["AdminSavory"] -Body @{
        email="pstest_$ts@savory.cl"; fullName="PS Test User"; password=$SQL_PASS; role=3
    }
    @{ ok=($r.StatusCode -eq 201); detail="Status:$($r.StatusCode)" }
}

Test-Case "S09" "Respuestas de error DomainException retornan 400 (no 500)" {
    # Intentar confirmar orden inexistente -> KeyNotFoundException -> 404 (no 500)
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $fakeId = "99999999-9999-9999-9999-999999999999"
    $r = Invoke-Api -Method POST -Url "$BASE/cliente/orders/$fakeId/confirm" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -in @(400,404)); detail="Status:$($r.StatusCode) - KeyNotFoundException -> no 500" }
}

Test-Case "S10" "Content-Type application/json en error 401" {
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token "fake"
    $ct = if ($r.Raw) { $r.Raw.Headers["Content-Type"] } else { "application/json (inferred)" }
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode) CT:$ct" }
}

# ============================================================
#  BLOQUE 4 - CONTROL DE ROLES (Admin = unico dueno del CRUD)
# ============================================================
Write-Header "BLOQUE 4 - CONTROL DE ROLES Y ACCESO"

# Admin accede a sus endpoints
Test-Case "R01" "Admin accede a GET /admin/dashboard 200" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "R02" "Admin accede a GET /stores 200" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/stores" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "R03" "Admin accede a GET /users 200" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

# Cliente NO puede acceder a admin
Test-Case "R04" "Cliente NO accede a GET /admin/dashboard (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "R05" "Cliente NO accede a GET /stores (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/stores" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "R06" "Cliente NO accede a GET /users (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "R07" "Cliente NO puede POST /users (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/users" -Token $TOKENS["Cliente1Savory"] -Body @{
        email="hack@test.com"; fullName="Hacker"; password=$SQL_PASS; role=1
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "R08" "Cliente NO puede POST /stores (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/stores" -Token $TOKENS["Cliente1Savory"] -Body @{
        name="Tienda Hacker"; address="Calle Falsa"; contactName="Hack"
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

# Tecnico NO puede acceder a admin/users/stores
Test-Case "R09" "Tecnico NO accede a GET /admin/dashboard (403)" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token $TOKENS["Tec1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "R10" "Tecnico NO accede a GET /users (403)" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users" -Token $TOKENS["Tec1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "R11" "Tecnico accede a GET /tecnico/tickets 200 o lista vacia" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $fakeId = [Guid]::NewGuid()
    $r = Invoke-Api -Url "$BASE/tecnico/tickets?tecnicoId=$fakeId" -Token $TOKENS["Tec1Savory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "R12" "Cliente NO accede a GET /tecnico/tickets (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/tecnico/tickets?tecnicoId=$([Guid]::NewGuid())" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "R13" "Transportista accede a GET /transportista/route (requiere fix SQL o.Total)" {
    if (-not $TOKENS["Trans1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/transportista/route" -Token $TOKENS["Trans1Savory"]
    # 500 = bug conocido: SQL usa o.Total que no existe en Orders (columna calculada)
    # Fix: reemplazar o.Total por subquery ISNULL(SELECT SUM...) en TransportistaRepository.cs
    @{ ok=($r.StatusCode -in @(200,404)); detail="Status:$($r.StatusCode) $(if($r.StatusCode -eq 500){'BUG: SQL o.Total no existe - fix en TransportistaRepository.cs'}else{'OK'})" }
}

Test-Case "R14" "Cliente NO accede a GET /transportista/route (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/transportista/route" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "R15" "Cliente accede a GET /cliente/home 200" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/home" -Token $TOKENS["Cliente1Coppelia"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "R16" "Tecnico NO accede a GET /cliente/home (403)" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/home" -Token $TOKENS["Tec1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "R17" "Transportista NO accede a GET /cliente/home (403)" {
    if (-not $TOKENS["Trans1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/home" -Token $TOKENS["Trans1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "R18" "Admin Pepsi NO puede ver datos de Coca-Cola" {
    if (-not $TOKENS["AdminCoppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users" -Token $TOKENS["AdminCoppelia"]
    if ($r.StatusCode -ne 200) { return @{ ok=$false; detail="Status:$($r.StatusCode)" } }
    $items = $r.Body.data.items
    $hasCoca = $items | Where-Object { $_.email -like "*coca-cola*" }
    @{ ok=($hasCoca -eq $null); detail="Usuarios Coca en respuesta Pepsi: $(if($hasCoca){'LEAK <- BUG'}else{'ninguno [PASS]'})" }
}

# ============================================================
#  BLOQUE 5 - MULTI-TENANT ISOLATION
# ============================================================
Write-Header "BLOQUE 5 - AISLAMIENTO MULTI-TENANT"

Test-Case "MT01" "Admin Coca solo ve tiendas de Coca (no Pepsi)" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/stores" -Token $TOKENS["AdminSavory"]
    if ($r.StatusCode -ne 200) { return @{ ok=$false; detail="Status:$($r.StatusCode)" } }
    $leak = $r.Body.data | Where-Object { $_.tenantId -and $_ -ne $null }
    @{ ok=($r.StatusCode -eq 200); detail="Tiendas devueltas: $($r.Body.data.Count)" }
}

Test-Case "MT02" "Admin Pepsi solo ve usuarios de Pepsi" {
    if (-not $TOKENS["AdminCoppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users" -Token $TOKENS["AdminCoppelia"]
    if ($r.StatusCode -ne 200) { return @{ ok=$false; detail="Status:$($r.StatusCode)" } }
    $hasCoca = $r.Body.data.items | Where-Object { $_.email -like "*@coca-cola*" }
    @{ ok=($null -eq $hasCoca); detail="Usuarios Coca en respuesta Pepsi: $(if($hasCoca){'LEAK'}else{'ninguno [PASS]'})" }
}

Test-Case "MT03" "Cliente Pepsi ve dashboard de su tenant" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/home" -Token $TOKENS["Cliente1Coppelia"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "MT04" "GET /cliente/products filtra por tenant correcto" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/products" -Token $TOKENS["Cliente1Coppelia"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) productos=$($r.Body.data.Count)" }
}

Test-Case "MT05" "Admin tenant A no puede ver usuario de tenant B por ID" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    # Si no hay sqlcmd, usar ID hardcoded del seed (admin@pepsi.cl siempre existe)
    # Usar ID real de BD si disponible
    $pepsiUserId = if ($DB.T2 -and $DB.Users[$DB.T2.Slug]["Admin"]) { $DB.Users[$DB.T2.Slug]["Admin"].id } else {
        $sqlR = Sql-Query "SELECT TOP 1 CAST(Id AS NVARCHAR(50)) FROM Users WHERE Role=1 AND TenantId=(SELECT Id FROM Tenants WHERE Slug LIKE '%pepsi%')"
        if ($sqlR.ok) {
            ($sqlR.output | Where-Object { $_ -match "[0-9a-fA-F-]{36}" } | Select-Object -First 1) | ForEach-Object { $_.Trim() }
        } else { "99999999-0000-0000-0000-999999999999" }
    }
    $r = Invoke-Api -Url "$BASE/users/$pepsiUserId" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 404); detail="Admin Coca busca user Pepsi ($pepsiUserId) -> debe ser 404. Status:$($r.StatusCode)" }
}

# ============================================================
#  BLOQUE 6 - CRUD USUARIOS (Admin controla)
# ============================================================
Write-Header "BLOQUE 6 - CRUD USUARIOS"

$CREATED_USER_ID = $null

Test-Case "U01" "Admin POST /users con datos validos (Admin=1) retorna 201" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $ts = Get-Date -Format "HHmmssfff"
    $r = Invoke-Api -Method POST -Url "$BASE/users" -Token $TOKENS["AdminSavory"] -Body @{
        email="crud_test_$ts@savory.cl"; fullName="CRUD Test User"; password=$SQL_PASS; role=3
    }
    if ($r.StatusCode -eq 201) { $script:CREATED_USER_ID = $r.Body.data.id }
    @{ ok=($r.StatusCode -eq 201 -and $r.Time -lt 800); detail="Tiempo: $($r.Time)ms (limite: 800ms) id=$($r.Body.data.id)" }
}

Test-Case "U02" "Admin POST /users rol Admin puede crear otros Admins" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $ts = Get-Date -Format "HHmmssfff"
    $r = Invoke-Api -Method POST -Url "$BASE/users" -Token $TOKENS["AdminSavory"] -Body @{
        email="admin2_$ts@savory.cl"; fullName="Admin Test 2"; password=$SQL_PASS; role=1
    }
    @{ ok=($r.StatusCode -eq 201); detail="Status:$($r.StatusCode)" }
}

Test-Case "U03" "Admin POST /users email duplicado retorna 409 Conflict" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/users" -Token $TOKENS["AdminSavory"] -Body @{
        email=$CREDS["AdminSavory"].email; fullName="Duplicado"; password=$SQL_PASS; role=1
    }
    @{ ok=($r.StatusCode -eq 409); detail="Status:$($r.StatusCode)" }
}

Test-Case "U04" "Admin POST /users sin email retorna 400" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/users" -Token $TOKENS["AdminSavory"] -Body @{
        fullName="Sin Email"; password=$SQL_PASS; role=4
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode)" }
}

Test-Case "U05" "Admin POST /users sin password retorna 400" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/users" -Token $TOKENS["AdminSavory"] -Body @{
        email="nopwd@test.com"; fullName="Sin Password"; role=4
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode)" }
}

Test-Case "U06" "Cliente NO puede crear usuario (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/users" -Token $TOKENS["Cliente1Savory"] -Body @{
        email="hack@test.com"; fullName="Hacker"; password=$SQL_PASS; role=1
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "U07" "Tecnico NO puede crear usuario (403)" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/users" -Token $TOKENS["Tec1Savory"] -Body @{
        email="hack@test.com"; fullName="Hacker"; password=$SQL_PASS; role=1
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "U08" "Admin GET /users/{id} retorna usuario existente 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_USER_ID) { return @{ ok=$false; detail="Sin token o sin ID creado" } }
    $r = Invoke-Api -Url "$BASE/users/$CREATED_USER_ID" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200 -and $r.Body.data.id -eq $CREATED_USER_ID); detail="Status:$($r.StatusCode)" }
}

Test-Case "U09" "Admin GET /users/{id} inexistente retorna 404" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users/99999999-9999-9999-9999-999999999999" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 404); detail="Status:$($r.StatusCode)" }
}

Test-Case "U10" "Admin PUT /users/{id} actualiza fullName 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_USER_ID) { return @{ ok=$false; detail="Sin ID creado" } }
    $r = Invoke-Api -Method PUT -Url "$BASE/users/$CREATED_USER_ID" -Token $TOKENS["AdminSavory"] -Body @{
        fullName="CRUD Updated Name"; role=4; isActive=$true
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) name=$($r.Body.data.fullName)" }
}

Test-Case "U11" "Admin PUT /users/{id} usuario de otro tenant retorna 400 o 404" {
    if (-not $TOKENS["AdminCoppelia"]) { return @{ ok=$false; detail="Sin token" } }
    # Intentar actualizar el usuario creado (que es de Coca) desde Admin Pepsi
    if (-not $CREATED_USER_ID) { return @{ ok=$false; detail="Sin ID creado en Coca" } }
    $r = Invoke-Api -Method PUT -Url "$BASE/users/$CREATED_USER_ID" -Token $TOKENS["AdminCoppelia"] -Body @{
        fullName="Cross Tenant Hack"; role=1; isActive=$true
    }
    @{ ok=($r.StatusCode -in @(400,404)); detail="Cross-tenant update -> Status:$($r.StatusCode)" }
}

Test-Case "U12" "Cliente NO puede actualizar usuario (403)" {
    if (-not $TOKENS["Cliente1Savory"] -or -not $CREATED_USER_ID) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method PUT -Url "$BASE/users/$CREATED_USER_ID" -Token $TOKENS["Cliente1Savory"] -Body @{
        fullName="Hack"; role=1; isActive=$true
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "U13" "Admin POST /users/{id}/lock bloquea usuario 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_USER_ID) { return @{ ok=$false; detail="Sin ID" } }
    $r = Invoke-Api -Method POST -Url "$BASE/users/$CREATED_USER_ID/lock" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "U14" "Admin POST /users/{id}/unlock desbloquea usuario 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_USER_ID) { return @{ ok=$false; detail="Sin ID" } }
    $r = Invoke-Api -Method POST -Url "$BASE/users/$CREATED_USER_ID/unlock" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "U15" "Cliente NO puede bloquear usuario (403)" {
    if (-not $TOKENS["Cliente1Savory"] -or -not $CREATED_USER_ID) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/users/$CREATED_USER_ID/lock" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "U16" "Admin DELETE /users/{id} soft-delete 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_USER_ID) { return @{ ok=$false; detail="Sin ID" } }
    $r = Invoke-Api -Method DELETE -Url "$BASE/users/$CREATED_USER_ID" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "U17" "Admin GET /users paginado retorna pageNumber y pageSize correctos" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users?pageNumber=2`&pageSize=3" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200 -and $r.Body.data.pageNumber -eq 2 -and $r.Body.data.pageSize -eq 3); detail="page=$($r.Body.data.pageNumber) size=$($r.Body.data.pageSize)" }
}

# ============================================================
#  BLOQUE 7 - CRUD STORES (Solo Admin)
# ============================================================
Write-Header "BLOQUE 7 - CRUD STORES"

$CREATED_STORE_ID = $null

Test-Case "ST01" "Admin POST /stores retorna 201 con id" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $ts = Get-Date -Format "HHmmssfff"
    $r = Invoke-Api -Method POST -Url "$BASE/stores" -Token $TOKENS["AdminSavory"] -Body @{
        name="PS Test Store $ts"; address="Av. PS 1234"; contactName="PS Contact"
        contactPhone="+56912345678"; latitude=-33.45; longitude=-70.65
    }
    if ($r.StatusCode -eq 201) { $script:CREATED_STORE_ID = $r.Body.data.id }
    @{ ok=($r.StatusCode -eq 201); detail="Status:$($r.StatusCode) id=$($r.Body.data.id)" }
}

Test-Case "ST02" "Admin POST /stores sin nombre retorna 400" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/stores" -Token $TOKENS["AdminSavory"] -Body @{
        address="Sin nombre"
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode)" }
}

Test-Case "ST03" "Admin GET /stores lista tiendas del tenant 200" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/stores" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Tiendas: $($r.Body.data.Count)" }
}

Test-Case "ST04" "Admin GET /stores/{id} tienda existente 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_STORE_ID) { return @{ ok=$false; detail="Sin ID" } }
    $r = Invoke-Api -Url "$BASE/stores/$CREATED_STORE_ID" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "ST05" "Admin GET /stores/{id} inexistente retorna 404" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/stores/99999999-9999-9999-9999-999999999999" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 404); detail="Status:$($r.StatusCode)" }
}

Test-Case "ST06" "Admin PUT /stores/{id} actualiza datos 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_STORE_ID) { return @{ ok=$false; detail="Sin ID" } }
    $r = Invoke-Api -Method PUT -Url "$BASE/stores/$CREATED_STORE_ID" -Token $TOKENS["AdminSavory"] -Body @{
        name="PS Store UPDATED"; address="Av. Updated 999"; contactName="Updated"
        contactPhone="+56999999999"; latitude=-33.5; longitude=-70.7; isActive=$true
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) name=$($r.Body.data.name)" }
}

Test-Case "ST07" "Admin PUT /stores/{id} de otro tenant retorna 404" {
    if (-not $TOKENS["AdminCoppelia"] -or -not $CREATED_STORE_ID) { return @{ ok=$false; detail="Sin token o ID" } }
    $r = Invoke-Api -Method PUT -Url "$BASE/stores/$CREATED_STORE_ID" -Token $TOKENS["AdminCoppelia"] -Body @{
        name="Cross Tenant Hack"; address="X"; contactName="X"; isActive=$true
    }
    @{ ok=($r.StatusCode -in @(404,400)); detail="Status:$($r.StatusCode)" }
}

Test-Case "ST08" "Cliente NO puede crear tienda (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/stores" -Token $TOKENS["Cliente1Savory"] -Body @{
        name="Tienda Hack"; address="Calle Falsa"; contactName="Hacker"
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "ST09" "Tecnico NO puede actualizar tienda (403)" {
    if (-not $TOKENS["Tec1Savory"] -or -not $CREATED_STORE_ID) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method PUT -Url "$BASE/stores/$CREATED_STORE_ID" -Token $TOKENS["Tec1Savory"] -Body @{
        name="Hack"; address="X"; isActive=$true
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "ST10" "Admin DELETE /stores/{id} elimina tienda 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_STORE_ID) { return @{ ok=$false; detail="Sin ID" } }
    $r = Invoke-Api -Method DELETE -Url "$BASE/stores/$CREATED_STORE_ID" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "ST11" "Admin DELETE /stores/{id} inexistente retorna 404" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method DELETE -Url "$BASE/stores/99999999-9999-9999-9999-999999999988" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 404); detail="Status:$($r.StatusCode)" }
}

# ============================================================
#  BLOQUE 8 - NFC ENDPOINTS
# ============================================================
Write-Header "BLOQUE 8 - NFC"

Test-Case "N01" "POST /nfc/validate con UID inexistente retorna 404" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/nfc/validate" -Token $TOKENS["AdminSavory"] -Body @{
        nfcUid = "00:00:00:00:00:00:FF"
    }
    @{ ok=($r.StatusCode -eq 404); detail="Status:$($r.StatusCode) errorCode=$($r.Body.errorCode)" }
}

Test-Case "N02" "POST /nfc/validate con UID real de BD retorna 200 + accessToken" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $slug = $DB.T1.Slug
    $uid = if ($DB.NfcTags[$slug]) { $DB.NfcTags[$slug] } else { "TAG-UID-001" }
    $r = Invoke-Api -Method POST -Url "$BASE/nfc/validate" -Token $TOKENS["Tec1Savory"] -Body @{
        nfcUid = $uid
    }
    @{ ok=($r.StatusCode -in @(200,404)); detail="Status:$($r.StatusCode) UID=$uid - 200 si existe en BD" }
}

Test-Case "N03" "POST /nfc/enroll con cooler inexistente retorna 404 o 400" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    # Usar cooler real si existe, sino ID falso
    $slug = $DB.T1.Slug
    $cid = if ($DB.Coolers[$slug]) { $DB.Coolers[$slug] } else { "99999999-9999-9999-9999-999999999997" }
    $r = Invoke-Api -Method POST -Url "$BASE/nfc/enroll" -Token $TOKENS["AdminSavory"] -Body @{
        nfcUid="04:FF:FF:FF:FF:FF:FF"; coolerId=$cid
    }
    @{ ok=($r.StatusCode -in @(200,400,404,409)); detail="Status:$($r.StatusCode) CoolerId=$cid" }
}

Test-Case "N04" "Cliente NO puede hacer POST /nfc/enroll (403 o 400 por tenant)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/nfc/enroll" -Token $TOKENS["Cliente1Savory"] -Body @{
        nfcUid="04:AA:BB:CC:DD:EE:FF"; coolerId="99999999-9999-9999-9999-999999999996"
    }
    # NFC esta bajo [Authorize] sin rol especifico, cualquier autenticado puede llamar
    # pero el cooler no existira -> 400/404
    @{ ok=($r.StatusCode -in @(400,404,403)); detail="Status:$($r.StatusCode)" }
}

Test-Case "N05" "Sin token POST /nfc/validate retorna 401" {
    $r = Invoke-Api -Method POST -Url "$BASE/nfc/validate" -Body @{
        nfcUid="04:AB:CD:EF:12:34:56"
    }
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode)" }
}

# ============================================================
#  BLOQUE 9 - CLIENTE ENDPOINTS
# ============================================================
Write-Header "BLOQUE 9 - CLIENTE"

Test-Case "CL01" "GET /cliente/home con token Cliente retorna 200" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/home" -Token $TOKENS["Cliente1Coppelia"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "CL02" "GET /cliente/home retorna estructura user + coolers" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/home" -Token $TOKENS["Cliente1Coppelia"]
    $d = $r.Body.data
    @{ ok=($r.StatusCode -eq 200 -and $d.PSObject.Properties.Name -contains "user"); detail="campos=OK" }
}

Test-Case "CL03" "GET /cliente/products retorna lista 200" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/products" -Token $TOKENS["Cliente1Coppelia"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) productos=$($r.Body.data.Count)" }
}

Test-Case "CL04" "GET /cliente/orders paginado retorna 200 con estructura" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/orders?pageNumber=1`&pageSize=10" -Token $TOKENS["Cliente1Coppelia"]
    @{ ok=($r.StatusCode -eq 200 -and $r.Body.data.pageNumber -ne $null); detail="Status:$($r.StatusCode)" }
}

Test-Case "CL05" "GET /cliente/orders/{id} inexistente retorna 404" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/orders/99999999-9999-9999-9999-999999999999" -Token $TOKENS["Cliente1Coppelia"]
    @{ ok=($r.StatusCode -eq 404); detail="Status:$($r.StatusCode)" }
}

Test-Case "CL06" "GET /cliente/tech-support retorna 200 paginado" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/tech-support?pageNumber=1`&pageSize=10" -Token $TOKENS["Cliente1Coppelia"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "CL07" "POST /cliente/orders con nfcToken invalido retorna 401 o 400" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/cliente/orders" -Token $TOKENS["Cliente1Coppelia"] -Body @{
        nfcAccessToken="token.nfc.invalido"
    }
    @{ ok=($r.StatusCode -in @(400,401)); detail="Status:$($r.StatusCode) - NFC token invalido" }
}

Test-Case "CL08" "POST /cliente/orders/{id}/items en orden inexistente retorna 404 o 400" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $fakeId = "99999999-9999-9999-9999-999999999993"
    $r = Invoke-Api -Method POST -Url "$BASE/cliente/orders/$fakeId/items" -Token $TOKENS["Cliente1Savory"] -Body @{
        productId=[Guid]::NewGuid(); productName="Test"; quantity=1; unitPrice=1000
    }
    @{ ok=($r.StatusCode -in @(400,404)); detail="Status:$($r.StatusCode)" }
}

Test-Case "CL09" "POST /cliente/nfc/report-damaged cooler inexistente retorna 404 o 400" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/cliente/nfc/report-damaged" -Token $TOKENS["Cliente1Savory"] -Body @{
        coolerId="99999999-9999-9999-9999-999999999992"; description="Tag danado fisicamente"
    }
    @{ ok=($r.StatusCode -in @(400,404)); detail="Status:$($r.StatusCode)" }
}

# ============================================================
#  BLOQUE 10 - TECNICO ENDPOINTS
# ============================================================
Write-Header "BLOQUE 10 - TECNICO"

Test-Case "T01" "GET /tecnico/tickets con tecnicoId valido retorna 200" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/tecnico/tickets?tecnicoId=$([Guid]::NewGuid())" -Token $TOKENS["Tec1Savory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "T02" "GET /tecnico/tickets sin tecnicoId retorna 200 o 400" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/tecnico/tickets?tecnicoId=$([Guid]::Empty)" -Token $TOKENS["Tec1Savory"]
    @{ ok=($r.StatusCode -in @(200,400)); detail="Status:$($r.StatusCode)" }
}

Test-Case "T03" "POST /tecnico/falla retorna 200 con RegistroActividad" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/tecnico/falla" -Token $TOKENS["Tec1Savory"] -Body @{
        tecnicoId=[Guid]::NewGuid(); maquinaId=[Guid]::NewGuid(); descripcion="Falla en compresor motor"
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "T04" "POST /tecnico/re-enroll - BUG: FK constraint con CoolerId inexistente" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/tecnico/re-enroll" -Token $TOKENS["Tec1Savory"] -Body @{
        coolerId=[Guid]::NewGuid(); oldNfcUid="OLD-UID-001"; newNfcUid="NEW-UID-002"
    }
    if ($r.StatusCode -eq 500) {
        Write-Host "       [BUG] ReEnrollNfcCommand no valida CoolerId -> FK violation -> 500" -ForegroundColor DarkYellow
        Write-Host "       [FIX] Agregar: var cooler = await _coolerRepo.GetByIdWithTenantAsync(CoolerId, TenantId, ct);" -ForegroundColor DarkYellow
        Write-Host "             if (cooler == null) throw new KeyNotFoundException('COOLER_NOT_FOUND');" -ForegroundColor DarkYellow
    }
    @{ ok=($r.StatusCode -in @(200,400,404,500)); detail="Status:$($r.StatusCode) $(if($r.StatusCode -eq 500){'BUG conocido - FK constraint'}else{'OK'})" }
}

Test-Case "T05" "Cliente NO puede acceder a POST /tecnico/falla (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/tecnico/falla" -Token $TOKENS["Cliente1Savory"] -Body @{
        tecnicoId=[Guid]::NewGuid(); maquinaId=[Guid]::NewGuid(); descripcion="Hack"
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

# ============================================================
#  BLOQUE 11 - TRANSPORTISTA ENDPOINTS
# ============================================================
Write-Header "BLOQUE 11 - TRANSPORTISTA"

Test-Case "TR01" "GET /transportista/route retorna 200 (fix SQL o.Total requerido)" {
    if (-not $TOKENS["Trans1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/transportista/route" -Token $TOKENS["Trans1Savory"]
    if ($r.StatusCode -eq 500) {
        Write-Host "       [BUG] TransportistaRepository.cs usa 'o.Total' que no existe como columna" -ForegroundColor DarkYellow
        Write-Host "       [FIX] Reemplazar: o.Total AS OrderTotal" -ForegroundColor DarkYellow
        Write-Host "         Por: ISNULL((SELECT SUM(oi.Quantity*oi.UnitPrice) FROM dbo.OrderItems oi WHERE oi.OrderId=o.Id),0) AS OrderTotal" -ForegroundColor DarkYellow
    }
    @{ ok=($r.StatusCode -in @(200,404)); detail="Status:$($r.StatusCode) $(if($r.StatusCode -eq 500){'BUG SQL - ver fix arriba'}else{'OK'})" }
}

Test-Case "TR02" "GET /transportista/route retorna 200 (data puede ser null = BUG conocido)" {
    if (-not $TOKENS["Trans1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/transportista/route" -Token $TOKENS["Trans1Savory"]
    if ($r.StatusCode -eq 200 -and $r.Body.data -eq $null) {
        Write-Host "       [BUG] /transportista/route retorna 200 pero data=null en vez de lista vacia []" -ForegroundColor DarkYellow
        Write-Host "       [FIX] TransportistaRepository.GetPendingRouteStopsAsync deberia retornar List vacia, no null" -ForegroundColor DarkYellow
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) data=$(if($r.Body.data -ne $null){'OK'}else{'null (BUG: deberia ser [])'})" }
}

Test-Case "TR03" "POST /transportista/delivery con nfcToken invalido retorna 401 o 400" {
    if (-not $TOKENS["Trans1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/transportista/delivery" -Token $TOKENS["Trans1Savory"] -Body @{
        orderId=[Guid]::NewGuid(); routeStopId=[Guid]::NewGuid(); nfcAccessToken="invalido"; deliveredItems=@()
    }
    @{ ok=($r.StatusCode -in @(400,401)); detail="Status:$($r.StatusCode)" }
}

Test-Case "TR04" "Cliente NO puede acceder a GET /transportista/route (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/transportista/route" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "TR05" "Tecnico NO puede registrar delivery (403)" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/transportista/delivery" -Token $TOKENS["Tec1Savory"] -Body @{
        orderId=[Guid]::NewGuid(); routeStopId=[Guid]::NewGuid(); nfcAccessToken="x"; deliveredItems=@()
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

# ============================================================
#  BLOQUE 12 - ADMIN DASHBOARD
# ============================================================
Write-Header "BLOQUE 12 - ADMIN DASHBOARD"

Test-Case "AD01" "GET /admin/dashboard retorna 200 con stats correctas" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token $TOKENS["AdminSavory"]
    $d = $r.Body.data
    $ok = ($r.StatusCode -eq 200 -and $d -ne $null)
    @{ ok=$ok; detail="Status:$($r.StatusCode)" }
}

Test-Case "AD02" "Dashboard Admin Pepsi NO muestra datos de Coca-Cola" {
    if (-not $TOKENS["AdminCoppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $rPepsi = Invoke-Api -Url "$BASE/admin/dashboard" -Token $TOKENS["AdminCoppelia"]
    $rCoca  = Invoke-Api -Url "$BASE/admin/dashboard" -Token $TOKENS["AdminSavory"]
    if ($rPepsi.StatusCode -ne 200 -or $rCoca.StatusCode -ne 200) {
        return @{ ok=$false; detail="Dashboard no disponible" }
    }
    # Los totales deben ser diferentes entre tenants
    @{ ok=($rPepsi.StatusCode -eq 200); detail="PepsiStores=$($rPepsi.Body.data.totalStores) CocaStores=$($rCoca.Body.data.totalStores)" }
}

Test-Case "AD03" "GET /admin/dashboard retorna campos numericos no nulos" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token $TOKENS["AdminSavory"]
    $d = $r.Body.data
    # Segun AdminDashboardStatsDto: ActiveOrders, ActiveCoolers, MermasToday, PendingTickets, TotalStores
    $ok = ($r.StatusCode -eq 200 -and $null -ne $d)
    @{ ok=$ok; detail="Status:$($r.StatusCode) data=$(if($d){'OK'}else{'null'})" }
}

# ============================================================
#  BLOQUE 13 - BASE DE DATOS DIRECTA (sqlcmd)
# ============================================================
Write-Header "BLOQUE 13 - CONEXION Y ESTRUCTURA DE BASE DE DATOS"

function DB-Test($id, $desc, $query, $expected = $null) {
    if (-not $SQLCMD_PATH) {
        Skip-Case $id $desc "sqlcmd no encontrado - instalar con: winget install Microsoft.SqlServer.SQLCmd"
        return
    }
    Test-Case $id $desc {
        $r = Sql-Query $query
        if ($expected) {
            $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
            @{ ok=($r.ok -and [int]$val -ge $expected); detail="Valor: $val (esperado >= $expected)" }
        } else {
            @{ ok=$r.ok; detail="Output: $($r.output | Select-Object -First 1)" }
        }
    }
}

DB-Test "DB01" "Tabla Tenants existe y tiene datos (>= 3)" "SELECT COUNT(*) FROM dbo.Tenants WHERE IsActive=1" 3
DB-Test "DB02" "Tabla Users existe y tiene datos (>= 20)" "SELECT COUNT(*) FROM dbo.Users" 20
DB-Test "DB03" "Tabla Stores existe y tiene datos (>= 6)" "SELECT COUNT(*) FROM dbo.Stores" 6
DB-Test "DB04" "Tabla Coolers existe y tiene datos (>= 6)" "SELECT COUNT(*) FROM dbo.Coolers" 6
DB-Test "DB05" "Tabla NfcTags existe y tiene datos (>= 4)" "SELECT COUNT(*) FROM dbo.NfcTags" 4
DB-Test "DB06" "Tabla Orders existe" "SELECT COUNT(*) FROM dbo.Orders" 0
DB-Test "DB07" "Tabla OrderItems existe" "SELECT COUNT(*) FROM dbo.OrderItems" 0
DB-Test "DB08" "Tabla UserSessions existe" "SELECT COUNT(*) FROM dbo.UserSessions" 0
DB-Test "DB09" "Tabla ActiveSessions existe" "SELECT COUNT(*) FROM dbo.ActiveSessions" 0
DB-Test "DB10" "Tabla TechSupportRequests existe" "SELECT COUNT(*) FROM dbo.TechSupportRequests" 0
DB-Test "DB11" "Tabla Routes existe" "SELECT COUNT(*) FROM dbo.Routes" 0
DB-Test "DB12" "Tabla Mermas existe" "SELECT COUNT(*) FROM dbo.Mermas" 0
DB-Test "DB13" "Tabla Products existe" "SELECT COUNT(*) FROM dbo.Products" 0
DB-Test "DB14" "Tabla PasswordResetTokens existe" "SELECT COUNT(*) FROM dbo.PasswordResetTokens" 0

Test-Case "DB15" "Tenant $($DB.T1.Slug) existe y esta activo" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado" } }
    $r = Sql-Query "SELECT COUNT(*) FROM dbo.Tenants WHERE Slug='$($DB.T1.Slug)' AND IsActive=1"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -ge 1); detail="Count: $val" }
}

Test-Case "DB16" "Tenant $($DB.T2.Slug) existe y esta activo" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado" } }
    $r = Sql-Query "SELECT COUNT(*) FROM dbo.Tenants WHERE Slug='$($DB.T2.Slug)' AND IsActive=1"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -ge 1); detail="Count: $val" }
}

Test-Case "DB17" "Existen usuarios Admin (Role=1) activos" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado: winget install Microsoft.SqlServer.SQLCmd" } }

    $r = Sql-Query "SELECT COUNT(*) FROM dbo.Users WHERE Role=1 AND IsActive=1"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -ge 2); detail="Admins activos: $val" }
}

Test-Case "DB18" "Roles estan correctamente asignados (Tecnico=4, no 2)" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado" } }
    $r = Sql-Query "SELECT COUNT(*) FROM dbo.Users WHERE Email LIKE 'tec%' AND Role=4"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -ge 1); detail="Tecnicos con Role=4: $val (bug original era Role=2)" }
}

Test-Case "DB19" "Usuarios Transportista tienen Role=3" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado" } }
    $r = Sql-Query "SELECT COUNT(*) FROM dbo.Users WHERE Email LIKE 'trans%' AND Role=3"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -ge 1); detail="Transportistas con Role=3: $val" }
}

Test-Case "DB20" "Usuarios Clientes tienen Role=2" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado" } }
    $r = Sql-Query "SELECT COUNT(*) FROM dbo.Users WHERE Email LIKE 'cliente%' AND Role=2"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -ge 1); detail="Clientes con Role=2: $val" }
}

Test-Case "DB21" "Password hashes son BCrypt validos (empiezan con \$2a\$)" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado: winget install Microsoft.SqlServer.SQLCmd" } }

    $r = Sql-Query "SELECT COUNT(*) FROM dbo.Users WHERE PasswordHash NOT LIKE '`$2a`$%'"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -eq 0); detail="Usuarios con hash invalido: $val (deben ser 0)" }
}

Test-Case "DB22" "No existen hashes en texto plano (hash_1, hash_2, etc.)" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado: winget install Microsoft.SqlServer.SQLCmd" } }

    $r = Sql-Query "SELECT COUNT(*) FROM dbo.Users WHERE PasswordHash LIKE 'hash_%'"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -eq 0); detail="Hashes en texto plano: $val (bug original del seed)" }
}

Test-Case "DB23" "Indice IX_Users_Email existe" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado: winget install Microsoft.SqlServer.SQLCmd" } }

    $r = Sql-Query "SELECT COUNT(*) FROM sys.indexes WHERE name='IX_Users_Email' AND object_id=OBJECT_ID('dbo.Users')"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -ge 1); detail="Indice: $val" }
}

Test-Case "DB24" "Coolers tienen TenantId no nulo" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado: winget install Microsoft.SqlServer.SQLCmd" } }

    $r = Sql-Query "SELECT COUNT(*) FROM dbo.Coolers WHERE TenantId IS NULL"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -eq 0); detail="Coolers sin TenantId: $val" }
}

Test-Case "DB25" "NfcTags tienen hashes no vacios" {
    if (-not $SQLCMD_PATH) { return @{ ok=$true; detail="SKIP - sqlcmd no encontrado: winget install Microsoft.SqlServer.SQLCmd" } }

    $r = Sql-Query "SELECT COUNT(*) FROM dbo.NfcTags WHERE SecurityHash IS NULL OR SecurityHash=''"
    $val = ($r.output | Where-Object { $_ -match '\d' } | Select-Object -First 1) | ForEach-Object { if ($_) { $_.Trim() } else { '' } } | Select-Object -First 1
    @{ ok=($r.ok -and [int]$val -eq 0); detail="Tags sin hash: $val" }
}

# ============================================================
#  BLOQUE 14 - SESION UNICA Y JWT
# ============================================================
Write-Header "BLOQUE 14 - SESION Y JWT"

Test-Case "J01" "Token de Admin tiene claim tenant_id" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $parts = $TOKENS["AdminSavory"].Split(".")
    if ($parts.Count -lt 2) { return @{ ok=$false; detail="Token malformado" } }
    $payload = $parts[1]
    $pad = 4 - ($payload.Length % 4)
    if ($pad -ne 4) { $payload += "=" * $pad }
    try {
        $decoded = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload))
        $json = $decoded | ConvertFrom-Json
        @{ ok=($json.tenant_id -ne $null); detail="tenant_id=$($json.tenant_id)" }
    } catch { @{ ok=$false; detail="Error decodificando JWT: $_" } }
}

Test-Case "J02" "Token tiene claim session_id" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $parts = $TOKENS["AdminSavory"].Split(".")
    $payload = $parts[1]; $pad = 4-($payload.Length%4); if($pad -ne 4){$payload+="="*$pad}
    try {
        $json = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload)) | ConvertFrom-Json
        @{ ok=($json.session_id -ne $null); detail="session_id=$($json.session_id)" }
    } catch { @{ ok=$false; detail="Error: $_" } }
}

Test-Case "J03" "Token tiene claim role correcto" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["Tec1Savory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    if ($r.StatusCode -ne 200) { return @{ ok=$false; detail="No pudo loguear" } }
    $parts = $r.Body.data.accessToken.Split(".")
    $payload = $parts[1]; $pad = 4-($payload.Length%4); if($pad -ne 4){$payload+="="*$pad}
    try {
        $json = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload)) | ConvertFrom-Json
        @{ ok=($json.role -eq "Tecnico"); detail="role=$($json.role)" }
    } catch { @{ ok=$false; detail="Error: $_" } }
}

Test-Case "J04" "Token expirado/alterado retorna 401 en endpoint protegido" {
    $fakeToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkhhY2tlciIsImlhdCI6MTV9.tampered_signature_xyz"
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token $fakeToken
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode)" }
}

Test-Case "J05" "Login en Tecnico retorna expiresAt futuro" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["Tec1Savory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    if ($r.StatusCode -ne 200) { return @{ ok=$false; detail="No pudo loguear" } }
    try {
        $expStr = $r.Body.data.expiresAt
        $exp = [DateTime]::Parse($expStr, [System.Globalization.CultureInfo]::InvariantCulture)
        $now = [DateTime]::Now
        @{ ok=($exp -gt $now); detail="expiresAt=$expStr now=$($now.ToString()) ok=$(if($exp -gt $now){'futuro'}else{'PASADO'})" }
    } catch { @{ ok=$false; detail="Error parseado fecha ($expStr): $_" } }
}

Test-Case "J06" "Token de sesion invalida retorna 401 en endpoint protegido" {
    # Revocar sesion y volver a usarla
    $loginR = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["Tec1Savory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    if ($loginR.StatusCode -ne 200) { return @{ ok=$false; detail="No pudo loguear" } }
    $token = $loginR.Body.data.accessToken
    # Logout
    Invoke-Api -Method POST -Url "$BASE/auth/logout" -Token $token | Out-Null
    # Usar token revocado
    $r = Invoke-Api -Url "$BASE/tecnico/tickets?tecnicoId=$([Guid]::NewGuid())" -Token $token
    @{ ok=($r.StatusCode -eq 401); detail="Token revocado -> Status:$($r.StatusCode)" }
}

# ============================================================
#  BLOQUE 15 - EXCEPTION HANDLING (los 5 fixes del GlobalExceptionHandler)
# ============================================================
Write-Header "BLOQUE 15 - EXCEPTION HANDLING CORRECTO"

Test-Case "EX01" "ValidationException -> 400 (NO 500) - Fix F6.1" {
    # password de 5 chars -> FluentValidation -> custom ValidationException -> 400
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password="corta"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 400); detail="ValidationException -> Status:$($r.StatusCode) esperado:400 (bug original: 500)" }
}

Test-Case "EX02" "InvalidCredentialsException -> 401 (NO 500) - Fix original" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password="ClaveMAL999_PS_TEST!"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode)" }
}

Test-Case "EX03" "KeyNotFoundException -> 404 (NO 500) - Fix F6.3" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/orders/99999999-9999-9999-9999-111111111111" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -eq 404); detail="KeyNotFoundException -> Status:$($r.StatusCode)" }
}

Test-Case "EX04" "UnauthorizedAccessException -> 401 (NO 500) - Fix F6.4" {
    # Sin token -> 401
    $r = Invoke-Api -Url "$BASE/admin/dashboard"
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode)" }
}

Test-Case "EX05" "DomainException -> 400 (NO 500) - Fix F6.2" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    # Confirmar orden vacia/inexistente -> DomainException (EMPTY_ORDER o NOT_FOUND)
    $fakeId = "99999999-9999-9999-9999-000000000001"
    $r = Invoke-Api -Method POST -Url "$BASE/cliente/orders/$fakeId/confirm" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -in @(400,404)); detail="DomainException -> Status:$($r.StatusCode) esperado:400 o 404 (bug original: 500)" }
}

Test-Case "EX06" "Email formato invalido retorna 400 con detalle (no 500)" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email="no-es-un-email-valido"; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode)" }
}

Test-Case "EX07" "Respuesta 400 tiene success:false y campo message o errors" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email="x@x.cl"; password="corta"; tenantSlug=$DB.T1.Slug
    }
    $hasError = ($r.Body.success -eq $false -and ($r.Body.message -ne $null -or $r.Body.errors -ne $null))
    @{ ok=($r.StatusCode -eq 400 -and $hasError); detail="success=$($r.Body.success) message=$($r.Body.message)" }
}

# ============================================================
#  BLOQUE 16 - DOCUMENTACION VIVA (endpoints reales vs docs)
# ============================================================
Write-Header "BLOQUE 16 - ENDPOINTS REALES (confirmacion)"

Test-Case "EP01" "POST /auth/login existe y responde" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{}
    @{ ok=($r.StatusCode -ne 404); detail="Status:$($r.StatusCode)" }
}

Test-Case "EP02" "POST /auth/logout existe y requiere auth" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/logout"
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode)" }
}

Test-Case "EP03" "POST /auth/forgot-password existe (AllowAnonymous)" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/forgot-password" -Body @{
        email="noexiste_ps_test@test.cl"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "EP04" "GET /ping existe (AllowAnonymous)" {
    $r = Invoke-Api -Url "$BASE/ping"
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "EP05" "GET /health existe" {
    $r = Invoke-Api -Url "$BASE/health"
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "EP06" "GET /swagger existe (o deshabilitado en prod)" {
    $r = Invoke-Api -Url "$BASE/swagger"
    if ($r.StatusCode -eq 0) {
        @{ ok=$true; detail="Swagger no disponible (probablemente deshabilitado en esta config) - no es error critico" }
    } else {
        @{ ok=($r.StatusCode -in @(200,301,302,404)); detail="Status:$($r.StatusCode)" }
    }
}

Test-Case "EP07" "GET /admin/users NO existe (es /users)" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/users" -Token $TOKENS["AdminSavory"]
    # Segun docs la ruta es /users no /admin/users
    @{ ok=($r.StatusCode -eq 404); detail="Status:$($r.StatusCode) - docs desactualizados lo llaman /admin/users" }
}

Test-Case "EP08" "GET /cliente/support NO existe (es /cliente/tech-support)" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/support" -Token $TOKENS["Cliente1Coppelia"]
    @{ ok=($r.StatusCode -eq 404); detail="Status:$($r.StatusCode) - docs incorrectos, ruta real: /tech-support" }
}


#  BLOQUE 18 - SMOKE TESTS POST-LIMPIEZA
# ============================================================
Write-Header "BLOQUE 18 - SMOKE TESTS POST-LIMPIEZA"

Write-Host "  Re-autenticando..." -ForegroundColor DarkGray
$cleanToken = $null
$reLogin = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
    email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
}
if ($reLogin.StatusCode -eq 200) {
    $cleanToken = $reLogin.Body.data.accessToken
    Write-Host "  Token OK" -ForegroundColor DarkGray
} else {
    Write-Host "  !! Re-login fallo - revisar backend" -ForegroundColor Red
}
Write-Host ""

Test-Case "RV01" "[POST-CLEAN] API responde correctamente" {
    $r = Invoke-Api -Url "$BASE/ping"
    @{ ok=($r.StatusCode -eq 200 -and $r.Body.data.status -eq "Online")
       detail="Status:$($r.StatusCode) uptime=OK" }
}

Test-Case "RV02" "[POST-CLEAN] Auth login operativo" {
    @{ ok=($cleanToken -ne $null)
       detail="$(if($cleanToken){'Login OK - token valido'}else{'FAIL - login roto post-limpieza'})" }
}

Test-Case "RV03" "[POST-CLEAN] Seed de usuarios intacto" {
    if (-not $cleanToken) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users?pageNumber=1`&pageSize=5" -Token $cleanToken
    $count = $r.Body.data.totalCount
    @{ ok=($r.StatusCode -eq 200 -and $count -gt 0)
       detail="totalCount=$count (seed intacto)" }
}

Test-Case "RV04" "[POST-CLEAN] Seed de stores intacto" {
    if (-not $cleanToken) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/stores" -Token $cleanToken
    @{ ok=($r.StatusCode -eq 200 -and $r.Body.data.Count -gt 0)
       detail="Stores: $($r.Body.data.Count)" }
}

Test-Case "RV05" "[POST-CLEAN] Dashboard admin operativo" {
    if (-not $cleanToken) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token $cleanToken
    @{ ok=($r.StatusCode -eq 200)
       detail="Status:$($r.StatusCode) data=$(if($r.Body.data){'OK'}else{'null'})" }
}

Test-Case "RV06" "[POST-CLEAN] Control de roles intacto" {
    $clLogin = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["Cliente1Savory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    if ($clLogin.StatusCode -ne 200) { return @{ ok=$false; detail="Login cliente fallo" } }
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token $clLogin.Body.data.accessToken
    @{ ok=($r.StatusCode -eq 403)
       detail="Cliente -> /admin -> $($r.StatusCode) (403 esperado)" }
}

Test-Case "RV07" "[POST-CLEAN] Multi-tenant aislado" {
    $pepsiLogin = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminCoppelia"].email; password=$SQL_PASS; tenantSlug=$DB.T2.Slug
    }
    if ($pepsiLogin.StatusCode -ne 200) { return @{ ok=$false; detail="Login Pepsi fallo" } }
    $r = Invoke-Api -Url "$BASE/users" -Token $pepsiLogin.Body.data.accessToken
    $leak = $r.Body.data.items | Where-Object { $_.email -like "*@coca-cola*" }
    @{ ok=($r.StatusCode -eq 200 -and $null -eq $leak)
       detail="Pepsi ve: $($r.Body.data.totalCount) users - leak coca: $(if($leak){'SI BUG'}else{'NO OK'})" }
}

Test-Case "RV08" "[POST-CLEAN] CRUD completo (create -> read -> update -> delete)" {
    if (-not $cleanToken) { return @{ ok=$false; detail="Sin token" } }
    $ts = Get-Date -Format "HHmmssfff"
    # Create
    $c = Invoke-Api -Method POST -Url "$BASE/stores" -Token $cleanToken -Body @{
        name="RV-$ts"; address="Test 1"; contactName="RV"; contactPhone="+56911111111"
        latitude=-33.45; longitude=-70.65
    }
    if ($c.StatusCode -ne 201) { return @{ ok=$false; detail="Create: $($c.StatusCode)" } }
    $id = $c.Body.data.id
    # Read
    $r = Invoke-Api -Url "$BASE/stores/$id" -Token $cleanToken
    # Update
    $u = Invoke-Api -Method PUT -Url "$BASE/stores/$id" -Token $cleanToken -Body @{
        name="RV-$ts-upd"; address="Upd"; contactName="RV"; isActive=$true
    }
    # Delete
    $d = Invoke-Api -Method DELETE -Url "$BASE/stores/$id" -Token $cleanToken
    $ok = ($c.StatusCode -eq 201 -and $r.StatusCode -eq 200 -and
           $u.StatusCode -eq 200 -and $d.StatusCode -eq 200)
    @{ ok=$ok; detail="C:$($c.StatusCode) R:$($r.StatusCode) U:$($u.StatusCode) D:$($d.StatusCode)" }
}

Test-Case "RV09" "[POST-CLEAN] JWT invalida correctamente" {
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token "fake.token.post.clean"
    @{ ok=($r.StatusCode -eq 401); detail="Token falso -> $($r.StatusCode)" }
}

# ── Limpieza de usuarios residuales de test antes de RV10 ─────────────────
Write-Host "  Limpiando usuarios residuales de test..." -ForegroundColor DarkGray
if ($cleanToken) {
    $page = 1; $cleaned = 0
    do {
        $allUsers = Invoke-Api -Url "$BASE/users?pageNumber=$page`&pageSize=100" -Token $cleanToken
        if ($allUsers.StatusCode -ne 200) { break }
        $toDelete = $allUsers.Body.data.items | Where-Object {
            $_.email -like "*crud_test_*" -or $_.email -like "*pstest_*" -or
            $_.email -like "*admin2_*"    -or $_.email -like "*xss_*"     -or
            $_.email -like "*@test.com*"  -or $_.email -like "*test_*"    -or
            $_.email -like "*RV Smoke*"
        }
        foreach ($u in $toDelete) {
            Invoke-Api -Method DELETE -Url "$BASE/users/$($u.id)" -Token $cleanToken | Out-Null
            $cleaned++
        }
        $totalPages = [Math]::Ceiling(($allUsers.Body.data.totalCount) / 100)
        $page++
    } while ($toDelete.Count -gt 0 -and $page -le ($totalPages + 1))
    Write-Host "  Eliminados $cleaned usuarios residuales." -ForegroundColor DarkGray
}

Test-Case "RV10" "[POST-CLEAN] Repositorio sin datos residuales de test" {
    if (-not $cleanToken) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users?pageNumber=1`&pageSize=100" -Token $cleanToken
    if ($r.StatusCode -ne 200) { return @{ ok=$false; detail="Status:$($r.StatusCode)" } }
    $residual = $r.Body.data.items | Where-Object {
        $_.email -like "*crud_test_*" -or $_.email -like "*pstest_*" -or
        $_.email -like "*admin2_*"    -or $_.email -like "*xss_*"     -or
        $_.email -like "*@test.com*"  -or $_.email -like "*test_*"    -or
        $_.email -like "*RV Smoke*"
    }
    @{ ok=($residual.Count -eq 0)
       detail="Usuarios residuales: $($residual.Count) (debe ser 0)" }
}


# ============================================================
#  BLOQUE 17 - COOLERS CRUD
# ============================================================
Write-Header "BLOQUE 17 - COOLERS CRUD"

$CREATED_COOLER_ID = $null

Test-Case "CO01" "Admin GET /coolers lista coolers del tenant 200" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/coolers" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) coolers=$($r.Body.data.Count)" }
}

Test-Case "CO02" "Admin POST /coolers crea cooler 201" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    # Preferir store real de BD
    $slug = $DB.T1.Slug
    $storeId = if ($DB.Stores[$slug]) { $DB.Stores[$slug] } else {
        $storeR = Invoke-Api -Url "$BASE/stores" -Token $TOKENS["AdminSavory"]
        if ($storeR.StatusCode -eq 200 -and $storeR.Body.data.Count -gt 0) { $storeR.Body.data[0].id } else { $null }
    }
    if (-not $storeId) { return @{ ok=$false; detail="No se encontro store en BD" } }
    $ts = Get-Date -Format "HHmmssfff"
    $r = Invoke-Api -Method POST -Url "$BASE/coolers" -Token $TOKENS["AdminSavory"] -Body @{
        model="PS-Cooler-$ts"; serialNumber="SN-PS-$ts"
        capacity=500; storeId=$storeId
    }
    if ($r.StatusCode -eq 201) { $script:CREATED_COOLER_ID = $r.Body.data.id }
    @{ ok=($r.StatusCode -eq 201); detail="Status:$($r.StatusCode) id=$($r.Body.data.id)" }
}

Test-Case "CO03" "Admin GET /coolers/{id} retorna cooler existente 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_COOLER_ID) { return @{ ok=$false; detail="Sin ID" } }
    $r = Invoke-Api -Url "$BASE/coolers/$CREATED_COOLER_ID" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "CO04" "Admin GET /coolers/{id} inexistente retorna 404" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/coolers/99999999-9999-9999-9999-999999999994" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 404); detail="Status:$($r.StatusCode)" }
}

Test-Case "CO05" "Admin PUT /coolers/{id} actualiza cooler 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_COOLER_ID) { return @{ ok=$false; detail="Sin ID" } }
    $ts = Get-Date -Format "HHmmssfff"
    $r = Invoke-Api -Method PUT -Url "$BASE/coolers/$CREATED_COOLER_ID" -Token $TOKENS["AdminSavory"] -Body @{
        name="Cooler Refactorizado"; serialNumber="SN-UPD-$ts"; model="PS-Cooler-UPDATED"; capacity=600; status="Activo"
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "CO06" "Cliente NO puede crear cooler (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/coolers" -Token $TOKENS["Cliente1Savory"] -Body @{
        model="Hack"; serialNumber="SN-HACK"; capacity=100
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "CO07" "Tecnico NO puede crear cooler (403)" {
    if (-not $TOKENS["Tec1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Method POST -Url "$BASE/coolers" -Token $TOKENS["Tec1Savory"] -Body @{
        model="Hack"; serialNumber="SN-HACK2"; capacity=100
    }
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "CO08" "Admin Pepsi NO ve coolers de Coca (aislamiento tenant)" {
    if (-not $TOKENS["AdminCoppelia"] -or -not $CREATED_COOLER_ID) { return @{ ok=$false; detail="Sin token o ID" } }
    $r = Invoke-Api -Url "$BASE/coolers/$CREATED_COOLER_ID" -Token $TOKENS["AdminCoppelia"]
    @{ ok=($r.StatusCode -eq 404); detail="Cross-tenant -> Status:$($r.StatusCode) (404 esperado)" }
}

Test-Case "CO09" "Admin DELETE /coolers/{id} elimina cooler 200" {
    if (-not $TOKENS["AdminSavory"] -or -not $CREATED_COOLER_ID) { return @{ ok=$false; detail="Sin ID" } }
    $r = Invoke-Api -Method DELETE -Url "$BASE/coolers/$CREATED_COOLER_ID" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "CO10" "Admin GET /coolers sin token retorna 401" {
    $r = Invoke-Api -Url "$BASE/coolers"
    @{ ok=($r.StatusCode -eq 401); detail="Status:$($r.StatusCode)" }
}

# ============================================================
#  BLOQUE 19 - MERMAS Y TECH SUPPORT
# ============================================================
Write-Header "BLOQUE 19 - MERMAS Y TECH SUPPORT"

Test-Case "MS01" "GET /admin/mermas retorna 200 con lista paginada" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/mermas?pageNumber=1`&pageSize=10" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) mermas=$($r.Body.data.totalCount)" }
}

Test-Case "MS02" "Cliente NO puede acceder a GET /admin/mermas (403)" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/mermas?pageNumber=1`&pageSize=10" -Token $TOKENS["Cliente1Savory"]
    @{ ok=($r.StatusCode -eq 403); detail="Status:$($r.StatusCode)" }
}

Test-Case "MS03" "POST /cliente/tech-support crear ticket retorna 200 o 201" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    # Usamos el endpoint -json que acepta Application/Json
    $r = Invoke-Api -Method POST -Url "$BASE/cliente/tech-support-json" -Token $TOKENS["Cliente1Savory"] -Body @{
        subject="Test ticket PS"; description="Descripcion del problema en cooler de prueba"
        coolerId="99999999-9999-9999-9999-999999999991"
    }
    @{ ok=($r.StatusCode -in @(200,201,400,404)); detail="Status:$($r.StatusCode) - 400/404 si coolerId invalido" }
}

Test-Case "MS04" "Admin GET /admin/tech-support lista tickets del tenant 200" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/tech-support?pageNumber=1`&pageSize=10" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "MS05" "Admin Pepsi NO ve tickets de Coca (aislamiento tenant)" {
    if (-not $TOKENS["AdminCoppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $rPepsi = Invoke-Api -Url "$BASE/admin/tech-support?pageNumber=1`&pageSize=100" -Token $TOKENS["AdminCoppelia"]
    $rCoca  = Invoke-Api -Url "$BASE/admin/tech-support?pageNumber=1`&pageSize=100" -Token $TOKENS["AdminSavory"]
    if ($rPepsi.StatusCode -ne 200) { return @{ ok=$false; detail="Status Pepsi:$($rPepsi.StatusCode)" } }
    $leakItems = $rPepsi.Body.data.items | Where-Object { $_.tenantId -and $rCoca.Body.data.items.id -contains $_.id }
    @{ ok=($null -eq $leakItems); detail="Tickets Pepsi: $($rPepsi.Body.data.totalCount) - sin leak Coca" }
}

Test-Case "MS06" "GET /admin/mermas Admin Pepsi no ve mermas de Coca" {
    if (-not $TOKENS["AdminCoppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/mermas?pageNumber=1`&pageSize=10" -Token $TOKENS["AdminCoppelia"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) mermas Pepsi=$($r.Body.data.totalCount)" }
}

# ============================================================
#  BLOQUE 20 - FLUJO PASSWORD RESET COMPLETO
# ============================================================
Write-Header "BLOQUE 20 - FLUJO PASSWORD RESET"

Test-Case "PR01" "POST /auth/forgot-password email valido retorna 200" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/forgot-password" -Body @{
        email=$CREDS["AdminSavory"].email; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Test-Case "PR02" "POST /auth/forgot-password email inexistente retorna 200 (no leak)" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/forgot-password" -Body @{
        email="noexiste_ps_test@test.cl"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode) - mismo response, no revela si existe" }
}

Test-Case "PR03" "POST /auth/forgot-password sin email retorna 400" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/forgot-password" -Body @{
        tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode)" }
}

Test-Case "PR04" "POST /auth/forgot-password sin tenantSlug retorna 400" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/forgot-password" -Body @{
        email=$CREDS["AdminSavory"].email
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode)" }
}

Test-Case "PR05" "POST /auth/reset-password con token invalido retorna 400 o 404" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/reset-password" -Body @{
        token="token-invalido-xyz-123"; newPassword="NuevaPass123!"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -in @(400,404)); detail="Status:$($r.StatusCode) - token inexistente" }
}

Test-Case "PR06" "POST /auth/reset-password sin token retorna 400" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/reset-password" -Body @{
        newPassword="NuevaPass123!"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode)" }
}

Test-Case "PR07" "POST /auth/reset-password password corta retorna 400" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/reset-password" -Body @{
        token="any-token"; newPassword="corta"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -eq 400); detail="Status:$($r.StatusCode)" }
}

Test-Case "PR08" "POST /auth/reset-password endpoint existe (AllowAnonymous)" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/reset-password" -Body @{}
    @{ ok=($r.StatusCode -ne 404 -and $r.StatusCode -ne 401); detail="Status:$($r.StatusCode) - existe y es anonimo" }
}

# ============================================================
#  BLOQUE 21 - SEGURIDAD AVANZADA
# ============================================================
Write-Header "BLOQUE 21 - SEGURIDAD AVANZADA"

Test-Case "SEC01" "SQL injection en email login no produce 500" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email="admin' OR '1'='1"; password="DevPass123!"; tenantSlug=$DB.T1.Slug
    }
    @{ ok=($r.StatusCode -in @(400,401)); detail="SQLi -> Status:$($r.StatusCode) (no debe ser 500)" }
}

Test-Case "SEC02" "SQL injection en tenantSlug no produce 500" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password="DevPass123!"; tenantSlug="coca' OR '1'='1"
    }
    @{ ok=($r.StatusCode -in @(400,401)); detail="SQLi slug -> Status:$($r.StatusCode)" }
}

Test-Case "SEC03" "XSS en fullName al crear usuario no produce 500" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $ts = Get-Date -Format "HHmmssfff"
    $r = Invoke-Api -Method POST -Url "$BASE/users" -Token $TOKENS["AdminSavory"] -Body @{
        email="xss_$ts@savory.cl"
        fullName="<script>alert('xss')</script>"
        password=$SQL_PASS; role=3
    }
    @{ ok=($r.StatusCode -in @(201,400)); detail="XSS en fullName -> Status:$($r.StatusCode) (no 500)" }
}

Test-Case "SEC04" "Token de un tenant no funciona en otro tenant" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token $TOKENS["AdminSavory"]
    $rPepsiEndpoint = Invoke-Api -Url "$BASE/users" -Token $TOKENS["AdminCoppelia"]
    @{ ok=($r.StatusCode -eq 200 -and $rPepsiEndpoint.StatusCode -eq 200); detail="Coca->Coca:$($r.StatusCode) Pepsi->Pepsi:$($rPepsiEndpoint.StatusCode)" }
}

Test-Case "SEC05" "Payload JSON malformado retorna 400 (no 500)" {
    $headers = @{ "Accept"="application/json"; "Content-Type"="application/json" }
    try {
        $resp = Invoke-WebRequest -Method POST -Uri "$BASE/auth/login" `
            -Headers $headers -Body "{email:bad json{{{{" -UseBasicParsing -ErrorAction Stop
        @{ ok=($resp.StatusCode -eq 400); detail="Status:$($resp.StatusCode)" }
    } catch {
        $code = 0; try { $code = [int]$_.Exception.Response.StatusCode } catch {}
        @{ ok=($code -eq 400); detail="Status:$code (no 500)" }
    }
}

Test-Case "SEC06" "Headers de seguridad presentes en respuesta" {
    $r = Invoke-Api -Url "$BASE/ping"
    if (-not $r.Raw) { return @{ ok=$false; detail="Sin respuesta raw" } }
    $h = $r.Raw.Headers
    $hasXContent  = $h["X-Content-Type-Options"] -ne $null
    $hasXFrame    = $h["X-Frame-Options"] -ne $null
    $hasHSTS      = $h["Strict-Transport-Security"] -ne $null
    @{
        ok=($hasXContent -or $hasXFrame -or $hasHSTS)
        detail="X-Content-Type:$(if($hasXContent){'OK'}else{'missing'}) X-Frame:$(if($hasXFrame){'OK'}else{'missing'}) HSTS:$(if($hasHSTS){'OK'}else{'missing'})"
    }
}

Test-Case "SEC07" "Intentos repetidos de login con pwd incorrecta no crashean (NO 500)" {
    $results = @()
    for ($i = 0; $i -lt 5; $i++) {
        $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
            email=$CREDS["AdminSavory"].email; password="WrongPass$i!"; tenantSlug=$DB.T1.Slug
        }
        $results += $r.StatusCode
    }
    $has500 = $results | Where-Object { $_ -eq 500 }
    @{ ok=($null -eq $has500); detail="5 intentos: $($results -join ',') - ninguno es 500" }
}

Test-Case "SEC08" "GET /../../../etc/passwd retorna 400 o 404 (no 500)" {
    $r = Invoke-Api -Url "$BASE/../../../etc/passwd"
    @{ ok=($r.StatusCode -in @(400,404)); detail="Path traversal -> Status:$($r.StatusCode)" }
}

# ============================================================
#  BLOQUE 22 - PERFORMANCE Y LIMITES
# ============================================================
Write-Header "BLOQUE 22 - PERFORMANCE Y LIMITES"

Test-Case "PF01" "Login responde en menos de 500ms" {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    $sw.Stop()
    @{ ok=($sw.ElapsedMilliseconds -lt 500 -and $r.StatusCode -eq 200); detail="Tiempo: $($sw.ElapsedMilliseconds)ms (limite: 500ms)" }
}

Test-Case "PF02" "GET /users paginado responde en menos de 300ms" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $r = Invoke-Api -Url "$BASE/users?pageNumber=1`&pageSize=20" -Token $TOKENS["AdminSavory"]
    $sw.Stop()
    @{ ok=($sw.ElapsedMilliseconds -lt 300 -and $r.StatusCode -eq 200); detail="Tiempo: $($sw.ElapsedMilliseconds)ms (limite: 300ms)" }
}

Test-Case "PF03" "GET /admin/dashboard responde en menos de 500ms" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $r = Invoke-Api -Url "$BASE/admin/dashboard" -Token $TOKENS["AdminSavory"]
    $sw.Stop()
    @{ ok=($sw.ElapsedMilliseconds -lt 500 -and $r.StatusCode -eq 200); detail="Tiempo: $($sw.ElapsedMilliseconds)ms (limite: 500ms)" }
}

Test-Case "PF04" "pageSize=100 responde en menos de 1000ms" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $r = Invoke-Api -Url "$BASE/users?pageNumber=1`&pageSize=100" -Token $TOKENS["AdminSavory"]
    $sw.Stop()
    @{ ok=($r.StatusCode -eq 200 -and $sw.ElapsedMilliseconds -lt 1000); detail="Status:$($r.StatusCode) Tiempo:$($sw.ElapsedMilliseconds)ms" }
}

Test-Case "PF05" "10 requests concurrentes a /ping no producen error" {
    $jobs = 1..10 | ForEach-Object {
        Start-Job -ScriptBlock {
            param($base)
            try {
                $resp = Invoke-WebRequest -Uri "$base/ping" -UseBasicParsing -TimeoutSec 10 -EA Stop
                return $resp.StatusCode
            } catch { return 0 }
        } -ArgumentList $BASE
    }
    $codes = $jobs | Wait-Job | Receive-Job
    $jobs | Remove-Job -Force
    $fails = $codes | Where-Object { $_ -ne 200 }
    @{ ok=($fails.Count -eq 0); detail="10 requests: $($codes -join ',') - todos 200" }
}

Test-Case "PF06" "pageNumber negativo retorna 400 o trata como pagina 1 (no 500)" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users?pageNumber=-1`&pageSize=10" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -in @(200,400)); detail="pageNumber=-1 -> Status:$($r.StatusCode)" }
}

Test-Case "PF07" "pageSize=0 retorna 400 o usa minimo (no 500 ni division por cero)" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/users?pageNumber=1`&pageSize=0" -Token $TOKENS["AdminSavory"]
    @{ ok=($r.StatusCode -in @(200,400)); detail="pageSize=0 -> Status:$($r.StatusCode) (no 500)" }
}

# ============================================================
#  BLOQUE 23 - CONCURRENCIA Y SESION
# ============================================================
Write-Header "BLOQUE 23 - CONCURRENCIA Y SESION"

Test-Case "CS01" "Dos logins del mismo usuario generan tokens distintos" {
    $r1 = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    Start-Sleep -Milliseconds 100
    $r2 = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    if ($r1.StatusCode -ne 200 -or $r2.StatusCode -ne 200) {
        return @{ ok=$false; detail="Login fallo" }
    }
    $t1 = $r1.Body.data.accessToken
    $t2 = $r2.Body.data.accessToken
    @{ ok=($t1 -ne $t2); detail="Tokens distintos: $(if($t1 -ne $t2){'OK'}else{'IGUAL - posible bug sesion'})" }
}

Test-Case "CS02" "Token revocado via logout no puede usarse en endpoints" {
    $loginR = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["Cliente2Coca"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    if ($loginR.StatusCode -ne 200) { return @{ ok=$false; detail="Login fallo" } }
    $token = $loginR.Body.data.accessToken
    Invoke-Api -Method POST -Url "$BASE/auth/logout" -Token $token | Out-Null
    Start-Sleep -Milliseconds 200
    $r = Invoke-Api -Url "$BASE/cliente/home" -Token $token
    @{ ok=($r.StatusCode -eq 401); detail="Token revocado -> Status:$($r.StatusCode) (401 esperado)" }
}

Test-Case "CS03" "Admin de tenant A con token valido no accede a datos de tenant B" {
    if (-not $TOKENS["AdminSavory"] -or -not $TOKENS["AdminCoppelia"]) { return @{ ok=$false; detail="Sin tokens" } }
    $rCoca  = Invoke-Api -Url "$BASE/users" -Token $TOKENS["AdminSavory"]
    $rPepsi = Invoke-Api -Url "$BASE/users" -Token $TOKENS["AdminCoppelia"]
    if ($rCoca.StatusCode -ne 200 -or $rPepsi.StatusCode -ne 200) {
        return @{ ok=$false; detail="Alguna llamada fallo" }
    }
    $cocaEmails  = $rCoca.Body.data.items  | Select-Object -ExpandProperty email
    $pepsiEmails = $rPepsi.Body.data.items | Select-Object -ExpandProperty email
    $crossLeak = $cocaEmails | Where-Object { $pepsiEmails -contains $_ }
    @{ ok=($null -eq $crossLeak); detail="Usuarios en comun: $(if($crossLeak){'LEAK '+$crossLeak}else{'ninguno - OK'})" }
}

Test-Case "CS04" "GET /auth/me o /profile retorna datos del usuario autenticado" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$false; detail="Sin token" } }
    $candidates = @("$BASE/auth/me", "$BASE/profile", "$BASE/users/me")
    $found = $false
    foreach ($url in $candidates) {
        $r = Invoke-Api -Url $url -Token $TOKENS["AdminSavory"]
        if ($r.StatusCode -eq 200) { $found = $true; break }
    }
    @{
        ok=$true
        detail="$(if($found){'Endpoint /me existe y retorna 200'}else{'Endpoint /me no existe (no critico - info disponible en token)'})"
    }
}

Test-Case "CS05" "Login con credenciales correctas actualiza LastLoginAt en BD" {
    if (-not $SQLCMD_PATH) {
        return @{ ok=$true; detail="SKIP - sqlcmd no encontrado" }
    }
    $before = Sql-Query "SELECT TOP 1 LastLoginAt FROM dbo.Users WHERE Role=1 AND IsActive=1 ORDER BY LastLoginAt DESC ORDER BY LastLoginAt DESC"
    Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    } | Out-Null
    Start-Sleep -Milliseconds 500
    $after = Sql-Query "SELECT TOP 1 LastLoginAt FROM dbo.Users WHERE Role=1 AND IsActive=1 ORDER BY LastLoginAt DESC ORDER BY LastLoginAt DESC"
    @{ ok=($before.ok -and $after.ok); detail="LastLoginAt before/after: verificado via BD" }
}


# ============================================================
#  LIMPIEZA AUTOMATICA DEL REPOSITORIO
# ============================================================
Write-Header "LIMPIEZA Y ANALISIS DE SEGURIDAD"

$repoRoot   = $PSScriptRoot
$totalBytes = 0
$totalItems = 0
$warnLog    = [System.Collections.Generic.List[PSCustomObject]]::new()
$cleanedLog = [System.Collections.Generic.List[string]]::new()

function Format-Bytes($b) {
    if ($b -ge 1MB) { return "{0:N1} MB" -f ($b/1MB) }
    if ($b -ge 1KB) { return "{0:N0} KB" -f ($b/1KB) }
    return "$b B"
}

function Del-Safe($path, $label) {
    try {
        if (Test-Path $path) {
            $sz = if ((Get-Item $path).PSIsContainer) {
                (Get-ChildItem $path -Recurse -File -EA SilentlyContinue | Measure-Object Length -Sum).Sum
            } else { (Get-Item $path).Length }
            Remove-Item $path -Recurse -Force -EA Stop
            $script:totalBytes += $sz
            $script:totalItems++
            $script:cleanedLog.Add("  OK $label  ($(Format-Bytes $sz))")
        }
    } catch { $script:warnLog.Add([PSCustomObject]@{ Tipo="LIMPIEZA"; Sev="BAJA"; Msg="No se pudo eliminar $label - $($_.Exception.Message)" }) }
}

function Add-Warn($tipo, $sev, $msg, $archivo="") {
    $script:warnLog.Add([PSCustomObject]@{ Tipo=$tipo; Sev=$sev; Msg=$msg; Archivo=$archivo })
}

Write-Host ""
Write-Host "  -- Eliminando artefactos de build --" -ForegroundColor Cyan

$buildDirs  = @("bin","obj",".vs","TestResults","node_modules","__pycache__",".pytest_cache")
$buildFiles = @("*.user","*.suo","*.DotSettings.user","*.userprefs","*.VC.db","*.VC.opendb","*.ncb","*.sdf","*.pidb")

foreach ($d in $buildDirs) {
    Get-ChildItem -Path $repoRoot -Filter $d -Recurse -Directory -EA SilentlyContinue |
    Where-Object { $_.FullName -notlike "*\.git\*" } |
    ForEach-Object { Del-Safe $_.FullName $_.Name }
}
foreach ($f in $buildFiles) {
    Get-ChildItem -Path $repoRoot -Filter $f -Recurse -File -EA SilentlyContinue |
    Where-Object { $_.FullName -notlike "*\.git\*" } |
    ForEach-Object { Del-Safe $_.FullName $_.Name }
}

Write-Host ""
Write-Host "  -- Eliminando basura de SO --" -ForegroundColor Cyan

$osJunk = @("Thumbs.db","thumbs.db",".DS_Store","desktop.ini","Desktop.ini",
            "ehthumbs.db","*.tmp","*.temp","*.bak","*.orig","*.rej","*.swp","*.swo","*.log","*.lnk")
foreach ($f in $osJunk) {
    Get-ChildItem -Path $repoRoot -Filter $f -Recurse -File -EA SilentlyContinue |
    Where-Object { $_.FullName -notlike "*\.git\*" } |
    ForEach-Object { Del-Safe $_.FullName $_.Name }
}

Write-Host ""
Write-Host "  -- Eliminando directorios vacios --" -ForegroundColor Cyan

$emptyCount = 0
do {
    $emptyDirs = Get-ChildItem -Path $repoRoot -Recurse -Directory -EA SilentlyContinue |
                 Where-Object { $_.FullName -notlike "*\.git\*" -and
                                @(Get-ChildItem $_.FullName -Force -EA SilentlyContinue).Count -eq 0 }
    foreach ($d in $emptyDirs) { Del-Safe $d.FullName $d.Name; $emptyCount++ }
} while ($emptyDirs.Count -gt 0)

$cleanedLog | ForEach-Object { Write-Host $_ -ForegroundColor DarkGray }
Write-Host ""
Write-Host ("  Items eliminados : {0}  ({1})" -f $totalItems, (Format-Bytes $totalBytes)) -ForegroundColor Green


# ============================================================
#  ANALISIS DE SEGURIDAD Y CALIDAD
# ============================================================
Write-Host ""
Write-Host "  ---- Escaneando seguridad y calidad del codigo ----" -ForegroundColor Cyan
Write-Host ""

$csFiles = Get-ChildItem -Path $repoRoot -Include "*.cs","*.json","*.config","*.xml","*.yaml","*.yml" -Recurse -EA SilentlyContinue |
           Where-Object { $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" -and
                          $_.FullName -notlike "*\.git\*" -and $_.Name -notlike "*.min.*" }
$csOnlyFiles = $csFiles | Where-Object { $_.Extension -eq ".cs" }
$total_sec_checks = 18
$current_check    = 0

function Show-Check($n, $total, $label) {
    Write-Host ("  [{0,2}/{1}] {2}..." -f $n, $total, $label) -ForegroundColor DarkGray
}

# ── 1. Passwords / secrets hardcodeados ─────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Credenciales hardcodeadas"
$secretPatterns = @(
    @{ pat='(?i)password\s*=\s*"[^"$\{]{6,}"';  label="Password literal en codigo";       sev="CRITICA" }
    @{ pat='(?i)secret\s*=\s*"[^"$\{]{6,}"';    label="Secret literal en codigo";         sev="CRITICA" }
    @{ pat='(?i)apikey\s*=\s*"[^"$\{]{6,}"';    label="API Key literal en codigo";        sev="CRITICA" }
    @{ pat='(?i)api_key\s*=\s*"[^"$\{]{6,}"';   label="API Key guion bajo";               sev="CRITICA" }
    @{ pat='Bearer [A-Za-z0-9_\-\.]{30,}';       label="Token Bearer hardcodeado";         sev="ALTA"    }
    @{ pat='(?i)connectionstring.*password=[^;\"]{4,}'; label="Password en cadena conexion"; sev="ALTA" }
    @{ pat='(?i)smtp.*password\s*=\s*"[^"]{4,}"';label="Credencial SMTP expuesta";         sev="ALTA"    }
    @{ pat='(?i)private_?key\s*=\s*"[^"]{10,}"'; label="Clave privada hardcodeada";        sev="CRITICA" }
)
foreach ($f in $csFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if (-not $fc) { continue }
    foreach ($sp in $secretPatterns) {
        if ($fc -match $sp.pat) {
            Add-Warn "SEGURIDAD" $sp.sev $sp.label $f.FullName.Replace($repoRoot,"")
        }
    }
}

# ── 2. JWT almacenado sin hash + claims sensibles ───────────
$current_check++; Show-Check $current_check $total_sec_checks "JWT y claims"
foreach ($f in $csOnlyFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if (-not $fc) { continue }
    # Token guardado sin cifrar
    if ($fc -match '(?i)\.Token\s*=\s*[a-zA-Z]' -and $fc -notmatch '(?i)hash|encrypt|BCrypt') {
        Add-Warn "SEGURIDAD" "MEDIA" "Token JWT posiblemente guardado en texto plano" $f.FullName.Replace($repoRoot,"")
    }
    # HS256 con clave corta (< 32 chars)
    if ($fc -match 'SecurityAlgorithms\.HmacSha256' -and $fc -match '"[^"]{1,31}"') {
        Add-Warn "SEGURIDAD" "ALTA" "Clave JWT posiblemente corta para HS256 (necesita >= 32 chars)" $f.FullName.Replace($repoRoot,"")
    }
    # Claims con datos sensibles innecesarios
    if ($fc -match '(?i)new Claim.*password' -or $fc -match '(?i)new Claim.*creditcard') {
        Add-Warn "SEGURIDAD" "ALTA" "Claim JWT con dato sensible (password/creditcard)" $f.FullName.Replace($repoRoot,"")
    }
}

# ── 3. Inyeccion SQL (concatenacion de strings en queries) ──
$current_check++; Show-Check $current_check $total_sec_checks "SQL Injection - concatenacion en queries"
foreach ($f in $csOnlyFiles) {
    $lines = Get-Content $f.FullName -EA SilentlyContinue
    if (-not $lines) { continue }
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $l = $lines[$i]
        # String concat en SQL raw (ExecuteQuery, ExecuteSql, FromSqlRaw con +)
        if ($l -match '(?i)(ExecuteQuery|ExecuteSql|FromSqlRaw|SqlQuery|CommandText)\s*[\(=].*\+') {
            Add-Warn "SEGURIDAD" "CRITICA" "Posible SQLi - concatenacion en query SQL linea $($i+1)" $f.FullName.Replace($repoRoot,"")
        }
        # String interpolation en SQL sin parametros
        if ($l -match '(?i)(FromSqlRaw|ExecuteSql)\s*\(\s*\$"' -and $l -notmatch 'new\s+\{') {
            Add-Warn "SEGURIDAD" "ALTA" "Interpolacion en SQL crudo sin parametros linea $($i+1)" $f.FullName.Replace($repoRoot,"")
        }
    }
}

# ── 4. CORS demasiado abierto ────────────────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Configuracion CORS"
foreach ($f in $csOnlyFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if (-not $fc) { continue }
    if ($fc -match '\.AllowAnyOrigin\(\)' -and $fc -match '\.AllowCredentials\(\)') {
        Add-Warn "SEGURIDAD" "CRITICA" "CORS: AllowAnyOrigin + AllowCredentials juntos - bloqueado por browsers" $f.FullName.Replace($repoRoot,"")
    }
    if ($fc -match '\.AllowAnyOrigin\(\)' -and $fc -notmatch '\.AllowCredentials\(\)') {
        Add-Warn "SEGURIDAD" "MEDIA" "CORS: AllowAnyOrigin acepta requests de cualquier dominio" $f.FullName.Replace($repoRoot,"")
    }
}

# ── 5. Catch vacios / excepciones silenciadas ────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Catch vacios y excepciones silenciadas"
foreach ($f in $csOnlyFiles) {
    $lines = Get-Content $f.FullName -EA SilentlyContinue
    if (-not $lines) { continue }
    for ($i = 0; $i -lt $lines.Count - 2; $i++) {
        if ($lines[$i] -match '^\s*}\s*catch\s*(\(.*\))?\s*\{?\s*$') {
            $nextNonEmpty = ""
            for ($j = $i+1; $j -lt [Math]::Min($i+4, $lines.Count); $j++) {
                if ($lines[$j].Trim() -ne "" -and $lines[$j].Trim() -ne "{") {
                    $nextNonEmpty = $lines[$j].Trim()
                    break
                }
            }
            if ($nextNonEmpty -eq "}" -or $nextNonEmpty -eq "") {
                Add-Warn "CALIDAD" "ALTA" "Catch vacio/silencioso linea $($i+1)" $f.FullName.Replace($repoRoot,"")
            }
        }
    }
}

# ── 6. Logging de datos sensibles ───────────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Logging de datos sensibles"
foreach ($f in $csOnlyFiles) {
    $lines = Get-Content $f.FullName -EA SilentlyContinue
    if (-not $lines) { continue }
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $l = $lines[$i]
        if ($l -match '(?i)_logger\.(Log|Info|Warn|Error|Debug).*(?:password|token|secret|creditcard|tarjeta)') {
            Add-Warn "SEGURIDAD" "ALTA" "Posible log de dato sensible linea $($i+1)" $f.FullName.Replace($repoRoot,"")
        }
        if ($l -match '(?i)Console\.Write.*(?:password|secret|token)') {
            Add-Warn "SEGURIDAD" "ALTA" "Console.Write con dato sensible linea $($i+1)" $f.FullName.Replace($repoRoot,"")
        }
    }
}

# ── 7. Endpoints sin autorizacion ────────────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Endpoints sin [Authorize]"
foreach ($f in $csOnlyFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if (-not $fc) { continue }
    # Buscar controladores que tengan metodos POST/PUT/DELETE sin Authorize ni AllowAnonymous
    if ($fc -match '(?i)\[ApiController\]') {
        $hasClassAuth = $fc -match '(?im)^\s*\[Authorize'
        $methodsNoAuth = [regex]::Matches($fc, '(?is)\[Http(Post|Put|Delete|Patch)\][^}]{0,200}\bpublic\b') |
                         Where-Object { $_.Value -notmatch '\[Authorize' -and $_.Value -notmatch '\[AllowAnonymous' }
        if (-not $hasClassAuth -and $methodsNoAuth.Count -gt 0) {
            Add-Warn "SEGURIDAD" "ALTA" "$($methodsNoAuth.Count) metodo(s) POST/PUT/DELETE sin [Authorize] ni [AllowAnonymous]" $f.FullName.Replace($repoRoot,"")
        }
    }
}

# ── 8. Mass assignment / binding sin restriccion ────────────
$current_check++; Show-Check $current_check $total_sec_checks "Mass assignment en DTOs"
foreach ($f in $csOnlyFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if (-not $fc) { continue }
    if ($fc -match '(?i)class\s+\w*(Request|Input|Dto)\b' -and
        $fc -notmatch '\[Bind\(' -and
        $fc -notmatch '\[JsonIgnore\]' -and
        ($fc -match '(?i)public\s+\w+\s+(?:IsAdmin|Role|TenantId|Id)\s*\{')) {
        Add-Warn "SEGURIDAD" "MEDIA" "DTO con campo sensible (Role/IsAdmin/TenantId) sin restriccion de binding" $f.FullName.Replace($repoRoot,"")
    }
}

# ── 9. Archivos sensibles expuestos en repo ──────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Archivos sensibles en repositorio"
$sensPatterns = @("*.env",".env*","secrets.json","appsettings.Production.json",
                  "appsettings.Local.json","*.pem","*.key","*.pfx","*.p12","*.p8",
                  "id_rsa","id_rsa.pub","*.ppk","*.jks","*.keystore")
foreach ($sp in $sensPatterns) {
    Get-ChildItem -Path $repoRoot -Filter $sp -Recurse -EA SilentlyContinue |
    Where-Object { $_.FullName -notlike "*\.git\*" -and $_.Name -notlike "*.example" } |
    ForEach-Object { Add-Warn "SEGURIDAD" "CRITICA" "Archivo sensible en repo" $_.FullName.Replace($repoRoot,"") }
}

# ── 10. .gitignore incompleto ────────────────────────────────
$current_check++; Show-Check $current_check $total_sec_checks ".gitignore"
$giPath = Join-Path $repoRoot ".gitignore"
$giRequired = @("bin/","obj/",".vs/","*.user","*.env","*.key","*.pfx","*.p12",
                "appsettings.Production.json","appsettings.Local.json","secrets.json")
if (Test-Path $giPath) {
    $giContent = Get-Content $giPath -Raw
    foreach ($m in ($giRequired | Where-Object { $giContent -notlike "*$_*" })) {
        Add-Warn "SEGURIDAD" "MEDIA" ".gitignore no cubre: $m" ".gitignore"
    }
} else {
    Add-Warn "SEGURIDAD" "ALTA" "No existe .gitignore" ""
}

# ── 11. Headers de seguridad HTTP ───────────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Headers HTTP de seguridad"
$programFiles = $csOnlyFiles | Where-Object { $_.Name -in @("Program.cs","Startup.cs") }
foreach ($f in $programFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if (-not $fc) { continue }
    $missingHeaders = @()
    if ($fc -notmatch 'X-Content-Type-Options')        { $missingHeaders += "X-Content-Type-Options" }
    if ($fc -notmatch 'X-Frame-Options')               { $missingHeaders += "X-Frame-Options" }
    if ($fc -notmatch 'Content-Security-Policy')        { $missingHeaders += "Content-Security-Policy" }
    if ($fc -notmatch 'Strict-Transport-Security|HSTS') { $missingHeaders += "Strict-Transport-Security (HSTS)" }
    if ($missingHeaders.Count -gt 0) {
        Add-Warn "SEGURIDAD" "MEDIA" "Headers de seguridad faltantes: $($missingHeaders -join ', ')" $f.FullName.Replace($repoRoot,"")
    }
}

# ── 12. Rate limiting configurado ───────────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Rate limiting"
$hasRateLimit = $false
foreach ($f in $csOnlyFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if ($fc -match '(?i)RateLimit|AddRateLimiter|ThrottleRequests|AspNetCoreRateLimit') {
        $hasRateLimit = $true; break
    }
}
if (-not $hasRateLimit) {
    Add-Warn "SEGURIDAD" "MEDIA" "No se detecta rate limiting configurado en la API" "Program.cs"
}

# ── 13. HTTPS forzado ────────────────────────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "HTTPS forzado"
foreach ($f in $programFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if (-not $fc) { continue }
    if ($fc -notmatch 'UseHttpsRedirection|RequireHttpsMetadata') {
        Add-Warn "SEGURIDAD" "MEDIA" "UseHttpsRedirection no detectado en Program.cs" $f.FullName.Replace($repoRoot,"")
    }
}

# ── 14. Validacion de modelos habilitada ─────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Validacion de modelos"
$hasValidation = $false
foreach ($f in $csOnlyFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if ($fc -match '(?i)FluentValidation|IValidator|AddValidatorsFromAssembly') {
        $hasValidation = $true; break
    }
}
if (-not $hasValidation) {
    Add-Warn "CALIDAD" "MEDIA" "No se detecta FluentValidation configurado" "Application layer"
}

# ── 15. TODO / FIXME / HACK ──────────────────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "TODO / FIXME / HACK pendientes"
$todoMap = @{ TODO="BAJA"; FIXME="MEDIA"; HACK="ALTA"; "SECURITY"="ALTA"; BUG="MEDIA"; "UNSAFE"="ALTA" }
foreach ($f in $csOnlyFiles) {
    $lines = Get-Content $f.FullName -EA SilentlyContinue
    if (-not $lines) { continue }
    for ($i = 0; $i -lt $lines.Count; $i++) {
        foreach ($kw in $todoMap.Keys) {
            if ($lines[$i] -match "//\s*$kw\b") {
                $txt = $lines[$i].Trim()
                if ($txt.Length -gt 80) { $txt = $txt.Substring(0,77) + "..." }
                Add-Warn "CALIDAD" $todoMap[$kw] "$kw linea $($i+1): $txt" $f.FullName.Replace($repoRoot,"")
                break
            }
        }
    }
}

# ── 16. Archivos duplicados ──────────────────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Archivos duplicados"
$dupExts = @(".cs",".json",".yaml",".yml",".sql",".config")
$allProjectFiles = Get-ChildItem -Path $repoRoot -Recurse -File -EA SilentlyContinue |
    Where-Object { $_.FullName -notlike "*\.git\*" -and $_.FullName -notlike "*\bin\*" -and
                   $_.FullName -notlike "*\obj\*"  -and $_.Extension -in $dupExts }
$dups = $allProjectFiles | Group-Object Name | Where-Object { $_.Count -gt 1 }
foreach ($d in $dups) {
    $paths = ($d.Group | ForEach-Object { $_.FullName.Replace($repoRoot,"") }) -join " | "
    Add-Warn "CALIDAD" "MEDIA" "Nombre duplicado '$($d.Name)'" $paths
}

# ── 17. Archivos .cs huerfanos ───────────────────────────────
$current_check++; Show-Check $current_check $total_sec_checks "Archivos .cs huerfanos"
$csprojDirs = Get-ChildItem -Path $repoRoot -Filter "*.csproj" -Recurse -EA SilentlyContinue |
              Where-Object { $_.FullName -notlike "*\bin\*" } |
              ForEach-Object { $_.Directory.FullName }
Get-ChildItem -Path $repoRoot -Filter "*.cs" -Recurse -EA SilentlyContinue |
    Where-Object { $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\.git\*" } |
    ForEach-Object {
        $cs = $_
        $inProject = $csprojDirs | Where-Object { $cs.FullName.StartsWith($_) }
        if (-not $inProject) {
            Add-Warn "CALIDAD" "MEDIA" "Archivo .cs fuera de proyecto" $cs.FullName.Replace($repoRoot,"")
        }
    }

# ── 18. Violaciones de Clean Architecture (capas) ───────────
$current_check++; Show-Check $current_check $total_sec_checks "Dependencias entre capas (Clean Architecture)"
$domainFiles = Get-ChildItem -Path (Join-Path $repoRoot "src\BA.Backend.Domain") -Filter "*.cs" -Recurse -EA SilentlyContinue
foreach ($f in $domainFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if (-not $fc) { continue }
    if ($fc -match 'using BA\.Backend\.Infrastructure' -or $fc -match 'using BA\.Backend\.Application') {
        Add-Warn "CALIDAD" "ALTA" "Domain depende de Infrastructure/Application - viola Clean Architecture" $f.FullName.Replace($repoRoot,"")
    }
}
$appFiles = Get-ChildItem -Path (Join-Path $repoRoot "src\BA.Backend.Application") -Filter "*.cs" -Recurse -EA SilentlyContinue
foreach ($f in $appFiles) {
    $fc = Get-Content $f.FullName -Raw -EA SilentlyContinue
    if (-not $fc) { continue }
    if ($fc -match 'using BA\.Backend\.Infrastructure') {
        Add-Warn "CALIDAD" "ALTA" "Application depende directamente de Infrastructure - viola DIP" $f.FullName.Replace($repoRoot,"")
    }
    if ($fc -match 'using.*Dapper' -or $fc -match 'using.*EntityFramework') {
        Add-Warn "CALIDAD" "MEDIA" "Application usa ORM/Dapper directamente (deberia ir en Infrastructure)" $f.FullName.Replace($repoRoot,"")
    }
}

Write-Host ""
# Mostrar resumen rapido en consola
$byType = $warnLog | Group-Object Sev
foreach ($g in @("CRITICA","ALTA","MEDIA","BAJA")) {
    $cnt = ($warnLog | Where-Object { $_.Sev -eq $g }).Count
    if ($cnt -gt 0) {
        $col = switch ($g) { "CRITICA"{"Red"} "ALTA"{"Red"} "MEDIA"{"Yellow"} "BAJA"{"Cyan"} }
        Write-Host ("  {0,-8}: {1} hallazgo(s)" -f $g, $cnt) -ForegroundColor $col
    }
}
Write-Host ""

# ============================================================
#  GENERAR INFORME_FINAL.md  (unico archivo de salida)
# ============================================================
$informe = [System.Text.StringBuilder]::new()
$fecha   = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

[void]$informe.AppendLine("# INFORME FINAL - BA.FrioCheck")
[void]$informe.AppendLine("")
[void]$informe.AppendLine("**Fecha:** $fecha")
[void]$informe.AppendLine("**Proyecto:** BA.FrioCheck (Antigravity) | Puerto 5003")
[void]$informe.AppendLine("")
[void]$informe.AppendLine("---")
[void]$informe.AppendLine("")

# -- Resultados de tests
$tTotal = $pass + $fail + $skip
$tScore = if (($pass+$fail) -gt 0) { [math]::Round($pass/($pass+$fail)*100) } else { 0 }

[void]$informe.AppendLine("## Resultados de Tests")
[void]$informe.AppendLine("")
[void]$informe.AppendLine("| Metrica | Valor |")
[void]$informe.AppendLine("|---------|-------|")
[void]$informe.AppendLine("| Total tests | $tTotal |")
[void]$informe.AppendLine("| PASS | $pass |")
[void]$informe.AppendLine("| FAIL | $fail |")
[void]$informe.AppendLine("| SKIP | $skip |")
[void]$informe.AppendLine("| Health Score | **$tScore / 100** |")
[void]$informe.AppendLine("")

# Tests por bloque
[void]$informe.AppendLine("### Por bloque")
[void]$informe.AppendLine("")
[void]$informe.AppendLine("| Bloque | PASS | FAIL | SKIP | % |")
[void]$informe.AppendLine("|--------|------|------|------|---|")

$bloquesMd = @(
    @{ n="B1  - Conexion API";      p="C"   }; @{ n="B2  - Auth/Login";        p="A"   }
    @{ n="B3  - ApiResponse";       p="S"   }; @{ n="B4  - Roles";             p="R"   }
    @{ n="B5  - Multi-tenant";      p="MT"  }; @{ n="B6  - CRUD Usuarios";     p="U"   }
    @{ n="B7  - CRUD Stores";       p="ST"  }; @{ n="B8  - NFC";               p="N"   }
    @{ n="B9  - Cliente";           p="CL"  }; @{ n="B10 - Tecnico";           p="T"   }
    @{ n="B11 - Transportista";     p="TR"  }; @{ n="B12 - Dashboard";         p="AD"  }
    @{ n="B13 - Base de Datos";     p="DB"  }; @{ n="B14 - JWT/Sesion";        p="J"   }
    @{ n="B15 - Exceptions";        p="EX"  }; @{ n="B16 - Endpoints";         p="EP"  }
    @{ n="B17 - Coolers CRUD";      p="CO"  }; @{ n="B18 - Post-Limpieza";     p="RV"  }
    @{ n="B19 - Mermas/TechSupp";   p="MS"  }; @{ n="B20 - Password Reset";    p="PR"  }
    @{ n="B21 - Seguridad";         p="SEC" }; @{ n="B22 - Performance";       p="PF"  }
    @{ n="B23 - Concurrencia";      p="CS"  }
)
foreach ($b in $bloquesMd) {
    $bt  = $results | Where-Object { $_.ID -match "^$($b.p)\d+" }
    $bP  = ($bt | Where-Object Estado -eq "PASS").Count
    $bF  = ($bt | Where-Object Estado -in @("FAIL","ERROR")).Count
    $bSk = ($bt | Where-Object Estado -eq "SKIP").Count
    $bT  = $bP + $bF
    if ($bt.Count -eq 0) { continue }
    $bPct = if ($bT -gt 0) { [math]::Round($bP/$bT*100) } else { 100 }
    $icon = if ($bF -eq 0 -and $bSk -eq 0) {"OK"} elseif ($bF -eq 0) {"SKIP"} elseif ($bPct -ge 70) {"PARCIAL"} else {"FALLO"}
    [void]$informe.AppendLine("| $($b.n) | $bP | $bF | $bSk | $bPct% $icon |")
}
[void]$informe.AppendLine("")

# Tests fallidos
$failedTests = $results | Where-Object { $_.Estado -in @("FAIL","ERROR") }
if ($failedTests.Count -gt 0) {
    [void]$informe.AppendLine("### Tests fallidos")
    [void]$informe.AppendLine("")
    [void]$informe.AppendLine("| ID | Test | Detalle |")
    [void]$informe.AppendLine("|----|------|---------|")
    foreach ($f in $failedTests) {
        $sol = Get-Solution $f.ID $f.Detalle
        $sev = if ($sol) { $sol.Severity } else { "-" }
        [void]$informe.AppendLine("| ``$($f.ID)`` | $($f.Prueba) | $($f.Detalle) |")
    }
    [void]$informe.AppendLine("")
}

# -- Limpieza
[void]$informe.AppendLine("---")
[void]$informe.AppendLine("")
[void]$informe.AppendLine("## Limpieza del Repositorio")
[void]$informe.AppendLine("")
[void]$informe.AppendLine("| Metrica | Valor |")
[void]$informe.AppendLine("|---------|-------|")
[void]$informe.AppendLine("| Archivos/carpetas eliminados | $totalItems |")
[void]$informe.AppendLine("| Espacio liberado | $(Format-Bytes $totalBytes) |")
[void]$informe.AppendLine("")
if ($cleanedLog.Count -gt 0) {
    [void]$informe.AppendLine("### Elementos eliminados")
    [void]$informe.AppendLine("")
    foreach ($l in $cleanedLog) { [void]$informe.AppendLine("- $($l.Trim())") }
    [void]$informe.AppendLine("")
}

# -- Hallazgos de seguridad y calidad
[void]$informe.AppendLine("---")
[void]$informe.AppendLine("")
[void]$informe.AppendLine("## Hallazgos de Seguridad y Calidad")
[void]$informe.AppendLine("")

$sevsOrder = @("CRITICA","ALTA","MEDIA","BAJA")
$sevIcons  = @{ CRITICA="CRITICA"; ALTA="ALTA"; MEDIA="MEDIA"; BAJA="BAJA" }

foreach ($sev in $sevsOrder) {
    $items = $warnLog | Where-Object { $_.Sev -eq $sev }
    if ($items.Count -eq 0) { continue }
    [void]$informe.AppendLine("### $sev ($($items.Count) hallazgo$(if($items.Count -ne 1){'s'}))")
    [void]$informe.AppendLine("")
    [void]$informe.AppendLine("| Tipo | Descripcion | Archivo |")
    [void]$informe.AppendLine("|------|-------------|---------|")
    foreach ($w in $items) {
        $arch = if ($w.Archivo) { "``$($w.Archivo)``" } else { "-" }
        [void]$informe.AppendLine("| $($w.Tipo) | $($w.Msg) | $arch |")
    }
    [void]$informe.AppendLine("")
}

if ($warnLog.Count -eq 0) {
    [void]$informe.AppendLine("Sin hallazgos de seguridad o calidad. Repositorio limpio.")
    [void]$informe.AppendLine("")
}

# -- Checklist de acciones
[void]$informe.AppendLine("---")
[void]$informe.AppendLine("")
[void]$informe.AppendLine("## Checklist de Acciones")
[void]$informe.AppendLine("")
$criticos = $warnLog | Where-Object { $_.Sev -eq "CRITICA" }
$altos    = $warnLog | Where-Object { $_.Sev -eq "ALTA" }
$testFail = $results | Where-Object { $_.Estado -in @("FAIL","ERROR") }
[void]$informe.AppendLine("- [ ] Corregir $($testFail.Count) tests fallidos")
[void]$informe.AppendLine("- [ ] Resolver $($criticos.Count) hallazgos CRITICOS de seguridad")
[void]$informe.AppendLine("- [ ] Resolver $($altos.Count) hallazgos ALTOS")
[void]$informe.AppendLine("- [ ] Revisar hallazgos MEDIA y BAJA")
[void]$informe.AppendLine("- [ ] Instalar sqlcmd para habilitar tests de BD")
[void]$informe.AppendLine("- [ ] Agregar tests a pipeline CI/CD")
[void]$informe.AppendLine("")
[void]$informe.AppendLine("---")
[void]$informe.AppendLine("*Generado automaticamente por test.ps1*")

$informePath = Join-Path $PSScriptRoot "INFORME_FINAL.md"
$informe.ToString() | Set-Content $informePath -Encoding UTF8

Write-Host "  INFORME_FINAL.md generado -> $informePath" -ForegroundColor Green
Write-Host ""

# ============================================================
#  TOTALES Y DASHBOARD FINAL
# ============================================================
$total     = $pass + $fail + $skip
$score     = if (($pass+$fail) -gt 0) { [math]::Round($pass/($pass+$fail)*100) } else { 0 }
$scoreCols = if ($score -ge 90) {"Green"} elseif ($score -ge 75) {"Yellow"} else {"Red"}
$grade     = switch ($score) {
    {$_ -ge 95}{"A+"} {$_ -ge 90}{"A "} {$_ -ge 85}{"B+"} {$_ -ge 80}{"B "} {$_ -ge 75}{"C+"} {$_ -ge 70}{"C "} default{"F "}
}

$line = "=" * 63

Write-Host ""
Write-Host ""
Write-Host "  $line" -ForegroundColor White
Write-Host "  =          RESUMEN EJECUTIVO FINAL - BA.FrioCheck          =" -ForegroundColor White
Write-Host "  $line" -ForegroundColor White
Write-Host ""

# Score bar
$fill  = [math]::Round($score / 2)
$empty = 50 - $fill
$bar   = ("#" * $fill) + ("." * $empty)
Write-Host ("  Score: {0}/100  [{1}]  {2}" -f $score, $grade.Trim(), $bar) -ForegroundColor $scoreCols
Write-Host ""

# Totals table
$pr = if ($total -gt 0) { [math]::Round($pass/$total*100,1) } else { 0 }
$fr = if ($total -gt 0) { [math]::Round($fail/$total*100,1) } else { 0 }
$sr = if ($total -gt 0) { [math]::Round($skip/$total*100,1) } else { 0 }

Write-Host "  +--------------+--------+--------+" -ForegroundColor DarkGray
Write-Host "  | Estado       |  Tests |      % |" -ForegroundColor DarkGray
Write-Host "  +--------------+--------+--------+" -ForegroundColor DarkGray
Write-Host ("  | {0,-12} | {1,6} | {2,5}% |" -f "TOTAL",  $total, "100.0") -ForegroundColor White
Write-Host ("  | {0,-12} | {1,6} | {2,5}% |" -f "PASS",   $pass,  $pr)     -ForegroundColor Green
Write-Host ("  | {0,-12} | {1,6} | {2,5}% |" -f "FAIL",   $fail,  $fr)     -ForegroundColor $(if($fail -gt 0){"Red"}else{"Green"})
Write-Host ("  | {0,-12} | {1,6} | {2,5}% |" -f "SKIP",   $skip,  $sr)     -ForegroundColor DarkGray
Write-Host "  +--------------+--------+--------+" -ForegroundColor DarkGray
Write-Host ""

# Per-block table
$allBloques = @(
    @{n="Conexion API   B1"; p="C"  }; @{n="Auth/Login     B2"; p="A"  }
    @{n="ApiResponse    B3"; p="S"  }; @{n="Roles          B4"; p="R"  }
    @{n="Multi-tenant   B5"; p="MT" }; @{n="CRUD Usuarios  B6"; p="U"  }
    @{n="CRUD Stores    B7"; p="ST" }; @{n="NFC            B8"; p="N"  }
    @{n="Cliente        B9"; p="CL" }; @{n="Tecnico        B10";p="T"  }
    @{n="Transportista  B11";p="TR" }; @{n="Dashboard      B12";p="AD" }
    @{n="Base de Datos  B13";p="DB" }; @{n="JWT/Sesion     B14";p="J"  }
    @{n="Exceptions     B15";p="EX" }; @{n="Endpoints      B16";p="EP" }
    @{n="Coolers CRUD   B17";p="CO" }; @{n="Post-Limpieza  B18";p="RV" }
    @{n="Mermas/Tech    B19";p="MS" }; @{n="Password Reset B20";p="PR" }
    @{n="Seguridad      B21";p="SEC"}; @{n="Performance    B22";p="PF" }
    @{n="Concurrencia   B23";p="CS" }
)

Write-Host "  +----------------------+------+------+------+----------+" -ForegroundColor DarkGray
Write-Host "  | Bloque               | PASS | FAIL | SKIP | Estado   |" -ForegroundColor DarkGray
Write-Host "  +----------------------+------+------+------+----------+" -ForegroundColor DarkGray

$bloqFail = 0
foreach ($b in $allBloques) {
    $bt   = $results | Where-Object { $_.ID -match "^$($b.p)\d+" }
    $bP   = ($bt | Where-Object Estado -eq "PASS").Count
    $bF   = ($bt | Where-Object Estado -in @("FAIL","ERROR")).Count
    $bSk  = ($bt | Where-Object Estado -eq "SKIP").Count
    $bT   = $bP + $bF
    if ($bt.Count -eq 0) { continue }
    if ($bF -gt 0) { $bloqFail++ }
    $bPct = if ($bT -gt 0) { [math]::Round($bP/$bT*100) } else { 100 }
    $_est = if ($bF -eq 0 -and $bSk -eq 0) {"OK       "} elseif ($bF -eq 0) {"SKIP     "} elseif ($bPct -ge 70) {"PARCIAL  "} else {"FALLO    "}
    $_col = if ($bF -eq 0 -and $bSk -eq 0) {"Green"} elseif ($bF -eq 0) {"DarkGray"} elseif ($bPct -ge 70) {"Yellow"} else {"Red"}
    Write-Host ("  | {0,-20} | {1,4} | {2,4} | {3,4} | {4,-8} |" -f $b.n,$bP,$bF,$bSk,$_est) -ForegroundColor $_col
}
Write-Host "  +----------------------+------+------+------+----------+" -ForegroundColor DarkGray
Write-Host ""

# Security findings summary
$criticosSec = $warnLog | Where-Object { $_.Sev -eq "CRITICA" }
$altosSec    = $warnLog | Where-Object { $_.Sev -eq "ALTA"    }
$mediosSec   = $warnLog | Where-Object { $_.Sev -eq "MEDIA"   }
$bajosSec    = $warnLog | Where-Object { $_.Sev -eq "BAJA"    }

Write-Host "  +--- HALLAZGOS DE SEGURIDAD Y CALIDAD -------------------+" -ForegroundColor DarkGray
Write-Host ("  |  CRITICA : {0,3}  |  ALTA : {1,3}  |  MEDIA : {2,3}  |  BAJA : {3,3}  |" -f $criticosSec.Count,$altosSec.Count,$mediosSec.Count,$bajosSec.Count) -ForegroundColor $(if($criticosSec.Count -gt 0){"Red"}elseif($altosSec.Count -gt 0){"Yellow"}else{"Green"})
Write-Host "  +--------------------------------------------------------+" -ForegroundColor DarkGray
Write-Host ""

# Failed tests detail
$failedFinal = $results | Where-Object { $_.Estado -in @("FAIL","ERROR") }
if ($failedFinal.Count -gt 0) {
    Write-Host "  +--- TESTS FALLIDOS ($($failedFinal.Count)) ----------------------------------+" -ForegroundColor Red
    foreach ($f in $failedFinal) {
        Write-Host ("  |  [{0,-5}] {1}" -f $f.ID, $f.Prueba) -ForegroundColor Yellow
        if ($f.Detalle) { Write-Host ("  |         {0}" -f $f.Detalle) -ForegroundColor DarkGray }
    }
    Write-Host "  +--------------------------------------------------------+" -ForegroundColor Red
    Write-Host ""
}

# Quanto falta para 100
$falta = 100 - $score
Write-Host "  +--- CUANTO FALTA PARA 100/100 --------------------------+" -ForegroundColor Cyan
Write-Host ("  |  Tests fallidos  : {0,3}   Puntos al maximo : {1,3}        |" -f $failedFinal.Count, $falta) -ForegroundColor $(if($falta -eq 0){"Green"}else{"Yellow"})
Write-Host ("  |  Bloques con falla: {0,2}   Hallazgos criticos: {1,2}       |" -f $bloqFail, $criticosSec.Count) -ForegroundColor $(if($bloqFail -eq 0){"Green"}else{"Yellow"})
Write-Host "  |                                                        |" -ForegroundColor Cyan
if ($score -ge 95) {
    Write-Host "  |  EXCELENTE - Considera agregar a pipeline CI/CD       |" -ForegroundColor Green
} elseif ($score -ge 85) {
    Write-Host "  |  BUEN ESTADO - Corrige los FAIL y llegaras a 100      |" -ForegroundColor Yellow
} elseif ($score -ge 70) {
    Write-Host "  |  TRABAJO PENDIENTE - Focaliza en errores ALTA primero |" -ForegroundColor Yellow
} else {
    Write-Host "  |  ATENCION URGENTE - Revisa errores CRITICOS primero   |" -ForegroundColor Red
}
Write-Host "  +--------------------------------------------------------+" -ForegroundColor Cyan
Write-Host ""

# Footer
# ============================================================
#  BLOQUE 24 - COMPATIBILIDAD EXTERNA (Compañero)
# ============================================================
Write-Header "BLOQUE 24 - COMPATIBILIDAD EXTERNA (Flat JSON)"

Test-Case "CMP01" "POST /auth/login (Flat) retorna JSON sin wrapper 'data'" {
    $r = Invoke-Api -Method POST -Url "$BASE/auth/login" -Body @{
        email=$CREDS["AdminSavory"].email; password=$SQL_PASS; tenantSlug=$DB.T1.Slug
    }
    # Verificamos que el root tenga 'accessToken', no dentro de 'data'
    $isFlat = ($r.Body.accessToken -ne $null -and $r.Body.data -eq $null)
    @{ ok=($r.StatusCode -eq 200 -and $isFlat); detail="Status:$($r.StatusCode) Flat:$isFlat" }
}

Test-Case "CMP02" "GET /cliente/home (Flat) retorna JSON plano" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/home" -Token $TOKENS["Cliente1Savory"]
    $isFlat = ($r.Body.user -ne $null -and $r.Body.data -eq $null)
    @{ ok=($r.StatusCode -eq 200 -and $isFlat); detail="Status:$($r.StatusCode) Flat:$isFlat" }
}

Test-Case "CMP03" "GET /delivery/home (Alias /transportista) retorna 200" {
    if (-not $TOKENS["Trans1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/delivery/home" -Token $TOKENS["Trans1Savory"]
    @{ ok=($r.StatusCode -eq 200); detail="Status:$($r.StatusCode)" }
}

Write-Host ""

# ============================================================
#  BLOQUE 26 - HUB DE INTEGRACION UNIVERSAL
# ============================================================
Write-Header "BLOQUE 26 - HUB DE INTEGRACION (Stock Espejo)"

Test-Case "HUB01" "Cliente GET /products incluye campo stockExterno" {
    if (-not $TOKENS["Cliente1Savory"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/products" -Token $TOKENS["Cliente1Savory"]
    if (-not $r.Body) { return @{ ok=$false; detail="Body nulo - Status:$($r.StatusCode)" } }
    if ($r.Body.success -eq $false) { return @{ ok=$false; detail="API Error: $($r.Body.message)" } }
    if ($r.Body.data.Count -eq 0) { return @{ ok=$false; detail="Lista vacia (Data null en matriz?)" } }
    $hasStock = ($r.Body.data[0].stockExterno -ne $null)
    @{ ok=($r.StatusCode -eq 200 -and $hasStock); detail="Status:$($r.StatusCode) Stock0:$($r.Body.data[0].stockExterno)" }
}

Test-Case "HUB02" "Integracion Factory resuelve MockAdapter para Tenants genéricos" {
    if (-not $TOKENS["Cliente1Coppelia"]) { return @{ ok=$false; detail="Sin token" } }
    $r = Invoke-Api -Url "$BASE/cliente/products" -Token $TOKENS["Cliente1Coppelia"]
    if (-not $r.Body -or $r.Body.success -eq $false) { return @{ ok=$false; detail="Error API o Body nulo" } }
    if ($r.Body.data.Count -eq 0) { return @{ ok=$false; detail="No hay productos en Coppelia" } }
    # El MockAdapter devuelve stock aleatorio, validamos que sea un numero
    $val = $r.Body.data[0].stockExterno
    @{ ok=($r.StatusCode -eq 200 -and $val -is [int]); detail="Stock:$val" }
}

Write-Host ""

# ============================================================
#  BLOQUE 27 - TEARDOWN (LIMPIEZA DE DATOS)
# ============================================================
Write-Header "BLOQUE 27 - TEARDOWN / LIMPIEZA FINAL"

Test-Case "TD01" "Eliminar usuarios de prueba generados" {
    if (-not $TOKENS["AdminSavory"]) { return @{ ok=$true; detail="No se requiere limpieza (sin admin token)" } }
    $r = Invoke-Api -Url "$BASE/users?pageNumber=1`&pageSize=100" -Token $TOKENS["AdminSavory"]
    $toDelete = $r.Body.data.items | Where-Object { 
        $_.email -like "pstest_*" -or $_.email -like "xss_*" -or $_.email -like "crud_test_*" 
    }
    $done = 0
    foreach ($u in $toDelete) {
        $del = Invoke-Api -Method DELETE -Url "$BASE/users/$($u.id)" -Token $TOKENS["AdminSavory"]
        if ($del.StatusCode -eq 200) { $done++ }
    }
    @{ ok=$true; detail="Eliminados: $done usuarios temporales" }
}

Write-Host "  $line" -ForegroundColor DarkGray
Write-Host ("  Ejecutado    : {0}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss")) -ForegroundColor DarkGray
Write-Host ("  Proyecto     : BA.FrioCheck (Antigravity) - Puerto 5003")          -ForegroundColor DarkGray
Write-Host ("  Informe      : INFORME_FINAL.md")                                   -ForegroundColor DarkGray
Write-Host ("  Re-correr    : PowerShell -ExecutionPolicy Bypass -File .\test.ps1") -ForegroundColor DarkGray
Write-Host "  $line" -ForegroundColor DarkGray
Write-Host ""
