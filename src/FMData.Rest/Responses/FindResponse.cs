using System;
using System.Collections.Generic;

namespace FMData.Rest.Responses
{
    public class FindResponse<TResponseType> : BaseResponse, IResponse, IFindResponse<TResponseType>
    {
        public FindResultType<TResponseType> Response { get; set; }
    }
}