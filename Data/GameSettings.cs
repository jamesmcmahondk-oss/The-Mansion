using System;
using System.IO;
using System.Text.Json;

namespace The_Mansion.Data
{
    /*
     * Player settings, remembered between games in
     *   %AppData%\The Mansion\settings.json
     *
     * The game is always laid out on a 1920x1080 "canvas"; the resolution
     * just scales that canvas, so hotspots and everything else line up at
     * every size.
     */
    public class GameSettings
    {
        // Window size in real screen pixels (16:9).
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;

        // Fill the whole monitor instead (black bars if it isn't 16:9).
        public bool Fullscreen { get; set; }

        // Sound effects and background (rain, ambience) volume, 0-100.
        public int EffectsVolume { get; set; } = 80;
        public int MusicVolume { get; set; } = 60;

        // Which dice the fights use: a folder in Assets/UI/Dice
        // ("" = the original dice in Assets/UI/Dice itself).
        public string DiceSet { get; set; } = "bone";

        public static readonly (string Folder, string Name)[] DiceSets =
        {
            ("bone", "Bone & Blood"),
            ("bronze", "Aged Bronze"),
            ("crimson", "Crimson Glass"),
            ("moonlit", "Moonlit Stone"),
            ("", "Old Bones (original)")
        };

        // The sizes offered in the Settings screen. All 16:9 so the game
        // never stretches.
        public static readonly (int Width, int Height)[] Resolutions =
        {
            (1280, 720),
            (1600, 900),
            (1920, 1080),
            (2560, 1440),
            (3840, 2160)
        };

        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "The Mansion",
            "settings.json");

        public static GameSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    return JsonSerializer.Deserialize<GameSettings>(File.ReadAllText(FilePath))
                           ?? new GameSettings();
                }
            }
            catch (Exception)
            {
                // Unreadable settings: just use the defaults.
            }

            return new GameSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this,
                    new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                // Not being able to save settings shouldn't stop the game.
            }
        }
    }
}
