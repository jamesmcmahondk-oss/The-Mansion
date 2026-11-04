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
    // The F1 debug menu.
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        // =========================================================
        // DEBUG MENU (F1) - only compiled into Debug builds
        // =========================================================

        private void ToggleDebugMenu()
        {
            if (DebugMenu.Visibility == Visibility.Visible)
            {
                DebugMenu.Visibility = Visibility.Collapsed;
                return;
            }

            BuildDebugSceneList();
            UpdateDebugFlagsText();
            DebugHotspotsCheckBox.IsChecked = _showHotspotDebug;
            DebugMenu.Visibility = Visibility.Visible;
        }

        private void BuildDebugSceneList()
        {
            DebugSceneList.Children.Clear();

            foreach (Scene scene in Scenes.All)
            {
                bool isCurrent = scene == _currentScene &&
                                 GameScreen.Visibility == Visibility.Visible;

                var button = new Button
                {
                    Content =
                        $"{scene.Name}   ({System.IO.Path.GetFileName(scene.BackgroundImage)}, " +
                        $"{scene.Hotspots.Count} hotspots)" +
                        (isCurrent ? "   <- current" : ""),
                    Tag = scene,
                    Height = 38,
                    Margin = new Thickness(0, 0, 0, 6),
                    Padding = new Thickness(10, 0, 10, 0),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 16,
                    Foreground = isCurrent ? Brushes.Lime : Brushes.White,
                    Background = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
                    Cursor = Cursors.Hand
                };

                button.Click += DebugSceneButton_Click;
                DebugSceneList.Children.Add(button);
            }

            DebugFightList.Children.Clear();

            foreach (Enemy enemy in Enemies.All)
            {
                var fightButton = new Button
                {
                    Content = $"{enemy.Id}   ({enemy.Name}, skill {enemy.Skill}, health {enemy.Health})",
                    Tag = enemy.Id,
                    Height = 38,
                    Margin = new Thickness(0, 0, 0, 6),
                    Padding = new Thickness(10, 0, 10, 0),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 16,
                    Foreground = Brushes.White,
                    Background = new SolidColorBrush(Color.FromRgb(0x33, 0x18, 0x18)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x33, 0x33)),
                    Cursor = Cursors.Hand
                };

                fightButton.Click += DebugFightButton_Click;
                DebugFightList.Children.Add(fightButton);
            }
        }

        private void DebugFightButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: string enemyId })
            {
                return;
            }

            // Need to be in a scene first.
            if (GameScreen.Visibility != Visibility.Visible || _currentScene == null)
            {
                DebugJumpToScene(Scenes.FrontGate);
            }

            if (IsInFight)
            {
                EndFight();
            }

            DebugMenu.Visibility = Visibility.Collapsed;
            StartFight(enemyId);
        }

        private void DebugSceneButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Scene scene })
            {
                DebugJumpToScene(scene);
            }
        }

        /*
         * Jumps straight into a scene from anywhere (title screen,
         * backstory, character roll or another scene).
         */
        private void DebugJumpToScene(Scene scene)
        {
            _backstoryTimer?.Stop();

            if (IsInDialogue)
            {
                EndDialogue();
            }

            if (IsInFight)
            {
                EndFight();
            }

            CloseInventory();
            _selectedItem = null;

            // Skipped character creation? Give the player test stats
            // so the stats panel and stat checks have something to use.
            if (_player.MaxHealth <= 0)
            {
                _player.Name = "Tester";
                _player.MaxHealth = 100;
                _player.Health = 100;
                _player.Strength = 12;
                _player.Luck = 12;
                _player.Wits = 12;
            }

            TitleScreen.Visibility = Visibility.Collapsed;
            BackstoryScreen.Visibility = Visibility.Collapsed;
            PlayerRollScreen.Visibility = Visibility.Collapsed;
            TrunkScreen.Visibility = Visibility.Collapsed;
            GameScreen.Visibility = Visibility.Visible;

            UpdatePlayerStats();
            ShowScene(scene);

            _interactionMode = InteractionMode.Look;
            UpdateMenuButtonImages();
            SceneCanvas.Cursor = _lookCursor;

            DebugMenu.Visibility = Visibility.Collapsed;
        }

        private void DebugHotspotsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _showHotspotDebug = DebugHotspotsCheckBox.IsChecked == true;

            UpdateDebugVisibility();

            // Redraw hotspots with/without outlines.
            if (_currentScene != null)
            {
                CreateSceneElements(_currentScene);
            }
        }

        private void UpdateDebugFlagsText()
        {
            DebugFlagsText.Text = _player.Flags.Count == 0
                ? "(none set)"
                : string.Join(", ", _player.Flags);
        }

        private void DebugSetFlag_Click(object sender, RoutedEventArgs e)
        {
            string flag = DebugFlagTextBox.Text.Trim();

            if (flag.Length > 0)
            {
                _player.SetFlag(flag);
                AfterStoryChange();      // updates background/hotspots now
                UpdateDebugFlagsText();
            }
        }

        private void DebugClearFlag_Click(object sender, RoutedEventArgs e)
        {
            string flag = DebugFlagTextBox.Text.Trim();

            if (flag.Length > 0)
            {
                _player.ClearFlag(flag);
                AfterStoryChange();
                UpdateDebugFlagsText();
            }
        }

        private void DebugMenuClose_Click(object sender, RoutedEventArgs e)
        {
            DebugMenu.Visibility = Visibility.Collapsed;
        }
    }
}
