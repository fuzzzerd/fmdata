using System;
using System.Runtime.Serialization;

namespace FMData.Xml.Tests.TestModels
{
    [DataContract(Name = "locations")]
    public class Locations
    {
        [DataMember] public string Location{ get; set; }
        [DataMember]  public DateTime Date { get; set; }
    }
}