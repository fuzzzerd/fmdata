using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace FMData.Xml.Tests.TestModels
{
    [DataContract(Name = "Users")]
    public class User
    {
        [DataMember] public int Id { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public DateTime Created { get; set; }
        [DataMember] public DateTime Modified { get; set; }
    }
}
