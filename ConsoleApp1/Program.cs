using Grpc.Core;
using Reverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Threading;
using System.Reactive.Linq;
using System.Collections.ObjectModel;

namespace ConsoleApp1
{
    class Program
    {
        public static void Main(string[] args)
        {
            var id = "";
            var channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
            
            var client = new ReverseService.ReverseServiceClient(channel);

            var chat = client.chat();

            Task.Run(async () =>
            {
                while (await chat.ResponseStream.MoveNext())
                {
                    var msg = chat.ResponseStream.Current.Message;
                    Console.WriteLine($"{msg.From}: {msg.Message}");
                }
            });

            Console.Write("Set name: ");
            id = Console.ReadLine();

            while (true)
            {
                var msg = Console.ReadLine();

                if (msg == "bye")
                    break;

                chat.RequestStream.WriteAsync(new ChatMessage
                {
                    From = id,
                    Message = msg
                }).ContinueWith(t => { });
            }

            channel.ShutdownAsync().Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
