namespace FMData
{
    /// <summary>
    /// Contains information about the response. Source Table, Layout, and Found Count information.
    /// </summary>
    public class DataInfoModel
    {
        /// <summary>
        /// Database the data came from.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Layout used for the response.
        /// </summary>
        public string Layout { get; set; }

        /// <summary>
        /// Table the data came from.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Total records in the table.
        /// </summary>
        public long TotalRecordCount { get; set; }

        /// <summary>
        /// Number of records matching the find request.
        /// </summary>
        public long FoundCount { get; set; }

        /// <summary>
        /// Number of records returned in the data portion of the response.
        /// </summary>
        public long ReturnedCount { get; set; }
    }
}
