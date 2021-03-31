using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Insert channel character: ");
            var letter = Console.ReadKey().KeyChar;
            Console.WriteLine();
            Console.WriteLine($"Inserted channel character: {letter}");





            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var channelName = letter + "_work_channel";
                channel.QueueDeclare(channelName, true, false, false, null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"Recieved message: {message}");

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(channelName, false, consumer);
                Console.WriteLine($" [*] Now Accepting messages");













                channelName = "control_channel";
                channel.QueueDeclare(channelName, true, false, false, null);

                var body = Encoding.UTF8.GetBytes(letter.ToString());

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: channelName,
                                     basicProperties: properties,
                                     body: body);
                Console.WriteLine($" [x] Sending body \"{letter}\" to \"{channelName}\"");







                Console.ReadKey();
            }
        }
    }
}
