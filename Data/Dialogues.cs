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

         // Offering items (ITEM button during the conversation).
         .OnItem("Party Hat", "Hat")

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
         .Choice("Your secret is safe with me.", "menu")

     .Line("Hat", "Great! Awesome, a dirty old party hat! Now please take it off me, before the rough paper scratches my scalp!")
         .Choice("(Take the hat back)", "menu");

        // =========================================================
        // KING HAUTAIN
        // FountainCloseup scene, ACT on the gold coins
        // =========================================================

        public static Dialogue HautainCoins { get; } =
            NewDialogue("HautainCoins", "King Hautain")
                .Portrait("Assets/Characters/Hautain.png")
                .StandAt(495, 205, 285) // On the big lily pad.

                .StartAt(p =>
                    p.HasFlag("DefeatedHautain")
                        ? "beaten"
                        : p.HasFlag("StoleCoins")
                            ? "thiefReturns"
                            : p.HasFlag("GivenRoyalCoin")
                                ? "subjectReturns"
                                : "start")

                // =====================================================
                // ITEMS OFFERED DURING CONVERSATION
                // =====================================================

                .OnItem(
                    "Gold Coins",
                    "returned",
                    removeItem: true,
                    effect: p =>
                    {
                        p.ClearFlag("StoleCoins");
                        p.StatusEffect = "None";
                    })

                .OnItem(
                    "Royal Gold Coin",
                    "keepit")

                .OnOtherItems(
                    "\"What is THIS?\" Hautain peers suspiciously at the {item}. "
                    + "\"The Crown does not accept bribes.\" "
                    + "He pauses. "
                    + "\"...Is it valuable?\"")

                // =====================================================
                // FIRST MEETING
                // =====================================================

                .Line(
                    "start",
                    "\"HALT!\" "
                    + "A tiny crowned figure rises between the lily pads and points a sceptre at your hand. "
                    + "\"Remove your grasping extremity from the Royal Treasury! "
                    + "You stand before Hautain the First, Keeper of the Deep Basin, "
                    + "Lord of the Seven Lily Pads, Defender of the Western Drain, "
                    + "and Sovereign of all waters contained within this fountain!\"",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "King? You're a frog on a lily pad.",
                        "frogInsult")

                    .Choice(
                        "Forgive me, Your Majesty. I didn't realise the fountain was occupied.",
                        "apology")

                    .Choice(
                        "The Royal Treasury?",
                        "treasury")

                    .Choice(
                        "Surely the Crown can spare a coin for a loyal subject.",
                        "loyalSubject",
                        onlyIf: p => p.Wits >= 12)

                    .Choice(
                        "Finders keepers. (Grab the coins)",
                        "grab",
                        effect: p =>
                        {
                            p.Give("Gold Coins");
                            p.SetFlag("StoleCoins");
                            p.StatusEffect = "Royal Grudge";
                        })

                // =====================================================
                // MAIN MENU
                // =====================================================

                .Line(
                    "menu",
                    "\"Well?\" Hautain draws himself up to his full and admittedly limited height. "
                    + "\"You remain within the Royal Presence. "
                    + "Try to make better use of it.\"")

                    .Choice(
                        "Tell me about your kingdom.",
                        "kingdom")

                    .Choice(
                        "Why are the coins yours?",
                        "treasury")

                    .Choice(
                        "What do you know about the Mansion?",
                        "mansion")

                    .Choice(
                        "Any advice about the front door?",
                        "frontDoor")

                    .Choice(
                        "You're still just a frog.",
                        "frogInsult")

                    .Choice(
                        "I'll be going.",
                        "leave")

                // =====================================================
                // THE ROYAL TREASURY
                // =====================================================

                .Line(
                    "treasury",
                    "\"Tribute.\" "
                    + "Hautain gestures proudly toward the coins beneath the water. "
                    + "\"Deposited by travellers, pilgrims, petitioners, "
                    + "and one extremely careless chimney sweep.\"")

                    .Choice(
                        "People throw coins into fountains to make wishes.",
                        "wishes")

                    .Choice(
                        "So you found them first.",
                        "foundThem")

                    .Choice(
                        "Then the coins belong to the Crown.",
                        "recognise")

                .Line(
                    "wishes",
                    "Hautain pauses. "
                    + "\"Yes. Their petitions were received.\" "
                    + "He glances toward the water. "
                    + "\"The Crown regrets that fulfilment remains subject to staffing limitations.\"")

                    .Choice(
                        "Staffing limitations?",
                        "staffing")

                    .Choice(
                        "At least somebody heard the wishes.",
                        "heardWishes")

                    .Choice(
                        "Convenient.",
                        "menu")

                .Line(
                    "staffing",
                    "\"The King presently occupies every major office of state.\"")

                    .Choice(
                        "Every office?",
                        "offices")

                .Line(
                    "offices",
                    "\"Monarch. Treasurer. Magistrate. Ambassador. "
                    + "Minister of Drainage. Commander of the Royal Guard.\" "
                    + "Hautain clears his throat. "
                    + "\"And, owing to recent budgetary constraints, Royal Guard.\"")

                    .Choice(
                        "So you don't have any guards.",
                        "noGuards")

                    .Choice(
                        "Very efficient.",
                        "efficient")

                .Line(
                    "noGuards",
                    "\"The Crown has never required guards against anyone possessing manners.\" "
                    + "He narrows his eyes. "
                    + "\"Do not force a constitutional review.\"")

                    .Choice(
                        "I'll behave.",
                        "apology")

                    .Choice(
                        "I'd like to see you try.",
                        "challenge")

                .Line(
                    "efficient",
                    "\"Precisely!\" "
                    + "Hautain sits straighter. "
                    + "\"At last, someone with an elementary understanding of government.\"")

                    .Choice(
                        "Long live the King.",
                        "recognise")

                    .Choice(
                        "Does the government provide refreshments?",
                        "refreshments")

                .Line(
                    "refreshments",
                    "\"The banquet department is undergoing restructuring.\" "
                    + "A fly passes overhead. "
                    + "Hautain watches it until it disappears. "
                    + "\"Extensive restructuring.\"")

                    .Choice(
                        "I see.",
                        "menu")

                .Line(
                    "heardWishes",
                    "For a moment Hautain's royal performance softens. "
                    + "\"A king who does not hear his subjects is merely a hat occupying a chair.\" "
                    + "He taps his crown. "
                    + "\"This is a very good hat.\"")

                    .Choice(
                        "It suits you.",
                        "recognise")

                    .Choice(
                        "I'll leave you to your duties.",
                        "leave")

                .Line(
                    "foundThem",
                    "\"FOUND them?\" "
                    + "Hautain recoils. "
                    + "\"You unlicensed turnip, possession is a vulgar concept! "
                    + "The currency is held in trust by the sovereign authority of the fountain.\"")

                    .Choice(
                        "Which is you.",
                        "authority")

                    .Choice(
                        "That sounds exactly like finding them.",
                        "frogInsult")

                .Line(
                    "authority",
                    "\"By unanimous approval of the Crown, yes.\"")

                    .Choice(
                        "How many members voted?",
                        "vote")

                .Line(
                    "vote",
                    "\"One.\"")

                    .Choice(
                        "A decisive majority.",
                        "efficient")

                // =====================================================
                // HAUTAIN'S KINGDOM
                // =====================================================

                .Line(
                    "kingdom",
                    "\"The Kingdom of Hautain encompasses the Upper Basin, "
                    + "Lower Basin, Seven Lily Pads, Western Drain, "
                    + "and all territories submerged during heavy rain.\"")

                    .Choice(
                        "Impressive.",
                        "kingdomPraise")

                    .Choice(
                        "That's a fountain.",
                        "kingdomInsult")

                    .Choice(
                        "What happened to your subjects?",
                        "subjects")

                .Line(
                    "kingdomPraise",
                    "\"Finally!\" "
                    + "Hautain points his sceptre toward you. "
                    + "\"The Crown recognises this rare outbreak of judgement.\"")

                    .Choice(
                        "You're welcome.",
                        "menu")

                .Line(
                    "kingdomInsult",
                    "\"And you are an impressive quantity of leg supporting remarkably little civilisation.\"")

                    .Choice(
                        "Fair enough.",
                        "menu")

                    .Choice(
                        "Still a fountain.",
                        "frogInsult")

                .Line(
                    "subjects",
                    "Hautain looks across the quiet water. "
                    + "\"Elsewhere.\" "
                    + "A pause. "
                    + "\"The affairs of private citizens are not discussed by the Crown.\"")

                    .Choice(
                        "Are you alone here?",
                        "alone")

                    .Choice(
                        "Of course.",
                        "menu")

                .Line(
                    "alone",
                    "\"A sovereign is never alone.\" "
                    + "Hautain adjusts his crown. "
                    + "\"A sovereign is... temporarily without court.\"")

                    .Choice(
                        "That sounds rather lonely.",
                        "lonely")

                    .Choice(
                        "I see.",
                        "menu")

                .Line(
                    "lonely",
                    "\"Kingship is not intended to be comfortable.\" "
                    + "He taps the sceptre against the lily pad. "
                    + "\"It is intended to be witnessed.\"")

                    .Choice(
                        "Then consider yourself witnessed, Your Majesty.",
                        "recognise")

                    .Choice(
                        "I'll leave you to your kingdom.",
                        "leave")

                // =====================================================
                // RESPECT / APOLOGY
                // =====================================================

                .Line(
                    "apology",
                    "Hautain lowers his sceptre by approximately half an inch. "
                    + "\"An apology offered before punishment. "
                    + "An uncommon luxury.\" "
                    + "He studies you. "
                    + "\"You may retain both hands, provided they remain outside the Treasury.\"")

                    .Choice(
                        "Very generous.",
                        "mercy")

                    .Choice(
                        "Thank you, Your Majesty.",
                        "recognise")

                    .Choice(
                        "I'll leave the coins alone.",
                        "leave")

                .Line(
                    "mercy",
                    "\"Mercy is among the Crown's most celebrated qualities.\" "
                    + "He pauses. "
                    + "\"It would be more celebrated if anyone remained to record it.\"")

                    .Choice(
                        "I'll remember.",
                        "recognise")

                .Line(
                    "recognise",
                    "Hautain's posture becomes noticeably grander. "
                    + "\"Good. The sovereignty of the fountain is formally recognised.\" "
                    + "He gestures to the stone edge. "
                    + "\"You may remain within the Royal Presence.\"")

                    .Choice(
                        "Long live King Hautain.",
                        "royalApproval")

                    .Choice(
                        "Tell me about your kingdom.",
                        "kingdom")

                    .Choice(
                        "What do you know about the Mansion?",
                        "mansion")

                    .Choice(
                        "I'll take my leave.",
                        "leave")

                .Line(
                    "royalApproval",
                    "\"Acceptable.\" "
                    + "Hautain nods. "
                    + "\"The Crown's opinion of you has improved from "
                    + "'probable thief' to 'adequately civilised visitor.'\"")

                    .Choice(
                        "I'll take it.",
                        "menu")

                // =====================================================
                // WITS / ROYAL COIN
                // =====================================================

                .Line(
                    "loyalSubject",
                    "Hautain freezes. "
                    + "\"A loyal... subject?\" "
                    + "He looks over one shoulder, then the other. "
                    + "\"You refer to yourself?\"")

                    .Choice(
                        "I recognise the sovereignty of King Hautain.",
                        "royalCoin",
                        effect: p =>
                        {
                            p.Give("Royal Gold Coin");
                            p.SetFlag("GivenRoyalCoin");
                        })

                    .Choice(
                        "It seemed like the easiest way to get a coin.",
                        "badFlattery")

                .Line(
                    "badFlattery",
                    "\"Politeness without conviction is merely treason wearing perfume!\"")

                    .Choice(
                        "Fine. I sincerely recognise your sovereignty.",
                        "royalCoin",
                        effect: p =>
                        {
                            p.Give("Royal Gold Coin");
                            p.SetFlag("GivenRoyalCoin");
                        })

                    .Choice(
                        "Never mind.",
                        "leave")

                .Line(
                    "royalCoin",
                    "Hautain reaches beneath the water and retrieves a single gold coin. "
                    + "He presents it atop both webbed hands. "
                    + "\"Then let it be recorded that the Crown acknowledges "
                    + "its first new subject in a great many seasons.\" "
                    + "He clears his throat. "
                    + "\"This is not payment. It is a Royal Gold Coin, "
                    + "bestowed as a mark of sovereign confidence.\"")

                    .Choice(
                        "I'll treasure it.",
                        "acceptCoin")

                    .Choice(
                        "I'll spend it in your honour.",
                        "acceptCoin")

                    .Choice(
                        "Does this make me nobility?",
                        "nobility")

                .Line(
                    "nobility",
                    "\"No.\"")

                    .Choice(
                        "Worth asking.",
                        "acceptCoin")

                .Line(
                    "acceptCoin",
                    "\"Speak well of your king when circumstances permit.\" "
                    + "Hautain lowers his voice. "
                    + "\"And occasionally when they do not.\"")

                    .Choice(
                        "Long live King Hautain.")

                // =====================================================
                // FROG INSULT ROUTE
                // =====================================================

                .Line(
                    "frogInsult",
                    "\"FROG?!\" "
                    + "Hautain's crown nearly slips as he rises. "
                    + "\"Frog is a BIOLOGICAL DESCRIPTION, not a form of address, "
                    + "you uncultivated drainpipe!\"",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "Fine. Your Majesty the Frog.",
                        "frogInsultAgain")

                    .Choice(
                        "My mistake, Your Majesty.",
                        "apology")

                    .Choice(
                        "You're very sensitive about this.",
                        "sensitive")

                .Line(
                    "frogInsultAgain",
                    "\"Yes, I am a frog! "
                    + "You are apparently a primate. "
                    + "Only one of us has managed to make something of it!\"",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "You're six inches tall.",
                        "sizeInsult")

                    .Choice(
                        "All right, that was good.",
                        "menu")

                    .Choice(
                        "I apologise.",
                        "apology")

                .Line(
                    "sensitive",
                    "\"I am not sensitive! "
                    + "The Crown merely maintains standards of diplomatic address!\"")

                    .Choice(
                        "Of course, Your Majesty.",
                        "menu")

                .Line(
                    "sizeInsult",
                    "\"The Crown is not small! "
                    + "The rest of the world is unnecessarily large!\"",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "That's one way of looking at it.",
                        "menu")

                    .Choice(
                        "Still six inches.",
                        "sizeInsultAgain")

                .Line(
                    "sizeInsultAgain",
                    "\"I require six inches to govern a kingdom! "
                    + "You require nearly six feet to steal from a fountain!\"",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "Fair point.",
                        "menu")

                    .Choice(
                        "Let's see what six inches can do.",
                        "challenge")

                // =====================================================
                // CHALLENGE / FIGHT
                // =====================================================

                .Line(
                    "challenge",
                    "\"You threaten a sovereign on sovereign territory? "
                    + "Even GEESE understand jurisdiction!\" "
                    + "Hautain lowers himself into a fighting stance.",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "I don't want to fight you.",
                        "apology")

                    .Choice(
                        "Do your worst.",
                        "duck")

                .Line(
                    "duck",
                    "\"Do my worst?\" "
                    + "Hautain levels his sceptre. "
                    + "\"I once negotiated with a duck!\"",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "A duck?",
                        "duckDiplomacy")

                .Line(
                    "duckDiplomacy",
                    "\"You cannot comprehend diplomacy at that level.\"",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "All right. Let's fight.",
                        "fight",
                        startsFight: "AngryHautain")

                    .Choice(
                        "Actually, I think I'll apologise.",
                        "apology")

                .Line(
                    "fight",
                    "\"THEN APPROACH, YOU OVERGROWN TROUSER RACK! "
                    + "You face the full military strength of the fountain!\"",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                // =====================================================
                // STEALING THE COINS
                // =====================================================

                .Line(
                    "grab",
                    "You plunge your hand into the fountain and scoop up the gold. "
                    + "Hautain stares at you in absolute disbelief. "
                    + "Then the tiny sceptre rises.",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "They're only coins.",
                        "onlyCoins")

                    .Choice(
                        "I need them more than you do.",
                        "needCoins")

                    .Choice(
                        "(Run!)",
                        "grudge")

                    .Choice(
                        "(Stand your ground and fight!)",
                        "fight",
                        startsFight: "AngryHautain")

                .Line(
                    "onlyCoins",
                    "\"ONLY COINS?!\" "
                    + "Hautain points furiously toward your pocket. "
                    + "\"You steal from a fountain? "
                    + "Even MAGPIES possess the decency to have feathers!\"",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "I didn't know they mattered.",
                        "coinMeaning")

                    .Choice(
                        "They're mine now.",
                        "grudge")

                .Line(
                    "coinMeaning",
                    "\"They are petitions! Wishes! Promises! "
                    + "Proof that travellers once stopped here and believed "
                    + "something might answer them!\" "
                    + "His voice drops. "
                    + "\"You have stolen from more than a king.\"")

                    .Choice(
                        "I'll return them.",
                        "returnInstruction")

                    .Choice(
                        "I still need them.",
                        "grudge")

                .Line(
                    "needCoins",
                    "\"Need does not become justice merely because it is spoken aloud, "
                    + "you ambulatory purse with delusions of morality!\"")

                    .Choice(
                        "I'll return them.",
                        "returnInstruction")

                    .Choice(
                        "No.",
                        "grudge")

                .Line(
                    "returnInstruction",
                    "\"Then restore the Royal Treasury through the proper diplomatic channel!\" "
                    + "Hautain points at your belongings. "
                    + "\"The ITEM button, you long-legged architectural mistake! "
                    + "The Crown will wait.\"")

                    .Choice(
                        "Understood.")

                .Line(
                    "grudge",
                    "\"GO, THEN! "
                    + "May every chair you sit upon be VERY SLIGHTLY DAMP! "
                    + "The Crown remembers every debt, every insult, "
                    + "and every muddy footprint!\" "
                    + "(Royal Grudge gained)",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "(Retreat from the fountain)")

                // =====================================================
                // RETURN WHILE HOLDING STOLEN COINS
                // =====================================================

                .Line(
                    "thiefReturns",
                    "Hautain rises slowly from behind a lily pad. "
                    + "\"The pocket-fingered gutter heron returns.\" "
                    + "He raises his sceptre. "
                    + "\"Have you brought the Royal Treasury?\"",
                    portrait: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "I came to return it.",
                        "returnInstruction")

                    .Choice(
                        "I came to apologise.",
                        "apologyNoCoins")

                    .Choice(
                        "I'm keeping the coins.",
                        "stillThief")

                    .Choice(
                        "I came to finish our fight.",
                        "fight",
                        startsFight: "AngryHautain")

                .Line(
                    "apologyNoCoins",
                    "\"The Crown acknowledges your apology.\" "
                    + "Hautain glances toward your pockets. "
                    + "\"The Crown also observes that apologies are considerably lighter than gold.\"")

                    .Choice(
                        "I'll return them.",
                        "returnInstruction")

                    .Choice(
                        "That's all you're getting.",
                        "stillThief")

                .Line(
                    "stillThief",
                    "\"Keep them, then! "
                    + "Perhaps holding something of value will constitute a new experience!\"")

                    .Choice(
                        "Goodbye, Your Majesty.")

                // =====================================================
                // RETURN THE STOLEN COINS
                // =====================================================

                .Line(
                    "returned",
                    "Hautain watches each coin sink back beneath the dark water. "
                    + "He counts them once. "
                    + "Then again. "
                    + "The sceptre finally lowers. "
                    + "\"The Royal Treasury is restored.\" "
                    + "A long silence follows. "
                    + "\"Returning what was stolen requires more character than taking it.\" "
                    + "(Royal Grudge lifted)")

                    .Choice(
                        "Can you forgive me?",
                        "forgive")

                    .Choice(
                        "We're even, then?",
                        "even")

                    .Choice(
                        "Long live King Hautain.",
                        "restored")

                .Line(
                    "forgive",
                    "\"Forgiveness is not an eraser.\" "
                    + "Hautain considers you. "
                    + "\"But it may serve as a bridge.\" "
                    + "He extends the tiny sceptre. "
                    + "\"The Crown is prepared to cross halfway.\"")

                    .Choice(
                        "Then I'll meet you there.")

                .Line(
                    "even",
                    "\"The Treasury is even.\" "
                    + "Hautain taps the lily pad with his sceptre. "
                    + "\"Trust is a separate account.\"")

                    .Choice(
                        "Fair enough.")

                .Line(
                    "restored",
                    "Hautain inclines his crowned head. "
                    + "\"You are restored to the good opinion of the fountain.\"")

                    .Choice(
                        "(Bow and withdraw)")

                // =====================================================
                // RETURN OF A RECOGNISED SUBJECT
                // =====================================================

                .Line(
                    "subjectReturns",
                    "Hautain rises to his full height upon the lily pad. "
                    + "\"The recognised subject returns!\" "
                    + "He looks toward the empty path behind you. "
                    + "\"Your ceremonial escort is regrettably delayed.\"")

                    .Choice(
                        "Long live King Hautain.",
                        "subjectGreeting")

                    .Choice(
                        "Any royal duties for me?",
                        "duties")

                    .Choice(
                        "What do you know about the Mansion?",
                        "mansion")

                    .Choice(
                        "Any advice about the front door?",
                        "frontDoor")

                    .Choice(
                        "I'll be going.",
                        "leave")

                .Line(
                    "subjectGreeting",
                    "\"Acceptable.\" "
                    + "Hautain settles back onto his lily pad. "
                    + "\"The realm remains damp but sovereign.\"")

                    .Choice(
                        "Glad to hear it.")

                .Line(
                    "duties",
                    "\"For the present: do not steal from the Crown, "
                    + "discourage herons, and speak favourably of the drainage.\"")

                    .Choice(
                        "I can manage two of those.",
                        "twoDuties")

                .Line(
                    "twoDuties",
                    "Hautain studies you. "
                    + "\"The Crown elects not to inquire.\"")

                    .Choice(
                        "(Bow and leave)")

                // =====================================================
                // THE MANSION
                // =====================================================

                .Line(
                    "mansion",
                    "\"The Mansion is a neighbouring power with unstable borders, "
                    + "unreliable servants, suspicious architecture, "
                    + "and APPALLING drainage.\"")

                    .Choice(
                        "Is it dangerous?",
                        "dangerousMansion")

                    .Choice(
                        "Do you know Quazimod?",
                        "quazimod")

                    .Choice(
                        "Anyone else I should know about?",
                        "residents")

                .Line(
                    "dangerousMansion",
                    "\"Naturally.\" "
                    + "Hautain looks toward the house. "
                    + "\"Enter if you must. "
                    + "But do not allow the place to convince you that it owns you.\"")

                    .Choice(
                        "What does that mean?",
                        "ominous")

                .Line(
                    "ominous",
                    "\"It means precisely what was said.\" "
                    + "Hautain settles back onto his lily pad. "
                    + "\"The Crown does not repeat ominous warnings. "
                    + "It diminishes them.\"")

                    .Choice(
                        "Understood.")

                .Line(
                    "quazimod",
                    "\"The crooked coat-rack from the house?\" "
                    + "Hautain considers the question. "
                    + "\"Excellent manners. Disturbing posture.\"")

                    .Choice(
                        "That sounds like him.",
                        "menu")

                .Line(
                    "residents",
                    "\"There is the enormous kitchen despot, "
                    + "the thin one who looks personally offended by soup, "
                    + "the airborne tavern incident, "
                    + "and the eight-legged Department of Unnecessary Infrastructure.\"")

                    .Choice(
                        "Magnus, Severin, Ebria and Velvet?",
                        "correctResidents")

                .Line(
                    "correctResidents",
                    "\"The Crown sees you have already begun the diplomatic briefings.\"")

                    .Choice(
                        "What about Pip?",
                        "pip")

                    .Choice(
                        "And Sola?",
                        "sola")

                    .Choice(
                        "What about Gréll?",
                        "grell")

                .Line(
                    "pip",
                    "\"The deceased librarian? "
                    + "Proper manners. "
                    + "Terrible military potential.\"")

                    .Choice(
                        "Fair.",
                        "menu")

                .Line(
                    "sola",
                    "\"The furry assassin.\"")

                    .Choice(
                        "She's a cat.",
                        "cat")

                .Line(
                    "cat",
                    "\"I am aware of her propaganda.\"")

                    .Choice(
                        "Right.",
                        "menu")

                .Line(
                    "grell",
                    "\"That creature is forbidden from Crown territory.\"")

                    .Choice(
                        "No ridiculous nickname?",
                        "grellSerious")

                .Line(
                    "grellSerious",
                    "\"No.\"")

                    .Choice(
                        "Understood.",
                        "menu")

                // =====================================================
                // FRONT DOOR
                // =====================================================

                .Line(
                    "frontDoor",
                    "\"The door responds well to courtesy, "
                    + "poorly to force, and not at all to currency.\" "
                    + "Hautain glances down at his treasury. "
                    + "\"A respectable policy.\"")

                    .Choice(
                        "So I should knock politely?",
                        "doorAdvice")

                .Line(
                    "doorAdvice",
                    "\"You should ALWAYS knock politely.\" "
                    + "He leans forward. "
                    + "\"One never knows whether the door is listening.\"")

                    .Choice(
                        "That's not reassuring.",
                        "notReassuring")

                .Line(
                    "notReassuring",
                    "\"Reassurance is not a Crown service.\"")

                    .Choice(
                        "Fair enough.")

                // =====================================================
                // RETURNING THE ROYAL GIFT
                // =====================================================

                .Line(
                    "keepit",
                    "Hautain recoils from the offered Royal Gold Coin. "
                    + "\"RETURN a royal gift?! "
                    + "Do you also return birthdays, you ill-bred puddle drinker?\"")

                    .Choice(
                        "I thought you might want it back.",
                        "giftMeaning")

                    .Choice(
                        "As you command, Your Majesty.",
                        "keepGift")

                .Line(
                    "giftMeaning",
                    "\"The coin means the Crown judged you worthy of trust.\" "
                    + "Hautain pushes it back toward you with his sceptre. "
                    + "\"Spend it. Treasure it. Lose it foolishly if that is your custom. "
                    + "But do not return it merely because you have forgotten what it means.\"")

                    .Choice(
                        "I'll keep it.",
                        "keepGift")

                .Line(
                    "keepGift",
                    "\"Good.\" "
                    + "Hautain settles back onto the lily pad. "
                    + "\"The matter is closed by Royal Decree.\"")

                    .Choice(
                        "Long live King Hautain.")

                // =====================================================
                // AFTER DEFEATING HAUTAIN
                // =====================================================

                .Line(
                    "beaten",
                    "King Hautain sits behind a torn lily leaf. "
                    + "His sceptre is bent almost in half, "
                    + "but his crown remains perfectly straight. "
                    + "\"Congratulations. You have defeated a frog.\" "
                    + "He glares at you. "
                    + "\"I hope history gives you the statue you deserve.\"")

                    .Choice(
                        "Are you all right?",
                        "beatenConcern")

                    .Choice(
                        "You challenged me.",
                        "beatenBlame")

                    .Choice(
                        "Not much of a king now.",
                        "beatenInsult")

                    .Choice(
                        "(Leave him alone)")

                .Line(
                    "beatenConcern",
                    "\"The Crown is bruised, humiliated, "
                    + "and entirely operational.\" "
                    + "Hautain tries to straighten the sceptre. "
                    + "It bends further. "
                    + "\"The sceptre has entered a period of reform.\"")

                    .Choice(
                        "I'll leave you to it.")

                .Line(
                    "beatenBlame",
                    "\"A king who permits theft without resistance "
                    + "is merely a spectator wearing jewellery.\"")

                    .Choice(
                        "Fair enough.")

                .Line(
                    "beatenInsult",
                    "\"And yet somehow still considerably more royal than you.\"")

                    .Choice(
                        "Can't argue with that.")

                // =====================================================
                // POLITE EXIT
                // =====================================================

                .Line(
                    "leave",
                    "\"Permission to withdraw is granted.\" "
                    + "Hautain raises his sceptre. "
                    + "\"Mind the eastern stone. It is loose, "
                    + "and the Crown accepts no liability for damaged ankles.\"")

                    .Choice(
                        "(Bow and step away)");

        public static Dialogue QuazimodoFrontDoor { get; } =
    NewDialogue("QuazimodoFrontDoor", "Quazimodo")
        .Portrait("Assets/Characters/Portraits/Quazimodo_portrait_framed.png")                     
        .StandAt(60, 350, 400)                      
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
