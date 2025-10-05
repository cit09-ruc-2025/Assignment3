using Assignment3.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Assignment3.Utils
{
    public class RequestValidator
    {
        private static List<string> _validMethods = new List<string>()
            {
                "create",
                "read",
                "update",
                "delete",
                "echo"
            };
        public Response ValidateRequest(Request request)
        {
            var errorList = new List<string>();
            ValidateMethod(request, ref errorList);
            ValidatePath(request, ref errorList);
            ValidateDate(request, ref errorList);
            ValidateBody(request, ref errorList);
            CheckForInvalidRequests(request, ref errorList);

            var response = new Response();
            response.Status = errorList.Count > 0 ? "4 " + string.Join(", ", errorList) : "1 Ok"; // Append Bad Request status code if there are errors
            return response;
        }

        #region Method Validation
        private void ValidateMethod(Request request, ref List<string> errorList)
        {
            var methodMissing = string.IsNullOrWhiteSpace(request.Method);
            var methodMissingMessage = "missing method";
            if (methodMissing) errorList.Add(methodMissingMessage);

            var methodInvalid = !_validMethods.Contains(request.Method);
            var methodInvalidMessage = "illegal method";
            if (methodInvalid) errorList.Add(methodInvalidMessage);
        }


        #endregion

        #region Path Validation
        private void ValidatePath(Request request, ref List<string> errorList)
        {
            var urlParser = new UrlParser();
            var validControllers = new List<string>() { "categories", "testing" };
            if (string.IsNullOrWhiteSpace(request.Path)) { errorList.Add("missing path"); return; }
            if (!urlParser.ParseUrl(request.Path) || !validControllers.Any(c => request.Path.ToLower().Contains(c.ToLower())) && request.Method != "echo") errorList.Add("illegal path");
            if (urlParser.HasId && urlParser.Id == null) errorList.Add("invalid path id");
            if (request.Method == "create" && urlParser.HasId) errorList.Add("invalid create request");
        }
        #endregion

        #region Date Validation
        private void ValidateDate(Request request, ref List<string> errorList)
        {
            if (string.IsNullOrWhiteSpace(request.Date))
            {
                errorList.Add("missing date");
                return;
            }

            var parseSuccess = long.TryParse(request.Date, out long _);
            if (!parseSuccess) errorList.Add("illegal date");
        }
        #endregion

        #region Body Validation
        private void ValidateBody(Request request, ref List<string> errorList)
        {
            var requiresBody = new List<string>() { "update", "create", "echo" };
            if (!requiresBody.Contains(request.Method)) return;

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                errorList.Add("missing body");
                return;
            }

            try
            {
                var parse = JsonSerializer.Deserialize<object>(request.Body);
            }
            catch (Exception ex)
            {
                if (request.Method != "echo") errorList.Add("illegal body");
            }
        }


        #endregion

        private void CheckForInvalidRequests(Request request, ref List<string> errorList)
        {
            var urlParser = new UrlParser();
            urlParser.ParseUrl(request.Path);
            var isBadRequest = (request.Method == "read" && urlParser.HasId && urlParser.Id == null 
                || request.Method == "create" && urlParser.HasId)
                || (request.Method == "delete" && !urlParser.HasId) 
                || (request.Method == "update" && !urlParser.HasId);
            if (isBadRequest) errorList.Add("bad request");
        }
    }
}
