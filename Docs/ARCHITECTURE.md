# Arquitectura

## Visión General

Devault sigue una arquitectura **Clean Architecture** simplificada con separación en capas:

```
Controllers → Services → Data (EF Core) → PostgreSQL
    ↕              ↕
  DTOs         Interfaces
```

El patrón principal es **Dependency Injection** con contratos (interfaces) en `Interfaces/` e implementaciones en `Services/`.

---

## Capas

### Controllers

Reciben solicitudes HTTP, validan input con DTOs, delegan la lógica a los servicios y retornan respuestas HTTP.

- `AuthController` - Registro y login de usuarios
- `SecretsController` - CRUD de secretos cifrados
- `WeatherForecastController` - Endpoint de prueba (template)

### Services

Contienen la lógica de negocio. Cada servicio implementa una interfaz definida en `Interfaces/`.

| Servicio | Interfaz | Responsabilidad |
|---|---|---|
| `JwtService` | `ITokenService` | Generación de access tokens JWT |
| `RefreshTokenService` | `IRefreshTokenService` | Gestión de refresh tokens |
| `BcryptService` | `IHasherService` | Hash y verificación de contraseñas |
| `AesGsmEncryptService` | `IEncryptService` | Cifrado/descifrado AES-256-GCM |

### Models

Entidades de dominio con lógica de validación en el constructor. Propiedades con setters privados para proteger la integridad.

| Modelo | Tabla | Descripción |
|---|---|---|
| `User` | `users` | Usuarios registrados |
| `Secret` | `secrets` | Secretos cifrados por usuario |
| `RefreshToken` | `refresh_tokens` | Tokens de refresco para sesión |

### DTO

Data Transfer Objects para validación y transferencia de datos entre capas.

- `UserRegisterDto` - Registro de usuario
- `UserAuthRequestDto` - Credenciales de login
- `SecretRequestDto` - Creación de secreto
- `SecretResponseDto` - Respuesta de secreto (pendiente)

### Data

Configuración de Entity Framework Core y migraciones.

- `DevaultDbContext` - Contexto de base de datos
- `Configuration/` - Configuraciones de flujo (Fluent API)
  - `UserConfiguration`
  - `SecretConfiguration`
  - `RefreshTokenConfiguration`

### Exceptions

Excepciones personalizadas del dominio.

- `EntityException` - Errores de validación en entidades

---

## Diagrama de Entidades

```
┌──────────────┐       ┌──────────────┐       ┌─────────────────┐
│     User     │ 1───∞ │    Secret    │       │   RefreshToken  │
│──────────────│       │──────────────│       │─────────────────│
│ Id (PK)      │       │ Id (PK)      │       │ Id (PK)         │
│ Name         │       │ Name         │       │ UserId (FK)     │
│ Email (UQ)   │       │ EncryptedVal │       │ Token (UQ)      │
│ PasswordHash │       │ UserId (FK)  │       │ CreatedAt       │
│ Rol          │       │ CreatedAt    │       │ ExpiresAt       │
│ CreatedAt    │       │ UpdatedAt    │       │ RevokedAt       │
│ UpdatedAt    │       └──────────────┘       └─────────────────┘
└──────────────┘                │                      │
       │ 1───∞                  │                      │
       └────────────────────────┴──────────────────────┘
```

---

## Cifrado de Secretos

Flujo de cifrado con AES-256-GCM:

1. Se recibe el texto plano
2. Se genera un nonce aleatorio de 16 bytes
3. Se cifra con AES-GCM usando la MasterKey (hasheada con SHA-256)
4. Se almacena: `nonce (12 bytes) + tag (16 bytes) + ciphertext` en Base64

El descifrado invierte el proceso extrayendo cada componente del valor almacenado.

---

## Autenticación

```
┌──────────┐      ┌──────────┐      ┌──────────────┐
│  Cliente │─────→│   Login  │─────→│  BCrypt      │
│          │      │          │      │  Verify      │
│          │      └──────────┘      └──────┬───────┘
│          │                               │
│          │      ┌──────────┐      ┌──────▼───────┐
│          │◄─────│  JWT     │◄─────│  Claims      │
│          │      │  Token   │      │  (Id, Name,  │
└──────────┘      └──────────┘      │   Email,     │
                                    │   Role)      │
                                    └──────────────┘
```

1. El cliente envía credenciales
2. Se verifica el hash con BCrypt
3. Se genera un JWT con claims (NameIdentifier, Name, Email, Role)
4. El token expira en 30 minutos

---

## Paquetes NuGet

| Paquete | Uso |
|---|---|
| `BCrypt.Net-Next` | Hash de contraseñas |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Autenticación JWT |
| `Microsoft.EntityFrameworkCore` | ORM |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Provider PostgreSQL |
| `Microsoft.AspNetCore.OpenApi` | Documentación OpenAPI |

---

## Variables de Configuración

### Jwt (User Secrets)

```json
{
  "Jwt": {
    "SecretKey": "clave-secreta-minimo-32-chars",
    "Issuer": "devault",
    "Audience": "devault"
  }
}
```

### CryptoSettings (User Secrets)

```json
{
  "CryptoSettings": {
    "MasterKey": "tu-master-key-secreta"
  }
}
```

### ConnectionStrings (appsettings.json o User Secrets)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=devault;Username=postgres;Password=your_password"
  }
}
```
