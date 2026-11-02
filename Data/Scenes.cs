using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
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
                    X = 720,
                    Y = 588,
                    Width = 303,
                    Height = 241,

                    Description =
                        "An ancient stone fountain. Something glitters beneath the water.",

                    TargetScene = "FountainCloseup"
                },

                new Hotspot
                {
                    Name = "Front Door",
                    X = 1053,
                    Y = 579,
                    Width = 127,
                    Height = 183,

                    Description =
                        "The enormous wooden door looks like it has not opened in years.",

                    TargetScene = "FrontDoor"
                },
                new Hotspot
                {
                    Name = "Gargoyle",

           

                      X = 154,
                      Y = 233,
                     
                      Width = 265,
                      Height = 231,

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

                    X = 641,
                    Y = 797,
                    Width = 413,
                    Height = 164,

                    Description =
                        "Several old gold coins rest at the bottom of the fountain.",

                    TargetScene = ""
                },

                new Hotspot
                {
                    Name = "Return",

                    X = 10,
                    Y = 10,

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

                    X = 10,
                    Y = 10,

                    Width = 217,
                    Height = 109,

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

                    X = 658,
                    Y = 387,
                    Width = 457,
                    Height = 554,

                    Description =
                        "Heavy woodden front doors. They are locked.",

                    TargetScene = ""
                },

                new Hotspot
                {
                    Name = "Door Knocker",

                   X = 781,
                   Y = 599,
                   Width = 215,
                   Height = 125,

                    Description =
                        "The iron door knocker is freezing cold.",

                    TargetScene = ""
                },

                new Hotspot
                {
                    Name = "Return",

                    X = 10,
                    Y = 10,

                    Width = 217,
                    Height = 109,

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