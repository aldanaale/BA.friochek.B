#Requires -Version 5.1
<#
.SYNOPSIS
    Clean-Project.ps1 - Auditoría y limpieza segura para BA-FrioCheck
#>

param(
    [string]$RootPath   = (Get-Location).Path,
    [string]$OutputDir  = "",
    [bool]  $DryRun     = $true,
    [bool]  $SkipAngular = $false
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"

# ─────────────────────────────────────────────────────────────────────────────
# HELPERS
# ─────────────────────────────────────────────────────────────────────────────
$script:Findings  = [System.Collections.ArrayList]::new()
$script:Actions   = [System.Collections.ArrayList]::new()
$script:StartTime = Get-Date
$Timestamp        = $script:StartTime.ToString("yyyyMMdd_HHmmss")

if ($OutputDir -eq "") { $OutputDir = Join-Path $RootPath "_audit_$Timestamp" }
if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null }

$LogFile    = Join-Path $OutputDir "audit.log"

function Write-Log {
    param([string]$Msg, [string]$Level = "INFO")
    $line = "[$Level] $(Get-Date -Format 'HH:mm:ss') | $Msg"
    Add-Content -Path $LogFile -Value $line
    Write-Host $line
}

function Add-Finding {
    param(
        [string]$Category,
        [string]$Severity,
        [string]$File,
        [string]$Detail,
        [string]$Fix = ""
    )
    $null = $script:Findings.Add([PSCustomObject]@{
        Category = $Category
        Severity = $Severity
        File     = $File.Replace($RootPath, ".")
        Detail   = $Detail
        Fix      = $Fix
    })
}

function Confirm-Action {
    param([string]$Prompt)
    if ($DryRun) { return $false }
    $r = Read-Host "$Prompt [s/N]"
    return $r -match "^[sS]$"
}

# ─────────────────────────────────────────────────────────────────────────────
# BANNER
# ─────────────────────────────────────────────────────────────────────────────
Write-Host "--- BA-FrioCheck Clean-Project Audit ---" -ForegroundColor Cyan
Write-Host "Raiz: $RootPath"
Write-Host "DryRun: $DryRun"
Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
# FASE 1 – DISCOVERY
# ─────────────────────────────────────────────────────────────────────────────
Write-Log "FASE 1: DISCOVERY"

$AllFiles = Get-ChildItem -Path $RootPath -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '\\(\.git|node_modules|\.vs|bin|obj|dist|\.angular)\\' }

$CsFiles    = $AllFiles | Where-Object { $_.Extension -eq ".cs" }
$JsonFiles  = $AllFiles | Where-Object { $_.Extension -eq ".json" }
$SqlFiles   = $AllFiles | Where-Object { $_.Extension -in ".sql",".sqlproj" }

Write-Log "Archivos indexados: $($AllFiles.Count)"

# [A] ARCHIVOS OBSOLETOS
Write-Log "Buscando archivos obsoletos..."
$ObsoletePatterns = @("*.bak","*.old","*_backup*","*.tmp")
foreach ($pat in $ObsoletePatterns) {
    Get-ChildItem -Path $RootPath -Recurse -Filter $pat -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\(\.git|node_modules|bin|obj)\\' } |
    ForEach-Object {
        Add-Finding "A_OBSOLETO" "MEDIO" $_.FullName "Archivo de backup: $($_.Name)" "Eliminar"
    }
}

# [B] CONNECTION STRINGS
Write-Log "Validando connection strings..."
$AppSettings = $JsonFiles | Where-Object { $_.Name -match "appsettings" }
foreach ($f in $AppSettings) {
    $content = Get-Content $f.FullName -Raw -ErrorAction SilentlyContinue
    if ($content -match 'Password\s*=\s*[^;{}\s"]{3,}') {
        Add-Finding "B_CONNSTR" "CRITICO" $f.FullName "Password hardcodeada" "Mover a secretos"
    }
}

# [D] CÓDIGO MUERTO
Write-Log "Buscando código muerto..."
foreach ($f in $CsFiles) {
    $content = Get-Content $f.FullName -Raw -ErrorAction SilentlyContinue
    if ($content -match 'catch\s*(\([^)]*\))?\s*\{\s*\}') {
        Add-Finding "D_MUERTO" "ALTO" $f.FullName "catch vacío" "Agregar logs"
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# FASE 2 – REPORTE CONSOLA
# ─────────────────────────────────────────────────────────────────────────────
Write-Log "FASE 2: REPORTE"
$Total = $script:Findings.Count
Write-Host "Total de hallazgos: $Total" -ForegroundColor Yellow

foreach ($f in $script:Findings) {
    Write-Host "[$($f.Severity)] $($f.Category): $($f.File) - $($f.Detail)"
}

# ─────────────────────────────────────────────────────────────────────────────
# FASE 3 – LIMPIEZA
# ─────────────────────────────────────────────────────────────────────────────
if (-not $DryRun) {
    Write-Log "FASE 3: LIMPIEZA"
    # Aquí irían las acciones de eliminación si el usuario lo desea
}

Write-Log "Fin de ejecución"
