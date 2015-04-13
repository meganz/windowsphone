using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Signature for MegaNode models in the MegaApp
    /// </summary>
    public interface IMegaNode
    {
        #region Public Methods

        /// <summary>
        /// Rename the current Node
        /// </summary>
        /// <returns>Result of the action</returns>
        Task<NodeActionResult> Rename();

        /// <summary>
        /// Move the node from its current location to a new folder destionation
        /// </summary>
        /// <param name="newParentNode">The root node of the destionation folder</param>
        /// <returns>Result of the action</returns>
        NodeActionResult Move(IMegaNode newParentNode);

        /// <summary>
        /// Remove the node from the cloud drive to the rubbish bin
        /// </summary>
        /// <param name="isMultiRemove">True if the node is in a multi-select scenario</param>
        /// <param name="waitEventRequest"></param>
        /// <returns>Result of the action</returns>
        NodeActionResult Remove(bool isMultiRemove, AutoResetEvent waitEventRequest = null);

        /// <summary>
        /// Delete the node permanently
        /// </summary>
        /// <returns>Result of the action</returns>
        NodeActionResult Delete();

        /// <summary>
        /// Get the node link from the Mega SDK to share the node with others 
        /// </summary>
        /// <returns>Result of the action</returns>
        NodeActionResult GetLink();

        /// <summary>
        /// Dowload the node to the specified download destionation
        /// </summary>
        /// <param name="transferQueu">Global app transfer queu to add the download to</param>
        /// <param name="downloadPath">Download destionation location</param>
        void Download(TransferQueu transferQueu, string downloadPath = null);

        /// <summary>
        /// Update core date associated with the SDK MNode object
        /// </summary>
        /// <param name="megaNode"></param>
        void Update(MNode megaNode);

        /// <summary>
        /// Load node thumbnail if available on disk. If not availble download it with the Mega SDK
        /// </summary>
        void SetThumbnailImage();

        /// <summary>
        /// Open the file that is represented by this node
        /// </summary>
        void Open();
      
        #endregion

        #region Properties

        /// <summary>
        /// The display name of the node
        /// </summary>
        string Name { get; set; }
      
        /// <summary>
        /// The creation time is the time when the file was uploaded to the MEGA Cloud
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
        /// Unique identifier of the node
        /// </summary>
        ulong Handle { get; set; }

        /// <summary>
        /// The size of the node in bytes
        /// </summary>
        ulong Size { get; }

        ObservableCollection<IMegaNode> ParentCollection { get; set; }

        ObservableCollection<IMegaNode> ChildCollection { get; set; }

        /// <summary>
        /// Specifies the node type TYPE_UNKNOWN = -1, TYPE_FILE = 0, TYPE_FOLDER = 1, TYPE_ROOT = 2, TYPE_INCOMING = 3, 
        /// TYPE_RUBBISH = 4, TYPE_MAIL = 5
        /// </summary>
        MNodeType Type { get; }

        /// <summary>
        /// Indicates how the node should be drawn on the screen
        /// </summary>
        NodeDisplayMode DisplayMode { get; set; }

        /// <summary>
        /// Indicates if the node is currently selected in a multi-select scenario
        /// Needed as path for the RadDatabounndListbox to auto select/deselect
        /// </summary>
        bool IsMultiSelected { get; set; }

        /// <summary>
        /// Returns if a node is an image. Based on its file extension.
        /// Not 100% proof because file extensions can be wrong
        /// </summary>
        bool IsImage { get; }

        /// <summary>
        /// A true/false value if the current thumbnail of the node is the default thumbnail image
        /// for that file/folder type
        /// </summary>
        bool IsThumbnailDefaultImage { get; set; }

        /// <summary>
        /// The uniform resource identifier of the current thumbnail for this node
        /// Could be a default file/folder type image or a thumbnail preview of the real picture
        /// </summary>
        Uri ThumbnailImageUri { get; set; }

        /// <summary>
        /// The TransferObjectModel that controls upload and download transfers of this node
        /// </summary>
        TransferObjectModel Transfer { get; set; }

        /// <summary>
        /// The original MNode from the Mega SDK that is the base for all app nodes 
        /// and used in as input/output in different SDK methods and functions
        /// </summary>
        MNode OriginalMNode { get; }

        #endregion
    }
}
