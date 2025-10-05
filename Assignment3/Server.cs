using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Assignment3.Models;
using System.Text.Json;
using Utils;
using Assignment3.Utils;
using System.Threading;

namespace Assignment3
{
    public class Server
    {
        private readonly int _port;
        private TcpListener _listener;
        private RequestValidator _validator = new();

        public Server(int port)
        {
            this._port = port;
        }

        public async Task Start()
        {
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();
            Console.WriteLine("Server started on port: {0}", _port);
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = HandleClient(client);
            }

        }

        private async Task HandleClient(TcpClient client)
        {
            var requestString = await client.ReadAsync();
            var request = ParseRequest(requestString);
            var validationResult = _validator.ValidateRequest(request);

            var message = string.Empty;

            if (validationResult.Status.Contains("missing path")
                || validationResult.Status.Contains("missing body")
                || validationResult.Status.Contains("illegal date")
                || validationResult.Status.Contains("illegal body")
                || validationResult.Status.Contains("illegal method")) message = validationResult.Status;
            else if (validationResult.Status.Contains("illegal path")) message = "5 Not found";
            else if (validationResult.Status.Contains("bad request")) message = "4 Bad Request";


            var response = new Response() { Status = message };
            await SendResponse(client, response.ToJson());

            client.Close();
        }

        private async Task SendResponse(TcpClient client, string message)
        {
            using var stream = client.GetStream();
            await stream.WriteAsync(Encoding.UTF8.GetBytes(message));
        }

        private Request ParseRequest(string requestString)
        {
            return JsonSerializer.Deserialize<Request>(requestString, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
