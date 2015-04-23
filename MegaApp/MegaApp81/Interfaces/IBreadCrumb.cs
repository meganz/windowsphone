using System.Collections.ObjectModel;

namespace MegaApp.Interfaces
{
    public interface IBreadCrumb
    {
        ObservableCollection<IMegaNode> BreadCrumbs { get;  }
        void BuildBreadCrumbs();
    }
}