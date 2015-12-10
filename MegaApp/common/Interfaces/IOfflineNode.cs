using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Signature for OfflineNode models in the MegaApp
    /// </summary>
    public interface IOfflineNode : IBaseNode
    {
        #region Public Methods

        /// <summary>
        /// Delete the node from offline
        /// </summary>        
        Task DeleteAsync();
        
        /// <summary>
        /// Load node thumbnail if available on disk
        /// </summary>
        //void SetThumbnailImage();

        /// <summary>
        /// Open the file that is represented by this node
        /// </summary>
        void Open();

        #endregion

        #region Properties

        ObservableCollection<IOfflineNode> ParentCollection { get; set; }

        ObservableCollection<IOfflineNode> ChildCollection { get; set; }

        /// <summary>
        /// The display path of the node
        /// </summary>
        string NodePath { get; set; }

        /// <summary>
        /// Unique identifier of the node
        /// </summary>
        String Base64Handle { get; set; }

        #endregion
    }
}
