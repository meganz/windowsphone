using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using mega;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.ViewModels;

namespace MegaApp.Services
{
    //class MegaService: IMegaService
    //{
        
        //public void GetPreviewLink(MegaSDK megaSdk, NodeViewModel nodeViewModel)
        //{
        //    megaSdk.exportNode(nodeViewModel.OriginalMNode, new ExportNodeRequestListener());
        //}

        //public async void Rename(MegaSDK megaSdk, NodeViewModel nodeViewModel)
        //{
        //    // Create prompt dialog with prefilled item name
        //    var textboxStyle = new Style(typeof(RadTextBox));
        //    textboxStyle.Setters.Add(new Setter(TextBox.TextProperty, nodeViewModel.Name));

        //    var inputPromptClosedEventArgs = await RadInputPrompt.ShowAsync(new string[] { UiResources.RenameButton, UiResources.CancelButton }, UiResources.RenameItem,
        //        vibrate: false, inputStyle: textboxStyle);

        //    if (inputPromptClosedEventArgs.Result != DialogResult.OK) return;

        //    megaSdk.renameNode(nodeViewModel.OriginalMNode, inputPromptClosedEventArgs.Text, new RenameNodeRequestListener(nodeViewModel));
        //}

        //public void Remove(MegaSDK megaSdk, NodeViewModel nodeViewModel, bool isMultiRemove, AutoResetEvent waitEventRequest)
        //{
        //    // Looking for the absolute parent of the node to remove
        //    MNode _absoluteParentNode, _parentNode;
        //    _absoluteParentNode = nodeViewModel.OriginalMNode;
        //    while ((_parentNode = megaSdk.getParentNode(_absoluteParentNode)) != null)
        //        _absoluteParentNode = _parentNode;

        //    // If the node is on the rubbish bin, delete it forever
        //    if (_absoluteParentNode.getType() == MNodeType.TYPE_RUBBISH)
        //    {
        //        if(!isMultiRemove)
        //            if (MessageBox.Show(String.Format(AppMessages.RemoveItemQuestion, nodeViewModel.Name),
        //                AppMessages.RemoveItemQuestion_Title, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;

        //        megaSdk.remove(nodeViewModel.OriginalMNode, new RemoveNodeRequestListener(nodeViewModel, isMultiRemove, _absoluteParentNode.getType(), waitEventRequest));
        //    }
        //    else // If the node is on the Cloud Drive, move it to the rubbish bin
        //    {
        //        if(!isMultiRemove)
        //            if (MessageBox.Show(String.Format(AppMessages.MoveToRubbishBinQuestion, nodeViewModel.Name),
        //                AppMessages.MoveToRubbishBinQuestion_Title, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;

        //        megaSdk.moveNode(nodeViewModel.GetMegaNode(), megaSdk.getRubbishNode(), new RemoveNodeRequestListener(nodeViewModel, isMultiRemove, _absoluteParentNode.getType(), waitEventRequest));
        //    }   
        //}

        //public void Move(MegaSDK megaSdk, NodeViewModel nodeViewModel, NodeViewModel newParentNode)
        //{
        //    if (megaSdk.checkMove(nodeViewModel.GetMegaNode(), newParentNode.GetMegaNode()).getErrorCode() == MErrorType.API_OK)
        //    {
        //        megaSdk.moveNode(nodeViewModel.GetMegaNode(), newParentNode.GetMegaNode(), 
        //            new MoveNodeRequestListener(newParentNode, nodeViewModel));
        //    }
        //}
    //}
}
