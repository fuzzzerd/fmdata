using System;
using System.Collections.Generic;
using System.Text;

namespace FMData
{
    public interface IResponse
    {
        string ErrorCode { get; set; }

        string Result { get; set; }
    }
}