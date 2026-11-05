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

        private InteractionMode _interactionMode = InteractionMode.Look;
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

        /*
         * A picture from inside the game, via ImageCache (each picture is read
         * once and reused). If it's missing, says so and returns null.
         * decodeHeight > 0 decodes big art at that height to save memory.
         */
        private static BitmapImage? LoadImage(string path, string what, int decodeHeight = 0)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                return ImageCache.Get(path, decodeHeight);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not load {what}:\n\n{path}\n\n{ex.Message}",
                    $"Missing {what}",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return null;
            }
        }

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
            ApplySoundSettings();

            // The title song plays over the title screen, the backstory, the
            // character roll and the trunk; the first scene's rain replaces it.
            Sound.Ambience("Title_music");
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

        private void BackstoryTimer_Tick(object? sender, EventArgs e)
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

        private void Backstory_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

            if (_backstoryPage < _backstoryPages.Length - 1)
            {
                ShowBackstoryPage(_backstoryPage + 1);
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
            SceneCanvas.Cursor = _lookCursor;
        }
        /*
         * Refreshes the stats panel in the HUD's right pocket.
         * Call this whenever the player's health, stats, weapon
         * or status effect change.
         */
        private static readonly Brush StatusActiveBrush = Paint.Rgb(0xC8, 0xA9, 0x6A);
        private static readonly Brush StatusNoneBrush = Paint.Rgb(0x88, 0x79, 0x66);

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
            StatStatusText.Foreground = hasStatus ? StatusActiveBrush : StatusNoneBrush;

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

            BitmapImage? picture = LoadImage(imagePath, "picture");
            if (picture != null)
            {
                PictureViewerImage.Source = picture;
                PictureViewer.Visibility = Visibility.Visible;
            }
        }

        private void AcceptRollButton_Click(object sender, RoutedEventArgs e)
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
            _player.MaxHealth = Random.Shared.Next(70, 101);
            _player.Health = _player.MaxHealth;
            _player.Strength = Random.Shared.Next(6, 19);
            _player.Luck = Random.Shared.Next(3, 19);
            _player.Wits = Random.Shared.Next(5, 19);

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

        private void RerollButton_Click(object sender, RoutedEventArgs e)
        {
            // (The button is disabled once the rerolls are used up.)
            if (_rerollsRemaining <= 0)
            {
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

            // The backstory is shown a page at a time (it doesn't fit on one
            // screen): each page types itself out, a click shows the rest of
            // the page, the next click turns the page.
            _backstoryPages = new[]
            {
                // Page 1
                string.Join("\n\n",
                    "There was a time when the Mansion was full of ordinary things.",
                    "Smoke rose from its chimneys. Servants crossed the yard behind the kitchens. Vegetables grew in neat rows beneath the garden wall, and candles burned late into the evening inside the Glasshouse.",
                    "People lived here.",
                    "Then, at some point, they didn't."),

                // Page 2
                string.Join("\n\n",
                    "Nobody remembers exactly when the Mansion was abandoned.",
                    "There was no fire. No terrible accident. No final carriage seen fleeing through the gates.",
                    "The gardens simply stopped being tended. The deliveries stopped coming. The gates rusted shut.",
                    "And eventually, people stopped asking why."),

                // Page 3
                string.Join("\n\n",
                    "Years passed.",
                    "Ivy climbed the walls. Paths vanished beneath weeds and autumn leaves. The surrounding countryside learned to ignore the old house on the hill.",
                    "But one thing remained difficult to ignore.",
                    "The lights never went out.",
                    "Even now, on certain nights, warm light appears behind darkened windows. Candles still flicker beyond the rain-streaked glass of the old Glasshouse.",
                    "Nobody goes there to light them. At least, nobody admits to it."),

                // Page 4
                string.Join("\n\n",
                    "And tonight, beneath a cold autumn moon, you are standing before the gates.",
                    "Rain falls quietly through the trees. Beyond the iron bars, the Mansion waits.",
                    "You don't remember the journey here. You don't remember deciding to come.",
                    "You try to remember where you were before this.",
                    "Nothing comes."),

                // Page 5
                string.Join("\n\n",
                    "Then a light appears in an upstairs window.",
                    "Something about the sight troubles you.",
                    "Not because it is frightening.",
                    "Because it is familiar.",
                    "You've seen that window before. You're certain of it.",
                    "You just can't remember when.")
            };

            ShowBackstoryPage(0);
        }

        private string[] _backstoryPages = Array.Empty<string>();
        private int _backstoryPage;

        private void ShowBackstoryPage(int page)
        {
            _backstoryPage = page;
            _backstoryFullText = _backstoryPages[page];
            BackstoryText.Text = "";
            _backstoryCharacterIndex = 0;

            BackstoryContinueText.Text = page < _backstoryPages.Length - 1
                ? $"Click to continue...   ({page + 1}/{_backstoryPages.Length})"
                : "Click to begin...";

            _backstoryTimer?.Stop();
            _backstoryTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(25)
            };

            _backstoryTimer.Tick += BackstoryTimer_Tick;
            _backstoryTimer.Start();
        }
        private void TitleButton_MouseDown(object sender, MouseButtonEventArgs e)
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
