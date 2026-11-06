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
    // The Esc game menu, saving and loading (Data/SaveSystem.cs).
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        // =========================================================
        // SAVE / LOAD (Data/SaveSystem.cs) and the Esc game menu
        // =========================================================

        // Where BACK on the save/load screen returns to.
        private Action? _saveLoadBack;

        private bool IsGameMenuOpen => GameMenuScreen.Visibility == Visibility.Visible;
        private bool IsSaveLoadOpen => SaveLoadScreen.Visibility == Visibility.Visible;

        // Saving is only allowed while exploring.
        private bool CanSaveNow =>
            GameScreen.Visibility == Visibility.Visible &&
            _currentScene != null &&
            !IsInDialogue &&
            !IsInFight &&
            !_gameOverShown;

        private void ShowGameMenu()
        {
            CloseInventory();

            GameMenuSaveButton.IsEnabled = CanSaveNow;
            GameMenuNoteText.Text = CanSaveNow
                ? ""
                : "You can't save during a conversation or a fight.";

            GameMenuScreen.Visibility = Visibility.Visible;
        }

        private void HideGameMenu() => GameMenuScreen.Visibility = Visibility.Collapsed;

        private void GameMenuResume_Click(object sender, RoutedEventArgs e) => HideGameMenu();

        private void GameMenuSave_Click(object sender, RoutedEventArgs e)
        {
            HideGameMenu();
            ShowSaveLoad(saving: true, back: ShowGameMenu);
        }

        private void GameMenuLoad_Click(object sender, RoutedEventArgs e)
        {
            HideGameMenu();
            ShowSaveLoad(saving: false, back: ShowGameMenu);
        }

        private void GameMenuJournal_Click(object sender, RoutedEventArgs e)
        {
            HideGameMenu();
            ShowJournal(back: ShowGameMenu);
        }

        private void GameMenuQuit_Click(object sender, RoutedEventArgs e)
        {
            var answer = MessageBox.Show(
                "Return to the title screen? Anything you haven't saved will be lost.",
                "Quit to title",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer == MessageBoxResult.Yes)
            {
                HideGameMenu();
                ResetToTitle();
            }
        }

        private void GameOverLoad_Click(object sender, RoutedEventArgs e)
        {
            ShowSaveLoad(saving: false, back: () => { });   // game over screen stays behind
        }

        private void SaveLoadBack_Click(object sender, RoutedEventArgs e)
        {
            SaveLoadScreen.Visibility = Visibility.Collapsed;
            _saveLoadBack?.Invoke();
        }

        /*
         * Shows the three slots. Saving: click any slot (asks before
         * overwriting). Loading: click a slot that has a save.
         */
        private void ShowSaveLoad(bool saving, Action back)
        {
            _saveLoadBack = back;

            SaveLoadTitleText.Text = saving ? "SAVE GAME" : "LOAD GAME";
            SaveLoadSubtitleText.Text = saving
                ? "Choose a slot to save your progress."
                : "Choose a saved game to continue.";

            SaveSlotsPanel.Children.Clear();

            // The autosave can be loaded but never saved over by hand.
            if (!saving)
            {
                SaveSlotsPanel.Children.Add(CreateSaveSlotButton(SaveSystem.AutosaveSlot, saving));
            }

            for (int slot = 1; slot <= SaveSystem.SlotCount; slot++)
            {
                SaveSlotsPanel.Children.Add(CreateSaveSlotButton(slot, saving));
            }

            SaveLoadScreen.Visibility = Visibility.Visible;
        }

        private Button CreateSaveSlotButton(int slot, bool saving)
        {
            SaveGame? save = SaveSystem.Load(slot);

            string slotName = slot == SaveSystem.AutosaveSlot ? "AUTOSAVE" : $"SLOT {slot}";

            var title = new TextBlock
            {
                Text = save == null
                    ? $"{slotName}  -  Empty"
                    : $"{slotName}  -  {save.PlayerName}",
                FontSize = 22
            };

            var details = new TextBlock
            {
                Text = save == null
                    ? (saving ? "Click to save here." : "Nothing saved here yet.")
                    : $"{PrettySceneName(save.SceneName)}   ·   Health {save.Health}/{save.MaxHealth}   ·   " +
                      $"{save.SavedAt:d MMM yyyy, HH:mm}",
                FontSize = 16,
                FontWeight = FontWeights.Normal,
                Foreground = Paint.Rgb(0xA8, 0x9A, 0x84),
                Margin = new Thickness(0, 4, 0, 0)
            };

            var button = new Button
            {
                Content = new StackPanel { Children = { title, details } },
                Height = 84,
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(20, 0, 20, 0),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Style = (Style)FindResource("GothicButton"),
                IsEnabled = saving || save != null
            };

            button.Click += (_, _) =>
            {
                if (saving)
                {
                    SaveToSlot(slot, overwriting: save != null);
                }
                else if (save != null)
                {
                    LoadSave(save);
                }
            };

            return button;
        }

        // "FountainCloseup" -> "Fountain Closeup"
        private static string PrettySceneName(string sceneName)
        {
            var text = new System.Text.StringBuilder();

            foreach (char c in sceneName)
            {
                if (char.IsUpper(c) && text.Length > 0)
                {
                    text.Append(' ');
                }
                text.Append(c);
            }

            return text.ToString();
        }

        private void SaveToSlot(int slot, bool overwriting)
        {
            if (!CanSaveNow || _currentScene == null)
            {
                return;
            }

            if (overwriting)
            {
                var answer = MessageBox.Show(
                    $"Overwrite the game saved in slot {slot}?",
                    "Save game",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            string? error = SaveSystem.Save(slot, SaveGame.From(_player, _currentScene.Name));

            if (error != null)
            {
                MessageBox.Show(
                    $"The game could not be saved.\n\n{error}",
                    "Save failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            SaveLoadScreen.Visibility = Visibility.Collapsed;
            ShowDescription($"Game saved in slot {slot}.");
        }

        private void LoadSave(SaveGame save)
        {
            Scene? scene = Scenes.GetScene(save.SceneName);

            if (scene == null)
            {
                MessageBox.Show(
                    $"This save is in a scene called '{save.SceneName}', which no longer exists.",
                    "Load failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Close whatever was going on.
            SaveLoadScreen.Visibility = Visibility.Collapsed;
            HideGameMenu();
            GameOverScreen.Visibility = Visibility.Collapsed;
            _gameOverShown = false;

            if (IsInDialogue)
            {
                EndDialogue();
            }

            EndFight();
            _backstoryTimer?.Stop();

            _player = save.ToPlayer();
            _selectedItem = null;
            CloseInventory();
            CloseJournal(goBack: false);
            CloseCrossPuzzle();
            CloseFramePuzzle();
            _unreadJournal.Clear();

            // Saves from before the journal: fill it in quietly.
            Journal.Update(_player);
            _fightSnapshot = null;

            TitleScreen.Visibility = Visibility.Collapsed;
            BackstoryScreen.Visibility = Visibility.Collapsed;
            PlayerRollScreen.Visibility = Visibility.Collapsed;
            TrunkScreen.Visibility = Visibility.Collapsed;
            GameScreen.Visibility = Visibility.Visible;

            ShowScene(scene);
            UpdatePlayerStats();

            _interactionMode = InteractionMode.Look;
            UpdateMenuButtonImages();
            SceneCanvas.Cursor = _lookCursor;
        }

        // Back to the title screen with a brand new hero.
        private void ResetToTitle()
        {
            CloseJournal(goBack: false);
            CloseCrossPuzzle();
            CloseFramePuzzle();
            Sound.Ambience("Title_music");
            _gameOverShown = false;
            GameOverScreen.Visibility = Visibility.Collapsed;
            SaveLoadScreen.Visibility = Visibility.Collapsed;
            HideGameMenu();

            if (IsInDialogue)
            {
                EndDialogue();
            }

            EndFight();
            CloseInventory();
            _backstoryTimer?.Stop();
            _replyShowing = false;
            PictureViewer.Visibility = Visibility.Collapsed;

            // A brand new hero: nothing from the last game carries over.
            _player = new Player();
            _rerollsRemaining = 3;
            _selectedItem = null;
            _fightSnapshot = null;
            _currentScene = null;
            _unreadJournal.Clear();
            _interactionMode = InteractionMode.Look;
            UpdateMenuButtonImages();

            GameScreen.Visibility = Visibility.Collapsed;
            TrunkScreen.Visibility = Visibility.Collapsed;
            PlayerRollScreen.Visibility = Visibility.Collapsed;
            BackstoryScreen.Visibility = Visibility.Collapsed;
            TitleScreen.Visibility = Visibility.Visible;
        }
    }
}
