using System;
using FuncSharp;

namespace YouTrackPuller.Library
{
    public enum HttpMethod
    {
        Post,
        Get
    }

    public sealed class HttpRequest
    {
        public HttpRequest(HttpMethod method, Uri uri, string bearerToken = null, string content = null)
        {
            Method = method;
            Uri = uri;
            Content = content.ToOption();
            BearerToken = bearerToken.ToOption();
        }

        public HttpMethod Method { get; }
        public Uri Uri { get; }
        public Option<string> Content { get; }
        public Option<string> BearerToken { get; }
    }
}
