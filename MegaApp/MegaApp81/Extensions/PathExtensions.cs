using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Shapes;
using mega;

namespace MegaApp.Extensions
{
    static class PathExtensions
    {
        public static void SetDataBinding(this Path path, string pathData)
        {
            // You cannot set the Data property of a Path object direct to a string source
            // The data property is of type Geometry
            // You need data binding to bind a string to the Geometry DataProperty
            var binding = new Binding
            {
                Source = pathData,
                Mode = BindingMode.OneWay,
            };
            BindingOperations.SetBinding(path, Path.DataProperty, binding);
        }
    }
}
