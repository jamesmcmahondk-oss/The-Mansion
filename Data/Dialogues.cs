using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * All conversations in the game. A hotspot starts one when the player
     * uses Talk on it and the hotspot's DialogueId matches a key here.
     */
    public static class Dialogues
    {
        private const string GargoyleBlessing = "Gargoyle's Blessing";

        // Choices the gargoyle offers whenever it is waiting for you.
        private static List<DialogueChoice> GargoyleChoices() => new()
        {
            new DialogueChoice
            {
                Text = "Hello! Why so glum?",
                Next = "sad"
            },
            new DialogueChoice
            {
                Text = "Do you know how to get into the mansion?",
                Next = "door"
            },
            new DialogueChoice
            {
                // Only clever heroes notice this - and only once.
                Text = "You're not entirely stone, are you?",
                Next = "secret",
                Condition = p => p.Wits >= 12 &&
                                 !p.Inventory.Contains(GargoyleBlessing),
                Effect = p =>
                {
                    p.Luck += 1;
                    p.Inventory.Add(GargoyleBlessing);
                }
            },
            new DialogueChoice
            {
                Text = "Goodbye.",
                Next = null
            }
        };

        public static Dialogue SadGargoyle { get; } = new Dialogue
        {
            Speaker = "Sad Gargoyle",
            Portrait = "",            // the scene is already a close-up
            StartNode = "start",
            Nodes =
            {
                ["start"] = new DialogueNode
                {
                    Text = "*Sigh.* Three hundred years on this pillar, and not one visitor has ever said hello.",
                    Choices = GargoyleChoices()
                },
                ["menu"] = new DialogueNode
                {
                    Text = "Was there something else, visitor? I have nowhere to be. Ever.",
                    Choices = GargoyleChoices()
                },
                ["sad"] = new DialogueNode
                {
                    Text = "Pigeons. Rain. More pigeons. And the gargoyle on the other pillar hasn't spoken to me since the Incident.",
                    Choices =
                    {
                        new DialogueChoice { Text = "What incident?", Next = "incident" },
                        new DialogueChoice { Text = "That sounds dreadful.", Next = "menu" }
                    }
                },
                ["incident"] = new DialogueNode
                {
                    Text = "We do not speak of the Incident. There was a bird. That is all I will say.",
                    Choices =
                    {
                        new DialogueChoice { Text = "Fair enough.", Next = "menu" }
                    }
                },
                ["door"] = new DialogueNode
                {
                    Text = "Locked, of course. They say the old door knocker only answers to someone with manners.",
                    Choices =
                    {
                        new DialogueChoice { Text = "Thanks for the tip.", Next = "menu" }
                    }
                },
                ["secret"] = new DialogueNode
                {
                    Text = "Shh! Not so loud! ...Here, take my blessing. You will need all the luck you can get in there. (Luck +1)",
                    Choices =
                    {
                        new DialogueChoice { Text = "Your secret is safe with me.", Next = "menu" }
                    }
                }
            }
        };

        // =========================================================
        // KING HAUTAIN - appears on the lily pad when you ACT on the
        // gold coins in the fountain close-up.
        // =========================================================

        private const string GoldCoins = "Gold Coins";
        private const string GoldCoin = "Royal Gold Coin";

        private static List<DialogueChoice> HautainChoices() => new()
        {
            new DialogueChoice
            {
                Text = "King? You're a frog on a lily pad.",
                Next = "insult"
            },
            new DialogueChoice
            {
                Text = "Forgive me, Your Majesty. I'll leave your gold alone.",
                Next = "apology"
            },
            new DialogueChoice
            {
                // Clever heroes can talk their way into a coin.
                Text = "Surely the Crown can spare a coin for a loyal subject?",
                Next = "flattered",
                Condition = p => p.Wits >= 12,
                Effect = p => p.Inventory.Add(GoldCoin)
            },
            new DialogueChoice
            {
                Text = "Finders keepers! (Grab the coins)",
                Next = "grab",
                Effect = p =>
                {
                    p.Inventory.Add(GoldCoins);
                    p.StatusEffect = "Royal Grudge";
                }
            }
        };

        public static Dialogue HautainCoins { get; } = new Dialogue
        {
            Speaker = "King Hautain",
            Portrait = "Assets/Characters/Hautain.png",

            // Standing on the big lily pad (background image pixels).
            PortraitX = 495,
            PortraitY = 205,
            PortraitHeight = 285,
            DimScene = false,

            // Already took the coins (or was given one)? He remembers.
            ChooseStartNode = p =>
                p.Inventory.Contains(GoldCoins) || p.Inventory.Contains(GoldCoin)
                    ? "again"
                    : "start",

            Nodes =
            {
                ["start"] = new DialogueNode
                {
                    Text = "...HALT, THIEF! Those coins belong to the Crown! I am Hautain, rightful king of this fountain, and you are trespassing!",
                    Choices = HautainChoices()
                },
                ["menu"] = new DialogueNode
                {
                    Text = "Well? What have you to say for yourself, thief?",
                    Choices = HautainChoices()
                },
                ["insult"] = new DialogueNode
                {
                    Text = "A FROG?! My ancestors have ruled this pond for three hundred years! Guards! GUARDS! ...Oh. I have no guards.",
                    Portrait = "Assets/Characters/Hautain_Attack.png",
                    Choices =
                    {
                        new DialogueChoice { Text = "Awkward.", Next = "menu" }
                    }
                },
                ["apology"] = new DialogueNode
                {
                    Text = "Hmph. At least SOMEONE in this wretched mansion has manners. You may withdraw from the royal presence.",
                    Choices =
                    {
                        new DialogueChoice { Text = "(Bow and back away)", Next = null }
                    }
                },
                ["flattered"] = new DialogueNode
                {
                    Text = "A loyal... subject? Nobody has called me that in years. Very well - ONE coin. Spend it wisely, and speak well of your king.",
                    Choices =
                    {
                        new DialogueChoice { Text = "Long live the King!", Next = null }
                    }
                },
                ["grab"] = new DialogueNode
                {
                    Text = "You DARE?! Mark my words, thief - the Crown never forgets! NEVER!",
                    Portrait = "Assets/Characters/Hautain_Attack.png",
                    Choices =
                    {
                        new DialogueChoice { Text = "(Run!)", Next = null }
                    }
                },
                ["again"] = new DialogueNode
                {
                    Text = "YOU again! Haven't you robbed the Crown enough for one day?",
                    Choices =
                    {
                        new DialogueChoice { Text = "Just admiring the view, Your Majesty.", Next = null }
                    }
                }
            }
        };

        private static readonly Dictionary<string, Dialogue> DialogueLookup =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "SadGargoyle", SadGargoyle },
                { "HautainCoins", HautainCoins }
            };

        public static Dialogue? GetDialogue(string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(dialogueId))
            {
                return null;
            }

            DialogueLookup.TryGetValue(dialogueId, out Dialogue? dialogue);
            return dialogue;
        }
    }
}
