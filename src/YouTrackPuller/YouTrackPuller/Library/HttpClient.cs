using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FuncSharp;

namespace YouTrackPuller.Library
{
    public static class HttpClient
    {
        public static Try<HttpResponse, HttpException> Send(HttpRequest request)
        {
            try
            {
                var webRequest = CreateWebRequest(request);
                var webResponse = webRequest.GetResponse();
                return CreateResponse(webResponse);
            }
            catch (Exception e) when (e is WebException || e is NotSupportedException || e is ProtocolViolationException || e is InvalidOperationException)
            {
                return Try.Error(
                    new HttpException(
                        message: "Request failed.",
                        webExceptionStatus: e.As<WebException>().Map(we => we.Status).ToNullable(),
                        socketError: e.InnerException.As<SocketException>().Map(s => s.SocketErrorCode).ToNullable(),
                        innerException: e
                    )
                );
            }
        }

        private static HttpWebRequest CreateWebRequest(HttpRequest request)
        {
            var webRequest = WebRequest.CreateHttp(request.Uri);
            webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            webRequest.ServicePoint.Expect100Continue = true;
            webRequest.Method = request.Method.Match(
                HttpMethod.Post, _ => "POST",
                HttpMethod.Get, _ => "GET"
            );

            request.BearerToken.Match(t => webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {t}"));
            request.Content.Match(content =>
            {
                var data = Encoding.UTF8.GetBytes(content);
                webRequest.ContentType = "application/json";

                using (var stream = new BufferedStream(webRequest.GetRequestStream()))
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
            });

            return webRequest;
        }

        private static Try<HttpResponse, HttpException> CreateResponse(WebResponse response)
        {
            var httpWebResponse = response.As<HttpWebResponse>().ToTry(_ => new HttpException("The response is not a HttpWebResponse."));

            return httpWebResponse.Map(r => new HttpResponse(r.StatusCode, ReadContent(r)));
        }

        private static string ReadContent(HttpWebResponse r)
        {
            using (var reader = new StreamReader(r.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
