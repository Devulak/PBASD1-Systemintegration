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
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                string queue;
                do
                {
                    Console.WriteLine("What channel are we publishing on?");
                    queue = Console.ReadLine();
                } while (string.IsNullOrEmpty(queue));

                var arguments = new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange","DeadLetterExchange" }
                };

                channel.QueueDeclare(queue: queue,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: arguments);

                while (true)
                {
                    int number;
                    bool? gotNumber = null;
                    do
                    {
                        if (gotNumber != null)
                        {
                            Console.WriteLine("That's not a number!");
                        }
                        Console.WriteLine("What number should we start with?");
                        string numberAsString = Console.ReadLine();
                        gotNumber = int.TryParse(numberAsString, out number);
                    } while (!gotNumber.HasValue || gotNumber != true);

                    byte[] body = BitConverter.GetBytes(number);

                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                                         routingKey: queue,
                                         basicProperties: properties,
                                         body: body);
                    Console.WriteLine(" [x] Sent to channel {0} with body: {1}", queue, number);
                }
            }
        }
    }
}
