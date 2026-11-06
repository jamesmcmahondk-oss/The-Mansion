using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using The_Mansion.Data;

namespace The_Mansion
{
    // The frame puzzle: three brass medallions that really press in
    // (logic in Data/FramePuzzle.cs; screen in MainWindow.xaml).
    // One click = one press: the status line changes, nothing to dismiss.
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        private bool IsFramePuzzleOpen => FramePuzzleScreen.Visibility == Visibility.Visible;

        // Busy animating a reset or the solve: clicks wait.
        private bool _framePuzzleBusy;

        private Image MedallionPressed(string medallion) => medallion switch
        {
            "Left" => MedallionLeftPressed,
            "Centre" => MedallionCentrePressed,
            _ => MedallionRightPressed
        };

        private void OpenFramePuzzle()
        {
            if (_player.HasFlag("SolvedPortraitGalleryPuzzle"))
            {
                ShowReply("The frame already stands open.");
                return;
            }

            _replyShowing = false;
            HideHoverLabel();
            HideRevealedHotspots();
            _framePuzzleBusy = false;

            ShowPressedMedallions();

            // The dates, once seen on the portraits.
            FramePuzzleClueText.Text = _player.HasFlag("ExaminedThreeAges")
                ? "The three portraits:  1821 (left)  ·  1798 (centre)  ·  1854 (right)"
                : "";
            FramePuzzleStatusText.Text = FramePuzzle.ShowsHint(_player)
                ? "Three brass medallions. The portraits are not three members of a family. They are one life, viewed at different times."
                : "Three brass medallions, the same woman's profile on each, at three different ages.";

            FramePuzzleScreen.Visibility = Visibility.Visible;
        }

        private void ShowPressedMedallions()
        {
            foreach (string medallion in FramePuzzle.Medallions)
            {
                Image pressed = MedallionPressed(medallion);
                pressed.BeginAnimation(OpacityProperty, null);
                pressed.Opacity = 1;
                pressed.Effect = null;
                pressed.Visibility = FramePuzzle.IsPressed(_player, medallion) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CloseFramePuzzle()
        {
            FramePuzzleScreen.Visibility = Visibility.Collapsed;
        }

        private void FramePuzzleBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            CloseFramePuzzle();
        }

        private void Medallion_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (_framePuzzleBusy || sender is not Ellipse { Tag: string medallion })
            {
                return;
            }

            if (FramePuzzle.IsPressed(_player, medallion))
            {
                FramePuzzleStatusText.Text = "That one is already pressed in.";
                return;
            }

            FramePuzzle.Result result = FramePuzzle.Press(_player, medallion, out string text);
            Sound.Play("puzzle_click");
            FramePuzzleStatusText.Text = text;

            // It goes down either way - you feel the click before the reset.
            MedallionPressed(medallion).Visibility = Visibility.Visible;

            if (result == FramePuzzle.Result.Wrong)
            {
                _framePuzzleBusy = true;
                After(650, () =>
                {
                    ShowPressedMedallions();       // all back up
                    _framePuzzleBusy = false;
                });
            }
            else if (result == FramePuzzle.Result.Solved)
            {
                SolveFramePuzzle();
            }
        }

        // The medallions glow, then you step back as the frame swings open.
        private void SolveFramePuzzle()
        {
            _framePuzzleBusy = true;

            foreach (string medallion in FramePuzzle.Medallions)
            {
                var glow = new DropShadowEffect { BlurRadius = 22, ShadowDepth = 0, Opacity = 0, Color = Color.FromRgb(0xFF, 0xC8, 0x6A) };
                MedallionPressed(medallion).Visibility = Visibility.Visible;
                MedallionPressed(medallion).Effect = glow;
                glow.BeginAnimation(DropShadowEffect.OpacityProperty,
                    new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(700)) { BeginTime = TimeSpan.FromMilliseconds(300) });
                glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                    new DoubleAnimation(22, 45, TimeSpan.FromMilliseconds(900)) { BeginTime = TimeSpan.FromMilliseconds(300) });
            }

            After(2200, () =>
            {
                CloseFramePuzzle();
                Sound.Play("door_creak");
                AfterStoryChange();      // the gallery picture: the frame stands open
                FadeInScene();
                ShowReply("Something releases inside the wall. The empty frame swings slowly open on hidden hinges.");
            });
        }

        private static void After(int milliseconds, Action action)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            timer.Tick += (_, _) => { timer.Stop(); action(); };
            timer.Start();
        }
    }
}
