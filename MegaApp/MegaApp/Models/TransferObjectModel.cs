using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.MegaApi;

namespace MegaApp.Models
{
    public class TransferObjectModel: BaseSdkViewModel
    {
        public string DisplayName { get; set; }
        private readonly string _filePath;

        public TransferObjectModel(string name, string filePath, NodeViewModel parentNode, MegaSDK megaSdk)
            :base(megaSdk)
        {
            DisplayName = name;
            _filePath = filePath;

            MegaSdk.startUpload(filePath, parentNode.GetMegaNode(), new UploadTransferListener(this));
        }

        private bool _isNotTransferring;
        public bool IsNotTransferring
        {
            get { return _isNotTransferring; }
            set
            {
                _isNotTransferring = value;
                OnPropertyChanged("IsNotTransferring");
            }
        }

        private ulong _totalBytes;
        public ulong TotalBytes
        {
            get { return _totalBytes; }
            set
            {
                _totalBytes = value;
                OnPropertyChanged("TotalBytes");
            }
        }

        private ulong _transferedBytes;
        public ulong TransferedBytes
        {
            get { return _transferedBytes; }
            set
            {
                _transferedBytes = value;
                OnPropertyChanged("TransferedBytes");
            }
        }

    }
}
