using System.Security.Claims;
using HotChocolate.AspNetCore.Authorization;
using OrderService.Models;

namespace OrderService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] {"MANAGER"})]
        public IQueryable<Order>GetOrders([Service] SimpleOrderKafkaContext context) => context.Orders;


        [Authorize]
        public IQueryable<Order> GetUserOrder([Service] SimpleOrderKafkaContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userToken = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(u=>u.Username == userToken).FirstOrDefault();
            var userOrder = context.Orders.Where(o=>o.UserId == user.Id).ToList();
            
            return new List<Order>(userOrder).AsQueryable();

        }
    }
}