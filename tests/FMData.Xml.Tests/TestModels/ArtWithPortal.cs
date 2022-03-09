using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FMData.Xml.Tests.TestModels
{
    [DataContract(Name = "layout")]
    public class ArtWithPortal
    {
        [DataMember(Name = "alt-Title")]
        public string Title { get; set; }
        [DataMember]
        public string Artist { get; set; }
        [DataMember]
        public string Style { get; set; }
        [DataMember]
        public int Length { get; set; }

        [PortalData("artlocations", TablePrefixFieldNames = "artlocations")]
        public IEnumerable<Locations> Locations { get; set; }
    }
}
