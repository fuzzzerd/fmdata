namespace FMData.Xml.Requests
{
    /// <summary>
    /// Create Request Wrapper
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    public class CreateRequest<T> : RequestBase, ICreateRequest<T>
    {
        /// <summary>
        /// The field data for the create request.
        /// </summary>
        public T Data { get; set; }
    }
}