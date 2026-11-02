using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Mansion.Models
{
    public class Scene
    {

        public string Name { get; set; }

        public string BackgroundImage { get; set; }

        public string Description { get; set; }

        public List<Hotspot> Hotspots { get; set; }
            = new();

        public List<Character> Characters { get; set; }
            = new();
    }
}
