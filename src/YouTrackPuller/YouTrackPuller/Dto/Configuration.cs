using System;
using System.Collections.Generic;

namespace YouTrackPuller.Dto
{
    public sealed class Configuration
    {
        public string BaseUrl { get; set; }

        public string PermanentToken { get; set; }

        public IEnumerable<FeedConfiguration> Feeds { get; set; }
    }

    public sealed class FeedConfiguration
    {
        public string Name { get; set; }
        public string Query { get; set; }
        public string TargetUrl { get; set; }
        public string TargetChannel { get; set; }
        public IEnumerable<DayOfWeek> DaysOfWeek { get; set; }
    }
}
