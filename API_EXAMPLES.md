# Ejemplos de la API

Acá tenés cómo pegarle a los endpoints principales usando `curl`. Acordate que el puerto por defecto es el 5000.

## 1. Login (Autenticación)

Para entrar al sistema, mandale un POST así:

```bash
curl -X POST "http://localhost:5000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@test.com",
    "password": "Admin123!",
    "tenantSlug": "admin"
  }'
```

Si todo sale bien, te va a devolver un JSON con el `accessToken`. Copialo porque lo vas a necesitar para lo demás.

---

## 📱 Front-End Móvil (App del Cliente)

### 2. Validar NFC
Escaneás un tag y te dice qué cooler es.
```bash
curl -X POST "http://localhost:5000/api/v1/nfc/validate" \
  -H "Authorization: Bearer TU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "nfcUid": "ABC123XYZ" }'
```

### 3. Crear un Pedido
Después de escanear, iniciás el pedido.
```bash
curl -X POST "http://localhost:5000/api/v1/cliente/orders" \
  -H "Authorization: Bearer TU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "nfcUid": "ABC123XYZ", "coolerId": "GUID_DEL_COOLER" }'
```

### 4. Soporte Técnico (Multipart)
Para mandar la falla con fotos de la cámara.
```bash
curl -X POST "http://localhost:5000/api/v1/cliente/tech-support" \
  -H "Authorization: Bearer TU_TOKEN" \
  -F "nfcUid=ABC123XYZ" \
  -F "faultType=Electrica" \
  -F "description=No enfria nada" \
  -F "scheduledDate=2026-03-25T10:00:00" \
  -F "photos=@falla1.jpg"
```

---

## Front-End Web (Administración)

### 5. Tiendas (Stores)
Para estas llamadas, tenés que estar logueado como Admin.

#### Listar locales
```bash
curl -X GET "http://localhost:5000/api/v1/stores" \
  -H "Authorization: Bearer PEGA_ACA_TU_TOKEN"
```

#### Crear un local nuevo
```bash
curl -X POST "http://localhost:5000/api/v1/stores" \
  -H "Authorization: Bearer TU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Local Vitacura 1",
    "address": "Av. Vitacura 1234",
    "contactName": "Juan",
    "latitude": -33.40,
    "longitude": -70.57
  }'
```

---

## Tips rápidos
- **Tenant ID:** No hace falta que lo mandes en las operaciones de tiendas o pedidos, el backend lo saca solito de tu token JWT.
- **Redirección:** En el login, fijate que viene un campo `redirectTo`. Usalo para saber a qué página mandar al usuario en el front.
- **Validación de Capacidad:** Si al agregar items te pasás de la capacidad del cooler, te va a rebotar con un error 400 `CAPACITY_EXCEEDED`.

Última actualización: 23 de Marzo, 2026.
