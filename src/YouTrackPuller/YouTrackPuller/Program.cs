using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FuncSharp;
using YouTrackPuller.Library;

namespace YouTrackPuller
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = args.FirstOrDefault() ?? "../..";
            var stream = ReadConfiguration(Path.Combine(path, "Configuration.json"));
            stream.Match(
                s => ParseConfiguration(s).Match(
                    c =>
                    {
                        var request = CreateRequest(c);
                        var response = HttpClient.Send(request);
                        var result = response.Map(r => r.Content.Map(c => c.FromJson<IEnumerable<Dto.YouTrackIssue>>().Get()).Get());
                        var messages = result.Map(issues => issues.Select(i => $"[[{i.Id}] - {i.Summary}](https://mews.myjetbrains.com/youtrack/issue/{i.Id})"));
                        foreach (var message in messages.Get())
                        {
                            var messageRequest = CreateMessageRequest(message);
                            var messageResponse = HttpClient.Send(messageRequest);

                        }
                    },
                    e => Console.WriteLine($"Failed to parse configuration:\n{e}")
                ),
                _ => Console.WriteLine("Sample config created.")
            );

            Console.ReadLine();
        }

        private static HttpRequest CreateMessageRequest(string message)
        {
            throw new NotImplementedException();
        }

        private static HttpRequest CreateRequest(Dto.Configuration configuration)
        {
            throw new NotImplementedException();
        }

        private static Option<FileStream> ReadConfiguration(string path)
        {
            if (File.Exists(path))
            {
                return File.Create(path).ToOption();
            }

            var sampleConfiguraton = new Dto.Configuration
            {
                PermanentToken = "sampleToken",
                Feeds = new[] { new Dto.FeedConfiguration { Query = "query", TargetUrl = "Url", DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday } } }
            };
            using (var stream = new StreamWriter(File.Create(path)))
            {
                stream.Write(sampleConfiguraton.ToJson().Get());
            }

            return Option.Empty;
        }

        private static Try<Dto.Configuration, Exception> ParseConfiguration(FileStream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd().FromJson<Dto.Configuration>();
            }
        }
    }
}
