using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using mega;

namespace MegaApp.UserControls
{
    public class PasswordStrengthIndicator : Grid
    {
        public MPasswordStrength Value
        {
            get { return (MPasswordStrength)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(MPasswordStrength),
                typeof(PasswordStrengthIndicator),
                new PropertyMetadata(MPasswordStrength.PASSWORD_STRENGTH_VERYWEAK, PropertyChangedCallback));

        public Brush IndicatorBackground
        {
            get { return (Brush)GetValue(IndicatorBackgroundProperty); }
            set { SetValue(IndicatorBackgroundProperty, value); }
        }

        public static readonly DependencyProperty IndicatorBackgroundProperty =
            DependencyProperty.Register(
                "IndicatorBackground",
                typeof(Brush),
                typeof(PasswordStrengthIndicator),
                new PropertyMetadata(new SolidColorBrush(Colors.Black), PropertyChangedCallback));

        public ObservableCollection<double> IndicatorsOpacity
        {
            get { return (ObservableCollection<double>)GetValue(IndicatorsOpacityProperty); }
            set { SetValue(IndicatorsOpacityProperty, value); }
        }

        public static readonly DependencyProperty IndicatorsOpacityProperty =
            DependencyProperty.Register(
                "IndicatorsOpacity",
                typeof(ObservableCollection<double>),
                typeof(PasswordStrengthIndicator),
                new PropertyMetadata(new ObservableCollection<double>()));

        public Brush IndicatorForeground
        {
            get { return (Brush)GetValue(IndicatorForegroundProperty); }
            set { SetValue(IndicatorForegroundProperty, value); }
        }

        public static readonly DependencyProperty IndicatorForegroundProperty =
            DependencyProperty.Register(
                "IndicatorForeground",
                typeof(Brush),
                typeof(PasswordStrengthIndicator),
                new PropertyMetadata(new SolidColorBrush(Colors.Black), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs dpc)
        {
            var control = d as PasswordStrengthIndicator;
            if (control == null) return;
            if (dpc.NewValue != null)
            {
                if (control.IndicatorsOpacity != null)
                {
                    control.IndicatorsOpacity.CollectionChanged -= control.IndicatorsOpacityOnCollectionChanged;
                    control.IndicatorsOpacity.CollectionChanged += control.IndicatorsOpacityOnCollectionChanged;
                }
                control.Draw();
            }
        }

        private void IndicatorsOpacityOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Draw();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Draw();
        }

        private void Draw()
        {
            this.Children.Clear();
            this.ColumnDefinitions.Clear();

            var indicators = Enum.GetNames(typeof(MPasswordStrength)).Length - 1;
            var value = (int)Value;
            for (int i = 0; i < indicators; i++)
            {
                this.ColumnDefinitions.Add(
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
                var box = new Border
                {
                    BorderThickness = new Thickness(0),
                    Background = IndicatorBackground,
                    Margin = i == indicators - 1 ? new Thickness(0, 0, 0, 0) : new Thickness(0, 0, 2, 0),
                };

                if (value != 0)
                {
                    if (i <= value-1)
                    {
                        var brush = IndicatorForeground as SolidColorBrush;
                        if (brush != null)
                        {
                            var colorFromBrush = brush.Color;
                            box.Background = new SolidColorBrush(colorFromBrush);
                            if (i < IndicatorsOpacity.Count)
                                box.Background.Opacity = IndicatorsOpacity[i];
                        }
                    }
                   
                }
                SetColumn(box, i);
                this.Children.Add(box);
            }
        }
    }
}
