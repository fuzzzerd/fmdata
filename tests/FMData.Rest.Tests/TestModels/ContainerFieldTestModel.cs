using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace FMData.Rest.Tests.TestModels
{
    [DataContract(Name = "Users")]
    public class ContainerFieldTestModel
    {
        [IgnoreDataMember] public int FileMakerRecordId { get; set; }
        [IgnoreDataMember] public int FileMakerModId { get; set; }
        [DataMember(Name = "ID")] public int Id { get; set; }
        [DataMember(Name = "Name")] public string Name { get; set; }
        [DataMember(Name = "SomeContainerField")] public string SomeContainerField { get; set; }
        [ContainerDataFor("SomeContainerField")]
        public byte[] SomeContainerFieldData { get; set; }
        [DataMember(Name = "Created")] public DateTime Created { get; set; }
        [DataMember(Name = "Modified")] public DateTime Modified { get; set; }
    }
}
