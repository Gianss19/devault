# API Reference

Base URL: `http://localhost:5164`

---

## Autenticación

Los endpoints protegidos requieren un header `Authorization` con un token JWT válido:

```
Authorization: Bearer <token>
```

---

## Auth

### Registrar Usuario

```
POST /api/Auth
```

**Body:**

```json
{
  "name": "string",
  "email": "string",
  "password": "string"
}
```

| Campo | Tipo | Requisitos |
|---|---|---|
| `name` | string | 3-100 caracteres |
| `email` | string | Formato válido, único |
| `password` | string | No vacío |

**Respuesta:** `201 Created`

---

### Iniciar Sesión

```
POST /api/Auth
```

**Body:**

```json
{
  "email": "string",
  "password": "string"
}
```

**Respuesta:** `200 OK`

```json
{
  "token": "string"
}
```

---

## Secrets

Todos los endpoints de secretos requieren autenticación.

### Crear Secreto

```
POST /api/Secrets
```

**Headers:**
```
Authorization: Bearer <token>
Content-Type: application/json
```

**Body:**

```json
{
  "name": "string"
}
```

| Campo | Tipo | Requisitos |
|---|---|---|
| `name` | string | 3-100 caracteres, único por usuario |

**Respuesta:** `201 Created`

---

### Listar Secretos del Usuario

```
GET /api/Secrets/secrets
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

---

### Obtener Secreto por ID

```
GET /api/Secrets/{id}
```

| Parámetro | Tipo | Descripción |
|---|---|---|
| `id` | guid | ID del secreto |

**Headers:**
```
Authorization: Bearer <token>
```

**Respuesta:** `200 OK`

---

### Eliminar Secreto

```
DELETE /api/Secrets/{id}
```

| Parámetro | Tipo | Descripción |
|---|---|---|
| `id` | guid | ID del secreto |

**Headers:**
```
Authorization: Bearer <token>
```

**Respuesta:** `200 OK`

---

## Errores

| Código | Descripción |
|---|---|
| `400` | Solicitud inválida / validación fallida |
| `401` | No autenticado / token inválido o expirado |
| `403` | No autorizado (rol insuficiente) |
| `404` | Recurso no encontrado |
| `429` | Demasiadas solicitudes (rate limit) |
| `500` | Error interno del servidor |

---

## Rate Limiting

| Política | Límite | Ventana |
|---|---|---|
| `PerUser` | 10 solicitudes | 10 segundos |
| Cola | 2 en espera | - |

Al superar el límite se retorna `429 Too Many Requests`.
