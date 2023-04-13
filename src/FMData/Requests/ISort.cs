namespace FMData
{
    /// <summary>
    /// Sort Interface
    /// </summary>
    public interface ISort
    {
        /// <summary>
        /// Sort Field
        /// </summary>
        string FieldName { get; set; }
        /// <summary>
        /// Sort Order (ascend/descend)
        /// </summary>
        string SortOrder { get; set; }
    }
}
