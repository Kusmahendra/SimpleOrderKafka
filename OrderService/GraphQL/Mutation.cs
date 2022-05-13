using OrderService.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {

        [Authorize]
        public async Task<OrderDetail> AddUserOrderAsync(
            UserOrderInput input,
            [Service] SimpleOrderKafkaContext context,
            ClaimsPrincipal claimsPrincipal)
        {

            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var order = new Order
            {
                Code = "101",
                UserId = user.Id
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var orderDetail = new OrderDetail
            {
                OrderId = order.Id,
                ProductId = input.ProductId,
                Quantity = input.Quantity
            };
            context.OrderDetails.Add(orderDetail);
            await context.SaveChangesAsync();


            //for (int i =0; i <input.)            
            return orderDetail;
        }
        [Authorize]
        public async Task<OrderOutput> SubmitOrderAsync(OrderDetailData input,
            [Service] IOptions<KafkaSettings> settings, 
            ClaimsPrincipal claimsPrincipal,
            [Service] SimpleOrderKafkaContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var order = new OrderDetailData
            {
                UserId = user.Id,
                ProductId = input.ProductId,
                Quantity = input.Quantity
            };
            var dts = DateTime.Now.ToString();
            var key = "order-" + dts;
            var val = JsonConvert.SerializeObject(order);

            var result = await KafkaHelper.SendMessage(settings.Value, "simpleorder", key, val);

            var messageResult = "Order was submitted successfully :" + val;

            OrderOutput resp = new OrderOutput
            {
                TransactionDate = dts,
                Message = messageResult
            };

            if (!result)
                resp.Message = "Failed to submit data";
         
            return await Task.FromResult(resp);
        }

        // public async Task<Order> AddOrderAsync(Order input, [Service] SimpleOrderKafkaContext context)
        // {
        //     var order = new Order
        //     {
        //         Type = input.Type,
        //         CompleteStatus = input.CompleteStatus
        //     };

        //     //EF
        //     context.Orders.Add(order);
        //     await context.SaveChangesAsync();

        //     return order;
            
        // }
            // 


    //     }

        // [Authorize]
        // public async Task<OrderData> AddOrderAsync(
        //     OrderData input,
        //     ClaimsPrincipal claimsPrincipal,
        //     [Service] SimpleOrderKafkaContext context)
        // {
        //     using var transaction = context.Database.BeginTransaction();
        //     var userName = claimsPrincipal.Identity.Name;

        //     try
        //     {
        //         var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
        //         if (user != null)
        //         {
        //             // EF
        //             var order = new Order
        //             {
        //                 Code = Guid.NewGuid().ToString(), // generate random chars using GUID
        //                 UserId = user.Id
        //             };
                                     
        //             foreach (var item in input.Details)
        //             {
        //                 var detial = new OrderDetail
        //                 {
        //                     OrderId = order.Id,
        //                     ProductId = item.ProductId,
        //                     Quantity = item.Quantity
        //                 };
        //                 order.OrderDetails.Add(detial);            
        //             }
        //             context.Orders.Add(order);
        //             context.SaveChanges();
        //             await transaction.CommitAsync();

        //             input.Id = order.Id;
        //             input.Code = order.Code;
        //         }
        //         else
        //             throw new Exception("user was not found");
        //     }
        //     catch(Exception err)
        //     {
        //         transaction.Rollback();
        //     }

        //     return input;
        // }
        

        
    }
}