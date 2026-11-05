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
    // Keyboard shortcuts.
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        // Number keys 1-5 pick dialogue choices (up to 5 are shown).
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
#if DEBUG
            // F1 opens/closes the debug menu from any screen.
            if (e.Key == Key.F1 ||
                (e.Key == Key.Escape && DebugMenu.Visibility == Visibility.Visible))
            {
                ToggleDebugMenu();
                e.Handled = true;
                return;
            }
#endif

            // A picture being looked at (right-click in the inventory, the
            // photograph...): Esc closes it, like a click does.
            if (PictureViewer.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Escape)
                {
                    PictureViewer.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
                return;
            }

            // Typing in a text box (player name, debug flags): keys are just letters.
            bool typing = Keyboard.FocusedElement is TextBox;

            // Hold Space to outline everything you can click in the scene.
            // (Handled so Space never "clicks" a focused button either.)
            if (e.Key == Key.Space && CanUseHotspots && !typing)
            {
                RevealHotspots();
                e.Handled = true;
                return;
            }

            // J opens/closes the journal (Esc closes it too).
            if (IsJournalOpen && ((e.Key == Key.J && !typing) || e.Key == Key.Escape))
            {
                CloseJournal();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.J && CanOpenJournal && !typing)
            {
                ShowJournal(back: null);
                e.Handled = true;
                return;
            }

            if (_inventoryOpen && (e.Key == Key.Left || e.Key == Key.Right))
            {
                TurnInventoryPage(e.Key == Key.Left ? -1 : +1);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && _inventoryOpen)
            {
                CloseInventory();
                e.Handled = true;
                return;
            }

            // Esc: back out of settings or the save/load screen, or open/close the game menu.
            if (e.Key == Key.Escape)
            {
                if (IsSettingsOpen)
                {
                    CloseSettings();
                    e.Handled = true;
                    return;
                }

                if (IsSaveLoadOpen)
                {
                    SaveLoadScreen.Visibility = Visibility.Collapsed;
                    _saveLoadBack?.Invoke();
                    e.Handled = true;
                    return;
                }

                if (IsGameMenuOpen)
                {
                    HideGameMenu();
                    e.Handled = true;
                    return;
                }

                if (GameScreen.Visibility == Visibility.Visible && !_gameOverShown)
                {
                    ShowGameMenu();
                    e.Handled = true;
                    return;
                }
            }

            // No dialogue keys while a menu is open.
            if (IsGameMenuOpen || IsSaveLoadOpen || IsSettingsOpen || IsJournalOpen)
            {
                return;
            }

            if (!IsInDialogue || _inventoryOpen)
            {
                return;
            }

            int index = e.Key switch
            {
                Key.D1 or Key.NumPad1 => 0,
                Key.D2 or Key.NumPad2 => 1,
                Key.D3 or Key.NumPad3 => 2,
                Key.D4 or Key.NumPad4 => 3,
                Key.D5 or Key.NumPad5 => 4,
                _ => -1
            };

            if (index >= 0)
            {
                ChooseDialogueOption(index);
                e.Handled = true;
            }
        }
    }
}
