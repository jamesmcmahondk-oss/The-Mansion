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

        private static readonly Dictionary<string, Dialogue> DialogueLookup =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "SadGargoyle", SadGargoyle }
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
