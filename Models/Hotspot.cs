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

        // Close-up scene opened when the player uses LOOK on this hotspot
        // (e.g. the fountain close-up). TargetScene is for moving on to a new
        // place and is used by ACT.
        public string CloseUpScene { get; set; } = "";

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

        // Fight started when the player uses ACT on this hotspot (Data/Enemies.cs).
        public string ActFightId { get; set; } = "";

        // What happens when the player uses an item on this hotspot,
        // keyed by item name. Items not listed get a "nothing happens" reply.
        // Several reactions per item are allowed (see ItemUse.Condition).
        public Dictionary<string, List<ItemUse>> ItemUses { get; set; } = new();

        // Optional reply for any other item ({item} is filled in), e.g.
        // "The rose bush is unimpressed by the {item}."
        public string OtherItemsText { get; set; } = "";

        // Reply when the player uses TALK on it (when there's no conversation).
        public string TalkText { get; set; } = "";

        // Optional: TALK runs this and shows the text it returns, so the
        // reply can change (e.g. a different line the second time).
        public Func<Player, string>? TalkResult { get; set; }

        // Reply and optional effect when the player uses ACT on it
        // (when there's no ACT conversation).
        public string ActText { get; set; } = "";
        public Action<Player>? ActEffect { get; set; }

        // Optional: ACT runs this and shows the text it returns, so the reply
        // can depend on what happened (e.g. a Luck test).
        public Func<Player, string>? ActResult { get; set; }

        // Optional: a full-size picture shown after the ACT (e.g. the
        // photograph you have just found). Click to close.
        public string ActPicture { get; set; } = "";

        // Optional: the hotspot only exists while this returns true,
        // e.g. p => !p.HasFlag("TookKey"). Checked whenever the scene refreshes.
        public Func<Player, bool>? VisibleWhen { get; set; }
    }
}
