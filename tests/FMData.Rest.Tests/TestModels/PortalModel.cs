using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FMData.Rest.Tests.TestModels
{
    [DataContract(Name = "the-layout")]
    public class PortalModel
    {
        public string ES_ONE { get; set; }
        public int ES_TWO { get; set; }
        public int ES_UTCDTO { get; set; }

        [PortalData("action", TablePrefixFieldNames = "action")]
        public IEnumerable<Action> Actions { get; set; }
    }
}
