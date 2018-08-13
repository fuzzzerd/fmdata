using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FMData.Rest.Tests.TestModels
{
    [Table("Users")]
    public class User
    {
        public int FileMakerRecordId { get; set; }
        public int FileMakerModId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}