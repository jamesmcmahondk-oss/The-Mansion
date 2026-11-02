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

        // Id of the node the conversation starts at.
        public string StartNode { get; set; } = "start";

        public Dictionary<string, DialogueNode> Nodes { get; set; } = new();
    }

    public class DialogueNode
    {
        // What the character says. Keep it to about two lines so the
        // choices still fit in the panel.
        public string Text { get; set; } = "";

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
    }
}
