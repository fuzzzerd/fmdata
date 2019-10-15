using System.Collections.Generic;

namespace FMData
{
    /// <summary>
    /// Layout Metadata
    /// </summary>
    public class LayoutMetadata
    {
        /// <summary>
        /// Name of layout
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A collection of FieldMetadata instances for the fields on the layout.
        /// </summary>
        public IReadOnlyCollection<FieldMetadata> FieldMetaData { get; set; }

        /// <summary>
        /// A collection of Value Lists available on this layout.
        /// </summary>
        public IReadOnlyCollection<ValueList> ValueLists { get; set; }
    }
}