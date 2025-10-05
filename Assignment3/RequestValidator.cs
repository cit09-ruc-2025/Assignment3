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
                return new Response { Status = "missing method" };

            }

            string[] validMethods = { "create", "read", "update", "delete", "echo" };

            if (!validMethods.Contains(request.Method.ToLower()))
            {
                return new Response { Status = "illegal method" };
            }

            if (string.IsNullOrEmpty(request.Path?.Trim()))
            {
                return new Response { Status = "missing path" };
            }

            if (string.IsNullOrEmpty(request.Date?.Trim()))
            {
                return new Response { Status = "missing date" };
            }

            if (!long.TryParse(request.Date, out long _))
            {
                return new Response { Status = "illegal date" };

            }

            string[] withBody = { "create", "update", "echo" };

            if (withBody.Contains(request.Method) && string.IsNullOrWhiteSpace(request.Body))
            {
                return new Response { Status = "missing body" };
            }


            return new Response { Status = "1 Ok" };
        }


    }

}
