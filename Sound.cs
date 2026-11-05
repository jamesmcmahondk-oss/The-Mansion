using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace The_Mansion
{
    /*
     * Sound effects and looping background sounds.
     *
     *   Sound.Play("stone_grind");        a one-off effect
     *   Sound.Ambience("rain_loop");       a looping background (replaces the last one)
     *   Sound.Ambience("");                silence
     *
     * Files live in Assets/Sounds as name.wav or name.mp3 and are copied next
     * to the game when it is built (WPF can't play sounds packed inside the
     * program). A missing file is simply silent, so sounds can be added one
     * at a time. Volumes come from Settings (0-100).
     */
    internal static class Sound
    {
        private static readonly string Folder =
            Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds");

        public static double EffectsVolume { get; set; } = 0.8;
        public static double MusicVolume { get; set; } = 0.6;

        // Players still playing (kept so they aren't collected mid-sound).
        private static readonly List<MediaPlayer> Playing = new();

        private static MediaPlayer? _ambience;
        private static string _ambienceName = "";

        private static string? Find(string name)
        {
            foreach (string ext in new[] { ".wav", ".mp3" })
            {
                string path = Path.Combine(Folder, name + ext);
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }

        public static void Play(string name, double volume = 1.0)
        {
            string? path = Find(name);
            if (path == null || EffectsVolume <= 0)
            {
                return;
            }

            var player = new MediaPlayer { Volume = Math.Clamp(EffectsVolume * volume, 0, 1) };
            player.MediaEnded += (_, _) => { player.Close(); Playing.Remove(player); };
            player.MediaFailed += (_, _) => { player.Close(); Playing.Remove(player); };
            Playing.Add(player);
            player.Open(new Uri(path));
            player.Play();
        }

        public static void Ambience(string name)
        {
            if (name == _ambienceName)
            {
                return;
            }

            _ambienceName = name;
            _ambience?.Stop();
            _ambience?.Close();
            _ambience = null;

            string? path = string.IsNullOrWhiteSpace(name) ? null : Find(name);
            if (path == null)
            {
                return;
            }

            _ambience = new MediaPlayer { Volume = MusicVolume };
            _ambience.MediaEnded += (_, _) =>
            {
                _ambience.Position = TimeSpan.Zero;   // loop
                _ambience.Play();
            };
            _ambience.Open(new Uri(path));
            _ambience.Play();
        }

        public static void ApplyMusicVolume()
        {
            if (_ambience != null)
            {
                _ambience.Volume = MusicVolume;
            }
        }
    }
}
