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

        // Position on the SceneCanvas
        public double X { get; set; }

        public double Y { get; set; }

        // Size of the clickable area
        public double Width { get; set; }

        public double Height { get; set; }

        // Text shown when the hotspot is clicked/examined
        public string Description { get; set; } = "";

        // Scene to load when this hotspot is activated.
        // Leave empty if clicking it should not change scene.
        public string TargetScene { get; set; } = "";
    }
}
