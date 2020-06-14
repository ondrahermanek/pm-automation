using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FuncSharp;
using YouTrackPuller.Dto;
using YouTrackPuller.Library;

namespace YouTrackPuller
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = args.FirstOrDefault() ?? "../../..";
            var configuration = ReadConfiguration($"{path}/configuration.json");
            configuration.Match(
                c => Run(c),
                e => Log("Failed to parse configuration", e)
            );

            Console.ReadLine();
        }

        private static void Run(Configuration configuration)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var dayOfWeek = now.DayOfWeek;
            var appliacableFeeds = configuration.Feeds.Where(f => f.DaysOfWeek.Contains(dayOfWeek)).ToList();

            foreach (var feed in appliacableFeeds)
            {
                var url = $"{configuration.BaseUrl}/api/issues?fields=idReadable,summary&top=20&query={feed.Query}";
                var request = new HttpRequest(HttpMethod.Get, new Uri(url), configuration.PermanentToken);
                var response = HttpClient.Send(request);
                response.Match(
                    r => r.Content.Match(
                        c => c.FromJson<IEnumerable<YouTrackIssue>>().Match(
                            issues => ReportIssues(configuration, feed, issues),
                            e => Log($"[{feed.Name}] Failed to parse issues", e)
                        ),
                        _ => Log($"[{feed.Name}] No issues fetched.")
                    ),
                    e => Log($"[{feed.Name}] Failed to fetch issues", e)
                );
            }
        }

        private static void ReportIssues(Configuration configuration, Dto.FeedConfiguration feed, IEnumerable<YouTrackIssue> issues)
        {
            foreach (var i in issues.Take(1))
            {
                var message = $"{feed.Name} [{i.Id}]({configuration.BaseUrl}/issue/{i.Id}) - {i.Summary}";
                var content = new
                {
                    Message = message,
                    Channel = feed.TargetChannel
                };
                var request = new HttpRequest(HttpMethod.Post, new Uri(feed.TargetUrl), content: content.ToJson().Get());
                HttpClient.Send(request).Match(
                    s => Log($"[{feed.Name}] Successfully reported {i.Id} issue."),
                    e => Log($"[{feed.Name}] Failed to report {i.Id} issue", e)
                );
            }
        }

        private static Try<Dto.Configuration, Exception> ReadConfiguration(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                var validContent = content.ToOption().Where(c => !String.IsNullOrEmpty(c)).ToTry(_ => new Exception("Empty configuration file."));
                var configuration = validContent.FlatMap(c => c.FromJson<Dto.Configuration>());
                return configuration;
            }

            var sampleConfiguraton = new Dto.Configuration
            {
                BaseUrl = "https://mews.myjetbrains.com/youtrack",
                PermanentToken = "sampleToken",
                Feeds = new[] { new Dto.FeedConfiguration { Name = "sample name & message prefix", Query = "url encoded YT query", TargetUrl = "Url", DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday } } }
            };
            using (var stream = new StreamWriter(File.Create(path)))
            {
                stream.Write(sampleConfiguraton.ToJson().Get());
            }

            return Try.Error(new Exception("Sample config created instead."));
        }

        private static Try<Dto.Configuration, Exception> ParseConfiguration(FileStream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd().ToOption().ToTry(_ => new Exception("Empty configuration file."));
                return content.FlatMap(c => c.FromJson<Dto.Configuration>());
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
        private static void Log(string message, Exception e)
        {
            var exceptionData = new
            {
                Message = e.Message,
                StackTrace = e.StackTrace
            };
            Log($"{message}:\n{exceptionData}");
        }
    }
}
