using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FMData.Rest.Tests.TestModels
{
    [DataContract(Name = "Users")]
    public class AdditionalDataUser
    {
        [IgnoreDataMember] public int FileMakerRecordId { get; set; }
        [IgnoreDataMember] public int FileMakerModId { get; set; }
        [DataMember] public int Id { get; set; }
        [DataMember] public int? ForeignKeyId { get; set; }
        [DataMember] public string Name { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}