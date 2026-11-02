using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * All conversations in the game. See SCENE_GUIDE.md for how to write them.
     *
     * A hotspot starts one with .TalkStarts("Id") or .ActStarts("Id").
     * Each Line is something the character says; the Choices under it are
     * the player's replies. A Choice with no goesTo ends the conversation.
     *
     * Conversations register themselves - just add a new one below.
     */
    public static class Dialogues
    {
        // Must stay at the top: dialogues below add themselves to it.
        private static readonly Dictionary<string, Dialogue> DialogueLookup =
            new(StringComparer.OrdinalIgnoreCase);

        private static DialogueBuilder NewDialogue(string id, string speaker)
            => new DialogueBuilder(id, speaker);

        // =========================================================
        // THE SAD GARGOYLE (FrontGargoyle scene, TALK)
        // =========================================================

        public static Dialogue SadGargoyle { get; } =
            NewDialogue("SadGargoyle", "Sad Gargoyle")
                // No portrait: the scene is already a close-up of it.

            .Line("start", "*Sigh.* Three hundred years on this pillar, and not one visitor has ever said hello.")
                .Choice("Hello! Why so glum?", "sad")
                .Choice("Do you know how to get into the mansion?", "door")
                .Choice("You're not entirely stone, are you?", "secret",
                        onlyIf: p => p.Wits >= 12 && !p.Has("Gargoyle's Blessing"),
                        effect: p =>
                        {
                            p.Luck += 1;
                            p.Give("Gargoyle's Blessing");
                        })
                .Choice("Goodbye.")

            .Line("menu", "Was there something else, visitor? I have nowhere to be. Ever.")
                .SameChoicesAs("start")

            .Line("sad", "Pigeons. Rain. More pigeons. And the gargoyle on the other pillar hasn't spoken to me since the Incident.")
                .Choice("What incident?", "incident")
                .Choice("That sounds dreadful.", "menu")

            .Line("incident", "We do not speak of the Incident. There was a bird. That is all I will say.")
                .Choice("Fair enough.", "menu")

            .Line("door", "Locked, of course. They say the old door knocker only answers to someone with manners.")
                .Choice("Thanks for the tip.", "menu")

            .Line("secret", "Shh! Not so loud! ...Here, take my blessing. You will need all the luck you can get in there. (Luck +1)")
                .Choice("Your secret is safe with me.", "menu");


        // =========================================================
        // KING HAUTAIN (FountainCloseup scene, ACT on the gold coins)
        // =========================================================

        public static Dialogue HautainCoins { get; } =
            NewDialogue("HautainCoins", "King Hautain")
                .Portrait("Assets/Characters/Hautain.png")
                .StandAt(495, 205, 285)        // on the big lily pad
                .StartAt(p => p.HasFlag("StoleCoins") || p.HasFlag("GivenRoyalCoin")
                                ? "again"
                                : "start")

                // Offering items (ITEM button during the conversation).
                .OnItem("Gold Coins", "returned",
                        removeItem: true,
                        effect: p =>
                        {
                            p.ClearFlag("StoleCoins");
                            p.StatusEffect = "None";
                        })
                .OnItem("Royal Gold Coin", "keepit")
                .OnOtherItems("\"What is THIS? The Crown does not accept bribes.\" He eyes the {item}. \"...Unless they are made of gold.\"")

            .Line("start", "...HALT, THIEF! Those coins belong to the Crown! I am Hautain, rightful king of this fountain, and you are trespassing!")
                .Choice("King? You're a frog on a lily pad.", "insult")
                .Choice("Forgive me, Your Majesty. I'll leave your gold alone.", "apology")
                .Choice("Surely the Crown can spare a coin for a loyal subject?", "flattered",
                        onlyIf: p => p.Wits >= 12,
                        effect: p =>
                        {
                            p.Give("Royal Gold Coin");
                            p.SetFlag("GivenRoyalCoin");
                        })
                .Choice("Finders keepers! (Grab the coins)", "grab",
                        effect: p =>
                        {
                            p.Give("Gold Coins");
                            p.SetFlag("StoleCoins");
                            p.StatusEffect = "Royal Grudge";
                        })

            .Line("menu", "Well? What have you to say for yourself, thief?")
                .SameChoicesAs("start")

            .Line("insult", "A FROG?! My ancestors have ruled this pond for three hundred years! Guards! GUARDS! ...Oh. I have no guards.",
                  portrait: "Assets/Characters/Hautain_Attack.png")
                .Choice("Awkward.", "menu")

            .Line("apology", "Hmph. At least SOMEONE in this wretched mansion has manners. You may withdraw from the royal presence.")
                .Choice("(Bow and back away)")

            .Line("flattered", "A loyal... subject? Nobody has called me that in years. Very well - ONE coin. Spend it wisely, and speak well of your king.")
                .Choice("Long live the King!")

            .Line("grab", "You DARE?! Mark my words, thief - the Crown never forgets! NEVER!",
                  portrait: "Assets/Characters/Hautain_Attack.png")
                .Choice("(Run!)")

            .Line("again", "YOU again! Haven't you robbed the Crown enough for one day?")
                .Choice("Just admiring the view, Your Majesty.")

            .Line("returned", "My coins! Every last one! ...Hmph. Perhaps you are not ENTIRELY a thief. The Crown is... appeased. (Royal Grudge lifted)")
                .Choice("Long live the King!")

            .Line("keepit", "Return a ROYAL GIFT? Do you wish to insult me? Keep it, you oaf, and spend it in my honour.")
                .Choice("As you wish, Your Majesty.");

        public static Dialogue QuazimodoFrontDoor { get; } =
    NewDialogue("QuazimodoFrontDoor", "Quazimodo")
        .Portrait("Assets/Characters/Quazimodo.png")                     
        .StandAt(589, 300, 700)
        .StartAt(p => p.HasFlag("MetQuazimodo")
            ? "again"
            : "start")

        .Line(
            "start",
            "The door opens a fraction. A pale, crooked figure peers through the gap.")
            .Choice(
                "Hello. May I come inside?",
                "polite",
                effect: p => p.SetFlag("MetQuazimodo"))
            .Choice(
                "Are you the butler?",
                "butler",
                effect: p => p.SetFlag("MetQuazimodo"))
            .Choice(
                "What happened to you?",
                "rude",
                effect: p => p.SetFlag("MetQuazimodo"))
            .Choice(
                "Sorry. Wrong mansion.",
                "leave",
                effect: p => p.SetFlag("MetQuazimodo"))

        .Line(
            "polite",
            "Quazimodo blinks. \"You said please. That places you above most of our visitors.\"")
            .Choice(
                "Is that a yes?",
                "admit")

        .Line(
            "butler",
            "\"Butler, porter, doorman, occasional exterminator. Titles multiply when staff do not.\"")
            .Choice(
                "Then may I come inside?",
                "admit")
            .Choice(
                "Exterminator?",
                "exterminator")

        .Line(
            "exterminator",
            "\"A regrettable duty. Some things enter the Mansion without being invited.\"")
            .Choice(
                "Was I invited?",
                "admit")
            .Choice(
                "I think I'll remain outside.",
                "leave")

        .Line(
            "rude",
            "Quazimodo studies you in silence. \"One might ask precisely the same of you.\"")
            .Choice(
                "Fair point. May I come inside?",
                "admit")
            .Choice(
                "I deserved that.",
                "admit")
            .Choice(
                "Never mind.",
                "leave")

        .Line(
            "admit",
            "\"The door was not locked. It was considering you.\" Quazimodo pulls it open.")
            .Choice(
                "That's not reassuring.",
                effect: p => p.SetFlag("FrontDoorOpen"))
            .Choice(
                "Thank you.",
                effect: p => p.SetFlag("FrontDoorOpen"))
            .Choice(
                "Doors don't think.",
                effect: p => p.SetFlag("FrontDoorOpen"))

        .Line(
            "leave",
            "\"A sensible decision. Those rarely survive prolonged contact with this house.\"")
            .Choice("Goodbye.")

        .Line(
            "again",
            "Quazimodo opens the door a fraction. \"Have you finished standing outside?\"")
            .Choice(
                "Yes. Let me in.",
                "admit")
            .Choice(
                "Not quite.",
                "leave");

        // =========================================================
        // LOOKUP (used by the game - no need to edit)
        // =========================================================

        internal static void Register(string id, Dialogue dialogue) => DialogueLookup[id] = dialogue;

        // Number of conversations (also makes sure they are all loaded).
        public static int Count => DialogueLookup.Count;

        public static Dialogue? GetDialogue(string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(dialogueId))
            {
                return null;
            }

            DialogueLookup.TryGetValue(dialogueId, out Dialogue? dialogue);
            return dialogue;
        }
    }
}
