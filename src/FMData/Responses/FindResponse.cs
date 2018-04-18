using System;
using System.Collections.Generic;

namespace FMData.Responses
{
    public class FindResponse<TResponseType> : BaseDataResponse
    {
        public IEnumerable<RecordBase<TResponseType, Dictionary<string,string>>> Data { get; set; }
    }
}