using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MegaApp.UserControls
{
    public class DrawerLayout : Grid
    {
        #region Globals and events

        private readonly PropertyPath _translatePath = new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)");
        private readonly PropertyPath _colorPath = new PropertyPath("(Grid.Background).(SolidColorBrush.Color)");

        private readonly TranslateTransform _listFragmentTransform = new TranslateTransform();
        private readonly TranslateTransform _deltaTransform = new TranslateTransform();
        private const int MaxAlpha = 100;

        public delegate void DrawerEventHandler(object sender);
        public event DrawerEventHandler DrawerOpened;
        public event DrawerEventHandler DrawerClosed;

        private Storyboard _fadeInStoryboard;
        private Storyboard _fadeOutStoryboard;
        private Grid _listFragment;
        private Grid _mainFragment;
        private Grid _shadowFragment;

        #endregion

        #region Dependency Properties

        public bool IsDrawerOpen
        {
            get { return (bool)GetValue(IsDrawerOpenProperty); }
            set { SetValue(IsDrawerOpenProperty, value); }
        }

        public static readonly DependencyProperty IsDrawerOpenProperty = DependencyProperty.Register("IsDrawerOpen", typeof(bool), typeof(DrawerLayout), new PropertyMetadata(false));

        private PropertyPath TranslatePath
        {
            get { return _translatePath; }
        }
        private PropertyPath ColorPath
        {
            get { return _colorPath; }
        }

        #endregion

        #region Methods

        public DrawerLayout()
        {
            IsDrawerOpen = false;
        }

        public void InitializeDrawerLayout()
        {
            if (Children == null) return;
            if (Children.Count < 2) return;

            try
            {
                _mainFragment = Children.OfType<Grid>().FirstOrDefault();
                _listFragment = Children.OfType<Grid>().ElementAt(1);
            }
            catch
            {
                return;
            }

            if (_mainFragment == null || _listFragment == null) return;

            _mainFragment.Name = "_mainFragment";
            _listFragment.Name = "_listFragment";

            // _mainFragment
            _mainFragment.HorizontalAlignment = HorizontalAlignment.Stretch;
            _mainFragment.VerticalAlignment = VerticalAlignment.Stretch;
            if (_mainFragment.Background == null) _mainFragment.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            // Render transform _listFragment
            _listFragment.HorizontalAlignment = HorizontalAlignment.Left;
            _listFragment.VerticalAlignment = VerticalAlignment.Stretch;

            _listFragment.Width = 420; //Hardcoded width (Eduardo design)

            if (_listFragment.Background == null) _listFragment.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            var animatedTranslateTransformList = new TranslateTransform { X = -_listFragment.Width, Y = 0 };
            var animatedTranslateTransformMain = new TranslateTransform { X = 0, Y = 0 };

            _listFragment.RenderTransform = animatedTranslateTransformList;
            _listFragment.RenderTransformOrigin = new Point(0.5, 0.5);

            _listFragment.UpdateLayout();

            _mainFragment.RenderTransform = animatedTranslateTransformMain;
            _mainFragment.RenderTransformOrigin = new Point(0.5, 0.5);

            _mainFragment.UpdateLayout();

            // Create a shadow element
            _shadowFragment = new Grid
            {
                Name = "_shadowFragment",
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Visibility = Visibility.Collapsed
            };
            _shadowFragment.Tap += shadowFragment_Tapped;
            _shadowFragment.IsHitTestVisible = false;

            // Set ZIndexes
            Canvas.SetZIndex(_shadowFragment, 50);
            Canvas.SetZIndex(_listFragment, 51);
            Children.Add(_shadowFragment);

            // Create a new fadeIn animation storyboard
            _fadeInStoryboard = new Storyboard();

            // New double animation
            var listFragmentAnimationIn = new DoubleAnimation { Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)), To = 0 };

            Storyboard.SetTarget(listFragmentAnimationIn, _listFragment);
            Storyboard.SetTargetProperty(listFragmentAnimationIn, TranslatePath);
            _fadeInStoryboard.Children.Add(listFragmentAnimationIn);

            var mainFragmentAnimationOut = new DoubleAnimation { Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)), To = 425 };

            Storyboard.SetTarget(mainFragmentAnimationOut, _mainFragment);
            Storyboard.SetTargetProperty(mainFragmentAnimationOut, TranslatePath);
            _fadeInStoryboard.Children.Add(mainFragmentAnimationOut);

            // Create a new fadeOut animation storyboard
            _fadeOutStoryboard = new Storyboard();

            // New double animation
            var listFragmentAnimationOut = new DoubleAnimation
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
                To = -_listFragment.Width
            };

            Storyboard.SetTarget(listFragmentAnimationOut, _listFragment);
            Storyboard.SetTargetProperty(listFragmentAnimationOut, TranslatePath);
            _fadeOutStoryboard.Children.Add(listFragmentAnimationOut);

            // New double animation
            var mainFragmentAnimationIn = new DoubleAnimation
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
                To = 0
            };

            Storyboard.SetTarget(mainFragmentAnimationIn, _mainFragment);
            Storyboard.SetTargetProperty(mainFragmentAnimationIn, TranslatePath);
            _fadeOutStoryboard.Children.Add(mainFragmentAnimationIn);
         
            _mainFragment.UseOptimizedManipulationRouting = true;
            _mainFragment.ManipulationStarted += mainFragment_ManipulationStarted;
           
            _listFragment.UseOptimizedManipulationRouting = true;
            _listFragment.ManipulationStarted += listFragment_ManipulationStarted;

        }
        public void OpenDrawer()
        {
            if (_fadeInStoryboard == null || _mainFragment == null || _listFragment == null) return;
            
            _shadowFragment.Visibility = Visibility.Visible;
            _shadowFragment.IsHitTestVisible = true;

            _mainFragment.IsHitTestVisible = false;
           
            _fadeInStoryboard.Begin();
            IsDrawerOpen = true;

            if (DrawerOpened != null)
                DrawerOpened(this);
        }
        
        public void CloseDrawer()
        {
            if (_fadeOutStoryboard == null || _mainFragment == null || _listFragment == null) return;
            _fadeOutStoryboard.Begin();
            _fadeOutStoryboard.Completed += fadeOutStoryboard_Completed;
            IsDrawerOpen = false;

            if (DrawerClosed != null)
                DrawerClosed(this);
        }

        public bool CloseIfOpen()
        {
            if (!IsDrawerOpen) return false;
            CloseDrawer();
            return true;
        }

        private void shadowFragment_Tapped(object sender, GestureEventArgs e)
        {
            _shadowFragment.IsHitTestVisible = false;
            _shadowFragment.Visibility = Visibility.Collapsed;

            _mainFragment.IsHitTestVisible = true;

            _fadeOutStoryboard.Begin();

            this.IsDrawerOpen = false;

            // raise close event
            if (DrawerClosed != null) DrawerClosed(this);

            
        }
       
        private void fadeOutStoryboard_Completed(object sender, object e)
        {
            //_shadowFragment.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            _shadowFragment.Visibility = Visibility.Collapsed;
            
            _mainFragment.IsHitTestVisible = true;

            if (DrawerClosed != null) DrawerClosed(this);
        }
        private void MoveListFragment(double left, Color color)
        {
            var s = new Storyboard();

            var doubleAnimationList = new DoubleAnimation
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
                To = left
            };

            Storyboard.SetTarget(doubleAnimationList, _listFragment);
            Storyboard.SetTargetProperty(doubleAnimationList, TranslatePath);
            s.Children.Add(doubleAnimationList);

            var doubleAnimationMain = new DoubleAnimation
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
                To = left <= -420 ? 0 : 425-left
            };

            Storyboard.SetTarget(doubleAnimationMain, _mainFragment);
            Storyboard.SetTargetProperty(doubleAnimationMain, TranslatePath);
            s.Children.Add(doubleAnimationMain);

            s.Begin();
        }

        #endregion

        #region List Fragment manipulation events

        private void listFragment_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            var listWidth = _listFragment.Width;
            
            if (!(e.ManipulationOrigin.X >= listWidth - 100) || !(e.ManipulationOrigin.X < listWidth)) return;
            _listFragment.ManipulationDelta += listFragment_ManipulationDelta;
            _listFragment.ManipulationCompleted += listFragment_ManipulationCompleted;
        }
        private void listFragment_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (Math.Abs(e.CumulativeManipulation.Translation.X) < 0) return;
            if (e.CumulativeManipulation.Translation.X <= -_listFragment.Width || e.CumulativeManipulation.Translation.X > 0)
            {
                listFragment_ManipulationCompleted(this, null);
                return;
            }

            _listFragmentTransform.X = e.CumulativeManipulation.Translation.X;
            _listFragment.RenderTransform = _listFragmentTransform;
            //MoveShadowFragment(e.CumulativeManipulation.Translation.X + _listFragment.Width);
        }
        private void listFragment_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            // Get left of _listFragment
            var transform = (TranslateTransform)_listFragment.RenderTransform;
            if (transform == null) return;
            var left = transform.X;

            // Set snap divider to 1/3 of _mainFragment width
            var snapLimit = _mainFragment.ActualWidth / 3;

            // Get init position of _listFragment
            const int initialPosition = 0;

            // If current left coordinate is smaller than snap limit, close drawer
            if (Math.Abs(initialPosition - left) > snapLimit)
            {
                MoveListFragment(-_listFragment.Width, Color.FromArgb(0, 0, 0, 0));
                
                _shadowFragment.Visibility = Visibility.Collapsed;
                _shadowFragment.IsHitTestVisible = false;

                _mainFragment.IsHitTestVisible = true;

                _listFragment.ManipulationDelta -= listFragment_ManipulationDelta;
                _listFragment.ManipulationCompleted -= listFragment_ManipulationCompleted;
                IsDrawerOpen = false;

                // raise DrawerClosed event
                if (DrawerClosed != null) DrawerClosed(this);
            }
            // else open drawer
            else if (Math.Abs(initialPosition - left) < snapLimit)
            {
                // move drawer to zero
                MoveListFragment(0, Color.FromArgb(190, 0, 0, 0));
                
                _shadowFragment.Visibility = Visibility.Visible;
                _shadowFragment.IsHitTestVisible = true;

                _mainFragment.IsHitTestVisible = false;

                _listFragment.ManipulationDelta -= listFragment_ManipulationDelta;
                _listFragment.ManipulationCompleted -= listFragment_ManipulationCompleted;
                IsDrawerOpen = true;

                // raise Drawer_Open event
                if (DrawerOpened != null) DrawerOpened(this);
            }
        }

        #endregion

        #region Main fragment manipulation events

        private void mainFragment_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            // If the user has the first touch on the left side of canvas, that means he's trying to swipe the drawer
            if (!(e.ManipulationOrigin.X <= 40)) return;

            // Manipulation can be allowed
            _mainFragment.ManipulationDelta += mainFragment_ManipulationDelta;
            _mainFragment.ManipulationCompleted += mainFragment_ManipulationCompleted;
        }
        private void mainFragment_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (Math.Abs(e.CumulativeManipulation.Translation.X) < 0) return;
            if (e.CumulativeManipulation.Translation.X >= _listFragment.Width)
            {
                mainFragment_ManipulationCompleted(this, null);
                return;
            }

            _deltaTransform.X = -_listFragment.Width + e.CumulativeManipulation.Translation.X;
            _listFragment.RenderTransform = _deltaTransform;
            //MoveShadowFragment(e.CumulativeManipulation.Translation.X);
        }
        private void mainFragment_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            // Get left of _listFragment
            var transform = (TranslateTransform)_listFragment.RenderTransform;
            if (transform == null) return;
            var left = transform.X;

            // Set snap divider to 1/3 of _mainFragment width
            var snapLimit = _mainFragment.ActualWidth / 3;

            // Get init position of _listFragment
            var initialPosition = -_listFragment.Width;

            // If current left coordinate is smaller than snap limit, close drawer
            if (Math.Abs(initialPosition - left) < snapLimit)
            {
                MoveListFragment(initialPosition, Color.FromArgb(0, 0, 0, 0));
                
                _shadowFragment.Visibility = Visibility.Collapsed;
                _shadowFragment.IsHitTestVisible = false;

                _mainFragment.IsHitTestVisible = true;

                _mainFragment.ManipulationDelta -= mainFragment_ManipulationDelta;
                _mainFragment.ManipulationCompleted -= mainFragment_ManipulationCompleted;
                IsDrawerOpen = false;

                // raise DrawerClosed event
                if (DrawerClosed != null) DrawerClosed(this);
            }
            // else open drawer
            else if (Math.Abs(initialPosition - left) > snapLimit)
            {
                // move drawer to zero
                MoveListFragment(0, Color.FromArgb(190, 0, 0, 0));
                
                _shadowFragment.Visibility = Visibility.Visible;
                _shadowFragment.IsHitTestVisible = true;

                _mainFragment.IsHitTestVisible = false;

                _mainFragment.ManipulationDelta -= mainFragment_ManipulationDelta;
                _mainFragment.ManipulationCompleted -= mainFragment_ManipulationCompleted;
                IsDrawerOpen = true;

                // raise DrawerClosed event
                if (DrawerOpened != null) DrawerOpened(this);
            }
        }

        #endregion

    }
}
