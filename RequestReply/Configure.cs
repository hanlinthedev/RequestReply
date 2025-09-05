using System;
using RequestReply.Services;

namespace RequestReply;

public static partial class Configure
{
   public static void ConfigureSwagger(this WebApplicationBuilder builder)
   {
      // Add services to the container.
      // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
      builder.Services.AddOpenApi();
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();
   }

   public static void ConfigureSwagger(this WebApplication app)
   {
      if (app.Environment.IsDevelopment())
      {
         app.MapOpenApi();
         app.UseSwagger();
         app.UseSwaggerUI();
      }
   }

   public static void ConfigureRabbitMQ(this WebApplicationBuilder builder)
   {
      builder.Services.AddSingleton<RabbitMqSvc>();
      builder.Services.AddHostedService<RabbitMqInitializer>();
   }
}
