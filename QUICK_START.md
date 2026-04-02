# Arrancá el proyecto en 2 minutos

Seguí estos pasos y ya tenés el backend corriendo para probar.

---

## 1. Instalación rápida
Necesitás el SDK de .NET 10. Si no lo tenés, bajalo de la página de Microsoft. También necesitás un SQL Server instalado localmente.

## 2. Cloná y restaurá
```bash
git clone https://github.com/aldanaale/backend_FCHEK.git
cd backend_FCHEK
dotnet restore
```

## 3. Configurá tu base de datos
Andá a `src/BA.Backend.WebAPI/appsettings.json` y fijate en la parte de `DefaultConnection`. Cambiá el nombre del servidor por el tuyo (ej: `localhost` o `TU_MAQUINA\SQLEXPRESS`).

```json
"DefaultConnection": "Server=TU_SERVIDOR;Database=BA_Backend_DB;Trusted_Connection=True;TrustServerCertificate=True;"
```

## 4. Dale play
Corré este comando en la terminal:
```bash
dotnet run --project src/BA.Backend.WebAPI
```

## 5. Probá todo en Swagger
Entrá a `http://localhost:5000/swagger`.
Logueate con estas credenciales de prueba:
- **Tenant:** `admin`
- **Email:** `admin@test.com`
- **Password:** `Admin123!`

---

### Comandos que te van a servir
- **Compilar todo:** `dotnet build`
- **Limpiar temporales:** `dotnet clean`
- **Actualizar tablas:** Se hace solo al arrancar, pero si necesitás forzarlo podés usar `dotnet ef database update`.

Última actualización: 23 de Marzo, 2026.
