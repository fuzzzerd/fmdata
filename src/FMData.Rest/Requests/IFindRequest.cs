using System.Collections.Generic;

namespace FMData.Rest.Requests
{
    public interface IFindRequest<TRequestType>
    {
        int Offset { get; set; }
        IEnumerable<TRequestType> Query { get; set; }
        int Range { get; set; }
        IEnumerable<Sort> Sort { get; set; }

        string ToJson();
    }
}