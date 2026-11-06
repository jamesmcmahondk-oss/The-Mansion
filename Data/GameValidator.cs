using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * Start-up check of the game data (Debug builds only).
     *
     * Everything in Data/*.cs refers to other things by name - scenes, items,
     * conversations, fights and picture files. A typo there doesn't stop the
     * game; it just quietly breaks something you only find by playing. This
     * goes through all of it when the game starts and lists anything that
     * points at something that doesn't exist.
     *
     * Story flags (p.HasFlag("...")) are written as code, so they can't be
     * checked here.
     */
    public static class GameValidator
    {
        public static List<string> FindProblems()
        {
            var problems = new List<string>();
            void Problem(string where, string what) => problems.Add($"{where}: {what}");

            void CheckScene(string where, string sceneName)
            {
                if (!string.IsNullOrWhiteSpace(sceneName) && Scenes.GetScene(sceneName) == null)
                    Problem(where, $"scene '{sceneName}' doesn't exist");
            }

            void CheckItem(string where, string itemName)
            {
                if (!string.IsNullOrWhiteSpace(itemName) && !Items.Exists(itemName))
                    Problem(where, $"item '{itemName}' isn't in Data/Items.cs");
            }

            void CheckDialogue(string where, string dialogueId)
            {
                if (!string.IsNullOrWhiteSpace(dialogueId) && Dialogues.GetDialogue(dialogueId) == null)
                    Problem(where, $"conversation '{dialogueId}' doesn't exist");
            }

            void CheckFight(string where, string enemyId)
            {
                if (!string.IsNullOrWhiteSpace(enemyId) && Enemies.GetEnemy(enemyId) == null)
                    Problem(where, $"fight '{enemyId}' isn't in Data/Enemies.cs");
            }

            void CheckImage(string where, string path)
            {
                if (!string.IsNullOrWhiteSpace(path) && !ResourceExists(path))
                    Problem(where, $"picture '{path}' wasn't found (missing, misspelt, or not a Resource in the .csproj)");
            }

            // ---- Scenes and hotspots ---------------------------------------
            foreach (Scene scene in Scenes.All)
            {
                string s = $"Scene {scene.Name}";

                CheckImage(s, scene.BackgroundImage);

                foreach (SceneBackground background in scene.AlternateBackgrounds)
                    CheckImage($"{s}, .BackgroundWhen", background.Image);

                foreach (SceneSprite sprite in scene.Sprites)
                    CheckImage($"{s}, .Sprite", sprite.Image);

                foreach (Hotspot hotspot in scene.Hotspots)
                {
                    string h = $"{s}, hotspot '{hotspot.Name}'";

                    CheckScene(h, hotspot.TargetScene);
                    CheckScene(h, hotspot.CloseUpScene);
                    CheckDialogue($"{h} .TalkStarts", hotspot.DialogueId);
                    CheckDialogue($"{h} .ActStarts", hotspot.ActDialogueId);
                    CheckFight($"{h} .ActStartsFight", hotspot.ActFightId);
                    CheckImage($"{h} .ShowsPicture", hotspot.ActPicture);

                    foreach (var use in hotspot.ItemUses)
                    {
                        CheckItem($"{h} .OnItem", use.Key);

                        foreach (ItemUse reaction in use.Value)
                            CheckDialogue($"{h} .OnItem(\"{use.Key}\")", reaction.DialogueId);
                    }
                }
            }

            // ---- Conversations ---------------------------------------------
            foreach (var (id, dialogue) in Dialogues.All)
            {
                string d = $"Conversation {id}";

                CheckImage($"{d} .Portrait", dialogue.Portrait);
                CheckImage($"{d} .Character", dialogue.Character);

                // A line that a choice or an item leads to must exist.
                void CheckLine(string where, string? lineId)
                {
                    if (!string.IsNullOrWhiteSpace(lineId) && !dialogue.Nodes.ContainsKey(lineId))
                        Problem(where, $"leads to line '{lineId}', which doesn't exist");
                }

                if (dialogue.ChooseStartNode == null)
                    CheckLine($"{d} start", dialogue.StartNode);

                foreach (var use in dialogue.ItemUses)
                {
                    CheckItem($"{d} .OnItem", use.Key);

                    foreach (DialogueItemUse reaction in use.Value)
                        CheckLine($"{d} .OnItem(\"{use.Key}\")", reaction.GoesTo);
                }

                foreach (var (lineId, node) in dialogue.Nodes)
                {
                    CheckImage($"{d}, line '{lineId}' portrait:", node.Portrait);
                    CheckImage($"{d}, line '{lineId}' character:", node.Character);

                    foreach (DialogueChoice choice in node.Choices)
                    {
                        CheckFight($"{d}, line '{lineId}' startsFight:", choice.StartsFight);
                        CheckLine($"{d}, line '{lineId}', choice \"{choice.Text}\"", choice.Next);
                    }
                }
            }

            // ---- Fights, items, the trunk ----------------------------------
            foreach (Enemy enemy in Enemies.All)
            {
                string e = $"Fight {enemy.Id}";
                CheckImage($"{e} .Portrait", enemy.Portrait);
                CheckImage($"{e} .BeatenPortrait", enemy.BeatenPortrait);
                CheckImage($"{e} .Character", enemy.Character);
                CheckImage($"{e} .BeatenCharacter", enemy.BeatenCharacter);
            }

            foreach (Item item in Items.All)
            {
                CheckImage($"Item '{item.Name}' icon", item.Icon);
                CheckImage($"Item '{item.Name}' picture", item.Picture);
            }

            foreach (TrunkItem trunkItem in Trunk.Items)
                CheckItem("Data/Trunk.cs", trunkItem.ItemName);

            // ---- Journal: ids are what saves remember, so they must be unique.
            foreach (var group in Journal.All.GroupBy(entry => entry.Id, StringComparer.OrdinalIgnoreCase))
            {
                if (group.Count() > 1)
                    Problem("Data/Journal.cs", $"the id '{group.Key}' is used by {group.Count()} entries");
            }

            return problems;
        }

        // Shows a list of problems, if there are any.
        public static void ShowProblems()
        {
            List<string> problems = FindProblems();

            if (problems.Count == 0)
            {
                return;
            }

            const int MaxShown = 25;
            string list = string.Join("\n", problems.Take(MaxShown).Select(p => "- " + p));

            if (problems.Count > MaxShown)
                list += $"\n...and {problems.Count - MaxShown} more.";

            MessageBox.Show(
                $"The game data has {problems.Count} problem(s):\n\n{list}\n\n" +
                "(This check only runs in Debug builds.)",
                "Game data check",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private static bool ResourceExists(string path)
        {
            try
            {
                var resource = Application.GetResourceStream(new Uri($"pack://application:,,,/{path}", UriKind.Absolute));
                resource?.Stream.Dispose();
                return resource != null;
            }
            catch (IOException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
