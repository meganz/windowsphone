using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;

namespace MegaApp.MegaApi
{
    class FastLoginRequestListener: MRequestListenerInterface
    {
        public void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            throw new NotImplementedException();
        }

        public void onRequestStart(MegaSDK api, MRequest request)
        {
            throw new NotImplementedException();
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            throw new NotImplementedException();
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
