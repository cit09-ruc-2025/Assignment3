using System;
using System.Threading.Tasks;
using Assignment3.Server;

namespace Assignment3
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int port = 5000;

            var server = new CJTPServer(port);

            await server.Run();
        }
    }
}
