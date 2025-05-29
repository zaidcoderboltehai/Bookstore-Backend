using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bookstore.API.Services
{
    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare queues
                _channel.QueueDeclare(queue: "order_notifications", durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclare(queue: "book_notifications", durable: true, exclusive: false, autoDelete: false);

                _logger.LogInformation("RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }
        }

        public void PublishMessage(string queueName, string message)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                _logger.LogInformation("Message published to queue {QueueName}: {Message}", queueName, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to queue {QueueName}", queueName);
            }
        }

        public void PublishOrderNotification(int orderId, string userEmail)
        {
            var notification = new
            {
                OrderId = orderId,
                UserEmail = userEmail,
                Message = $"Order {orderId} has been created successfully",
                Timestamp = DateTime.UtcNow
            };

            var message = JsonSerializer.Serialize(notification);
            PublishMessage("order_notifications", message);
        }

        public void PublishBookAddedNotification(int bookId, string bookName)
        {
            var notification = new
            {
                BookId = bookId,
                BookName = bookName,
                Message = $"New book '{bookName}' has been added",
                Timestamp = DateTime.UtcNow
            };

            var message = JsonSerializer.Serialize(notification);
            PublishMessage("book_notifications", message);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
