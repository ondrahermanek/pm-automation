using System;
using System.Net;
using System.Net.Sockets;
using FuncSharp;

namespace YouTrackPuller.Library
{
    public class HttpException : Exception
    {
        public HttpException(string message, HttpResponse response = null, WebExceptionStatus? webExceptionStatus = null, SocketError? socketError = null, Exception innerException = null)
            : base(message, innerException)
        {
            Response = response.ToOption();
            WebExceptionStatus = webExceptionStatus.ToOption();
            SocketError = socketError.ToOption();
        }

        public Option<HttpResponse> Response { get; }

        public Option<WebExceptionStatus> WebExceptionStatus { get; }

        public Option<SocketError> SocketError { get; set; }

        public object GetDetails()
        {
            return new
            {
                WebExceptionStatus = WebExceptionStatus.ToNullable(),
                SocketError = SocketError.ToNullable(),
                Response = Response.Map(r => new
                {
                    Code = r.Code,
                    Content = r.Value
                }).GetOrNull()
            };
        }
    }
}
