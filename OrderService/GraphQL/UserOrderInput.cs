namespace OrderService.GraphQL
{
    public record UserOrderInput
    (
        int? Id,
        int? OrderId,
        int ProductId,
        int Quantity
    );
}