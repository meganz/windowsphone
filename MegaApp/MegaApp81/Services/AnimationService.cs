using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace MegaApp.Services
{
    static class AnimationService
    {
        public static RadMoveAnimation GetFolderListingAnimation()
        {
            return new RadMoveAnimation
            {
                StartPoint = new Point(0, 1400), 
                EndPoint = new Point(0,0),
                Duration = new Duration(new TimeSpan(0,0,0,0,500))
            };
        }

        public static RadMoveAnimation GetOpenDialogAnimation()
        {
            return new RadMoveAnimation()
            {
                MoveDirection = MoveDirection.TopIn,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200))
            };
        }
    }
}
