using System.Collections.Generic;

namespace FMData
{
    /// <summary>
    /// Used to help map portal instances to properties on a model.
    /// </summary>
    public class PortalDataAttribute : System.Attribute
    { 
        /// <summary>
        /// The name of the container field to load data from.
        /// </summary>
        public string NamedPortalInstance {get;set;}

        /// <summary>
        /// Constructor for Attribute
        /// </summary>
        /// <param name="namedPortal">The name of the portal on the layout.</param>
        public PortalDataAttribute(string namedPortal)
        {
            NamedPortalInstance = namedPortal;
        }
    }
}