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
    // The journal (Data/Journal.cs).
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        // =========================================================
        // JOURNAL (Data/Journal.cs) - J, or JOURNAL in the game menu
        // =========================================================

        private Action? _journalBack;

        private bool IsJournalOpen => JournalScreen.Visibility == Visibility.Visible;

        private bool CanOpenJournal =>
            GameScreen.Visibility == Visibility.Visible &&
            !_gameOverShown &&
            !IsGameMenuOpen &&
            !IsSaveLoadOpen &&
            !IsSettingsOpen;

        /*
         * Adds any entries that have just come true and announces them.
         * Called whenever the stats are refreshed, i.e. after anything happens.
         */
        private void CheckJournal()
        {
            if (GameScreen.Visibility != Visibility.Visible)
            {
                return;
            }

            List<Journal.JournalEntry> added = Journal.Update(_player);

            if (added.Count > 0)
            {
                ShowJournalToast(string.Join("  ·  ", added.Select(entry => entry.Title)));
            }
        }

        private void ShowJournalToast(string titles)
        {
            JournalToastText.Text = titles;

            var fade = new DoubleAnimationUsingKeyFrames();
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3800))));
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(4600))));

            JournalToast.BeginAnimation(OpacityProperty, fade);
        }

        private void ShowJournal(Action? back)
        {
            _journalBack = back;
            CloseInventory();
            HideRevealedHotspots();

            // The notice isn't needed once you're reading it.
            JournalToast.BeginAnimation(OpacityProperty, null);
            JournalToast.Opacity = 0;

            JournalEntriesPanel.Children.Clear();

            // Newest first.
            for (int i = _player.Journal.Count - 1; i >= 0; i--)
            {
                Journal.JournalEntry? entry = Journal.Get(_player.Journal[i]);
                if (entry == null)
                {
                    continue;   // an entry removed since the game was saved
                }

                JournalEntriesPanel.Children.Add(new TextBlock
                {
                    Text = entry.Title,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xA9, 0x6A)),
                    FontFamily = new FontFamily("Georgia"),
                    FontWeight = FontWeights.Bold,
                    FontSize = 24,
                    TextWrapping = TextWrapping.Wrap
                });

                JournalEntriesPanel.Children.Add(new TextBlock
                {
                    Text = entry.Text,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xDD, 0xD4, 0xC3)),
                    FontFamily = new FontFamily("Georgia"),
                    FontSize = 19,
                    LineHeight = 28,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 26)
                });
            }

            int count = JournalEntriesPanel.Children.Count / 2;
            JournalSubtitleText.Text = count == 0
                ? "Nothing written yet. What you discover will be noted here."
                : $"{count} {(count == 1 ? "entry" : "entries")}, newest first  ·  J or Esc to close";

            JournalScroll.ScrollToTop();
            JournalScreen.Visibility = Visibility.Visible;
        }

        // goBack: return to where it was opened from (e.g. the game menu).
        private void CloseJournal(bool goBack = true)
        {
            if (!IsJournalOpen)
            {
                return;
            }

            JournalScreen.Visibility = Visibility.Collapsed;

            Action? back = _journalBack;
            _journalBack = null;

            if (goBack)
            {
                back?.Invoke();
            }
        }

        private void JournalClose_Click(object sender, RoutedEventArgs e) => CloseJournal();
    }
}
