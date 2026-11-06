using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * Builders: short, readable ways to write scenes, hotspots, dialogues
     * and items. Each method returns the builder, so you chain them:
     *
     *   NewScene("FountainCloseup", "Assets/Backgrounds/Mansion/fountain_closeup.png",
     *            "Lily pads float on the dark water.")
     *       .Hotspot("Gold Coins", 595, 655, 295, 175)
     *           .Describe("Several old gold coins rest at the bottom.")
     *           .ActStarts("HautainCoins")
     *       .ReturnTo("FrontGate", "You step away from the fountain.");
     *
     * See SCENE_GUIDE.md in the project folder for the full list.
     */

    // =============================================================
    // SCENES
    // =============================================================
    public class SceneBuilder
    {
        private readonly Scene _scene;
        private bool _registered;

        internal SceneBuilder(string name, string backgroundImage, string description)
        {
            _scene = new Scene
            {
                Name = name,
                BackgroundImage = backgroundImage,
                Description = description
            };
        }

        /// <summary>
        /// Adds a clickable area. X/Y/width/height are pixels of the background
        /// image (0-1536 across, 0-1024 down) - measure them with the F1
        /// hotspot editor, which copies this line to the clipboard for you.
        /// </summary>
        public HotspotBuilder Hotspot(string name, double x, double y, double width, double height)
        {
            var hotspot = new Hotspot
            {
                Name = name,
                X = x,
                Y = y,
                Width = width,
                Height = height
            };

            _scene.Hotspots.Add(hotspot);
            return new HotspotBuilder(this, hotspot);
        }

        /// <summary>
        /// Puts a picture in the scene (transparent PNG) at X/Y with the given
        /// height, in background image pixels - e.g. a character sitting under
        /// a bench. Optional onlyWhen hides it while false. Pair it with a
        /// .Hotspot(...) in the same place to make it clickable.
        /// </summary>
        public SceneBuilder Sprite(string image, double x, double y, double height,
                                   Func<Player, bool>? onlyWhen = null)
        {
            _scene.Sprites.Add(new SceneSprite
            {
                Image = image,
                X = x,
                Y = y,
                Height = height,
                VisibleWhen = onlyWhen
            });

            return this;
        }

        /// <summary>
        /// Show a different background once something has happened, e.g.
        /// .BackgroundWhen(p => p.HasFlag("FrontDoorOpen"), "Assets/Backgrounds/Mansion/Front_door_open.png")
        /// The new picture should be framed like the normal one so hotspots
        /// still line up. Several can be added; the first true one wins.
        /// </summary>
        public SceneBuilder BackgroundWhen(Func<Player, bool> condition, string backgroundImage)
        {
            _scene.AlternateBackgrounds.Add(new SceneBackground
            {
                Condition = condition,
                Image = backgroundImage
            });

            return this;
        }

        /// <summary>
        /// The scene's description, when it changes with the story, e.g.
        /// .DescribeScene(p => p.HasFlag("X") ? "The door stands open." : "The door is shut.")
        /// </summary>
        public SceneBuilder DescribeScene(Func<Player, string> description)
        {
            _scene.DescriptionWhen = description;
            return this;
        }

        /// <summary>
        /// Makes the Return arrow (top left) go back to another scene.
        /// </summary>
        public SceneBuilder ReturnTo(string sceneName, string text = "")
        {
            _scene.Hotspots.Add(new Hotspot
            {
                Name = "Return",
                ScreenSpace = true,      // the arrow is UI, not part of the picture
                X = 10,
                Y = 0,
                Width = 220,
                Height = 110,
                Description = text,
                TargetScene = sceneName
            });

            return this;
        }

        public Scene Build()
        {
            if (!_registered)
            {
                Scenes.Register(_scene);
                _registered = true;
            }

            return _scene;
        }

        public static implicit operator Scene(SceneBuilder builder) => builder.Build();
    }

    public class HotspotBuilder
    {
        private readonly SceneBuilder _scene;
        private readonly Hotspot _hotspot;

        internal HotspotBuilder(SceneBuilder scene, Hotspot hotspot)
        {
            _scene = scene;
            _hotspot = hotspot;
        }

        /// <summary>Text shown when the hotspot is clicked (LOOK, or any verb with no reaction of its own).</summary>
        public HotspotBuilder Describe(string text)
        {
            _hotspot.Description = text;
            return this;
        }

        /// <summary>
        /// LOOK text that changes with the story, e.g.
        /// .Describe(p => p.HasFlag("MetPip") ? "That's Pip." : "A pencil sketch.")
        /// It may also set flags (it runs each time the player LOOKs).
        /// </summary>
        public HotspotBuilder Describe(Func<Player, string> look)
        {
            _hotspot.LookResult = look;
            return this;
        }

        /// <summary>LOOK on it sets a story flag (e.g. "you have read the clue", for the journal).</summary>
        public HotspotBuilder LookSetsFlag(string flag)
        {
            _hotspot.LookFlag = flag;
            return this;
        }

        /// <summary>ACT on it moves the player on to another place (a new scene).</summary>
        public HotspotBuilder GoesTo(string sceneName)
        {
            _hotspot.TargetScene = sceneName;
            return this;
        }

        /// <summary>LOOK on it opens a close-up scene (e.g. the fountain close-up).</summary>
        public HotspotBuilder CloseUp(string sceneName)
        {
            _hotspot.CloseUpScene = sceneName;
            return this;
        }

        /// <summary>Reply when the player uses TALK on it (no conversation).</summary>
        public HotspotBuilder OnTalk(string text)
        {
            _hotspot.TalkText = text;
            return this;
        }

        /// <summary>
        /// TALK on it runs this and shows the text it returns - for replies
        /// that change, e.g. a different line the second time:
        /// .OnTalk(p => p.HasFlag("X") ? "Again?" : "Hello.")
        /// </summary>
        public HotspotBuilder OnTalk(Func<Player, string> talk)
        {
            _hotspot.TalkResult = talk;
            return this;
        }

        /// <summary>ACT on it plays this sound (Assets/Sounds/name.wav), e.g. .Sound("door_knock").</summary>
        public HotspotBuilder Sound(string soundName)
        {
            _hotspot.ActSound = soundName;
            return this;
        }

        /// <summary>
        /// The ACT needs this stat, e.g. .NeedsStat("Wits", 12): hovering shows
        /// "needs Wits 12 (yours: 9)". Check it in the OnAct with p.Meets(...)
        /// and give a clear reply either way - and always leave another way.
        /// </summary>
        public HotspotBuilder NeedsStat(string stat, int needed)
        {
            _hotspot.StatName = stat;
            _hotspot.StatNeeded = needed;
            return this;
        }

        /// <summary>ACT on it opens a puzzle screen, e.g. .ActOpensPuzzle("Cross").</summary>
        public HotspotBuilder ActOpensPuzzle(string puzzle)
        {
            _hotspot.ActPuzzle = puzzle;
            return this;
        }

        /// <summary>TALK on it starts a conversation from Data/Dialogues.cs.</summary>
        public HotspotBuilder TalkStarts(string dialogueId)
        {
            _hotspot.DialogueId = dialogueId;
            return this;
        }

        /// <summary>
        /// Reply when the player uses ACT on it, plus an optional effect,
        /// e.g. .OnAct("You pocket the key.", p => p.Give("Rusty Key"))
        /// </summary>
        public HotspotBuilder OnAct(string text, Action<Player>? effect = null)
        {
            _hotspot.ActText = text;
            _hotspot.ActEffect = effect;
            return this;
        }

        /// <summary>
        /// ACT on it runs this and shows the text it returns - for replies
        /// that depend on what happens, e.g.
        /// .OnAct(p => p.Meets("Luck", 11) ? "Got it!" : "It slips away.")
        /// </summary>
        public HotspotBuilder OnAct(Func<Player, string> action)
        {
            _hotspot.ActResult = action;
            return this;
        }

        /// <summary>
        /// After the ACT, go to the scene this returns ("" to stay), e.g.
        /// .ThenGoesTo(p => p.HasFlag("Solved") ? "PortraitGallery" : "")
        /// The ACT's reply is shown in the new scene.
        /// </summary>
        public HotspotBuilder ThenGoesTo(Func<Player, string> sceneName)
        {
            _hotspot.ThenGoTo = sceneName;
            return this;
        }

        /// <summary>Show this picture full-size after the ACT (click to close).</summary>
        public HotspotBuilder ShowsPicture(string imagePath)
        {
            _hotspot.ActPicture = imagePath;
            return this;
        }

        /// <summary>ACT on it starts a conversation from Data/Dialogues.cs.</summary>
        public HotspotBuilder ActStarts(string dialogueId)
        {
            _hotspot.ActDialogueId = dialogueId;
            return this;
        }

        /// <summary>ACT on it starts a fight from Data/Enemies.cs.</summary>
        public HotspotBuilder ActStartsFight(string enemyId)
        {
            _hotspot.ActFightId = enemyId;
            return this;
        }

        /// <summary>
        /// What happens when the player uses a particular item on it.
        /// Add one line per item. Other items get "You can't use...".
        /// The same item can be listed more than once with different onlyIf
        /// conditions; the first one that's true is used.
        /// </summary>
        public HotspotBuilder OnItem(
            string itemName,
            string text,
            Action<Player>? effect = null,
            bool removeItem = false,
            string startsDialogue = "",
            Func<Player, bool>? onlyIf = null)
        {
            if (!_hotspot.ItemUses.TryGetValue(itemName, out List<ItemUse>? uses))
            {
                uses = new List<ItemUse>();
                _hotspot.ItemUses[itemName] = uses;
            }

            uses.Add(new ItemUse
            {
                Text = text,
                Effect = effect,
                RemoveItem = removeItem,
                DialogueId = startsDialogue,
                Condition = onlyIf
            });

            return this;
        }

        /// <summary>
        /// Reply when any item without its own .OnItem is used on it.
        /// {item} is replaced by the item's name, e.g.
        /// .OnOtherItems("The rose bush is unimpressed by the {item}.")
        /// </summary>
        public HotspotBuilder OnOtherItems(string text)
        {
            _hotspot.OtherItemsText = text;
            return this;
        }

        /// <summary>
        /// The hotspot only exists while this is true,
        /// e.g. .OnlyWhen(p => !p.HasFlag("TookKey"))
        /// </summary>
        public HotspotBuilder OnlyWhen(Func<Player, bool> condition)
        {
            _hotspot.VisibleWhen = condition;
            return this;
        }

        // Carry on with the scene.
        public HotspotBuilder Hotspot(string name, double x, double y, double width, double height)
            => _scene.Hotspot(name, x, y, width, height);

        public SceneBuilder ReturnTo(string sceneName, string text = "")
            => _scene.ReturnTo(sceneName, text);

        public SceneBuilder BackgroundWhen(Func<Player, bool> condition, string backgroundImage)
            => _scene.BackgroundWhen(condition, backgroundImage);

        public SceneBuilder Sprite(string image, double x, double y, double height,
                                   Func<Player, bool>? onlyWhen = null)
            => _scene.Sprite(image, x, y, height, onlyWhen);

        public static implicit operator Scene(HotspotBuilder builder) => builder._scene.Build();
    }

    // =============================================================
    // DIALOGUES
    // =============================================================
    public class DialogueBuilder
    {
        private readonly string _id;
        private readonly Dialogue _dialogue = new();
        private DialogueNode? _currentLine;
        private bool _hasLine;
        private bool _registered;

        internal DialogueBuilder(string id, string speaker)
        {
            _id = id;
            _dialogue.Speaker = speaker;
        }

        /// <summary>
        /// Close-up framed portrait, always shown in the same spot on the left,
        /// e.g. "Assets/Characters/Portraits/Pip_portrait_framed.png".
        /// </summary>
        public DialogueBuilder Portrait(string imagePath)
        {
            _dialogue.Portrait = imagePath;
            return this;
        }

        /// <summary>
        /// Full-body character picture shown in the scene, e.g.
        /// "Assets/Characters/Pip.png". Place it with .StandAt(...);
        /// without it, the character stands on the right.
        /// </summary>
        public DialogueBuilder Character(string imagePath)
        {
            _dialogue.Character = imagePath;
            return this;
        }

        /// <summary>
        /// Where the .Character(...) picture stands in the scene (background
        /// image pixels: top-left corner and height).
        /// </summary>
        public DialogueBuilder StandAt(double x, double y, double height)
        {
            _dialogue.CharacterX = x;
            _dialogue.CharacterY = y;
            _dialogue.CharacterHeight = height;
            return this;
        }

        /// <summary>
        /// Story flag set as soon as this conversation starts (after .StartAt
        /// has picked the first line), e.g. .MetFlag("MetHautain"). Other
        /// conversations can then use onlyIf: p => p.HasFlag("MetHautain")
        /// so the player only asks about characters they have met.
        /// </summary>
        public DialogueBuilder MetFlag(string flag)
        {
            _dialogue.MetFlag = flag;
            return this;
        }

        /// <summary>Darken (or not) the scene during the conversation.</summary>
        public DialogueBuilder DimScene(bool dim = true)
        {
            _dialogue.DimScene = dim;
            return this;
        }

        /// <summary>
        /// Pick the first line from the player's state,
        /// e.g. .StartAt(p => p.HasFlag("MetPip") ? "again" : "start")
        /// Without this the conversation starts at the first Line.
        /// </summary>
        public DialogueBuilder StartAt(Func<Player, string> chooseLine)
        {
            _dialogue.ChooseStartNode = chooseLine;
            return this;
        }

        /// <summary>
        /// Something the character says. lineId is the name other choices
        /// use to jump here. Optional portrait / character swap the close-up
        /// or the full-body picture for this line only.
        /// </summary>
        public DialogueBuilder Line(string lineId, string text, string portrait = "", string character = "")
        {
            var line = new DialogueNode
            {
                Text = text,
                Portrait = portrait,
                Character = character
            };

            _dialogue.Nodes[lineId] = line;

            if (!_hasLine)
            {
                _dialogue.StartNode = lineId;
                _hasLine = true;
            }

            _currentLine = line;
            return this;
        }

        /// <summary>
        /// A reply the player can pick for the line above.
        ///   goesTo  - line to jump to (leave out to end the conversation)
        ///   onlyIf  - only show it when true, e.g. p => p.Wits >= 12
        ///   effect  - runs when picked, e.g. p => p.Give("Gold Coins")
        ///   startsFight - end the conversation and start this fight (Data/Enemies.cs)
        /// </summary>
        public DialogueBuilder Choice(
            string text,
            string? goesTo = null,
            Func<Player, bool>? onlyIf = null,
            Action<Player>? effect = null,
            string startsFight = "")
        {
            CurrentLine(nameof(Choice)).Choices.Add(new DialogueChoice
            {
                Text = text,
                Next = goesTo,
                Condition = onlyIf,
                Effect = effect,
                StartsFight = startsFight
            });

            return this;
        }

        /// <summary>
        /// How the character reacts when the player offers this item
        /// (ITEM button during the conversation). Works on any line.
        ///   goesTo     - line to jump to (leave out to end the conversation)
        ///   effect     - runs when offered, e.g. p => p.ClearFlag("StoleCoins")
        ///   removeItem - the character keeps it
        ///   onlyIf     - only react like this when true
        /// </summary>
        public DialogueBuilder OnItem(
            string itemName,
            string? goesTo = null,
            Action<Player>? effect = null,
            bool removeItem = false,
            Func<Player, bool>? onlyIf = null)
        {
            if (!_dialogue.ItemUses.TryGetValue(itemName, out List<DialogueItemUse>? uses))
            {
                uses = new List<DialogueItemUse>();
                _dialogue.ItemUses[itemName] = uses;
            }

            // Same item more than once: the first reaction whose onlyIf is true wins.
            uses.Add(new DialogueItemUse
            {
                GoesTo = goesTo,
                Effect = effect,
                RemoveItem = removeItem,
                Condition = onlyIf
            });

            return this;
        }

        /// <summary>
        /// Reply to any item without its own OnItem. "{item}" is replaced
        /// by the item's name. The player then goes back to where they were.
        /// </summary>
        public DialogueBuilder OnOtherItems(string text)
        {
            _dialogue.OtherItemText = text;
            return this;
        }

        /// <summary>Give the line above the same replies as an earlier line.</summary>
        public DialogueBuilder SameChoicesAs(string lineId)
        {
            if (!_dialogue.Nodes.TryGetValue(lineId, out DialogueNode? other))
            {
                throw new InvalidOperationException(
                    $"Dialogue '{_id}': SameChoicesAs(\"{lineId}\") - there is no earlier line called '{lineId}'.");
            }

            CurrentLine(nameof(SameChoicesAs)).Choices.AddRange(other.Choices);
            return this;
        }

        private DialogueNode CurrentLine(string method)
        {
            return _currentLine ?? throw new InvalidOperationException(
                $"Dialogue '{_id}': write a Line(...) before {method}(...).");
        }

        public Dialogue Build()
        {
            if (!_registered)
            {
                CheckLinks();
                Dialogues.Register(_id, _dialogue);
                _registered = true;
            }

            return _dialogue;
        }

        // Catch typos in goesTo straight away instead of mid-game.
        private void CheckLinks()
        {
            foreach (var pair in _dialogue.ItemUses)
            {
                foreach (DialogueItemUse use in pair.Value)
                {
                    string? next = use.GoesTo;

                    if (next != null && !_dialogue.Nodes.ContainsKey(next))
                    {
                        throw new InvalidOperationException(
                            $"Dialogue '{_id}': OnItem(\"{pair.Key}\") goes to '{next}', " +
                            $"but there is no line with that name.");
                    }
                }
            }

            foreach (var pair in _dialogue.Nodes)
            {
                foreach (DialogueChoice choice in pair.Value.Choices)
                {
                    if (choice.Next != null && !_dialogue.Nodes.ContainsKey(choice.Next))
                    {
                        throw new InvalidOperationException(
                            $"Dialogue '{_id}', line '{pair.Key}': the choice \"{choice.Text}\" " +
                            $"goes to '{choice.Next}', but there is no line with that name.");
                    }
                }
            }
        }

        public static implicit operator Dialogue(DialogueBuilder builder) => builder.Build();
    }

    // =============================================================
    // ENEMIES
    // =============================================================
    public class EnemyBuilder
    {
        private readonly Enemy _enemy;
        private bool _registered;

        internal EnemyBuilder(string id, string name, int skill, int health)
        {
            _enemy = new Enemy
            {
                Id = id,
                Name = name,
                Skill = skill,
                Health = health
            };
        }

        /// <summary>Close-up framed portrait, always in the same spot (as in dialogues).</summary>
        public EnemyBuilder Portrait(string imagePath)
        {
            _enemy.Portrait = imagePath;
            return this;
        }

        /// <summary>Close-up portrait shown once the enemy is defeated.</summary>
        public EnemyBuilder BeatenPortrait(string imagePath)
        {
            _enemy.BeatenPortrait = imagePath;
            return this;
        }

        /// <summary>
        /// Full-body picture shown in the scene during the fight, e.g.
        /// "Assets/Characters/Hautain_Attack.png". Place it with .StandAt(...).
        /// </summary>
        public EnemyBuilder Character(string imagePath)
        {
            _enemy.Character = imagePath;
            return this;
        }

        /// <summary>Where the .Character(...) picture stands (background image pixels).</summary>
        public EnemyBuilder StandAt(double x, double y, double height)
        {
            _enemy.CharacterX = x;
            _enemy.CharacterY = y;
            _enemy.CharacterHeight = height;
            return this;
        }

        /// <summary>Darken (or not) the scene during the fight.</summary>
        public EnemyBuilder DimScene(bool dim = true)
        {
            _enemy.DimScene = dim;
            return this;
        }

        /// <summary>Text shown when the fight starts.</summary>
        public EnemyBuilder Intro(string text)
        {
            _enemy.IntroText = text;
            return this;
        }

        /// <summary>The player can't run away from this fight.</summary>
        public EnemyBuilder NoFleeing()
        {
            _enemy.CanFlee = false;
            return this;
        }

        /// <summary>Text shown, and optional effect, when the player wins.</summary>
        public EnemyBuilder OnWin(string text, Action<Player>? effect = null)
        {
            _enemy.WinText = text;
            _enemy.OnWin = effect;
            return this;
        }

        /// <summary>Optional effect when the player runs away.</summary>
        public EnemyBuilder OnFlee(Action<Player> effect)
        {
            _enemy.OnFlee = effect;
            return this;
        }

        public Enemy Build()
        {
            if (!_registered)
            {
                Enemies.Register(_enemy);
                _registered = true;
            }

            return _enemy;
        }

        public static implicit operator Enemy(EnemyBuilder builder) => builder.Build();
    }
}
