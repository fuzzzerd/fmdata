using System.Collections.Generic;

namespace FMData
{
    public interface IFindRequest<TRequestType> : IFileMakerRequest
    {
        int Offset { get; set; }
        int Limit { get; set; }

        IEnumerable<TRequestType> Query { get; set; }
        IEnumerable<ISort> Sort { get; set; }
    }
}