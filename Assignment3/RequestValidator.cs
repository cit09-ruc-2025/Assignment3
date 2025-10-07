using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Assignment3
{
    public class RequestValidator
    {

        public Response ValidateRequest(Request request)
        {
            if (string.IsNullOrEmpty(request.Method?.Trim()))
            {
                return new Response { Status = "missing method", Success = false };

            }

            string[] validMethods = { "create", "read", "update", "delete", "echo" };

            if (!validMethods.Contains(request.Method.ToLower()))
            {
                return new Response { Status = "illegal method", Success = false };
            }

            if (request.Method != "echo" && string.IsNullOrEmpty(request.Path?.Trim()))
            {
                return new Response { Status = "missing path", Success = false };
            }

            if (string.IsNullOrEmpty(request.Date?.Trim()))
            {
                return new Response { Status = "missing date", Success = false };
            }

            if (!long.TryParse(request.Date, out long _))
            {
                return new Response { Status = "illegal date", Success = false };

            }

            string[] withBody = { "create", "update", "echo" };

            if (withBody.Contains(request.Method.ToLower()))
            {
                if (string.IsNullOrWhiteSpace(request.Body))
                {
                    return new Response { Status = "missing body", Success = false };

                }

                if (request.Method != "echo")
                {
                    try
                    {
                        JsonDocument.Parse(request.Body);
                        return new Response { Status = "1 Ok", Success = true };

                    }
                    catch (JsonException)
                    {
                        return new Response { Status = "illegal body", Success = false };
                    }
                }
            }
            return new Response { Status = "1 Ok", Success = true };
        }
    }
}
