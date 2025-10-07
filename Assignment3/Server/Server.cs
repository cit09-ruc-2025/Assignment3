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
    public class CJTPServer
    {

        private TcpListener _server;

        private static readonly List<string> _validControllers = ["categories", "testing"];

        public int Port { get; set; }

        private ICategoryService _categoryService = new CategoryService();

        public CJTPServer(int port)
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

                var response = new Response();

                RequestValidator validator = new RequestValidator();

                response = validator.ValidateRequest(request);
                Console.WriteLine(response.Status + response.Success);
                if (response.Success)
                {

                    var requestValidController = false;

                    if (request.Method != "echo")
                    {
                        foreach (var controller in _validControllers)
                        {

                            if (request.Path.ToLower().Contains(controller.ToLower()))
                            {
                                requestValidController = true;
                                break;
                            }
                        }
                    }

                    if (!requestValidController && request.Method != "echo")
                    {
                        response = new Response { Status = "5 Not found" };
                    }
                    else
                    {
                        response = HandleRequest(request);
                    }

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
                    {
                        Console.WriteLine("here");

                        return new Response { Body = request.Body };
                    }

                case "read":
                    return HandleRead(request);

                case "create":
                    return HandleCreate(request);

                case "update":
                    return HandleUpdate(request);

                case "delete":
                    return HandleDelete(request);


                default:
                    return new Response();
            }
        }

        private Response HandleRead(Request request)
        {
            var urlParser = new UrlParser();
            urlParser.ParseUrl(request.Path);
            if (urlParser.HasId)
            {
                var category = _categoryService.GetCategory(int.Parse(urlParser.Id));
                if (category == null) return new Response() { Status = "5 Not found" };
                return new Response() { Status = "1 Ok", Body = ConvertToJsonString(_categoryService.GetCategory(int.Parse(urlParser.Id))) };
            }

            return new Response() { Status = "1 Ok", Body = ConvertToJsonString(_categoryService.GetCategories()) };
        }

        private Response HandleDelete(Request request)
        {
            var urlParser = new UrlParser();
            urlParser.ParseUrl(request.Path);
            if (!urlParser.HasId) return new Response() { Status = "4 Bad Request" };

            var id = int.Parse(urlParser.Id);
            var deleted = _categoryService.DeleteCategory(id);
            if (deleted) return new Response() { Status = "1 Ok" };
            else return new Response() { Status = "5 Not found" };
        }

        private Response HandleCreate(Request request)
        {
            var urlParser = new UrlParser();
            var parsed = urlParser.ParseUrl(request.Path);

            if (urlParser.HasId || !parsed)
            {
                return new Response { Status = "4 Bad Request" };
            }
            else
            {
                var categories = _categoryService.GetCategories();


                var requestValue = JsonSerializer.Deserialize<Category>(request.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (string.IsNullOrEmpty(requestValue.Name))
                {
                    return new Response { Status = "4 Bad Request" };
                }

                var created = _categoryService.CreateCategory(-1, requestValue.Name);

                if (created)
                {
                    var createdCategory = _categoryService.GetCategories().Find(c => c.Name == requestValue.Name);

                    return new Response { Status = "1 Ok", Body = ConvertToJsonString(createdCategory) };
                }

                return new Response { Status = "4 Bad Request" };

            }
        }

        private Response HandleUpdate(Request request)
        {
            var urlParser = new UrlParser();
            var parsed = urlParser.ParseUrl(request.Path);

            if (!urlParser.HasId || !parsed)
            {
                return new Response { Status = "4 Bad Request" };
            }

            if (request.Path.Contains("categories"))
            {
                var category = _categoryService.GetCategory(int.Parse(urlParser.Id));
                if (category == null)
                {
                    return new Response { Status = "5 Not Found" };
                }

                var newValue = JsonSerializer.Deserialize<Category>(
                    request.Body,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                var updated = _categoryService.UpdateCategory(int.Parse(urlParser.Id), newValue.Name);

                if (updated)
                {
                    var updatedCategory = _categoryService.GetCategory(int.Parse(urlParser.Id));
                    return new Response
                    {
                        Status = "3 Updated",
                        Body = ConvertToJsonString(updatedCategory)
                    };
                }
            }

            return new Response { Body = string.Empty };
        }

        private static string ConvertToJsonString<T>(T items)
        {
            return JsonSerializer.Serialize(items, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

    }
}
