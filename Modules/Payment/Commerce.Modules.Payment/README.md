# Payment Module

Skeleton module for the `Payment` domain in the modular monolith solution.

## API conventions

`Payment` is the first sample module migrated to the shared API response contract and direct MediatR controller binding:

- controllers bind directly to `Command` / `Query` models
- `Command` / `Query` types implement `IRequest<TResponse>` directly
- request models are `sealed record` types with property-based `init` properties
- route parameters remain separate and are merged in controllers before `_mediator.Send(...)`
- JSON request/response contracts use global `snake_case`
- list query params use explicit `snake_case` names such as `page` and `page_size`
- `GET /api/payment/methods` and `GET /api/payment/methods/deleted` return `PagedApiResponse<T>`
- `POST /api/payment/methods` returns `201` with `ApiResponse<T>`
- `PUT /api/payment/methods/{id}` and `DELETE /api/payment/methods/{id}` return `200` with `ApiResponse<T>`
- not found cases throw `NotFoundAppException`
- validation failures throw `ValidationAppException`
- duplicate payment method codes throw `ConflictAppException`

List responses include pagination metadata with `total`, `page`, and `page_size`. Error responses are produced by the host-level global exception middleware and always follow:

```json
{
  "status": 404,
  "error_code": "1404",
  "message": "Payment method was not found."
}
```

The module keeps the existing soft-delete behavior and shared persistence foundation intact.
