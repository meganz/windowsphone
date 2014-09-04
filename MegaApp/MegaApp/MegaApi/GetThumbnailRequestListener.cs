using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GetThumbnailRequestListener: MRequestListenerInterface
    {
        private readonly NodeViewModel _node;
        public GetThumbnailRequestListener(NodeViewModel node)
        {
            this._node = node;
        }

        #region MRequestListenerInterface

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (e.getErrorCode() != MErrorType.API_OK) return;

                string[] test = Directory.GetFiles(Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.ThumbnailsDirectory));

                string filename = request.getFile();
                
                if (test.Length > 0)
                    MessageBox.Show(test[0]);

                

                //using (FileStream file = File.OpenRead(Path.Combine(ApplicationData.Current.LocalFolder.Path, "thumbnails", _node.Name)))
                //{
                //    var bitmapImage = new BitmapImage();
                //    bitmapImage.SetSource(file);

                //    _node.Image = bitmapImage;
                //}
            });
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
            // No status necessary
        }

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show(e.ToString()));
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // No update status necessary
        }

        #endregion



    }
}
