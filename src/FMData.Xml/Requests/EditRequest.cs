using System;
using System.Linq;

namespace FMData.Xml.Requests
{
    /// <summary>
    /// Create Request Wrapper
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    public class EditRequest<T> : RequestBase, IEditRequest<T>
    {
        /// <summary>
        /// The field data for the create request.
        /// </summary>
        public T Data { get; set; }
        /// <summary>
        /// Modification Id
        /// </summary>
        public int ModId { get; set; }
        /// <summary>
        /// RecordId
        /// </summary>
        public int RecordId { get; set; }

        /// <summary>
        /// Serialize the request to the format needed for filemaker to accept it.
        /// </summary>
        /// <returns>String representation of the request.</returns>
        public override string SerializeRequest()
        {
            var dictionary = Data.AsDictionary(IncludeNullValuesInSerializedOutput);
            var stringContent = string.Join("", dictionary.Select(i => $"&{Uri.EscapeDataString(i.Key)}={Uri.EscapeDataString(i.Value.ToString())}"));
            var requestContent = $"-edit&-lay={Layout}{stringContent}";
            return requestContent;
        }
    }
}
