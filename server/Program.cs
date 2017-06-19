using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reverse;

namespace server
{
    class GreeterImpl : Reverse.ReverseService.ReverseServiceBase
    {
        private List<IServerStreamWriter<ChatMessageFromServer>> clients;
        public GreeterImpl()
        {

            clients = new List<IServerStreamWriter<ChatMessageFromServer>>();
        }

        public override Task<ReverseReply> ReverseString(ReverseRequest request, ServerCallContext context)
        {

            Console.WriteLine($"Received from client {request.Data}");
            return Task.FromResult(new ReverseReply { Reversed = request.Data });
        }
        public override async Task chat(IAsyncStreamReader<ChatMessage> requestStream, IServerStreamWriter<ChatMessageFromServer> responseStream, ServerCallContext context)
        {
            clients.Add(responseStream);

            while (await requestStream.MoveNext())
            {
                var message = requestStream.Current;
                foreach (var client in clients)
                    await client.WriteAsync(new ChatMessageFromServer() { Message = message });
            }

        }
    }

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { ReverseService.BindService(new GreeterImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
