using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Assignment3;

namespace Server;

public class EchoServer
{

  private TcpListener _server;

  public int Port { get; set; }

  public EchoServer(int port)
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
        response = handleRequest(request);
      }

      var responseJson = JsonSerializer.Serialize(response,
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

      byte[] responseBuffer = Encoding.UTF8.GetBytes(responseJson);

      await stream.WriteAsync(responseBuffer, 0, responseBuffer.Length);
      await stream.FlushAsync();
    }
  }


  private Response handleRequest(Request request)
  {
    switch (request.Method.ToLower())
    {
      case "echo":
        return new Response { Body = request.Body };

      case "read":
        {
          var urlParser = new UrlParser();
          var parsed = urlParser.ParseUrl(request.Path);
          if (!parsed)
          {
            return new Response { Status = "4 Bad Request" };

          }
          else
          {
            if (request.Path.Contains("categories"))
            {
              var categoryService = new CategoryService();

              if (urlParser.HasId)
              {

                var category = categoryService.GetCategory(int.Parse(urlParser.Id));

                if (category == null)
                {
                  return new Response
                  {
                    Status = "5 Not Found",
                  };
                }

                return new Response
                {
                  Status = "1 Ok",
                  Body = ConvertToJsonString(category)
                };
              }
              else
              {
                var categories = categoryService.GetCategories();
                return new Response
                {
                  Status = "1 Ok",
                  Body = ConvertToJsonString(categories)
                };
              }
            }
            return new Response { Body = string.Empty };
          }

        }

      case "create":
        {
          var urlParser = new UrlParser();
          var parsed = urlParser.ParseUrl(request.Path);

          if (urlParser.HasId || !parsed)
          {
            return new Response { Status = "4 Bad Request" };
          }
          else
          {
            var categoryService = new CategoryService();

            var categories = categoryService.GetCategories();

            var newId = categories.Max((c) => c.Id) + 1;

            var requestValue = JsonSerializer.Deserialize<Category>(request.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var created = categoryService.CreateCategory(newId, requestValue.Name);

            if (created)
            {
              var createdCategory = categoryService.GetCategories().Find(c => c.Name == requestValue.Name);

              return new Response { Status = "1 Ok", Body = ConvertToJsonString(createdCategory) };
            }

            return new Response { Status = "4 Bad Request" };


          }
        }

      case "update":
        {
          var urlParser = new UrlParser();
          var parsed = urlParser.ParseUrl(request.Path);

          if (!urlParser.HasId || !parsed)
          {
            return new Response { Status = "4 Bad Request" };
          }
          else
          {
            if (request.Path.Contains("categories"))
            {
              var categoryService = new CategoryService();

              var category = categoryService.GetCategory(int.Parse(urlParser.Id));

              if (category == null)
              {
                return new Response
                {
                  Status = "5 not found",
                };
              }

              var newValue = JsonSerializer.Deserialize<Category>(request.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

              var updated = categoryService.UpdateCategory(int.Parse(urlParser.Id), newValue.Name);

              if (updated)
              {
                var updateCategory = categoryService.GetCategory(int.Parse(urlParser.Id));
                return new Response
                {
                  Status = "3 updated",
                  Body = ConvertToJsonString(updateCategory)
                };

              }

            }
            return new Response { Body = string.Empty };
          }
        }

      case "delete":
        {
          var urlParser = new UrlParser();
          var parsed = urlParser.ParseUrl(request.Path);

          if (!urlParser.HasId || !parsed)
          {
            return new Response { Status = "4 Bad Request" };
          }
          else
          {
            return new Response { Body = string.Empty };
          }
        }


      default:
        return new Response { Body = string.Empty };
    }
  }

  private static string ConvertToJsonString<T>(T items)
  {
    return JsonSerializer.Serialize(items, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
  }

}