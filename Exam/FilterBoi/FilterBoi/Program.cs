using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers.Binary;

namespace FilterBoi
{
    class Program
    {
        private static int numToAdd;

        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                Console.WriteLine("What channel are we consuming on?");
                string consumeChannel = Console.ReadLine();
                Console.WriteLine("What channel are we publishing on?");
                string publishChannel = Console.ReadLine();

                Random rnd = new();
                numToAdd = rnd.Next(0, 50);
                Console.WriteLine("Rolling the dice... " + numToAdd + "!");

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                // Input channel
                channel.QueueDeclare(consumeChannel, true, false, false);

                EventingBasicConsumer consumer = new(channel);
                consumer.Received += (sender, ea) =>
                {
                    int bodyAsNumber = BinaryPrimitives.ReadInt32LittleEndian(ea.Body.Span);
                    Console.WriteLine(" [x] Received {0}", bodyAsNumber);
                    Console.WriteLine("Adding {0} to number... {1}, {2}!", numToAdd, bodyAsNumber, bodyAsNumber += numToAdd);
                    Console.WriteLine(" [x] Publishing to {0}", publishChannel);

                    byte[] body = BitConverter.GetBytes(bodyAsNumber);

                    channel.BasicPublish(exchange: ea.Exchange,
                                         routingKey: publishChannel,
                                         basicProperties: ea.BasicProperties,
                                         body: body);

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(consumeChannel, false, consumer);

                Console.WriteLine(" [*] Waiting for messages.");
                Console.ReadLine();
            }
        }
    }
}
