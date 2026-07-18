# Referencia API / API Reference

Base URL: `http://localhost:5164`

---

## Autenticación / Authentication

Los endpoints protegidos requieren un header `Authorization` con un token JWT válido:
Protected endpoints require an `Authorization` header with a valid JWT token:

```
Authorization: Bearer <token>
```

---

## Auth

### Registrar Usuario / Register User

```
POST /api/Auth/signup
```

**Body:**

```json
{
  "name": "string",
  "email": "string",
  "password": "string"
}
```

| Campo / Field | Tipo / Type | Requisitos / Requirements |
|---|---|---|
| `name` | string | 3-100 caracteres / characters |
| `email` | string | Formato válido, único / Valid format, unique |
| `password` | string | 8+ chars, 1 uppercase, 1 lowercase, 1 digit, 1 special |

**Respuesta exitosa / Success:** `200 OK`

```json
{
  "id": "guid",
  "name": "string",
  "email": "string"
}
```

**Errores / Errors:**
- `400` - "User is null" / "User already exists" / "Contraseña No segura."

---

### Iniciar Sesión / Login

```
POST /api/Auth/login
```

**Body:**

```json
{
  "email": "string",
  "password": "string"
}
```

**Respuesta exitosa / Success:** `200 OK`

```json
{
  "accessToken": "string (JWT)",
  "refreshToken": "string",
  "expiresAt": "datetime"
}
```

**Errores / Errors:**
- `400` - "Invalid Credentials."

---

### Cerrar Sesión / Logout

```
POST /api/Auth/logout
```

**Headers:**
```
Authorization: Bearer <token>
```

**Body:**

```json
{
  "token": "string (refresh token)"
}
```

**Respuesta exitosa / Success:** `200 OK`

**Errores / Errors:**
- `400` - "Token inválido o ya revocado."

---

## Secrets

Todos los endpoints de secretos requieren autenticación y rol User o Admin.
All secret endpoints require authentication with User or Admin role.

### Crear Secreto / Create Secret

```
POST /api/Secrets/create
```

**Headers:**
```
Authorization: Bearer <token>
Content-Type: application/json
```

**Body:**

```json
{
  "name": "string",
  "value": "string"
}
```

| Campo / Field | Tipo / Type | Requisitos / Requirements |
|---|---|---|
| `name` | string | 3-100 caracteres / characters, único por usuario / unique per user |
| `value` | string | Texto plano del secreto / Secret plaintext |

**Respuesta exitosa / Success:** `201 Created`

**Errores / Errors:**
- `400` - "El nombre no puede estar vacío." / "El valor del secreto no puede estar vacío."
- `401` - "Token inválido."

---

### Listar Secretos / List Secrets

```
GET /api/Secrets/all
```

**Headers:**
```
Authorization: Bearer <token>
```

**Respuesta:** `200 OK`

```json
[
  {
    "id": "guid",
    "name": "string",
    "createdAt": "datetime"
  }
]
```

> **Nota / Note:** El valor cifrado del secreto nunca se retorna en esta respuesta.
> The encrypted value of the secret is never returned in this response.

---

### Obtener Secreto por ID / Get Secret by ID

```
GET /api/Secrets/{id:guid}
```

| Parámetro / Param | Tipo / Type | Descripción / Description |
|---|---|---|
| `id` | guid | ID del secreto / Secret ID |

**Headers:**
```
Authorization: Bearer <token>
```

**Respuesta:** `200 OK`

```json
{
  "id": "guid",
  "name": "string",
  "createdAt": "datetime"
}
```

**Errores / Errors:**
- `404` - Secreto no encontrado o no pertenece al usuario / Not found or not owned by user

---

### Eliminar Secreto / Delete Secret

```
DELETE /api/Secrets/{id:guid}
```

| Parámetro / Param | Tipo / Type | Descripción / Description |
|---|---|---|
| `id` | guid | ID del secreto / Secret ID |

**Headers:**
```
Authorization: Bearer <token>
```

**Respuesta exitosa / Success:** `204 No Content`

**Errores / Errors:**
- `404` - Secreto no encontrado o no pertenece al usuario

---

## Users

### Listar Todos los Usuarios / List All Users (Admin)

```
GET /api/Users/all
```

**Headers:**
```
Authorization: Bearer <token>
```

**Requiere rol / Requires role:** `Admin`

**Respuesta:** `200 OK`

```json
[
  {
    "id": "guid",
    "name": "string",
    "email": "string"
  }
]
```

---

### Cambiar Nombre / Change Name

```
POST /api/Users/change-name
```

**Body:**

```json
{
  "newName": "string"
}
```

| Campo / Field | Tipo / Type | Requisitos / Requirements |
|---|---|---|
| `newName` | string | 3-100 caracteres / characters |

**Respuesta exitosa / Success:** `200 OK`

---

### Cambiar Contraseña / Change Password

```
POST /api/Users/change-password
```

**Body:**

```json
{
  "newPassword": "string"
}
```

| Campo / Field | Tipo / Type | Requisitos / Requirements |
|---|---|---|
| `newPassword` | string | 8+ chars, 1 uppercase, 1 lowercase, 1 digit, 1 special |

**Respuesta exitosa / Success:** `200 OK`

---

### Eliminar Usuario / Delete User (Admin)

```
DELETE /api/Users/{id:guid}
```

| Parámetro / Param | Tipo / Type | Descripción / Description |
|---|---|---|
| `id` | guid | ID del usuario / User ID |

**Requiere rol / Requires role:** `Admin`

**Respuesta exitosa / Success:** `204 No Content`

> **Nota / Note:** Eliminar un usuario elimina en cascada sus secretos y refresh tokens.
> Deleting a user cascade-deletes their secrets and refresh tokens.

---

## Errores / Errors

| Código / Code | Descripción / Description |
|---|---|
| `400` | Solicitud inválida / Bad request |
| `401` | No autenticado / Unauthenticated |
| `403` | No autorizado / Forbidden (insufficient role) |
| `404` | Recurso no encontrado / Not found |
| `429` | Demasiadas solicitudes / Too many requests |
| `500` | Error interno del servidor / Internal server error |

---

## Rate Limiting

| Política / Policy | Límite / Limit | Ventana / Window |
|---|---|---|
| `PerUser` | 10 solicitudes / requests | 10 segundos / seconds |
| Cola / Queue | 2 en espera / queued | - |

Al superar el límite se retorna `429 Too Many Requests`.
When the limit is exceeded, `429 Too Many Requests` is returned.
