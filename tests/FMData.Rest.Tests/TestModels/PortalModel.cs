using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FMData.Rest.Tests.TestModels
{
    [DataContract(Name ="the-layout")]
    public class PortalModel
    {
        public string ES_ONE { get; set; }
        public int ES_TWO { get; set; }
        public int ES_UTCDTO { get; set; }

        [PortalData("action", TablePrefixFieldNames = "action")]
        public IEnumerable<Action> Actions { get; set; }
    }

    [DataContract(Name ="actionLayout")]
    public class Action
    {
        [DataMember(Name ="ID")]
        public int ID { get; set; }
        [DataMember(Name = "Action")]
        public string ActionName { get; set; }
    }
}