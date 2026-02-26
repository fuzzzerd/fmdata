namespace FMData
{
    /// <summary>
    /// Represents per-portal limit and offset parameters for a find request.
    /// </summary>
    public class PortalRequestData
    {
        /// <summary>
        /// The name of the portal (table occurrence).
        /// </summary>
        public string PortalName { get; set; }

        /// <summary>
        /// The maximum number of portal records to return.
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// The number of portal records to skip before returning results.
        /// </summary>
        public int? Offset { get; set; }
    }
}
