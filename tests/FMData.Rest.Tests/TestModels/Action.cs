using System.Runtime.Serialization;

namespace FMData.Rest.Tests.TestModels
{
    [DataContract(Name ="actionLayout")]
    public class Action
    {
        [DataMember(Name ="ID")]
        public int ID { get; set; }
        [DataMember(Name = "Action")]
        public string ActionName { get; set; }
    }
}