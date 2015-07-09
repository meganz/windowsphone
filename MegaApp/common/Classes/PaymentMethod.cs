using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;

namespace MegaApp.Classes
{
    public class PaymentMethod
    {
        public MPaymentMethod PaymentMethodType { get; set; }
        public string Name { get; set; }        
        public Uri PaymentMethodUri { get; set; }
    }
}
