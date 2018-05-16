using System.Collections.Generic;

namespace FMData
{
    public interface IFindResponse<TResponseType>
    {
        FindResultType<TResponseType> Response { get; set; }
    }

    public class FindResultType<T>
    {
        public IEnumerable<RecordBase<T, Dictionary<string, string>>> Data { get; set; }
    }
}