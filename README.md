# DevVault

Backend API para gestión de secretos cifrados con autenticación JWT.
*Encrypted secrets management backend API with JWT authentication.*

---

## Descripción / Description

**ES:** DevVault es un proyecto backend construido en ASP.NET Core (.NET 10) que permite a los usuarios registrar cuentas, autenticarse y almacenar secretos de forma segura. Los secretos se cifran con AES-256-GCM antes de persistirse en PostgreSQL, garantizando que el texto plano nunca se almacene en la base de datos.

**EN:** DevVault is a backend project built in ASP.NET Core (.NET 10) that allows users to register accounts, authenticate, and store secrets securely. Secrets are encrypted with AES-256-GCM before being persisted in PostgreSQL, ensuring plaintext is never stored in the database.

> **Nota / Note:** Este proyecto tiene enfoque en **backend**. El frontend incluido fue desarrollado por una IA con el único propósito de proveer una interfaz gráfica para interactuar con la API. No refleja un diseño de UI propio.
>
> This project is **backend-focused**. The included frontend was developed by AI with the sole purpose of providing a graphical interface to interact with the API. It does not represent original UI design.

---

## Stack Tecnológico / Tech Stack

| Componente / Component | Tecnología / Technology |
|---|---|
| Runtime | .NET 10 |
| Framework | ASP.NET Core |
| Base de datos / Database | PostgreSQL 18 |
| ORM | Entity Framework Core |
| Autenticación / Auth | JWT Bearer (HS256) |
| Cifrado / Encryption | AES-256-GCM |
| Hash de contraseñas / Password hashing | BCrypt |
| Contenedores / Containers | Docker + Docker Compose |
| Testing Backend | xUnit + Moq + EF Core InMemory |
| Testing Frontend | Custom HTML test runner |

---

## Requisitos Previos / Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (solo para desarrollo local / only for local dev)
- Docker y Docker Compose / Docker and Docker Compose
- PostgreSQL 18 (incluido en docker compose / included in docker compose)

---

## Configuración / Setup

### 1. Clonar el repositorio / Clone the repository

```bash
git clone https://github.com/tu-usuario/devault.git
cd devault
```

### 2. Configurar User Secrets / Configure User Secrets

```bash
dotnet user-secrets init
```

```bash
dotnet user-secrets set "Jwt:SecretKey" "tu-clave-secreta-minimo-32-caracteres"
dotnet user-secrets set "Jwt:Issuer" "devault"
dotnet user-secrets set "Jwt:Audience" "devault"
dotnet user-secrets set "CryptoSettings:MasterKey" "tu-master-key-secreta"
```

### 3. Configurar cadena de conexión / Configure connection string

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=devault;Username=postgres;Password=tu_password"
```

### 4. Aplicar migraciones / Apply migrations

```bash
dotnet ef database update
```

### 5. Ejecutar / Run

```bash
dotnet run
```

La API estará disponible en `http://localhost:5164`.
The API will be available at `http://localhost:5164`.

---

## Docker

### Iniciar con Docker Compose / Start with Docker Compose

```bash
docker compose up -d
```

Esto levanta PostgreSQL y la API en contenedores.
This starts PostgreSQL and the API in containers.

### Variables de entorno / Environment variables

Crear un archivo `.env` en la raiz (no commitearlo):
Create a `.env` file in the root (do not commit it):

```env
DB_PASSWORD=tu_password_seguro
JWT_SECRET_KEY=tu_clave_secreta_minimo_32_caracteres
MASTER_KEY=tu_master_key_secreta
```

Si no se crea `.env`, docker compose no arrancará hasta que se configuren las variables.
If no `.env` file is created, docker compose will not start until the variables are configured.

### Comandos utiles / Useful commands

```bash
docker compose up -d              # Iniciar en background / Start in background
docker compose down               # Detener y eliminar / Stop and remove
docker compose logs -f api        # Ver logs de la API / View API logs
docker compose down -v            # Detener y eliminar volumes (borra BD) / Stop and remove volumes (deletes DB)
```

La API estara en `http://localhost:5164` y PostgreSQL en `localhost:5432`.
The API will be at `http://localhost:5164` and PostgreSQL at `localhost:5432`.

---

## Estructura del Proyecto / Project Structure

```
devault/
├── Controllers/          # Endpoints de la API / API endpoints
├── Services/             # Lógica de negocio / Business logic
├── Interfaces/           # Contratos de servicios / Service contracts
├── Models/               # Entidades de dominio / Domain entities
├── DTO/                  # Data Transfer Objects
├── Exceptions/           # Excepciones personalizadas / Custom exceptions
├── Data/                 # DbContext y configs EF Core
│   ├── Configuration/    # Fluent API configurations
│   └── Migrations/       # Migraciones de BD / DB migrations
├── Properties/
├── Docs/                 # Documentación / Documentation
├── test/                 # Pruebas unitarias backend / Backend unit tests
├── frontend/             # UI para interactuar con la API (generada por IA)
├── Dockerfile            # Imagen Docker multi-stage / Multi-stage Docker image
├── docker-compose.yml    # Orquestacion de servicios / Service orchestration
├── .dockerignore
├── Program.cs            # Punto de entrada / Entry point
└── appsettings.json
```

---

## Endpoints Principales / Main Endpoints

| Método / Method | Ruta / Route | Descripción / Description | Auth |
|---|---|---|---|
| `POST` | `/api/Auth/signup` | Registrar usuario / Register user | No |
| `POST` | `/api/Auth/login` | Iniciar sesión / Login | No |
| `POST` | `/api/Auth/refresh` | Refrescar token / Refresh token | No |
| `POST` | `/api/Auth/logout` | Cerrar sesión / Logout | Sí / Yes |
| `POST` | `/api/Auth/register-admin` | Registrar admin / Register admin | Admin |
| `POST` | `/api/Secrets/create` | Crear secreto / Create secret | Sí / Yes |
| `GET` | `/api/Secrets/all` | Listar secretos / List secrets | Sí / Yes |
| `GET` | `/api/Secrets/{id}` | Obtener secreto / Get secret | Sí / Yes |
| `GET` | `/api/Secrets/{id}/reveal` | Revelar secreto / Reveal secret | Sí / Yes |
| `PUT` | `/api/Secrets/{id}` | Actualizar secreto / Update secret | Sí / Yes |
| `DELETE` | `/api/Secrets/{id}` | Eliminar secreto / Delete secret | Sí / Yes |
| `GET` | `/api/Users/me` | Mi perfil / My profile | Sí / Yes |
| `GET` | `/api/Users/all` | Listar usuarios / List users | Admin |
| `POST` | `/api/Users/change-name` | Cambiar nombre / Change name | Sí / Yes |
| `POST` | `/api/Users/change-password` | Cambiar contraseña / Change password | Sí / Yes |
| `DELETE` | `/api/Users/{id}` | Eliminar usuario / Delete user | Admin |

Documentación completa en / Full docs at [Docs/API.md](Docs/API.md).

---

## Pruebas / Testing

### Backend

```bash
cd test
dotnet test
```

96 pruebas cubriendo modelos, servicios y controllers.
96 tests covering models, services, and controllers.

### Frontend

Abrir `frontend/test/test-runner.html` en el navegador (via HTTP server).
Open `frontend/test/test-runner.html` in a browser (via HTTP server).

---

## Seguridad / Security

- Contraseñas hasheadas con BCrypt (nunca se almacena texto plano)
  *Passwords hashed with BCrypt (plaintext never stored)*
- Secretos cifrados con AES-256-GCM (nonce + tag + ciphertext)
  *Secrets encrypted with AES-256-GCM (nonce + tag + ciphertext)*
- Tokens JWT de corta duración (30 min)
  *Short-lived JWT tokens (30 min)*
- Refresh tokens con expiración de 7 días y rotación automática
  *Refresh tokens with 7-day expiration and automatic rotation*
- Rate limiting por usuario (10 req / 10 seg) + rate limiting en login (5 req / min)
  *Per-user rate limiting (10 req / 10 sec) + login rate limiting (5 req / min)*
- Headers de seguridad (CSP, X-Frame-Options, nosniff)
  *Security headers (CSP, X-Frame-Options, nosniff)*
- HTTPS en producción / HTTPS in production

Ver / See [Docs/SECURITY.md](Docs/SECURITY.md) for details.

---

## Documentación / Documentation

- [Referencia API / API Reference](Docs/API.md)
- [Arquitectura / Architecture](Docs/ARCHITECTURE.md)
- [Seguridad / Security](Docs/SECURITY.md)
