using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MegaApp.Converters;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Primitives;

namespace MegaApp.Classes
{
    /// <summary>
    /// Represents a control that provides a different image depending on the resolution of the device.
    /// 
    /// </summary>
    public class CustomMultiResolutionImage : RadMultiResolutionImage
    {
        /// <summary>
        /// Identifies the <see cref="P:Telerik.Windows.Controls.CustomMultiResolutionImage.Source"/> dependency property.
        /// 
        /// </summary>
        //public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(CustomMultiResolutionImage), new PropertyMetadata((PropertyChangedCallback)null));
        ///// <summary>
        ///// Identifies the <see cref="P:Telerik.Windows.Controls.CustomMultiResolutionImage.Stretch"/> dependency property.
        ///// 
        ///// </summary>
        //public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(CustomMultiResolutionImage), new PropertyMetadata((object)Stretch.Uniform));
        private Image image;

        /// <summary>
        /// Gets or sets the source for the image.
        /// 
        /// </summary>
        //public ImageSource Source
        //{
        //    get
        //    {
        //        return (ImageSource)this.GetValue(CustomMultiResolutionImage.SourceProperty);
        //    }
        //    set
        //    {
        //        this.SetValue(CustomMultiResolutionImage.SourceProperty, (object)value);
        //    }
        //}

        /// <summary>
        /// Gets or sets a value that describes how an <see cref="T:Telerik.Windows.Controls.CustomMultiResolutionImage"/> should be stretched to fill the destination rectangle.
        /// 
        /// </summary>
        //public Stretch Stretch
        //{
        //    get
        //    {
        //        return (Stretch)this.GetValue(CustomMultiResolutionImage.StretchProperty);
        //    }
        //    set
        //    {
        //        this.SetValue(CustomMultiResolutionImage.StretchProperty, (object)value);
        //    }
        //}

        /// <summary>
        /// Occurs when there is an error associated with image retrieval or format.
        /// 
        /// </summary>
        public new event EventHandler<ExceptionRoutedEventArgs> ImageFailed;

        ///// <summary>
        ///// Occurs when this image source is downloaded and decoded with no failure. You can use this event to determine the size of an image before rendering it.
        ///// 
        ///// </summary>
        public new event EventHandler<RoutedEventArgs> ImageOpened;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Telerik.Windows.Controls.CustomMultiResolutionImage"/> class.
        /// 
        /// </summary>
        public CustomMultiResolutionImage()
        {
            this.DefaultStyleKey = (object)typeof(RadMultiResolutionImage);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>. In simplest terms, this means the method is called just before a UI element displays in an application. For more information, see Remarks.
        /// 
        /// </summary>
        public override void OnApplyTemplate()
        {
            //base.OnApplyTemplate();
            this.image = this.GetTemplatePart<Image>("PART_Image", true);
            this.image.SetBinding(Image.SourceProperty, new Binding()
            {
                Mode = BindingMode.OneWay,
                Path = new PropertyPath("Source", new object[0]),
                Source = (object)this,
                ConverterParameter = (object)Application.Current.Host.Content.ScaleFactor,
                Converter = (IValueConverter)new CustomMultiResolutionResolver()
            });
            this.image.ImageFailed += this.ImageFailed;
            this.image.ImageOpened += this.ImageOpened;
        }
    }
}
