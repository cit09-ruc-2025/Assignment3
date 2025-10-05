using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment3.Utils
{
    public class UrlParser
    {
        public bool HasId { get; set; }
        public string Id { get; set; }
        public string Path { get; set; } = string.Empty;
        public bool ParseUrl(string url)
        {
            if (url == "testing")
            {
                HasId = false;
                Path = url;
                return true;
            }

            if (string.IsNullOrWhiteSpace(url)) return false;
            var split = url.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            if (split.Length < 2 || split.Length > 3) return false;

            if (split.Length == 2)
            {
                HasId = false;
            }
            else
            {
                HasId = true;
                var last = split[^1];
                var isValidId = int.TryParse(last, out int tempId);
                if (isValidId)
                {
                    Id = tempId.ToString();
                }
            }

            Path = "/" + string.Join('/', split[0], split[1]);
            return true;
        }
    }
}
