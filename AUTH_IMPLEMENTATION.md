# Detalles de la implementación de Auth

Acá te cuento cómo quedó armada la parte de seguridad y el login. La idea es que sea robusto pero no se vuelva un lío de mantener.

## Archivos clave

- **LoginCommand.cs**: Es lo que recibimos del front (email, pass, slug de la empresa y el ID del dispositivo).
- **LoginResponseDto.cs**: Lo que devolvemos (token JWT, cuándo vence, datos del usuario y a qué pantalla mandarlo según su rol).
- **LoginCommandHandler.cs**: Acá está toda la magia. Valida que el usuario exista, que la empresa (tenant) esté activa y que la contraseña coincida usando BCrypt.

## Cómo funciona la Sesión Única

Queríamos evitar que alguien use la misma cuenta en dos teléfonos al mismo tiempo. Lo que hacemos es:
1. Cuando alguien se loguea, pedimos un "Fingerprint" del dispositivo.
2. Si detectamos que ya había otra sesión abierta en un dispositivo distinto, la matamos (IsRevoked = 1).
3. Así, el usuario siempre tiene una sola sesión activa.

## Seguridad técnica

- **BCrypt:** Las contraseñas no se guardan en texto plano, usamos BCrypt con un factor de trabajo de 12. Es lento a propósito para que sea difícil de hackear.
- **JWT:** Los tokens duran 15 minutos. Es corto, pero así si alguien roba un token, le sirve por poco tiempo.
- **Claims:** Dentro del token metemos el `tenant_id` y el `session_id`. Así el backend sabe quién es el usuario en cada llamada sin volver a preguntar.

## Pendientes

Todavía nos falta configurar un servicio de mail real para los envíos de recuperación de contraseña y meter la rotación de tokens (Refresh Tokens) para que el usuario no tenga que loguearse cada 15 minutos.

Última actualización: 23 de Marzo, 2026.
