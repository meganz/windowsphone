using System.Collections.ObjectModel;

namespace MegaApp.Interfaces
{
    public interface IBreadCrumb
    {
        ObservableCollection<IBaseNode> BreadCrumbs { get;  }
        void BuildBreadCrumbs();
    }
}