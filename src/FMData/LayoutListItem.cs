using System.Collections.Generic;

namespace FMData
{
    /// <summary>
    /// Class used to track nested items
    /// </summary>
    public class LayoutListItem
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is Folder
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        public IReadOnlyCollection<LayoutListItem> FolderLayoutNames { get; set; }
    }
}
