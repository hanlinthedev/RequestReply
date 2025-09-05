using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using QueueWorker.Application;

namespace QueueWorker;

public static class RabbitMqSvc
{
   private static IConnection? _connection;
   private static IChannel? _channel;

   // Use the same queue name as your publisher
   private const string RequestQueueName = "purchase_q";

   public static async Task ConnectRm(IConfiguration config)
   {
      var rabbitMqConfig = config.GetSection("RabbitMq").Get<RabbitMqConfig>()!;
      var factory = new ConnectionFactory
      {
         HostName = rabbitMqConfig.Host,
         VirtualHost = rabbitMqConfig.Vhost,
         UserName = rabbitMqConfig.Username,
         Password = rabbitMqConfig.Password,
         AutomaticRecoveryEnabled = true,
         NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
         RequestedHeartbeat = TimeSpan.FromMinutes(5),
         ConsumerDispatchConcurrency = 3
      };

      _connection = await factory.CreateConnectionAsync();
      _channel = await _connection.CreateChannelAsync();
      var consumer = new AsyncEventingBasicConsumer(_channel);

      // Ensure the request queue exists
      await _channel.QueueDeclareAsync(
          queue: RequestQueueName,
          durable: true,
          exclusive: false,
          autoDelete: false,
          arguments: null
      );

      // Configure the consumer to handle incoming messages
      consumer.ReceivedAsync += async (model, ea) =>
      {
         var body = ea.Body.ToArray();
         var message = Encoding.UTF8.GetString(body);
         var props = ea.BasicProperties;
         var replyProps = new BasicProperties();
         replyProps.CorrelationId = props.CorrelationId;
         Console.WriteLine($"[Worker] Received message with CorrelationId: {props.CorrelationId}");
         Console.WriteLine($"[Worker] Message content: {message}");
         try
         {
            var request = JsonSerializer.Deserialize<dynamic>(message);
            string productId = request!.GetProperty("ProductId").GetString();
            int quantity = request!.GetProperty("Quantity").GetInt32();
            Console.WriteLine($"[Worker] Processing request...{request}");
            Console.WriteLine($"[Worker] Received request for Product: {productId}");

            // --- Simulate processing ---
            var response = new
            {
               IsSuccess = true,
               StatusMessage = "Order successfully processed.",
               OrderId = Guid.NewGuid().ToString()
            };
            // --------------------------
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

            // Publish the response back to the client's reply queue
            await _channel.BasicPublishAsync(
                  exchange: string.Empty,
                  routingKey: props.ReplyTo!, // Use the reply_to from the request
                  mandatory: false,
                  basicProperties: replyProps,
                  body: responseBytes
              );
         }
         catch (Exception ex)
         {
            Console.WriteLine($"[Worker] Error processing message: {ex.Message}");
            // You should handle error responses here as well
         }
         finally
         {
            // Acknowledge the message regardless of outcome
            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
         }
      };

      // Start consuming messages from the request queue
      await _channel.BasicConsumeAsync(consumer: consumer, queue: RequestQueueName, autoAck: false);
   }
}