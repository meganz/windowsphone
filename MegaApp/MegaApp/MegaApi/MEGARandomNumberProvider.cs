using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text;
using System.Security.Cryptography;
using mega;

namespace MegaApp.MegaApi
{
    class MEGARandomNumberProvider : MRandomNumberProvider
    {
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        public virtual void GenerateRandomBlock(byte[] value)
        {
            rngCsp.GetBytes(value);
        }
    }
}
