using System.Collections.Generic;

namespace FMData
{
    /// <summary>
    /// FIeld Metadata Instance
    /// </summary>
    public class FieldMetadata
    {
        /// <summary>
        /// Field Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Field Type.
        /// </summary>
        /// <example>normal</example>
        public string Type { get; set; }

        /// <summary>
        /// Field Display Type.
        /// </summary>
        /// <example>editText</example>
        public string DisplayType { get; set; }

        /// <summary>
        /// Field Value List.
        /// </summary>
        public string ValueList { get; set; }

        /// <summary>
        /// Is Field Global?
        /// </summary>
        public bool Global { get; set; }

        /// <summary>
        /// Is Field AutoEnter.
        /// </summary>
        public bool AutoEnter { get; set; }

        /// <summary>
        /// Is Field Four Digit Year?
        /// </summary>
        public bool FourDigitYear { get; set; }

        /// <summary>
        /// Field Maximum Repeat.
        /// </summary>
        public int MaxRepeat { get; set; }

        /// <summary>
        /// Field Maximum Length (characters).
        /// </summary>
        public int MaxCharacters { get; set; }

        /// <summary>
        /// Field NotEmpty
        /// </summary>
        public bool NotEmpty { get; set; }

        /// <summary>
        /// Is Numeric
        /// </summary>
        public bool Numeric { get; set; }

        /// <summary>
        /// Time Of day
        /// </summary>
        /// <value></value>
        public bool TimeOfDay { get; set; }

        /// <summary>
        /// Repetition Start
        /// </summary>
        public int RepetitionStart { get; set; }

        /// <summary>
        /// Repetition End
        /// </summary>
        public int RepetitionEnd { get; set; }
    }

    /// <summary>
    /// A Value List Item
    /// </summary>
    public class ValueListItem
    {
        /// <summary>
        /// Value to Display
        /// </summary>
        public string DisplayValue { get; set; }

        /// <summary>
        /// Value to be saved in field.
        /// </summary>
        public string Value { get; set; }
    }

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

    /// <summary>
    /// Layout Metadata
    /// </summary>
    public class LayoutMetadata
    {
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