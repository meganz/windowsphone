using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Resources;

namespace MegaApp.Classes
{
    // "DataContact" and "DataMember" necessary for serialization during app deactivation
    // when the app opened the Web Browser for the Fortumo Payment
    [DataContract]
    public class Product : ProductBase
    {
        [DataMember] public int Months { get; set; }
        [DataMember] public ulong Handle { get; set; }
        [DataMember] public ObservableCollection<PaymentMethod> PaymentMethods { get; set; }        
        
        public Product()
        {
            PaymentMethods = new ObservableCollection<PaymentMethod>();
        }

        public string Period
        {
            get { return Months == 1 ? UiResources.Monthly : UiResources.Annually; }
        }

        public string PricePeriod
        {
            get { return Months == 1 ? UiResources.Month : UiResources.Year; }
        }

        public string MicrosoftStoreId
        {
            get
            {
                switch (this.AccountType)
                {
                    case MAccountType.ACCOUNT_TYPE_FREE:
                        return null;

                    case MAccountType.ACCOUNT_TYPE_LITE:
                        switch (this.Months)
                        {
                            case 1:
                                return AppResources.ProLiteMonth;
                            case 12:
                                return AppResources.ProLiteYear;
                        }
                        break;

                    case MAccountType.ACCOUNT_TYPE_PROI:
                        switch (this.Months)
                        {
                            case 1:
                                return AppResources.Pro1Month;
                            case 12:
                                return AppResources.Pro1Year;
                        }
                        break;

                    case MAccountType.ACCOUNT_TYPE_PROII:
                        switch (this.Months)
                        {
                            case 1:
                                return AppResources.Pro2Month;
                            case 12:
                                return AppResources.Pro2Year;
                        }
                        break;

                    case MAccountType.ACCOUNT_TYPE_PROIII:
                        switch (this.Months)
                        {
                            case 1:
                                return AppResources.Pro3Month;
                            case 12:
                                return AppResources.Pro3Year;
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("AccountType");
                }

                return null;
            }
        }
    }
}
