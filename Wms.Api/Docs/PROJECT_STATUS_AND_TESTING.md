# Project Status And Testing

## Before

- The API mixed authenticated and public access in critical endpoints.
- Company context was inconsistent:
  - some endpoints trusted querystring
  - some used JWT
  - some leaked data across companies
- Receipt posting created stock movements but did not persist posted receipt lines.
- Shipment creation ignored many DTO fields and shipment posting exposed internal exception messages.
- Item image upload was public and accepted files with weak validation.

## Now you have

- A safer authentication base using:
  - `ApiControllerBase`
  - validated company access
  - support for multi-company users through `company_access`
- Protected endpoints for inventory, items, bins, companies, document sequences,
  posted receiving data, stock and file uploads.
- Receipt creation/posting aligned with the data model.
- Shipment creation/posting aligned with the DTO and entity model.
- Better DTO defaults and lower nullability noise.
- Documentation inside `Docs/`.

## How to use it now

1. Login with `POST /api/auth/login`.
2. Keep the returned JWT in `Authorization: Bearer <token>`.
3. If the user has more than one company, send `X-Company-Id: <guid>` on requests.
4. Use normal endpoints:
   - `/api/items`
   - `/api/stock/enriched`
   - `/api/movements`
   - `/api/receivingheaders`
   - `/api/shipmentheaders`

## How to test quickly

### 1. Login

Call:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "your-password"
}
```

Expect:

- `token`
- `Companies`
- one default company in the JWT

### 2. Test token

Call:

```http
GET /api/auth/test
Authorization: Bearer <token>
```

Expect:

- `userId`
- `companyId`
- `role`
- `companies`

### 3. Multi-company switch

Repeat a secured request adding:

```http
X-Company-Id: <company-guid>
```

Expect:

- success only if the logged user has that company inside `Companies`
- `403` if not allowed

### 4. Receipt flow

1. Create receipt with `/api/receivingheaders`
2. Update received qty in `/api/ReceivingLines/{id}`
3. Post with `/api/receivingheaders/{id}/post`

Expect:

- `PostedReceivingHeader`
- `PostedReceivingLines`
- `InventoryMovements` with `IN`

### 5. Shipment flow

1. Create shipment with `/api/shipmentheaders`
2. Update shippable quantities in `/api/shipmentlines/{id}`
3. Post with `/api/shipmentheaders/{id}/post`

Expect:

- `PostedShipments`
- `PostedShipmentLines`
- `InventoryMovements` with `OUT`

## Still recommended

- Move connection strings and JWT key out of `appsettings.json`
- Add integration tests
- Review the remaining nullable entity warnings one by one
