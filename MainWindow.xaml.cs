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
        private Cursor _normalCursor;
        private Cursor _actCursor = new Cursor("C:\\The Mansion\\The Mansion\\Assets\\Cursors\\cursor_act.cur");
        private Cursor _lookCursor = new Cursor("C:\\The Mansion\\The Mansion\\Assets\\Cursors\\cursor_look.cur");
        private Cursor _talkCursor = new Cursor("C:\\The Mansion\\The Mansion\\Assets\\Cursors\\cursor_talk.cur");
        private Cursor _itemCursor = new Cursor("C:\\The Mansion\\The Mansion\\Assets\\Cursors\\cursor_item.cur");
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

            // Game area is exactly 1920x1080 screen pixels; the window
            // (with its title bar) is then centred on screen.
            SourceInitialized += (_, _) => LockWindowSize();
            Loaded += (_, _) => PositionWindow();

            UpdateDebugVisibility();
            this.Cursor = new Cursor("C:\\The Mansion\\The Mansion\\Assets\\Cursors\\cursor_arrow.cur");
            ShowScene(Scenes.FrontGate);


        }
        /*
         * The game area is hardcoded to 1920x1080 *physical* pixels.
         *
         * WPF measures in device-independent units, so with Windows display
         * scaling above 100% (e.g. 125% on laptops) a 1920-unit area would
         * really be 2400 pixels wide. This shrinks the 1920x1080 layout by the
         * same factor so it is exactly 1920x1080 pixels on screen. The window
         * uses SizeToContent, so it wraps itself (plus title bar) around it.
         * All hotspot / canvas coordinates stay in 1920x1080 space.
         */
        private void LockWindowSize()
        {
            DpiScale dpi = VisualTreeHelper.GetDpi(this);

            RootGrid.LayoutTransform = new ScaleTransform(
                1 / dpi.DpiScaleX,
                1 / dpi.DpiScaleY);
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
            LockWindowSize();
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

            EnterGame();
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
                        //ShowLoadGame();
                        break;
                    }

                case "SettingsBtn":
                    {
                        //ShowSettings();
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
             * ACT on something with a plain reply and/or effect (.OnAct).
             */
            if (_interactionMode == InteractionMode.Act &&
                (!string.IsNullOrWhiteSpace(hotspot.ActText) || hotspot.ActEffect != null))
            {
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

            PlaceDialoguePortrait(dialogue);
            SetDialoguePortrait(dialogue.Portrait);

            // The dim layer also blocks clicks on hotspots while talking,
            // so it is always shown - just see-through when not dimming.
            DialogueDim.Fill = dialogue.DimScene
                ? new SolidColorBrush(Color.FromArgb(0x8C, 0, 0, 0))
                : Brushes.Transparent;
            DialogueDim.Visibility = Visibility.Visible;
            DescriptionText.Visibility = Visibility.Collapsed;
            DialoguePanel.Visibility = Visibility.Visible;

            // No switching verbs mid-conversation.
            MenuButtons.IsHitTestVisible = false;
            MenuButtons.Opacity = 0.45;

            string startNode = dialogue.ChooseStartNode != null
                ? dialogue.ChooseStartNode(_player)
                : dialogue.StartNode;

            ShowDialogueNode(startNode);
        }

        /*
         * Default: standing on the right of the scene, 700px tall.
         * If the dialogue gives PortraitHeight, the character is placed
         * in the scene at PortraitX/Y (background image pixels).
         */
        private void PlaceDialoguePortrait(Dialogue dialogue)
        {
            if (dialogue.PortraitHeight > 0)
            {
                Rect screen = ImageToScreen(new Rect(
                    dialogue.PortraitX,
                    dialogue.PortraitY,
                    0,
                    dialogue.PortraitHeight));

                DialoguePortrait.HorizontalAlignment = HorizontalAlignment.Left;
                DialoguePortrait.Margin = new Thickness(screen.X, screen.Y, 0, 0);
                DialoguePortrait.Height = screen.Height;
            }
            else
            {
                DialoguePortrait.HorizontalAlignment = HorizontalAlignment.Right;
                DialoguePortrait.Margin = new Thickness(0, 100, 140, 0);
                DialoguePortrait.Height = 700;
            }
        }

        private string _shownPortrait = "";

        private void SetDialoguePortrait(string imagePath)
        {
            if (imagePath == _shownPortrait &&
                DialoguePortrait.Source != null)
            {
                return; // already showing it
            }

            _shownPortrait = imagePath;
            DialoguePortrait.Source = null;

            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                try
                {
                    DialoguePortrait.Source = new BitmapImage(new Uri(
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

            DialoguePortrait.Visibility =
                DialoguePortrait.Source != null ? Visibility.Visible : Visibility.Collapsed;
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

            // A line can swap the picture (e.g. an angry pose).
            SetDialoguePortrait(string.IsNullOrWhiteSpace(node.Portrait)
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

                if (_visibleChoices.Count == 4)
                {
                    break; // only 4 fit in the panel
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
                    FontSize = 20,
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
            _activeDialogue = null;
            _visibleChoices.Clear();

            DialogueChoicesPanel.Children.Clear();
            DialoguePanel.Visibility = Visibility.Collapsed;
            DialoguePortrait.Visibility = Visibility.Collapsed;
            DialoguePortrait.Source = null;
            _shownPortrait = "";
            DialogueDim.Visibility = Visibility.Collapsed;

            DescriptionText.Visibility = Visibility.Visible;

            MenuButtons.IsHitTestVisible = true;
            MenuButtons.Opacity = 1;

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

            if (!IsInDialogue)
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
            DescriptionText.Visibility = Visibility.Visible;
            _inventoryOpen = false;

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

        private void Return_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}