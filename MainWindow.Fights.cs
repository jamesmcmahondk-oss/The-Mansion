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
    // Fights (Data/Enemies.cs), the game over screen and close-up portraits.
    // (Part of MainWindow - see MainWindow.xaml.cs.)
    public partial class MainWindow
    {
        // =========================================================
        // FIGHTS (Fighting Fantasy style - Data/Enemies.cs)
        // =========================================================

        private Fight? _fight;
        private bool _fightOver;
        private bool _diceRolling;
        private DispatcherTimer? _diceTimer;

        // Player state when the fight started, for "Try the fight again".
        private PlayerSnapshot? _fightSnapshot;
        private string _lastFightId = "";

        private bool IsInFight => _fight != null;

        private const int HitDamage = 10;
        private const int CriticalDamage = 20;

        private sealed class PlayerSnapshot
        {
            public int Health, Strength, Luck, Wits;
            public string Weapon = "", StatusEffect = "";
            public List<string> Inventory = new();
            public HashSet<string> Flags = new();
        }

        private PlayerSnapshot TakeSnapshot() => new()
        {
            Health = _player.Health,
            Strength = _player.Strength,
            Luck = _player.Luck,
            Wits = _player.Wits,
            Weapon = _player.Weapon,
            StatusEffect = _player.StatusEffect,
            Inventory = new List<string>(_player.Inventory),
            Flags = new HashSet<string>(_player.Flags, StringComparer.OrdinalIgnoreCase)
        };

        private void RestoreSnapshot(PlayerSnapshot snapshot)
        {
            _player.Health = snapshot.Health;
            _player.Strength = snapshot.Strength;
            _player.Luck = snapshot.Luck;
            _player.Wits = snapshot.Wits;
            _player.Weapon = snapshot.Weapon;
            _player.StatusEffect = snapshot.StatusEffect;
            _player.Inventory = new List<string>(snapshot.Inventory);
            _player.Flags = new HashSet<string>(snapshot.Flags, StringComparer.OrdinalIgnoreCase);
        }

        // Strength / 2 plus the weapon's bonus (no weapon = -1).
        private int PlayerAttackBonus(out string detail)
        {
            int strength = _player.Strength / 2;
            int weapon = string.IsNullOrWhiteSpace(_player.Weapon) || _player.Weapon == "None"
                ? -1
                : Items.GetItem(_player.Weapon).WeaponBonus;

            detail = $"STR {strength} {(weapon >= 0 ? "+" : "-")} {Math.Abs(weapon)} weapon";
            return strength + weapon;
        }

        private void StartFight(string enemyId)
        {
            _replyShowing = false;

            Enemy? enemy = Enemies.GetEnemy(enemyId);

            if (enemy == null)
            {
                MessageBox.Show(
                    $"The enemy '{enemyId}' was not found in Data/Enemies.cs.",
                    "Missing enemy",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (IsInDialogue)
            {
                EndDialogue();
            }

            CloseInventory();
            _selectedItem = null;

            _fight = new Fight(enemy);
            _fightOver = false;
            _diceRolling = false;
            _fightSnapshot = TakeSnapshot();
            _lastFightId = enemyId;

            // Enemy picture (same placement rules as dialogue portraits).
            PlaceCharacter(enemy.CharacterX, enemy.CharacterY, enemy.CharacterHeight);
            SetDialogueCharacter(enemy.Character);
            SetCloseUpPortrait(enemy.Portrait);

            DialogueDim.Fill = enemy.ShouldDimScene ? DimBrush : Brushes.Transparent;
            DialogueDim.Visibility = Visibility.Visible;
            HideHoverLabel();
            HideRevealedHotspots();

            DescriptionText.Visibility = Visibility.Collapsed;
            DialoguePanel.Visibility = Visibility.Collapsed;
            FightPanel.Visibility = Visibility.Visible;

            MenuButtons.Visibility = Visibility.Collapsed;
            FightButtons.Visibility = Visibility.Visible;
            SetFightButtonsEnabled(true);

            FightTitleText.Text = $"FIGHT!  {enemy.Name.ToUpperInvariant()}";
            EnemyLabelText.Text = enemy.Name.ToUpperInvariant();

            foreach (Image die in new[] { PlayerDie1, PlayerDie2, EnemyDie1, EnemyDie2 })
            {
                SetDieFace(die, 6);
                die.Opacity = 0.35;
            }

            PlayerAttackText.Text = "-";
            EnemyAttackText.Text = "-";
            PlayerAttackBonus(out string detail);
            PlayerAttackDetail.Text = $"2d6 + {detail}";
            EnemyAttackDetail.Text = $"2d6 + skill {enemy.Skill}";

            FightResultText.Text = string.IsNullOrWhiteSpace(enemy.IntroText)
                ? $"{enemy.Name} attacks! Roll the bones..."
                : enemy.IntroText;

            UpdateEnemyHealth();
            UpdatePlayerStats();
        }

        private void UpdateEnemyHealth()
        {
            if (_fight == null)
            {
                return;
            }

            int health = Math.Max(0, _fight.EnemyHealth);
            int max = Math.Max(1, _fight.Enemy.Health);

            EnemyHealthText.Text = $"{health}/{max}";
            EnemyHealthFill.Width = new GridLength(health, GridUnitType.Star);
            EnemyHealthEmpty.Width = new GridLength(max - Math.Min(health, max), GridUnitType.Star);
        }

        private void SetFightButtonsEnabled(bool enabled)
        {
            foreach (Image button in new[] { AttackBtn, FightItemBtn, FleeBtn })
            {
                button.IsHitTestVisible = enabled;
                button.Opacity = enabled ? 1 : 0.45;
            }
        }

        private static readonly Dictionary<string, (string Normal, string Glow)> FightButtonImages = new()
        {
            ["AttackBtn"] = ("Assets/UI/attack.png", "Assets/UI/attack_glow.png"),
            ["FightItemBtn"] = ("Assets/UI/item.png", "Assets/UI/item_glow.png"),
            ["FleeBtn"] = ("Assets/UI/flee.png", "Assets/UI/flee_glow.png")
        };

        private void FightButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Image button && FightButtonImages.TryGetValue(button.Name, out var images))
            {
                SetButtonImage(button, images.Glow);
            }
        }

        private void FightButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Image button && FightButtonImages.TryGetValue(button.Name, out var images))
            {
                SetButtonImage(button, images.Normal);
            }
        }

        private void FightButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (_fight == null || _fightOver || _diceRolling || sender is not Image button)
            {
                return;
            }

            switch (button.Name)
            {
                case "AttackBtn":
                    RollFightRound(playerCanHit: true, prefix: "");
                    break;

                case "FightItemBtn":
                    if (_inventoryOpen)
                    {
                        CloseInventory();
                    }
                    else
                    {
                        OpenInventory();
                    }
                    break;

                case "FleeBtn":
                    Flee();
                    break;
            }
        }

        // ---- Dice --------------------------------------------------------

        private void SetDieFace(Image die, int value) =>
            die.Source = LoadImage(DiePath(_settings.DiceSet, value), "die picture");

        // The picture of one face of a dice set (Settings > Dice).
        private static string DiePath(string diceSet, int value) =>
            string.IsNullOrWhiteSpace(diceSet)
                ? $"Assets/UI/Dice/die_{value}.png"
                : $"Assets/UI/Dice/{diceSet}/die_{value}.png";

        private static void SetDieAngle(Image die, double angle)
        {
            if (die.RenderTransform is RotateTransform rotate)
            {
                rotate.Angle = angle;
            }
        }

        /*
         * One round: roll everyone's dice with a short tumbling animation,
         * then work out who hits. playerCanHit = false is a free attack for
         * the enemy (after using an item): winning the roll just dodges.
         */
        private void RollFightRound(bool playerCanHit, string prefix)
        {
            if (_fight == null)
            {
                return;
            }

            _diceRolling = true;
            SetFightButtonsEnabled(false);

            int p1 = Dice.D6(), p2 = Dice.D6(), e1 = Dice.D6(), e2 = Dice.D6();

            Image[] dice = { PlayerDie1, PlayerDie2, EnemyDie1, EnemyDie2 };
            Random random = Random.Shared;   // just for the tumbling look; the result is rolled above
            int ticks = 0;
            const int totalTicks = 9;    // ~70 ms each = ~0.65 s

            foreach (Image die in dice)
            {
                die.Opacity = 1;
            }

            PlayerAttackText.Text = "...";
            EnemyAttackText.Text = "...";
            FightResultText.Text = string.IsNullOrEmpty(prefix) ? "The bones tumble..." : prefix;

            _diceTimer?.Stop();
            _diceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(70) };
            Sound.Play("dice_roll");
            _diceTimer.Tick += (_, _) =>
            {
                ticks++;

                if (ticks < totalTicks)
                {
                    // Tumbling: random faces, shaking angles (calming down).
                    double wobble = 28 * (1 - (double)ticks / totalTicks) + 6;
                    foreach (Image die in dice)
                    {
                        SetDieFace(die, random.Next(1, 7));
                        SetDieAngle(die, random.NextDouble() * 2 * wobble - wobble);
                    }
                    return;
                }

                _diceTimer.Stop();

                // Land on the real result, slightly askew like real dice.
                int[] values = { p1, p2, e1, e2 };
                for (int i = 0; i < dice.Length; i++)
                {
                    SetDieFace(dice[i], values[i]);
                    SetDieAngle(dice[i], random.NextDouble() * 12 - 6);
                }

                ResolveFightRound(p1, p2, e1, e2, playerCanHit, prefix);
            };
            _diceTimer.Start();
        }

        private void ResolveFightRound(int p1, int p2, int e1, int e2, bool playerCanHit, string prefix)
        {
            if (_fight == null)
            {
                return;
            }

            Enemy enemy = _fight.Enemy;

            int bonus = PlayerAttackBonus(out string detail);
            int playerTotal = p1 + p2 + bonus;
            int enemyTotal = e1 + e2 + enemy.Skill;

            PlayerAttackText.Text = playerTotal.ToString();
            PlayerAttackDetail.Text = $"{p1}+{p2} + {detail}";
            EnemyAttackText.Text = enemyTotal.ToString();
            EnemyAttackDetail.Text = $"{e1}+{e2} + skill {enemy.Skill}";

            bool playerCrit = p1 == 6 && p2 == 6;
            bool enemyCrit = e1 == 6 && e2 == 6;
            bool playerFumble = p1 == 1 && p2 == 1;
            bool enemyFumble = e1 == 1 && e2 == 1;

            // Who wins the round? +1 player, -1 enemy, 0 nobody.
            int winner;
            bool critical = false;
            string why = "";

            if (playerCrit && !enemyCrit)
            {
                winner = 1; critical = true; why = "Double six! ";
            }
            else if (enemyCrit && !playerCrit)
            {
                winner = -1; critical = true; why = $"{enemy.Name} rolls a double six! ";
            }
            else if (playerFumble && !enemyFumble)
            {
                winner = -1; why = "Snake eyes - you stumble! ";
            }
            else if (enemyFumble && !playerFumble)
            {
                winner = 1; why = $"{enemy.Name} rolls snake eyes and stumbles! ";
            }
            else
            {
                winner = Math.Sign(playerTotal - enemyTotal);
            }

            int damage = critical ? CriticalDamage : HitDamage;
            string outcome;

            if (winner > 0 && playerCanHit)
            {
                _fight.HurtEnemy(damage);
                Sound.Play("hit");
                outcome = critical
                    ? $"{why}A crushing blow strikes {enemy.Name}! (-{damage})"
                    : $"{why}You strike {enemy.Name}! (-{damage})";
            }
            else if (winner > 0)
            {
                outcome = $"{why}You dodge {enemy.Name}'s attack.";
            }
            else if (winner < 0)
            {
                _player.Health = Math.Max(0, _player.Health - damage);
                Sound.Play("hit");
                outcome = critical
                    ? $"{why}A vicious blow! (-{damage} Health)"
                    : $"{why}{enemy.Name} hits you! (-{damage} Health)";
            }
            else
            {
                outcome = "Your blows clash - neither of you is hurt.";
            }

            FightResultText.Text = string.IsNullOrEmpty(prefix) ? outcome : $"{prefix} {outcome}";

            UpdateEnemyHealth();
            UpdatePlayerStats();

            _diceRolling = false;

            if (_fight.EnemyDefeated)
            {
                WinFight();
            }
            else if (_player.Health <= 0)
            {
                LoseFight();
            }
            else
            {
                SetFightButtonsEnabled(true);
            }
        }

        // ---- Items, fleeing, the end -------------------------------------

        private void UseItemInFight(string itemName)
        {
            if (_fight == null)
            {
                return;
            }

            Item item = Items.GetItem(itemName);

            if (item.UseInFight == null)
            {
                FightResultText.Text = $"The {itemName} won't help in a fight.";
                return;   // no turn used
            }

            string message = item.UseInFight(_player, _fight);

            if (item.UsedUpInFight)
            {
                _player.Take(itemName);
            }

            UpdateEnemyHealth();
            UpdatePlayerStats();

            if (_fight.EnemyDefeated)
            {
                FightResultText.Text = message;
                WinFight();
                return;
            }

            // Using an item gives the enemy a free attack.
            RollFightRound(playerCanHit: false, prefix: message);
        }

        private void Flee()
        {
            if (_fight == null)
            {
                return;
            }

            Enemy enemy = _fight.Enemy;

            if (!enemy.CanFlee)
            {
                FightResultText.Text = "There is no escape - you must fight!";
                return;
            }

            _player.Health = Math.Max(0, _player.Health - HitDamage);
            UpdatePlayerStats();

            if (_player.Health <= 0)
            {
                FightResultText.Text = $"You turn to run - and {enemy.Name} strikes you down.";
                LoseFight();
                return;
            }

            enemy.OnFlee?.Invoke(_player);
            EndFight();
            ShowReply($"You flee from {enemy.Name}, taking a parting blow as you go. (-{HitDamage} Health)");
            AfterStoryChange();
        }

        private void WinFight()
        {
            if (_fight == null)
            {
                return;
            }

            _fightOver = true;
            FightButtons.Visibility = Visibility.Collapsed;

            // The beaten figure stays in the scene, if there is one.
            if (!string.IsNullOrWhiteSpace(_fight.Enemy.BeatenCharacter))
            {
                SetDialogueCharacter(_fight.Enemy.BeatenCharacter);
            }
            else
            {
                DialogueCharacter.Visibility = Visibility.Collapsed;
            }

            // Show the beaten close-up, if there is one.
            if (!string.IsNullOrWhiteSpace(_fight.Enemy.BeatenPortrait))
            {
                SetCloseUpPortrait(_fight.Enemy.BeatenPortrait);
            }

            FightResultText.Text =
                $"{FightResultText.Text}  You have defeated {_fight.Enemy.Name}!  (Click to continue)";
        }

        // Clicking the fight panel after a win carries on.
        private void FightPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_fight == null || !_fightOver || _player.Health <= 0)
            {
                return;
            }

            e.Handled = true;

            Enemy enemy = _fight.Enemy;
            enemy.OnWin?.Invoke(_player);

            EndFight();

            ShowReply(string.IsNullOrWhiteSpace(enemy.WinText)
                ? $"{enemy.Name} is defeated."
                : enemy.WinText);

            AfterStoryChange();
        }

        private void LoseFight()
        {
            _fightOver = true;
            SetFightButtonsEnabled(false);

            string killer = _fight?.Enemy.Name ?? "your foe";
            ShowGameOver($"Slain by {killer}. The Mansion claims another visitor.", canRetryFight: true);
        }

        // Puts the HUD back to normal after a fight.
        private void EndFight()
        {
            _diceTimer?.Stop();
            _fight = null;
            _fightOver = false;
            _diceRolling = false;

            CloseInventory();

            FightPanel.Visibility = Visibility.Collapsed;
            FightButtons.Visibility = Visibility.Collapsed;
            MenuButtons.Visibility = Visibility.Visible;
            SetMenuForDialogue(false);

            DialogueCharacter.Visibility = Visibility.Collapsed;
            DialogueCharacter.Source = null;
            _shownCharacter = "";
            HideCloseUpPortrait();
            DialogueDim.Visibility = Visibility.Collapsed;

            DescriptionText.Visibility = Visibility.Visible;
        }

        // ---- Game over ---------------------------------------------------

        private bool _gameOverShown;

        private void ShowGameOver(string text, bool canRetryFight)
        {
            if (_gameOverShown)
            {
                return;
            }

            _gameOverShown = true;

            GameOverText.Text = text;
            GameOverRetryButton.Visibility = canRetryFight && _fightSnapshot != null
                ? Visibility.Visible
                : Visibility.Collapsed;

            GameOverScreen.Visibility = Visibility.Visible;
        }

        private void GameOverRetry_Click(object sender, RoutedEventArgs e)
        {
            if (_fightSnapshot == null)
            {
                return;
            }

            _gameOverShown = false;
            GameOverScreen.Visibility = Visibility.Collapsed;

            RestoreSnapshot(_fightSnapshot);
            EndFight();
            UpdatePlayerStats();
            StartFight(_lastFightId);
        }

        private void GameOverRestart_Click(object sender, RoutedEventArgs e)
        {
            ResetToTitle();
        }

        /*
         * Places the portrait image: inside the scene at X/Y/height
         * (background image pixels), or on the right if height is 0.
         */
        // ---- Close-up portrait (.Portrait) ------------------------------

        // Where close-up portraits always appear, in background image pixels
        // (X, Y, height) - the same units as .StandAt(...).
        private const double PortraitSpotX = 60;
        private const double PortraitSpotY = 350;
        private const double PortraitSpotHeight = 400;

        private string _shownCloseUp = "";

        private void SetCloseUpPortrait(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                HideCloseUpPortrait();
                return;
            }

            if (imagePath == _shownCloseUp && DialoguePortrait.Source != null)
            {
                DialoguePortrait.Visibility = Visibility.Visible;
                return;
            }

            BitmapImage? portrait = LoadImage(imagePath, "portrait");
            if (portrait == null)
            {
                HideCloseUpPortrait();
                return;
            }

            DialoguePortrait.Source = portrait;
            _shownCloseUp = imagePath;

            Rect spot = ImageToScreen(new Rect(PortraitSpotX, PortraitSpotY, 0, PortraitSpotHeight));
            DialoguePortrait.Margin = new Thickness(spot.X, spot.Y, 0, 0);
            DialoguePortrait.Height = spot.Height;
            DialoguePortrait.Visibility = Visibility.Visible;
        }

        private void HideCloseUpPortrait()
        {
            DialoguePortrait.Visibility = Visibility.Collapsed;
            DialoguePortrait.Source = null;
            _shownCloseUp = "";
        }

        private void PlaceCharacter(double x, double y, double height)
        {
            _characterDecodeHeight = height > 0 ? (int)Math.Round(height) : CharacterDecodeHeight;

            if (height > 0)
            {
                Rect screen = ImageToScreen(new Rect(x, y, 0, height));

                DialogueCharacter.HorizontalAlignment = HorizontalAlignment.Left;
                DialogueCharacter.Margin = new Thickness(screen.X, screen.Y, 0, 0);
                DialogueCharacter.Height = screen.Height;
            }
            else
            {
                DialogueCharacter.HorizontalAlignment = HorizontalAlignment.Right;
                DialogueCharacter.Margin = new Thickness(0, 100, 140, 0);
                DialogueCharacter.Height = 700;
            }
        }
    }
}
