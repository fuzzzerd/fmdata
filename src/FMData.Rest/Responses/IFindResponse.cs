using System.Collections.Generic;

namespace FMData.Rest.Responses
{
    public interface IFindResponse<TResponseType>
    {
        IEnumerable<RecordBase<TResponseType, Dictionary<string, string>>> Data { get; set; }
    }
}