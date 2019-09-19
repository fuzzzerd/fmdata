using System;
using System.Linq;

namespace FMData.Xml.Requests
{
    /// <summary>
    /// Create Request Wrapper
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    public class CreateRequest<T> : RequestBase, ICreateRequest<T>
    {
        /// <summary>
        /// The field data for the create request.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Serialize this request to the FileMaker data format.
        /// </summary>
        /// <returns>String for the post data to FMS.</returns>
        public override string SerializeRequest()
        {
            var layout = Layout;
            var dictionary = Data.AsDictionary(IncludeNullValuesInSerializedOutput);
            var stringContent = string.Join("", dictionary.Select(i => $"&{Uri.EscapeDataString(i.Key)}={Uri.EscapeDataString(i.Value.ToString())}"));
            var requestContent = $"-new&-lay={layout}{stringContent}";
            return requestContent;
        }
    }
}