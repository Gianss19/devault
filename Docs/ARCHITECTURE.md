# Arquitectura / Architecture

## Visión General / Overview

Devault sigue una arquitectura **Modular Monolith** con separación por módulos dentro de un único proyecto desplegable.
Devault follows a **Modular Monolith** architecture with module separation within a single deployable project.

```
Controllers → Services → Data (EF Core) → PostgreSQL
    ↕              ↕
  DTOs         Interfaces
```

Cada módulo (Auth, Secrets, Users) agrupa su controller, DTOs y lógica de negocio en un mismo proyecto. Los servicios comparten `DevaultDbContext` a través de Dependency Injection.
Each module (Auth, Secrets, Users) groups its controller, DTOs, and business logic in a single project. Services share `DevaultDbContext` via Dependency Injection.

> **Por qué Modular Monolith / Why Modular Monolith:** Un monolith modular es suficiente para esta escala y evita el over-engineering de Clean Architecture (proyectos separados, capas innecesarias). Si el sistema crece, los módulos pueden extraerse a servicios separados sin reescribir la lógica de negocio.
>
> A modular monolith is sufficient at this scale and avoids the over-engineering of Clean Architecture (separate projects, unnecessary layers). If the system grows, modules can be extracted to separate services without rewriting business logic.

> **Nota / Note:** Este proyecto tiene enfoque en **backend**. El frontend fue desarrollado por una IA con el único propósito de proveer una UI para interactuar con la API.
> This project is **backend-focused**. The frontend was developed by AI solely to provide a UI for API interaction.

---

## Capas / Layers

### Controllers

Reciben solicitudes HTTP, validan input con DTOs, delegan la lógica a los servicios y retornan respuestas HTTP.
Receive HTTP requests, validate input with DTOs, delegate logic to services, and return HTTP responses.

| Controller | Responsabilidad / Responsibility |
|---|---|
| `AuthController` | Registro, login, logout y registro admin / User registration, login, logout, admin registration |
| `SecretsController` | CRUD de secretos cifrados + revelar y actualizar / Encrypted secrets CRUD + reveal and update |
| `UsersController` | Gestión de usuarios (admin), perfil propio y cambio de credenciales / User management (admin), own profile and credential changes |

### Services

Contienen la lógica de negocio. Cada servicio implementa una interfaz definida en `Interfaces/`.
Contain business logic. Each service implements an interface defined in `Interfaces/`.

| Servicio / Service | Interfaz / Interface | Responsabilidad / Responsibility |
|---|---|---|
| `JwtService` | `ITokenService` | Generación de access tokens JWT / JWT access token generation |
| `RefreshTokenService` | `IRefreshTokenService` | Gestión de refresh tokens / Refresh token management |
| `BcryptService` | `IHasherService` | Hash y verificación de contraseñas / Password hashing and verification |
| `AesGsmEncryptService` | `IEncryptService` | Cifrado/descifrado AES-256-GCM / AES-256-GCM encryption/decryption |

### Models

Entidades de dominio con lógica de validación en el constructor. Propiedades con setters privados para proteger la integridad.
Domain entities with validation logic in the constructor. Private setters to protect integrity.

| Modelo / Model | Tabla / Table | Descripción / Description |
|---|---|---|
| `User` | `users` | Usuarios registrados / Registered users |
| `Secret` | `secrets` | Secretos cifrados por usuario / Encrypted secrets per user |
| `RefreshToken` | `refresh_tokens` | Tokens de refresco para sesión / Session refresh tokens |

### DTO

Data Transfer Objects para validación y transferencia de datos entre capas.
Data Transfer Objects for validation and data transfer between layers.

| DTO | Uso / Use |
|---|---|
| `UserRegisterDto` | Registro de usuario / User registration |
| `AdminRegisterDto` | Registro de admin / Admin registration |
| `UserAuthRequestDto` | Credenciales de login / Login credentials |
| `UserResponseDto` | Respuesta con datos del usuario (sin hash) / User data response (no hash) |
| `ChangeNameUserDto` | Cambio de nombre / Name change |
| `ChangePasswordDto` | Cambio de contraseña / Password change |
| `SecretRequestDto` | Creación de secreto / Secret creation |
| `SecretResponseDto` | Respuesta de secreto (metadata, sin valor cifrado) / Secret response (metadata, no encrypted value) |
| `SecretDetailResponseDto` | Respuesta con secreto descifrado / Response with decrypted secret |
| `SecretUpdateDto` | Actualización de secreto / Secret update |
| `TokenResponseDto` | Respuesta de autenticación con tokens / Auth response with tokens |
| `RefreshTokenRequestDto` | Solicitud con refresh token / Request with refresh token |

### Data

Configuración de Entity Framework Core y migraciones.
Entity Framework Core configuration and migrations.

- `DevaultDbContext` - Contexto de base de datos / Database context
- `Configuration/` - Configuraciones Fluent API

### Exceptions

Excepciones personalizadas del dominio.
Custom domain exceptions.

- `EntityException` - Errores de validación en entidades / Entity validation errors

---

## Diagrama de Entidades / Entity Diagram

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

## Cifrado de Secretos / Secret Encryption

Flujo de cifrado con AES-256-GCM:
AES-256-GCM encryption flow:

1. Se recibe el texto plano / Plaintext is received
2. Se genera un nonce aleatorio de 12 bytes / A random 12-byte nonce is generated
3. Se cifra con AES-GCM usando la MasterKey (hasheada con SHA-256) / Encrypted with AES-GCM using MasterKey (SHA-256 hashed)
4. Se almacena: `nonce (12 bytes) + tag (16 bytes) + ciphertext` en Base64 / Stored as: `nonce (12 bytes) + tag (16 bytes) + ciphertext` in Base64

El descifrado invierte el proceso extrayendo cada componente del valor almacenado.
Decryption reverses the process by extracting each component from the stored value.

---

## Autenticación / Authentication

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

1. El cliente envía credenciales / Client sends credentials
2. Se verifica el hash con BCrypt / Hash is verified with BCrypt
3. Se genera un JWT con claims (NameIdentifier, Name, Email, Role) / JWT is generated with claims
4. Se genera un refresh token (7 días) / Refresh token is generated (7 days)
5. El access token expira en 30 minutos / Access token expires in 30 minutes

---

## Middleware Pipeline

El orden del pipeline en `Program.cs`:
Pipeline order in `Program.cs`:

1. Exception Handler (manejo global de errores / global error handling)
2. ForwardedHeaders (para reverse proxies como Azure / for reverse proxies like Azure)
3. Security Headers (X-Content-Type-Options, X-Frame-Options, CSP)
4. CORS (orígenes configurados / configured origins)
5. Default Files (servir index.html como raíz / serve index.html as root)
6. Static Files (frontend desde wwwroot / frontend from wwwroot)
7. HSTS (solo producción / production only)
8. HTTPS Redirection
9. Rate Limiter (10 req / 10 seg por usuario + 5 req / min en login)
10. Authentication (JWT Bearer)
11. Authorization (roles)
12. Controllers
13. Health Checks (`/health`)

---

## Paquetes NuGet / NuGet Packages

| Paquete / Package | Uso / Purpose |
|---|---|
| `BCrypt.Net-Next` | Hash de contraseñas / Password hashing |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Autenticación JWT / JWT authentication |
| `Microsoft.EntityFrameworkCore` | ORM |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Provider PostgreSQL |
| `Microsoft.AspNetCore.OpenApi` | Documentación OpenAPI / OpenAPI docs |
| `AspNetCore.HealthChecks.NpgSql` | Health check de PostgreSQL |

---

## Frontend

```
frontend/
├── index.html          # Entry point (CSP, X-Frame-Options en meta)
├── css/styles.css      # Dark theme responsive
├── js/
│   ├── utils.js        # escapeHtml, validaciones, DOM helpers
│   ├── api.js          # Capa de comunicación con la API
│   ├── auth.js         # Manejo de sesión JWT en memoria
│   └── app.js          # UI: login, signup, secrets, users, profile
└── test/
    ├── test-runner.html # Runner de pruebas en navegador
    ├── utils.test.js    # Tests de utilidades
    ├── api.test.js      # Tests de la capa API
    └── auth.test.js     # Tests de autenticación
```

El frontend fue generado por IA y usa HTML/CSS/JS puro sin dependencias externas. Almacena tokens JWT únicamente en memoria (nunca en localStorage o cookies). Usa `textContent` en lugar de `innerHTML` para prevenir XSS.
The frontend was generated by AI and uses vanilla HTML/CSS/JS with no external dependencies. Tokens are stored in memory only (never localStorage or cookies). Uses `textContent` instead of `innerHTML` to prevent XSS.

---

## Configuración / Configuration

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
    "DefaultConnection": "Host=localhost;Port=5432;Database=devault;Username=postgres;Password=tu_password"
  }
}
```
