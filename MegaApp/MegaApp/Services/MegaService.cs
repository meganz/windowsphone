using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.MegaApi;
using MegaApp.Models;

namespace MegaApp.Services
{
    static class MegaService
    {
        public static void GetPreviewLink(MegaSDK megaSdk, NodeViewModel nodeViewModel)
        {
            megaSdk.exportNode(nodeViewModel.GetBaseNode(), new ExportNodeRequestListener());
        }
    }
}
