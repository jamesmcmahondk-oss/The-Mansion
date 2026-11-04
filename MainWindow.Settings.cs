using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using The_Mansion.Data;
using The_Mansion.Models;
using System.Windows.Threading;

namespace The_Mansion
{
    // Settings: resolution and fullscreen (Data/GameSettings.cs).
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        // =========================================================
        // SETTINGS: resolution and fullscreen (Data/GameSettings.cs)
        // =========================================================

        private GameSettings _settings = GameSettings.Load();

        private Action? _settingsBack;

        private bool IsSettingsOpen => SettingsScreen.Visibility == Visibility.Visible;

        // The monitor's size in real pixels.
        private (double Width, double Height) ScreenPixels()
        {
            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            return (SystemParameters.PrimaryScreenWidth * dpi.DpiScaleX,
                    SystemParameters.PrimaryScreenHeight * dpi.DpiScaleY);
        }

        // Resolutions that fit on this monitor (always at least the smallest).
        private List<(int Width, int Height)> AvailableResolutions()
        {
            var (screenWidth, screenHeight) = ScreenPixels();
            var list = new List<(int Width, int Height)>();

            foreach (var resolution in GameSettings.Resolutions)
            {
                if (resolution.Width <= screenWidth && resolution.Height <= screenHeight)
                {
                    list.Add(resolution);
                }
            }

            if (list.Count == 0)
            {
                list.Add(GameSettings.Resolutions[0]);
            }

            return list;
        }

        /*
         * Sizes the window and scales the 1920x1080 game canvas to match.
         * Because only the canvas is scaled, every hotspot, button and
         * picture keeps lining up.
         */
        private void ApplyDisplaySettings()
        {
            DpiScale dpi = VisualTreeHelper.GetDpi(this);

            if (_settings.Fullscreen)
            {
                // Fit the canvas to the whole monitor (16:9 fits exactly).
                double screenWidth = SystemParameters.PrimaryScreenWidth;    // in WPF units
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                double scale = Math.Min(screenWidth / 1920, screenHeight / 1080);

                RootGrid.LayoutTransform = new ScaleTransform(scale, scale);

                SizeToContent = SizeToContent.Manual;
                WindowState = WindowState.Normal;     // must be normal before restyling
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
            }
            else
            {
                // Make sure the chosen size still fits (e.g. a smaller monitor).
                var available = AvailableResolutions();
                if (!available.Contains((_settings.Width, _settings.Height)))
                {
                    (_settings.Width, _settings.Height) = available[^1];
                }

                // Scale in real pixels: undo Windows display scaling too.
                double scaleX = _settings.Width / 1920.0 / dpi.DpiScaleX;
                double scaleY = _settings.Height / 1080.0 / dpi.DpiScaleY;

                RootGrid.LayoutTransform = new ScaleTransform(scaleX, scaleY);

                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanMinimize;
                SizeToContent = SizeToContent.WidthAndHeight;

                // Re-centre once the window has its new size.
                Dispatcher.BeginInvoke(new Action(PositionWindow), DispatcherPriority.Loaded);
            }
        }

        private void ShowSettings(Action back)
        {
            _settingsBack = back;
            BuildSettingsScreen();
            SettingsScreen.Visibility = Visibility.Visible;
        }

        private void BuildSettingsScreen()
        {
            FullscreenButton.Content = _settings.Fullscreen ? "FULLSCREEN: ON" : "FULLSCREEN: OFF";

            ResolutionList.Children.Clear();

            foreach (var (width, height) in AvailableResolutions())
            {
                bool current = !_settings.Fullscreen &&
                               width == _settings.Width &&
                               height == _settings.Height;

                var button = new Button
                {
                    Content = current ? $"{width} x {height}   (current)" : $"{width} x {height}",
                    Height = 46,
                    Margin = new Thickness(0, 0, 0, 8),
                    Style = (Style)FindResource("GothicButton"),
                    IsEnabled = !_settings.Fullscreen
                };

                if (current)
                {
                    button.BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xD9, 0x8A));
                    button.Background = new SolidColorBrush(Color.FromRgb(0x4A, 0x24, 0x18));
                }

                button.Click += (_, _) =>
                {
                    _settings.Width = width;
                    _settings.Height = height;
                    _settings.Save();
                    ApplyDisplaySettings();
                    BuildSettingsScreen();
                };

                ResolutionList.Children.Add(button);
            }

            SettingsNoteText.Text = _settings.Fullscreen
                ? "Fullscreen fits the game to your screen. Turn it off to pick a window size."
                : "Sizes bigger than your screen are not shown.";
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.Fullscreen = !_settings.Fullscreen;
            _settings.Save();
            ApplyDisplaySettings();
            BuildSettingsScreen();
        }

        private void SettingsBack_Click(object sender, RoutedEventArgs e) => CloseSettings();

        private void CloseSettings()
        {
            SettingsScreen.Visibility = Visibility.Collapsed;
            _settingsBack?.Invoke();
        }

        private void GameMenuSettings_Click(object sender, RoutedEventArgs e)
        {
            HideGameMenu();
            ShowSettings(back: ShowGameMenu);
        }

        private void Return_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
