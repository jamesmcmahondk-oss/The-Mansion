using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
        private Cursor _CurrentInteractionCursor;
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

            // Died outside a fight (e.g. a trap)?
            if (_player.Health <= 0 && _player.MaxHealth > 0 &&
                !IsInFight && GameScreen.Visibility == Visibility.Visible)
            {
                ShowGameOver("Your adventure ends here.", canRetryFight: false);
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


        private void ShowDescription(string text)
        {
            DescriptionText.Text = text;
        }
        private void ShowScene(Scene scene)
        {
            _currentScene = scene;

            // .BackgroundWhen(...) can swap the picture after story events.
            _currentBackground = scene.GetBackground(_player);
            LoadSceneBackground(_currentBackground);

            DescriptionText.Text = scene.Description;

            CreateSceneElements(scene);
        }

        private void LoadSceneBackground(string imagePath)
        {
            try
            {
                var imageUri = new Uri(
                    $"pack://application:,,,/{imagePath}",
                    UriKind.Absolute
                );

                RoomBackground.Source = new BitmapImage(imageUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not load scene image:\n\n" +
                    $"{imagePath}\n\n" +
                    $"{ex.Message}",
                    "Scene image error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void CreateSceneElements(Scene scene)
        {
            /*
             * Remove the previous scene's hotspots, characters,
             * and debug selection rectangle.
             */
            SceneCanvas.Children.Clear();

            _selectionRectangle = null;
            _isCreatingHotspot = false;
            _hasDragged = false;

            // Pictures placed in the scene (.Sprite), under the hotspots.
            foreach (SceneSprite sprite in scene.Sprites)
            {
                if (sprite.VisibleWhen != null && !sprite.VisibleWhen(_player))
                {
                    continue;
                }

                Image? spriteImage = CreateSpriteImage(sprite);
                if (spriteImage != null)
                {
                    SceneCanvas.Children.Add(spriteImage);
                }
            }

            foreach (Hotspot hotspot in scene.Hotspots)
            {
                // .OnlyWhen(...) hotspots vanish while their condition is false.
                if (hotspot.VisibleWhen != null &&
                    !hotspot.VisibleWhen(_player))
                {
                    continue;
                }

                Rectangle hotspotRectangle =
                    CreateHotspotRectangle(hotspot);

                SceneCanvas.Children.Add(hotspotRectangle);
            }

            /*
             * Character rendering can be added here next.
             */
        }

        private Image? CreateSpriteImage(SceneSprite sprite)
        {
            try
            {
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(
                        $"pack://application:,,,/{sprite.Image}",
                        UriKind.Absolute)),
                    Stretch = Stretch.Uniform,
                    IsHitTestVisible = false
                };

                Rect screen = ImageToScreen(new Rect(sprite.X, sprite.Y, 0, sprite.Height));
                image.Height = screen.Height;
                Canvas.SetLeft(image, screen.X);
                Canvas.SetTop(image, screen.Y);
                Panel.SetZIndex(image, 5);
                return image;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not load sprite:\n\n{sprite.Image}\n\n{ex.Message}",
                    "Sprite error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return null;
            }
        }

        private Rectangle CreateHotspotRectangle(Hotspot hotspot)
        {

            var hotspotRect = new Rect(
                hotspot.X, hotspot.Y, hotspot.Width, hotspot.Height);

            Rect screenRect = hotspot.ScreenSpace
                ? hotspotRect
                : ImageToScreen(hotspotRect);

            var rectangle = new Rectangle
            {
                Width = screenRect.Width,
                Height = screenRect.Height,

                Tag = hotspot,
                Cursor = _CurrentInteractionCursor
            };

            if (_showHotspotDebug)
            {
                rectangle.Fill = new SolidColorBrush(
                    Color.FromArgb(65, 255, 0, 0)
                );

                rectangle.Stroke = Brushes.Lime;
                rectangle.StrokeThickness = 3;

                rectangle.ToolTip =
                    $"{hotspot.Name}\n" +
                    $"X = {hotspot.X:0}\n" +
                    $"Y = {hotspot.Y:0}\n" +
                    $"Width = {hotspot.Width:0}\n" +
                    $"Height = {hotspot.Height:0}";
            }
            else
            {
                /*
                 * Transparent fill keeps the complete interior
                 * of the rectangle clickable.
                 */
                rectangle.Fill = Brushes.Transparent;
                rectangle.Stroke = null;
                rectangle.ToolTip = null;
            }

            Canvas.SetLeft(rectangle, screenRect.X);
            Canvas.SetTop(rectangle, screenRect.Y);

            Panel.SetZIndex(rectangle, 10);

        
            rectangle.MouseEnter += Hotspot_MouseEnter;
            rectangle.MouseLeave += Hotspot_MouseLeave;
            rectangle.MouseLeftButtonDown += Hotspot_MouseLeftButtonDown;

            return rectangle;
        }

        private void Hotspot_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            /*
             * In debug mode, holding Shift allows the user to draw
             * a new hotspot over an existing hotspot.
             *
             * Without Shift, the existing hotspot remains clickable.
             */
            if (_showHotspotDebug &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                return;
            }

            if (sender is not Rectangle rectangle)
            {
                return;
            }

            if (rectangle.Tag is not Hotspot hotspot)
            {
                return;
            }

            e.Handled = true;

            /*
             * TALK on something that can talk starts a conversation.
             */
            if (_interactionMode == InteractionMode.Talk &&
                !string.IsNullOrWhiteSpace(hotspot.DialogueId))
            {
                StartDialogue(hotspot.DialogueId);
                return;
            }

            /*
             * TALK on something with a plain reply (.OnTalk).
             */
            if (_interactionMode == InteractionMode.Talk &&
                !string.IsNullOrWhiteSpace(hotspot.TalkText))
            {
                ShowDescription(hotspot.TalkText);
                return;
            }

            /*
             * ACT on something with an ACT conversation (e.g. a character
             * who appears when you touch something).
             */
            if (_interactionMode == InteractionMode.Act &&
                !string.IsNullOrWhiteSpace(hotspot.ActDialogueId))
            {
                StartDialogue(hotspot.ActDialogueId);
                return;
            }

            /*
             * ACT on something that starts a fight (.ActStartsFight).
             */
            if (_interactionMode == InteractionMode.Act &&
                !string.IsNullOrWhiteSpace(hotspot.ActFightId))
            {
                StartFight(hotspot.ActFightId);
                return;
            }

            /*
             * ACT on something with a plain reply and/or effect (.OnAct).
             */
            if (_interactionMode == InteractionMode.Act &&
                (!string.IsNullOrWhiteSpace(hotspot.ActText) || hotspot.ActEffect != null || hotspot.ActResult != null))
            {
                if (hotspot.ActResult != null)
                {
                    ShowDescription(hotspot.ActResult(_player));
                    AfterStoryChange();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(hotspot.ActText))
                {
                    ShowDescription(hotspot.ActText);
                }

                hotspot.ActEffect?.Invoke(_player);
                AfterStoryChange();
                return;
            }

            /*
             * ITEM: use the selected item on the hotspot. (Screen UI such as
             * the Return arrow still works normally.)
             */
            if (_interactionMode == InteractionMode.Item &&
                !hotspot.ScreenSpace)
            {
                if (_selectedItem == null)
                {
                    OpenInventory();
                }
                else
                {
                    UseSelectedItemOn(hotspot);
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(hotspot.Description))
            {
                DescriptionText.Text = hotspot.Description;
            }

            if (string.IsNullOrWhiteSpace(hotspot.TargetScene))
            {
                return;
            }

            Scene? targetScene =
                Scenes.GetScene(hotspot.TargetScene);

            if (targetScene == null)
            {
                MessageBox.Show(
                    $"The target scene '{hotspot.TargetScene}' " +
                    $"was not found.\n\n" +
                    $"Hotspot: {hotspot.Name}",
                    "Missing scene",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            ShowScene(targetScene);
        }

        private void SceneCanvas_MouseMove(
            object sender,
            MouseEventArgs e)
        {
            Point currentPosition =
                e.GetPosition(SceneCanvas);

            if (_showHotspotDebug)
            {
                Point imagePosition = ScreenToImage(
                    new Rect(currentPosition, new Size(0, 0))).Location;

                DebugCoordinates.Text =
                    $"Image: X={imagePosition.X:0} " +
                    $"Y={imagePosition.Y:0}  " +
                    $"(screen {currentPosition.X:0},{currentPosition.Y:0})";
            }

            if (!_showHotspotDebug ||
                !_isCreatingHotspot ||
                _selectionRectangle == null)
            {
                return;
            }

            double movementX =
                Math.Abs(currentPosition.X - _dragStartPoint.X);

            double movementY =
                Math.Abs(currentPosition.Y - _dragStartPoint.Y);

            if (movementX >= DragThreshold ||
                movementY >= DragThreshold)
            {
                _hasDragged = true;
            }

            UpdateSelectionRectangle(currentPosition);
        }

        private void SceneCanvas_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!_showHotspotDebug)
            {
                return;
            }

            /*
             * If the mouse is over an existing hotspot, require Shift
             * to start drawing a new selection over it.
             */
            if (e.OriginalSource is Rectangle existingRectangle &&
                existingRectangle.Tag is Hotspot &&
                !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                return;
            }

            _dragStartPoint =
                ClampPointToCanvas(
                    e.GetPosition(SceneCanvas)
                );

            _isCreatingHotspot = true;
            _hasDragged = false;

            RemoveSelectionRectangle();

            _selectionRectangle = new Rectangle
            {
                Width = 0,
                Height = 0,

                Fill = new SolidColorBrush(
                    Color.FromArgb(80, 0, 200, 255)
                ),

                Stroke = Brushes.Cyan,
                StrokeThickness = 3,

                StrokeDashArray =
                    new DoubleCollection
                    {
                        5,
                        3
                    },

                IsHitTestVisible = false
            };

            Canvas.SetLeft(
                _selectionRectangle,
                _dragStartPoint.X
            );

            Canvas.SetTop(
                _selectionRectangle,
                _dragStartPoint.Y
            );

            Panel.SetZIndex(
                _selectionRectangle,
                100
            );

            SceneCanvas.Children.Add(
                _selectionRectangle
            );

            SceneCanvas.CaptureMouse();

            DebugSelection.Text =
                $"Start: X={_dragStartPoint.X:0} " +
                $"Y={_dragStartPoint.Y:0}";

            e.Handled = true;
        }

        private void SceneCanvas_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!_showHotspotDebug ||
                !_isCreatingHotspot ||
                _selectionRectangle == null)
            {
                return;
            }

            Point endPoint =
                ClampPointToCanvas(
                    e.GetPosition(SceneCanvas)
                );

            UpdateSelectionRectangle(endPoint);

            _isCreatingHotspot = false;

            SceneCanvas.ReleaseMouseCapture();

            if (!_hasDragged)
            {
                RemoveSelectionRectangle();

                DebugSelection.Text =
                    "Drag to create a hotspot";

                return;
            }

            double x =
                Canvas.GetLeft(_selectionRectangle);

            double y =
                Canvas.GetTop(_selectionRectangle);

            double width =
                _selectionRectangle.Width;

            double height =
                _selectionRectangle.Height;

            /*
             * Scenes.cs stores hotspots in source-image pixels,
             * so convert the on-screen selection first.
             */
            Rect imageRect = ScreenToImage(
                new Rect(x, y, width, height));

            x = imageRect.X;
            y = imageRect.Y;
            width = imageRect.Width;
            height = imageRect.Height;

            /*
             * Round the coordinates because whole-number positions
             * are easier to use in Scenes.cs.
             */
            x = Math.Round(x);
            y = Math.Round(y);
            width = Math.Round(width);
            height = Math.Round(height);

            string hotspotCode =
                CreateHotspotCode(
                    x,
                    y,
                    width,
                    height
                );

            DebugSelection.Text =
                $"Selection: X={x:0} Y={y:0} " +
                $"W={width:0} H={height:0}\n" +
                "Hotspot code copied to clipboard.";

            try
            {
                Clipboard.SetText(hotspotCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"The hotspot was created, but its code " +
                    $"could not be copied to the clipboard.\n\n" +
                    $"{ex.Message}",
                    "Clipboard error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            e.Handled = true;
        }

        private void UpdateSelectionRectangle(
            Point currentPosition)
        {
            if (_selectionRectangle == null)
            {
                return;
            }

            currentPosition =
                ClampPointToCanvas(currentPosition);

            double x =
                Math.Min(
                    _dragStartPoint.X,
                    currentPosition.X
                );

            double y =
                Math.Min(
                    _dragStartPoint.Y,
                    currentPosition.Y
                );

            double width =
                Math.Abs(
                    currentPosition.X -
                    _dragStartPoint.X
                );

            double height =
                Math.Abs(
                    currentPosition.Y -
                    _dragStartPoint.Y
                );

            Canvas.SetLeft(
                _selectionRectangle,
                x
            );

            Canvas.SetTop(
                _selectionRectangle,
                y
            );

            _selectionRectangle.Width = width;
            _selectionRectangle.Height = height;

            DebugSelection.Text =
                $"Selection: X={x:0} Y={y:0} " +
                $"W={width:0} H={height:0}";
        }

        private Point ClampPointToCanvas(Point point)
        {
            double x = Math.Max(
                0,
                Math.Min(
                    point.X,
                    SceneCanvas.ActualWidth
                )
            );

            double y = Math.Max(
                0,
                Math.Min(
                    point.Y,
                    SceneCanvas.ActualHeight
                )
            );

            return new Point(x, y);
        }

        private string CreateHotspotCode(
            double x,
            double y,
            double width,
            double height)
        {
            /*
             * InvariantCulture ensures periods are used as decimal
             * separators regardless of Windows regional settings.
             */
            string xText =
                x.ToString(
                    "0",
                    CultureInfo.InvariantCulture
                );

            string yText =
                y.ToString(
                    "0",
                    CultureInfo.InvariantCulture
                );

            string widthText =
                width.ToString(
                    "0",
                    CultureInfo.InvariantCulture
                );

            string heightText =
                height.ToString(
                    "0",
                    CultureInfo.InvariantCulture
                );

            // Paste into a scene in Data/Scenes.cs (see SCENE_GUIDE.md).
            return
$@"            .Hotspot(""New Hotspot"", {xText}, {yText}, {widthText}, {heightText})
                .Describe(""Describe the hotspot here."")";
        }

        private void RemoveSelectionRectangle()
        {
            if (_selectionRectangle == null)
            {
                return;
            }

            SceneCanvas.Children.Remove(
                _selectionRectangle
            );

            _selectionRectangle = null;
        }

        private void UpdateDebugVisibility()
        {
            DebugPanel.Visibility =
                _showHotspotDebug
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void LookBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            var imagePath = "Assets/UI/look_glow.png";

            try
            {
                var imageUri = new Uri(
                    $"pack://application:,,,/{imagePath}",
                    UriKind.Absolute
                );

                LookBtn.Source = new BitmapImage(imageUri);
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not load scene image:\n\n" +
                    $"{imagePath}\n\n" +
                    $"{ex.Message}",
                    "Scene image error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

        }

      
        private void MenuButton_MouseEnter(object sender, MouseEventArgs e)
        {

            if (sender is not Image buttonImage)
            {
                return;
            }

            string imagePath = buttonImage.Source.ToString();
            string buttonName = buttonImage.Name;

            switch (buttonName)
            {
                case "LookBtn":
                    {
                         imagePath = "Assets/UI/look_glow.png";
                        break;
                    }

                case "TalkBtn":
                    {
                         imagePath = "Assets/UI/talk_glow.png";
                        break;
                    }

                case "ActBtn":
                    {
                         imagePath = "Assets/UI/act_glow.png";
                        break;
                    }

                case "ItemBtn":
                    {
                         imagePath = "Assets/UI/item_glow.png";
                        break;
                    }
            }
            try
            {
                var imageUri = new Uri(
                    $"pack://application:,,,/{imagePath}",
                    UriKind.Absolute
                );

                buttonImage.Source = new BitmapImage(imageUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not load button image:\n\n" +
                    $"{imagePath}\n\n" +
                    $"{ex.Message}",
                    "button image error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            e.Handled = true;
        }
        private void SetButtonImage(Image buttonImage, string imagePath)
        {
            try
            {
                var imageUri = new Uri(
                    $"pack://application:,,,/{imagePath}",
                    UriKind.Absolute
                );

                buttonImage.Source = new BitmapImage(imageUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not load button image:\n\n" +
                    $"{imagePath}\n\n" +
                    $"{ex.Message}",
                    "Button image error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        private void UpdateMenuButtonImages()
        {
            SetButtonImage(
                LookBtn,
                _interactionMode == InteractionMode.Look
                    ? "Assets/UI/look_glow.png"
                    : "Assets/UI/look.png"
            );

            SetButtonImage(
                TalkBtn,
                _interactionMode == InteractionMode.Talk
                    ? "Assets/UI/talk_glow.png"
                    : "Assets/UI/talk.png"
            );

            SetButtonImage(
                ActBtn,
                _interactionMode == InteractionMode.Act
                    ? "Assets/UI/Act_glow.png"
                    : "Assets/UI/Act.png"
            );

            SetButtonImage(
                ItemBtn,
                _interactionMode == InteractionMode.Item
                    ? "Assets/UI/item_glow.png"
                    : "Assets/UI/item.png"
            );
        }

        private void MenuButton_MouseLeave(object sender, MouseEventArgs e)
        {

            UpdateMenuButtonImages();
            e.Handled = true;

        }

        private void MenuButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Image buttonImage)
            {
                return;
            }
            string buttonName = buttonImage.Name;

            // Picking another verb closes the inventory and drops the item.
            if (buttonName != "ItemBtn")
            {
                CloseInventory();
                _selectedItem = null;
            }

            switch (buttonName)
            {
                case "LookBtn":
                    {
                        _interactionMode = InteractionMode.Look;
                        SceneCanvas.Cursor = _lookCursor;
                     
                        ShowDescription("Look at something...");
                        break;
                    }

                case "TalkBtn":
                    {
                        _interactionMode = InteractionMode.Talk;
                        SceneCanvas.Cursor = _talkCursor;

                        ShowDescription("Talk to someone...");
                        break;
                    }

                case "ActBtn":
                    {
                        _interactionMode = InteractionMode.Act;
                        SceneCanvas.Cursor = _actCursor;

                        ShowDescription("Interact with something...");
                        break;
                    }

                case "ItemBtn":
                    {
                        // During a conversation ITEM offers something to the character.
                        if (IsInDialogue)
                        {
                            if (_inventoryOpen)
                            {
                                CloseInventory();
                            }
                            else
                            {
                                OpenInventory();
                            }
                            break;
                        }

                        _interactionMode = InteractionMode.Item;
                        SceneCanvas.Cursor = _itemCursor;

                        // ITEM again while the inventory is open closes it.
                        if (_inventoryOpen)
                        {
                            CloseInventory();
                        }
                        else
                        {
                            OpenInventory();
                        }
                        break;
                    }
            }

            e.Handled = true;
        }
        private void Hotspot_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is not Rectangle rectangle)
            {
                return;
            }

            if (rectangle.Tag is not Hotspot hotspot)
            {
                return;
            }

            switch (_interactionMode)
            {
                case InteractionMode.Look:
                    ShowDescription($"Look at {hotspot.Name}");
                    break;

                case InteractionMode.Talk:
                    ShowDescription($"Talk to {hotspot.Name}");
                    break;

                case InteractionMode.Act:
                    ShowDescription($"Use {hotspot.Name}");
                    break;

                case InteractionMode.Item:
                    ShowDescription(_selectedItem != null
                        ? $"Use {_selectedItem} on {hotspot.Name}"
                        : $"Use item on {hotspot.Name}");
                    break;
            }
        }

        private void Hotspot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_interactionMode == InteractionMode.Item && _selectedItem != null)
            {
                ShowDescription($"Use {_selectedItem} on...");
            }
            else if (_currentScene != null)
            {
                ShowDescription(_currentScene.Description);
            }
        }

        // =========================================================
        // TALK DIALOG
        // =========================================================

        private Dialogue? _activeDialogue;

        // Choices currently on screen, in the order they are numbered.
        private readonly List<DialogueChoice> _visibleChoices = new();

        private static readonly Brush ChoiceBrush =
            new SolidColorBrush(Color.FromRgb(0xB9, 0xA9, 0x8D));

        private static readonly Brush ChoiceHoverBrush =
            new SolidColorBrush(Color.FromRgb(0xFF, 0xD9, 0x8A));

        private bool IsInDialogue => _activeDialogue != null;

        private void StartDialogue(string dialogueId)
        {
            Dialogue? dialogue = Dialogues.GetDialogue(dialogueId);

            if (dialogue == null)
            {
                MessageBox.Show(
                    $"The dialogue '{dialogueId}' was not found in Data/Dialogues.cs.",
                    "Missing dialogue",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _activeDialogue = dialogue;

            PlaceDialogueCharacter(dialogue);
            SetDialogueCharacter(dialogue.Character);
            SetCloseUpPortrait(dialogue.Portrait);

            // The dim layer also blocks clicks on hotspots while talking,
            // so it is always shown - just see-through when not dimming.
            DialogueDim.Fill = dialogue.ShouldDimScene
                ? new SolidColorBrush(Color.FromArgb(0x8C, 0, 0, 0))
                : Brushes.Transparent;
            DialogueDim.Visibility = Visibility.Visible;
            DescriptionText.Visibility = Visibility.Collapsed;
            DialoguePanel.Visibility = Visibility.Visible;

            // No switching verbs mid-conversation - except ITEM, to offer things.
            SetMenuForDialogue(true);

            string startNode = dialogue.ChooseStartNode != null
                ? dialogue.ChooseStartNode(_player)
                : dialogue.StartNode;

            // "You have now met this character" - set after the start line is
            // picked, so a first meeting still starts at the first-meeting line.
            if (!string.IsNullOrWhiteSpace(dialogue.MetFlag))
            {
                _player.SetFlag(dialogue.MetFlag);
            }

            ShowDialogueNode(startNode);
        }

        /*
         * Default: standing on the right of the scene, 700px tall.
         * If the dialogue gives CharacterHeight (.StandAt), the character is
         * placed in the scene at CharacterX/Y (background image pixels).
         */
        private void PlaceDialogueCharacter(Dialogue dialogue)
        {
            PlaceCharacter(dialogue.CharacterX, dialogue.CharacterY, dialogue.CharacterHeight);
        }

        private string _shownCharacter = "";

        private void SetDialogueCharacter(string imagePath)
        {
            if (imagePath == _shownCharacter &&
                DialogueCharacter.Source != null)
            {
                return; // already showing it
            }

            _shownCharacter = imagePath;
            DialogueCharacter.Source = null;

            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                try
                {
                    DialogueCharacter.Source = new BitmapImage(new Uri(
                        $"pack://application:,,,/{imagePath}",
                        UriKind.Absolute));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Could not load portrait:\n\n{imagePath}\n\n{ex.Message}",
                        "Portrait error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            DialogueCharacter.Visibility =
                DialogueCharacter.Source != null ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowDialogueNode(string nodeId)
        {
            if (_activeDialogue == null)
            {
                return;
            }

            if (!_activeDialogue.Nodes.TryGetValue(nodeId, out DialogueNode? node))
            {
                MessageBox.Show(
                    $"Dialogue node '{nodeId}' was not found.",
                    "Missing dialogue node",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                EndDialogue();
                return;
            }

            _currentLineId = nodeId;
            DisplayDialogueNode(node);
        }

        // Shows a line and its choices. Also used for one-off replies that
        // aren't part of the script (e.g. refusing an offered item).
        private void DisplayDialogueNode(DialogueNode node)
        {
            if (_activeDialogue == null)
            {
                return;
            }

            DialogueItemHint.Visibility = _player.Inventory.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            // A line can swap the picture (e.g. an angry pose).
            SetDialogueCharacter(string.IsNullOrWhiteSpace(node.Character)
                ? _activeDialogue.Character
                : node.Character);

            SetCloseUpPortrait(string.IsNullOrWhiteSpace(node.Portrait)
                ? _activeDialogue.Portrait
                : node.Portrait);

            DialogueSpeakerText.Text = _activeDialogue.Speaker.ToUpperInvariant();
            DialogueLineText.Text = node.Text;

            _visibleChoices.Clear();

            foreach (DialogueChoice choice in node.Choices)
            {
                if (choice.Condition == null || choice.Condition(_player))
                {
                    _visibleChoices.Add(choice);
                }

                if (_visibleChoices.Count == 5)
                {
                    break; // only 5 fit in the panel
                }
            }

            if (_visibleChoices.Count == 0)
            {
                _visibleChoices.Add(new DialogueChoice { Text = "(Leave)", Next = null });
            }

            DialogueChoicesPanel.Children.Clear();

            for (int i = 0; i < _visibleChoices.Count; i++)
            {
                int index = i;

                var choiceText = new TextBlock
                {
                    Text = $"{i + 1}.  {_visibleChoices[i].Text}",
                    Foreground = ChoiceBrush,
                    FontFamily = new FontFamily("Georgia"),
                    FontSize = 16,
                    Margin = new Thickness(12, 0, 0, 4),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Cursor = Cursors.Hand,
                    Background = Brushes.Transparent
                };

                choiceText.MouseEnter += (_, _) => choiceText.Foreground = ChoiceHoverBrush;
                choiceText.MouseLeave += (_, _) => choiceText.Foreground = ChoiceBrush;
                choiceText.MouseLeftButtonDown += (_, args) =>
                {
                    args.Handled = true;
                    ChooseDialogueOption(index);
                };

                DialogueChoicesPanel.Children.Add(choiceText);
            }
        }

        private void ChooseDialogueOption(int index)
        {
            if (_activeDialogue == null ||
                index < 0 ||
                index >= _visibleChoices.Count)
            {
                return;
            }

            DialogueChoice choice = _visibleChoices[index];

            if (choice.Effect != null)
            {
                choice.Effect(_player);
                UpdatePlayerStats();
            }

            // "Draw your weapon!" - end the conversation and fight.
            if (!string.IsNullOrWhiteSpace(choice.StartsFight))
            {
                StartFight(choice.StartsFight);
                return;
            }

            if (string.IsNullOrWhiteSpace(choice.Next))
            {
                EndDialogue();
            }
            else
            {
                ShowDialogueNode(choice.Next);
            }
        }

        private void EndDialogue()
        {
            if (_inventoryOpen)
            {
                InventoryPanel.Visibility = Visibility.Collapsed;
                _inventoryOpen = false;
            }

            _activeDialogue = null;
            _visibleChoices.Clear();

            DialogueChoicesPanel.Children.Clear();
            DialoguePanel.Visibility = Visibility.Collapsed;
            DialogueCharacter.Visibility = Visibility.Collapsed;
            DialogueCharacter.Source = null;
            _shownCharacter = "";
            HideCloseUpPortrait();
            DialogueDim.Visibility = Visibility.Collapsed;

            DescriptionText.Visibility = Visibility.Visible;

            SetMenuForDialogue(false);

            // Choices may have changed flags/items, so hotspots may change.
            RefreshSceneHotspots();

            if (_currentScene != null)
            {
                ShowDescription(_currentScene.Description);
            }
        }

        // Number keys 1-4 pick dialogue choices.
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
            if (IsGameMenuOpen || IsSaveLoadOpen || IsSettingsOpen)
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
                _ => -1
            };

            if (index >= 0)
            {
                ChooseDialogueOption(index);
                e.Handled = true;
            }
        }

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

        // =========================================================
        // INVENTORY
        // =========================================================

        private bool _inventoryOpen;

        // Item picked from the inventory, waiting to be used on something.
        private string? _selectedItem;

        private static readonly Brush SlotBorderBrush =
            new SolidColorBrush(Color.FromRgb(0x80, 0x6B, 0x4A));

        private static readonly Brush SlotHoverBrush =
            new SolidColorBrush(Color.FromRgb(0xFF, 0xD9, 0x8A));

        private void OpenInventory()
        {
            InventorySlots.Children.Clear();

            // Same item twice? Show it once.
            var itemNames = new List<string>();
            foreach (string name in _player.Inventory)
            {
                if (!itemNames.Contains(name))
                {
                    itemNames.Add(name);
                }
            }

            foreach (string name in itemNames)
            {
                InventorySlots.Children.Add(CreateInventorySlot(Items.GetItem(name)));

                if (InventorySlots.Children.Count == 10)
                {
                    break; // 10 slots fit in the panel
                }
            }

            InventoryDescriptionText.Text = itemNames.Count == 0
                ? "Your pockets are empty."
                : "Hover over an item to examine it.";

            if (IsInFight)
            {
                // Using something in the fight: inventory replaces the dice panel.
                FightPanel.Visibility = Visibility.Collapsed;
                InventoryHintText.Text = "Click an item to use it in the fight  ·  Esc to go back";
            }
            else if (IsInDialogue && _activeDialogue != null)
            {
                // Offering something to the character: inventory replaces the conversation.
                DialoguePanel.Visibility = Visibility.Collapsed;
                InventoryHintText.Text =
                    $"Click an item to offer it to {_activeDialogue.Speaker}  ·  Esc to go back";
            }
            else
            {
                InventoryHintText.Text = "Click an item to use it  ·  Esc to close";
            }

            DescriptionText.Visibility = Visibility.Collapsed;
            InventoryPanel.Visibility = Visibility.Visible;
            _inventoryOpen = true;
        }

        private void CloseInventory()
        {
            if (!_inventoryOpen)
            {
                return;
            }

            InventoryPanel.Visibility = Visibility.Collapsed;
            _inventoryOpen = false;

            // Back to the fight.
            if (IsInFight)
            {
                FightPanel.Visibility = Visibility.Visible;
                return;
            }

            // Back to the conversation.
            if (IsInDialogue)
            {
                DialoguePanel.Visibility = Visibility.Visible;
                return;
            }

            DescriptionText.Visibility = Visibility.Visible;

            if (_selectedItem == null && _currentScene != null)
            {
                ShowDescription(_currentScene.Description);
            }
        }

        private Border CreateInventorySlot(Item item)
        {
            UIElement content;

            BitmapImage? icon = null;
            if (!string.IsNullOrWhiteSpace(item.Icon))
            {
                try
                {
                    icon = new BitmapImage(new Uri(
                        $"pack://application:,,,/{item.Icon}",
                        UriKind.Absolute));
                }
                catch
                {
                    icon = null; // fall back to initials
                }
            }

            if (icon != null)
            {
                content = new Image
                {
                    Source = icon,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(4)
                };
            }
            else
            {
                content = new TextBlock
                {
                    Text = GetInitials(item.Name),
                    Foreground = SlotHoverBrush,
                    FontFamily = new FontFamily("Georgia"),
                    FontSize = 34,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            var slot = new Border
            {
                Width = 116,
                Height = 116,
                Margin = new Thickness(0, 0, 12, 0),
                Background = new SolidColorBrush(Color.FromArgb(0x99, 0, 0, 0)),
                BorderBrush = SlotBorderBrush,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(6),
                Cursor = Cursors.Hand,
                ToolTip = item.Name,
                Child = content
            };

            slot.MouseEnter += (_, _) =>
            {
                slot.BorderBrush = SlotHoverBrush;
                InventoryDescriptionText.Text = string.IsNullOrWhiteSpace(item.Description)
                    ? item.Name
                    : $"{item.Name}: {item.Description}";
            };

            slot.MouseLeave += (_, _) => slot.BorderBrush = SlotBorderBrush;

            slot.MouseLeftButtonDown += (_, args) =>
            {
                args.Handled = true;
                SelectItem(item.Name);
            };

            return slot;
        }

        private static string GetInitials(string name)
        {
            string initials = "";
            foreach (string word in name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                initials += char.ToUpperInvariant(word[0]);
                if (initials.Length == 2)
                {
                    break;
                }
            }
            return initials;
        }

        private void SelectItem(string itemName)
        {
            if (IsInFight)
            {
                CloseInventory();
                UseItemInFight(itemName);
                return;
            }

            if (IsInDialogue)
            {
                CloseInventory();
                OfferItemInDialogue(itemName);
                return;
            }

            _selectedItem = itemName;
            _interactionMode = InteractionMode.Item;
            SceneCanvas.Cursor = _itemCursor;

            CloseInventory();
            ShowDescription($"Use {itemName} on...");
        }

        private void UseSelectedItemOn(Hotspot hotspot)
        {
            if (_selectedItem == null)
            {
                return;
            }

            string itemName = _selectedItem;

            if (!hotspot.ItemUses.TryGetValue(itemName, out ItemUse? use))
            {
                ShowDescription($"You try the {itemName} on the {hotspot.Name}. Nothing happens.");
                return;
            }

            ShowDescription(use.Text);

            if (use.Effect != null)
            {
                use.Effect(_player);
            }

            if (use.RemoveItem)
            {
                _player.Inventory.Remove(itemName);
                _selectedItem = null;
            }

            AfterStoryChange();

            if (!string.IsNullOrWhiteSpace(use.DialogueId))
            {
                StartDialogue(use.DialogueId);
            }
        }

        private void DebugGiveAllItems_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in Items.All)
            {
                if (!_player.Inventory.Contains(item.Name))
                {
                    _player.Inventory.Add(item.Name);
                }
            }

            UpdatePlayerStats();
            DebugMenu.Visibility = Visibility.Collapsed;
        }

        /*
         * Call after anything that can change the player's stats, items or
         * story flags: refreshes the stats panel and re-checks which
         * hotspots should exist (.OnlyWhen).
         */
        private void AfterStoryChange()
        {
            UpdatePlayerStats();
            RefreshSceneHotspots();
        }

        // Background currently on screen (so it's only reloaded when it changes).
        private string _currentBackground = "";

        private void RefreshSceneHotspots()
        {
            if (_currentScene == null)
            {
                return;
            }

            // Something happened that changes the picture? (.BackgroundWhen)
            string background = _currentScene.GetBackground(_player);

            if (background != _currentBackground)
            {
                _currentBackground = background;
                LoadSceneBackground(background);
            }

            CreateSceneElements(_currentScene);
        }

        // =========================================================
        // ITEMS DURING CONVERSATIONS
        // =========================================================

        // Id of the line currently showing (so "(Back)" can return to it).
        private string _currentLineId = "";

        private void SetMenuForDialogue(bool inDialogue)
        {
            foreach (Image button in new[] { ActBtn, LookBtn, TalkBtn })
            {
                button.IsHitTestVisible = !inDialogue;
                button.Opacity = inDialogue ? 0.45 : 1;
            }

            // ITEM stays usable so the player can offer things.
            ItemBtn.IsHitTestVisible = true;
            ItemBtn.Opacity = 1;
        }

        private void OfferItemInDialogue(string itemName)
        {
            if (_activeDialogue == null)
            {
                return;
            }

            if (_activeDialogue.ItemUses.TryGetValue(itemName, out DialogueItemUse? use) &&
                (use.Condition == null || use.Condition(_player)))
            {
                use.Effect?.Invoke(_player);

                if (use.RemoveItem)
                {
                    _player.Take(itemName);
                }

                UpdatePlayerStats();

                if (string.IsNullOrWhiteSpace(use.GoesTo))
                {
                    EndDialogue();
                }
                else
                {
                    ShowDialogueNode(use.GoesTo);
                }

                return;
            }

            // Not interested: say so, then go back to the same line.
            string reply = string.IsNullOrWhiteSpace(_activeDialogue.OtherItemText)
                ? $"{_activeDialogue.Speaker} shows no interest in the {itemName}."
                : _activeDialogue.OtherItemText.Replace("{item}", itemName);

            DisplayDialogueNode(new DialogueNode
            {
                Text = reply,
                Choices =
                {
                    new DialogueChoice { Text = "(Back)", Next = _currentLineId }
                }
            });
        }

        // =========================================================
        // CAR TRUNK SCREEN (pick one starting item - Data/Trunk.cs)
        // =========================================================

        private TrunkItem? _selectedTrunkItem;
        private Border? _selectedTrunkOutline;

        private static readonly Brush TrunkHoverBorder =
            new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xD9, 0x8A));

        private static readonly Brush TrunkHoverFill =
            new SolidColorBrush(Color.FromArgb(0x1A, 0xFF, 0xD9, 0x8A));

        private static readonly Brush TrunkSelectedBorder =
            new SolidColorBrush(Color.FromRgb(0xFF, 0xD9, 0x8A));

        private static readonly Brush TrunkSelectedFill =
            new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xD9, 0x8A));

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

        // =========================================================
        // FIGHTS (Fighting Fantasy style - Data/Enemies.cs)
        // =========================================================

        private Fight? _fight;
        private bool _fightOver;
        private bool _diceRolling;
        private DispatcherTimer? _diceTimer;

        // Player state when the fight started, for "Try the fight again".
        private PlayerSnapshot? _fightSnapshot;
        private string _lastFightId = "";

        private bool IsInFight => _fight != null;

        private const int HitDamage = 10;
        private const int CriticalDamage = 20;

        private sealed class PlayerSnapshot
        {
            public int Health, Strength, Luck, Wits;
            public string Weapon = "", StatusEffect = "";
            public List<string> Inventory = new();
            public HashSet<string> Flags = new();
        }

        private PlayerSnapshot TakeSnapshot() => new()
        {
            Health = _player.Health,
            Strength = _player.Strength,
            Luck = _player.Luck,
            Wits = _player.Wits,
            Weapon = _player.Weapon,
            StatusEffect = _player.StatusEffect,
            Inventory = new List<string>(_player.Inventory),
            Flags = new HashSet<string>(_player.Flags, StringComparer.OrdinalIgnoreCase)
        };

        private void RestoreSnapshot(PlayerSnapshot snapshot)
        {
            _player.Health = snapshot.Health;
            _player.Strength = snapshot.Strength;
            _player.Luck = snapshot.Luck;
            _player.Wits = snapshot.Wits;
            _player.Weapon = snapshot.Weapon;
            _player.StatusEffect = snapshot.StatusEffect;
            _player.Inventory = new List<string>(snapshot.Inventory);
            _player.Flags = new HashSet<string>(snapshot.Flags, StringComparer.OrdinalIgnoreCase);
        }

        // Strength / 2 plus the weapon's bonus (no weapon = -1).
        private int PlayerAttackBonus(out string detail)
        {
            int strength = _player.Strength / 2;
            int weapon = string.IsNullOrWhiteSpace(_player.Weapon) || _player.Weapon == "None"
                ? -1
                : Items.GetItem(_player.Weapon).WeaponBonus;

            detail = $"STR {strength} {(weapon >= 0 ? "+" : "-")} {Math.Abs(weapon)} weapon";
            return strength + weapon;
        }

        private void StartFight(string enemyId)
        {
            Enemy? enemy = Enemies.GetEnemy(enemyId);

            if (enemy == null)
            {
                MessageBox.Show(
                    $"The enemy '{enemyId}' was not found in Data/Enemies.cs.",
                    "Missing enemy",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (IsInDialogue)
            {
                EndDialogue();
            }

            CloseInventory();
            _selectedItem = null;

            _fight = new Fight(enemy);
            _fightOver = false;
            _diceRolling = false;
            _fightSnapshot = TakeSnapshot();
            _lastFightId = enemyId;

            // Enemy picture (same placement rules as dialogue portraits).
            PlaceCharacter(enemy.CharacterX, enemy.CharacterY, enemy.CharacterHeight);
            SetDialogueCharacter(enemy.Character);
            SetCloseUpPortrait(enemy.Portrait);

            DialogueDim.Fill = enemy.ShouldDimScene
                ? new SolidColorBrush(Color.FromArgb(0x8C, 0, 0, 0))
                : Brushes.Transparent;
            DialogueDim.Visibility = Visibility.Visible;

            DescriptionText.Visibility = Visibility.Collapsed;
            DialoguePanel.Visibility = Visibility.Collapsed;
            FightPanel.Visibility = Visibility.Visible;

            MenuButtons.Visibility = Visibility.Collapsed;
            FightButtons.Visibility = Visibility.Visible;
            SetFightButtonsEnabled(true);

            FightTitleText.Text = $"FIGHT!  {enemy.Name.ToUpperInvariant()}";
            EnemyLabelText.Text = enemy.Name.ToUpperInvariant();

            foreach (Image die in new[] { PlayerDie1, PlayerDie2, EnemyDie1, EnemyDie2 })
            {
                SetDieFace(die, 6);
                die.Opacity = 0.35;
            }

            PlayerAttackText.Text = "-";
            EnemyAttackText.Text = "-";
            PlayerAttackBonus(out string detail);
            PlayerAttackDetail.Text = $"2d6 + {detail}";
            EnemyAttackDetail.Text = $"2d6 + skill {enemy.Skill}";

            FightResultText.Text = string.IsNullOrWhiteSpace(enemy.IntroText)
                ? $"{enemy.Name} attacks! Roll the bones..."
                : enemy.IntroText;

            UpdateEnemyHealth();
            UpdatePlayerStats();
        }

        private void UpdateEnemyHealth()
        {
            if (_fight == null)
            {
                return;
            }

            int health = Math.Max(0, _fight.EnemyHealth);
            int max = Math.Max(1, _fight.Enemy.Health);

            EnemyHealthText.Text = $"{health}/{max}";
            EnemyHealthFill.Width = new GridLength(health, GridUnitType.Star);
            EnemyHealthEmpty.Width = new GridLength(max - Math.Min(health, max), GridUnitType.Star);
        }

        private void SetFightButtonsEnabled(bool enabled)
        {
            foreach (Image button in new[] { AttackBtn, FightItemBtn, FleeBtn })
            {
                button.IsHitTestVisible = enabled;
                button.Opacity = enabled ? 1 : 0.45;
            }
        }

        private static readonly Dictionary<string, (string Normal, string Glow)> FightButtonImages = new()
        {
            ["AttackBtn"] = ("Assets/UI/attack.png", "Assets/UI/attack_glow.png"),
            ["FightItemBtn"] = ("Assets/UI/item.png", "Assets/UI/item_glow.png"),
            ["FleeBtn"] = ("Assets/UI/flee.png", "Assets/UI/flee_glow.png")
        };

        private void FightButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Image button && FightButtonImages.TryGetValue(button.Name, out var images))
            {
                SetButtonImage(button, images.Glow);
            }
        }

        private void FightButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Image button && FightButtonImages.TryGetValue(button.Name, out var images))
            {
                SetButtonImage(button, images.Normal);
            }
        }

        private void FightButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (_fight == null || _fightOver || _diceRolling || sender is not Image button)
            {
                return;
            }

            switch (button.Name)
            {
                case "AttackBtn":
                    RollFightRound(playerCanHit: true, prefix: "");
                    break;

                case "FightItemBtn":
                    if (_inventoryOpen)
                    {
                        CloseInventory();
                    }
                    else
                    {
                        OpenInventory();
                    }
                    break;

                case "FleeBtn":
                    Flee();
                    break;
            }
        }

        // ---- Dice --------------------------------------------------------

        private static readonly BitmapImage[] DieFaces = LoadDieFaces();

        private static BitmapImage[] LoadDieFaces()
        {
            var faces = new BitmapImage[7];
            for (int n = 1; n <= 6; n++)
            {
                faces[n] = new BitmapImage(new Uri(
                    $"pack://application:,,,/Assets/UI/Dice/die_{n}.png",
                    UriKind.Absolute));
            }
            return faces;
        }

        private static void SetDieFace(Image die, int value) => die.Source = DieFaces[value];

        private static void SetDieAngle(Image die, double angle)
        {
            if (die.RenderTransform is RotateTransform rotate)
            {
                rotate.Angle = angle;
            }
        }

        /*
         * One round: roll everyone's dice with a short tumbling animation,
         * then work out who hits. playerCanHit = false is a free attack for
         * the enemy (after using an item): winning the roll just dodges.
         */
        private void RollFightRound(bool playerCanHit, string prefix)
        {
            if (_fight == null)
            {
                return;
            }

            _diceRolling = true;
            SetFightButtonsEnabled(false);

            int p1 = Dice.D6(), p2 = Dice.D6(), e1 = Dice.D6(), e2 = Dice.D6();

            Image[] dice = { PlayerDie1, PlayerDie2, EnemyDie1, EnemyDie2 };
            var random = new Random();
            int ticks = 0;
            const int totalTicks = 9;    // ~70 ms each = ~0.65 s

            foreach (Image die in dice)
            {
                die.Opacity = 1;
            }

            PlayerAttackText.Text = "...";
            EnemyAttackText.Text = "...";
            FightResultText.Text = string.IsNullOrEmpty(prefix) ? "The bones tumble..." : prefix;

            _diceTimer?.Stop();
            _diceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(70) };
            _diceTimer.Tick += (_, _) =>
            {
                ticks++;

                if (ticks < totalTicks)
                {
                    // Tumbling: random faces, shaking angles (calming down).
                    double wobble = 28 * (1 - (double)ticks / totalTicks) + 6;
                    foreach (Image die in dice)
                    {
                        SetDieFace(die, random.Next(1, 7));
                        SetDieAngle(die, random.NextDouble() * 2 * wobble - wobble);
                    }
                    return;
                }

                _diceTimer.Stop();

                // Land on the real result, slightly askew like real dice.
                int[] values = { p1, p2, e1, e2 };
                for (int i = 0; i < dice.Length; i++)
                {
                    SetDieFace(dice[i], values[i]);
                    SetDieAngle(dice[i], random.NextDouble() * 12 - 6);
                }

                ResolveFightRound(p1, p2, e1, e2, playerCanHit, prefix);
            };
            _diceTimer.Start();
        }

        private void ResolveFightRound(int p1, int p2, int e1, int e2, bool playerCanHit, string prefix)
        {
            if (_fight == null)
            {
                return;
            }

            Enemy enemy = _fight.Enemy;

            int bonus = PlayerAttackBonus(out string detail);
            int playerTotal = p1 + p2 + bonus;
            int enemyTotal = e1 + e2 + enemy.Skill;

            PlayerAttackText.Text = playerTotal.ToString();
            PlayerAttackDetail.Text = $"{p1}+{p2} + {detail}";
            EnemyAttackText.Text = enemyTotal.ToString();
            EnemyAttackDetail.Text = $"{e1}+{e2} + skill {enemy.Skill}";

            bool playerCrit = p1 == 6 && p2 == 6;
            bool enemyCrit = e1 == 6 && e2 == 6;
            bool playerFumble = p1 == 1 && p2 == 1;
            bool enemyFumble = e1 == 1 && e2 == 1;

            // Who wins the round? +1 player, -1 enemy, 0 nobody.
            int winner;
            bool critical = false;
            string why = "";

            if (playerCrit && !enemyCrit)
            {
                winner = 1; critical = true; why = "Double six! ";
            }
            else if (enemyCrit && !playerCrit)
            {
                winner = -1; critical = true; why = $"{enemy.Name} rolls a double six! ";
            }
            else if (playerFumble && !enemyFumble)
            {
                winner = -1; why = "Snake eyes - you stumble! ";
            }
            else if (enemyFumble && !playerFumble)
            {
                winner = 1; why = $"{enemy.Name} rolls snake eyes and stumbles! ";
            }
            else
            {
                winner = Math.Sign(playerTotal - enemyTotal);
            }

            int damage = critical ? CriticalDamage : HitDamage;
            string outcome;

            if (winner > 0 && playerCanHit)
            {
                _fight.HurtEnemy(damage);
                outcome = critical
                    ? $"{why}A crushing blow strikes {enemy.Name}! (-{damage})"
                    : $"{why}You strike {enemy.Name}! (-{damage})";
            }
            else if (winner > 0)
            {
                outcome = $"{why}You dodge {enemy.Name}'s attack.";
            }
            else if (winner < 0)
            {
                _player.Health = Math.Max(0, _player.Health - damage);
                outcome = critical
                    ? $"{why}A vicious blow! (-{damage} Health)"
                    : $"{why}{enemy.Name} hits you! (-{damage} Health)";
            }
            else
            {
                outcome = "Your blows clash - neither of you is hurt.";
            }

            FightResultText.Text = string.IsNullOrEmpty(prefix) ? outcome : $"{prefix} {outcome}";

            UpdateEnemyHealth();
            UpdatePlayerStats();

            _diceRolling = false;

            if (_fight.EnemyDefeated)
            {
                WinFight();
            }
            else if (_player.Health <= 0)
            {
                LoseFight();
            }
            else
            {
                SetFightButtonsEnabled(true);
            }
        }

        // ---- Items, fleeing, the end -------------------------------------

        private void UseItemInFight(string itemName)
        {
            if (_fight == null)
            {
                return;
            }

            Item item = Items.GetItem(itemName);

            if (item.UseInFight == null)
            {
                FightResultText.Text = $"The {itemName} won't help in a fight.";
                return;   // no turn used
            }

            string message = item.UseInFight(_player, _fight);

            if (item.UsedUpInFight)
            {
                _player.Take(itemName);
            }

            UpdateEnemyHealth();
            UpdatePlayerStats();

            if (_fight.EnemyDefeated)
            {
                FightResultText.Text = message;
                WinFight();
                return;
            }

            // Using an item gives the enemy a free attack.
            RollFightRound(playerCanHit: false, prefix: message);
        }

        private void Flee()
        {
            if (_fight == null)
            {
                return;
            }

            Enemy enemy = _fight.Enemy;

            if (!enemy.CanFlee)
            {
                FightResultText.Text = "There is no escape - you must fight!";
                return;
            }

            _player.Health = Math.Max(0, _player.Health - HitDamage);
            UpdatePlayerStats();

            if (_player.Health <= 0)
            {
                FightResultText.Text = $"You turn to run - and {enemy.Name} strikes you down.";
                LoseFight();
                return;
            }

            enemy.OnFlee?.Invoke(_player);
            EndFight();
            ShowDescription($"You flee from {enemy.Name}, taking a parting blow as you go. (-{HitDamage} Health)");
            AfterStoryChange();
        }

        private void WinFight()
        {
            if (_fight == null)
            {
                return;
            }

            _fightOver = true;
            FightButtons.Visibility = Visibility.Collapsed;
            DialogueCharacter.Visibility = Visibility.Collapsed;

            // Show the beaten close-up, if there is one.
            if (!string.IsNullOrWhiteSpace(_fight.Enemy.BeatenPortrait))
            {
                SetCloseUpPortrait(_fight.Enemy.BeatenPortrait);
            }

            FightResultText.Text =
                $"{FightResultText.Text}  You have defeated {_fight.Enemy.Name}!  (Click to continue)";
        }

        // Clicking the fight panel after a win carries on.
        private void FightPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_fight == null || !_fightOver || _player.Health <= 0)
            {
                return;
            }

            e.Handled = true;

            Enemy enemy = _fight.Enemy;
            enemy.OnWin?.Invoke(_player);

            EndFight();

            ShowDescription(string.IsNullOrWhiteSpace(enemy.WinText)
                ? $"{enemy.Name} is defeated."
                : enemy.WinText);

            AfterStoryChange();
        }

        private void LoseFight()
        {
            _fightOver = true;
            SetFightButtonsEnabled(false);

            string killer = _fight?.Enemy.Name ?? "your foe";
            ShowGameOver($"Slain by {killer}. The Mansion claims another visitor.", canRetryFight: true);
        }

        // Puts the HUD back to normal after a fight.
        private void EndFight()
        {
            _diceTimer?.Stop();
            _fight = null;
            _fightOver = false;
            _diceRolling = false;

            CloseInventory();

            FightPanel.Visibility = Visibility.Collapsed;
            FightButtons.Visibility = Visibility.Collapsed;
            MenuButtons.Visibility = Visibility.Visible;
            SetMenuForDialogue(false);

            DialogueCharacter.Visibility = Visibility.Collapsed;
            DialogueCharacter.Source = null;
            _shownCharacter = "";
            HideCloseUpPortrait();
            DialogueDim.Visibility = Visibility.Collapsed;

            DescriptionText.Visibility = Visibility.Visible;
        }

        // ---- Game over ---------------------------------------------------

        private bool _gameOverShown;

        private void ShowGameOver(string text, bool canRetryFight)
        {
            if (_gameOverShown)
            {
                return;
            }

            _gameOverShown = true;

            GameOverText.Text = text;
            GameOverRetryButton.Visibility = canRetryFight && _fightSnapshot != null
                ? Visibility.Visible
                : Visibility.Collapsed;

            GameOverScreen.Visibility = Visibility.Visible;
        }

        private void GameOverRetry_Click(object sender, RoutedEventArgs e)
        {
            if (_fightSnapshot == null)
            {
                return;
            }

            _gameOverShown = false;
            GameOverScreen.Visibility = Visibility.Collapsed;

            RestoreSnapshot(_fightSnapshot);
            EndFight();
            UpdatePlayerStats();
            StartFight(_lastFightId);
        }

        private void GameOverRestart_Click(object sender, RoutedEventArgs e)
        {
            ResetToTitle();
        }

        /*
         * Places the portrait image: inside the scene at X/Y/height
         * (background image pixels), or on the right if height is 0.
         */
        // ---- Close-up portrait (.Portrait) ------------------------------

        // Where close-up portraits always appear, in background image pixels
        // (X, Y, height) - the same units as .StandAt(...).
        private const double PortraitSpotX = 60;
        private const double PortraitSpotY = 350;
        private const double PortraitSpotHeight = 400;

        private string _shownCloseUp = "";

        private void SetCloseUpPortrait(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                HideCloseUpPortrait();
                return;
            }

            if (imagePath == _shownCloseUp && DialoguePortrait.Source != null)
            {
                DialoguePortrait.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                DialoguePortrait.Source = new BitmapImage(new Uri(
                    $"pack://application:,,,/{imagePath}",
                    UriKind.Absolute));
                _shownCloseUp = imagePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not load portrait:\n\n{imagePath}\n\n{ex.Message}",
                    "Portrait error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                HideCloseUpPortrait();
                return;
            }

            Rect spot = ImageToScreen(new Rect(PortraitSpotX, PortraitSpotY, 0, PortraitSpotHeight));
            DialoguePortrait.Margin = new Thickness(spot.X, spot.Y, 0, 0);
            DialoguePortrait.Height = spot.Height;
            DialoguePortrait.Visibility = Visibility.Visible;
        }

        private void HideCloseUpPortrait()
        {
            DialoguePortrait.Visibility = Visibility.Collapsed;
            DialoguePortrait.Source = null;
            _shownCloseUp = "";
        }

        private void PlaceCharacter(double x, double y, double height)
        {
            if (height > 0)
            {
                Rect screen = ImageToScreen(new Rect(x, y, 0, height));

                DialogueCharacter.HorizontalAlignment = HorizontalAlignment.Left;
                DialogueCharacter.Margin = new Thickness(screen.X, screen.Y, 0, 0);
                DialogueCharacter.Height = screen.Height;
            }
            else
            {
                DialogueCharacter.HorizontalAlignment = HorizontalAlignment.Right;
                DialogueCharacter.Margin = new Thickness(0, 100, 140, 0);
                DialogueCharacter.Height = 700;
            }
        }

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

            for (int slot = 1; slot <= SaveSystem.SlotCount; slot++)
            {
                SaveSlotsPanel.Children.Add(CreateSaveSlotButton(slot, saving));
            }

            SaveLoadScreen.Visibility = Visibility.Visible;
        }

        private Button CreateSaveSlotButton(int slot, bool saving)
        {
            SaveGame? save = SaveSystem.Load(slot);

            var title = new TextBlock
            {
                Text = save == null
                    ? $"SLOT {slot}  -  Empty"
                    : $"SLOT {slot}  -  {save.PlayerName}",
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
                Foreground = new SolidColorBrush(Color.FromRgb(0xA8, 0x9A, 0x84)),
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
            _gameOverShown = false;
            GameOverScreen.Visibility = Visibility.Collapsed;
            SaveLoadScreen.Visibility = Visibility.Collapsed;
            HideGameMenu();

            if (IsInDialogue)
            {
                EndDialogue();
            }

            EndFight();

            _player = new Player();
            _rerollsRemaining = 3;
            _selectedItem = null;
            _fightSnapshot = null;
            _currentScene = null;

            GameScreen.Visibility = Visibility.Collapsed;
            TrunkScreen.Visibility = Visibility.Collapsed;
            PlayerRollScreen.Visibility = Visibility.Collapsed;
            BackstoryScreen.Visibility = Visibility.Collapsed;
            TitleScreen.Visibility = Visibility.Visible;
        }

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