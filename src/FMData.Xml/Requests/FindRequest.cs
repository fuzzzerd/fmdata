﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FMData.Xml.Requests
{
    /// <summary>
    /// Find request for an instance T.
    /// </summary>
    /// <typeparam name="T">The type to use for the find request parameters.</typeparam>
    public class FindRequest<T> : RequestBase, IFindRequest<T>
    {
        /// <summary>
        /// The query values to provide to FMS.
        /// </summary>
        public IEnumerable<RequestQueryInstance<T>> Query { get { return _query; } }
        private readonly List<RequestQueryInstance<T>> _query = new List<RequestQueryInstance<T>>();

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
        public ICollection<ISort> Sort { get; set; }

        /// <summary>
        /// Determines if container data attributes are processed and loaded.
        /// </summary>
        public bool LoadContainerData { get; set; }

        /// <summary>
        /// Serialize the request. 
        /// </summary>
        /// <returns>The string representation for this request to be sent along the wire to FMS.</returns>
        public override string SerializeRequest()
        {
            var dictionary = Query.First().QueryInstance.AsDictionary(IncludeNullValuesInSerializedOutput);
            var stringContent = string.Join("", dictionary.Select(i => $"&{Uri.EscapeDataString(i.Key)}={Uri.EscapeDataString(i.Value.ToString())}"));
            var requestContent = $"-find&-lay={Layout}{stringContent}";
            return requestContent;
        }

        /// <summary>
        /// Add an instance to the query collection.
        /// </summary>
        /// <param name="query">The object to add to the query.</param>
        /// <param name="omit">Flag indicating if this instance represents a find or an omit.</param>
        public void AddQuery(T query, bool omit = false) => _query.Add(new RequestQueryInstance<T>(query, omit));

        /// <summary>
        /// Adds a sort field with a direction to the sort collection.
        /// </summary>
        /// <param name="fieldName">The field to sort by.</param>
        /// <param name="sortDirection">The direction to sort.</param>
        public void AddSort(string fieldName, string sortDirection)
        {
            throw new NotImplementedException();
        }
    }
}
