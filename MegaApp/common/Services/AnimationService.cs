using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Animation;

namespace MegaApp.Services
{
    static class AnimationService
    {
        public static RadMoveAnimation GetOpenDialogAnimation()
        {
            return new RadMoveAnimation()
            {
                MoveDirection = MoveDirection.TopIn,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200))
            };
        }

        public static RadMoveAnimation GetOpenMessageDialogAnimation()
        {
            return new RadMoveAnimation()
            {
                MoveDirection = MoveDirection.BottomIn,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200))
            };
        }

        public static RadMoveAnimation GetCloseMessageDialogAnimation()
        {
            return new RadMoveAnimation()
            {
                MoveDirection = MoveDirection.TopOut,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200))
            };
        }

        public static RadMoveAnimation GetPageInAnimation()
        {
            return new RadMoveAnimation()
            {
                MoveDirection = MoveDirection.RightIn,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 100))
            };
        }

        public static RadMoveAnimation GetPageOutAnimation()
        {
            return new RadMoveAnimation()
            {
                MoveDirection = MoveDirection.LeftOut,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 100))
            };
        }
    }
}
