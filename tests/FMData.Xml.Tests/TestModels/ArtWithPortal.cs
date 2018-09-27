using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FMData.Xml.Tests.TestModels
{
    [DataContract(Name = "layout")]
    public class ArtWithPortal
    {
        [DataMember(Name = "alt-Title")]
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Style { get; set; }
        public int length { get; set; }

        [PortalData("artlocations", TablePrefixFieldNames = "artlocations")]
        public IEnumerable<Locations> Locations { get; set; }
    }
}