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
        public static RadFadeAnimation GetListViewAddAnimation()
        {
            return new RadFadeAnimation()
            {
                EndOpacity = 1.0,
                StartOpacity = 0.0,
                AutoReverse = false,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200))
            };
        }

        public static RadFadeAnimation GetListViewRemoveAnimation()
        {
            //return new RadMoveAnimation()
            //{
            //    MoveDirection = MoveDirection.BottomOut,
            //    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200))
            //};
            return new RadFadeAnimation()
            {
                EndOpacity = 0.0,
                StartOpacity = 1.0,
                AutoReverse = false,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200))
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
