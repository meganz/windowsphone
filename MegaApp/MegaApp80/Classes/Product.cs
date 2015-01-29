using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Classes
{
    class Product
    {
        public string Name { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public int GbStorage { get; set; }
        public int GbTransfer { get; set; }
        public int Months { get; set; }
        public ulong Handle { get; set; }
        public Uri ProductUri { get; set; }

        public string Period
        {
            get 
            {
                return Months == 1 ? "Monthly" : "Annually";
            }
        }

        public string Storage
        {
            get
            {
                switch (GbStorage)
                {
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

        public string Price
        {
            get
            {
                return String.Format("{0:N} {1}", (double)Amount/100, Currency);
            }
        }
    }
}
