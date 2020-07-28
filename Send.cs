using System;
using RabbitMQ.Client;
using System.Text;

namespace Send
{
    class Program
    {
        static void Main(string[] args)
        {
      Console.WriteLine("krece 1");
      ConnectionFactory1 fact1 = new ConnectionFactory1();
      Console.WriteLine("krece 2");
      IConnectionFactory factory = new ConnectionFactory();
              factory.Uri = new Uri("amqp://mlzjkbio:BZNeb36FxAlyA0SRjzeXzKp89z1KwxCN@roedeer.rmq.cloudamqp.com/mlzjkbio");
              using (var connection = factory.CreateConnection())
              using (var channel = connection.CreateModel())
              {
                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "test_exchange",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
              }

              Console.WriteLine(" Press [enter] to exit.");
              Console.ReadLine();
    }
    }
}
