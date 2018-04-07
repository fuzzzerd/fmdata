using System.Collections.Generic;

namespace FMData.Responses
{
    /// <summary>
    /// FileMaker record with Dictionary<string,string> as field data and portal data.
    /// </summary>
    public class Record 
        : RecordBase<Dictionary<string, string>, Dictionary<string, string>>
    { }
}