using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment3
{
    public class RequestValidator
    {
        public Response ValidateRequest(Request request)
        {
            var list = new List<string>();

            var methodMissing = string.IsNullOrWhiteSpace(request.Method);
            var methodMissingMessage = "missing method";
            if (methodMissing) list.Add(methodMissingMessage);

            var validMethods = new List<string>()
            {
                "get",
                "post",
                "update",
                "delete",
            };
            var methodInvalid = !validMethods.Contains(request.Method);
            var methodInvalidMessage = "illegal method";
            if (methodInvalid) list.Add(methodInvalidMessage);



            return new Response() { Status = string.Join(", ", list) };
        }

    }
}
