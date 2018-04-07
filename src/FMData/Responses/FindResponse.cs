using System;
using System.Collections.Generic;

namespace FMData.Responses
{
    public class FindResponse : BaseDataResponse
    {
        public IEnumerable<Record> Data { get; set; }
    }
}