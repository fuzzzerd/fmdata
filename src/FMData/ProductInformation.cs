using System;

namespace FMData
{
    /// <summary>
    /// Information about the FileMaker Server
    /// </summary>
    public class ProductInformation
    {
        /// <summary>
        /// Product Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Date of Build
        /// </summary>
        public DateTime BuildDate { get; set; }

        /// <summary>
        /// FileMaker Server Version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Date Format String
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Time Format String
        /// </summary>
        public string TimeFormat { get; set; }

        /// <summary>
        /// FileMaker TimeStamp (aka DateTime, NOT SQL Timestamp) Format
        /// </summary>
        public string TimeStampFormat { get; set; }
    }
}