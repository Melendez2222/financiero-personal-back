# Mis Cuentas — Backend (ASP.NET Core, Clean Architecture)

API REST en **.NET 10** que implementa el contrato de `financiero-personal-web/api-contract.md`.
Auth JWT (usuarios sin roles), **EF Core 10 + PostgreSQL**. Arquitectura en 4 capas.

## Estructura
```
src/
  FinancieroPersonal.Domain          # entidades + enums (sin dependencias)
  FinancieroPersonal.Application      # DTOs, IAppDbContext, servicios (lógica resumen/dashboard)
  FinancieroPersonal.Infrastructure   # EF Core (AppDbContext), Npgsql, JWT, BCrypt, DbSeeder
  FinancieroPersonal.Api              # controllers, Program.cs, JWT, CORS, middleware de errores
docker-compose.yml                    # postgres:16
```
Dependencias: Api → Application + Infrastructure · Infrastructure → Application → Domain.

## Requisitos
- .NET SDK 10
- PostgreSQL (Docker recomendado)

## Base de datos (Docker)
```bash
docker compose up -d            # postgres:16 en localhost:5432 (admin / admin123 / mi_basedatos)
```
> Si tienes un PostgreSQL nativo en Windows ocupando el 5432, deshabilita su servicio
> (`Stop-Service postgresql-x64-NN; Set-Service postgresql-x64-NN -StartupType Disabled` como admin)
> o publica el contenedor en otro puerto (`-p 5433:5432`) y ajusta `ConnectionStrings:Default`.

La cadena de conexión vive en `src/FinancieroPersonal.Api/appsettings.json`:
```
Host=localhost;Port=5432;Database=mi_basedatos;Username=admin;Password=admin123
```

## Correr
```bash
dotnet run --project src/FinancieroPersonal.Api    # http://localhost:5080
```
Al arrancar aplica migraciones (`Database.Migrate()`) y **siembra** la data demo si la BD está vacía
(usuarios Ana/Luis · clave `123456`, categorías, 6 meses, movimientos, metas).

### Migraciones EF (manual)
```bash
dotnet ef migrations add <Nombre> --project src/FinancieroPersonal.Infrastructure --startup-project src/FinancieroPersonal.Api
dotnet ef database update            --project src/FinancieroPersonal.Infrastructure --startup-project src/FinancieroPersonal.Api
```

## Endpoints
Todos bajo `/api` (ver `financiero-personal-web/api-contract.md`):
`auth` (login/register/me) · `categorias` · `periodos` (+`/resumen`, `/iniciar`) ·
`movimientos` · `metas` · `configuracion` · `dashboard`. Todo salvo login/register requiere JWT.

## Conexión con el frontend
El frontend (`financiero-personal-web`) consume este API con `VITE_USE_MOCKS=false` y
`VITE_API_URL=http://localhost:5080/api`. CORS permite `http://localhost:5173` y `5174`
(configurable en `appsettings.json` → `Cors:Origins`).

## Producción
- Mover `Jwt:Key` y la cadena de conexión a variables de entorno / user-secrets.
- Apuntar la cadena a Neon (mismo proveedor Npgsql, mismas migraciones).
