using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using mega;
using MegaApp.Extensions;
using MegaApp.Resources;

namespace MegaApp.Classes
{
    // "DataContact" and "DataMember" necessary for serialization during app deactivation
    // when the app opened the Web Browser for the Fortumo Payment
    [DataContract]
    public class ProductBase
    {
        [DataMember] public MAccountType AccountType { get; set; }
        [DataMember] public int Amount { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public string FormattedPrice { get; set; }
        [DataMember] public string Currency { get; set; }
        [DataMember] public int GbStorage { get; set; }
        [DataMember] public int GbTransfer { get; set; }        
        [DataMember] public string ProductPathData { get; set; }
        [DataMember] public Color ProductColor { get; set; }        
        [DataMember] public bool IsNewOffer { get; set; }
        [DataMember] public bool Purchased { get; set; }        

        public SolidColorBrush ProductColorBrush 
        {
            get { return new SolidColorBrush(ProductColor); }
            set { ProductColor = value.Color; }
        }

        public ProductBase()
        {
            IsNewOffer = false;
            Purchased = false;
        }

        public string Storage
        {
            get
            {
                return AccountType == MAccountType.ACCOUNT_TYPE_FREE ? "50 GB" : 
                    Convert.ToUInt64(GbStorage).FromGBToBytes().ToStringAndSuffix();
            }
        }

        public string Transfer
        {
            get
            {
                return AccountType == MAccountType.ACCOUNT_TYPE_FREE ? UiResources.Limited : 
                    Convert.ToUInt64(GbTransfer).FromGBToBytes().ToStringAndSuffix();
            }
        }

        public string BasePrice
        {
            get
            {
                return AccountType == MAccountType.ACCOUNT_TYPE_FREE ? UiResources.UI_Free : 
                    string.Format(UiResources.UI_FromBasePrice, (double)(Amount/12) / 100, Currency);
            }
        }
    }
}
