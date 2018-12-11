using System;
using System.Collections.Generic;
using System.Linq;

namespace FMData.Xml.Requests
{
    /// <summary>
    /// Find request for an instance T.
    /// </summary>
    /// <typeparam name="T">The type to use for the find request parameters.</typeparam>
    public class FindRequest<T> : IFindRequest<T>
    {
        /// <summary>
        /// The layout to execute the request on.
        /// </summary>
        public string Layout { get; set; }
        /// <summary>
        /// The query values to provide to FMS.
        /// </summary>
        public IEnumerable<T> Query { get; set; }
        /// <summary>
        /// Offset amount (skip)
        /// </summary>
        public int Offset { get; set; }
        /// <summary>
        /// Limit amount (take)
        /// </summary>
        public int Limit { get; set; }
        /// <summary>
        /// Sort options for the results.
        /// </summary>
        public IEnumerable<ISort> Sort { get; set; }
        /// <summary>
        /// The layout to utilize for the response projection.
        /// </summary>
        /// <value></value>
        public string ResponseLayout { get; set; }
        /// <summary>
        /// Script to run.
        /// </summary>
        public string Script { get; set; }
        /// <summary>
        /// Script Parameter.
        /// </summary>
        public string ScriptParameter { get; set; }
        /// <summary>
        /// Script to run.
        /// </summary>
        public string PreRequestScript { get; set; }
        /// <summary>
        /// Script Parameter.
        /// </summary>
        public string PreRequestScriptParameter { get; set; }
        /// <summary>
        /// Script to run.
        /// </summary>
        public string PreSortScript { get; set; }
        /// <summary>
        /// Script Parameter.
        /// </summary>
        public string PreSortScriptParameter { get; set; }

        /// <summary>
        /// Determines if container data attributes are processed and loaded.
        /// </summary>
        public bool LoadContainerData { get; set; }

        /// <summary>
        /// Serialize the request. 
        /// </summary>
        /// <returns>The string representation for this request to be sent along the wire to FMS.</returns>
        public string SerializeRequest()
        {
            var dictionary = Query.First().AsDictionary(false);
            var stringContent = string.Join("", dictionary.Select(i => $"&{Uri.EscapeDataString(i.Key)}={Uri.EscapeDataString(i.Value.ToString())}"));
            var requestContent = $"-find&-lay={Layout}{stringContent}";
            return requestContent;
        }
    }
}