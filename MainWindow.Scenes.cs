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
    // Showing scenes, their hotspots and sprites, and the LOOK / TALK / ACT verbs on a hotspot.
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        private void ShowDescription(string text)
        {
            DescriptionText.Text = text;
        }
        private void ShowScene(Scene scene)
        {
            bool newScene = _currentScene != scene;

            _currentScene = scene;
            _replyShowing = false;
            HideHoverLabel();

            // .BackgroundWhen(...) can swap the picture after story events.
            _currentBackground = scene.GetBackground(_player);
            LoadSceneBackground(_currentBackground);

            DescriptionText.Text = scene.Description;

            CreateSceneElements(scene);

            if (GameScreen.Visibility == Visibility.Visible)
            {
                Sound.Ambience(AmbienceFor(scene));
            }

            if (newScene && GameScreen.Visibility == Visibility.Visible)
            {
                FadeInScene();
                Autosave();
                PreloadNeighbours(scene);
            }
        }

        /*
         * While the player looks around, quietly load the backgrounds of the
         * places they can go next, so moving there doesn't stutter.
         */
        private void PreloadNeighbours(Scene scene)
        {
            Dispatcher.InvokeAsync(() =>
            {
                foreach (Hotspot hotspot in scene.Hotspots)
                {
                    foreach (string name in new[] { hotspot.TargetScene, hotspot.CloseUpScene })
                    {
                        Scene? next = Scenes.GetScene(name);
                        if (next != null)
                        {
                            ImageCache.Preload(next.GetBackground(_player));
                        }
                    }
                }
            }, DispatcherPriority.ApplicationIdle);
        }

        // Looping background sound for a scene: rain outdoors, the hall's
        // quiet inside. (Add indoor scenes here as they are built.)
        private static string AmbienceFor(Scene scene) => scene.Name switch
        {
            "EntranceHall" => "hall_ambience",
            _ => "rain_loop"
        };

        /*
         * The black curtain snaps shut and fades away, so moving between
         * scenes is a short fade from black rather than a hard cut.
         */
        private void FadeInScene()
        {
            var fade = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(450))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            SceneFade.BeginAnimation(OpacityProperty, fade);
        }

        /*
         * Every new scene is saved to the autosave slot (only while exploring:
         * never mid-conversation or mid-fight). Load it from any Load screen.
         */
        private void Autosave()
        {
            if (!CanSaveNow || _currentScene == null)
            {
                return;
            }

            // A failed autosave shouldn't interrupt play; manual saves still report errors.
            SaveSystem.Save(SaveSystem.AutosaveSlot, SaveGame.From(_player, _currentScene.Name));
        }

        private void LoadSceneBackground(string imagePath)
        {
            BitmapImage? background = LoadImage(imagePath, "scene picture");
            if (background != null)
            {
                RoomBackground.Source = background;
            }
        }

        private void CreateSceneElements(Scene scene)
        {
            /*
             * Remove the previous scene's hotspots, characters,
             * and debug selection rectangle.
             */
            SceneCanvas.Children.Clear();
            _hotspotsRevealed = false;

            // The hotspot under the mouse may be gone now (e.g. an item was
            // taken), so don't leave its name floating there.
            HideHoverLabel();

            // The Return arrow only shows where it leads somewhere (.ReturnTo).
            Return.Visibility = scene.Hotspots.Any(h => h.ScreenSpace)
                ? Visibility.Visible
                : Visibility.Collapsed;

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
        }

        private Image? CreateSpriteImage(SceneSprite sprite)
        {
            BitmapImage? picture = LoadImage(sprite.Image, "sprite");
            if (picture == null)
            {
                return null;
            }

            var image = new Image
            {
                Source = picture,
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

                Tag = hotspot
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
             * Screen UI (the Return arrow) works with any verb.
             */
            if (hotspot.ScreenSpace)
            {
                GoToScene(hotspot.TargetScene, hotspot.Name);

                // The .ReturnTo text ("You step back out into the rain.") is
                // shown after the move, so the new scene's description
                // doesn't replace it straight away.
                if (!string.IsNullOrWhiteSpace(hotspot.Description))
                {
                    ShowDescription(hotspot.Description);
                }
                return;
            }

            switch (_interactionMode)
            {
                case InteractionMode.Look:
                    LookAt(hotspot);
                    break;

                case InteractionMode.Talk:
                    TalkTo(hotspot);
                    break;

                case InteractionMode.Act:
                    ActOn(hotspot);
                    break;

                case InteractionMode.Item:
                    if (_selectedItem == null)
                    {
                        OpenInventory();
                    }
                    else
                    {
                        UseSelectedItemOn(hotspot);
                    }
                    break;
            }
        }

        // Replies when a verb does nothing on a hotspot: Data/Replies.cs
        // (a few variants each, so they don't repeat).

        /*
         * LOOK: describe it, and open its close-up scene if it has one.
         */
        private void LookAt(Hotspot hotspot)
        {
            if (!string.IsNullOrWhiteSpace(hotspot.CloseUpScene))
            {
                GoToScene(hotspot.CloseUpScene, hotspot.Name);
                return;
            }

            ShowReply(string.IsNullOrWhiteSpace(hotspot.Description)
                ? Replies.Nothing()
                : hotspot.Description);
        }

        /*
         * TALK: a conversation (.TalkStarts), a plain reply (.OnTalk),
         * or "Who or what are you talking to?".
         */
        private void TalkTo(Hotspot hotspot)
        {
            if (!string.IsNullOrWhiteSpace(hotspot.DialogueId))
            {
                StartDialogue(hotspot.DialogueId);
                return;
            }

            if (hotspot.TalkResult != null)
            {
                ShowReply(hotspot.TalkResult(_player));
                AfterStoryChange();
                return;
            }

            ShowReply(string.IsNullOrWhiteSpace(hotspot.TalkText)
                ? Replies.NoTalk()
                : hotspot.TalkText);
        }

        /*
         * ACT: a conversation (.ActStarts), a fight (.ActStartsFight), an
         * action (.OnAct), moving on to a new place (.GoesTo) - or
         * "You can't do that."
         */
        private void ActOn(Hotspot hotspot)
        {
            if (!string.IsNullOrWhiteSpace(hotspot.ActSound))
            {
                Sound.Play(hotspot.ActSound);
            }

            if (!string.IsNullOrWhiteSpace(hotspot.ActDialogueId))
            {
                StartDialogue(hotspot.ActDialogueId);
                return;
            }

            if (!string.IsNullOrWhiteSpace(hotspot.ActFightId))
            {
                StartFight(hotspot.ActFightId);
                return;
            }

            if (!string.IsNullOrWhiteSpace(hotspot.ActPuzzle))
            {
                OpenPuzzle(hotspot.ActPuzzle);
                return;
            }

            if (hotspot.ActResult != null)
            {
                ShowReply(hotspot.ActResult(_player));
                AfterStoryChange();
                return;
            }

            if (!string.IsNullOrWhiteSpace(hotspot.ActText) || hotspot.ActEffect != null)
            {
                if (!string.IsNullOrWhiteSpace(hotspot.ActText))
                {
                    ShowReply(hotspot.ActText);
                }

                hotspot.ActEffect?.Invoke(_player);
                AfterStoryChange();

                if (!string.IsNullOrWhiteSpace(hotspot.ActPicture))
                {
                    ShowPicture(hotspot.ActPicture);
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(hotspot.TargetScene))
            {
                GoToScene(hotspot.TargetScene, hotspot.Name);
                return;
            }

            ShowReply(Replies.NoAct());
        }

        private void GoToScene(string sceneName, string hotspotName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            Scene? targetScene = Scenes.GetScene(sceneName);

            if (targetScene == null)
            {
                MessageBox.Show(
                    $"The target scene '{sceneName}' " +
                    $"was not found.\n\n" +
                    $"Hotspot: {hotspotName}",
                    "Missing scene",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            ShowScene(targetScene);
        }
    }
}
