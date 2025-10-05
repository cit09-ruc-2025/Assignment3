using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Utils.Models;
using System.Text.Json;
using Utils;
using Assignment3.Utils;
using Assignment3.Interfaces;
using System.Threading;

namespace Assignment3
{
    public class Server
    {
        private readonly int _port;
        private TcpListener _listener;
        private static readonly List<string> _validControllers = ["categories", "testing"];
        private static readonly ICategoryService _categoryService = new CategoryService();


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
            var validator = new RequestValidator();
            var requestString = await client.ReadAsync();
            var request = ParseRequest(requestString);
            var validationResult = validator.ValidateRequest(request);
            var requestValidController = false;
            foreach (var controller in _validControllers)
            {

                if (validator.UrlParser.Path.ToLower().Contains(controller.ToLower()))
                {
                    requestValidController = true;
                    break;
                }
            }

            var message = string.Empty;

            // Check for errors
            if (validationResult.Status.Contains("missing path")) message = validationResult.Status;
            else if (!requestValidController && request.Method != "echo") message = "5 Not found";
            else if (validationResult.Status.Contains("missing body")
                || validationResult.Status.Contains("illegal date")
                || validationResult.Status.Contains("illegal body")
                || validationResult.Status.Contains("illegal method")) message = validationResult.Status;
            else if (validationResult.Status.Contains("bad request")) message = "4 Bad Request";

            var response = new Response() { Status = message };
            if (string.IsNullOrWhiteSpace(message) || request.Method == "echo") // No errors found
            {
                var processedResponse = ProcessRequest(request, validator);
                response.Status = processedResponse.Status ?? response.Status;
                response.Body = processedResponse.Body;
            }

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

        private (string Status, string Body) ProcessRequest(Request request, RequestValidator validator)
        {
            switch (request.Method.ToLower())
            {
                case "echo":
                    return (null, request.Body);
                case "read":
                    return HandleRead(request, validator);
                case "update":
                    return HandleUpdate(request, validator);
                case "delete":
                    return HandleDelete(request, validator);
                case "create":
                    return HandleCreate(request, validator);
                default:
                    return (string.Empty, string.Empty);
            }
        }

        private (string, string) HandleRead(Request request, RequestValidator validator)
        {
            if (validator.UrlParser.HasId)
            {
                var category = _categoryService.GetCategory(int.Parse(validator.UrlParser.Id));
                if (category == null) return ("5 Not found", string.Empty);
                return ("1 Ok", _categoryService.GetCategory(int.Parse(validator.UrlParser.Id)).ToJson());
            }

            return ("1 Ok", _categoryService.GetCategories().ToJson());
        }

        private (string, string) HandleUpdate(Request request, RequestValidator validator)
        {
            if (validator.UrlParser.HasId)
            {
                var id = int.Parse(validator.UrlParser.Id);
                var requestCategory = JsonSerializer.Deserialize<Category>(request.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                var category = _categoryService.UpdateCategory(id, requestCategory.Name);
                var updatedCategory = _categoryService.GetCategory(id);
                if (category) return ("3 Updated", updatedCategory.ToJson());
            }

            return ("5 Not found", string.Empty);
        }

        private (string, string) HandleCreate(Request request, RequestValidator validator)
        {
            var requestCategory = JsonSerializer.Deserialize<Category>(request.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var createdSuccesfully = _categoryService.CreateCategory(-1, requestCategory.Name);
            if (createdSuccesfully)
            {
                var createdCategory = _categoryService.GetCategories().FirstOrDefault(c => c.Name == requestCategory.Name);
                return ("2 Created", createdCategory.ToJson());
            }

            return ("4 Bad Request", string.Empty);
        }

        private (string, string) HandleDelete(Request request, RequestValidator validator)
        {
            if (validator.UrlParser.HasId)
            {
                var id = int.Parse(validator.UrlParser.Id);
                var deleted = _categoryService.DeleteCategory(id);
                if (deleted) return ("1 Ok", string.Empty);
            }
            return ("5 Not found", string.Empty);
        }

    }
}
