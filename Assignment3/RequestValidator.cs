using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Assignment3
{
    public class RequestValidator
    {
        public Response ValidateRequest(Request request)
        {
            var list = new List<string>();
            ValidateMethod(request, ref list);
            ValidatePath(request, ref list);
            ValidateDate(request, ref list);
            ValidateBody(request, ref list);

            var response = new Response();
            response.Status = list.Count > 0 ? "4 " + string.Join(", ", list) : "1 Ok";
            return response;
        }

        private void ValidateMethod(Request request, ref List<string> errorList)
        {
            var methodMissing = string.IsNullOrWhiteSpace(request.Method);
            if (methodMissing)
            {
                errorList.Add("missing method");
                return;
            }

            var validMethods = new List<string>
            {
                "create",
                "read",
                "update",
                "delete",
                "echo"
            };

            if (!validMethods.Contains(request.Method.ToLower()))
                errorList.Add("illegal method");
        }

        private void ValidatePath(Request request, ref List<string> errorList)
        {
            if (string.IsNullOrWhiteSpace(request.Path))
            {
                errorList.Add("missing path");
                return;
            }

            if (!(new UrlParser()).ParseUrl(request.Path))
                errorList.Add("illegal path");
        }

        private void ValidateDate(Request request, ref List<string> errorList)
        {
            if (string.IsNullOrWhiteSpace(request.Date))
            {
                errorList.Add("missing date");
                return;
            }

            if (!long.TryParse(request.Date, out long timestamp) || timestamp <= 0)
                errorList.Add("illegal date");
        }

        private void ValidateBody(Request request, ref List<string> errorList)
        {
            if (string.IsNullOrWhiteSpace(request.Method))
                return;

            string method = request.Method.ToLower();
            var requiresBody = new List<string> { "update", "create", "echo" };

            if (!requiresBody.Contains(method))
                return;

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                errorList.Add("missing body");
                return;
            }

            // using echo to allow plain text 
            if (method == "echo")
                return;

            try
            {
                var parse = JsonSerializer.Deserialize<object>(request.Body);
            }
            catch
            {
                errorList.Add("illegal body");
            }
        }
    }
}
