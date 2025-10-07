using System;
using System.Linq;

namespace Assignment3
{
    public class UrlParser
    {
        public bool HasId { get; set; }
        public int Id { get; set; }
        public string Path { get; set; }
        public bool ParseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            // splitting by '/'
            var split = url.Trim('/').Split('/');

            // at least "api/resource" is required
            if (split.Length < 2) return false;

            // check if last segment is an ID
            if (int.TryParse(split[^1], out int tempId))
            {
                HasId = true;
                Id = tempId;
                Path = string.Join("/", split.Take(split.Length - 1));
            }
            else
            {
                HasId = false;
                Path = string.Join("/", split);
            }

            return true;
        }
    }
}
