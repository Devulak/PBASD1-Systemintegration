using System;
using System.Net.Http;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DeadLetter
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("DeadLetterExchange", ExchangeType.Direct, true);
            channel.QueueDeclare("input_deadLetters", true, false, false);
            channel.QueueBind("input_deadLetters", "DeadLetterExchange", "");

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, eventArgs) =>
            {
                var message = eventArgs.Body.ToString();
                var deathReasonBytes = eventArgs.BasicProperties.Headers["x-first-death-reason"] as byte[];
                var deathReason = System.Text.Encoding.UTF8.GetString(deathReasonBytes);

                Console.WriteLine($"Deadletter: {message}. Reason: {deathReason}");
            };


            channel.BasicConsume("input_deadLetters", true, consumer);

            Console.ReadLine();

            channel.Close();
            connection.Close();

        }
    }
}
