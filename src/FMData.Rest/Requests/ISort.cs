namespace FMData.Rest.Requests
{
    public interface ISort
    {
        string FieldName { get; set; }
        string SortOrder { get; set; }
    }
}