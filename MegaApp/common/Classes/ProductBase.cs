using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using mega;

namespace MegaApp.Classes
{
    public class ProductBase
    {
        public MAccountType AccountType { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public int GbStorage { get; set; }
        public int GbTransfer { get; set; }
        public Uri ProductUri { get; set; }
        public Color ProductColor { get; set; }
        public Brush ProductColorBrush { get; set; }
        public bool IsNewOffer { get; set; }

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
