using System;
using System.Collections.Generic;
using System.Text;

namespace YouTrackPuller.Dto
{
    public sealed class Configuration
    {
        public string PermanentToken { get; set; }

        public IEnumerable<FeedConfiguration> Feeds { get; set; }
    }

    public sealed class FeedConfiguration
    {
        public IEnumerable<DayOfWeek> DaysOfWeek { get; set; }
        public string Query { get; set; }
        public string TargetUrl { get; set; }
    }
}
