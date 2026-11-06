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
                foreach (Journal.JournalEntry entry in added)
                {
                    _unreadJournal.Add(entry.Id);
                }

                Sound.Play("journal_entry");
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

        // Ink colours on the journal's paper.
        private static readonly Brush JournalInk = Paint.Rgb(0x3A, 0x24, 0x12);
        private static readonly Brush JournalFadedInk = Paint.Rgb(0x6B, 0x4E, 0x31);
        private static readonly Brush JournalRedInk = Paint.Rgb(0x7A, 0x1E, 0x14);

        // Entries not read yet this session (marked on the contents page).
        private readonly HashSet<string> _unreadJournal = new(StringComparer.OrdinalIgnoreCase);
        private string _journalSelected = "";
        private string _journalTab = Journal.People;

        private void ShowJournal(Action? back)
        {
            _journalBack = back;
            CloseInventory();
            HideRevealedHotspots();

            // The notice isn't needed once you're reading it.
            JournalToast.BeginAnimation(OpacityProperty, null);
            JournalToast.Opacity = 0;

            List<Journal.JournalEntry> entries = JournalEntriesNewestFirst();

            // Open on the newest unread entry, else the newest one.
            Journal.JournalEntry? first =
                entries.FirstOrDefault(entry => _unreadJournal.Contains(entry.Id)) ??
                entries.FirstOrDefault();

            if (first != null)
            {
                _journalTab = first.Category;
            }

            ShowJournalTab(_journalTab, first);
            Sound.Play("journal_open");
            JournalScreen.Visibility = Visibility.Visible;
        }

        private List<Journal.JournalEntry> JournalEntriesNewestFirst()
        {
            var entries = new List<Journal.JournalEntry>();

            for (int i = _player.Journal.Count - 1; i >= 0; i--)
            {
                Journal.JournalEntry? entry = Journal.Get(_player.Journal[i]);
                if (entry != null)      // null: an entry removed since the game was saved
                {
                    entries.Add(entry);
                }
            }

            return entries;
        }

        // Switches the left page to one tab (People / Puzzles / Mementos) and
        // shows the given entry, or that tab's newest one.
        private void ShowJournalTab(string tab, Journal.JournalEntry? show = null)
        {
            _journalTab = tab;
            List<Journal.JournalEntry> entries = JournalEntriesNewestFirst()
                .Where(entry => entry.Category == tab).ToList();

            show ??= entries.FirstOrDefault(entry => _unreadJournal.Contains(entry.Id)) ??
                     entries.FirstOrDefault();
            _journalSelected = show?.Id ?? "";

            JournalSubtitleText.Text = entries.Count == 0
                ? "Nothing written here yet."
                : $"{entries.Count} {(entries.Count == 1 ? "entry" : "entries")}, newest first";

            ShowJournalEntry(show);
            BuildJournalTabs();
            BuildJournalContents(entries);
            JournalScroll.ScrollToTop();
        }

        private void BuildJournalTabs()
        {
            JournalTabs.Children.Clear();
            List<Journal.JournalEntry> all = JournalEntriesNewestFirst();

            foreach (string tab in Journal.Categories)
            {
                bool current = tab == _journalTab;
                int count = all.Count(entry => entry.Category == tab);
                bool unread = all.Any(entry => entry.Category == tab && _unreadJournal.Contains(entry.Id));

                var label = new TextBlock
                {
                    FontFamily = Georgia,
                    FontSize = 21,
                    Margin = new Thickness(0, 0, 26, 0),
                    Foreground = current ? JournalRedInk : JournalFadedInk,
                    FontWeight = current ? FontWeights.Bold : FontWeights.Normal,
                    Cursor = Cursors.Hand,
                    Background = Brushes.Transparent
                };

                label.Inlines.Add(new System.Windows.Documents.Run($"{tab} ({count})"));
                if (unread)
                {
                    label.Inlines.Add(new System.Windows.Documents.Run(" *")
                    {
                        Foreground = JournalRedInk,
                        FontWeight = FontWeights.Bold
                    });
                }

                label.MouseEnter += (_, _) => label.TextDecorations = TextDecorations.Underline;
                label.MouseLeave += (_, _) => label.TextDecorations = null;
                label.MouseLeftButtonDown += (_, args) =>
                {
                    args.Handled = true;
                    if (tab != _journalTab)
                    {
                        Sound.Play("journal_open");
                        ShowJournalTab(tab);
                    }
                };

                JournalTabs.Children.Add(label);
            }
        }

        // The left page: one line per entry. The chosen one is in red ink,
        // unread ones are marked "new".
        private void BuildJournalContents(List<Journal.JournalEntry> entries)
        {
            JournalEntriesPanel.Children.Clear();

            foreach (Journal.JournalEntry entry in entries)
            {
                bool selected = entry.Id == _journalSelected;

                var line = new TextBlock
                {
                    FontFamily = Georgia,
                    FontSize = 22,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(8, 0, 8, 14),
                    Foreground = selected ? JournalRedInk : JournalInk,
                    FontWeight = selected ? FontWeights.Bold : FontWeights.Normal,
                    Cursor = Cursors.Hand
                };

                line.Inlines.Add(new System.Windows.Documents.Run(entry.Title));

                if (_unreadJournal.Contains(entry.Id))
                {
                    line.Inlines.Add(new System.Windows.Documents.Run("   new")
                    {
                        Foreground = JournalRedInk,
                        FontStyle = FontStyles.Italic,
                        FontSize = 16
                    });
                }

                line.MouseEnter += (_, _) => line.TextDecorations = TextDecorations.Underline;
                line.MouseLeave += (_, _) => line.TextDecorations = null;
                line.MouseLeftButtonDown += (_, args) =>
                {
                    args.Handled = true;
                    _journalSelected = entry.Id;
                    ShowJournalTab(_journalTab, entry);
                };

                JournalEntriesPanel.Children.Add(line);
            }
        }

        // The right page.
        private void ShowJournalEntry(Journal.JournalEntry? entry)
        {
            if (entry == null)
            {
                JournalEntryTitle.Text = "";
                JournalEntryText.Text = "What you discover will be written here.";
                JournalEntryText.Foreground = JournalFadedInk;
                JournalEntryText.FontStyle = FontStyles.Italic;
                PlaceJournalPicture(null);
                return;
            }

            _unreadJournal.Remove(entry.Id);

            JournalEntryTitle.Text = entry.Title;
            JournalEntryText.Text = string.IsNullOrWhiteSpace(entry.Where)
                ? entry.TextFor(_player)
                : $"Where: {entry.Where}\n\n{entry.TextFor(_player)}";
            PlaceJournalPicture(string.IsNullOrWhiteSpace(entry.Picture)
                ? null
                : LoadImage(entry.Picture, "journal picture"));
            JournalEntryText.Foreground = JournalInk;
            JournalEntryText.FontStyle = FontStyles.Normal;
            JournalEntryScroll.ScrollToTop();
        }

        // With a picture, the title and text move down below it.
        private void PlaceJournalPicture(ImageSource? picture)
        {
            JournalEntryPicture.Source = picture;
            JournalEntryPicture.Visibility = picture != null ? Visibility.Visible : Visibility.Collapsed;

            double titleTop = picture != null ? 395 : 110;
            double textTop = picture != null ? 450 : 200;
            Canvas.SetTop(JournalEntryTitle, titleTop);
            Canvas.SetTop(JournalEntryScroll, textTop);
            JournalEntryScroll.Height = 790 - textTop;
        }

        // Clicking the dark area around the book closes it.
        private void JournalScreen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == JournalScreen)
            {
                CloseJournal();
                e.Handled = true;
            }
        }

        private void JournalClose_MouseEnter(object sender, MouseEventArgs e) =>
            JournalCloseText.Foreground = JournalRedInk;

        private void JournalClose_MouseLeave(object sender, MouseEventArgs e) =>
            JournalCloseText.Foreground = JournalFadedInk;

        private void JournalClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            CloseJournal();
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

    }
}
