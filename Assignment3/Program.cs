using System;
using System.Threading.Tasks;

namespace Assignment3
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new Server(5000);
            await server.Start();
        }
    }
}
