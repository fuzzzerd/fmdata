namespace FMData.Rest.Responses
{
    /// <summary>
    /// Find Response
    /// </summary>
    /// <typeparam name="TResponseType">The type found from response layout.</typeparam>
    public class FindResponse<TResponseType> : BaseResponse, IResponse, IFindResponse<TResponseType>
    {
        /// <summary>
        /// Response Object
        /// </summary>
        public FindResultType<TResponseType> Response { get; set; }
    }
}