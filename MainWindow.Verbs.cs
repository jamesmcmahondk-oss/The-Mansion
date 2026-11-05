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
    // The verb buttons, click replies, and showing what is clickable (hover names, Space).
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        // The verb buttons' pictures: (normal, glowing) - the glow shows on
        // hover and for the verb in use.
        private static readonly Dictionary<string, (string Normal, string Glow)> VerbButtonImages = new()
        {
            ["LookBtn"] = ("Assets/UI/look.png", "Assets/UI/look_glow.png"),
            ["TalkBtn"] = ("Assets/UI/talk.png", "Assets/UI/talk_glow.png"),
            ["ActBtn"] = ("Assets/UI/act.png", "Assets/UI/act_glow.png"),
            ["ItemBtn"] = ("Assets/UI/item.png", "Assets/UI/item_glow.png")
        };

        private void MenuButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Image button && VerbButtonImages.TryGetValue(button.Name, out var images))
            {
                SetButtonImage(button, images.Glow);
            }

            e.Handled = true;
        }

        private static void SetButtonImage(Image buttonImage, string imagePath)
        {
            BitmapImage? picture = LoadImage(imagePath, "button picture");
            if (picture != null)
            {
                buttonImage.Source = picture;
            }
        }

        private void UpdateMenuButtonImages()
        {
            SetVerbButton(LookBtn, InteractionMode.Look);
            SetVerbButton(TalkBtn, InteractionMode.Talk);
            SetVerbButton(ActBtn, InteractionMode.Act);
            SetVerbButton(ItemBtn, InteractionMode.Item);
        }

        private void SetVerbButton(Image button, InteractionMode mode)
        {
            var images = VerbButtonImages[button.Name];
            SetButtonImage(button, _interactionMode == mode ? images.Glow : images.Normal);
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

            // A new verb: hover text takes over from the last reply again.
            _replyShowing = false;

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
        /*
         * A reply to a click (e.g. "You can't do that.") stays on screen until
         * the player clicks again to dismiss it - hovering over hotspots
         * doesn't replace it. See ShowReply / Window_PreviewMouseLeftButtonDown.
         */
        private bool _replyShowingValue;

        // Setting this also shows/hides the blinking "click to continue" hint.
        private bool _replyShowing
        {
            get => _replyShowingValue;
            set
            {
                _replyShowingValue = value;

                if (value)
                {
                    ContinueHint.Visibility = Visibility.Visible;
                    ContinueHint.BeginAnimation(OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.2,
                        Duration = TimeSpan.FromSeconds(0.7),
                        AutoReverse = true,
                        RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
                    });
                }
                else
                {
                    ContinueHint.BeginAnimation(OpacityProperty, null);
                    ContinueHint.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ShowReply(string text)
        {
            ShowDescription(text);
            _replyShowing = true;
        }

        private void DismissReply()
        {
            _replyShowing = false;

            // Back to the hover text for whatever is under the mouse.
            if (Mouse.DirectlyOver is Rectangle { Tag: Hotspot hotspot })
            {
                ShowDescription(HoverText(hotspot));
            }
            else if (_interactionMode == InteractionMode.Item && _selectedItem != null)
            {
                ShowDescription($"Use {_selectedItem} on...");
            }
            else if (_currentScene != null)
            {
                ShowDescription(_currentScene.Description);
            }
        }

        /*
         * While a reply is showing, the next click anywhere in the game just
         * dismisses it. (Clicks on the verb buttons dismiss it and still work.)
         */
        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // A picture being looked at closes on any click.
            if (PictureViewer.Visibility == Visibility.Visible)
            {
                PictureViewer.Visibility = Visibility.Collapsed;
                e.Handled = true;
                return;
            }

            if (!_replyShowing ||
                GameScreen.Visibility != Visibility.Visible ||
                IsInDialogue || IsInFight || _inventoryOpen ||
                IsGameMenuOpen || IsSaveLoadOpen || IsSettingsOpen || IsCrossPuzzleOpen || IsJournalOpen ||
                DebugMenu.Visibility == Visibility.Visible ||
                GameOverScreen.Visibility == Visibility.Visible)
            {
                return;
            }

            // (OriginalSource can be a piece of text - a Run - which isn't a
            // Visual, so IsAncestorOf can't be asked about it.)
            bool onVerbButton = e.OriginalSource is Visual source &&
                                MenuButtons.IsAncestorOf(source);

            DismissReply();

            if (!onVerbButton)
            {
                e.Handled = true;
            }
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

            _hoveredHotspot = hotspot;
            UpdateHoverLabel(Mouse.GetPosition(SceneCanvas));

            if (_replyShowing)
            {
                return;     // keep the reply until it's clicked away
            }

            ShowDescription(HoverText(hotspot));
        }

        // =========================================================
        // SHOWING WHAT'S CLICKABLE
        // =========================================================

        private Hotspot? _hoveredHotspot;
        private bool _hotspotsRevealed;

        // Scene hotspots only respond while exploring.
        private bool CanUseHotspots =>
            GameScreen.Visibility == Visibility.Visible &&
            !IsInDialogue &&
            !IsInFight &&
            !_gameOverShown &&
            !IsGameMenuOpen &&
            !IsSaveLoadOpen &&
            !IsSettingsOpen &&
            !IsJournalOpen &&
            !IsCrossPuzzleOpen;

        /*
         * The name of the thing under the cursor floats just below-right of
         * it (flipped left/up near the edges so it never leaves the screen).
         */
        private void UpdateHoverLabel(Point mouse)
        {
            if (_hoveredHotspot == null || !CanUseHotspots)
            {
                HideHoverLabel();
                return;
            }

            HoverLabelText.Text = _hoveredHotspot.ScreenSpace ? "Go back" : _hoveredHotspot.Name;
            HoverLabel.Visibility = Visibility.Visible;
            HoverLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size size = HoverLabel.DesiredSize;

            double x = mouse.X + 26;
            double y = mouse.Y + 30;

            if (x + size.Width > 1910)
            {
                x = mouse.X - size.Width - 12;
            }

            if (y + size.Height > 790)
            {
                y = mouse.Y - size.Height - 12;
            }

            Canvas.SetLeft(HoverLabel, Math.Max(4, x));
            Canvas.SetTop(HoverLabel, Math.Max(4, y));
        }

        private void HideHoverLabel()
        {
            _hoveredHotspot = null;
            HoverLabel.Visibility = Visibility.Collapsed;
        }

        private void SceneCanvas_MouseLeave(object sender, MouseEventArgs e) => HideHoverLabel();

        /*
         * Holding Space outlines every hotspot in the scene and names it,
         * so nothing clickable is hidden in the picture. Let go to hide them.
         */
        private void RevealHotspots()
        {
            if (_hotspotsRevealed || !CanUseHotspots || _inventoryOpen)
            {
                return;
            }

            _hotspotsRevealed = true;

            foreach (Rectangle rectangle in SceneCanvas.Children.OfType<Rectangle>().ToList())
            {
                if (rectangle.Tag is not Hotspot hotspot || hotspot.ScreenSpace)
                {
                    continue;
                }

                var outline = new Rectangle
                {
                    Width = rectangle.Width,
                    Height = rectangle.Height,
                    Stroke = RevealOutlineBrush,
                    StrokeThickness = 3,
                    StrokeDashArray = new DoubleCollection { 4, 2 },
                    Fill = RevealFillBrush,
                    RadiusX = 10,
                    RadiusY = 10,
                    IsHitTestVisible = false,
                    Tag = RevealTag
                };
                Canvas.SetLeft(outline, Canvas.GetLeft(rectangle));
                Canvas.SetTop(outline, Canvas.GetTop(rectangle));
                Panel.SetZIndex(outline, 11);
                SceneCanvas.Children.Add(outline);

                var name = new Border
                {
                    Background = RevealLabelBrush,
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(8, 2, 8, 3),
                    IsHitTestVisible = false,
                    Tag = RevealTag,
                    Child = new TextBlock
                    {
                        Text = hotspot.Name,
                        Foreground = RevealOutlineBrush,
                        FontFamily = Georgia,
                        FontWeight = FontWeights.Bold,
                        FontSize = 18
                    }
                };
                name.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double left = Canvas.GetLeft(rectangle) + (rectangle.Width - name.DesiredSize.Width) / 2;
                double top = Canvas.GetTop(rectangle) + (rectangle.Height - name.DesiredSize.Height) / 2;
                Canvas.SetLeft(name, Math.Clamp(left, 4, 1916 - name.DesiredSize.Width));
                Canvas.SetTop(name, Math.Clamp(top, 4, 796 - name.DesiredSize.Height));
                Panel.SetZIndex(name, 12);
                SceneCanvas.Children.Add(name);
            }
        }

        private const string RevealTag = "reveal";

        private static readonly Brush RevealOutlineBrush = Paint.Argb(0xE0, 0xC8, 0xA9, 0x6A);
        private static readonly Brush RevealFillBrush = Paint.Argb(0x30, 0xC8, 0xA9, 0x6A);
        private static readonly Brush RevealLabelBrush = Paint.Argb(0xB0, 0, 0, 0);
        private static readonly FontFamily Georgia = new("Georgia");

        private void HideRevealedHotspots()
        {
            if (!_hotspotsRevealed)
            {
                return;
            }

            _hotspotsRevealed = false;

            foreach (FrameworkElement element in SceneCanvas.Children.OfType<FrameworkElement>()
                         .Where(child => RevealTag.Equals(child.Tag)).ToList())
            {
                SceneCanvas.Children.Remove(element);
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && _hotspotsRevealed)
            {
                HideRevealedHotspots();
                e.Handled = true;
            }
        }

        private string HoverText(Hotspot hotspot)
        {
            if (hotspot.ScreenSpace)
            {
                return "Go back";
            }

            switch (_interactionMode)
            {
                case InteractionMode.Look:
                    return $"Look at {hotspot.Name}";

                case InteractionMode.Talk:
                    return $"Talk to {hotspot.Name}";

                case InteractionMode.Act:
                    bool hasAction =
                        !string.IsNullOrWhiteSpace(hotspot.ActDialogueId) ||
                        !string.IsNullOrWhiteSpace(hotspot.ActFightId) ||
                        !string.IsNullOrWhiteSpace(hotspot.ActPuzzle) ||
                        !string.IsNullOrWhiteSpace(hotspot.ActText) ||
                        hotspot.ActEffect != null ||
                        hotspot.ActResult != null;

                    return !hasAction && !string.IsNullOrWhiteSpace(hotspot.TargetScene)
                        ? $"Move to {hotspot.Name}"
                        : $"Use {hotspot.Name}";

                default:
                    return _selectedItem != null
                        ? $"Use {_selectedItem} on {hotspot.Name}"
                        : $"Use item on {hotspot.Name}";
            }
        }

        private void Hotspot_MouseLeave(object sender, MouseEventArgs e)
        {
            HideHoverLabel();

            // A click reply is showing: leave it there.
            if (_replyShowing)
            {
                return;
            }

            if (_interactionMode == InteractionMode.Item && _selectedItem != null)
            {
                ShowDescription($"Use {_selectedItem} on...");
            }
            else if (_currentScene != null)
            {
                ShowDescription(_currentScene.Description);
            }
        }
    }
}
