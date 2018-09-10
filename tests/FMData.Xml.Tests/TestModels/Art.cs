using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace FMData.Xml.Tests.TestModels
{
    [Table("layout")]
    public class Art
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Style { get; set; }
        public int length { get; set; }
    }

    [DataContract(Name ="layout")]
    public class ArtDataCM
    {
        [DataMember(Name ="alt-Title")]
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Style { get; set; }
        public int length { get; set; }
    }

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

    [DataContract(Name = "locations")]
    public class Locations
    {
        public string Location{ get; set; }
        public DateTime Date { get; set; }
    }
}