using System;
using System.Threading.Tasks;
using Server;

namespace Assignment3
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int port = 5000;

            var server = new EchoServer(port);

            await server.Run();
        }
    }
}
