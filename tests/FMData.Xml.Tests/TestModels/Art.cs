using System.ComponentModel.DataAnnotations.Schema;

namespace FMData.Xml.Tests.TestModels
{
    [Table("layout")]
    public class Art
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Style { get; set; }
        public int length { get; set; }
    }
}