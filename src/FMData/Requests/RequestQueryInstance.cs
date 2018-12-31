namespace FMData
{
    /// <summary>
    /// Middle layer class used for custom JsonConvert
    /// </summary>
    /// <typeparam name="TRequestType"></typeparam>
    public class RequestQueryInstance<TRequestType>
    {
        /// <summary>
        /// new instance 
        /// </summary>
        /// <param name="query">The underlying object containing query parameters.</param>
        /// <param name="omit">Flag indicating omit or not.</param>
        public RequestQueryInstance(TRequestType query, bool omit)
        {
            QueryInstance = query;
            Omit = omit;
        }

        /// <summary>
        /// The query object containing the query parameters.
        /// </summary>
        public TRequestType QueryInstance { get; set; }

        /// <summary>
        /// Boolean flag indicating if this instance should be entered as a find request FIND or OMIT.
        /// </summary>
        public bool Omit { get; set; } = false;
    }
}