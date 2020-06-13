using System.Runtime.Serialization;

namespace YouTrackPuller.Dto
{
    [DataContract]
    public sealed class YouTrackIssue
    {
        [DataMember(Name = "idReadable")]
        public string Id { get; set; }

        [DataMember(Name = "summary")]
        public string Summary { get; set; }
    }
}
