using System;
using System.IO;
using Newtonsoft.Json;

namespace FMData.Rest
{
    /// <summary>
    /// When including nulls, we write null as empty string (thanks FileMaker Data API.)
    /// </summary>
    sealed class NullJsonWriter : JsonTextWriter
    {
        public NullJsonWriter(TextWriter textWriter) : base(textWriter)
        {
        }

        public override void WriteNull()
        {
            this.WriteValue(string.Empty);
        }
    }
}
