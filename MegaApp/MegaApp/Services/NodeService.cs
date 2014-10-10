using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.Services
{
    static class NodeService
    {
        public static IEnumerable<string> GetFiles(IEnumerable<NodeViewModel> nodes, string directory)
        {
            return nodes.Select(node => Path.Combine(directory, node.GetMegaNode().getBase64Handle())).ToList();
        }
    }
}
