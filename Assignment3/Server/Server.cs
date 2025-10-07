using Assignment3.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Assignment3.Server
{
    public class Server
    {

        private TcpListener _server;

        public int Port { get; set; }

        private ICategoryService _categoryService = new CategoryService();

        public Server(int port)
        {
            this.Port = port;
        }

        public async Task Run()
        {
            _server = new TcpListener(IPAddress.Loopback, Port);

            _server.Start();

            Console.WriteLine($"Server started on port {Port}");

            while (true)
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClient(client));
            }
        }

        private async Task HandleClient(TcpClient client)
        {

            using (client)
            {
                var stream = client.GetStream();

                byte[] buffer = new byte[1024];

                var memoryStream = new MemoryStream();

                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await memoryStream.WriteAsync(buffer, 0, bytesRead);
                    if (bytesRead < buffer.Length) break;
                }

                string requestJson = Encoding.UTF8.GetString(memoryStream.ToArray()).Trim();

                Request request = JsonSerializer.Deserialize<Request>(requestJson,
                  new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                RequestValidator validator = new RequestValidator();

                Response response = validator.ValidateRequest(request);

                if (response.Success)
                {
                    response = HandleRequest(request);
                }

                var responseJson = JsonSerializer.Serialize(response,
                  new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                byte[] responseBuffer = Encoding.UTF8.GetBytes(responseJson);

                await stream.WriteAsync(responseBuffer, 0, responseBuffer.Length);
                await stream.FlushAsync();
            }
        }


        private Response HandleRequest(Request request)
        {
            switch (request.Method.ToLower())
            {
                case "echo":
                    return new Response();

                case "read":
                    return new Response();

                case "create":
                    return new Response();

                case "update":
                    return new Response();

                case "delete":
                    return new Response();


                default:
                    return new Response();
            }
        }

        private static string ConvertToJsonString<T>(T items)
        {
            return JsonSerializer.Serialize(items, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

    }
}
