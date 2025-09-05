
using System.Collections.Concurrent;
using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RequestReply.Application;

namespace RequestReply.Services;

public class RabbitMqSvc : IDisposable
{
   static private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
   static private string? _replyQueueName;
   static private IConnection? _connection;
   static private IChannel? _channel;
   public RabbitMqSvc()
   {
   }

   public static async Task ConnectRm(IConfiguration config)
   {
      var _config = config.GetSection("RabbitMq").Get<RabbitMqConfig>()!;
      ConnectionFactory _factory = new ConnectionFactory
      {
         HostName = _config.Host,
         VirtualHost = _config.Vhost,
         UserName = _config.Username,
         Password = _config.Password
      };
      _factory.AutomaticRecoveryEnabled = true;
      _factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
      _factory.RequestedHeartbeat = TimeSpan.FromMinutes(5);
      _factory.ConsumerDispatchConcurrency = 3;

      _connection = await _factory.CreateConnectionAsync();
      _channel = await _connection.CreateChannelAsync();
      AsyncEventingBasicConsumer _consumer = new AsyncEventingBasicConsumer(_channel);
      // Make sure the request queue exists
      await _channel.QueueDeclareAsync(
          queue: "purchase_q",
          durable: true,
          exclusive: false,
          autoDelete: false,
          arguments: null
      );

      // Make sure the reply queue exists
      var replyQueue = await _channel.QueueDeclareAsync(
          queue: "purchase_reply_q",
          durable: true,
          exclusive: false,
          autoDelete: false,
          arguments: null
      );
      _replyQueueName = replyQueue.QueueName;

      _consumer.ReceivedAsync += async (model, ea) =>
       {
          var correlationId = ea.BasicProperties.CorrelationId!;
          if (_callbackMapper.TryRemove(correlationId, out var tcs))
          {
             var body = ea.Body.ToArray();
             var response = Encoding.UTF8.GetString(body);
             tcs.TrySetResult(response);
          }
          await Task.CompletedTask;
       };

      await _channel.BasicConsumeAsync(consumer: _consumer, queue: _replyQueueName, autoAck: true);
   }

   public async Task<string> CallAsync(string message, string requestQueue)
   {
      var correlationId = Guid.NewGuid().ToString();
      var tcs = new TaskCompletionSource<string>();
      _callbackMapper.TryAdd(correlationId, tcs);

      var props = new BasicProperties();
      props.CorrelationId = correlationId;
      props.ReplyTo = _replyQueueName;

      var messageBytes = Encoding.UTF8.GetBytes(message);

      await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: requestQueue, false, basicProperties: props, body: messageBytes);

      return await tcs.Task;
   }

   public void Dispose()
   {
      _channel?.Dispose();
      _connection?.Dispose();
   }


}
