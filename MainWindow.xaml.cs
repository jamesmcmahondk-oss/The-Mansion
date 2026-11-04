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
    public partial class MainWindow : Window
    {
        private Player _player = new Player();

        private Random _random = new Random();

        private int _rerollsRemaining = 3;
        private DispatcherTimer? _backstoryTimer;

        private string _backstoryFullText = "";

        private int _backstoryCharacterIndex;

        private enum InteractionMode
        {
            Look,
            Talk,
            Act,
            Item
        }
        private InteractionMode _interactionMode =
    InteractionMode.Look;
        private Scene? _currentScene;

        /*
         * Set to true while positioning hotspots.
         * Set to false for normal gameplay.
         */
        private bool _showHotspotDebug = false;

        /*
         * Drag-selection state.
         */
        private bool _isCreatingHotspot;
        private bool _hasDragged;

        private Point _dragStartPoint;

        private Rectangle? _selectionRectangle;
// private Cursor _normalCursor;
        // Cursors are built into the game (Resource files in Assets/Cursors),
        // so they work wherever the game is installed.
        private Cursor _actCursor = LoadCursor("cursor_act.cur");
        private Cursor _lookCursor = LoadCursor("cursor_look.cur");
        private Cursor _talkCursor = LoadCursor("cursor_talk.cur");
        private Cursor _itemCursor = LoadCursor("cursor_item.cur");

        /*
         * Loads a cursor from Assets/Cursors inside the game.
         * If it can't be found, the normal arrow is used instead of crashing.
         */
        private static Cursor LoadCursor(string fileName)
        {
            try
            {
                var resource = Application.GetResourceStream(new Uri(
                    $"pack://application:,,,/Assets/Cursors/{fileName}",
                    UriKind.Absolute));

                if (resource != null)
                {
                    return new Cursor(resource.Stream);
                }
            }
            catch (Exception)
            {
                // Fall through to the default cursor.
            }

            return Cursors.Arrow;
        }
        /*
         * Small movement threshold so a normal click on an existing
         * hotspot is not mistaken for drawing a new hotspot.
         */
        private const double DragThreshold = 5.0;

        public MainWindow()
        {
            InitializeComponent();

            // Load all game data now, so a mistake in Data/Scenes.cs,
            // Dialogues.cs or Items.cs shows up straight away at start-up.
            _ = Scenes.All.Count();
            _ = Dialogues.Count;
            _ = Items.All.Count();
            _ = Enemies.All.Count();

#if DEBUG
            // Check every reference in the game data (scene names, items,
            // conversations, fights, pictures) and list any mistakes.
            Loaded += (_, _) => GameValidator.ShowProblems();
#endif

            // Game area is exactly 1920x1080 screen pixels; the window
            // (with its title bar) is then centred on screen.
            // Window size comes from Settings (resolution / fullscreen).
            SourceInitialized += (_, _) => ApplyDisplaySettings();
            Loaded += (_, _) =>
            {
                if (!_settings.Fullscreen)
                {
                    PositionWindow();
                }
            };

            UpdateDebugVisibility();
            this.Cursor = LoadCursor("cursor_arrow.cur");
            ShowScene(Scenes.FrontGate);


        }

        /*
         * Centre the window on the screen's work area (the area not covered
         * by the taskbar). If the window is taller than the work area, as it
         * is on a 1920x1080 monitor once the title bar is added, it is pinned
         * to the top instead so the title bar with the close (X) button is
         * always visible.
         */
        private void PositionWindow()
        {
            Rect workArea = SystemParameters.WorkArea;

            Left = workArea.Left + (workArea.Width - ActualWidth) / 2;

            Top = ActualHeight <= workArea.Height
                ? workArea.Top + (workArea.Height - ActualHeight) / 2
                : workArea.Top;
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            ApplyDisplaySettings();
        }

        /*
         * Scene background placement - must match RoomBackground in
         * MainWindow.xaml. Backgrounds are 1536x1024 source images scaled
         * by 1.25 (to 1920x1280) and shifted up 240px so the middle 1920x800
         * is visible above the HUD.
         *
         * Hotspots in Data/Scenes.cs use source-image pixel coordinates
         * (0-1536, 0-1024), so they always sit on the right part of the
         * picture even if this placement changes.
         */
        private const double SceneImageScale = 1.25;
        private const double SceneImageCropTop = 240;

        private static Rect ImageToScreen(Rect imageRect)
        {
            return new Rect(
                imageRect.X * SceneImageScale,
                imageRect.Y * SceneImageScale - SceneImageCropTop,
                imageRect.Width * SceneImageScale,
                imageRect.Height * SceneImageScale);
        }

        private static Rect ScreenToImage(Rect screenRect)
        {
            return new Rect(
                screenRect.X / SceneImageScale,
                (screenRect.Y + SceneImageCropTop) / SceneImageScale,
                screenRect.Width / SceneImageScale,
                screenRect.Height / SceneImageScale);
        }

        private void ShowScreen(UIElement screen)
        {
            TitleScreen.Visibility = Visibility.Collapsed;
            GameScreen.Visibility = Visibility.Collapsed;
// LoadGameScreen.Visibility = Visibility.Collapsed;
  //          SettingsScreen.Visibility = Visibility.Collapsed;
    //        CreditsScreen.Visibility = Visibility.Collapsed;

            screen.Visibility = Visibility.Visible;
        }

        private void BackstoryTimer_Tick(
    object? sender,
    EventArgs e)
        {
            if (_backstoryCharacterIndex >=
                _backstoryFullText.Length)
            {
                _backstoryTimer?.Stop();
                return;
            }

            BackstoryText.Text +=
                _backstoryFullText[_backstoryCharacterIndex];

            _backstoryCharacterIndex++;
        }
        private void Backstory_MouseLeftButtonDown(
    object sender,
    MouseButtonEventArgs e)
        {
            if (_backstoryTimer != null &&
                _backstoryTimer.IsEnabled)
            {
                _backstoryTimer.Stop();

                BackstoryText.Text =
                    _backstoryFullText;

                _backstoryCharacterIndex =
                    _backstoryFullText.Length;

                return;
            }

            ShowPlayerRollScreen();
        }
        private void EnterGame()
        {
            BackstoryScreen.Visibility =
                Visibility.Collapsed;

            TitleScreen.Visibility =
                Visibility.Collapsed;

            GameScreen.Visibility =
                Visibility.Visible;

            UpdatePlayerStats();

            ShowScene(Scenes.FrontGate);

            _interactionMode =
                InteractionMode.Look;

            UpdateMenuButtonImages();

            if (_lookCursor != null)
            {
                SceneCanvas.Cursor =
                    _lookCursor;
            }
        }
        /*
         * Refreshes the stats panel in the HUD's right pocket.
         * Call this whenever the player's health, stats, weapon
         * or status effect change.
         */
        private void UpdatePlayerStats()
        {
            int health = Math.Max(0, _player.Health);
            int maxHealth = Math.Max(1, _player.MaxHealth);

            StatHealthText.Text = $"{health}/{maxHealth}";
            StatStrengthText.Text = _player.Strength.ToString();
            StatLuckText.Text = _player.Luck.ToString();
            StatWitsText.Text = _player.Wits.ToString();

            HealthBarFill.Width = new GridLength(health, GridUnitType.Star);
            HealthBarEmpty.Width = new GridLength(maxHealth - Math.Min(health, maxHealth), GridUnitType.Star);

            StatWeaponText.Text = string.IsNullOrWhiteSpace(_player.Weapon) ? "None" : _player.Weapon;

            bool hasStatus = !string.IsNullOrWhiteSpace(_player.StatusEffect) &&
                             _player.StatusEffect != "None";

            StatStatusText.Text = hasStatus ? _player.StatusEffect : "None";
            StatStatusText.Foreground = hasStatus
                ? new SolidColorBrush(Color.FromRgb(0xC8, 0xA9, 0x6A))
                : new SolidColorBrush(Color.FromRgb(0x88, 0x79, 0x66));

            // Anything worth writing down?
            CheckJournal();

            // Died outside a fight (e.g. a trap)?
            if (_player.Health <= 0 && _player.MaxHealth > 0 &&
                !IsInFight && GameScreen.Visibility == Visibility.Visible)
            {
                ShowGameOver("Your adventure ends here.", canRetryFight: false);
            }
        }

        /*
         * Clicking WEAPON in the stats panel puts the weapon away, or takes
         * out the best weapon you're carrying. (Some puzzles care whether
         * you're holding a weapon - e.g. the Faceless Knight.)
         */
        private void StatWeaponText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (IsInFight || IsInDialogue || GameScreen.Visibility != Visibility.Visible)
            {
                return;
            }

            bool armed = !string.IsNullOrWhiteSpace(_player.Weapon) && _player.Weapon != "None";

            if (armed)
            {
                string weapon = _player.Weapon;
                _player.Weapon = "None";
                ShowReply($"You put the {weapon} away.");
            }
            else
            {
                Item? best = _player.Inventory
                    .Select(Items.GetItem)
                    .Where(i => i.WeaponBonus > 0)
                    .OrderByDescending(i => i.WeaponBonus)
                    .FirstOrDefault();

                if (best == null)
                {
                    ShowReply("You have nothing to use as a weapon.");
                    return;
                }

                _player.Weapon = best.Name;
                ShowReply($"You take out the {best.Name}.");
            }

            UpdatePlayerStats();
        }

        // Shows a picture full-screen until the next click.
        private void ShowPicture(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            try
            {
                PictureViewerImage.Source = new BitmapImage(new Uri(
                    $"pack://application:,,,/{imagePath}", UriKind.Absolute));
                PictureViewer.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load picture:\n\n{imagePath}\n\n{ex.Message}",
                    "Picture error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AcceptRollButton_Click(
      object sender,
      RoutedEventArgs e)
        {
            string playerName =
                PlayerNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(playerName))
            {
                RollCommentText1.Text =
                    "Even doomed heroes require a name.";

                PlayerNameTextBox.Focus();

                return;
            }

            _player.Name = playerName;

            PlayerRollScreen.Visibility =
                Visibility.Collapsed;

            // Pick a starting item from the car trunk, then the game begins.
            ShowTrunkScreen();
        }
        private void RollPlayer()
        {
            _player.MaxHealth = _random.Next(70, 101);

            _player.Health = _player.MaxHealth;

            _player.Strength = _random.Next(6, 19);

            _player.Luck = _random.Next(3, 19);

            _player.Wits = _random.Next(5, 19);

            UpdateRollScreen();
        }
        private void UpdateRollScreen()
        {
            RollHealthText.Text =
                $"{_player.Health} / {_player.MaxHealth}";

            RollStrengthText.Text =
                _player.Strength.ToString();

            RollLuckText.Text =
                _player.Luck.ToString();

            RollWitsText.Text =
                _player.Wits.ToString();

            RerollsText.Text =
                $"Rerolls remaining: {_rerollsRemaining}";

            RollCommentText.Text =
                GetRollComment();
        }
        private void ShowPlayerRollScreen()
        {
            BackstoryScreen.Visibility =
                Visibility.Collapsed;

            PlayerRollScreen.Visibility =
                Visibility.Visible;

            _rerollsRemaining = 3;

            RerollButton.IsEnabled = true;

            RollPlayer();
        }
        private string GetRollComment()
        {
            int total =
                _player.Strength +
                _player.Luck +
                _player.Wits;

            if (_player.Strength <= 7)
            {
                return
                    "You possess all the physical menace of damp laundry.";
            }

            if (_player.Luck <= 5)
            {
                return
                    "Fortune does not favour you. In fact, Fortune appears to actively dislike you.";
            }

            if (_player.Wits <= 6)
            {
                return
                    "Thinking may not be your greatest weapon. Fortunately, doors occasionally open when pushed.";
            }

            if (_player.Strength >= 16)
            {
                return
                    "Subtlety is optional when you can simply move the furniture.";
            }

            if (_player.Luck >= 16)
            {
                return
                    "Things have an irritating habit of working out for you.";
            }

            if (_player.Wits >= 16)
            {
                return
                    "You're suspiciously clever for someone entering an obviously haunted mansion.";
            }

            if (total >= 43)
            {
                return
                    "Remarkably competent. Frankly, the mansion deserves better.";
            }

            return
                "Thoroughly average. Perhaps the monsters will underestimate you.";
        }
        private void RerollButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            if (_rerollsRemaining <= 0)
            {
                ShowDescription(
                    "Fate has grown tired of your indecision."
                );

                return;
            }

            _rerollsRemaining--;

            RollPlayer();

            if (_rerollsRemaining == 0)
            {
                RerollButton.IsEnabled = false;
            }
        }


        private void StartNewGame()
        {
            TitleScreen.Visibility = Visibility.Collapsed;
            GameScreen.Visibility = Visibility.Collapsed;

            BackstoryScreen.Visibility = Visibility.Visible;

            _backstoryFullText =
                "Nobody remembers exactly when the mansion was abandoned.\n\n" +

                "Some say its last owner simply disappeared. " +
                "Others insist that the house was never truly empty.\n\n" +

                "Whatever the truth, people in the surrounding countryside " +
                "have learned to keep their distance after sunset.\n\n" +

                "You, unfortunately, have never been especially good " +
                "at taking advice.\n\n" +

                "And tonight, beneath a cold autumn moon, " +
                "you find yourself standing before its gates.";

            BackstoryText.Text = "";

            _backstoryCharacterIndex = 0;

            _backstoryTimer = new DispatcherTimer();

            _backstoryTimer.Interval =
                TimeSpan.FromMilliseconds(25);

            _backstoryTimer.Tick += BackstoryTimer_Tick;

            _backstoryTimer.Start();
        }
       
        private void ReturnToTitleScreen()
        {
            GameScreen.Visibility = Visibility.Collapsed;

            TitleScreen.Visibility = Visibility.Visible;
        }
        private void TitleButton_MouseDown(
    object sender,
    MouseButtonEventArgs e)
        {
            if (sender is not Rectangle button)
            {
                return;
            }

            switch (button.Name)
            {
                case "NewGameBtn":
                    {
                        StartNewGame();
                        break;
                    }

                case "LoadGameBtn":
                    {
                        ShowSaveLoad(saving: false, back: () => { });   // title stays behind
                        break;
                    }

                case "SettingsBtn":
                    {
                        ShowSettings(back: () => { });   // title stays behind
                        break;
                    }

                case "CreditsBtn":
                    {
                    //    ShowCredits();
                        break;
                    }

                case "QuitBtn":
                    {
                        Close();
                        break;
                    }
            }

            e.Handled = true;
        }
    }
}
