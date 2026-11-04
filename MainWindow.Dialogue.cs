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
    // Conversations (Data/Dialogues.cs).
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
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
            _replyShowing = false;

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
            HideHoverLabel();
            HideRevealedHotspots();
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
            _replyShowing = false;

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

            // Meeting someone (.MetFlag) or a choice may have earned a journal
            // entry; announce it now rather than at the next stats refresh.
            CheckJournal();

            if (_currentScene != null)
            {
                ShowDescription(_currentScene.Description);
            }
        }
    }
}
