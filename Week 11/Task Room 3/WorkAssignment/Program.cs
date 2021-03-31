using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkAssignment
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var arguments = new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange","DeadLetterExchange" }
                };

                channel.QueueDeclare(queue: "input_channel",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: arguments);


                while (true)
                {
                    Console.WriteLine("Which letter to sent to?");
                    var letter = Console.ReadKey().KeyChar;
                    Console.WriteLine();

                    Console.WriteLine("Message to sent?");
                    var message = Console.ReadLine();
                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.Headers = new Dictionary<string, object>
                    {
                        { "alphabet", letter.ToString() }
                    };

                    channel.BasicPublish(exchange: "",
                                         routingKey: "input_channel",
                                         basicProperties: properties,
                                         body: body);
                    Console.WriteLine(" [x] Sent {0} with header of alphabet: {1}", message, letter);
                }
            }
        }
    }
}
