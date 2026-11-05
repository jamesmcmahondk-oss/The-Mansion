using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * What goes into a save file. Everything the story remembers lives in
     * the player (stats, items, flags), plus which scene they are in.
     */
    public class SaveGame
    {
        public int Version { get; set; } = 1;
        public DateTime SavedAt { get; set; }
        public string SceneName { get; set; } = "";

        public string PlayerName { get; set; } = "";
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Strength { get; set; }
        public int Luck { get; set; }
        public int Wits { get; set; }
        public string Weapon { get; set; } = "None";
        public string StatusEffect { get; set; } = "None";
        public List<string> Inventory { get; set; } = new();
        public List<string> Flags { get; set; } = new();
        public List<string> Journal { get; set; } = new();

        public static SaveGame From(Player player, string sceneName) => new()
        {
            SavedAt = DateTime.Now,
            SceneName = sceneName,
            PlayerName = player.Name,
            Health = player.Health,
            MaxHealth = player.MaxHealth,
            Strength = player.Strength,
            Luck = player.Luck,
            Wits = player.Wits,
            Weapon = player.Weapon,
            StatusEffect = player.StatusEffect,
            Inventory = new List<string>(player.Inventory),
            Flags = new List<string>(player.Flags),
            Journal = new List<string>(player.Journal)
        };

        public Player ToPlayer()
        {
            var player = new Player
            {
                Name = PlayerName,
                Health = Health,
                MaxHealth = MaxHealth,
                Strength = Strength,
                Luck = Luck,
                Wits = Wits,
                Weapon = Weapon,
                StatusEffect = StatusEffect,
                Inventory = new List<string>(Inventory),
                Journal = new List<string>(Journal)
            };

            foreach (string flag in Flags)
            {
                player.SetFlag(flag);
            }

            return player;
        }
    }

    /*
     * Reads and writes save slots as small JSON files in
     *   %AppData%\The Mansion\Saves\slot1.json ... slot3.json (+ autosave.json)
     * (C:\Users\<you>\AppData\Roaming\The Mansion\Saves).
     */
    public static class SaveSystem
    {
        public const int SlotCount = 3;

        // Slot 0 is the autosave, written every time a new scene is entered.
        public const int AutosaveSlot = 0;

        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static string SaveFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "The Mansion",
            "Saves");

        private static string SlotPath(int slot) => Path.Combine(
            SaveFolder,
            slot == AutosaveSlot ? "autosave.json" : $"slot{slot}.json");

        // Returns an error message, or null if it worked.
        public static string? Save(int slot, SaveGame save)
        {
            try
            {
                Directory.CreateDirectory(SaveFolder);

                // Write a temporary file first and then swap it in, so a crash
                // or power cut mid-save can't leave a half-written save behind.
                string path = SlotPath(slot);
                string temp = path + ".tmp";
                File.WriteAllText(temp, JsonSerializer.Serialize(save, JsonOptions));
                File.Move(temp, path, overwrite: true);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // The save in a slot, or null if it's empty or unreadable.
        public static SaveGame? Load(int slot)
        {
            try
            {
                string path = SlotPath(slot);
                return File.Exists(path)
                    ? JsonSerializer.Deserialize<SaveGame>(File.ReadAllText(path))
                    : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool AnySaves()
        {
            for (int slot = AutosaveSlot; slot <= SlotCount; slot++)
            {
                if (Load(slot) != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
