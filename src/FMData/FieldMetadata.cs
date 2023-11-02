namespace FMData;

/// <summary>
/// Field Metadata Instance
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
    /// The data type for the field.
    /// </summary>
    /// <example>text</example>
    public string Result { get; set; }

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
