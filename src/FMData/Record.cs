using System.Collections.Generic;

namespace FMData
{
    /// <summary>
    /// FileMaker record with Dictionary as field data and portal data.
    /// </summary>
    public class Record 
        : RecordBase<Dictionary<string, string>, Dictionary<string, string>>
    { }
}