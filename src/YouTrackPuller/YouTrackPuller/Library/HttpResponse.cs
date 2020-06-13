using System.Net;
using FuncSharp;

namespace YouTrackPuller.Library
{
    public sealed class HttpResponse
    {
        public HttpResponse(HttpStatusCode code, string content)
        {
            Code = code;
            Content = content.ToOption();
        }

        public HttpStatusCode Code { get; }

        public Option<string> Content { get; }

        public string Value
        {
            get { return Content.GetOrNull(); }
        }

        public override string ToString()
        {
            return $"{(int)Code} {Code} ({Value})";
        }
    }
}
