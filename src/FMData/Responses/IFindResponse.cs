using System.Collections.Generic;

namespace FMData
{
    /// <summary>
    /// Find Response Interface
    /// </summary>
    /// <typeparam name="TResponseType"></typeparam>
    public interface IFindResponse<TResponseType>
    {
        /// <summary>
        /// The Response main response
        /// </summary>
        FindResultType<TResponseType> Response { get; set; }
    }

    /// <summary>
    /// Find Result Response Type
    /// Needed to wrap the response in a 'Response' object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FindResultType<T> : ActionResponse
    {
        /// <summary>
        /// The data contained in the response.
        /// </summary>
        public IEnumerable<RecordBase<T, Dictionary<string, IEnumerable<Dictionary<string, string>>>>> Data { get; set; }
    }
}
