using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace Wpf.XP.Controls
{
    /// <summary>
    /// Windows XP Window Frame
    /// </summary>
    public class WindowFrame : ContentControl
    {
        #region Private fields

        private bool _loaded = false;

        private Window _window = null!;

        private TextBlock _title = null!;

        private Image _titleBarLeft = null!;
        private Image _titleBarMiddle = null!;
        private Image _titleBarRight = null!;

        private Image _sideLeft = null!;
        private Image _sideRight = null!;

        private Image _bottomLeft = null!;
        private Image _bottomMiddle = null!;
        private Image _bottomRight = null!;

        private ImageButton _minimizeButton = null!;
        private ImageButton _maximizeButton = null!;
        private ImageButton _restoreButton = null!;
        private ImageButton _closeButton = null!;

        #endregion

        #region Public properties

        public string Title
        {
            get => GetValue(TitleProperty) as string ?? "";
            set => SetValue(TitleProperty, value);
        }

        public ImageSource? Icon
        {
            get => GetValue(IconProperty) as ImageSource;
            set => SetValue(IconProperty, value);
        }

        public ResizeMode ResizeMode
        {
            get => (ResizeMode)GetValue(ResizeModeProperty);
            set => SetValue(ResizeModeProperty, value);
        }

        #endregion

        #region Dependency properties registration

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(WindowFrame),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(WindowFrame),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ResizeModeProperty =
            DependencyProperty.Register("ResizeMode", typeof(ResizeMode), typeof(WindowFrame),
            new FrameworkPropertyMetadata(ResizeMode.CanResize, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ResizeModePropertyChanged));

        #endregion

        public WindowFrame() : base()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            bool designer = DesignerProperties.GetIsInDesignMode(this);

            // designer does not like any of this
            if (designer)
                return;

            _window = Window.GetWindow(this);

            _title = FindChild<TextBlock>(this, "Title")!;

            _titleBarLeft = FindChild<Image>(this, "TitleBarLeft")!;
            _titleBarMiddle = FindChild<Image>(this, "TitleBarMiddle")!;
            _titleBarRight = FindChild<Image>(this, "TitleBarRight")!;

            _sideLeft = FindChild<Image>(this, "SideLeft")!;
            _sideRight = FindChild<Image>(this, "SideRight")!;

            _bottomLeft = FindChild<Image>(this, "BottomLeft")!;
            _bottomMiddle = FindChild<Image>(this, "BottomMiddle")!;
            _bottomRight = FindChild<Image>(this, "BottomRight")!;

            _minimizeButton = FindChild<ImageButton>(this, "MinimizeButton")!;
            _maximizeButton = FindChild<ImageButton>(this, "MaximizeButton")!;
            _restoreButton = FindChild<ImageButton>(this, "RestoreButton")!;
            _closeButton = FindChild<ImageButton>(this, "CloseButton")!;

            HookWindowProc();
            _window.StateChanged += Window_StateChanged;

            _window.Activated += Window_Activated;
            _window.Deactivated += Window_Deactivated;

            _minimizeButton.Click += MinimizeButton_Click;
            _maximizeButton.Click += MaximizeButton_Click;
            _restoreButton.Click += RestoreButton_Click;
            _closeButton.Click += CloseButton_Click;

            UpdateTitlebarButtons(ResizeMode);

            var windowChrome = new WindowChrome
            {
                ResizeBorderThickness = new Thickness(4),
                CaptionHeight = 30,
                GlassFrameThickness = new Thickness(4)
            };

            WindowChrome.SetWindowChrome(_window, windowChrome);

            _loaded = true;
        }

        #region Maximize Fix

        // to fix maximize - https://stackoverflow.com/a/46465322
        private void HookWindowProc()
        {
            IntPtr handle = new WindowInteropHelper(_window).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    Native.WmGetMinMaxInfo(hwnd, lParam, (int)MinWidth, (int)MinHeight);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }

        #endregion

        #region Resize Mode

        private void UpdateTitlebarButtons(ResizeMode resizeMode)
        {
            if (resizeMode == ResizeMode.NoResize)
            {
                _minimizeButton.Visibility = Visibility.Collapsed;
                _maximizeButton.Visibility = Visibility.Collapsed;
                _restoreButton.Visibility = Visibility.Collapsed;
                return;
            }

            _maximizeButton.IsEnabled = resizeMode != ResizeMode.CanMinimize;
            _restoreButton.IsEnabled = resizeMode != ResizeMode.CanMinimize;

            _minimizeButton.Visibility = Visibility.Visible;
            _maximizeButton.Visibility = _window.WindowState != WindowState.Maximized ? Visibility.Visible : Visibility.Collapsed;
            _restoreButton.Visibility = _window.WindowState == WindowState.Maximized ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void ResizeModePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            WindowFrame? windowFrame = source as WindowFrame;
            if (windowFrame == null)
                return;

            if (!windowFrame._loaded)
                return;

            ResizeMode resizeMode = (ResizeMode)e.NewValue;

            windowFrame.UpdateTitlebarButtons(resizeMode);
        }

        #endregion

        #region Activity

        private ImageSource CreateImageSource(string uri)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Wpf.XP;component/Resources/Frame/" + uri + ".png"));
        }

        private DropShadowEffect CreateNewDropShadowEffect(DropShadowEffect effect, double newOpacity)
        {
            DropShadowEffect newEffect = new DropShadowEffect();

            newEffect.BlurRadius = effect.BlurRadius;
            newEffect.Direction = effect.Direction;
            newEffect.Opacity = newOpacity;
            newEffect.ShadowDepth = effect.ShadowDepth;
            newEffect.Color = effect.Color;

            return newEffect;
        }

        private void ChangeActive(bool isActive)
        {
            string suffix = isActive ? "" : "Inactive";

            _titleBarLeft.Source = CreateImageSource("TitleLeft" + suffix);
            _titleBarMiddle.Source = CreateImageSource("TitleMiddle" + suffix);
            _titleBarRight.Source = CreateImageSource("TitleRight" + suffix);

            _sideLeft.Source = CreateImageSource("SideLeft" + suffix);
            _sideRight.Source = CreateImageSource("SideRight" + suffix);

            _bottomLeft.Source = CreateImageSource("BottomLeft" + suffix);
            _bottomMiddle.Source = CreateImageSource("BottomMiddle" + suffix);
            _bottomRight.Source = CreateImageSource("BottomRight" + suffix);

            _minimizeButton.Image = CreateImageSource("MinimizeButton" + suffix);
            _maximizeButton.Image = CreateImageSource("MaximizeButton" + suffix);
            _maximizeButton.DisabledImage = CreateImageSource("MaximizeButtonDisabled" + suffix);
            _restoreButton.Image = CreateImageSource("RestoreButton" + suffix);
            _restoreButton.DisabledImage = CreateImageSource("RestoreButtonDisabled" + suffix);
            _closeButton.Image = CreateImageSource("CloseButton" + suffix);

            Color textColor = new Color();

            textColor.R = (byte)(isActive ? 255 : 216);
            textColor.G = (byte)(isActive ? 255 : 228);
            textColor.B = (byte)(isActive ? 255 : 248);
            textColor.A = 255;

            _title.Foreground = new SolidColorBrush(textColor);

            // effect is read only so we have to create a new one every time
            var effect = CreateNewDropShadowEffect((DropShadowEffect)_title.Effect, isActive ? 1d : 0d);
            _title.Effect = effect;
        }

        private void Window_Activated(object? sender, EventArgs e)
        {
            ChangeActive(true);
        }

        private void Window_Deactivated(object? sender, EventArgs e)
        {
            ChangeActive(false);
        }

        #endregion

        #region Buttons

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            _window.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            _window.WindowState = WindowState.Maximized;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            _window.WindowState = WindowState.Normal;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _window.Close();
        }

        #endregion

        #region Window State

        private void Window_StateChanged(object? sender, EventArgs e)
        {
            if (_window.WindowState == WindowState.Maximized)
            {
                _maximizeButton.Visibility = Visibility.Collapsed;
                _restoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                _maximizeButton.Visibility = Visibility.Visible;
                _restoreButton.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// Finds a Child of a given item in the visual tree. <br/>
        /// https://stackoverflow.com/a/1759923
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T? FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T? foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T? childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        #endregion

        static WindowFrame()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowFrame), new FrameworkPropertyMetadata(typeof(WindowFrame)));
        }
    }
}
