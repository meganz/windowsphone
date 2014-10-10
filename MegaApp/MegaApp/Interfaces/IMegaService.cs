using mega;
using MegaApp.Models;

namespace MegaApp.Interfaces
{
    public interface IMegaService
    {
        void GetPreviewLink(MegaSDK megaSdk, NodeViewModel nodeViewModel);
        void Rename(MegaSDK megaSdk, NodeViewModel nodeViewModel);
        void Remove(MegaSDK megaSdk, NodeViewModel nodeViewModel);
    }
}