using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RequestReply;
using RequestReply.Services;


var builder = WebApplication.CreateBuilder(args);

builder.ConfigureRabbitMQ();
builder.ConfigureSwagger();

var app = builder.Build();

app.ConfigureSwagger();
app.UseHttpsRedirection();

app.MapPost("/test", async ([FromBody] object req, [FromServices] RabbitMqSvc svc) =>
{
    var da = await svc.CallAsync(JsonSerializer.Serialize(req), "purchase_q");
    var daObj = JsonSerializer.Deserialize<dynamic>(da);
    Console.WriteLine($"[API] Received response: {da}");
    return Results.Ok(daObj);
});

app.Run();
