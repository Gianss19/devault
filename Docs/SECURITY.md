# Security

## Philosophy

DevVault follows a **Zero Trust** security model.

No user, request, network or device is trusted by default. Every request must be authenticated, authorized and validated before accessing protected resources.

---

# Authentication

- ASP.NET Core JWT Bearer Authentication
- Passwords hashed with BCrypt
- Access Tokens signed using HS256 (HMAC SHA-256)
- Short-lived Access Tokens
- Refresh Token support planned

---

# Authorization

Every protected endpoint requires authentication.

Authorization is based on:

- JWT Signature
- Claims
- User Roles
- Resource Ownership

Users may only access their own secrets.

---

# Encryption

Secrets are encrypted before being stored.

Algorithm:

- AES-256-GCM

Each encrypted secret stores:

- Ciphertext
- Nonce
- Authentication Tag

Plaintext secrets are never stored in the database.

---

# Password Security

Passwords are hashed using BCrypt.

Passwords are never encrypted.

Passwords are never logged.

Passwords are never recoverable.

---

# JWT Security

JWTs contain only non-sensitive claims.

Current claims:

- User Id
- Username
- Email
- Role
- JTI

JWTs are digitally signed.

JWTs are not encrypted.

---

# Transport Security

Production requires HTTPS.

HTTP is redirected automatically.

HSTS should be enabled.

---

# Database Security

Sensitive values stored:

- Encrypted Secrets
- Password Hashes
- Refresh Token Hashes (planned)

No plaintext credentials are stored.

---

# Input Validation

Every request is validated.

Model validation is enforced.

Unexpected inputs are rejected.

---

# Rate Limiting

Planned protections:

- Login Rate Limiting
- Refresh Token Rate Limiting
- Authenticated API Rate Limiting
- Temporary account lockout

---

# Logging

Security events are logged.

Examples:

- Login
- Failed Login
- Secret Created
- Secret Updated
- Secret Deleted
- Password Changed

Secrets themselves are never logged.

---

# Secret Management

Application secrets are not committed to source control.

Development:

- User Secrets

Production:

- Environment Variables
- Azure Key Vault (planned)

---

# Future Improvements

- Refresh Token Rotation
- Multi-Factor Authentication
- Envelope Encryption
- Key Rotation
- Security Headers
- Audit Dashboard
- Device Management