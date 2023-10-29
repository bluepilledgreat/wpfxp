using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.XP.Controls
{
    internal class ImageButton : Button
    {
        private bool _hover = false;
        private bool _pressed = false;

        public ImageSource? Image
        {
            get => GetValue(ImageProperty) as ImageSource;
            set => SetValue(ImageProperty, value);
        }

        public ImageSource? HoverImage
        {
            get => GetValue(HoverImageProperty) as ImageSource;
            set => SetValue(HoverImageProperty, value);
        }

        public ImageSource? PressImage
        {
            get => GetValue(PressImageProperty) as ImageSource;
            set => SetValue(PressImageProperty, value);
        }

        public ImageSource? DisabledImage
        {
            get => GetValue(DisabledImageProperty) as ImageSource;
            set => SetValue(DisabledImageProperty, value);
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ImagePropertyChanged));

        public static readonly DependencyProperty HoverImageProperty =
            DependencyProperty.Register("HoverImage", typeof(ImageSource), typeof(ImageButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ImagePropertyChanged));

        public static readonly DependencyProperty PressImageProperty =
            DependencyProperty.Register("PressImage", typeof(ImageSource), typeof(ImageButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ImagePropertyChanged));

        public static readonly DependencyProperty DisabledImageProperty =
            DependencyProperty.Register("DisabledImage", typeof(ImageSource), typeof(ImageButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ImagePropertyChanged));

        private static void ImagePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ImageButton? imageButton = source as ImageButton;

            if (imageButton == null)
                return;

            imageButton.UpdateImage();
        }

        public ImageButton() : base()
        {
            this.MouseEnter += ImageButton_MouseEnter;
            this.MouseLeave += ImageButton_MouseLeave;

            this.PreviewMouseDown += ImageButton_PreviewMouseDown;
            this.PreviewMouseUp += ImageButton_PreviewMouseUp;

            this.IsEnabledChanged += ImageButton_IsEnabledChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateImage();
        }

        private void UpdateImage()
        {
            // TODO: create image automatically
            Image? image = this.Content as Image;

            if (image == null)
                return;

            if (!IsEnabled)
            {
                image.Source = this.DisabledImage ?? this.Image;

                return;
            }

            if (_pressed && this.PressImage != null)
            {
                image.Source = this.PressImage;
            }
            else if (_hover && this.HoverImage != null)
            {
                image.Source = this.HoverImage;
            }
            else
            {
                image.Source = this.Image;
            }
        }

        protected override void OnContentChanged(object oldValue, object newValue)
        {
            base.OnContentChanged(oldValue, newValue);

            UpdateImage();
        }

        private void ImageButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateImage();
        }

        private void ImageButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _hover = true;
            UpdateImage();
        }

        private void ImageButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _hover = false;
            UpdateImage();
        }

        private void ImageButton_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _pressed = true;
            UpdateImage();
        }

        private void ImageButton_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _pressed = false;
            UpdateImage();
        }
    }
}
