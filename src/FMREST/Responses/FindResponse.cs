using System;
using System.Collections.Generic;

namespace FMREST.Responses
{
    public class FindResponse : BaseDataResponse
    {
        public IEnumerable<Record> Data { get; set; }
    }
}