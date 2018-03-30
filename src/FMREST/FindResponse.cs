namespace FMREST
{
    using System;
    using System.Collections.Generic;

    public class Record
    {
        public Dictionary<string,string> FieldData{get;set;}
        public Dictionary<string,string> PortalData{get;set;}
        public int RecordId {get;set;}
        public int ModId {get;set;}
    }

    public class FindResponse
    {
        public string ErrorCode {get;set;}
        public string Result {get;set;}
        public IEnumerable<Record> Data {get;set;}
    }
}