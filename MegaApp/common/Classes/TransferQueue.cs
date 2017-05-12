using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Enums;
using MegaApp.Models;

namespace MegaApp.Classes
{
    public class TransferQueue : ObservableCollection<TransferObjectModel>
    {
        public TransferQueue()
        {
            Uploads = new ObservableCollection<TransferObjectModel>();
            Downloads = new ObservableCollection<TransferObjectModel>();

            Uploads.CollectionChanged += UploadsOnCollectionChanged;
            Downloads.CollectionChanged += DownloadsOnCollectionChanged;
        }

        /// <summary>
        /// Select and return all transfers in the queue.
        /// </summary>
        /// <returns>Download and upload transfers combined in one list.</returns>
        public IList<TransferObjectModel> SelectAll()
        {
            var result = new List<TransferObjectModel>(this.Downloads.Count + this.Uploads.Count);
            result.AddRange(this.Downloads);
            result.AddRange(this.Uploads);
            return result;
        }

        /// <summary>
        /// Clear the complete queue
        /// </summary>
        public void Clear()
        {
            this.Downloads.Clear();
            this.Uploads.Clear();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var transferObject = (TransferObjectModel) item;

                    switch (transferObject.Type)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            DownloadSort(transferObject);
                            break;
                        case MTransferType.TYPE_UPLOAD:
                            UploadSort(transferObject);
                            break;
                    }
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var transferObject = (TransferObjectModel)item;

                    switch (transferObject.Type)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            Downloads.Remove(transferObject);
                            break;
                        case MTransferType.TYPE_UPLOAD:
                            Uploads.Remove(transferObject);
                            break;
                    }
                }
            }
        }

        private void DownloadsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    ((TransferObjectModel)item).PropertyChanged += DownloadsOnPropertyChanged;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    ((TransferObjectModel)item).PropertyChanged -= DownloadsOnPropertyChanged;
                }
            }
        }

        private void UploadsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    ((TransferObjectModel)item).PropertyChanged += UploadsOnPropertyChanged;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    ((TransferObjectModel)item).PropertyChanged -= UploadsOnPropertyChanged;
                }
            }
        }
      
       

        private void UploadsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("Status")) return;

            UploadSort((TransferObjectModel) sender);
        }

        private void UploadSort(TransferObjectModel transferObject)
        {
            if (Uploads.Contains(transferObject))
                if (!Uploads.Remove(transferObject)) return;

            var inserted = false;

            for (var i = 0; i <= Uploads.Count - 1; i++)
            {
                if ((int)transferObject.Status <= (int)Uploads[i].Status)
                {
                    Uploads.Insert(i, transferObject);
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
                Uploads.Add(transferObject);
        }

        private void DownloadSort(TransferObjectModel transferObject)
        {
            if (Downloads.Contains(transferObject))
                if (!Downloads.Remove(transferObject)) return;

            var inserted = false;

            for (var i = 0; i <= Downloads.Count - 1; i++)
            {
                if ((int)transferObject.Status <= (int)Downloads[i].Status)
                {
                    Downloads.Insert(i, transferObject);
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
                Downloads.Add(transferObject);
        }

        private void DownloadsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("Status")) return;

            DownloadSort((TransferObjectModel)sender);
        }

        public ObservableCollection<TransferObjectModel> Uploads { get; private set; }

        public ObservableCollection<TransferObjectModel> Downloads { get; private set; }
    }
}
