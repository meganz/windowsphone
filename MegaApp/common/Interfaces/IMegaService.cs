using System.Threading;
using mega;
using MegaApp.ViewModels;

namespace MegaApp.Interfaces
{
    public interface IMegaService
    {
        void GetPreviewLink(MegaSDK megaSdk, NodeViewModel nodeViewModel);
        void Rename(MegaSDK megaSdk, NodeViewModel nodeViewModel);
        void Remove(MegaSDK megaSdk, NodeViewModel nodeViewModel, bool isMultiRemove, AutoResetEvent waitEventRequest);
        void Move(MegaSDK megaSdk, NodeViewModel nodeViewModel, NodeViewModel newParentNode);
    }
}