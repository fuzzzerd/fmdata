using System;
using System.Runtime.Serialization;

namespace FMData.Xml.Tests.TestModels
{
    [DataContract(Name = "locations")]
    public class Locations
    {
        public string Location{ get; set; }
        public DateTime Date { get; set; }
    }
}