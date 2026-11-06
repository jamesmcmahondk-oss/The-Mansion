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
        // THE SAD GARGOYLE
        // FrontGargoyle scene, TALK
        //
        // This is the unnamed front gargoyle, NOT Dûnán.
        //
        // Dialogue rules:
        // - Maximum 5 choices per node.
        // - No manual line breaks.
        // - Keep each dialogue panel to 1-3 compact sentences.
        // =========================================================
        // =========================================================
        // SAD GARGOYLE
        //
        // Only one gargoyle remains at the Front Gate.
        //
        // The other gargoyle did not break, disappear mysteriously,
        // or become the victim of some terrible Gothic incident.
        //
        // The other gargoyle simply left.
        //
        // This is funny initially, but it also establishes the remaining
        // gargoyle's central problem:
        //
        // Nothing physically keeps the gargoyle on the pillar.
        // Duty, habit, fear, and wounded pride keep the gargoyle there.
        // =========================================================

        public static Dialogue SadGargoyle { get; } =
            NewDialogue("SadGargoyle", "Sad Gargoyle")
                .MetFlag("MetGargoyle")

                .StartAt(p =>
                    !p.HasFlag("MetGargoyle")
                        ? "start"
                        : !p.HasFlag("GargoyleNoticedRain")
                            ? "rain"
                            : p.HasFlag("LearnedGargoyleName") && !p.HasFlag("ToldGargoyleName")
                                ? "foundNameStart"
                                : "again")

                .Portrait(
                    "Assets/Characters/Portraits/Gargoyle_portrait_framed.png")

                // =====================================================
                // ITEMS
                // =====================================================

                // He wears it for the rest of this visit (the close-up shows
                // it) until a choice leads to "hatRemove".
                .OnItem(
                    "Party Hat",
                    "hat",
                    effect: p => p.SetFlag("GargoyleWearingHat"))

                .OnOtherItems(
                    "The gargoyle examines the {item}. " +
                    "\"No, thank you. I already possess several things I never asked for. " +
                    "Most of them are moss.\"")

                // =====================================================
                // FIRST MEETING
                // =====================================================

                .Line(
                    "start",
                    "The gargoyle watches you through the rain. " +
                    "After several seconds, the gargoyle glances toward the empty pillar opposite. " +
                    "\"Oh. You stopped.\"")
                    .Choice(
                        "Hello.",
                        "hello")
                    .Choice(
                        "What happened to the other gargoyle?",
                        "otherGone")
                    .Choice(
                        "How do I get into the Mansion?",
                        "door")
                    .Choice(
                        "Why do you look so miserable?",
                        "sad")
                    .Choice(
                        "Goodbye.",
                        "goodbye")

                .Line(
                    "hello",
                    "\"Hello.\" " +
                    "The gargoyle considers the word carefully. " +
                    "\"That was pleasant. Brief, but pleasant.\"")
                    .Choice(
                        "Don't people usually say hello?",
                        "visitors")
                    .Choice(
                        "What's your name?",
                        "name")
                    .Choice(
                        "What happened to the other gargoyle?",
                        "otherGone")
                    .Choice(
                        "Can you help me get inside?",
                        "door")
                    .Choice(
                        "I'll leave you to it.",
                        "goodbye")

                // =====================================================
                // SECOND VISIT: SOMETHING WRONG WITH THE PLAYER
                // =====================================================

                .Line(
                    "rain",
                    "The gargoyle squints at you through the drizzle. " +
                    "\"You don't cast rain properly.\"")
                    .Choice(
                        "What?",
                        "rainNothing",
                        effect: p => p.SetFlag("GargoyleNoticedRain"))

                .Line(
                    "rainNothing",
                    "\"Nothing.\" " +
                    "The gargoyle looks away rather quickly. " +
                    "\"I have recently learned the value of not completing certain observations.\"")
                    .Choice(
                        "What does that mean?",
                        "rainRefusal")
                    .Choice(
                        "Did the other gargoyle teach you that?",
                        "rainOther")
                    .Choice(
                        "Never mind.",
                        "menu")

                .Line(
                    "rainRefusal",
                    "\"It means the rain passes through your outline incorrectly.\" " +
                    "A pause. " +
                    "\"It also means I am no longer discussing it.\"")
                    .Choice(
                        "That's not reassuring.",
                        "menu")

                .Line(
                    "rainOther",
                    "\"No. The other gargoyle generally completed observations at exhausting length.\"")
                    .Choice(
                        "Where is the other gargoyle?",
                        "otherGone")
                    .Choice(
                        "I see.",
                        "menu")

                // =====================================================
                // MAIN MENU
                // =====================================================

                .Line(
                    "menu",
                    "\"Was there something else? " +
                    "I have nowhere to be. Apparently that is now a personal choice.\"")
                    .Choice(
                        "Tell me about yourself.",
                        "about")
                    .Choice(
                        "What happened to the other gargoyle?",
                        "otherGone")
                    .Choice(
                        "How do I get into the Mansion?",
                        "door")
                    .Choice(
                        "What have you seen from up here?",
                        "observations")
                    .Choice(
                        "That's all for now.",
                        "goodbye")

                // =====================================================
                // RETURN VISITS
                // =====================================================

                // =====================================================
                // THE NAME (read in the Portrait Ledger: Cairn the Steadfast).
                // Not a resolution - the gargoyle stays on the pillar.
                // =====================================================

                .Line(
                    "foundNameStart",
                    "\"You've returned.\" " +
                    "The gargoyle looks toward the empty pillar, and then, for once, at you.")
                    .Choice(
                        "I found your name.",
                        "foundName")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "foundName",
                    "The gargoyle goes very still. Rain runs off the stone brow. " +
                    "For a long time the gargoyle says nothing at all.")
                    .Choice(
                        "\"Cairn the Steadfast.\"",
                        "nameRead",
                        effect: p => p.SetFlag("ToldGargoyleName"))
                    .Choice(
                        "Perhaps another time.",
                        "menu")

                .Line(
                    "nameRead",
                    "Another long silence. Then, very quietly: " +
                    "\"Dúnán remembered.\"")
                    .Choice(
                        "It was written in a ledger in the Portrait Gallery.",
                        "notWhatIMeant")

                .Line(
                    "notWhatIMeant",
                    "\"That is not what I meant.\" " +
                    "The gargoyle turns back to the rain. After a while, under its breath, testing whether it still fits: \"Cairn.\"")
                    .Choice(
                        "It suits you.",
                        "nameSuits")
                    .Choice(
                        "I'll leave you with it.",
                        "goodbye")

                .Line(
                    "nameSuits",
                    "\"It is a name for something that stays put.\" " +
                    "A pause. \"I am no longer certain that is a compliment.\"")
                    .SameChoicesAs("menu")

                .Line(
                    "again",
                    "\"You've returned.\" " +
                    "The gargoyle looks toward the empty pillar. " +
                    "\"Good. That makes one of you.\"")
                    .SameChoicesAs("menu")

                // =====================================================
                // ABOUT THE GARGOYLE
                // =====================================================

                .Line(
                    "about",
                    "\"There is remarkably little to tell. " +
                    "I sit, I watch, and rain occurs.\"")
                    .Choice(
                        "What's your name?",
                        "name")
                    .Choice(
                        "How long have you been here?",
                        "years")
                    .Choice(
                        "Why are you sad?",
                        "sad")
                    .Choice(
                        "Can't you leave?",
                        "leavePillar")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "name",
                    "\"I had a name once.\"")
                    .Choice(
                        "What happened to it?",
                        "forgotName")
                    .Choice(
                        "Did the other gargoyle know it?",
                        "otherKnewName")
                    .Choice(
                        "Can I call you Gargoyle?",
                        "callGargoyle")
                    .Choice(
                        "Never mind.",
                        "menu")

                .Line(
                    "forgotName",
                    "\"People stopped using it. Eventually, so did I.\"")
                    .Choice(
                        "That's rather sad.",
                        "nameSad")
                    .Choice(
                        "Perhaps you'll remember.",
                        "nameRemember")
                    .Choice(
                        "Gargoyle will do for now.",
                        "callGargoyle")

                .Line(
                    "otherKnewName",
                    "\"Yes.\" " +
                    "The gargoyle glances at the empty pillar. " +
                    "\"That information appears to have left with the relevant party.\"")
                    .Choice(
                        "You could go and ask.",
                        "goAsk")
                    .Choice(
                        "That's unfortunate.",
                        "menu")

                .Line(
                    "nameSad",
                    "\"Perhaps. On the other hand, it might have been dreadful.\"")
                    .Choice(
                        "A fair point.",
                        "menu")

                .Line(
                    "nameRemember",
                    "\"Possibly. I remembered a pigeon from 1846 yesterday. " +
                    "There is clearly still some activity.\"")
                    .Choice(
                        "What was special about it?",
                        "pigeon")
                    .Choice(
                        "That's encouraging.",
                        "menu")

                .Line(
                    "callGargoyle",
                    "\"Original.\"")
                    .Choice(
                        "You don't like it?",
                        "sarcasm")
                    .Choice(
                        "I'll work on it.",
                        "menu")

                .Line(
                    "sarcasm",
                    "\"Three centuries to perfect sarcasm. " +
                    "Please don't make me explain it.\"")
                    .Choice(
                        "Point taken.",
                        "menu")

                // =====================================================
                // TIME AND ROUTINE
                // =====================================================

                .Line(
                    "years",
                    "\"Three hundred and seventeen years. Possibly eighteen. " +
                    "There was a winter when nobody remembered my birthday.\"")
                    .Choice(
                        "When is your birthday?",
                        "birthday")
                    .Choice(
                        "How do you pass the time?",
                        "routine")
                    .Choice(
                        "That sounds unbearable.",
                        "unbearable")
                    .Choice(
                        "Was the other gargoyle here all that time?",
                        "otherYears")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "birthday",
                    "\"We never established one. " +
                    "That may have contributed to the problem.\"")
                    .Choice(
                        "We could choose today.",
                        "birthdayToday")
                    .Choice(
                        "The other gargoyle forgot?",
                        "otherBirthday")
                    .Choice(
                        "That would explain it.",
                        "menu")

                .Line(
                    "birthdayToday",
                    "\"Today?\" " +
                    "The gargoyle looks at the sky. " +
                    "\"The weather is disappointing. Yes, that seems appropriate.\"")
                    .Choice(
                        "Happy birthday.",
                        "birthdayThanks")
                    .Choice(
                        "We can reconsider.",
                        "menu")

                .Line(
                    "birthdayThanks",
                    "\"Thank you. I shall try not to look older.\"")
                    .Choice(
                        "You're doing very well.",
                        "menu")

                .Line(
                    "otherBirthday",
                    "\"The other gargoyle claimed birthdays encouraged expectations.\"")
                    .Choice(
                        "That sounds convenient.",
                        "menu")

                .Line(
                    "unbearable",
                    "\"Only if you think about it. Unfortunately, I have had time.\"")
                    .Choice(
                        "I'll change the subject.",
                        "menu")

                .Line(
                    "routine",
                    "\"Rain, pigeons, moss, and occasionally a visitor. " +
                    "Sometimes Hautain declares war on one. Then rain again.\"")
                    .Choice(
                        "Tell me about Hautain.",
                        "hautain",
                        onlyIf: p => p.HasFlag("MetHautain"))
                    .Choice(
                        "What's wrong with pigeons?",
                        "pigeon")
                    .Choice(
                        "Didn't you talk to the other gargoyle?",
                        "otherConversation")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "visitors",
                    "\"People look at the house, then the fountain. " +
                    "Occasionally they look at me and say, 'That's an ugly one.'\"")
                    .Choice(
                        "That's rude.",
                        "rudeVisitors")
                    .Choice(
                        "You are rather imposing.",
                        "imposing")
                    .Choice(
                        "What did they say about the other one?",
                        "otherCompliment")
                    .Choice(
                        "Well, hello again.",
                        "helloAgain")

                .Line(
                    "rudeVisitors",
                    "\"Yes. Especially when they were not referring to me. " +
                    "It made the exchange confusing.\"")
                    .Choice(
                        "I can imagine.",
                        "menu")

                .Line(
                    "imposing",
                    "\"Thank you. I was intended to discourage evil. " +
                    "Results have been mixed.\"")
                    .Choice(
                        "At least you're still trying.",
                        "menu")

                .Line(
                    "otherCompliment",
                    "\"They called the other gargoyle expressive.\" " +
                    "The gargoyle looks toward the empty pillar. " +
                    "\"Apparently expression includes walking away.\"")
                    .Choice(
                        "You sound offended.",
                        "offendedOther")
                    .Choice(
                        "Perhaps it does.",
                        "menu")

                .Line(
                    "helloAgain",
                    "\"Careful. I may begin expecting this.\"")
                    .Choice(
                        "I'll visit again.",
                        "visitAgain")

                .Line(
                    "pigeon",
                    "\"A pigeon can look directly into your eyes while ruining your afternoon. " +
                    "There is something deliberate about that.\"")
                    .Choice(
                        "They probably don't mean it.",
                        "pigeonIntent")
                    .Choice(
                        "Was the Incident about a pigeon?",
                        "incident")
                    .Choice(
                        "I'll take your word for it.",
                        "menu")

                .Line(
                    "pigeonIntent",
                    "\"You have not spent three centuries gathering evidence.\"")
                    .Choice(
                        "Fair enough.",
                        "menu")

                // =====================================================
                // SADNESS AND COMPANIONSHIP
                // =====================================================

                .Line(
                    "sad",
                    "\"Sad? Mostly bored. My face was carved this way.\"")
                    .Choice(
                        "Why carve you looking miserable?",
                        "sculptor")
                    .Choice(
                        "So you're happy?",
                        "happy")
                    .Choice(
                        "Can I cheer you up?",
                        "cheer")
                    .Choice(
                        "Is this about the other gargoyle?",
                        "aboutOther")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "sculptor",
                    "\"The sculptor was going through a divorce. " +
                    "I believe I represent his opinion of marriage.\"")
                    .Choice(
                        "That explains the expression.",
                        "menu")

                .Line(
                    "happy",
                    "\"I am a stone creature outside a haunted house in the rain. " +
                    "Let us not demand extremes.\"")
                    .Choice(
                        "Fair.",
                        "menu")

                .Line(
                    "cheer",
                    "\"Conversation is sufficient.\"")
                    .Choice(
                        "Really?",
                        "conversation")
                    .Choice(
                        "I could tell you a joke.",
                        "joke")
                    .Choice(
                        "Then I'll come back.",
                        "visitAgain")

                .Line(
                    "conversation",
                    "\"You ask questions and wait for the answers. " +
                    "That puts you ahead of the pigeons.\"")
                    .Choice(
                        "A high standard.",
                        "menu")
                    .Choice(
                        "I'll visit again.",
                        "visitAgain")

                .Line(
                    "joke",
                    "\"Does it involve a stone creature walking into a tavern?\"")
                    .Choice(
                        "Possibly.",
                        "jokeAnswer")
                    .Choice(
                        "No.",
                        "jokeAnswer")

                .Line(
                    "jokeAnswer",
                    "\"I envy its mobility.\"")
                    .Choice(
                        "You could walk into a tavern.",
                        "couldLeave")
                    .Choice(
                        "I'll spare you the rest.",
                        "menu")

                .Line(
                    "visitAgain",
                    "\"I would like that. Within reason. " +
                    "I do have a schedule.\"")
                    .Choice(
                        "Rain, pigeons, moss?",
                        "schedule")
                    .Choice(
                        "Naturally.",
                        "menu")

                .Line(
                    "schedule",
                    "\"Precisely. I cannot let everything slide.\"")
                    .Choice(
                        "Unlike the other gargoyle?",
                        "otherSlide")
                    .Choice(
                        "Of course not.",
                        "menu")

                .Line(
                    "otherSlide",
                    "\"The other gargoyle climbed. There was very little sliding.\"")
                    .Choice(
                        "So the other gargoyle really left?",
                        "truthOther")
                    .Choice(
                        "Good technique, then.",
                        "menu")

                // =====================================================
                // THE MISSING GARGOYLE
                // =====================================================

                .Line(
                    "otherGone",
                    "The gargoyle looks toward the empty pillar. " +
                    "\"The other gargoyle and I no longer work together.\"")
                    .Choice(
                        "Where did the other gargoyle go?",
                        "whereOther")
                    .Choice(
                        "Why did the other gargoyle leave?",
                        "whyOther")
                    .Choice(
                        "Did something happen?",
                        "incident")
                    .Choice(
                        "You could have led with 'left.'",
                        "ledWithLeft")
                    .Choice(
                        "Never mind.",
                        "menu")

                .Line(
                    "whereOther",
                    "\"Away.\"")
                    .Choice(
                        "That's not a place.",
                        "awayNotPlace")
                    .Choice(
                        "Did the other gargoyle say where?",
                        "otherDestination")
                    .Choice(
                        "Helpful.",
                        "menu")

                .Line(
                    "awayNotPlace",
                    "\"From this pillar, it is an excellent place.\"")
                    .Choice(
                        "Do you miss the other gargoyle?",
                        "missOther")
                    .Choice(
                        "Why don't you follow?",
                        "goAsk")
                    .Choice(
                        "Fair enough.",
                        "menu")

                .Line(
                    "otherDestination",
                    "\"The exact words were, 'Somewhere with fewer pigeons and less you.'\"")
                    .Choice(
                        "That was harsh.",
                        "otherHarsh")
                    .Choice(
                        "Very specific.",
                        "otherSpecific")
                    .Choice(
                        "Do you miss the other gargoyle?",
                        "missOther")

                .Line(
                    "otherHarsh",
                    "\"Yes. The lack of forwarding address was particularly inconsiderate.\"")
                    .Choice(
                        "That isn't what I meant.",
                        "menu")

                .Line(
                    "otherSpecific",
                    "\"There are many places with fewer pigeons. " +
                    "The second requirement narrows matters considerably.\"")
                    .Choice(
                        "You could go looking.",
                        "goAsk")
                    .Choice(
                        "I see.",
                        "menu")

                .Line(
                    "whyOther",
                    "\"The other gargoyle claimed to be tired of the rain, the pigeons, " +
                    "the Mansion, and listening to me describe the rain, pigeons, and Mansion.\"")
                    .Choice(
                        "Was that unfair?",
                        "unfairOther")
                    .Choice(
                        "How long had this been building?",
                        "otherYears")
                    .Choice(
                        "Did you try to stop it?",
                        "stopOther")
                    .Choice(
                        "That sounds understandable.",
                        "understandableOther")

                .Line(
                    "unfairOther",
                    "\"Completely. Some of my observations about rain were excellent.\"")
                    .Choice(
                        "I'm sure they were.",
                        "menu")

                .Line(
                    "otherYears",
                    "\"We shared the gate for three hundred and sixteen years.\"")
                    .Choice(
                        "And then the other gargoyle just left?",
                        "truthOther")
                    .Choice(
                        "You must miss the company.",
                        "missOther")
                    .Choice(
                        "That's a long time.",
                        "longTimeOther")

                .Line(
                    "longTimeOther",
                    "\"It was. Most of it occurred very slowly.\"")
                    .Choice(
                        "I can imagine.",
                        "menu")

                .Line(
                    "stopOther",
                    "\"Of course not. I assumed the whole thing was rhetorical.\"")
                    .Choice(
                        "Even while the other gargoyle climbed down?",
                        "climbedDown")
                    .Choice(
                        "A reasonable mistake.",
                        "notReasonable")

                .Line(
                    "climbedDown",
                    "\"Especially then. It added emphasis.\"")
                    .Choice(
                        "When did you realize?",
                        "realizedGone")

                .Line(
                    "realizedGone",
                    "\"Shortly after the other gargoyle passed through the gate " +
                    "and became difficult to hear.\"")
                    .Choice(
                        "You really misread that situation.",
                        "misread")
                    .Choice(
                        "Did you call after the other gargoyle?",
                        "calledAfter")

                .Line(
                    "notReasonable",
                    "\"Thank you. It was not.\"")
                    .Choice(
                        "At least you know now.",
                        "menu")

                .Line(
                    "misread",
                    "\"In my defence, nothing similar had happened during the previous three centuries.\"")
                    .Choice(
                        "Fair defence.",
                        "menu")

                .Line(
                    "calledAfter",
                    "\"I said, 'You'll be back.'\"")
                    .Choice(
                        "Was there an answer?",
                        "answerBack")

                .Line(
                    "answerBack",
                    "\"Yes.\" " +
                    "The gargoyle becomes very interested in the rain. " +
                    "\"It was not encouraging.\"")
                    .Choice(
                        "What was it?",
                        "lastWords")
                    .Choice(
                        "I won't ask.",
                        "menu")

                .Line(
                    "lastWords",
                    "\"The reply contained a detailed suggestion involving a pigeon.\"")
                    .Choice(
                        "The Incident?",
                        "incident")
                    .Choice(
                        "I understand.",
                        "menu")

                .Line(
                    "understandableOther",
                    "\"You have known the other gargoyle for no time at all, " +
                    "and already you are taking sides.\"")
                    .Choice(
                        "I'm not taking sides.",
                        "takingSides")
                    .Choice(
                        "Perhaps there were good reasons.",
                        "goodReasons")
                    .Choice(
                        "Sorry.",
                        "menu")

                .Line(
                    "takingSides",
                    "\"Good. The other side is currently elsewhere.\"")
                    .Choice(
                        "Right.",
                        "menu")

                .Line(
                    "goodReasons",
                    "\"Those were the exact words used.\"")
                    .Choice(
                        "That doesn't make them wrong.",
                        "menu")

                .Line(
                    "ledWithLeft",
                    "\"Left is an unnecessarily simple description of a complicated betrayal.\"")
                    .Choice(
                        "So the other gargoyle left.",
                        "truthOther")
                    .Choice(
                        "Complicated betrayal. Understood.",
                        "menu")

                .Line(
                    "truthOther",
                    "\"Yes.\" " +
                    "The gargoyle looks at the empty pillar. " +
                    "\"The other gargoyle climbed down and walked away.\"")
                    .Choice(
                        "That's it?",
                        "thatsIt")
                    .Choice(
                        "Gargoyles can do that?",
                        "canDoThat")
                    .Choice(
                        "Why haven't you?",
                        "leavePillar")
                    .Choice(
                        "Do you miss the other gargoyle?",
                        "missOther")

                .Line(
                    "thatsIt",
                    "\"There was also shouting.\"")
                    .Choice(
                        "About what?",
                        "incident")
                    .Choice(
                        "Naturally.",
                        "menu")

                .Line(
                    "canDoThat",
                    "\"Apparently.\" " +
                    "The gargoyle looks down at the pillar beneath its feet. " +
                    "\"The discovery has introduced several administrative concerns.\"")
                    .Choice(
                        "Such as?",
                        "administrativeConcerns")
                    .Choice(
                        "You mean you could leave too.",
                        "couldLeave")

                .Line(
                    "administrativeConcerns",
                    "\"Whether leaving constitutes desertion, retirement, insubordination, " +
                    "or simply going somewhere less wet.\"")
                    .Choice(
                        "What do you think?",
                        "leavingMeaning")
                    .Choice(
                        "Probably the last one.",
                        "menu")

                .Line(
                    "leavingMeaning",
                    "\"I have had only three centuries to consider it.\"")
                    .Choice(
                        "Take your time.",
                        "menu")

                .Line(
                    "missOther",
                    "\"No.\" " +
                    "A long pause follows. " +
                    "\"The silence is merely arranged differently now.\"")
                    .Choice(
                        "You miss the other gargoyle.",
                        "admitMiss")
                    .Choice(
                        "I'll leave that alone.",
                        "menu")

                .Line(
                    "admitMiss",
                    "\"The other gargoyle was irritating, critical, impatient, " +
                    "and frequently correct.\"")
                    .Choice(
                        "That's not a no.",
                        "notNo")

                .Line(
                    "notNo",
                    "\"No.\" " +
                    "Another pause. " +
                    "\"It is not.\"")
                    .Choice(
                        "Perhaps you'll meet again.",
                        "meetAgain")
                    .Choice(
                        "I understand.",
                        "menu")

                .Line(
                    "meetAgain",
                    "\"Perhaps.\" " +
                    "The gargoyle looks at the road beyond the gate. " +
                    "\"One of us would have to move.\"")
                    .Choice(
                        "It could be you.",
                        "couldLeave")
                    .Choice(
                        "There's no hurry.",
                        "menu")

                .Line(
                    "aboutOther",
                    "\"No.\"")
                    .Choice(
                        "That was quick.",
                        "quickNo")
                    .Choice(
                        "All right.",
                        "menu")

                .Line(
                    "quickNo",
                    "\"I have had considerable time to prepare the answer.\"")
                    .Choice(
                        "Do you miss the other gargoyle?",
                        "missOther")
                    .Choice(
                        "I'll leave it alone.",
                        "menu")

                .Line(
                    "otherConversation",
                    "\"Constantly. The other gargoyle objected to this description.\"")
                    .Choice(
                        "Perhaps you did most of the talking.",
                        "mostTalking")
                    .Choice(
                        "What did you talk about?",
                        "otherTopics")

                .Line(
                    "mostTalking",
                    "\"That allegation formed a substantial part of our final conversation.\"")
                    .Choice(
                        "The Incident?",
                        "incident")
                    .Choice(
                        "I see.",
                        "menu")

                .Line(
                    "otherTopics",
                    "\"Rain, pigeons, visitors, the gradual decline of masonry, " +
                    "and whether silence can be considered a contribution.\"")
                    .Choice(
                        "What did you decide?",
                        "silenceContribution")

                .Line(
                    "silenceContribution",
                    "\"The other gargoyle demonstrated the argument by leaving.\"")
                    .Choice(
                        "Convincing.",
                        "menu")

                .Line(
                    "offendedOther",
                    "\"I am not offended.\"")
                    .Choice(
                        "You sound offended.",
                        "stillOffended")
                    .Choice(
                        "Of course not.",
                        "menu")

                .Line(
                    "stillOffended",
                    "\"That is the rain.\"")
                    .Choice(
                        "Naturally.",
                        "menu")

                // =====================================================
                // THE INCIDENT
                // =====================================================

                .Line(
                    "incident",
                    "\"We do not speak of the Incident.\"")
                    .Choice(
                        "You just mentioned it.",
                        "incidentMention")
                    .Choice(
                        "Was it why the other gargoyle left?",
                        "incidentCause")
                    .Choice(
                        "Was anyone hurt?",
                        "incidentHurt")
                    .Choice(
                        "Was it about a pigeon?",
                        "incidentBird")
                    .Choice(
                        "Fair enough.",
                        "menu")

                .Line(
                    "incidentMention",
                    "\"Mentioning and discussing are separate administrative categories.\"")
                    .Choice(
                        "That sounds invented.",
                        "inventedCategory")
                    .Choice(
                        "Very well.",
                        "menu")

                .Line(
                    "inventedCategory",
                    "\"Most categories are.\"")
                    .Choice(
                        "Fair point.",
                        "menu")

                .Line(
                    "incidentCause",
                    "\"The Incident did not cause the departure.\"")
                    .Choice(
                        "Then what did?",
                        "incidentTruth")
                    .Choice(
                        "So they were unrelated?",
                        "notUnrelated")

                .Line(
                    "incidentTruth",
                    "\"My refusal to acknowledge the Incident caused the departure.\"")
                    .Choice(
                        "What happened?",
                        "incidentBird")
                    .Choice(
                        "That sounds more likely.",
                        "menu")

                .Line(
                    "notUnrelated",
                    "\"Nothing is unrelated after two centuries of arguing about it.\"")
                    .Choice(
                        "Two centuries?",
                        "incidentYears")
                    .Choice(
                        "I see.",
                        "menu")

                .Line(
                    "incidentHurt",
                    "\"Dignity suffered extensive casualties.\"")
                    .Choice(
                        "Whose dignity?",
                        "incidentWhose")

                .Line(
                    "incidentWhose",
                    "\"Primarily mine. The pigeon recovered immediately.\"")
                    .Choice(
                        "What did the pigeon do?",
                        "incidentBird")

                .Line(
                    "incidentBird",
                    "\"A pigeon landed on my head.\"")
                    .Choice(
                        "That's the Incident?",
                        "incidentJustPigeon")
                    .Choice(
                        "What did the other gargoyle do?",
                        "otherLaughed")

                .Line(
                    "incidentJustPigeon",
                    "\"There were additional details.\"")
                    .Choice(
                        "Such as?",
                        "incidentDetails")

                .Line(
                    "incidentDetails",
                    "\"The pigeon remained for nine hours.\"")
                    .Choice(
                        "Why didn't the other gargoyle help?",
                        "otherLaughed")
                    .Choice(
                        "Tragic.",
                        "menu")

                .Line(
                    "otherLaughed",
                    "\"The other gargoyle laughed.\"")
                    .Choice(
                        "For nine hours?",
                        "laughedNineHours")
                    .Choice(
                        "That seems understandable.",
                        "understandableLaugh")

                .Line(
                    "laughedNineHours",
                    "\"There were pauses for breathing.\"")
                    .Choice(
                        "And you've argued about this ever since?",
                        "incidentYears")

                .Line(
                    "understandableLaugh",
                    "\"You are taking sides again.\"")
                    .Choice(
                        "Sorry.",
                        "menu")

                .Line(
                    "incidentYears",
                    "\"Two hundred and twelve years, four months, " +
                    "and approximately six days.\"")
                    .Choice(
                        "Still holding a grudge?",
                        "stoneGrudge")
                    .Choice(
                        "Very approximate.",
                        "incidentApproximate")
                    .Choice(
                        "Did you ever apologise?",
                        "incidentApology")

                .Line(
                    "stoneGrudge",
                    "\"I am made of stone. We hold things.\"")
                    .Choice(
                        "Apparently.",
                        "menu")

                .Line(
                    "incidentApproximate",
                    "\"There was fog. I lost an afternoon.\"")
                    .Choice(
                        "Understandable.",
                        "menu")

                .Line(
                    "incidentApology",
                    "\"I am considering the wording.\"")
                    .Choice(
                        "For two hundred years?",
                        "apologyWording")

                .Line(
                    "apologyWording",
                    "\"I would hate to make it awkward.\"")
                    .Choice(
                        "The other gargoyle already left.",
                        "alreadyAwkward")
                    .Choice(
                        "Naturally.",
                        "menu")

                .Line(
                    "alreadyAwkward",
                    "\"Yes. That has complicated delivery.\"")
                    .Choice(
                        "You could deliver it yourself.",
                        "goAsk")
                    .Choice(
                        "I see.",
                        "menu")

                // =====================================================
                // FRONT DOOR AND ENTRY
                // =====================================================

                .Line(
                    "door",
                    "\"Knock, wait, and be polite. " +
                    "Continue being polite after it becomes difficult.\"")
                    .Choice(
                        "Why would it become difficult?",
                        "quazimodo")
                    .Choice(
                        "How polite?",
                        "extraPolite")
                    .Choice(
                        "Would money help?",
                        "payment")
                    .Choice(
                        "What if I force my way in?",
                        "force")
                    .Choice(
                        "Thank you.",
                        "menu")

                .Line(
                    "extraPolite",
                    "\"More than saying 'please' while expecting obedience. " +
                    "Ask permission and accept that the answer belongs to someone else.\"")
                    .Choice(
                        "And that works?",
                        "mannersWork")
                    .Choice(
                        "I'll try.",
                        "menu")

                .Line(
                    "mannersWork",
                    "\"Occasionally. It is also less tiring than being thrown down the steps.\"")
                    .Choice(
                        "I'll start with manners.",
                        "menu")

                .Line(
                    "payment",
                    "\"Quazimodo has been known to accept a persuasive expression of gratitude.\"")
                    .Choice(
                        "Gold?",
                        "paymentGold")
                    .Choice(
                        "Would ordinary change do?",
                        "paymentChange")
                    .Choice(
                        "I thought the door wanted manners.",
                        "knockerManners")
                    .Choice(
                        "Good to know.",
                        "menu")

                .Line(
                    "paymentGold",
                    "\"Gold is traditionally persuasive. " +
                    "Even when it has a frog on it.\"")
                    .Choice(
                        "Hautain's coin?",
                        "paymentRoyal",
                        onlyIf: p => p.HasFlag("MetHautain"))
                    .Choice(
                        "Noted.",
                        "menu")

                .Line(
                    "paymentRoyal",
                    "\"If His Majesty gives you one. " +
                    "Do not confuse that with permission to empty the fountain.\"")
                    .Choice(
                        "Understood.",
                        "menu")

                .Line(
                    "paymentChange",
                    "\"Offer what you have to Quazimodo, not to the knocker. " +
                    "The knocker has no pockets.\"")
                    .Choice(
                        "That distinction seems important.",
                        "knockerManners")
                    .Choice(
                        "I'll see what I have.",
                        "menu")

                .Line(
                    "knockerManners",
                    "\"The knocker is iron and has no standards. " +
                    "The gentleman who answers does. Some are negotiable.\"")
                    .Choice(
                        "Tell me about Quazimodo.",
                        "quazimodo",
                        onlyIf: p => p.HasFlag("MetQuazimodo"))
                    .Choice(
                        "Thanks.",
                        "menu")

                .Line(
                    "force",
                    "\"You could try.\"")
                    .Choice(
                        "Would it work?",
                        "forceResult")
                    .Choice(
                        "I'll try asking first.",
                        "menu")

                .Line(
                    "forceResult",
                    "\"Something would give way. " +
                    "Whether it would be the door or part of you is less certain.\"")
                    .Choice(
                        "Manners it is.",
                        "menu")

                .Line(
                    "quazimodo",
                    "\"Quazimodo. Butler, doorman, and cleaner of visitors " +
                    "who mistake those jobs for weaknesses.\"")
                    .Choice(
                        "What's Quazimodo like?",
                        "quazimodoLike",
                        onlyIf: p => p.HasFlag("MetQuazimodo"))
                    .Choice(
                        "Can I trust Quazimodo?",
                        "quazimodoTrust",
                        onlyIf: p => p.HasFlag("MetQuazimodo"))
                    .Choice(
                        "Any advice?",
                        "quazimodoAdvice")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "quazimodoLike",
                    "\"Dry, patient, and rather stronger than his posture suggests.\"")
                    .Choice(
                        "Good to know.",
                        "menu")

                .Line(
                    "quazimodoTrust",
                    "\"To keep his word? Generally. " +
                    "To tell you everything? Certainly not.\"")
                    .Choice(
                        "Fair distinction.",
                        "menu")

                .Line(
                    "quazimodoAdvice",
                    "\"Do not begin by asking what happened to his face. " +
                    "He has heard the question and improved the answer.\"")
                    .Choice(
                        "I'll be careful.",
                        "menu")

                // =====================================================
                // OBSERVATIONS AND THE MANSION
                // =====================================================

                .Line(
                    "observations",
                    "\"Three centuries facing a driveway. " +
                    "I have developed a thorough understanding of arrivals.\"")
                    .Choice(
                        "Tell me about the Mansion.",
                        "mansion")
                    .Choice(
                        "Tell me about Hautain.",
                        "hautain",
                        onlyIf: p => p.HasFlag("MetHautain"))
                    .Choice(
                        "Who visits?",
                        "arrivals")
                    .Choice(
                        "Do people leave?",
                        "departures")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "mansion",
                    "\"Lights appear in empty windows, and footsteps cross rooms with no floors. " +
                    "Sometimes the roofline changes when the fog clears.\"")
                    .Choice(
                        "Does the building change?",
                        "houseChanges")
                    .Choice(
                        "Is it dangerous?",
                        "houseDanger")
                    .Choice(
                        "Have you been inside?",
                        "inside")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "houseChanges",
                    "\"I cannot prove it, but I have looked at that roof rather a lot.\"")
                    .Choice(
                        "I'll take your word for it.",
                        "menu")

                .Line(
                    "houseDanger",
                    "\"Yes, but so is a staircase in the rain. " +
                    "Pay attention, and do not assume everything frightening wants to hurt you.\"")
                    .Choice(
                        "Including you?",
                        "frighteningGargoyle")
                    .Choice(
                        "Useful advice.",
                        "menu")

                .Line(
                    "frighteningGargoyle",
                    "\"Especially me. I would have to stand up. " +
                    "It would become an occasion.\"")
                    .Choice(
                        "Let's avoid that.",
                        "menu")

                .Line(
                    "inside",
                    "\"I remember a workshop, a cart, and a disagreement " +
                    "about which direction I should face.\"")
                    .Choice(
                        "And then?",
                        "installed")

                .Line(
                    "installed",
                    "\"This. For three centuries.\"")
                    .Choice(
                        "I'm sorry.",
                        "menu")
                    .Choice(
                        "At least they chose a good view.",
                        "view")

                .Line(
                    "view",
                    "\"They chose the driveway. I believe the garden was extra.\"")
                    .Choice(
                        "Of course.",
                        "menu")

                .Line(
                    "arrivals",
                    "\"Travellers, tradesmen, people carrying invitations, " +
                    "and people insisting they had invitations.\"")
                    .Choice(
                        "Who gets admitted?",
                        "admitted")
                    .Choice(
                        "Any memorable visitors?",
                        "memorable")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "admitted",
                    "\"That is Quazimodo's department. " +
                    "Mine is looking discouraging and carrying rainwater.\"")
                    .Choice(
                        "You're doing splendidly.",
                        "menu")

                .Line(
                    "memorable",
                    "\"A man once arrived with a ladder to repair a third-floor window.\"")
                    .Choice(
                        "What happened?",
                        "ladder")

                .Line(
                    "ladder",
                    "\"He spent an hour looking for it. " +
                    "There had been a third floor when he set out.\"")
                    .Choice(
                        "Did he leave?",
                        "ladderLeave")

                .Line(
                    "ladderLeave",
                    "\"Eventually. He invoiced for the hour. I admired that.\"")
                    .Choice(
                        "Quite right.",
                        "menu")

                .Line(
                    "departures",
                    "\"Some leave. Some return. Some stand at the gate " +
                    "for a very long time first.\"")
                    .Choice(
                        "Like the other gargoyle?",
                        "otherDeparture")
                    .Choice(
                        "What are they waiting for?",
                        "gateWaiting")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "otherDeparture",
                    "\"No. The other gargoyle did not hesitate at all.\"")
                    .Choice(
                        "That still bothers you.",
                        "missOther")
                    .Choice(
                        "I see.",
                        "menu")

                .Line(
                    "gateWaiting",
                    "\"I never ask. People standing at gates usually have enough questions.\"")
                    .Choice(
                        "Fair enough.",
                        "menu")

                // =====================================================
                // KING HAUTAIN
                // =====================================================

                .Line(
                    "hautain",
                    "\"King Hautain the First. Do use the title. " +
                    "It saves everyone time.\"")
                    .Choice(
                        "Is Hautain really a king?",
                        "hautainKing")
                    .Choice(
                        "Anything I should avoid?",
                        "hautainWarning")
                    .Choice(
                        "Hautain seems harmless.",
                        "hautainHarmless")
                    .Choice(
                        "Do you get along?",
                        "hautainFriend")
                    .Choice(
                        "Something else.",
                        "menu")

                .Line(
                    "hautainKing",
                    "\"He has a crown, territory, and strong opinions about borders. " +
                    "I have met human kings with fewer qualifications.\"")
                    .Choice(
                        "Fair enough.",
                        "menu")

                .Line(
                    "hautainWarning",
                    "\"Do not take his coins, call him small, or mention herons " +
                    "unless you have an afternoon to spare.\"")
                    .Choice(
                        "Noted.",
                        "menu")

                .Line(
                    "hautainHarmless",
                    "\"Hautain is small. So is a wasp. " +
                    "Neither appreciates the comparison.\"")
                    .Choice(
                        "I'll be diplomatic.",
                        "menu")

                .Line(
                    "hautainFriend",
                    "\"He calls me the Northern Stone Dignitary. I call him Your Majesty. " +
                    "Neither of us has anywhere else to be.\"")
                    .Choice(
                        "That sounds like friendship.",
                        "hautainFriendship")

                .Line(
                    "hautainFriendship",
                    "\"Do not tell Hautain. He would establish a ceremony.\"")
                    .Choice(
                        "Your secret is safe.",
                        "menu")

                // =====================================================
                // DUTY, CHOICE, AND THE EMPTY PILLAR
                // =====================================================

                .Line(
                    "leavePillar",
                    "\"I was placed here to watch the entrance.\"")
                    .Choice(
                        "That wasn't my question.",
                        "pillarQuestion")
                    .Choice(
                        "The other gargoyle managed it.",
                        "otherManaged")
                    .Choice(
                        "Are you attached to the pillar?",
                        "pillarQuestion")
                    .Choice(
                        "Fair enough.",
                        "menu")

                .Line(
                    "pillarQuestion",
                    "\"No. It was not.\"")
                    .Choice(
                        "There are no chains or mortar.",
                        "noticed",
                        onlyIf: p =>
                            p.Wits >= 12 &&
                            !p.Has("Gargoyle's Blessing"))
                    .Choice(
                        "Then what's keeping you here?",
                        "whatKeepsHere")
                    .Choice(
                        "I won't press.",
                        "menu")

                .Line(
                    "otherManaged",
                    "\"Yes. The other gargoyle has always been distressingly practical.\"")
                    .Choice(
                        "You could do the same.",
                        "couldLeave")
                    .Choice(
                        "Do you want to?",
                        "wantLeave")
                    .Choice(
                        "I understand.",
                        "menu")

                .Line(
                    "whatKeepsHere",
                    "\"Duty.\"")
                    .Choice(
                        "Duty isn't mortar.",
                        "duty")
                    .Choice(
                        "You've already done your duty.",
                        "warned")
                    .Choice(
                        "That's your choice, then.",
                        "choiceHere")

                .Line(
                    "noticed",
                    "You examine the gargoyle's feet. " +
                    "Nothing physically joins them to the pillar. " +
                    "\"Ah. You noticed.\"")
                    .Choice(
                        "You could leave.",
                        "couldLeave")
                    .Choice(
                        "Duty keeps you here.",
                        "duty")
                    .Choice(
                        "The empty pillar should have made this obvious.",
                        "obviousEmptyPillar")
                    .Choice(
                        "I won't tell anyone.",
                        "secretSafe")

                .Line(
                    "obviousEmptyPillar",
                    "\"I had hoped you would interpret that as structural damage.\"")
                    .Choice(
                        "There isn't any damage.",
                        "noDamage")
                    .Choice(
                        "The other gargoyle left, didn't it?",
                        "truthOther")

                .Line(
                    "noDamage",
                    "\"Yes. The evidence has been persistently unhelpful.\"")
                    .Choice(
                        "You could leave too.",
                        "couldLeave")
                    .Choice(
                        "Duty keeps you here.",
                        "duty")

                .Line(
                    "couldLeave",
                    "\"Perhaps. I was told to watch, warn, and remain. " +
                    "Nobody told me when to stop.\"")
                    .Choice(
                        "You can decide that yourself.",
                        "ownDecision")
                    .Choice(
                        "You've certainly warned me.",
                        "warned")
                    .Choice(
                        "Go find the other gargoyle.",
                        "goAsk")
                    .Choice(
                        "I'll leave it alone.",
                        "menu")

                .Line(
                    "wantLeave",
                    "\"Wanting is not the difficulty.\"")
                    .Choice(
                        "What is?",
                        "leavingDifficulty")
                    .Choice(
                        "Then leave.",
                        "ownDecision")

                .Line(
                    "leavingDifficulty",
                    "\"After three centuries, standing up would make the whole arrangement " +
                    "look rather foolish.\"")
                    .Choice(
                        "Only if you stay.",
                        "ownDecision")
                    .Choice(
                        "Nobody is judging you.",
                        "emptyPillarJudges")

                .Line(
                    "emptyPillarJudges",
                    "The gargoyle looks toward the empty pillar. " +
                    "\"The architecture is.\"")
                    .Choice(
                        "Architecture will recover.",
                        "ownDecision")
                    .Choice(
                        "Fair enough.",
                        "menu")

                .Line(
                    "duty",
                    "\"Duty is more effective than mortar. Mortar eventually cracks.\"")
                    .Choice(
                        "You've done your duty.",
                        "warned")
                    .Choice(
                        "You could choose something else.",
                        "ownDecision")
                    .Choice(
                        "The other gargoyle did.",
                        "otherChoice")
                    .Choice(
                        "I understand.",
                        "menu")

                .Line(
                    "otherChoice",
                    "\"Yes.\" " +
                    "The gargoyle looks toward the empty pillar. " +
                    "\"The other gargoyle was always untidily decisive.\"")
                    .Choice(
                        "Perhaps the other gargoyle was right.",
                        "ownDecision")
                    .Choice(
                        "Perhaps you'll meet again.",
                        "meetAgain")

                .Line(
                    "choiceHere",
                    "\"That is a deeply unpleasant way of phrasing it.\"")
                    .Choice(
                        "Because it's true?",
                        "choiceTruth")
                    .Choice(
                        "Sorry.",
                        "menu")

                .Line(
                    "choiceTruth",
                    "\"Yes.\"")
                    .Choice(
                        "You can make another choice.",
                        "ownDecision")
                    .Choice(
                        "Take your time.",
                        "menu")

                .Line(
                    "goAsk",
                    "\"That would require leaving the pillar.\"")
                    .Choice(
                        "Yes.",
                        "goAskYes")
                    .Choice(
                        "That is generally how looking works.",
                        "goAskYes")
                    .Choice(
                        "Never mind.",
                        "menu")

                .Line(
                    "goAskYes",
                    "\"You have reduced the problem to its most irritatingly accurate form.\"")
                    .Choice(
                        "Consider it.",
                        "ownDecision")
                    .Choice(
                        "You're welcome.",
                        "menu")

                .Line(
                    "ownDecision",
                    "The gargoyle looks toward the road, then toward the empty pillar. " +
                    "\"I shall consider it. Do not expect immediate movement. " +
                    "I have a reputation.\"")
                    .Choice(
                        "Take your time.",
                        "blessingOffer")
                    .Choice(
                        "Just don't take another three centuries.",
                        "threeCenturies")

                .Line(
                    "threeCenturies",
                    "\"No promises. I am new to urgency.\"")
                    .Choice(
                        "Fair enough.",
                        "blessingOffer")

                .Line(
                    "warned",
                    "\"Yes, I suppose I have. A productive evening. " +
                    "I shall need to sit down.\"")
                    .Choice(
                        "You're already sitting.",
                        "alreadySitting")

                .Line(
                    "alreadySitting",
                    "\"Then I am ahead of schedule.\"")
                    .Choice(
                        "Thank you for helping.",
                        "blessingOffer")

                .Line(
                    "secretSafe",
                    "\"Thank you. The other gargoyle would be insufferable " +
                    "if word reached wherever the other gargoyle has gone.\"")
                    .Choice(
                        "The other gargoyle already knows.",
                        "otherAlreadyKnows")
                    .Choice(
                        "I won't say a word.",
                        "blessingOffer")

                .Line(
                    "otherAlreadyKnows",
                    "\"Yes. That may explain the departure.\"")
                    .Choice(
                        "Perhaps.",
                        "blessingOffer")

                // =====================================================
                // BLESSING
                // =====================================================

                .Line(
                    "blessingOffer",
                    "\"You noticed what most people overlook. " +
                    "Allow me to return the courtesy. A little luck.\"")
                    .Choice(
                        "Thank you. I'll accept.",
                        "blessing",
                        onlyIf: p => !p.Has("Gargoyle's Blessing"),
                        effect: p =>
                        {
                            if (!p.Has("Gargoyle's Blessing"))
                            {
                                p.Luck += 1;
                                p.Give("Gargoyle's Blessing");
                            }
                        })
                    .Choice(
                        "Keep it. You may need it.",
                        "declineBlessing")

                .Line(
                    "blessing",
                    "The gargoyle touches one stone finger to your forehead. " +
                    "\"There. Try not to spend it all on something foolish.\" " +
                    "(Gargoyle's Blessing received. Luck +1.)")
                    .Choice(
                        "I'll try.",
                        "menu")
                    .Choice(
                        "No promises.",
                        "blessingFoolish")

                .Line(
                    "blessingFoolish",
                    "\"An honest answer. I shall lower my expectations accordingly.\"")
                    .Choice(
                        "Probably wise.",
                        "menu")

                .Line(
                    "declineBlessing",
                    "\"Perhaps I might need it. Ask again if you change your mind. " +
                    "I am unlikely to have gone shopping.\"")
                    .Choice(
                        "Unlike the other gargoyle.",
                        "shoppingOther")
                    .Choice(
                        "I'll remember.",
                        "menu")

                .Line(
                    "shoppingOther",
                    "\"If the other gargoyle returns with shopping, " +
                    "I shall consider the entire departure forgiven.\"")
                    .Choice(
                        "A generous policy.",
                        "menu")

                // =====================================================
                // PARTY HAT
                // =====================================================

                .Line(
                    "hat",
                    "You place the party hat on the gargoyle's head. " +
                    "\"I see we have reached the ceremonial portion of the visit.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_PartyHat_portrait_framed.png")
                    .Choice(
                        "It suits you.",
                        "hatSuits")
                    .Choice(
                        "Happy birthday.",
                        "hatBirthday")
                    .Choice(
                        "You look happier already.",
                        "hatHappy")
                    .Choice(
                        "The other gargoyle would be jealous.",
                        "hatOther")
                    .Choice(
                        "I'll take it off.",
                        "hatRemove",
                        effect: p => p.ClearFlag("GargoyleWearingHat"))

                .Line(
                    "hatSuits",
                    "\"I am an ancient guardian of stone and shadow. " +
                    "I am wearing a paper hat.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_PartyHat_portrait_framed.png")
                    .Choice(
                        "Both things can be true.",
                        "hatBoth")
                    .Choice(
                        "I'll remove it.",
                        "hatRemove",
                        effect: p => p.ClearFlag("GargoyleWearingHat"))

                .Line(
                    "hatBoth",
                    "\"I dislike how reasonable that is.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_PartyHat_portrait_framed.png")
                    .Choice(
                        "Would you like to keep it on?",
                        "hatKeep")
                    .Choice(
                        "I'll take it off.",
                        "hatRemove",
                        effect: p => p.ClearFlag("GargoyleWearingHat"))

                .Line(
                    "hatBirthday",
                    "\"Nobody has dressed me for an occasion before. " +
                    "The pigeons' intentions were less festive.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_PartyHat_portrait_framed.png")
                    .Choice(
                        "You're welcome.",
                        "hatThanks")
                    .Choice(
                        "Want to keep it on?",
                        "hatKeep")

                .Line(
                    "hatOther",
                    "\"The other gargoyle would laugh.\" " +
                    "A pause. " +
                    "\"Extensively.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_PartyHat_portrait_framed.png")
                    .Choice(
                        "Would that be so bad?",
                        "hatLaugh")
                    .Choice(
                        "I'll remove it.",
                        "hatRemove",
                        effect: p => p.ClearFlag("GargoyleWearingHat"))

                .Line(
                    "hatLaugh",
                    "\"No.\" " +
                    "The gargoyle looks toward the empty pillar. " +
                    "\"Not entirely.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_PartyHat_portrait_framed.png")
                    .Choice(
                        "Then perhaps it can stay for a moment.",
                        "hatKeep")
                    .Choice(
                        "I'll take it off.",
                        "hatRemove",
                        effect: p => p.ClearFlag("GargoyleWearingHat"))

                .Line(
                    "hatThanks",
                    "\"Thank you for the thought. The hat itself remains under review.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_PartyHat_portrait_framed.png")
                    .Choice(
                        "I'll remove it.",
                        "hatRemove",
                        effect: p => p.ClearFlag("GargoyleWearingHat"))
                    .Choice(
                        "What's wrong with it?",
                        "hatKeep")

                .Line(
                    "hatHappy",
                    "\"My face is carved from stone. Please manage your expectations.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_PartyHat_portrait_framed.png")
                    .Choice(
                        "Fair enough. I'll remove it.",
                        "hatRemove",
                        effect: p => p.ClearFlag("GargoyleWearingHat"))
                    .Choice(
                        "Want to keep wearing it?",
                        "hatKeep")

                .Line(
                    "hatKeep",
                    "\"No. The paper is scratching my scalp.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_PartyHat_portrait_framed.png")
                    .Choice(
                        "You have a scalp?",
                        "hatScalp")
                    .Choice(
                        "Right. Sorry.",
                        "hatRemove",
                        effect: p => p.ClearFlag("GargoyleWearingHat"))

                .Line(
                    "hatScalp",
                    "\"Apparently. Today has been educational.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_PartyHat_portrait_framed.png")
                    .Choice(
                        "I'll take it off.",
                        "hatRemove",
                        effect: p => p.ClearFlag("GargoyleWearingHat"))

                .Line(
                    "hatRemove",
                    "You remove the party hat. " +
                    "\"A memorable celebration. Let us not repeat it annually.\"",
                    portrait:
                        "Assets/Characters/Portraits/" +
                        "Gargoyle_portrait_framed.png")
                    .Choice(
                        "No promises.",
                        "menu")
                    .Choice(
                        "I'll leave you in peace.",
                        "goodbye")

                // =====================================================
                // GOODBYE
                // =====================================================

                .Line(
                    "goodbye",
                    "\"Goodbye. Do come back.\"")
                    .Choice(
                        "I will.")
                    .Choice(
                        "You just want someone to talk to.",
                        "goodbyeTalk")
                    .Choice(
                        "Maybe the other gargoyle will return.",
                        "goodbyeOther")

                .Line(
                    "goodbyeTalk",
                    "\"Obviously. I thought I had been remarkably transparent about it.\"")
                    .Choice(
                        "See you later.")

                .Line(
                    "goodbyeOther",
                    "The gargoyle looks at the empty pillar. " +
                    "\"Perhaps.\" " +
                    "A pause. " +
                    "\"Until then, you will have to do.\"")
                    .Choice(
                        "Charming.")
                    .Choice(
                        "I'll come back.");
        // =========================================================
        // KING HAUTAIN
        // FountainCloseup scene, ACT on the gold coins
        //
        // IMPORTANT:
        // Maximum of 5 player replies/choices on any dialogue node.
        // =========================================================

        public static Dialogue HautainCoins { get; } =
            NewDialogue("HautainCoins", "King Hautain")
                .MetFlag("MetHautain")
                .Portrait("Assets/Characters/Portraits/Hautain_portrait_framed.png")
                .Character("Assets/Characters/Hautain.png")
                .StandAt(495, 205, 285)

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
                    "\"What is THIS?\" Hautain inspects the {item}. "
                    + "\"The Crown does not accept bribes.\" "
                    + "He pauses. "
                    + "\"...Is it valuable?\"")

                // =====================================================
                // FIRST ENCOUNTER
                // 5 choices exactly.
                // =====================================================

                .Line(
                    "start",
                    "\"HALT!\" "
                    + "A tiny crowned figure rises between the lily pads and levels a sceptre at your hand. "
                    + "\"Remove your grasping extremity from the Royal Treasury! "
                    + "You stand before Hautain the First, Keeper of the Deep Basin, "
                    + "Lord of the Seven Lily Pads, Defender of the Western Drain, "
                    + "and Sovereign of all waters contained within this fountain!\"",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "King? You're a frog on a lily pad.",
                        "frogInsult")

                    .Choice(
                        "Forgive me, Your Majesty. I didn't realise.",
                        "apology")

                    .Choice(
                        "The Royal Treasury?",
                        "treasury")

                    .Choice(
                        "Surely the Crown can spare one coin for a loyal subject.",
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
                // MAIN AUDIENCE MENU
                // Maximum 5.
                // =====================================================

                .Line(
                    "menu",
                    "\"Well?\" Hautain draws himself up. "
                    + "\"You remain within the Royal Presence. "
                    + "Try to make better use of it.\"")

                    .Choice(
                        "Tell me about your kingdom.",
                        "kingdom")

                    .Choice(
                        "Why are those coins yours?",
                        "treasury")

                    .Choice(
                        "What do you know about the Mansion?",
                        "mansion")

                    .Choice(
                        "Any advice about the front door?",
                        "frontDoor")

                    .Choice(
                        "I'll be going.",
                        "leave")

                // =====================================================
                // THE TREASURY
                // =====================================================

                .Line(
                    "treasury",
                    "\"Tribute.\" "
                    + "Hautain gestures toward the coins beneath the water. "
                    + "\"Deposited by travellers, pilgrims, petitioners, "
                    + "and one extremely careless chimney sweep.\"")

                    .Choice(
                        "They're wishing coins.",
                        "wishes")

                    .Choice(
                        "So you found them first.",
                        "foundThem")

                    .Choice(
                        "Then they belong to the Crown.",
                        "recognise")

                    .Choice(
                        "Tell me about your kingdom.",
                        "kingdom")

                    .Choice(
                        "Never mind.",
                        "menu")

                .Line(
                    "wishes",
                    "\"Yes. Their petitions were received.\" "
                    + "Hautain glances at the water. "
                    + "\"The Crown regrets that fulfilment remains subject to staffing limitations.\"")

                    .Choice(
                        "Staffing limitations?",
                        "staffing")

                    .Choice(
                        "At least somebody heard them.",
                        "heardWishes")

                    .Choice(
                        "That's convenient.",
                        "menu")

                .Line(
                    "staffing",
                    "\"The King currently occupies every major office of state.\"")

                    .Choice(
                        "Every office?",
                        "offices")

                    .Choice(
                        "Sounds efficient.",
                        "efficient")

                    .Choice(
                        "Sounds lonely.",
                        "lonely")

                .Line(
                    "offices",
                    "\"Monarch. Treasurer. Magistrate. Ambassador. "
                    + "Minister of Drainage. Commander of the Royal Guard.\" "
                    + "He clears his throat. "
                    + "\"And, owing to recent budgetary constraints, Royal Guard.\"")

                    .Choice(
                        "So you have no guards.",
                        "noGuards")

                    .Choice(
                        "Very efficient.",
                        "efficient")

                    .Choice(
                        "Long live the bureaucracy.",
                        "efficient")

                .Line(
                    "noGuards",
                    "\"The Crown has never required guards against anyone possessing manners.\" "
                    + "Hautain narrows his eyes. "
                    + "\"Do not force a constitutional review.\"")

                    .Choice(
                        "I'll behave.",
                        "apology")

                    .Choice(
                        "I'd like to see you try.",
                        "challenge")

                    .Choice(
                        "Fair enough.",
                        "menu")

                .Line(
                    "efficient",
                    "\"Precisely! At last, someone with an elementary understanding of government.\"")

                    .Choice(
                        "Long live the King.",
                        "recognise")

                    .Choice(
                        "Does the government provide refreshments?",
                        "refreshments")

                    .Choice(
                        "Let's talk about something else.",
                        "menu")

                .Line(
                    "refreshments",
                    "\"The banquet department is undergoing restructuring.\" "
                    + "A fly passes overhead. Hautain watches it disappear. "
                    + "\"Extensive restructuring.\"")

                    .Choice(
                        "I see.",
                        "menu")

                .Line(
                    "heardWishes",
                    "\"A king who does not hear his subjects is merely a hat occupying a chair.\" "
                    + "He taps his crown. "
                    + "\"This is a very good hat.\"")

                    .Choice(
                        "It suits you.",
                        "recognise")

                    .Choice(
                        "That's surprisingly wise.",
                        "wise")

                    .Choice(
                        "I'll leave you to it.",
                        "leave")

                .Line(
                    "wise",
                    "\"Surprisingly?\" "
                    + "Hautain squints at you. "
                    + "\"The compliment arrives carrying an insult under its coat.\"")

                    .Choice(
                        "Fair.",
                        "menu")

                    .Choice(
                        "My apologies, Your Majesty.",
                        "apology")

                .Line(
                    "foundThem",
                    "\"FOUND them? You unlicensed turnip! "
                    + "The coins are held in trust by the sovereign authority of the fountain!\"")

                    .Choice(
                        "Which is you.",
                        "authority")

                    .Choice(
                        "Still sounds like finding them.",
                        "frogInsult")

                    .Choice(
                        "Fair enough.",
                        "menu")

                .Line(
                    "authority",
                    "\"By unanimous approval of the Crown, yes.\"")

                    .Choice(
                        "How many members voted?",
                        "vote")

                    .Choice(
                        "Long live the King.",
                        "recognise")

                .Line(
                    "vote",
                    "\"One.\"")

                    .Choice(
                        "A decisive majority.",
                        "efficient")

                // =====================================================
                // KINGDOM
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
                        "Where are your subjects?",
                        "subjects")

                    .Choice(
                        "What's the Western Drain?",
                        "westernDrain")

                    .Choice(
                        "Let's discuss something else.",
                        "menu")

                .Line(
                    "kingdomPraise",
                    "\"Finally! The Crown recognises this rare outbreak of judgement.\"")

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
                    "westernDrain",
                    "\"Site of the Crown's greatest military campaign.\"")

                    .Choice(
                        "Against whom?",
                        "drainEnemy")

                    .Choice(
                        "Sounds important.",
                        "menu")

                .Line(
                    "drainEnemy",
                    "\"An invading force possessing six legs and no respect for sovereign borders.\"")

                    .Choice(
                        "Ants?",
                        "ants")

                .Line(
                    "ants",
                    "\"The Crown does not dignify insurgents by naming them.\"")

                    .Choice(
                        "Definitely ants.",
                        "menu")

                .Line(
                    "subjects",
                    "Hautain gazes across the still fountain. "
                    + "\"Elsewhere.\" "
                    + "He straightens his crown. "
                    + "\"The private affairs of citizens are not discussed by the Crown.\"")

                    .Choice(
                        "Are you alone here?",
                        "lonely")

                    .Choice(
                        "Understood.",
                        "menu")

                .Line(
                    "lonely",
                    "\"A sovereign is never alone.\" "
                    + "A pause. "
                    + "\"A sovereign is temporarily without court.\"")

                    .Choice(
                        "That sounds lonely.",
                        "lonelyTruth")

                    .Choice(
                        "Then consider yourself witnessed.",
                        "recognise")

                    .Choice(
                        "I should leave you to your duties.",
                        "leave")

                .Line(
                    "lonelyTruth",
                    "\"Kingship is not intended to be comfortable.\" "
                    + "Hautain taps his sceptre against the lily pad. "
                    + "\"It is intended to be witnessed.\"")

                    .Choice(
                        "Then I witness King Hautain.",
                        "recognise")

                    .Choice(
                        "I'll leave you to your kingdom.",
                        "leave")

                // =====================================================
                // APOLOGY / RECOGNITION
                // =====================================================

                .Line(
                    "apology",
                    "Hautain lowers his sceptre by approximately half an inch. "
                    + "\"An apology offered before punishment. An uncommon luxury.\" "
                    + "He studies you. "
                    + "\"You may retain both hands, provided they remain outside the Treasury.\"")

                    .Choice(
                        "Thank you, Your Majesty.",
                        "recognise")

                    .Choice(
                        "Very generous.",
                        "mercy")

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

                    .Choice(
                        "I'll leave you to it.",
                        "leave")

                .Line(
                    "recognise",
                    "Hautain grows visibly taller. "
                    + "Perhaps by a millimetre. "
                    + "\"Good. The sovereignty of the fountain is formally recognised.\"")

                    .Choice(
                        "Long live King Hautain.",
                        "royalApproval")

                    .Choice(
                        "Tell me about your kingdom.",
                        "kingdom")

                    .Choice(
                        "What about the Mansion?",
                        "mansion")

                    .Choice(
                        "Advice about the front door?",
                        "frontDoor")

                    .Choice(
                        "I'll take my leave.",
                        "leave")

                .Line(
                    "royalApproval",
                    "\"The Crown's opinion of you has improved from "
                    + "'probable thief' to 'adequately civilised visitor.'\"")

                    .Choice(
                        "High praise indeed.",
                        "menu")

                // =====================================================
                // WITS PATH / ROYAL GOLD COIN
                // =====================================================

                .Line(
                    "loyalSubject",
                    "Hautain freezes. "
                    + "\"A loyal... subject?\" "
                    + "He looks behind himself. "
                    + "\"You refer to yourself?\"")

                    .Choice(
                        "I recognise King Hautain's sovereignty.",
                        "royalCoin",
                        effect: p =>
                        {
                            p.Give("Royal Gold Coin");
                            p.SetFlag("GivenRoyalCoin");
                        })

                    .Choice(
                        "It sounded like an easy way to get a coin.",
                        "badFlattery")

                    .Choice(
                        "Never mind.",
                        "menu")

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
                        "Forget it.",
                        "menu")

                .Line(
                    "royalCoin",
                    "Hautain retrieves one coin and presents it solemnly. "
                    + "\"The Crown acknowledges its first new subject in a great many seasons. "
                    + "This is not payment. It is a Royal Gold Coin, "
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
                    + "He lowers his voice. "
                    + "\"And occasionally when they do not.\"")

                    .Choice(
                        "Long live the King!")

                // =====================================================
                // FROG / SIZE INSULTS
                // =====================================================

                .Line(
                    "frogInsult",
                    "\"FROG?! Frog is a BIOLOGICAL DESCRIPTION, not a form of address, "
                    + "you uncultivated drainpipe!\"",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "Fine. Your Majesty the Frog.",
                        "frogAgain")

                    .Choice(
                        "My apologies, Your Majesty.",
                        "apology")

                    .Choice(
                        "You're very sensitive about this.",
                        "sensitive")

                    .Choice(
                        "I'll leave.",
                        "leave")

                .Line(
                    "frogAgain",
                    "\"Yes, I am a frog! "
                    + "You are apparently a primate. "
                    + "Only one of us has managed to make something of it!\"",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "You're six inches tall.",
                        "sizeInsult")

                    .Choice(
                        "All right, that was good.",
                        "menu")

                    .Choice(
                        "Let's fight.",
                        "challenge")

                .Line(
                    "sensitive",
                    "\"I am NOT sensitive! "
                    + "The Crown maintains standards of diplomatic address!\"")

                    .Choice(
                        "Of course, Your Majesty.",
                        "menu")

                    .Choice(
                        "Whatever you say, frog.",
                        "frogAgain")

                .Line(
                    "sizeInsult",
                    "\"The Crown is not small! "
                    + "The rest of the world is unnecessarily large!\"",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "That's one way of looking at it.",
                        "menu")

                    .Choice(
                        "Still six inches.",
                        "sixInches")

                    .Choice(
                        "Let's settle this.",
                        "challenge")

                .Line(
                    "sixInches",
                    "\"I require six inches to govern a kingdom! "
                    + "You require nearly six feet to steal from a fountain!\"",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "Fair point.",
                        "menu")

                    .Choice(
                        "Prove you're a king, then.",
                        "challenge")

                // =====================================================
                // CHALLENGE / COMBAT
                // =====================================================

                .Line(
                    "challenge",
                    "\"You threaten a sovereign on sovereign territory? "
                    + "Even GEESE understand jurisdiction!\"",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "I don't want to fight.",
                        "apology")

                    .Choice(
                        "Do your worst.",
                        "duck")

                    .Choice(
                        "Actually, I'll leave.",
                        "leave")

                .Line(
                    "duck",
                    "\"Do my worst? I once negotiated with a DUCK!\"",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "A duck?",
                        "duckDiplomacy")

                .Line(
                    "duckDiplomacy",
                    "\"You cannot comprehend diplomacy at that level.\"",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "Let's fight.",
                        "fight",
                        startsFight: "AngryHautain")

                    .Choice(
                        "Fair enough. I apologise.",
                        "apology")

                .Line(
                    "fight",
                    "\"THEN APPROACH, YOU OVERGROWN TROUSER RACK! "
                    + "You face the full military strength of the fountain!\"",
                    character: "Assets/Characters/Hautain_Attack.png")

                // =====================================================
                // COIN THEFT
                // =====================================================

                .Line(
                    "grab",
                    "You scoop the gold from the fountain. "
                    + "Hautain stares at your hand in stunned silence. "
                    + "Then the sceptre rises.",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "They're only coins.",
                        "onlyCoins")

                    .Choice(
                        "I need them more than you.",
                        "needCoins")

                    .Choice(
                        "I'll put them back.",
                        "returnInstruction")

                    .Choice(
                        "(Run!)",
                        "grudge")

                    .Choice(
                        "(Fight!)",
                        "fight",
                        startsFight: "AngryHautain")

                .Line(
                    "onlyCoins",
                    "\"ONLY COINS?! "
                    + "You steal from a fountain? "
                    + "Even MAGPIES possess the decency to have feathers!\"",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "I didn't know they mattered.",
                        "coinMeaning")

                    .Choice(
                        "They're mine now.",
                        "grudge")

                    .Choice(
                        "I'll return them.",
                        "returnInstruction")

                .Line(
                    "coinMeaning",
                    "\"They are petitions! Wishes! Promises! "
                    + "Proof that travellers once stopped here and believed something might answer them!\" "
                    + "His voice becomes quieter. "
                    + "\"You have stolen from more than a king.\"")

                    .Choice(
                        "I'll return them.",
                        "returnInstruction")

                    .Choice(
                        "I still need them.",
                        "needCoins")

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
                    "\"Then restore the Treasury through the proper diplomatic channel! "
                    + "Present the coins to the Crown.\"")

                    .Choice(
                        "Understood.")

                .Line(
                    "grudge",
                    "\"GO, THEN! "
                    + "May every chair you sit upon be VERY SLIGHTLY DAMP! "
                    + "The Crown remembers!\" "
                    + "(Royal Grudge gained)",
                    character: "Assets/Characters/Hautain_Attack.png")

                    .Choice(
                        "(Retreat)")

                // =====================================================
                // THIEF RETURNS
                // =====================================================

                .Line(
                    "thiefReturns",
                    "Hautain rises from behind a lily pad. "
                    + "\"The pocket-fingered gutter heron returns. "
                    + "Have you brought the Royal Treasury?\"",
                    character: "Assets/Characters/Hautain_Attack.png")

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

                    .Choice(
                        "Never mind.",
                        "leave")

                .Line(
                    "apologyNoCoins",
                    "\"The Crown acknowledges the apology. "
                    + "The Crown also observes that apologies are considerably lighter than gold.\"")

                    .Choice(
                        "I'll return them.",
                        "returnInstruction")

                    .Choice(
                        "That's all you're getting.",
                        "stillThief")

                .Line(
                    "stillThief",
                    "\"Then keep them! "
                    + "Perhaps holding something of value will constitute a new experience!\"")

                    .Choice(
                        "Goodbye, Your Majesty.")

                // =====================================================
                // RETURNING STOLEN GOLD
                // =====================================================

                .Line(
                    "returned",
                    "Hautain watches the coins sink beneath the water. "
                    + "He counts them twice before lowering his sceptre. "
                    + "\"The Royal Treasury is restored. "
                    + "Returning what was stolen requires more character than taking it.\" "
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
                    "\"Forgiveness is not an eraser. "
                    + "But it may serve as a bridge.\" "
                    + "He extends his sceptre. "
                    + "\"The Crown is prepared to cross halfway.\"")

                    .Choice(
                        "Then I'll meet you there.")

                .Line(
                    "even",
                    "\"The Treasury is even. "
                    + "Trust is a separate account.\"")

                    .Choice(
                        "Fair enough.")

                .Line(
                    "restored",
                    "\"You are restored to the good opinion of the fountain.\"")

                    .Choice(
                        "(Bow and withdraw)")

                // =====================================================
                // RECOGNISED SUBJECT RETURNS
                // Maximum 5.
                // =====================================================

                .Line(
                    "subjectReturns",
                    "\"The recognised subject returns!\" "
                    + "Hautain looks at the empty path behind you. "
                    + "\"Your ceremonial escort is regrettably delayed.\"")

                    .Choice(
                        "Long live King Hautain.",
                        "subjectGreeting")

                    .Choice(
                        "Any royal duties for me?",
                        "duties")

                    .Choice(
                        "Tell me about the Mansion.",
                        "mansion")

                    .Choice(
                        "Advice about the front door?",
                        "frontDoor")

                    .Choice(
                        "I'll be going.",
                        "leave")

                .Line(
                    "subjectGreeting",
                    "\"Acceptable. "
                    + "The realm remains damp but sovereign.\"")

                    .Choice(
                        "Glad to hear it.")

                .Line(
                    "duties",
                    "\"Do not steal from the Crown, discourage herons, "
                    + "and speak favourably of the drainage.\"")

                    .Choice(
                        "I can manage two of those.",
                        "twoDuties")

                    .Choice(
                        "Consider it done.",
                        "menu")

                .Line(
                    "twoDuties",
                    "\"The Crown elects not to inquire.\"")

                    .Choice(
                        "(Bow and leave)")

                // =====================================================
                // THE MANSION
                //
                // Split residents into a submenu so this never exceeds
                // five replies.
                // =====================================================

                .Line(
                    "mansion",
                    "\"The Mansion is a neighbouring power with unstable borders, "
                    + "unreliable servants, suspicious architecture, "
                    + "and APPALLING drainage.\"")

                    .Choice(
                        "Is it dangerous?",
                        "mansionDanger")

                    .Choice(
                        "What do you think of its residents?",
                        "residentMenu",
                        onlyIf: p => p.HasFlag("MetMagnusAndSeverin") || p.HasFlag("MetEbria") || p.HasFlag("MetPip")
                            || (p.HasFlag("MetVelvet") && p.HasFlag("MetSola")) || p.HasFlag("MetGrell"))

                    .Choice(
                        "What about Quazimod?",
                        "quazimod",
                        onlyIf: p => p.HasFlag("MetQuazimodo"))

                    .Choice(
                        "That's all I needed.",
                        "menu")

                .Line(
                    "mansionDanger",
                    "\"Naturally. "
                    + "Enter if you must, but do not allow the place "
                    + "to convince you that it owns you.\"")

                    .Choice(
                        "What does that mean?",
                        "ominous")

                    .Choice(
                        "I'll remember that.",
                        "menu")

                .Line(
                    "ominous",
                    "\"Exactly what was said. "
                    + "The Crown does not repeat ominous warnings. "
                    + "It diminishes them.\"")

                    .Choice(
                        "Understood.",
                        "menu")

                // Residents only appear once the player has met them
                // (MetMagnusAndSeverin, MetEbria, MetPip, MetVelvet + MetSola,
                // MetGrell). Split in two so there is always a way back.
                .Line(
                    "residentMenu",
                    "\"The Crown maintains comprehensive diplomatic opinions.\"")

                    .Choice(
                        "Magnus and Severin?",
                        "chefs",
                        onlyIf: p => p.HasFlag("MetMagnusAndSeverin"))

                    .Choice(
                        "I met Ebria.",
                        "ebria",
                        onlyIf: p => p.HasFlag("MetEbria"))

                    .Choice(
                        "Pip?",
                        "pip",
                        onlyIf: p => p.HasFlag("MetPip"))

                    .Choice(
                        "And the others?",
                        "residentMenu2",
                        onlyIf: p => (p.HasFlag("MetVelvet") && p.HasFlag("MetSola")) || p.HasFlag("MetGrell"))

                    .Choice(
                        "Never mind.",
                        "mansion")

                .Line(
                    "residentMenu2",
                    "\"The Crown has opinions on those as well.\"")

                    .Choice(
                        "Velvet and Sola?",
                        "velvetSola",
                        onlyIf: p => p.HasFlag("MetVelvet") && p.HasFlag("MetSola"))

                    .Choice(
                        "Gréll?",
                        "grell",
                        onlyIf: p => p.HasFlag("MetGrell"))

                    .Choice(
                        "Back to the others.",
                        "residentMenu")

                .Line(
                    "chefs",
                    "\"The enormous kitchen despot and the thin one "
                    + "who looks personally offended by soup.\"")

                    .Choice(
                        "Accurate.",
                        "residentMenu")

                    .Choice(
                        "You don't like them?",
                        "chefOpinion")

                .Line(
                    "chefOpinion",
                    "\"The Crown neither likes nor dislikes foreign powers. "
                    + "It merely notes that Magnus still owes the Treasury three snails.\"")

                    .Choice(
                        "I'll let him know.",
                        "residentMenu")

                // His version of the Lily Pad War. Ebria's version: "I fell in the pond."
                .Line(
                    "ebria",
                    "\"THE BENCH-DWELLING MENACE.\" Hautain very nearly falls off his lily pad. "
                    + "\"That creature once DESCENDED FROM THE HEAVENS upon my Eastern Provinces!\"")

                    .Choice(
                        "She says she fell in the pond.",
                        "ebriaFell")

                    .Choice(
                        "What happened?",
                        "ebriaMore")

                .Line(
                    "ebriaMore",
                    "\"Three hours of savage combat. She seized the Eastern Lily. "
                    + "The Royal Navy - that is, I - repelled her with tremendous valour.\"")

                    .Choice(
                        "And then?",
                        "ebriaFell")

                .Line(
                    "ebriaFell",
                    "\"FELL?\" Hautain swells to twice his size. \"She was DEFEATED. "
                    + "Into the water. Which is technically falling. Tactically.\"")

                    .Choice(
                        "Sounds like Ebria.",
                        "residentMenu")

                .Line(
                    "pip",
                    "\"The deceased librarian? Proper manners. "
                    + "Terrible military potential.\"")

                    .Choice(
                        "That's Pip.",
                        "residentMenu")

                .Line(
                    "velvetSola",
                    "\"Velvet is the eight-legged Department of Unnecessary Infrastructure.\" "
                    + "Hautain lowers his voice. "
                    + "\"The cat is the furry assassin.\"")

                    .Choice(
                        "Sola is just a cat.",
                        "cat")

                    .Choice(
                        "Velvet means well.",
                        "velvetMeansWell")

                    .Choice(
                        "Anything else?",
                        "residentMenu")

                .Line(
                    "cat",
                    "\"I am aware of her propaganda.\"")

                    .Choice(
                        "Of course.",
                        "residentMenu")

                .Line(
                    "velvetMeansWell",
                    "\"Most infrastructure does. "
                    + "It remains infrastructure.\"")

                    .Choice(
                        "Fair enough.",
                        "residentMenu")

                // Gréll deliberately has no joke.
                .Line(
                    "grell",
                    "\"That creature is forbidden from Crown territory.\"")

                    .Choice(
                        "No ridiculous nickname?",
                        "grellSerious")

                    .Choice(
                        "Understood.",
                        "residentMenu")

                .Line(
                    "grellSerious",
                    "\"No.\"")

                    .Choice(
                        "Understood.",
                        "residentMenu")

                .Line(
                    "quazimod",
                    "\"The crooked coat-rack from the house? "
                    + "Excellent manners. Disturbing posture.\"")

                    .Choice(
                        "That sounds like Quazimod.",
                        "menu")

                // =====================================================
                // FRONT DOOR
                // =====================================================

                .Line(
                    "frontDoor",
                    "\"The door responds well to courtesy, "
                    + "poorly to force, and not at all to currency.\" "
                    + "Hautain glances at his treasury. "
                    + "\"A respectable policy.\"")

                    .Choice(
                        "So I should knock politely?",
                        "doorAdvice")

                    .Choice(
                        "Can I bribe it?",
                        "doorBribe")

                    .Choice(
                        "I'll work it out.",
                        "menu")

                .Line(
                    "doorAdvice",
                    "\"You should ALWAYS knock politely. "
                    + "One never knows whether the door is listening.\"")

                    .Choice(
                        "That's not reassuring.",
                        "notReassuring")

                    .Choice(
                        "Good advice.",
                        "menu")

                .Line(
                    "doorBribe",
                    "\"No. "
                    + "And if you attempt to use Crown currency for such vulgar purposes, "
                    + "the Treasury will take a constitutional interest.\"")

                    .Choice(
                        "Noted.",
                        "menu")

                .Line(
                    "notReassuring",
                    "\"Reassurance is not a Crown service.\"")

                    .Choice(
                        "Fair enough.",
                        "menu")

                // =====================================================
                // PLAYER TRIES TO RETURN ROYAL GIFT
                // =====================================================

                .Line(
                    "keepit",
                    "Hautain recoils from the Royal Gold Coin. "
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
                    "\"It means the Crown judged you worthy of trust. "
                    + "Spend it. Treasure it. Lose it foolishly if that is your custom. "
                    + "But do not return it because you have forgotten what it means.\"")

                    .Choice(
                        "I'll keep it.",
                        "keepGift")

                .Line(
                    "keepGift",
                    "\"Good. "
                    + "The matter is closed by Royal Decree.\"")

                    .Choice(
                        "Long live the King!")

                // =====================================================
                // AFTER DEFEAT
                // Maximum 4.
                // =====================================================

                .Line(
                    "beaten",
                    "King Hautain sits behind a torn lily leaf. "
                    + "His sceptre is bent nearly in half, "
                    + "but his crown remains perfectly straight. "
                    + "\"Congratulations. You have defeated a frog. "
                    + "I hope history gives you the statue you deserve.\"",
                    portrait: "Assets/Characters/Portraits/Hautain_Beaten_portrait_framed.png")

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
                    "\"The Crown is bruised, humiliated, and entirely operational.\" "
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
                // EXIT
                // =====================================================

                .Line(
                    "leave",
                    "\"Permission to withdraw is granted.\" "
                    + "Hautain raises his sceptre. "
                    + "\"Mind the eastern stone. It is loose, "
                    + "and the Crown accepts no liability for damaged ankles.\"")

                    .Choice(
                        "(Bow and step away)");

        // =========================================================
        // QUAZIMODO
        // FrontDoor scene, ACT on the front doors
        //
        // ENTRY CONDITIONS:
        // - Player follows the exceptionally polite dialogue route.
        // - Player offers Royal Gold Coin.
        // - Player offers Gold Coins.
        // - Player offers Loose Change from the trunk.
        // - Player defeats Quazimodo in combat.
        //
        // IMPORTANT:
        // Maximum of 5 player replies/choices on any dialogue node.
        // =========================================================

        public static Dialogue QuazimodoFrontDoor { get; } =
            NewDialogue("QuazimodoFrontDoor", "Quazimodo")
                .MetFlag("MetQuazimodo")
                .Portrait("Assets/Characters/Portraits/Quazimodo_portrait_framed.png")

                .StartAt(p =>
                    p.HasFlag("DefeatedQuazimodo")
                        ? "defeated"
                        : p.HasFlag("MetQuazimodo")
                            ? "again"
                            : "start")

                // =====================================================
                // ITEMS OFFERED DURING THE CONVERSATION
                // =====================================================

                // Hautain's royal gift.
                .OnItem(
                    "Royal Gold Coin",
                    "royalCoin",
                    removeItem: true,
                    effect: p =>
                    {
                        p.SetFlag("MetQuazimodo");
                        p.SetFlag("PaidQuazimodo");
                        p.SetFlag("FrontDoorOpen");
                    })

                // The fountain's ordinary gold coins.
                .OnItem(
                    "Gold Coins",
                    "goldCoins",
                    removeItem: true,
                    effect: p =>
                    {
                        p.SetFlag("MetQuazimodo");
                        p.SetFlag("PaidQuazimodo");
                        p.SetFlag("FrontDoorOpen");

                        // If these were stolen from Hautain, the Royal Grudge remains.
                        // Paying Quazimodo does not repair the theft.
                    })

                // Loose change taken from the trunk.
                .OnItem(
                    "Loose Change",
                    "looseChange",
                    removeItem: true,
                    effect: p =>
                    {
                        p.SetFlag("MetQuazimodo");
                        p.SetFlag("PaidQuazimodo");
                        p.SetFlag("FrontDoorOpen");
                    })

                // The party hat from the trunk.
                .OnItem(
                    "Party Hat",
                    "partyHat",
                    removeItem: true,
                    effect: p =>
                    {
                        p.SetFlag("MetQuazimodo");
                        p.SetFlag("QuazimodoPartyHat");
                    })

                .OnOtherItems(
                    "Quazimodo examines the {item} without taking it. "
                    + "\"No, thank you. The Mansion already contains one of those.\"")

                // =====================================================
                // FIRST MEETING
                // 5 choices exactly.
                // =====================================================

                .Line(
                    "start",
                    "The heavy door opens no wider than a handspan. "
                    + "A pale, crooked figure peers through the gap. "
                    + "One eye studies you. The other appears to be inspecting the weather.")

                    .Choice(
                        "Good evening. I apologise for disturbing you.",
                        "politeOpening",
                        effect: p => p.SetFlag("MetQuazimodo"))

                    .Choice(
                        "Hello. May I come inside?",
                        "ordinaryRequest",
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
                        "Open the door or I'll open it myself.",
                        "threat",
                        effect: p => p.SetFlag("MetQuazimodo"))

                // =====================================================
                // EXCEPTIONALLY POLITE ROUTE
                //
                // Merely saying "please" is not enough.
                // The player must remain patient, respectful, and
                // willing to accept Quazimodo's authority at the door.
                // =====================================================

                .Line(
                    "politeOpening",
                    "Quazimodo blinks slowly. "
                    + "\"An apology before a demand. How unconventional.\" "
                    + "The door opens another inch. "
                    + "\"State the nature of your disturbance.\"")

                    .Choice(
                        "I hoped to request shelter, if the household permits it.",
                        "politePurpose")

                    .Choice(
                        "I would like to come inside, please.",
                        "ordinaryRequest")

                    .Choice(
                        "I'm here to see whoever owns this place.",
                        "ownerRequest")

                    .Choice(
                        "Never mind. I shouldn't have disturbed you.",
                        "respectfulWithdrawal")

                .Line(
                    "politePurpose",
                    "\"Shelter.\" "
                    + "Quazimodo looks past you toward the grounds. "
                    + "\"From the night, the weather, or something less accommodating?\"")

                    .Choice(
                        "I don't yet know. That is why I ask rather than presume.",
                        "politeHonesty")

                    .Choice(
                        "From whatever is out here.",
                        "outsideDanger")

                    .Choice(
                        "Does the reason matter?",
                        "impatient")

                    .Choice(
                        "If the answer is no, I will leave peacefully.",
                        "respectfulWithdrawal")

                .Line(
                    "politeHonesty",
                    "Quazimodo studies you for several quiet seconds. "
                    + "\"You admit ignorance, request permission, and have not once attempted "
                    + "to place your foot in the doorway.\"")

                    .Choice(
                        "It is your doorway.",
                        "finalCourtesy")

                    .Choice(
                        "I was raised to respect another person's home.",
                        "finalCourtesy")

                    .Choice(
                        "Would putting my foot there help?",
                        "footInDoor")

                    .Choice(
                        "I can wait while you decide.",
                        "patientWait")

                .Line(
                    "patientWait",
                    "\"Patience as well.\" "
                    + "Quazimodo appears mildly troubled by this accumulation of manners. "
                    + "\"You are making refusal administratively difficult.\"")

                    .Choice(
                        "Take whatever time you require.",
                        "finalCourtesy")

                    .Choice(
                        "Then perhaps you should say yes.",
                        "impatient")

                    .Choice(
                        "I can return another evening.",
                        "respectfulWithdrawal")

                .Line(
                    "finalCourtesy",
                    "Quazimodo looks over his shoulder into the darkness beyond the door. "
                    + "\"The house has admitted far worse people for considerably poorer reasons.\"")

                    .Choice(
                        "Then may I enter?",
                        "politeAdmission",
                        effect: p =>
                        {
                            p.SetFlag("FrontDoorOpen");
                            p.SetFlag("QuazimodoAdmittedPlayer");
                        })

                    .Choice(
                        "Only if you are certain.",
                        "politeAdmission",
                        effect: p =>
                        {
                            p.SetFlag("FrontDoorOpen");
                            p.SetFlag("QuazimodoAdmittedPlayer");
                        })

                    .Choice(
                        "Thank you for considering it.",
                        "politeAdmission",
                        effect: p =>
                        {
                            p.SetFlag("FrontDoorOpen");
                            p.SetFlag("QuazimodoAdmittedPlayer");
                        })

                .Line(
                    "politeAdmission",
                    "Quazimodo releases the door. It groans slowly inward. "
                    + "\"Very well. You may enter.\" "
                    + "He steps aside. "
                    + "\"Please wipe your feet. The floor dislikes being reminded of the outside.\"")

                    .Choice(
                        "Thank you, Quazimodo.")

                    .Choice(
                        "Of course.")

                    .Choice(
                        "The floor dislikes it?",
                        "floorDislikes")

                .Line(
                    "floorDislikes",
                    "\"It sulks.\" "
                    + "Quazimodo glances down at the threshold. "
                    + "\"You will recognise the sound.\"")

                    .Choice(
                        "I'll wipe my feet.")

                // =====================================================
                // ORDINARY REQUEST
                //
                // Polite, but not polite enough to be admitted.
                // The player can move into the deeper courtesy route,
                // offer payment through ITEM, leave, or become rude.
                // =====================================================

                // =====================================================
                // PARTY HAT
                // =====================================================

                .Line(
                    "partyHat",
                    "Quazimodo regards the party hat for a long moment. "
                    + "Then, with enormous dignity, he puts it on and snaps the string under his chin. "
                    + "\"It is not my birthday. It has not been my birthday since 1782.\"",
                    portrait: "Assets/Characters/Portraits/Quazimodo_PartyHat_portrait_framed.png",
                    character: "Assets/Characters/Quazimodo_PartyHat.png")

                    .Choice(
                        "It suits you.",
                        "partyHatSuits")

                    .Choice(
                        "Happy un-birthday!",
                        "partyHatSuits")

                .Line(
                    "partyHatSuits",
                    "\"Nothing suits me. I merely tolerate things.\" "
                    + "The tinsel trembles. "
                    + "\"I shall keep it. For emergencies.\"",
                    portrait: "Assets/Characters/Portraits/Quazimodo_PartyHat_portrait_framed.png",
                    character: "Assets/Characters/Quazimodo_PartyHat.png")

                    .Choice(
                        "About getting inside...",
                        "ordinaryRequest")

                    .Choice(
                        "Goodbye, Quazimodo.")

                .Line(
                    "ordinaryRequest",
                    "\"You may ask.\" "
                    + "Quazimodo waits. "
                    + "\"You have now done so.\"")

                    .Choice(
                        "Would the household kindly grant me shelter?",
                        "politePurpose")

                    .Choice(
                        "Is that a no?",
                        "doorPolicy")

                    .Choice(
                        "What would persuade you?",
                        "paymentHint")

                    .Choice(
                        "Just open the door.",
                        "impatient")

                    .Choice(
                        "Then I'll leave.",
                        "leave")

                .Line(
                    "doorPolicy",
                    "\"It is not yet an answer.\" "
                    + "Quazimodo's fingers remain wrapped around the edge of the door. "
                    + "\"The distinction matters to doors.\"")

                    .Choice(
                        "Then what is your answer?",
                        "entryRefused")

                    .Choice(
                        "May I explain why I need shelter?",
                        "politePurpose")

                    .Choice(
                        "What would change the answer?",
                        "paymentHint")

                    .Choice(
                        "That makes no sense.",
                        "impatient")

                .Line(
                    "entryRefused",
                    "\"At present? No.\"")

                    .Choice(
                        "May I ask why?",
                        "whyRefused")

                    .Choice(
                        "Would a small gratuity help?",
                        "paymentHint")

                    .Choice(
                        "I accept your decision.",
                        "respectfulWithdrawal")

                    .Choice(
                        "Then I'll force my way in.",
                        "threat")

                .Line(
                    "whyRefused",
                    "\"You are unknown, uninvited, and standing on the wrong side of a door "
                    + "that has survived several determined visitors.\"")

                    .Choice(
                        "Then allow me to introduce myself properly.",
                        "politeOpening")

                    .Choice(
                        "What happened to the other visitors?",
                        "previousVisitors")

                    .Choice(
                        "Would payment establish my credentials?",
                        "paymentHint")

                    .Choice(
                        "I'll take my chances.",
                        "leave")

                .Line(
                    "previousVisitors",
                    "\"Some entered.\" "
                    + "Quazimodo pauses. "
                    + "\"Some did not.\"")

                    .Choice(
                        "What happened to those who entered?",
                        "insideWarning")

                    .Choice(
                        "I would still like to request entry.",
                        "politePurpose")

                    .Choice(
                        "Perhaps I'll remain outside.",
                        "leave")

                .Line(
                    "insideWarning",
                    "\"They became residents.\"")

                    .Choice(
                        "That sounds ominous.",
                        "ominousHouse")

                    .Choice(
                        "I only need shelter for the night.",
                        "ordinaryRequest")

                    .Choice(
                        "I've changed my mind.",
                        "leave")

                .Line(
                    "ominousHouse",
                    "\"That was not the intention.\" "
                    + "Quazimodo considers the Mansion behind him. "
                    + "\"It is, however, accurate.\"")

                    .Choice(
                        "May I still come inside?",
                        "ordinaryRequest")

                    .Choice(
                        "I'll remain outside.",
                        "leave")

                // =====================================================
                // PAYMENT / ITEM HINT
                //
                // The actual payment is offered through the ITEM button.
                // =====================================================

                .Line(
                    "paymentHint",
                    "\"The household does not charge admission.\" "
                    + "Quazimodo lowers his voice. "
                    + "\"The household's doorman is less doctrinaire.\"")

                    .Choice(
                        "Are you asking for money?",
                        "askingMoney")

                    .Choice(
                        "I may have something suitable.",
                        "offerItemHint")

                    .Choice(
                        "I won't bribe you.",
                        "noBribe")

                    .Choice(
                        "That's disgraceful.",
                        "offendedByBribe")

                    .Choice(
                        "Never mind.",
                        "leave")

                .Line(
                    "askingMoney",
                    "\"Certainly not.\" "
                    + "Quazimodo extends one long, pale hand through the gap. "
                    + "\"I am allowing you the opportunity to express gratitude in advance.\"")

                    .Choice(
                        "I'll see what I have.",
                        "offerItemHint")

                    .Choice(
                        "That sounds exactly like asking for money.",
                        "bribeSemantics")

                    .Choice(
                        "I would rather earn entry through courtesy.",
                        "politeOpening")

                    .Choice(
                        "No.",
                        "noBribe")

                .Line(
                    "offerItemHint",
                    "\"Use the ITEM button when you have decided how grateful you are.\" "
                    + "Quazimodo keeps his hand extended. "
                    + "\"Coins are traditional. Furniture is inconvenient.\"")

                    .Choice(
                        "Understood.")

                    .Choice(
                        "I'll consider it.",
                        "again")

                    .Choice(
                        "I'd rather ask politely.",
                        "politeOpening")

                .Line(
                    "bribeSemantics",
                    "\"A bribe purchases corruption.\" "
                    + "Quazimodo's hand remains patiently extended. "
                    + "\"A gratuity acknowledges service. The distinction is largely grammatical.\"")

                    .Choice(
                        "I'll see what I have.",
                        "offerItemHint")

                    .Choice(
                        "I don't find that convincing.",
                        "noBribe")

                    .Choice(
                        "Let me try asking properly instead.",
                        "politeOpening")

                .Line(
                    "noBribe",
                    "\"An admirable principle.\" "
                    + "Quazimodo begins closing the door. "
                    + "\"Principles are particularly useful outdoors. They require very little shelter.\"")

                    .Choice(
                        "Wait. Let me ask properly.",
                        "politeOpening")

                    .Choice(
                        "I may reconsider.",
                        "paymentHint")

                    .Choice(
                        "Goodbye.",
                        "leave")

                .Line(
                    "offendedByBribe",
                    "\"Disgrace requires an audience.\" "
                    + "Quazimodo looks at the empty grounds. "
                    + "\"At present, this is merely a private arrangement.\"")

                    .Choice(
                        "I'll ask properly instead.",
                        "politeOpening")

                    .Choice(
                        "You're unbelievable.",
                        "impatient")

                    .Choice(
                        "Goodbye.",
                        "leave")

                // =====================================================
                // ROYAL GOLD COIN
                // =====================================================

                .Line(
                    "royalCoin",
                    "Quazimodo takes the Royal Gold Coin between two long fingers. "
                    + "He turns it toward the dim light and studies the tiny crowned frog upon it. "
                    + "\"Royal currency.\"",
                    portrait: "Assets/Characters/Portraits/Quazimodo_Bribed_portrait_framed.png")

                    .Choice(
                        "King Hautain gave it to me.",
                        "hautainGift",
                        onlyIf: p => p.HasFlag("MetHautain"))

                    .Choice(
                        "Will that cover admission?",
                        "paidAdmission")

                    .Choice(
                        "On second thought, give it back.",
                        "tooLateRoyal")

                .Line(
                    "hautainGift",
                    "\"Then either you possess exceptional manners or His Majesty has suffered "
                    + "a sudden decline in judgement.\" "
                    + "Quazimodo slips the coin into his coat. "
                    + "\"Both possibilities qualify you for entry.\"")

                    .Choice(
                        "Thank you.",
                        "paidAdmission")

                    .Choice(
                        "Please treat it respectfully.",
                        "paidAdmission")

                .Line(
                    "tooLateRoyal",
                    "\"I am afraid it has already entered the household accounts.\" "
                    + "Quazimodo waits. "
                    + "\"The household accounts are my left pocket.\"")

                    .Choice(
                        "Fine. Open the door.",
                        "paidAdmission")

                .Line(
                    "paidAdmission",
                    "The coin disappears into Quazimodo's coat. "
                    + "The door swings inward. "
                    + "\"Your gratitude has been received. "
                    + "The Mansion is prepared to tolerate your presence.\"")

                    .Choice(
                        "That's comforting.")

                    .Choice(
                        "Thank you.")

                    .Choice(
                        "I preferred the word 'welcome'.")

                // =====================================================
                // ORDINARY GOLD COINS
                // =====================================================

                .Line(
                    "goldCoins",
                    "Quazimodo weighs the old gold coins in his palm. "
                    + "\"A generous expression of gratitude.\" "
                    + "He studies the water dripping from them. "
                    + "\"And a recently submerged one.\"",
                    portrait: "Assets/Characters/Portraits/Quazimodo_Bribed_portrait_framed.png")

                    .Choice(
                        "They came from the fountain.",
                        "fountainCoins")

                    .Choice(
                        "Does it matter where they came from?",
                        "coinOrigin")

                    .Choice(
                        "Will you let me in?",
                        "goldAdmission")

                .Line(
                    "fountainCoins",
                    "\"Then I assume King Hautain approved the transfer.\" "
                    + "Quazimodo looks at you for a long moment. "
                    + "\"No? I thought not.\"")

                    .Choice(
                        "Are you going to return them?",
                        "returnToHautain")

                    .Choice(
                        "Are you going to let me in?",
                        "goldAdmission")

                    .Choice(
                        "Give them back.",
                        "tooLateGold")

                .Line(
                    "coinOrigin",
                    "\"Only if the former owner is armed, royal, or amphibious.\"")

                    .Choice(
                        "Possibly all three.",
                        "fountainCoins")

                    .Choice(
                        "Just open the door.",
                        "goldAdmission")

                .Line(
                    "returnToHautain",
                    "\"Probably not.\" "
                    + "Quazimodo places the coins inside his coat. "
                    + "\"Diplomatic relations between the Mansion and the fountain are already strained.\"")

                    .Choice(
                        "Then at least let me in.",
                        "goldAdmission")

                .Line(
                    "tooLateGold",
                    "\"They have already been entered into the accounts.\" "
                    + "Quazimodo pats his left pocket. "
                    + "\"Reversing the transaction would require paperwork.\"")

                    .Choice(
                        "Fine. Open the door.",
                        "goldAdmission")

                .Line(
                    "goldAdmission",
                    "Quazimodo steps back and pulls the door open. "
                    + "\"You may enter.\" "
                    + "He glances toward the fountain. "
                    + "\"If a small monarch asks, we have never met.\"")

                    .Choice(
                        "Understood.")

                    .Choice(
                        "I make no promises.")

                    .Choice(
                        "Long live the King.")

                // =====================================================
                // LOOSE CHANGE FROM THE TRUNK
                // =====================================================

                .Line(
                    "looseChange",
                    "Quazimodo counts the loose change methodically. "
                    + "Several coins appear to have spent years beneath a seat cushion. "
                    + "\"An eclectic currency.\"")

                    .Choice(
                        "It's everything I had.",
                        "allTheChange")

                    .Choice(
                        "Is it enough?",
                        "changeAdmission")

                    .Choice(
                        "There may be crumbs mixed in.",
                        "crumbs")

                .Line(
                    "allTheChange",
                    "\"Then the gesture exceeds the amount.\" "
                    + "Quazimodo pockets the coins. "
                    + "\"An uncommon financial principle.\"")

                    .Choice(
                        "May I come inside now?",
                        "changeAdmission")

                .Line(
                    "crumbs",
                    "Quazimodo removes a small crumb from the pile and examines it. "
                    + "\"The kitchen will assess its value separately.\"")

                    .Choice(
                        "Of course it will.",
                        "changeAdmission")

                .Line(
                    "changeAdmission",
                    "Quazimodo deposits the change somewhere deep within his coat. "
                    + "The sound of coins continues for longer than seems physically reasonable. "
                    + "He opens the door. "
                    + "\"Payment is adequate. Barely.\"")

                    .Choice(
                        "Barely is enough.")

                    .Choice(
                        "Thank you.")

                    .Choice(
                        "How many pockets do you have?")

                // =====================================================
                // BUTLER BRANCH
                // =====================================================

                .Line(
                    "butler",
                    "\"Butler. Porter. Doorman. Undertaker of small administrative duties. "
                    + "Titles multiply when staff do not.\"")

                    .Choice(
                        "Then may I request entry properly?",
                        "politeOpening")

                    .Choice(
                        "Undertaker?",
                        "undertaker")

                    .Choice(
                        "Doorman? Then open the door.",
                        "impatient")

                    .Choice(
                        "What happened to the rest of the staff?",
                        "missingStaff")

                    .Choice(
                        "Never mind.",
                        "leave")

                .Line(
                    "undertaker",
                    "\"Only administratively.\" "
                    + "Quazimodo pauses. "
                    + "\"Usually.\"")

                    .Choice(
                        "Usually?",
                        "usuallyUndertaker")

                    .Choice(
                        "I would still like to come inside.",
                        "ordinaryRequest")

                    .Choice(
                        "Perhaps not.",
                        "leave")

                .Line(
                    "usuallyUndertaker",
                    "\"You are asking a great many questions for someone standing outside.\"")

                    .Choice(
                        "Then let me ask from inside.",
                        "ordinaryRequest")

                    .Choice(
                        "Fair point.",
                        "butler")

                .Line(
                    "missingStaff",
                    "\"Some left. Some died. Some did both, though not necessarily in that order.\"")

                    .Choice(
                        "That's not reassuring.",
                        "ominousHouse")

                    .Choice(
                        "May I come inside?",
                        "ordinaryRequest")

                    .Choice(
                        "I think I'll leave.",
                        "leave")

                // =====================================================
                // RUDE BRANCH
                // =====================================================

                .Line(
                    "rude",
                    "Quazimodo studies you in silence. "
                    + "\"One might ask precisely the same of you.\"")

                    .Choice(
                        "Fair point. I apologise.",
                        "rudeApology")

                    .Choice(
                        "I didn't mean to offend you.",
                        "rudeApology")

                    .Choice(
                        "No, seriously. What happened?",
                        "rudeAgain")

                    .Choice(
                        "Just let me inside.",
                        "impatient")

                    .Choice(
                        "Never mind.",
                        "leave")

                .Line(
                    "rudeApology",
                    "\"An apology is an admirable beginning.\" "
                    + "Quazimodo's hand remains on the door. "
                    + "\"It is not, by itself, an invitation.\"")

                    .Choice(
                        "May I begin again properly?",
                        "politeOpening")

                    .Choice(
                        "What would earn an invitation?",
                        "paymentHint")

                    .Choice(
                        "Then forget it.",
                        "leave")

                .Line(
                    "rudeAgain",
                    "\"Time, gravity, inheritance, and several occupational incidents.\"")

                    .Choice(
                        "Occupational incidents?",
                        "occupationalIncidents")

                    .Choice(
                        "I deserved that answer.",
                        "rudeApology")

                    .Choice(
                        "You're avoiding the question.",
                        "impatient")

                .Line(
                    "occupationalIncidents",
                    "\"The Mansion employs unusually heavy furniture.\"")

                    .Choice(
                        "I'll stop asking.",
                        "rudeApology")

                    .Choice(
                        "That cannot be the whole story.",
                        "impatient")

                // =====================================================
                // IMPATIENCE
                // =====================================================

                .Line(
                    "impatient",
                    "\"Impatience is not a key.\" "
                    + "Quazimodo begins closing the door. "
                    + "\"It does, however, resemble one when viewed by an impatient person.\"")

                    .Choice(
                        "Wait. I apologise.",
                        "rudeApology")

                    .Choice(
                        "What would persuade you?",
                        "paymentHint")

                    .Choice(
                        "Open it now.",
                        "threat")

                    .Choice(
                        "Fine. Goodbye.",
                        "leave")

                .Line(
                    "footInDoor",
                    "\"It would help the door close upon your foot.\"")

                    .Choice(
                        "I won't do that.",
                        "politeHonesty")

                    .Choice(
                        "Maybe I'll try it.",
                        "threat")

                .Line(
                    "ownerRequest",
                    "\"The Mansion's ownership is a matter of considerable disagreement.\"")

                    .Choice(
                        "Then may I speak to whoever is in charge?",
                        "inCharge")

                    .Choice(
                        "May I request shelter instead?",
                        "politePurpose")

                    .Choice(
                        "That sounds complicated.",
                        "ordinaryRequest")

                .Line(
                    "inCharge",
                    "\"Everyone believes someone else is in charge.\" "
                    + "Quazimodo looks into the darkness behind him. "
                    + "\"The arrangement has proved surprisingly durable.\"")

                    .Choice(
                        "Then may I come inside?",
                        "ordinaryRequest")

                    .Choice(
                        "I'd rather remain outside.",
                        "leave")

                .Line(
                    "outsideDanger",
                    "\"A broad category.\" "
                    + "Quazimodo surveys the dark grounds. "
                    + "\"Unfortunately, the interior falls within it.\"")

                    .Choice(
                        "I would still like to request shelter.",
                        "politeHonesty")

                    .Choice(
                        "That isn't encouraging.",
                        "ordinaryRequest")

                    .Choice(
                        "I'll take my chances outside.",
                        "leave")

                // =====================================================
                // THREAT / FIGHT ROUTE
                // =====================================================

                .Line(
                    "threat",
                    "The door opens slightly wider. "
                    + "Quazimodo steps into the gap, revealing considerably more height "
                    + "and considerably less concern than expected. "
                    + "\"You may attempt it.\"",
                    portrait: "Assets/Characters/Portraits/Quazimodo_portrait_framed.png")

                    .Choice(
                        "Move aside.",
                        "fightWarning")

                    .Choice(
                        "I don't want trouble.",
                        "backDown")

                    .Choice(
                        "Perhaps we can arrange payment.",
                        "paymentHint")

                    .Choice(
                        "I apologise. Let me start again.",
                        "rudeApology")

                    .Choice(
                        "(Attack Quazimodo)",
                        "fight",
                        startsFight: "QuazimodoDoorFight")

                .Line(
                    "fightWarning",
                    "\"No.\" "
                    + "Quazimodo rolls one shoulder. Something in it clicks several times. "
                    + "\"You may now decide whether the conversation has ended "
                    + "or merely become physical.\"")

                    .Choice(
                        "(Attack Quazimodo)",
                        "fight",
                        startsFight: "QuazimodoDoorFight")

                    .Choice(
                        "I was bluffing.",
                        "backDown")

                    .Choice(
                        "Let's discuss payment.",
                        "paymentHint")

                    .Choice(
                        "I apologise.",
                        "rudeApology")

                .Line(
                    "backDown",
                    "\"A wise revision.\" "
                    + "Quazimodo returns one hand to the door. "
                    + "\"Wisdom arriving late is still preferable to a physician arriving later.\"")

                    .Choice(
                        "May I begin again?",
                        "politeOpening")

                    .Choice(
                        "What would persuade you?",
                        "paymentHint")

                    .Choice(
                        "I'll leave.",
                        "leave")

                .Line(
                    "fight",
                    "\"Very well.\" "
                    + "Quazimodo opens the door fully and steps out onto the landing. "
                    + "\"Please avoid bleeding on the threshold. It encourages the wood.\"",
                    portrait: "Assets/Characters/Portraits/Quazimodo_portrait_framed.png")

                // =====================================================
                // AFTER QUAZIMODO IS DEFEATED
                //
                // Assumption:
                // The fight sets DefeatedQuazimodo.
                // The admission choice then opens the doors.
                // =====================================================

                .Line(
                    "defeated",
                    "Quazimodo leans heavily against the partly opened door. "
                    + "His coat is torn, but his professional disapproval remains immaculate. "
                    + "\"You have established that you can enter the Mansion.\"",
                    portrait: "Assets/Characters/Portraits/Quazimodo_Beaten_portrait_framed.png")

                    .Choice(
                        "Then let me in.",
                        "forcedAdmission",
                        effect: p =>
                        {
                            p.SetFlag("MetQuazimodo");
                            p.SetFlag("FrontDoorOpen");
                            p.SetFlag("ForcedEntry");
                        })

                    .Choice(
                        "I'm sorry.",
                        "defeatedApology")

                    .Choice(
                        "You could have just opened the door.",
                        "blameQuazimodo")

                    .Choice(
                        "Are you all right?",
                        "defeatedConcern")

                    .Choice(
                        "I'll leave instead.",
                        "leave")

                .Line(
                    "defeatedApology",
                    "\"Your regret is noted.\" "
                    + "Quazimodo straightens his bow tie. "
                    + "\"It would have been more useful before the furniture became involved.\"")

                    .Choice(
                        "May I still enter?",
                        "forcedAdmission",
                        effect: p =>
                        {
                            p.SetFlag("MetQuazimodo");
                            p.SetFlag("FrontDoorOpen");
                            p.SetFlag("ForcedEntry");
                        })

                    .Choice(
                        "I should leave.",
                        "leave")

                .Line(
                    "blameQuazimodo",
                    "\"You could have said please with greater conviction.\"")

                    .Choice(
                        "Fair point.",
                        "forcedAdmission",
                        effect: p =>
                        {
                            p.SetFlag("MetQuazimodo");
                            p.SetFlag("FrontDoorOpen");
                            p.SetFlag("ForcedEntry");
                        })

                    .Choice(
                        "Still blaming you.",
                        "forcedAdmission",
                        effect: p =>
                        {
                            p.SetFlag("MetQuazimodo");
                            p.SetFlag("FrontDoorOpen");
                            p.SetFlag("ForcedEntry");
                        })

                .Line(
                    "defeatedConcern",
                    "\"I have endured worse visitors.\" "
                    + "Quazimodo looks into the Mansion. "
                    + "\"Most were already inside.\"")

                    .Choice(
                        "May I enter?",
                        "forcedAdmission",
                        effect: p =>
                        {
                            p.SetFlag("MetQuazimodo");
                            p.SetFlag("FrontDoorOpen");
                            p.SetFlag("ForcedEntry");
                        })

                    .Choice(
                        "I'll give you a moment.",
                        "leave")

                .Line(
                    "forcedAdmission",
                    "Quazimodo steps away from the threshold. "
                    + "\"Enter, then.\" "
                    + "His gaze follows you toward the darkness. "
                    + "\"You have opened the door. That does not mean the house has welcomed you.\"")

                    .Choice(
                        "I'll take my chances.")

                    .Choice(
                        "Understood.")

                // =====================================================
                // RESPECTFUL WITHDRAWAL
                //
                // This does not open the door, but Quazimodo remembers
                // that the player behaved respectfully.
                // =====================================================

                .Line(
                    "respectfulWithdrawal",
                    "Quazimodo pauses before closing the door. "
                    + "\"You asked, received no answer, and did not immediately resort to violence.\" "
                    + "He inclines his head. "
                    + "\"The standard is low. You have nevertheless exceeded it.\"")

                    .Choice(
                        "Thank you.",
                        effect: p => p.SetFlag("QuazimodoRespectsPlayer"))

                    .Choice(
                        "Perhaps I'll return later.",
                        effect: p => p.SetFlag("QuazimodoRespectsPlayer"))

                // =====================================================
                // RETURN VISIT
                //
                // Maximum 5 choices.
                // Still no automatic admission.
                // =====================================================

                .Line(
                    "again",
                    "The door opens a fraction. "
                    + "Quazimodo peers through the gap. "
                    + "\"You have returned. "
                    + "The outside has apparently failed to improve.\"")

                    .Choice(
                        "May I request entry properly?",
                        "politeOpening")

                    .Choice(
                        "What would persuade you to let me in?",
                        "paymentHint")

                    .Choice(
                        "I have something to offer.",
                        "offerItemHint")

                    .Choice(
                        "Open the door.",
                        "impatient")

                    .Choice(
                        "Not yet. Goodbye.",
                        "leave")

                // =====================================================
                // LEAVE
                // =====================================================

                .Line(
                    "leave",
                    "\"A sensible decision.\" "
                    + "Quazimodo begins closing the door. "
                    + "\"Those rarely survive prolonged contact with this house.\"")

                    .Choice(
                        "Goodbye.");
        // =========================================================
        // EBRIA, the drunk fairy
        // Found beneath the stone bench in BenchCloseup.
        //
        // FIRST ENCOUNTER RULES:
        // - Ebria is funny, sharp, drunk and occasionally mean.
        // - She is NEVER stupid because she is drunk.
        // - Drinking is avoidance, not carefree hedonism.
        // - She does not willingly discuss her sister.
        // - References to "we", memory and identity are deliberate cracks.
        // - Do NOT reveal: sister, twin, mirror, violet ribbon or guilt yet.
        // - "Just Ebria" is deliberate foreshadowing of her identity arc.
        // =========================================================

        public static Dialogue Ebria { get; } =
            NewDialogue("Ebria", "Ebria")
                .MetFlag("MetEbria")
                .Portrait("Assets/Characters/Portraits/Ebria_portrait_framed.png")
                .DimScene(false)
                .StartAt(p => p.HasFlag("MetEbria") ? "again" : "start")

                .OnItem(
                    "Corkscrew",
                    "corkscrew",
                    removeItem: true,
                    effect: p => p.SetFlag("GaveEbriaCorkscrew"))

                .OnItem("Empty Bottle", "emptyBottle")

                // The mirror from the well. She refuses it - more firmly each time.
                // It isn't hers, so it is never taken.
                .OnItem("Cracked Hand Mirror", "mirror",
                        effect: p => p.SetFlag("ShowedEbriaMirror"),
                        onlyIf: p => !p.HasFlag("ShowedEbriaMirror"))
                .OnItem("Cracked Hand Mirror", "mirrorAgain",
                        effect: p => p.SetFlag("ShowedEbriaMirrorTwice"),
                        onlyIf: p => !p.HasFlag("ShowedEbriaMirrorTwice"))
                .OnItem("Cracked Hand Mirror", "mirrorRefused")

                .OnOtherItems(
                    "Ebria studies the {item} with grave concentration. " +
                    "\"Can I drink it? No? Then you've misunderstood our relationship.\"")

            // ---------------------------------------------------------
            // THE MIRROR (offered with ITEM)
            // ---------------------------------------------------------

            .Line("mirror",
                "Ebria looks at the mirror. The drunken smile disappears. \"No.\"")
                .Choice("No?", "mirrorNo")

            .Line("mirrorNo",
                "\"I said no.\"")
                .Choice("It's a mirror.", "mirrorSee")
                .Choice("(Put it away)", "menu")

            .Line("mirrorSee",
                "\"I can see that.\" She doesn't look at it again.")
                .Choice("(Put it away)", "menu")

            .Line("mirrorAgain",
                "\"Put that away.\"")
                .Choice("(Put it away)", "menu")

            .Line("mirrorRefused",
                "Ebria will not look at it.")
                .Choice("(Put it away)", "menu")

            // ---------------------------------------------------------
            // FIRST MEETING
            // ---------------------------------------------------------

            .Line(
                "start",
                "\"Ow.\" " +
                "A tiny voice comes from beneath the bench. " +
                "A fairy peers out, bottle clasped against her chest. " +
                "\"You're supposed to sit on the top part.\" " +
                "She squints up at you. " +
                "\"Who are you? No, wait. Let me guess. Lost, dead, or annoying?\"")
                .Choice("What are you doing under the bench?", "under")
                .Choice("What's in the bottle?", "bottle")
                .Choice("Who are you?", "who")
                .Choice("Are you all right?", "alright")
                .Choice("I'll leave you alone.")

            // ---------------------------------------------------------
            // MAIN MENU
            // ---------------------------------------------------------

            .Line(
                "menu",
                "Ebria takes another drink. " +
                "\"Well? You've already found me. Might as well make it inconvenient.\"")
                .Choice("Why are you hiding under here?", "under")
                .Choice("Why do you drink so much?", "whyDrink")
                .Choice("Do you live in the mansion?", "mansion")
                .Choice(
                    "King Hautain mentioned you.",
                    "hautain",
                    onlyIf: p => p.HasFlag("MetHautain"))
                .Choice("That's enough questions.")

            // ---------------------------------------------------------
            // RETURN VISIT
            // ---------------------------------------------------------

            .Line(
                "again",
                "\"Oh. You.\" " +
                "Ebria peers around you suspiciously. " +
                "\"Did you bring wine? No? Then this friendship remains terribly one-sided.\"")
                .SameChoicesAs("menu")

            // ---------------------------------------------------------
            // WHO ARE YOU?
            // ---------------------------------------------------------

            .Line(
                "who",
                "\"Ebria.\" " +
                "She pauses as though expecting something else to follow. " +
                "\"Just Ebria.\" " +
                "Another pause. She drinks. " +
                "\"That's plenty.\"")
                .Choice("Just Ebria?", "justEbria")
                .Choice("Nice to meet you.", "menu")

            .Line(
                "justEbria",
                "For a moment she doesn't answer. " +
                "\"Were you expecting a title?\" " +
                "She lifts the bottle. " +
                "\"Fine. Ebria the Damp. Ebria Beneath-the-Bench. " +
                "First of Her Name, hopefully last.\"")
                .Choice("I'll stick with Ebria.", "menu")

            // ---------------------------------------------------------
            // UNDER THE BENCH
            // ---------------------------------------------------------

            .Line(
                "under",
                "\"It's dry. Mostly quiet. " +
                "Nobody expects anything from things found under benches.\" " +
                "She pats the stone beside her. " +
                "\"And this one doesn't ask stupid questions.\"")
                .Choice("I just asked a stupid question, didn't I?", "stupidQuestion")
                .Choice("Are you hiding from someone?", "hiding")
                .Choice("Fair enough.", "menu")

            .Line(
                "stupidQuestion",
                "\"Oh, several.\" " +
                "She raises the bottle toward you. " +
                "\"But you're new. There's still time to improve.\"")
                .Choice("Encouraging.", "menu")

            .Line(
                "hiding",
                "\"Hiding? No.\" " +
                "She considers this while taking a drink. " +
                "\"Hiding implies somebody is looking for you.\" " +
                "She stares at the bottle for a moment. " +
                "\"I'm sheltering.\"")
                .Choice("From what?", "sheltering")
                .Choice("Right. Sheltering.", "menu")

            .Line(
                "sheltering",
                "\"Rain. Conversation. Consequences. Quazimodo when he's carrying a broom.\" " +
                "She nods solemnly. " +
                "\"Mostly the broom.\"")
                .Choice("Sounds dangerous.", "menu")

            // ---------------------------------------------------------
            // THE BOTTLE
            // ---------------------------------------------------------

            .Line(
                "bottle",
                "Ebria immediately pulls the bottle closer. " +
                "\"Mine.\" " +
                "She looks at the label. " +
                "\"Elderflower, apparently. Or furniture polish. " +
                "The writing keeps wandering about.\"")
                .Choice("Do you even know what you're drinking?", "knowDrink")
                .Choice("I wasn't going to take it.", "menu")

            .Line(
                "knowDrink",
                "\"Of course I do.\" " +
                "She takes a deliberate sip. " +
                "\"I'm drinking this.\" " +
                "She taps the bottle. " +
                "\"What this is remains a secondary concern.\"")
                .Choice("That seems sensible.", "menu")

            // ---------------------------------------------------------
            // WHY DO YOU DRINK?
            //
            // This gets dangerously close to the real reason.
            // Ebria allows one truthful answer, then immediately deflects.
            // ---------------------------------------------------------

            .Line(
                "whyDrink",
                "\"Because being drunk makes people ask fewer questions.\" " +
                "She looks directly at you. " +
                "\"Usually.\"")
                .Choice("I'm serious.", "seriousDrink")
                .Choice("Point taken.", "menu")

            .Line(
                "seriousDrink",
                "The smile slips from her voice for just a moment. " +
                "\"So am I.\" " +
                "She takes another drink. " +
                "\"Some things are louder when you're sober.\"")
                .Choice("What things?", "whatThings")
                .Choice("I won't pry.", "menu")

            .Line(
                "whatThings",
                "\"Memories.\" " +
                "The answer comes too quickly. " +
                "Ebria looks down at the bottle. " +
                "\"Also frogs. Horrible creatures. Never stop talking.\"")
                .Choice("You changed the subject.", "changedSubject")
                .Choice("Frogs. Right.", "menu")

            .Line(
                "changedSubject",
                "\"Yes.\" " +
                "She drinks. " +
                "\"And beautifully, I thought.\"")
                .Choice("All right.", "menu")

            // ---------------------------------------------------------
            // ARE YOU ALL RIGHT?
            //
            // Adds some of Ebria's established mean-drunk quality.
            // She is observant and capable of turning a question back
            // against whoever asked it.
            // ---------------------------------------------------------

            .Line(
                "alright",
                "\"Are you?\" " +
                "Ebria looks you over. " +
                "\"You're wandering around a dead garden talking to things under furniture. " +
                "I'm drinking. At least one of us has a plan.\"")
                .Choice("I'll take that as a no.", "notAlright")
                .Choice("I was only asking.", "onlyAsking")
                .Choice("Fair point.", "menu")

            .Line(
                "notAlright",
                "\"Take it however you like.\" " +
                "She settles farther beneath the bench. " +
                "\"Everyone else does.\"")
                .Choice("I'll leave it alone.", "menu")

            .Line(
                "onlyAsking",
                "\"People usually are.\" " +
                "She takes a drink. " +
                "\"They seldom want the answer nearly as much as they want credit for asking.\"")
                .Choice("That was a little harsh.", "harsh")
                .Choice("Fine. I won't ask again.", "menu")

            .Line(
                "harsh",
                "Ebria looks away. " +
                "\"Yes.\" " +
                "She takes another drink. " +
                "\"I'm very talented.\"")
                .Choice("I noticed.", "menu")

            // ---------------------------------------------------------
            // THE MANSION
            //
            // "We did" is the first accidental indication that there
            // used to be another person central to Ebria's existence.
            // No relationship is revealed here.
            // ---------------------------------------------------------

            .Line(
                "mansion",
                "\"Live there?\" " +
                "She glances toward the mansion. " +
                "\"I used to spend more time inside.\" " +
                "A pause. " +
                "\"We did.\" " +
                "Ebria blinks, then immediately drinks. " +
                "\"Anyway. Too many stairs.\"")
                .Choice("We?", "we")
                .Choice("What happened?", "whatHappened")
                .Choice("Too many stairs. Understood.", "menu")

            .Line(
                "we",
                "\"Did I say we?\" " +
                "She stares into the bottle. " +
                "\"Clearly this isn't working hard enough.\"")
                .Choice("Who did you mean?", "whoWe")
                .Choice("Forget I asked.", "menu")

            .Line(
                "whoWe",
                "Ebria goes very still. " +
                "\"Someone who should be.\" " +
                "The bottle rises between you. " +
                "\"Congratulations. You've reached the end of that conversation.\"")
                .Choice("All right.", "menu")

            .Line(
                "whatHappened",
                "\"Life.\" " +
                "She takes another drink. " +
                "\"Death.\" " +
                "Another. " +
                "\"Architecture.\" " +
                "She points vaguely toward the mansion. " +
                "\"One of those. Pick whichever answer makes you go away.\"")
                .Choice("Architecture it is.", "menu")

            // ---------------------------------------------------------
            // HAUTAIN
            // ---------------------------------------------------------

            .Line(
                "hautain",
                "Ebria groans. " +
                "\"Oh, not the frog.\" " +
                "She points the bottle at you. " +
                "\"Whatever he told you, remember that he is four inches tall " +
                "and calls himself a king.\"")
                .Choice("He said you invaded his lily pad.", "lilyPad")
                .Choice("You don't like him?", "likeHautain")
                .Choice("I'll keep that in mind.", "menu")

            .Line(
                "lilyPad",
                "\"Invaded.\" " +
                "Ebria snorts. " +
                "\"I fell in the pond.\" " +
                "She gestures indignantly. " +
                "\"Apparently collapsing face-first onto sovereign territory " +
                "constitutes an act of war.\"")
                .Choice("Did you apologize?", "apologizeHautain")
                .Choice("Sounds like Hautain.", "menu")

            .Line(
                "apologizeHautain",
                "\"Eventually.\" " +
                "She takes a drink. " +
                "\"To the lily pad.\"")
                .Choice("Naturally.", "menu")

            .Line(
                "likeHautain",
                "\"I adore him.\" " +
                "Ebria takes a long drink. " +
                "\"Preferably from a great distance, in another pond, " +
                "on another continent.\"")
                .Choice("I'll tell him you said that.", "tellHautain")
                .Choice("Understood.", "menu")

            .Line(
                "tellHautain",
                "\"Do. He'll declare war again.\" " +
                "She sighs. " +
                "\"It gives him something to do.\"")
                .Choice("Fair enough.", "menu")

            // ---------------------------------------------------------
            // CORKSCREW
            //
            // A tiny relationship beat:
            // practical kindness makes Ebria uncomfortable because it
            // creates the possibility of actually liking the player.
            // ---------------------------------------------------------

            .Line(
                "corkscrew",
                "Ebria stares at the corkscrew. " +
                "\"Oh.\" " +
                "For once, she seems completely lost for words. " +
                "She takes it carefully. " +
                "\"Do you have any idea how many corks I've opened with my teeth?\"")
                .Choice("I'm guessing several.", "corkscrewSeveral")
                .Choice("Consider it a gift.", "corkscrewGift")

            .Line(
                "corkscrewSeveral",
                "\"Thirty-seven.\" " +
                "She runs her thumb reverently over the corkscrew. " +
                "\"Thirty-eight if you count the one I swallowed.\"")
                .Choice("I wasn't going to.", "menu")

            .Line(
                "corkscrewGift",
                "Ebria looks at you, then quickly back at the corkscrew. " +
                "\"Well. That's inconvenient.\"")
                .Choice("What is?", "giftInconvenient")

            .Line(
                "giftInconvenient",
                "\"Now I have to like you a little.\" " +
                "She points the corkscrew at you. " +
                "\"Don't make a habit of it.\"")
                .Choice("No promises.", "menu")

            // ---------------------------------------------------------
            // EMPTY BOTTLE
            //
            // The "empty things" line is intentionally ambiguous here.
            // It gains additional meaning once Ebria's history is known.
            // ---------------------------------------------------------

            .Line(
                "emptyBottle",
                "Ebria takes the bottle with sudden interest. " +
                "She turns it upside down. Nothing. " +
                "She looks at you. Then at the bottle. Then back at you. " +
                "\"You brought a drunk fairy an empty bottle.\"")
                .Choice("I thought you might want it.", "wantEmptyBottle")
                .Choice("In hindsight, that was cruel.", "emptyCruel")

            .Line(
                "wantEmptyBottle",
                "\"For what? Memories?\" " +
                "She hands it back. " +
                "\"I've already got too many empty things.\"")
                .Choice("...Right.", "menu")

            .Line(
                "emptyCruel",
                "\"Good.\" " +
                "She nods approvingly. " +
                "\"Self-awareness. Rare around here.\"")
                .Choice("Sorry.", "menu");

        // =========================================================
        // THE PORTRAIT LEDGER
        // Read by right-clicking the Portrait Ledger in the inventory
        // (from the hidden compartment in the Portrait Gallery). Pages are
        // choices. Foreshadowing, not answers.
        // =========================================================

        public static Dialogue PortraitLedger { get; } =
            NewDialogue("PortraitLedger", "Portrait Ledger")

                .Line(
                    "cover",
                    "PORTRAIT COMMISSIONS. A handwritten record of portraits commissioned for the Mansion. " +
                    "Several pages have been removed. Several names have been crossed out. One appears to have been written repeatedly.")
                    .Choice(
                        "(Turn to the repeated name.)",
                        "mirelle1798")
                    .Choice(
                        "(Turn to the Gate Guardians.)",
                        "gate",
                        effect: p =>
                        {
                            p.SetFlag("ReadGateGuardianLedgerEntry");
                            p.SetFlag("LearnedGargoyleName");
                        })
                    .Choice(
                        "(Turn to the Family Portrait.)",
                        "family",
                        effect: p => p.SetFlag("ReadFamilyPortraitLedgerEntry"))
                    .Choice(
                        "(Turn to the later pages.)",
                        "vesper")
                    .Choice(
                        "(Close the ledger.)")

                .Line(
                    "contents",
                    "You turn back to the front of the ledger. PORTRAIT COMMISSIONS.")
                    .SameChoicesAs("cover")

                // ---- Mirélle ----

                .Line(
                    "mirelle1798",
                    "1798. Portrait of Mirélle. Rejected by subject.")
                    .Choice(
                        "(Turn the page.)",
                        "mirelle1821")

                .Line(
                    "mirelle1821",
                    "1821. Portrait of Mirélle. Rejected by subject.")
                    .Choice(
                        "(Turn the page.)",
                        "mirelle1854")

                .Line(
                    "mirelle1854",
                    "1854. Portrait of Mirélle. Rejected by subject. Replacement requested. Reason not supplied.")
                    .Choice(
                        "(Think about it.)",
                        "mirelleThought",
                        onlyIf: p => p.HasFlag("MetMirelle"))
                    .Choice(
                        "(Back to the front.)",
                        "contents",
                        effect: p => ReadMirellePages(p))

                .Line(
                    "mirelleThought",
                    "Mirélle did not reject one portrait. Mirélle rejected every version that admitted time had passed.")
                    .Choice(
                        "(Back to the front.)",
                        "contents",
                        effect: p => ReadMirellePages(p))

                // ---- The gate gargoyles ----

                .Line(
                    "gate",
                    "Gate Guardians. Commissioned as a pair. Dúnán the Patient. Cairn the Steadfast.")
                    .Choice(
                        "(Read the second name again.)",
                        "cairn")
                    .Choice(
                        "(Back to the front.)",
                        "contents")

                .Line(
                    "cairn",
                    "Cairn the Steadfast. The ink is as clear as the day it was written. Nobody has needed to say it in a long time.")
                    .Choice(
                        "(Back to the front.)",
                        "contents")

                // ---- The family portrait ----

                .Line(
                    "family",
                    "Family Portrait. Main entrance steps. Subjects: a list of names in a careful hand. " +
                    "The final line has been blurred by water. Only the pressure marks remain.")
                    .Choice(
                        "(Think of the photograph.)",
                        "familyThought",
                        onlyIf: p => p.HasFlag("ComparedPhotographToPortrait"))
                    .Choice(
                        "(Back to the front.)",
                        "contents")

                .Line(
                    "familyThought",
                    "The missing name occupies the same place in the list as the absence in the picture.")
                    .Choice(
                        "(Back to the front.)",
                        "contents")

                // ---- Lady Vesper and Pip ----

                .Line(
                    "vesper",
                    "Portrait of Lady Vesper. Subject requested alterations to the hands. Artist refused.")
                    .Choice(
                        "(Think about it.)",
                        "vesperThought",
                        onlyIf: p => p.HasFlag("MetVesper"))
                    .Choice(
                        "(Turn the page.)",
                        "pip")
                    .Choice(
                        "(Back to the front.)",
                        "contents")

                .Line(
                    "vesperThought",
                    "You understand the disagreement.")
                    .Choice(
                        "(Turn the page.)",
                        "pip")
                    .Choice(
                        "(Back to the front.)",
                        "contents")

                .Line(
                    "pip",
                    "Informal study. Phillip Jackson and bear. Not commissioned. Preserved by request.")
                    .Choice(
                        "(Think about it.)",
                        "pipThought",
                        onlyIf: p => p.HasFlag("MetPip"))
                    .Choice(
                        "(Back to the front.)",
                        "contents")

                .Line(
                    "pipThought",
                    "The bear looks much the same.")
                    .Choice(
                        "(Back to the front.)",
                        "contents");

        // Reading all three of Mirélle's pages (with her three portraits seen
        // and Mirélle met) unlocks "I saw your portraits." with her later.
        private static void ReadMirellePages(Player p)
        {
            p.SetFlag("ReadMirelleLedgerEntries");
            if (p.HasFlag("MetMirelle") && p.HasFlag("ExaminedThreeAges"))
            {
                p.SetFlag("UnderstandsMirellePortraits");
            }
        }

        // =========================================================
        // LOOKUP (used by the game - no need to edit)
        // =========================================================

        internal static void Register(string id, Dialogue dialogue) => DialogueLookup[id] = dialogue;

        // Number of conversations (also makes sure they are all loaded).
        public static int Count => DialogueLookup.Count;

        // Every conversation by id (used by the start-up data check).
        public static IReadOnlyDictionary<string, Dialogue> All => DialogueLookup;

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
