using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FMData.Rest.Tests.TestModels
{
    [Table("Users")]
    public class ContainerFieldTestModel
    {
        public int FileMakerRecordId { get; set; }
        public int FileMakerModId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string SomeContainerField { get; set; }
        [ContainerDataFor("SomeContainerField")]
        public byte[] SomeContainerFieldData { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}