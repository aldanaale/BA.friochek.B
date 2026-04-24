# API MAESTRO — BA.FrioCheck

Guía exhaustiva para la integración multiplataforma con el Backend de FrioCheck.

---

## 🔐 CABECERAS Y SEGURIDAD

*   **Base URL**: `http://localhost:5003`
*   **Auth**: `Authorization: Bearer <JWT_TOKEN>`
*   **Roles**: Solo los endpoints de `/auth` y `/ping` son públicos.

---

## 📡 CATÁLOGO DE ENDPOINTS Y PAYLOADS

### 1. Autenticación (`Auth`)
| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| `POST` | `/auth/login` | Anónimo | Inicio de sesión global. |

**Request JSON:**
```json
{ "email": "...", "password": "..." }
```

---

### 2. Retailer / Cliente (`Cliente`)
| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| `GET` | `/cliente/home` | Cliente | Dashboard de la tienda. |
| `POST` | `/cliente/pedido` | Cliente | Orden rápida de productos. |

**Request JSON (Pedido):**
```json
{
  "entries": [
    { "coolerId": "...", "items": [{ "productId": "...", "quantity": 10 }] }
  ]
}
```

---

### 3. Logística / Transporte (`Transportista`)
| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| `GET` | `/transportista/route` | Transp. | Hoja de ruta diaria. |
| `POST` | `/transportista/delivery` | Transp. | Confirmar entrega (Stock). |

**Request JSON (Entrega):**
```json
{
  "orderId": "...", "routeStopId": "...", "nfcAccessToken": "...", "deliveredItems": [...]
}
```

---

### 4. Soporte y Activos (`Coolers / NFC`)
| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| `POST` | `/nfc/validate` | Cualquiera | Valida Tag NFC físico. |
| `POST` | `/nfc/enroll` | Admin | Asocia Tag a Cooler. |

**Response JSON (Validate):**
```json
{ "success": true, "data": { "coolerId": "...", "nfcAccessToken": "..." } }
```

---

### 5. Administración Central (`Admin / Users`)
| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| `GET` | `/admin/dashboard` | Admin | Estadísticas globales. |
| `POST` | `/users` | Admin | Crear nuevo usuario. |

---

## 🚦 RESPUESTAS DE ERROR ESTÁNDAR

`400/401/403/500`

```json
{
  "success": false,
  "errorCode": "ERROR_CODE",
  "message": "Descripción legible",
  "errors": { "Campo": ["Error"] }
}
```
