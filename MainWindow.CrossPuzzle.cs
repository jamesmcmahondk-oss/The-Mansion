using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using The_Mansion.Data;

namespace The_Mansion
{
    // The cross puzzle: three stone spirals that really turn
    // (logic in Data/CrossPuzzle.cs; screen in MainWindow.xaml).
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        private bool IsCrossPuzzleOpen => CrossPuzzleScreen.Visibility == Visibility.Visible;

        // The angle each disc is turned to on screen. Kept as a running total
        // (not 0-360) so a turn always animates the short way round.
        private readonly Dictionary<string, double> _discAngles = new();
        private bool _crossPuzzleSolving;

        private Image SpiralDisc(string spiral) => spiral switch
        {
            "Upper" => SpiralUpperDisc,
            "Left" => SpiralLeftDisc,
            _ => SpiralRightDisc
        };

        private static RotateTransform DiscRotation(Image disc) => (RotateTransform)disc.RenderTransform;

        // ACT on a hotspot with .ActOpensPuzzle(...).
        private void OpenPuzzle(string puzzle)
        {
            if (puzzle != "Cross")
            {
                ShowReply(Replies.NoAct());
                return;
            }

            if (_player.HasFlag("CrossOpened"))
            {
                ShowReply("The spirals are locked in place now. The panel in the plinth stands open.");
                return;
            }

            _replyShowing = false;
            HideHoverLabel();
            HideRevealedHotspots();
            _crossPuzzleSolving = false;

            foreach (string spiral in CrossPuzzle.Spirals)
            {
                double angle = CrossPuzzle.Angle(_player, spiral);
                _discAngles[spiral] = angle;

                Image disc = SpiralDisc(spiral);
                DiscRotation(disc).BeginAnimation(RotateTransform.AngleProperty, null);
                DiscRotation(disc).Angle = angle;
                disc.Effect = new DropShadowEffect { BlurRadius = 22, ShadowDepth = 5, Opacity = 0.85, Color = Colors.Black };
            }

            bool knowsClue = _player.HasFlag("ReadGravestone");
            CrossPuzzleClueText.Text = knowsClue
                ? "THRICE FOR HEAVEN  ·  ONCE FOR THE HEART  ·  NEVER FOR THE HAND THAT LET GO"
                : "";
            CrossPuzzleStatusText.Text = knowsClue
                ? "Three stone spirals, each in its own socket. They look as if they would turn."
                : "Three stone spirals, each in its own socket. The carving around them is worn smooth by many hands.";

            CrossPuzzleScreen.Visibility = Visibility.Visible;
        }

        private void CloseCrossPuzzle()
        {
            CrossPuzzleScreen.Visibility = Visibility.Collapsed;
        }

        private void CrossPuzzleBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            CloseCrossPuzzle();
        }

        private void SpiralDisc_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => TurnDisc(sender, e, +1);

        private void SpiralDisc_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => TurnDisc(sender, e, -1);

        private void TurnDisc(object sender, MouseButtonEventArgs e, int step)
        {
            e.Handled = true;

            if (_crossPuzzleSolving || sender is not Image { Tag: string spiral } disc)
            {
                return;
            }

            if (!CrossPuzzle.CanTurn(_player))
            {
                // Without the clue the stone won't give: a small shudder.
                var shudder = new DoubleAnimation(_discAngles[spiral], _discAngles[spiral] + 3 * step,
                                                  TimeSpan.FromMilliseconds(60))
                {
                    AutoReverse = true,
                    RepeatBehavior = new RepeatBehavior(2)
                };
                DiscRotation(disc).BeginAnimation(RotateTransform.AngleProperty, shudder);
                CrossPuzzleStatusText.Text = "You push the spiral. The stone won't move.";
                return;
            }

            bool solved = CrossPuzzle.Turn(_player, spiral, step);

            double from = DiscRotation(disc).Angle;
            double to = _discAngles[spiral] + 90 * step;
            _discAngles[spiral] = to;

            // Heavy stone: eases out with a slight settle at the end.
            var turn = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(520))
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.25 }
            };
            DiscRotation(disc).BeginAnimation(RotateTransform.AngleProperty, turn);

            CrossPuzzleStatusText.Text = step > 0
                ? "The spiral grinds a quarter turn under your hand."
                : "You force the spiral back a quarter turn.";

            if (solved)
            {
                SolveCrossPuzzle();
            }
        }

        // The spirals lock with a glow, then you step back as the plinth opens.
        private void SolveCrossPuzzle()
        {
            _crossPuzzleSolving = true;
            CrossPuzzleStatusText.Text = "Something clicks deep inside the stone. The spirals lock in place.";

            foreach (string spiral in CrossPuzzle.Spirals)
            {
                var glow = new DropShadowEffect { BlurRadius = 22, ShadowDepth = 0, Opacity = 0, Color = Color.FromRgb(0xFF, 0xC8, 0x6A) };
                SpiralDisc(spiral).Effect = glow;
                glow.BeginAnimation(DropShadowEffect.OpacityProperty,
                    new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(700)) { BeginTime = TimeSpan.FromMilliseconds(450) });
                glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                    new DoubleAnimation(22, 45, TimeSpan.FromMilliseconds(900)) { BeginTime = TimeSpan.FromMilliseconds(450) });
            }

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(2200) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                CloseCrossPuzzle();
                AfterStoryChange();      // the plinth picture opens
                FadeInScene();
                ShowReply("Somewhere below, stone slides against stone. The panel in the plinth has opened.");
            };
            timer.Start();
        }
    }
}
