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
        IEnumerable<RequestQueryInstance<TRequestType>> Query { get; }

        /// <summary>
        /// The sort options for this request.
        /// </summary>
        ICollection<ISort> Sort { get; set; }

        /// <summary>
        /// Determines if container data attributes are processed and loaded.
        /// </summary>
        bool LoadContainerData { get; set; }

        /// <summary>
        /// Add query data to the find request.
        /// </summary>
        void AddQuery(TRequestType query, bool omit = false);

        /// <summary>
        /// Adds a sort field with a direction to the sort collection.
        /// </summary>
        /// <param name="fieldName">The field to sort by.</param>
        /// <param name="sortDirection">The direction to sort.</param>
        void AddSort(string fieldName, string sortDirection);
    }
}
