using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using mega;
using MegaApp.Resources;

namespace MegaApp.Classes
{
    // "DataContact" and "DataMember" necessary for serialization during app deactivation
    // when the app opened the Web Browser for the Fortumo Payment
    [DataContract]
    public class ProductBase
    {
        [DataMember] public MAccountType AccountType { get; set; }
        [DataMember] public String Name { get; set; }
        [DataMember] public int Amount { get; set; }
        [DataMember] public String Currency { get; set; }
        [DataMember] public int GbStorage { get; set; }
        [DataMember] public int GbTransfer { get; set; }        
        [DataMember] public String ProductPathData { get; set; }
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

        public String Storage
        {
            get
            {
                if (AccountType == MAccountType.ACCOUNT_TYPE_FREE)
                {
                    return "50 GB";
                }
                else
                {
                    switch (GbStorage)
                    {                        
                        case 200:
                            return "200 GB";
                        case 500:
                            return "500 GB";
                        case 2048:
                            return "2 TB";
                        case 4096:
                            return "4 TB";
                    }
                }
                
                return null;
            }
        }

        public String Transfer
        {
            get
            {
                if (AccountType == MAccountType.ACCOUNT_TYPE_FREE)                
                    return UiResources.Limited;
                else
                    return String.Format("{0} TB", GbTransfer/1024);
            }
        }

        public String BasePrice
        {
            get
            {
                if (AccountType == MAccountType.ACCOUNT_TYPE_FREE)
                    return UiResources.UI_Free;
                else
                    return String.Format(UiResources.UI_FromBasePrice, (double)(Amount/12) / 100, Currency);
            }
        }
    }
}
