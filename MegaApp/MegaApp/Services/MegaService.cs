using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using mega;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;
using Telerik.Windows.Controls;

namespace MegaApp.Services
{
    class MegaService: IMegaService
    {
        public void GetPreviewLink(MegaSDK megaSdk, NodeViewModel nodeViewModel)
        {
            megaSdk.exportNode(nodeViewModel.GetMegaNode(), new ExportNodeRequestListener());
        }

        public async void Rename(MegaSDK megaSdk, NodeViewModel nodeViewModel)
        {
            // Create prompt dialog with prefilled item name
            var textboxStyle = new Style(typeof(RadTextBox));
            textboxStyle.Setters.Add(new Setter(TextBox.TextProperty, nodeViewModel.Name));

            var inputPromptClosedEventArgs = await RadInputPrompt.ShowAsync(new string[] { UiResources.RenameButton, UiResources.CancelButton }, UiResources.RenameItem,
                vibrate: false, inputStyle: textboxStyle);

            if (inputPromptClosedEventArgs.Result != DialogResult.OK) return;

            megaSdk.renameNode(nodeViewModel.GetMegaNode(), inputPromptClosedEventArgs.Text, new RenameNodeRequestListener(nodeViewModel));
        }

        public void Remove(MegaSDK megaSdk, NodeViewModel nodeViewModel, bool isMultiRemove)
        {
            if(!isMultiRemove)
                if (MessageBox.Show(String.Format(AppMessages.RemoveItemQuestion, nodeViewModel.Name),
                    AppMessages.RemoveItemQuestion_Title, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;

            megaSdk.moveNode(nodeViewModel.GetMegaNode(), megaSdk.getRubbishNode(), new RemoveNodeRequestListener(nodeViewModel, isMultiRemove));
        }

        public void Move(MegaSDK megaSdk, NodeViewModel nodeViewModel, NodeViewModel newParentNode)
        {
            if (megaSdk.checkMove(nodeViewModel.GetMegaNode(), newParentNode.GetMegaNode()).getErrorCode() == MErrorType.API_OK)
            {
                megaSdk.moveNode(nodeViewModel.GetMegaNode(), newParentNode.GetMegaNode(), 
                    new MoveNodeRequestListener(newParentNode, nodeViewModel));
            }
        }
    }
}
