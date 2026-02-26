using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FMData.Rest.Requests
{
    /// <summary>
    /// The object that contains the parameters to serialize for a find request.
    /// </summary>
    public partial class FindRequest<TRequestType> : RequestBase, IFindRequest<TRequestType>
    {
        /// <summary>
        /// The find request dictionary.
        /// </summary>
        [JsonProperty("query")]
        public IEnumerable<RequestQueryInstance<TRequestType>> Query { get { return _query; } }

        private readonly List<RequestQueryInstance<TRequestType>> _query = new List<RequestQueryInstance<TRequestType>>();

        /// <summary>
        /// Maximum number of records to return for this request.
        /// </summary>
        [JsonProperty("limit")]
        public int Limit { get; set; } = 100; // default

        /// <summary>
        /// The number of records to skip before returning records for this request.
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; set; } = 1; // default

        /// <summary>
        /// The sort fields and directions for this request.
        /// </summary>
        [JsonProperty("sort")]
        public ICollection<ISort> Sort { get; set; }

        /// <summary>
        /// Determines if container data attributes are processed and loaded.
        /// </summary>
        public bool LoadContainerData { get; set; }

        /// <summary>
        /// The portal configurations for this request.
        /// </summary>
        [JsonIgnore]
        public ICollection<PortalRequestData> Portals { get; set; }

        /// <summary>
        /// Extension data dictionary used to inject portal dot-notation keys
        /// (e.g., "portal", "limit.PortalName", "offset.PortalName") into the
        /// serialized JSON output.
        /// </summary>
        [JsonExtensionData]
        private IDictionary<string, JToken> _portalExtensionData;

        /// <summary>
        /// Configure a portal's limit and/or offset parameters.
        /// If the portal has already been configured, updates its values.
        /// </summary>
        /// <param name="portalName">The name of the portal (table occurrence).</param>
        /// <param name="limit">The maximum number of portal records to return.</param>
        /// <param name="offset">The number of portal records to skip.</param>
        public void ConfigurePortal(string portalName, int? limit = null, int? offset = null)
        {
            if (Portals == null)
            {
                Portals = new List<PortalRequestData>();
            }

            var existing = Portals.FirstOrDefault(p => p.PortalName == portalName);
            if (existing != null)
            {
                if (limit.HasValue) existing.Limit = limit;
                if (offset.HasValue) existing.Offset = offset;
            }
            else
            {
                Portals.Add(new PortalRequestData
                {
                    PortalName = portalName,
                    Limit = limit,
                    Offset = offset
                });
            }
        }

        /// <summary>
        /// Create a find request from Json
        /// </summary>
        /// <param name="json">The incoming Json data to deserialize.</param>
        /// <returns>An instance of the FindRequest object from the provided Json string.</returns>
        public static FindRequest<T> FromJson<T>(string json) => JsonConvert.DeserializeObject<FindRequest<T>>(json);

        /// <summary>
        /// JSON Convert the current object to a string for passing out to the API.
        /// </summary>
        /// <returns></returns>
        public override string SerializeRequest()
        {
            PopulatePortalExtensionData();

            return JsonConvert.SerializeObject(this,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = IncludeNullValuesInSerializedOutput ? NullValueHandling.Include : NullValueHandling.Ignore,
                    DefaultValueHandling = IncludeDefaultValuesInSerializedOutput ? DefaultValueHandling.Include : DefaultValueHandling.Ignore,
                    Converters =
                    {
                        new FormatNumbersAsTextConverter(),
                        new RequestQueryInstanceConverter<TRequestType>(this)
                    }
                });
        }

        /// <summary>
        /// Populates the extension data dictionary from the Portals collection
        /// so that dot-notation keys appear as top-level JSON properties.
        /// </summary>
        private void PopulatePortalExtensionData()
        {
            _portalExtensionData = null;

            if (Portals == null || Portals.Count == 0)
            {
                return;
            }

            _portalExtensionData = new Dictionary<string, JToken>();

            var portalNames = Portals.Select(p => p.PortalName).ToList();
            _portalExtensionData["portal"] = JToken.FromObject(portalNames);

            foreach (var portal in Portals)
            {
                if (portal.Limit.HasValue)
                {
                    _portalExtensionData[$"limit.{portal.PortalName}"] = portal.Limit.Value;
                }
                if (portal.Offset.HasValue)
                {
                    _portalExtensionData[$"offset.{portal.PortalName}"] = portal.Offset.Value;
                }
            }
        }

        /// <summary>
        /// Add an instance to the query collection.
        /// </summary>
        /// <param name="query">The object to add to the query.</param>
        /// <param name="omit">Flag indicating if this instance represents a find or an omit.</param>
        public void AddQuery(TRequestType query, bool omit = false) => _query.Add(new RequestQueryInstance<TRequestType>(query, omit));

        /// <summary>
        /// Adds a sort field with a direction to the sort collection.
        /// </summary>
        /// <param name="fieldName">The field to sort by.</param>
        /// <param name="sortDirection">The direction to sort.</param>
        public void AddSort(string fieldName, string sortDirection)
        {
            if (Sort == null)
            {
                Sort = new List<ISort>();
            }
            Sort.Add(new Sort { SortOrder = sortDirection, FieldName = fieldName });
        }
    }
}
