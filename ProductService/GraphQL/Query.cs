using HotChocolate.AspNetCore.Authorization;
using ProductService.Models;

namespace ProductService.GraphQL
{
    public class Query
    {
        [Authorize]
        public IQueryable<Product> GetProducts([Service] SimpleOrderKafkaContext context) =>
            context.Products;
        
    }
}