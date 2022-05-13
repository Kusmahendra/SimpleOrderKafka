using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using OrderProcessor.Models;
using Microsoft.EntityFrameworkCore;


Console.WriteLine("Hello, OrderProcessor!");

IConfiguration configuration = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", true, true)
      .Build();

var config = new ConsumerConfig
{
    BootstrapServers = "127.0.0.1:9092",
    GroupId = "tester",
    AutoOffsetReset = AutoOffsetReset.Earliest

};

var topic = "SimpleOrderKafka";

CancellationTokenSource cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => {
    e.Cancel = true; // prevent the process from terminating.
    cts.Cancel();
};

using (var consumer = new ConsumerBuilder<string, string>(config).Build())
{
    Console.WriteLine("Connected");
    consumer.Subscribe(topic);
    try
    {
        while (true)
        {
        var cr = consumer.Consume(cts.Token);
        Console.WriteLine($"Consumed record with key: {cr.Message.Key} and value: {cr.Message.Value}");

        OrderDetailData message = JsonConvert.DeserializeObject<OrderDetailData>(cr.Message.Value);
        using(var context = new SimpleOrderKafkaContext())
        {
            Order newOrder = new Order();
            newOrder.Code = "Submit dari Kafka";
            newOrder.UserId = message.UserId;
            context.Orders.Add(newOrder);
            context.SaveChanges();

            OrderDetail newOrderDetail = new OrderDetail();
            newOrderDetail.OrderId = newOrder.Id;
            newOrderDetail.ProductId = message.ProductId;
            newOrderDetail.Quantity = message.Quantity;

            context.OrderDetails.Add(newOrderDetail);
            context.SaveChanges();

        }

        }
    }
    catch (OperationCanceledException)
    {
    // Ctrl-C was pressed.
    }
    finally
    {
    consumer.Close();
    }
}

