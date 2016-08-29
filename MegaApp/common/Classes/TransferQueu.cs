using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Enums;
using MegaApp.Models;

namespace MegaApp.Classes
{
    public class TransferQueu: ObservableCollection<TransferObjectModel>
    {
        public TransferQueu()
        {
            Uploads = new ObservableCollection<TransferObjectModel>();
            Downloads = new ObservableCollection<TransferObjectModel>();

            Uploads.CollectionChanged += UploadsOnCollectionChanged;
            Downloads.CollectionChanged += DownloadsOnCollectionChanged;
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
                        case TransferType.Download:
                            DownloadSort(transferObject);
                            break;
                        case TransferType.Upload:
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
                        case TransferType.Download:
                            Downloads.Remove(transferObject);
                            break;
                        case TransferType.Upload:
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
