using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FMData.Xml.Tests.TestModels
{
    [DataContract(Name = "layout")]
    public class Art
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Style { get; set; }
        public int length { get; set; }
    }
}