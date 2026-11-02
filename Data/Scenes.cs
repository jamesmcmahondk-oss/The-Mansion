using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * Hotspot X/Y/Width/Height are in pixels of the scene's background
     * image (0-1536 across, 0-1024 down). Only Y 192-832 of the image is
     * visible above the HUD. Return hotspots use ScreenSpace = true.
     */
    public static class Scenes
    {
        public static Scene FrontGate { get; } = new Scene
        {
            Name = "FrontGate",

            BackgroundImage =
                "Assets/Backgrounds/Mansion/frontgate.png",

            Description =
                "You are standing before an old and creepy mansion.",

            Hotspots =
            {
                new Hotspot
                {
                    Name = "Fountain",
                    X = 615,
                    Y = 545,
                    Width = 270,
                    Height = 205,

                    Description =
                        "An ancient stone fountain. Something glitters beneath the water.",

                    TargetScene = "FountainCloseup"
                },

                new Hotspot
                {
                    Name = "Front Door",
                    X = 905,
                    Y = 505,
                    Width = 95,
                    Height = 140,

                    Description =
                        "The enormous wooden door looks like it has not opened in years.",

                    TargetScene = "FrontDoor"
                },
                new Hotspot
                {
                    Name = "Gargoyle",

                    X = 135,
                    Y = 205,
                    Width = 205,
                    Height = 165,

                    Description = "A sulky and rather sad looking gargoyle sits here.",
                
                    TargetScene = "FrontGargoyle"
                },
                            }
        };

        public static Scene FountainCloseup { get; } = new Scene
        {
            Name = "FountainCloseup",

            BackgroundImage =
                "Assets/Backgrounds/Mansion/fountain_closeup.png",

            Description =
                "Lily pads float on the dark water. Beneath the surface, a small pile of gold coins glitters.",

            Hotspots =
            {
                new Hotspot
                {
                    Name = "Gold Coins",

                    X = 595,
                    Y = 655,
                    Width = 295,
                    Height = 175,

                    Description =
                        "Several old gold coins rest at the bottom of the fountain.",

                    TargetScene = ""
                },

                new Hotspot
                {
                    Name = "Return",

                    // Matches the Return arrow image (screen pixels).
                    ScreenSpace = true,
                    X = 10,
                    Y = 0,
                    Width = 220,
                    Height = 110,

                    Description =
                        "You step away from the fountain.",

                    TargetScene = "FrontGate"
                }
            }
        };
        public static Scene FrontGargoyle { get; } = new Scene
        {
            Name = "FrontGargoyle",

            BackgroundImage =
               "Assets/Backgrounds/Mansion/FrontGargoyle.png",

            Description =
               "The sad garogyle watches you carefully with its stoneface.",

             Hotspots =
            {
                new Hotspot
                {
                    Name = "Return",

                    // Matches the Return arrow image (screen pixels).
                    ScreenSpace = true,
                    X = 10,
                    Y = 0,
                    Width = 220,
                    Height = 110,

                    Description =
                        "You leave the sad gargoyle to its sad and boring fate.",

                    TargetScene = "FrontGate"
                }
        }
        };

        public static Scene FrontDoor { get; } = new Scene
        {
            Name = "FrontDoor",

            BackgroundImage =
                "Assets/Backgrounds/Mansion/Front_door.png",

            Description =
                "The heavy front door towers over you. A grotesque metal knocker hangs at its center.",

            Hotspots =
            {
                new Hotspot
                {
                    Name = "Front Doors",

                    X = 590,
                    Y = 300,
                    Width = 375,
                    Height = 500,

                    Description =
                        "Heavy woodden front doors. They are locked.",

                    TargetScene = ""
                },

                new Hotspot
                {
                    Name = "Door Knocker",

                   X = 685,
                    Y = 515,
                    Width = 175,
                    Height = 90,

                    Description =
                        "The iron door knocker is freezing cold.",

                    TargetScene = ""
                },

                new Hotspot
                {
                    Name = "Return",

                    // Matches the Return arrow image (screen pixels).
                    ScreenSpace = true,
                    X = 10,
                    Y = 0,
                    Width = 220,
                    Height = 110,

                    Description =
                        "You step away from the front door.",

                    TargetScene = "FrontGate"
                }
            }
        };

        private static readonly Dictionary<string, Scene> SceneLookup =
            new Dictionary<string, Scene>(
                StringComparer.OrdinalIgnoreCase)
            {
                { FrontGate.Name, FrontGate },
                { FountainCloseup.Name, FountainCloseup },
                { FrontDoor.Name, FrontDoor },
                {FrontGargoyle.Name, FrontGargoyle }
            };

        public static Scene? GetScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            SceneLookup.TryGetValue(sceneName, out Scene? scene);

            return scene;
        }
    }
}