using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Resources;

namespace MegaApp.Classes
{
    class Product : ProductBase
    {
        public int Months { get; set; }
        public ulong Handle { get; set; }
        
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
