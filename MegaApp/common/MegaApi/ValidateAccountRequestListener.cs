using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class ValidateAccountRequestListener : MRequestListenerInterface
    {
        private readonly string _productId;

        public ValidateAccountRequestListener(string productId)
        {
            _productId = productId;
        }


        public void onRequestStart(MegaSDK api, MRequest request)
        {
            // ignore
        }

        public void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            if (e.getErrorCode() != MErrorType.API_OK) return;

            switch (request.getType())
            {
                case MRequestType.TYPE_ACCOUNT_DETAILS:
                {
                    var accountType = request.getMAccountDetails().getProLevel();

                    switch (_productId)
                    {
                        case "mega.microsoftstore.prolite.oneMonth":
                        case "mega.microsoftstore.prolite.oneYear":
                            {
                                if (accountType != MAccountType.ACCOUNT_TYPE_LITE)
                                {
                                    // If account is already higher, ignore
                                    if (accountType == MAccountType.ACCOUNT_TYPE_PROI ||
                                        accountType == MAccountType.ACCOUNT_TYPE_PROII ||
                                        accountType == MAccountType.ACCOUNT_TYPE_PROIII) return;
                                    LicenseService.RetryLicense(_productId);
                                }
                                break;
                            }
                        case "mega.microsoftstore.pro1.oneMonth":
                        case "mega.microsoftstore.pro1.oneYear":
                            {
                                if (accountType != MAccountType.ACCOUNT_TYPE_PROI)
                                {
                                    // If account is already higher, ignore
                                    if (accountType == MAccountType.ACCOUNT_TYPE_PROII ||
                                        accountType == MAccountType.ACCOUNT_TYPE_PROIII) return;
                                    LicenseService.RetryLicense(_productId);
                                }
                                break;
                            }
                        case "mega.microsoftstore.pro2.oneMonth":
                        case "mega.microsoftstore.pro2.oneYear":
                            {
                                if (accountType != MAccountType.ACCOUNT_TYPE_PROII)
                                {
                                    // If account is already higher, ignore
                                    if (accountType == MAccountType.ACCOUNT_TYPE_PROIII) return;
                                    LicenseService.RetryLicense(_productId);
                                }
                                break;
                            }
                        case "mega.microsoftstore.pro3.oneMonth":
                        case "mega.microsoftstore.pro3.oneYear":
                            {
                                if (accountType != MAccountType.ACCOUNT_TYPE_PROIII)
                                {
                                    LicenseService.RetryLicense(_productId);
                                }
                                break;
                            }
                    }
                    break;
                }
            }            
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // ignore
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // ignore
        }
    }
}
