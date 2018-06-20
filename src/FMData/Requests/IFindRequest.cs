using System.Collections.Generic;

namespace FMData
{
    /// <summary>
    /// Find Request Interface
    /// </summary>
    /// <typeparam name="TRequestType">The type of object to find.</typeparam>
    public interface IFindRequest<TRequestType> : IFileMakerRequest
    {
        /// <summary>
        /// The Offset (number of records to skip)
        /// </summary>
        int Offset { get; set; }
        /// <summary>
        /// The Limit (number of records to return)
        /// </summary>
        int Limit { get; set; }

        /// <summary>
        /// The object to use as query parameters.
        /// </summary>
        IEnumerable<TRequestType> Query { get; set; }
        /// <summary>
        /// The sort options for this request.
        /// </summary>
        IEnumerable<ISort> Sort { get; set; }
    }
}