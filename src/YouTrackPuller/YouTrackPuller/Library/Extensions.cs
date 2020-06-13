using System;
using FuncSharp;
using Newtonsoft.Json;

namespace YouTrackPuller.Library
{
    public static class Extensions
    {
        public static Try<T, Exception> FromJson<T>(this string s)
        {
            return Try.Create(_ => JsonConvert.DeserializeObject<T>(s));
        }
        public static Try<string, Exception> ToJson(this object obj)
        {
            return Try.Create(_ => JsonConvert.SerializeObject(obj));
        }
    }
}
