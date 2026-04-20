# WMS API Hardening Guide

## What was corrected

- JWT authentication now validates claims with a safer base controller.
- Authorization is enabled globally in the service container.
- The token now carries:
  - `userId`
  - `email`
  - `isSuperAdmin`
  - `companyId` for the default company
  - `role` and `ClaimTypes.Role` for compatibility
  - one `company_access` claim per company available to the user
- Company context is resolved in this order:
  - explicit company id from route/query where the endpoint supports it
  - `X-Company-Id` request header
  - default `companyId` claim in the JWT
- Requests are rejected when the authenticated user tries to operate on a company outside of their allowed `company_access` claims.

## Core files

- `Controllers/ApiControllerBase.cs`
  - Centralizes authenticated user context.
  - Resolves the active company safely.
  - Prevents trusting arbitrary company ids without checking access.
- `Repository/JwtService .cs`
  - Generates multi-company aware JWT claims.
- `Controllers/AuthController.cs`
  - Validates login input and returns the list of companies attached to the user.
- `Program.cs`
  - Enables authorization and keeps JWT claim mapping predictable.

## Functional fixes included

- `ReceivingHeadersController`
  - Now persists `ExternalDocumentNo`, `VendorCode`, `VendorName`, `ReceiptDate`, and line `UOM`.
  - Posting a receipt now creates `PostedReceivingLines`, not just `InventoryMovements`.
- `ItemImagesController`
  - Requires authentication.
  - Validates company access to the item before storing the file.
  - Restricts image extensions and size.
- `ItemsController`, `BinsController`, `StockController`, `InventoryMovementsController`,
  `PostedReceivingHeadersController`, `PostedReceivingLinesController`,
  `CompaniesController`, `DocumentSequencesController`, `PickHeadersController`
  - Moved under authenticated access.
  - Use validated company context instead of trusting unrestricted public access.

## Client integration notes

- Existing login still returns `Companies`.
- If the user has access to multiple companies, the client can send `X-Company-Id`
  to switch company context for requests.
- If the client does not send that header, the API uses the default company embedded
  in the JWT.

## Recommended next steps

- Move `Jwt:Key` and `ConnectionStrings:WmsDb` out of `appsettings.json` into secrets or environment variables.
- Add automated integration tests for:
  - login
  - access denied on foreign company ids
  - item image upload
  - create and post receipt
  - shipment posting
- Normalize remaining nullable-reference warnings entity by entity.
