# Portal Soporte

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
| `soporte_db`    | Identity, tipos de conexión y registros  |
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

En el registro de Azure AD, agregue la URI de redirección: `http://localhost:5271/signin-oidc`.

### 3. Ejecutar la aplicación

```bash
dotnet restore Soporte.sln
dotnet run --project src/Soporte.Web
```

La aplicación queda en `http://localhost:5271`. Las migraciones de EF Core se aplican automáticamente al arrancar.

## Estructura funcional

- **Inicio** — Panel de acceso al portal
- **Registrar cliente** — Lee clientes activos de `teamsupport_db` y registra una conexión en `soporte_db`
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
  "DefaultConnection": "Host=localhost;Port=5432;Database=soporte_db;Username=postgres;Password=postgres",
  "TeamSupportConnection": "Host=localhost;Port=5432;Database=teamsupport_db;Username=postgres;Password=postgres"
}
```

## API para extensión TeamSupport (solo lectura)

Expone `client_connections` en JSON compatible con la extensión de navegador. La búsqueda es **exacta** (sin normalización): compara el texto recibido con `TeamSupportClientName` o `TeamSupportClientRazonSocial`.

Configure el token en `ExtensionApi:Token` (`appsettings.json` o User Secrets).

### Consulta individual

```http
GET /api/client-connections/lookup?q=ABAD IMPORT EXPORT SL&token=SU_TOKEN
```

Respuesta:

```json
{
  "ok": true,
  "match": "ABAD IMPORT EXPORT SL",
  "Conexion": "GOToAssist",
  "Observaciones": "",
  "Contacto": "Soporte N2",
  "Telefono": "+34 600 000 000",
  "Email": "soporte@empresa.com"
}
```

### Consulta en lote

```http
POST /api/client-connections/lookup/batch?token=SU_TOKEN
Content-Type: application/json

{ "customers": ["ABAD IMPORT EXPORT SL", "OTRO CLIENTE"] }
```

Si hay varias conexiones para el mismo cliente, `Conexion` y `Observaciones` se concatenan; `Contacto`, `Telefono` y `Email` provienen del registro más reciente.

CORS habilitado para `*.teamsupport.com`.

## API urgencia de tickets (extensión ts-severity_ext)

Tabla `ticket_urgencies` en `soporte_db`: número de ticket + indicador urgente.

Autenticación: query `token` o cabecera `X-Api-Key` (mismo valor que `ExtensionApi:Token`).

### Consultar urgencia

```http
GET /api/ticket-urgency/141499?token=SU_TOKEN
```

Respuesta:

```json
{
  "ok": true,
  "ticketNumber": "141499",
  "isUrgent": true,
  "updatedAt": "2026-06-19T13:10:00Z"
}
```

### Marcar / desmarcar urgente

```http
PUT /api/ticket-urgency?token=SU_TOKEN
Content-Type: application/json

{ "ticketNumber": "141499", "isUrgent": true }
```

## Comandos útiles

```bash
# Crear nueva migración
dotnet ef migrations add <Nombre> --project src/Soporte.Web

# Aplicar migraciones manualmente
dotnet ef database update --project src/Soporte.Web
```
