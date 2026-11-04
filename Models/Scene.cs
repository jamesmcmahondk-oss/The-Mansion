using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Mansion.Models
{
    public class Scene
    {

        public string Name { get; set; } = "";

        public string BackgroundImage { get; set; } = "";

        public string Description { get; set; } = "";

        public List<Hotspot> Hotspots { get; set; }
            = new();

        // Other backgrounds to show once something has happened, e.g. the
        // door standing open. Checked top to bottom; the first one whose
        // condition is true wins. If none match, BackgroundImage is used.
        public List<SceneBackground> AlternateBackgrounds { get; set; }
            = new();

        // Pictures placed in the scene on top of the background, e.g. a
        // character sitting under a bench. Drawn in the order added.
        public List<SceneSprite> Sprites { get; set; }
            = new();

        public string GetBackground(Player player)
        {
            foreach (SceneBackground background in AlternateBackgrounds)
            {
                if (background.Condition(player))
                {
                    return background.Image;
                }
            }

            return BackgroundImage;
        }
    }

    public class SceneSprite
    {
        // Transparent PNG, e.g. "Assets/Characters/Ebria_UnderBench.png".
        public string Image { get; set; } = "";

        // Top-left corner and height in background image pixels (like
        // hotspots). The width follows from the picture's shape.
        public double X { get; set; }
        public double Y { get; set; }
        public double Height { get; set; }

        // Optional: only shown while this is true.
        public Func<Player, bool>? VisibleWhen { get; set; }
    }

    public class SceneBackground
    {
        public Func<Player, bool> Condition { get; set; } = _ => false;

        public string Image { get; set; } = "";
    }
}
