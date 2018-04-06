using System;
using System.Collections.Generic;

namespace FMData.Responses
{
    public class FindResponse<T> : BaseDataResponse
    {
        public IEnumerable<RecordBase<T,Dictionary<string,string>>> Data { get; set; }
    }
}