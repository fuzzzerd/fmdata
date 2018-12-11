using System;
using System.Runtime.Serialization;

namespace FMData.Rest.Tests.TestModels
{
    [DataContract(Name = "Users")]
    public class User
    {
        [IgnoreDataMember]public int FileMakerRecordId { get; set; }
        [IgnoreDataMember]public int FileMakerModId { get; set; }
        [DataMember]public int Id { get; set; }
        [DataMember]public string Name { get; set; }
        [DataMember]public DateTime Created { get; set; }
        [DataMember]public DateTime Modified { get; set; }
    }
}