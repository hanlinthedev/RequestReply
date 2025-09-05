using System;

namespace QueueWorker.Application;

public class RabbitMqConfig
{
   public required string Host { get; set; }
   public required string Vhost { get; set; }
   public required string Username { get; set; }
   public required string Password { get; set; }
}
