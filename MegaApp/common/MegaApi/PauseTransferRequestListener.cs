using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Telerik.Windows.Controls;

namespace MegaApp.MegaApi
{
    class PauseTransferRequestListener: BaseRequestListener
    {
        private bool _pause;

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return _pause ? ProgressMessages.PauseTransfers : ProgressMessages.ResumeTransfers; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return _pause ? AppMessages.PausingTransfersFailed : AppMessages.ResumingTransfersFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return _pause ? AppMessages.PausingTransfersFailed_Title: AppMessages.ResumingTransfersFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Override Methods

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            //Get if transfers were paused (true) or resumed (false)
            _pause = request.getFlag();

            base.onRequestStart(api, request);
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            //Get if transfers were paused (true) or resumed (false)
            _pause = request.getFlag();

            ObservableCollection<TransferObjectModel> transfersList;
            switch(request.getNumber())
            {
                case (int)MTransferType.TYPE_DOWNLOAD:
                    transfersList = TransfersService.MegaTransfers.Downloads;
                    break;

                case (int)MTransferType.TYPE_UPLOAD:
                    transfersList = TransfersService.MegaTransfers.Uploads;
                    break;

                default:
                    transfersList = TransfersService.MegaTransfers;
                    break;
            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var numTransfers = transfersList.Count;
                for (int i=0; i<numTransfers; i++)
                {
                    var item = transfersList.ElementAt(i);
                    if (item == null) continue;

                    if (item.TransferedBytes < item.TotalBytes || item.TransferedBytes == 0)
                    {
                        switch (item.Status)
                        {
                            case TransferStatus.Downloading:
                            case TransferStatus.Uploading:
                            case TransferStatus.Queued:
                            {
                                if (_pause)
                                    item.Status = TransferStatus.Paused;
                                break;
                            }
                                    
                            case TransferStatus.Paused:
                            {
                                if (!_pause)
                                    item.Status = TransferStatus.Queued;
                                break;
                            }
                        }
                    }
                }
            });
        }

        #endregion
    }
}
