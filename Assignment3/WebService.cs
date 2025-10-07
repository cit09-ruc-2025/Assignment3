using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Assignment3
{
    public class WebService
    {
        private TcpListener _listener;
        private readonly CategoryService _categoryService;

        public WebService()
        {
            _categoryService = new CategoryService();
        }

        // Start the server
        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, 50004);
            _listener.Start();
            Console.WriteLine("Server listening on port 50004...");

            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }

        // Handle each client
        private void HandleClient(object clientObj)
        {
            var client = (TcpClient)clientObj;
            NetworkStream stream = client.GetStream();

            try
            {
                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) return;

                string requestString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received request: {requestString}");

                // Validate request
                var validator = new RequestValidator();
                var request = JsonSerializer.Deserialize<Request>(requestString);

                if (request == null || string.IsNullOrEmpty(request.Method) || string.IsNullOrEmpty(request.Path))
                {
                    SendResponse(stream, 4, "Bad Request: Invalid JSON or missing fields");
                    return;
                }


                // Parse the path
                var parser = new UrlParser();
                if (!parser.ParseUrl(request.Path))
                {
                    SendResponse(stream, 4, "Bad Request: Invalid Path");
                    return;
                }

                // Handle CRUD or echo
                HandleRequest(stream, request, parser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        // Process the request
        private void HandleRequest(NetworkStream stream, Request request, UrlParser parser)
        {
            switch (request.Method.ToLower())
            {
                case "read":
                    if (parser.HasId)
                    {
                        var category = _categoryService.GetCategory(parser.Id);
                        if (category == null)
                            SendResponse(stream, 5, "Not Found");
                        else
                            SendResponse(stream, 1, JsonSerializer.Serialize(category));
                    }
                    else
                    {
                        var categories = _categoryService.GetCategories();
                        SendResponse(stream, 1, JsonSerializer.Serialize(categories));
                    }
                    break;

                case "create":
                    if (parser.HasId)
                    {
                        SendResponse(stream, 4, "Bad Request: ID should not be in path");
                        return;
                    }
                    var newCat = JsonSerializer.Deserialize<Category>(request.Body);
                    var created = _categoryService.CreateCategory(newCat.name);
                    SendResponse(stream, 2, JsonSerializer.Serialize(created));
                    break;

                case "update":
                    if (!parser.HasId)
                    {
                        SendResponse(stream, 4, "Bad Request: ID missing");
                        return;
                    }
                    var updateCat = JsonSerializer.Deserialize<Category>(request.Body);
                    bool updated = _categoryService.UpdateCategory(parser.Id, updateCat.name);
                    SendResponse(stream, updated ? 3 : 5, updated ? "Updated" : "Not Found");
                    break;

                case "delete":
                    if (!parser.HasId)
                    {
                        SendResponse(stream, 4, "Bad Request: ID missing");
                        return;
                    }
                    bool deleted = _categoryService.DeleteCategory(parser.Id);
                    SendResponse(stream, deleted ? 1 : 5, deleted ? "Ok" : "Not Found");
                    break;

                case "echo":
                    SendResponse(stream, 1, request.Body);
                    break;

                default:
                    SendResponse(stream, 4, "Bad Request: Unknown Method");
                    break;
            }
        }

        // Send CJTP-compliant JSON response
        private void SendResponse(NetworkStream stream, int statusCode, string body)
        {
            var response = JsonSerializer.Serialize(new
            {
                status = statusCode,
                body = body
            });

            byte[] data = Encoding.UTF8.GetBytes(response);
            stream.Write(data, 0, data.Length);
        }
    }
}
