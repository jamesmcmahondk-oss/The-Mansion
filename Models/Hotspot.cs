using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Mansion.Models
{
    public class Hotspot
    {
        // Name used to identify the hotspot
        public string Name { get; set; } = "";

        // Position and size of the clickable area, in pixels of the scene's
        // background image (0-1536 across, 0-1024 down), NOT screen pixels.
        // Only the middle band (Y 192-832) of the image is visible in game.
        // Use hotspot debug mode to measure these - it reports image pixels.
        public double X { get; set; }

        public double Y { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        // Text shown when the hotspot is clicked/examined
        public string Description { get; set; } = "";

        // Scene to load when this hotspot is activated.
        // Leave empty if clicking it should not change scene.
        public string TargetScene { get; set; } = "";

        // True for hotspots on fixed screen UI (like the Return arrow) rather
        // than on the background picture: X/Y/Width/Height are then plain
        // 1920x1080 screen pixels.
        public bool ScreenSpace { get; set; }

        // Conversation started when the player uses TALK on this hotspot.
        // Must match a key in Data/Dialogues.cs. Leave empty if it can't talk.
        public string DialogueId { get; set; } = "";

        // Conversation started when the player uses ACT on this hotspot,
        // e.g. a character who appears when you touch something.
        public string ActDialogueId { get; set; } = "";

        // What happens when the player uses an item on this hotspot,
        // keyed by item name. Items not listed get a "nothing happens" reply.
        public Dictionary<string, ItemUse> ItemUses { get; set; } = new();
    }
}
