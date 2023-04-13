using System.Collections.Generic;

namespace FMData
{
    /// <summary>
    /// FileMaker record with Dictionary as field data and portal data.
    /// </summary>
    public class ContainerDataForAttribute : System.Attribute
    {
        /// <summary>
        /// The name of the container field to load data from.
        /// </summary>
        public string ContainerField { get; set; }

        /// <summary>
        /// Constructor for Attribute
        /// </summary>
        /// <param name="containerField">The name of the container field to load data from.</param>
        public ContainerDataForAttribute(string containerField)
        {
            ContainerField = containerField;
        }
    }
}
