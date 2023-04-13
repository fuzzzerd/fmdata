using System.Collections.Generic;

namespace FMData
{
    /// <summary>
    /// Value List
    /// </summary>
    public class ValueList
    {
        /// <summary>
        /// Value List Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value List Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Value List Items
        /// </summary>
        public IReadOnlyCollection<ValueListItem> Values { get; set; }
    }
}
