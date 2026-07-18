# DevVault

Backend API para gestión de secretos cifrados con autenticación JWT.

## Descripción

DevVault es una API REST construida en ASP.NET Core que permite a los usuarios registrar cuentas, autenticarse y almacenar secretos de forma segura. Los secretos se cifran con AES-256-GCM antes de persistirse en PostgreSQL, garantizando que el texto plano nunca se almacene en la base de datos.

## Stack Tecnológico

| Componente | Tecnología |
|---|---|
| Runtime | .NET 10 |
| Framework | ASP.NET Core |
| Base de datos | PostgreSQL |
| ORM | Entity Framework Core |
| Autenticación | JWT Bearer (HS256) |
| Cifrado | AES-256-GCM |
| Hash de contraseñas | BCrypt |

## Requisitos Previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL en ejecución (puerto 5432)
- User Secrets de .NET configurado

## Configuración

### 1. Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/devault.git
cd devault
```

### 2. Configurar User Secrets

```bash
dotnet user-secrets init
```

```bash
dotnet user-secrets set "Jwt:SecretKey" "tu-clave-secreta-minimo-32-caracteres"
dotnet user-secrets set "Jwt:Issuer" "devault"
dotnet user-secrets set "Jwt:Audience" "devault"
dotnet user-secrets set "CryptoSettings:MasterKey" "tu-master-key-secreta"
```

### 3. Configurar cadena de conexión

Editar `appsettings.json` o usar User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=devault;Username=postgres;Password=tu_password"
```

### 4. Aplicar migraciones

```bash
dotnet ef database update
```

### 5. Ejecutar

```bash
dotnet run
```

La API estará disponible en `http://localhost:5164`.

## Estructura del Proyecto

```
devault/
├── Controllers/       # Endpoints de la API
├── Services/          # Lógica de negocio
├── Interfaces/        # Contratos de servicios
├── Models/            # Entidades de dominio
├── DTO/               # Data Transfer Objects
├── Exceptions/        # Excepciones personalizadas
├── Data/              # DbContext y configuraciones EF Core
│   ├── Configuration/
│   └── Migrations/
├── Properties/
├── Docs/              # Documentación adicional
└── Program.cs         # Punto de entrada
```

## Endpoints Principales

| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| `POST` | `/api/Auth` | Registrar usuario | No |
| `POST` | `/api/Auth` | Iniciar sesión | No |
| `POST` | `/api/Secrets` | Crear secreto | Sí |
| `GET` | `/api/Secrets/secrets` | Listar secretos del usuario | Sí |
| `GET` | `/api/Secrets/{id}` | Obtener secreto por ID | Sí |
| `DELETE` | `/api/Secrets/{id}` | Eliminar secreto | Sí |

Documentación completa en [Docs/API.md](Docs/API.md).

## Seguridad

- Contraseñas hasheadas con BCrypt (nunca se almacena texto plano)
- Secretos cifrados con AES-256-GCM (nonce + tag + ciphertext)
- Tokens JWT de corta duración (30 min)
- Refresh tokens con expiración de 7 días
- Rate limiting por usuario (10 req / 10 seg)
- Validación de claims en JWT
- HTTPS en producción

Ver [Docs/SECURITY.md](Docs/SECURITY.md) para detalles completos.

## Documentación

- [API Reference](Docs/API.md)
- [Arquitectura](Docs/ARCHITECTURE.md)
- [Seguridad](Docs/SECURITY.md)
