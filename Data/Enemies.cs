using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * Everyone the player can fight. See SCENE_GUIDE.md ("Fights").
     *
     * Each round both sides roll 2d6:
     *   player = 2d6 + Strength / 2 + weapon bonus (no weapon = -1)
     *   enemy  = 2d6 + Skill
     * Higher hits for 10 (a double six hits for 20; a double one always
     * misses). A tie is a miss for both.
     *
     * Skill guide: 3 feeble, 5 fair, 7 tough, 9 deadly.
     * Health guide: each hit is 10, so Health 30 = about 3 hits.
     *
     * Start a fight with  .Choice("...", startsFight: "Id")  in a dialogue,
     * or  .ActStartsFight("Id")  on a hotspot.
     */
    public static class Enemies
    {
        // Must stay at the top: enemies below add themselves to it.
        private static readonly Dictionary<string, Enemy> EnemyLookup =
            new(StringComparer.OrdinalIgnoreCase);

        private static EnemyBuilder NewEnemy(string id, string name, int skill, int health)
            => new EnemyBuilder(id, name, skill, health);

        // =========================================================
        // KING HAUTAIN, furious (fountain close-up, after grabbing the coins)
        // =========================================================

        public static Enemy AngryHautain { get; } =
            NewEnemy("AngryHautain", "King Hautain", skill: 4, health: 30)
                .Portrait("Assets/Characters/Portraits/Hautain_portrait_framed.png")
                .BeatenPortrait("Assets/Characters/Portraits/Hautain_Beaten_portrait_framed.png")
                .Character("Assets/Characters/Hautain_Attack.png")
                .BeatenCharacter("Assets/Characters/Hautain_Beaten.png")
                .StandAt(495, 205, 285)
                .Intro("King Hautain leaps from his lily pad, sceptre raised! \"For the CROWN!\"")
                .OnWin("King Hautain flops into the water with a mighty splash and paddles away into the reeds, croaking oaths of revenge.",
                       p =>
                       {
                           p.SetFlag("DefeatedHautain");
                           p.StatusEffect = "None";
                       });

        // =========================================================
        // QUAZIMODO
        // Front door, if the player attempts to force entry
        // =========================================================

        public static Enemy QuazimodoDoorFight { get; } =
            NewEnemy(
                "QuazimodoDoorFight",
                "Quazimodo",
                skill: 6,
                health: 40)

                .Portrait(
                    "Assets/Characters/Portraits/Quazimodo_portrait_framed.png")

                .BeatenPortrait(
                    "Assets/Characters/Portraits/Quazimodo_Beaten_portrait_framed.png")

                // On the threshold, knife raised (same canvas as his dialogue pictures).
                .Character("Assets/Characters/Quazimodo_Attack.png")
                .BeatenCharacter("Assets/Characters/Quazimodo_Beaten.png")
                .StandAt(619, 282, 458)

                .Intro(
                    "Quazimodo slowly steps through the doorway and closes "
                    + "the heavy door behind him. "
                    + "He rolls one crooked shoulder. "
                    + "Several joints click in quick succession.\n\n"
                    + "\"Very well.\"\n\n"
                    + "He removes his gloves one finger at a time.\n\n"
                    + "\"Please avoid bleeding on the threshold. "
                    + "It encourages the wood.\"")

              .OnWin(
    "Quazimodo staggers backward against the front door. "
    + "For several seconds he remains there, breathing heavily. "
    + "Then he straightens his bow tie.\n\n"
    + "\"Very good. You have established that you can enter "
    + "the Mansion.\"\n\n"
    + "He pulls the enormous door open and steps aside.\n\n"
    + "\"Whether you should is now somebody else's problem.\"",
    p =>
    {
        p.SetFlag("DefeatedQuazimodo");
        p.SetFlag("FrontDoorOpen");
        p.SetFlag("ForcedEntry");
    });

        // =========================================================
        // LOOKUP (used by the game - no need to edit)
        // =========================================================

        internal static void Register(Enemy enemy) => EnemyLookup[enemy.Id] = enemy;

        public static IEnumerable<Enemy> All => EnemyLookup.Values;

        public static Enemy? GetEnemy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            EnemyLookup.TryGetValue(id, out Enemy? enemy);
            return enemy;
        }
    }
}
