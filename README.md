# Portal Conexiones

Portal web corporativo para registrar conexiones de clientes leídos desde TeamSupport, con autenticación Microsoft (Valsoft).

## Stack tecnológico

- ASP.NET Core MVC (.NET 10 LTS)
- PostgreSQL + Entity Framework Core
- ASP.NET Identity
- Bootstrap 5 + Razor
- Microsoft Entra ID (cuenta corporativa Valsoft)
- Migraciones automáticas al iniciar la aplicación

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (opcional, para PostgreSQL local)

## Inicio rápido

### 1. Base de datos

```bash
docker compose up -d
```

Esto crea dos bases de datos en PostgreSQL:

| Base de datos      | Uso                                      |
|--------------------|------------------------------------------|
| `conexiones_db`    | Identity, tipos de conexión y registros  |
| `teamsupport_db`   | Lectura de clientes (solo lectura)       |

### 2. Configurar Microsoft Entra ID

Registre la aplicación en el tenant de Valsoft y configure en `appsettings.Development.json` o User Secrets:

```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "Domain": "valsoftcorp.com",
  "TenantId": "<TENANT_ID_VALSOFT>",
  "ClientId": "<CLIENT_ID>",
  "CallbackPath": "/signin-oidc"
}
```

En el registro de Azure AD, agregue la URI de redirección: `https://localhost:7xxx/signin-oidc` (ver puerto en `launchSettings.json`).

### 3. Ejecutar la aplicación

```bash
dotnet restore Conexiones.sln
dotnet run --project src/Conexiones.Web
```

Las migraciones de EF Core se aplican automáticamente al arrancar.

## Estructura funcional

- **Inicio** — Panel de acceso al portal
- **Registrar cliente** — Lee clientes activos de `teamsupport_db` y registra una conexión en `conexiones_db`
- **Conexiones** — Listado de registros realizados
- **Tipos de conexión** — CRUD completo sobre la tabla `connection_types`

## Mapeo TeamSupport

La entidad `TeamSupportClient` mapea la tabla **`public.customers`** de `teamsupport_db` (mismo esquema usado por Telematel/MoneyPenny):

| Columna TeamSupport | Propiedad C#      | Descripción              |
|---------------------|-------------------|--------------------------|
| `CustomerId`        | `CustomerId`      | Clave primaria (varchar) |
| `CompanyName`       | `CompanyName`     | Nombre comercial         |
| `ErpId`             | `ErpId`           | Identificador ERP        |
| `RazonSocial`       | `RazonSocial`     | Razón social             |
| `TipoCliente`       | `TipoCliente`     | Tipo de cliente          |
| `ValsoftCompany`    | `ValsoftCompany`  | Compañía Valsoft         |

También existe la tabla `customer_tickets` (`CustomerId`, `TicketNumber`) para relación ticket-cliente.

En producción, apunte `TeamSupportConnection` al servidor Azure PostgreSQL de Telematel (`tmtpostgresql.postgres.database.azure.com`).

## Cadenas de conexión

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=conexiones_db;Username=postgres;Password=postgres",
  "TeamSupportConnection": "Host=localhost;Port=5432;Database=teamsupport_db;Username=postgres;Password=postgres"
}
```

## Comandos útiles

```bash
# Crear nueva migración
dotnet ef migrations add <Nombre> --project src/Conexiones.Web

# Aplicar migraciones manualmente
dotnet ef database update --project src/Conexiones.Web
```
