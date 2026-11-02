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

            UpdateDebugVisibility();
            this.Cursor = new Cursor("C:\\The Mansion\\The Mansion\\Assets\\Cursors\\cursor_arrow.cur");
            ShowScene(Scenes.FrontGate);


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

            LoadSceneBackground(scene.BackgroundImage);

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

            var rectangle = new Rectangle
            {
                Width = hotspot.Width,
                Height = hotspot.Height,

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

            Canvas.SetLeft(rectangle, hotspot.X);
            Canvas.SetTop(rectangle, hotspot.Y);

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
                DebugCoordinates.Text =
                    $"Mouse: X={currentPosition.X:0} " +
                    $"Y={currentPosition.Y:0}";
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

            return
$@"new Hotspot
{{
    Name = ""New Hotspot"",

    X = {xText},
    Y = {yText},

    Width = {widthText},
    Height = {heightText},

    Description = ""Describe the hotspot here."",

    TargetScene = """"
}}";
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

                        ShowDescription("Choose an item...");
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
                    ShowDescription($"Use item on {hotspot.Name}");
                    break;
            }
        }

        private void Hotspot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_currentScene != null)
            {
                ShowDescription(_currentScene.Description);
            }
        }

        private void Return_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}