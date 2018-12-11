using System.Runtime.Serialization;

namespace FMData.Xml.Tests.TestModels
{
    [DataContract(Name ="layout")]
    public class ArtDataCM
    {
        [DataMember(Name ="alt-Title")]
        public string Title { get; set; }
        [DataMember] public string Artist { get; set; }
        [DataMember] public string Style { get; set; }
        [DataMember] public int length { get; set; }
    }
}