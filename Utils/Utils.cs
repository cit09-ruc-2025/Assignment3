using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Utils
{
    public static class Utils
    {
        public static async Task<string> ReadAsync(this TcpClient client)
        {
            await Task.Delay(1000);
            var stream = client.GetStream();

            byte[] buffer = new byte[1024];

            var readCount = await stream.ReadAsync(buffer);

            return Encoding.UTF8.GetString(buffer, 0, readCount);
        }

        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        }
    }
}
