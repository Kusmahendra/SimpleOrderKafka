using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using OrderService.GraphQL;
using HotChocolate;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// database
// dependecy Injection
var connString = builder.Configuration.GetConnectionString("MyDatabase");
builder.Services.AddDbContext<SimpleOrderKafkaContext>(options => 
    options.UseSqlServer(connString)
    );

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));

// graphql
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddAuthorization();

builder.Services.AddControllers();

builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

// role-based identity
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
            ValidateIssuer = true,
            ValidAudience = builder.Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("TokenSettings").GetValue<string>("Key"))),
            ValidateIssuerSigningKey = true
        };

    });



var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapGraphQL(); //graphql app
app.MapGet("/", () => "Hello World!");

app.Run();
