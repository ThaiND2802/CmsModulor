# Payment Module

Skeleton module for the `Payment` domain in the modular monolith solution.

## API conventions

`Payment` is the first sample module migrated to the shared API response contract:

- `GET /api/payment/methods` and `GET /api/payment/methods/deleted` return `PagedApiResponse<T>`
- `POST /api/payment/methods` returns `201` with `ApiResponse<T>`
- `PUT /api/payment/methods/{id}` and `DELETE /api/payment/methods/{id}` return `200` with `ApiResponse<T>`
- not found cases throw `NotFoundAppException`
- validation failures throw `ValidationAppException`
- duplicate payment method codes throw `ConflictAppException`

List responses include pagination metadata with `total`, `page`, and `pageSize`. Error responses are produced by the host-level global exception middleware and always follow:

```json
{
  "status": 404,
  "errorCode": "1404",
  "message": "Payment method was not found."
}
```

The module keeps the existing soft-delete behavior and shared persistence foundation intact.
