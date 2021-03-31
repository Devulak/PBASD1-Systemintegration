using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DynamicRouter
{
    class Program
    {
        private static readonly HashSet<char> channelRouting = new();

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var arguments = new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange","DeadLetterExchange" }
                };

                // Input channel
                string channelName = "input_channel";
                channel.QueueDeclare(channelName, true, false, false, arguments);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    var letter = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["alphabet"])[0];
                    Console.WriteLine(" [x] Received {0}", letter);

                    if (channelRouting.Contains(letter))
                    {
                        var channelToForward = $"{letter}_work_channel";
                        Console.WriteLine($" [x] Forwarding to {channelToForward}");
                        channel.BasicPublish(exchange: ea.Exchange,
                                             routingKey: channelToForward,
                                             basicProperties: ea.BasicProperties,
                                             body: ea.Body);
                    }
                    else
                    {
                        channel.BasicReject(ea.DeliveryTag, false);
                    }

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(channelName, false, consumer);





                // Control channel
                channelName = "control_channel";
                channel.QueueDeclare(channelName, true, false, false, null);

                consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var character = message[0];

                    Console.WriteLine($" [x] Control recieved: {character}");

                    channelRouting.Add(character);

                    foreach (char item in channelRouting)
                    {
                        Console.WriteLine($"     Allowed channel routing: {item}");
                    }

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(channelName, false, consumer);






                Console.WriteLine(" [*] Waiting for messages.");
                Console.ReadLine();
            }
        }
    }
}
