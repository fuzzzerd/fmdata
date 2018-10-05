using System.Runtime.Serialization;

namespace FMData.Rest.Tests.TestModels
{
    [DataContract(Name ="Somelayout")]
    public class ModelWithLayout
    {
        public string Name { get; set; }
        public string AnotherField { get; set; }
    }
}