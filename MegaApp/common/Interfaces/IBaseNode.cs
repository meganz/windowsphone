using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Base signature for Node models in the MegaApp
    /// </summary>
    public interface IBaseNode
    {
        #region Properties

        /// <summary>
        /// The display name of the node
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The creation time is the time when the file was uploaded to the MEGA Cloud or
        /// when the file was saved for offline
        /// </summary>
        string CreationTime { get; }

        /// <summary>
        /// The modification time is the modification time of the original file
        /// </summary>
        string ModificationTime { get; }

        /// <summary>
        /// Returns the default location to load or save the thumbnail image for this node
        /// </summary>
        string ThumbnailPath { get; }

        /// <summary>
        /// Get the readable information about a node
        /// </summary>
        string Information { get; }        

        /// <summary>
        /// The size of the node in bytes
        /// </summary>
        ulong Size { get; set; }
        
        /// <summary>
        /// Indicates if the node is currently selected in a multi-select scenario
        /// Needed as path for the RadDatabounndListbox to auto select/deselect
        /// </summary>
        bool IsMultiSelected { get; set; }

        /// <summary>
        /// Returns if a node is a folder.        
        /// </summary>
        bool IsFolder { get; }

        /// <summary>
        /// Returns if a node is an image. Based on its file extension.
        /// Not 100% proof because file extensions can be wrong
        /// </summary>
        bool IsImage { get; }

        /// <summary>
        /// A true/false value if the current thumbnail of the node is the default thumbnail image
        /// for that file/folder type
        /// </summary>
        bool IsDefaultImage { get; set; }

        /// <summary>
        /// The uniform resource identifier of the current thumbnail for this node
        /// Could be a default file/folder type image or a thumbnail preview of the real picture
        /// </summary>
        Uri ThumbnailImageUri { get; set; }

        /// <summary>
        /// Vector data that represents the default image for a specific filetype / folder
        /// </summary>
        string DefaultImagePathData { get; set; }

        #endregion
    }
}
