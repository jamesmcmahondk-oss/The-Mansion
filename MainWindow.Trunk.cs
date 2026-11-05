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
    // The car trunk screen (pick a starting item - Data/Trunk.cs).
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        // =========================================================
        // CAR TRUNK SCREEN (pick one starting item - Data/Trunk.cs)
        // =========================================================

        private TrunkItem? _selectedTrunkItem;
        private Border? _selectedTrunkOutline;

        private static readonly Brush TrunkHoverBorder =
            Paint.Argb(0xAA, 0xFF, 0xD9, 0x8A);

        private static readonly Brush TrunkHoverFill =
            Paint.Argb(0x1A, 0xFF, 0xD9, 0x8A);

        private static readonly Brush TrunkSelectedBorder =
            Paint.Rgb(0xFF, 0xD9, 0x8A);

        private static readonly Brush TrunkSelectedFill =
            Paint.Argb(0x33, 0xFF, 0xD9, 0x8A);

        // trunk.png pixels -> screen (must match the trunk Image in MainWindow.xaml).
        private static Rect TrunkToScreen(double x, double y, double width, double height)
        {
            return new Rect(x * 1.25, y * 1.25 - 100, width * 1.25, height * 1.25);
        }

        private void ShowTrunkScreen()
        {
            _selectedTrunkItem = null;
            _selectedTrunkOutline = null;

            TrunkCanvas.Children.Clear();

            foreach (TrunkItem trunkItem in Trunk.Items)
            {
                TrunkCanvas.Children.Add(CreateTrunkOutline(trunkItem));
            }

            TrunkItemNameText.Text = "Hover over an item";
            TrunkItemDescriptionText.Text = "Click one to choose it.";
            TrunkTakeButton.Content = "CHOOSE AN ITEM";
            TrunkTakeButton.IsEnabled = false;

            TrunkScreen.Visibility = Visibility.Visible;
        }

        private Border CreateTrunkOutline(TrunkItem trunkItem)
        {
            Rect screen = TrunkToScreen(trunkItem.X, trunkItem.Y, trunkItem.Width, trunkItem.Height);

            var outline = new Border
            {
                Width = screen.Width,
                Height = screen.Height,
                CornerRadius = new CornerRadius(14),
                BorderThickness = new Thickness(3),
                BorderBrush = Brushes.Transparent,
                Background = Brushes.Transparent,   // keeps the whole area clickable
                Cursor = Cursors.Hand,
                Tag = trunkItem
            };

            Canvas.SetLeft(outline, screen.X);
            Canvas.SetTop(outline, screen.Y);

            outline.MouseEnter += (_, _) =>
            {
                if (outline != _selectedTrunkOutline)
                {
                    outline.BorderBrush = TrunkHoverBorder;
                    outline.Background = TrunkHoverFill;
                }

                ShowTrunkItemInfo(trunkItem);
            };

            outline.MouseLeave += (_, _) =>
            {
                if (outline != _selectedTrunkOutline)
                {
                    outline.BorderBrush = Brushes.Transparent;
                    outline.Background = Brushes.Transparent;
                }

                // Back to the chosen item's info (if any).
                if (_selectedTrunkItem != null)
                {
                    ShowTrunkItemInfo(_selectedTrunkItem);
                }
                else
                {
                    TrunkItemNameText.Text = "Hover over an item";
                    TrunkItemDescriptionText.Text = "Click one to choose it.";
                }
            };

            outline.MouseLeftButtonDown += (_, args) =>
            {
                args.Handled = true;
                SelectTrunkItem(trunkItem, outline);
            };

            return outline;
        }

        private void ShowTrunkItemInfo(TrunkItem trunkItem)
        {
            Item item = Items.GetItem(trunkItem.ItemName);

            TrunkItemNameText.Text = item.Name;
            TrunkItemDescriptionText.Text = item.Description;
        }

        private void SelectTrunkItem(TrunkItem trunkItem, Border outline)
        {
            // Un-highlight the previous choice.
            if (_selectedTrunkOutline != null)
            {
                _selectedTrunkOutline.BorderBrush = Brushes.Transparent;
                _selectedTrunkOutline.Background = Brushes.Transparent;
            }

            _selectedTrunkItem = trunkItem;
            _selectedTrunkOutline = outline;

            outline.BorderBrush = TrunkSelectedBorder;
            outline.Background = TrunkSelectedFill;

            ShowTrunkItemInfo(trunkItem);

            TrunkTakeButton.Content = $"TAKE THE {trunkItem.ItemName.ToUpperInvariant()}";
            TrunkTakeButton.IsEnabled = true;
        }

        private void TrunkTakeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTrunkItem == null)
            {
                return;
            }

            _player.Give(_selectedTrunkItem.ItemName);
            _selectedTrunkItem.OnTake?.Invoke(_player);

            TrunkScreen.Visibility = Visibility.Collapsed;

            EnterGame();
        }
    }
}
