namespace FMData
{
    /// <summary>
    /// All supported FileMaker DataAPI endpoint versions.
    /// </summary>
    public enum RestTargetVersion
    {
        /// <summary>
        /// Uses v1 endpoint of the DataAPI.
        /// </summary>
        v1,
        /// <summary>
        /// Uses v2 endpoint of the DataAPI.
        /// </summary>
        v2,
        /// <summary>
        /// Uses the latest endpoint version of the DataAPI.
        /// </summary>
        vLatest
    }
}
