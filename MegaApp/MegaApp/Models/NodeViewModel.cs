using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using mega;

namespace MegaApp.Models
{
    /// <summary>
    /// ViewModel of the main MEGA datatype (MNode)
    /// </summary>
    class NodeViewModel : BaseViewModel
    {
        // Original MNode object from the MEGA SDK
        private readonly MNode _baseNode;
        // Offset DateTime value to calculate the correct creation and modification time
        private static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseNode">Original MEGA node</param>
        public NodeViewModel(MNode baseNode)
        {
            _baseNode = baseNode;
            this.Name = baseNode.getName();
            this.Size = baseNode.getSize();
        }

        #region Methods

        /// <summary>
        /// Convert the size to a readable string
        /// </summary>
        /// <param name="size">size in bytes</param>
        /// <returns>The size in readable string with correct appendix</returns>
        private static string ConvertSizeToString(ulong size)
        {
            double result = size/1024;

            return result > 1024 ? String.Format("{0:F1} MB", (result/1024)) : String.Format("{0:F1} KB", result);
        }

        /// <summary>
        /// Convert the MEGA time to a C# DateTime object in local time
        /// </summary>
        /// <param name="time">MEGA time</param>
        /// <returns>DateTime object in local time</returns>
        private static DateTime ConvertDateToString(ulong time)
        {
            return OriginalDateTime.AddSeconds(time).ToLocalTime();
        }

        #endregion

        #region Properties

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private ulong _size;
        public ulong Size
        {
            get { return _size; }
            set
            {
                _size = value;
                OnPropertyChanged("Size");
            }
        }
        
        public MNodeType Type
        {
            get { return _baseNode.getType(); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
    
        public string CreationTime
        {
            get { return ConvertDateToString(_baseNode.getCreationTime()).ToShortDateString(); }
        }

        public string ModificationTime
        {
            get { return ConvertDateToString(_baseNode.getModificationTime()).ToShortDateString(); }
        }

        public string DisplaySize
        {
            get { return ConvertSizeToString(Size); }
        }

        #endregion

        
    }
}
