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
            var errorList = new List<string>();
            var success = true;

            if (string.IsNullOrEmpty(request.Method?.Trim()))
            {
                errorList.Add("missing method");
                success = false;

            }

            string[] validMethods = { "create", "read", "update", "delete", "echo" };

            if (!validMethods.Contains(request.Method?.ToLower()))
            {
                errorList.Add("illegal method");
                success = false;
            }

            if (request.Method != "echo" && string.IsNullOrEmpty(request.Path?.Trim()))
            {
                errorList.Add("missing path");
                success = false;
            }

            if (string.IsNullOrEmpty(request.Date?.Trim()))
            {
                errorList.Add("missing date");
                success = false;
            }

            if (!long.TryParse(request.Date, out long _))
            {
                errorList.Add("illegal date");
                success = false;

            }

            string[] withBody = { "create", "update", "echo" };

            if (withBody.Contains(request.Method?.ToLower()))
            {
                if (string.IsNullOrWhiteSpace(request.Body))
                {
                    errorList.Add("missing body");
                    success = false;

                }

                if (request.Method != "echo")
                {
                    try
                    {
                        JsonDocument.Parse(request.Body);

                    }
                    catch (Exception)
                    {
                        errorList.Add("illegal body");
                        success = false;
                    }
                }
            }

            if (success) return new Response { Status = "1 Ok", Success = true };

            return new Response { Status = "4 " + string.Join("; ", errorList), Success = false };
        }
    }
}
