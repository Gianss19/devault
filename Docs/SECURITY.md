# Seguridad / Security

## Filosofía / Philosophy

DevVault sigue un modelo de seguridad **Zero Trust**.
DevVault follows a **Zero Trust** security model.

Ningún usuario, solicitud, red o dispositivo es confiable por defecto. Cada solicitud debe ser autenticada, autorizada y validada antes de acceder a recursos protegidos.
No user, request, network, or device is trusted by default. Every request must be authenticated, authorized, and validated before accessing protected resources.

---

## Autenticación / Authentication

- ASP.NET Core JWT Bearer Authentication
- Contraseñas hasheadas con BCrypt / Passwords hashed with BCrypt
- Access Tokens firmados con HS256 (HMAC SHA-256) / Access Tokens signed with HS256
- Access Tokens de corta duración (30 min) / Short-lived Access Tokens (30 min)
- Refresh Tokens con expiración de 7 días / Refresh Tokens with 7-day expiration
- Refresh Token Rotation: al usar un refresh token, se revoca el anterior / Refresh Token Rotation: using a refresh token revokes the previous one
- Audience validation habilitada en JWT / Audience validation enabled on JWT

---

## Autorización / Authorization

Cada endpoint protegido requiere autenticación.
Every protected endpoint requires authentication.

La autorización se basa en:
Authorization is based on:

- Firma del JWT / JWT Signature
- Claims del usuario / User claims
- Roles del usuario / User roles
- Propiedad del recurso / Resource ownership

Los usuarios solo pueden acceder a sus propios secretos.
Users may only access their own secrets.

---

## Cifrado / Encryption

Los secretos se cifran antes de almacenarse.
Secrets are encrypted before being stored.

**Algoritmo / Algorithm:** AES-256-GCM

Cada secreto cifrado almacena:
Each encrypted secret stores:

- Ciphertext (texto cifrado / ciphertext)
- Nonce (vector de inicialización / initialization vector)
- Authentication Tag (etiqueta de autenticación / authentication tag)

El texto plano de los secretos nunca se almacena en la base de datos.
Plaintext secrets are never stored in the database.

---

## Seguridad de Contraseñas / Password Security

Las contraseñas se hashean usando BCrypt.
Passwords are hashed using BCrypt.

Las contraseñas nunca se cifran (solo se hashean).
Passwords are never encrypted (only hashed).

Las contraseñas nunca se registran en logs.
Passwords are never logged.

Las contraseñas nunca son recuperables.
Passwords are never recoverable.

Requisitos de contraseña / Password requirements:
- Mínimo 8 caracteres / Minimum 8 characters
- 1 mayúscula / 1 uppercase
- 1 minúscula / 1 lowercase
- 1 dígito / 1 digit
- 1 carácter especial / 1 special character

---

## JWT Security

Los JWTs contienen solo claims no sensibles.
JWTs contain only non-sensitive claims.

Claims actuales / Current claims:
- User Id (NameIdentifier)
- Username (Name)
- Email
- Role (User / Admin)

Los JWTs están firmados digitalmente.
JWTs are digitally signed.

Los JWTs no están cifrados (el payload es legible).
JWTs are not encrypted (payload is readable).

---

## Seguridad en Transporte / Transport Security

Producción requiere HTTPS.
Production requires HTTPS.

HTTP se redirige automáticamente.
HTTP is redirected automatically.

HSTS está habilitado en producción.
HSTS is enabled in production.

---

## Seguridad de Base de datos / Database Security

Valores sensibles almacenados / Sensitive values stored:
- Secretos cifrados / Encrypted secrets
- Hashes de contraseñas / Password hashes
- Refresh tokens (almacenados como hash SHA256) / Refresh tokens (stored as SHA256 hash)

No se almacenan credenciales en texto plano.
No plaintext credentials are stored.

---

## Validación de Entrada / Input Validation

Cada solicitud es validada.
Every request is validated.

La validación de modelos está habilitada.
Model validation is enabled.

Entradas inesperadas son rechazadas.
Unexpected inputs are rejected.

---

## Rate Limiting

Política `PerUser` configurada:
`PerUser` policy configured:

- 10 solicitudes por ventana de 10 segundos / 10 requests per 10-second window
- Cola de 2 en espera / Queue of 2
- Rechazo con `429 Too Many Requests` / Rejected with `429 Too Many Requests`
- Clave por `NameIdentifier` (autenticado) o IP (anónimo) / Keyed by `NameIdentifier` (authenticated) or IP (anonymous)

Política `Login` configurada:
`Login` policy configured:

- 5 solicitudes por minuto por IP / 5 requests per minute per IP
- Sin cola / No queue
- Aplicada solo al endpoint `/api/Auth/login` / Applied only to `/api/Auth/login` endpoint

---

## Headers de Seguridad / Security Headers

Configurados en middleware y meta tags:
Configured in middleware and meta tags:

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Referrer-Policy: no-referrer`
- `Content-Security-Policy: frame-ancestors 'none'`
- CSP en frontend: `script-src 'self'`, `connect-src 'self' http://localhost:5164`

---

## CORS

Política `AllowFrontend` configurada:
`AllowFrontend` policy configured:

- En desarrollo / In development: permite `localhost`, `127.0.0.1`, orígenes configurados y origen `null` (para pruebas desde archivos locales)
- En desarrollo: allows `localhost`, `127.0.0.1`, configured origins, and `null` origin (for local file testing)
- En producción / In production: solo orígenes configurados vía `Cors:Origins` en configuración
- In production: only origins configured via `Cors:Origins` in configuration
- `AllowCredentials()` habilitado / enabled

---

## ForwardedHeaders

Habilitado para reverse proxies (Azure App Service, Nginx, etc.):
Enabled for reverse proxies (Azure App Service, Nginx, etc.):

- `XForwardedFor` - Preserva la IP real del cliente / Preserves real client IP
- `XForwardedProto` - Detecta HTTPS correctamente detrás del proxy / Correctly detects HTTPS behind proxy

---

## Gestión de Secretos / Secret Management

Los secretos de la aplicación no se incluyen en el control de versiones.
Application secrets are not committed to source control.

Desarrollo / Development:
- User Secrets de .NET / .NET User Secrets

Producción / Production:
- Variables de entorno / Environment Variables
- Azure Key Vault (planeado / planned)

---

## Testing de Seguridad / Security Testing

96 pruebas unitarias cubren modelos, servicios y controllers.
96 unit tests cover models, services, and controllers.

Pruebas de frontend verifican:
Frontend tests verify:

- escapeHtml previene XSS / escapeHtml prevents XSS
- Validación de emails y contraseñas / Email and password validation
- Enmascaramiento de valores sensibles / Sensitive value masking
- Gestión de tokens en memoria / In-memory token management

---

## Mejoras Futuras / Future Improvements

- Autenticación Multi-Factor / Multi-Factor Authentication
- Envelope Encryption
- Rotación de claves / Key Rotation
- Dashboard de auditoría / Audit Dashboard
- Gestión de dispositivos / Device Management
- Bloqueo de cuenta por intentos fallidos / Account lockout on failed attempts
