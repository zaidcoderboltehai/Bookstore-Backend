namespace Bookstore.API.Services
{
    public interface IRabbitMQService
    {
        void PublishMessage(string queueName, string message);
        void PublishOrderNotification(int orderId, string userEmail);
        void PublishBookAddedNotification(int bookId, string bookName);
    }
}
