using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Enums;
using MegaApp.Interfaces;

namespace MegaApp.Classes
{
    /// <summary>
    /// Class to provide easy access to useful information of links.
    /// </summary>
    public class LinkInformation
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        public LinkInformation()
        {
            this.SelectedNodes = new List<IMegaNode>();
            this.Reset();
        }

        /// <summary>
        /// Method to reset all the class properties to the default values.
        /// </summary>
        public void Reset()
        {
            this.ActiveLink = null;
            this.UriLink = UriLinkType.None;
            this.LinkAction = LinkAction.None;
            this.PublicNode = null;            
            this.DownloadPath = null;
            this.SelectedNodes.Clear();

            this.HasFetchedNodesFolderLink = false;
        }

        #region Properties

        /// <summary>
        /// Link which is being processed or will be processed by the app.
        /// </summary>
        public String ActiveLink { get; set; }

        /// <summary>
        /// Type of the current link.
        /// </summary>
        public UriLinkType UriLink { get; set; }

        /// <summary>
        /// Operation to realize with the current active link.
        /// </summary>
        public LinkAction LinkAction { get; set; }

        /// <summary>
        /// Node obtained from a file link.
        /// </summary>
        public MNode PublicNode { get; set; }

        /// <summary>
        /// Selected nodes to process from a folder link.
        /// </summary>
        public List<IMegaNode> SelectedNodes { get; set; }

        /// <summary>
        /// The download path for the selected nodes in case of download operation.
        /// </summary>
        public String DownloadPath { get; set; }

        /// <summary>
        /// Indicates if the app has already fetched nodes of the folder link.
        /// </summary>
        public bool HasFetchedNodesFolderLink { get; set; }

        #endregion
    }
}
