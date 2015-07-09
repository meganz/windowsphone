using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using mega;

namespace MegaApp.Classes
{
    // "DataContact" and "DataMember" necessary for serialization during app deactivation
    // when the app opened the Web Browser for the Fortumo Payment
    [DataContract]
    public class ProductBase
    {
        [DataMember] public MAccountType AccountType { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public int Amount { get; set; }
        [DataMember] public string Currency { get; set; }
        [DataMember] public int GbStorage { get; set; }
        [DataMember] public int GbTransfer { get; set; }
        [DataMember] public Uri ProductUri { get; set; }
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
                return null;
            }
        }

        public string Transfer
        {
            get
            {
                return String.Format("{0} TB", GbTransfer/1024);
            }
        }

        public string BasePrice
        {
            get
            {
                return String.Format("{0:N} {1}", (double)(Amount/12) / 100, Currency);
            }
        }
    }
}
