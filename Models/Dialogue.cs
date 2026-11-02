using System;
using System.Collections.Generic;

namespace The_Mansion.Models
{
    /*
     * A conversation with one character.
     *
     * A dialogue is a set of nodes. Each node is one thing the character
     * says, followed by the choices the player can answer with. Each choice
     * points at the next node to show, or at nothing (null) to end the
     * conversation.
     */
    public class Dialogue
    {
        // Name shown above the character's lines, e.g. "Sad Gargoyle".
        public string Speaker { get; set; } = "";

        // Optional character picture shown beside the conversation,
        // e.g. "Assets/Characters/Pip.png". Leave empty for none
        // (for example when the scene itself is a close-up of the character).
        public string Portrait { get; set; } = "";

        // Where the portrait appears. Leave PortraitHeight at 0 for the
        // default spot (standing on the right of the scene). Set all three
        // to put the character *in* the scene instead, e.g. on a lily pad.
        // Same units as hotspots: pixels of the scene's background image
        // (0-1536 across, 0-1024 down; only Y 192-832 is visible).
        public double PortraitX { get; set; }
        public double PortraitY { get; set; }
        public double PortraitHeight { get; set; }

        // Darken the scene behind the conversation. Turn off for characters
        // standing in the scene so they still look part of it.
        public bool DimScene { get; set; } = true;

        // Id of the node the conversation starts at.
        public string StartNode { get; set; } = "start";

        // Optional: pick the start node from the player's state instead,
        // e.g. a different greeting the second time you meet.
        public Func<Player, string>? ChooseStartNode { get; set; }

        public Dictionary<string, DialogueNode> Nodes { get; set; } = new();

        // What happens when the player offers an item (ITEM button during
        // the conversation), keyed by item name.
        public Dictionary<string, DialogueItemUse> ItemUses { get; set; }
            = new(StringComparer.OrdinalIgnoreCase);

        // Reply to any other item. "{item}" is replaced by the item's name.
        // Empty = "<Speaker> shows no interest in the <item>."
        public string OtherItemText { get; set; } = "";
    }

    public class DialogueItemUse
    {
        // Line to jump to. null ends the conversation.
        public string? GoesTo { get; set; }

        // Optional: runs when the item is offered.
        public Action<Player>? Effect { get; set; }

        // Take the item away (the character keeps it).
        public bool RemoveItem { get; set; }

        // Optional: only react like this when true; otherwise it counts
        // as "any other item".
        public Func<Player, bool>? Condition { get; set; }
    }

    public class DialogueNode
    {
        // What the character says. Keep it to about two lines so the
        // choices still fit in the panel.
        public string Text { get; set; } = "";

        // Optional: swap the character's picture for this line only,
        // e.g. "Assets/Characters/Hautain_Attack.png" when they get angry.
        public string Portrait { get; set; } = "";

        // Up to 4 choices are shown. A node with no choices gets an
        // automatic "(Leave)" choice.
        public List<DialogueChoice> Choices { get; set; } = new();
    }

    public class DialogueChoice
    {
        // What the player says.
        public string Text { get; set; } = "";

        // Node to go to next. null ends the conversation.
        public string? Next { get; set; }

        // Optional: the choice is only shown when this returns true,
        // e.g. p => p.Wits >= 12
        public Func<Player, bool>? Condition { get; set; }

        // Optional: runs when the choice is picked, e.g. to change stats
        // or give an item: p => p.Luck += 1
        public Action<Player>? Effect { get; set; }

        // Optional: end the conversation and start this fight (Data/Enemies.cs).
        public string StartsFight { get; set; } = "";
    }
}
