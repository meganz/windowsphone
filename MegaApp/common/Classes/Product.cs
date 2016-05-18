using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
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
            get 
            {
                return Months == 1 ? UiResources.Monthly : UiResources.Annually;
            }
        }        

        public string Price
        {
            get
            {
                return String.Format("{0:N} {1}", (double)Amount/100, Currency);
            }
        }
    }
}
