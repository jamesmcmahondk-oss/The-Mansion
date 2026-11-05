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
    // The inventory panel, using items on hotspots and offering them in conversations.
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        // =========================================================
        // INVENTORY
        // =========================================================

        private bool _inventoryOpen;

        // Item picked from the inventory, waiting to be used on something.
        private string? _selectedItem;

        private static readonly Brush SlotBorderBrush =
            Paint.Rgb(0x80, 0x6B, 0x4A);

        private static readonly Brush SlotBackgroundBrush = Paint.Argb(0x99, 0, 0, 0);

        private static readonly Brush SlotHoverBrush =
            Paint.Rgb(0xFF, 0xD9, 0x8A);

        /*
         * Up to 10 items fit in the row. With more, the row shows 9 at a
         * time between ◀ ▶ arrows (the mouse wheel and the arrow keys page too).
         */
        private const int InventorySlotsWhenFits = 10;
        private const int InventorySlotsPerPage = 9;
        private int _inventoryPage;
        private string _inventoryHint = "";

        private List<string> InventoryItemNames()
        {
            // Same item twice? Show it once.
            var itemNames = new List<string>();
            foreach (string name in _player.Inventory)
            {
                if (!itemNames.Contains(name))
                {
                    itemNames.Add(name);
                }
            }
            return itemNames;
        }

        private int InventoryPageCount(int itemCount) =>
            itemCount <= InventorySlotsWhenFits
                ? 1
                : (itemCount + InventorySlotsPerPage - 1) / InventorySlotsPerPage;

        private void ShowInventoryPage()
        {
            InventorySlots.Children.Clear();

            List<string> itemNames = InventoryItemNames();
            int pages = InventoryPageCount(itemNames.Count);
            _inventoryPage = Math.Clamp(_inventoryPage, 0, pages - 1);

            if (pages == 1)
            {
                foreach (string name in itemNames)
                {
                    InventorySlots.Children.Add(CreateInventorySlot(Items.GetItem(name)));
                }

                InventoryHintText.Text = _inventoryHint;
                return;
            }

            InventorySlots.Children.Add(CreateInventoryPageArrow(-1, _inventoryPage > 0));

            foreach (string name in itemNames
                         .Skip(_inventoryPage * InventorySlotsPerPage)
                         .Take(InventorySlotsPerPage))
            {
                InventorySlots.Children.Add(CreateInventorySlot(Items.GetItem(name)));
            }

            // Keep the right arrow in the same place on a part-filled last page.
            int missing = InventorySlotsPerPage -
                (InventorySlots.Children.Count - 1);
            if (missing > 0)
            {
                InventorySlots.Children.Add(new Border { Width = missing * 128 });
            }

            InventorySlots.Children.Add(CreateInventoryPageArrow(+1, _inventoryPage < pages - 1));

            InventoryHintText.Text = $"Page {_inventoryPage + 1} of {pages}  ·  {_inventoryHint}";
        }

        private void TurnInventoryPage(int step)
        {
            if (!_inventoryOpen)
            {
                return;
            }

            int pages = InventoryPageCount(InventoryItemNames().Count);
            int page = Math.Clamp(_inventoryPage + step, 0, pages - 1);

            if (page != _inventoryPage)
            {
                _inventoryPage = page;
                ShowInventoryPage();
            }
        }

        private Border CreateInventoryPageArrow(int step, bool enabled)
        {
            var arrow = new Border
            {
                Width = 44,
                Height = 116,
                Margin = new Thickness(0, 0, step < 0 ? 12 : 0, 0),
                Background = SlotBackgroundBrush,
                BorderBrush = SlotBorderBrush,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(6),
                Opacity = enabled ? 1.0 : 0.3,
                Cursor = enabled ? Cursors.Hand : null,
                Child = new TextBlock
                {
                    Text = step < 0 ? "\u25C0" : "\u25B6",
                    Foreground = SlotHoverBrush,
                    FontSize = 26,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            if (enabled)
            {
                arrow.MouseEnter += (_, _) => arrow.BorderBrush = SlotHoverBrush;
                arrow.MouseLeave += (_, _) => arrow.BorderBrush = SlotBorderBrush;
            }

            arrow.MouseLeftButtonDown += (_, args) =>
            {
                args.Handled = true;
                TurnInventoryPage(step);
            };

            return arrow;
        }

        private void InventoryPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TurnInventoryPage(e.Delta > 0 ? -1 : +1);
            e.Handled = true;
        }

        private void OpenInventory()
        {
            _replyShowing = false;
            _inventoryPage = 0;

            List<string> itemNames = InventoryItemNames();

            InventoryDescriptionText.Text = itemNames.Count == 0
                ? "Your pockets are empty."
                : "Hover over an item to examine it.";

            if (IsInFight)
            {
                // Using something in the fight: inventory replaces the dice panel.
                FightPanel.Visibility = Visibility.Collapsed;
                _inventoryHint = "Click an item to use it in the fight  ·  Esc to go back";
            }
            else if (IsInDialogue && _activeDialogue != null)
            {
                // Offering something to the character: inventory replaces the conversation.
                DialoguePanel.Visibility = Visibility.Collapsed;
                _inventoryHint =
                    $"Click an item to offer it to {_activeDialogue.Speaker}  ·  Esc to go back";
            }
            else
            {
                _inventoryHint = "Click an item to use it  ·  Esc to close";
            }

            ShowInventoryPage();

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
                    icon = ImageCache.Get(item.Icon);
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
                    FontFamily = Georgia,
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
                Background = SlotBackgroundBrush,
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
                string name = item.IsMemento ? $"{item.Name} (Memento)" : item.Name;
                InventoryDescriptionText.Text = string.IsNullOrWhiteSpace(item.Description)
                    ? name
                    : $"{name}: {item.Description}";
            };

            slot.MouseLeave += (_, _) => slot.BorderBrush = SlotBorderBrush;

            slot.MouseLeftButtonDown += (_, args) =>
            {
                args.Handled = true;
                SelectItem(item.Name);
            };

            // Right-click: look at the item up close (if it has a picture).
            slot.MouseRightButtonDown += (_, args) =>
            {
                args.Handled = true;
                ShowPicture(string.IsNullOrWhiteSpace(item.Picture) ? item.Icon : item.Picture);
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
            _replyShowing = false;

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

        /*
         * Wrong item: the hotspot's own line (.OnOtherItems), else the
         * item's own line (misuse:), else a general variant.
         */
        private static string CantUseText(string itemName, Hotspot hotspot)
        {
            if (!string.IsNullOrWhiteSpace(hotspot.OtherItemsText))
            {
                return Replies.Fill(hotspot.OtherItemsText, itemName, hotspot.Name);
            }

            string misuse = Items.GetItem(itemName).MisuseText;

            return string.IsNullOrWhiteSpace(misuse)
                ? Replies.NoUse(itemName, hotspot.Name)
                : Replies.Fill(misuse, itemName, hotspot.Name);
        }

        private void UseSelectedItemOn(Hotspot hotspot)
        {
            if (_selectedItem == null)
            {
                return;
            }

            string itemName = _selectedItem;

            ItemUse? use = hotspot.ItemUses.TryGetValue(itemName, out List<ItemUse>? uses)
                ? uses.FirstOrDefault(u => u.Condition == null || u.Condition(_player))
                : null;

            if (use == null)
            {
                ShowReply(CantUseText(itemName, hotspot));
                return;
            }

            ShowReply(use.Text);

            if (use.Effect != null)
            {
                use.Effect(_player);
            }

            if (use.RemoveItem)
            {
                _player.Take(itemName);
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

            DialogueItemUse? use = _activeDialogue.ItemUses.TryGetValue(itemName, out List<DialogueItemUse>? uses)
                ? uses.FirstOrDefault(u => u.Condition == null || u.Condition(_player))
                : null;

            if (use != null)
            {
                use.Effect?.Invoke(_player);

                if (use.RemoveItem)
                {
                    _player.Take(itemName);
                }

                UpdatePlayerStats();
                RefreshSceneBackground();

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
    }
}
