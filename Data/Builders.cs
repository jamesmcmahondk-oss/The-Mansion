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

        /// <summary>Clicking it moves the player to another scene.</summary>
        public HotspotBuilder GoesTo(string sceneName)
        {
            _hotspot.TargetScene = sceneName;
            return this;
        }

        /// <summary>Reply when the player uses TALK on it (no conversation).</summary>
        public HotspotBuilder OnTalk(string text)
        {
            _hotspot.TalkText = text;
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

        /// <summary>ACT on it starts a conversation from Data/Dialogues.cs.</summary>
        public HotspotBuilder ActStarts(string dialogueId)
        {
            _hotspot.ActDialogueId = dialogueId;
            return this;
        }

        /// <summary>
        /// What happens when the player uses a particular item on it.
        /// Add one line per item. Other items get "Nothing happens".
        /// </summary>
        public HotspotBuilder OnItem(
            string itemName,
            string text,
            Action<Player>? effect = null,
            bool removeItem = false,
            string startsDialogue = "")
        {
            _hotspot.ItemUses[itemName] = new ItemUse
            {
                Text = text,
                Effect = effect,
                RemoveItem = removeItem,
                DialogueId = startsDialogue
            };

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

        /// <summary>Character picture, e.g. "Assets/Characters/Pip.png".</summary>
        public DialogueBuilder Portrait(string imagePath)
        {
            _dialogue.Portrait = imagePath;
            return this;
        }

        /// <summary>
        /// Put the character inside the scene (background image pixels:
        /// top-left corner and height) instead of on the right. Also stops
        /// the scene dimming so they look part of it.
        /// </summary>
        public DialogueBuilder StandAt(double x, double y, double height)
        {
            _dialogue.PortraitX = x;
            _dialogue.PortraitY = y;
            _dialogue.PortraitHeight = height;
            _dialogue.DimScene = false;
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
        /// use to jump here. Optional portrait swaps the picture for this line.
        /// </summary>
        public DialogueBuilder Line(string lineId, string text, string portrait = "")
        {
            var line = new DialogueNode
            {
                Text = text,
                Portrait = portrait
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
        /// </summary>
        public DialogueBuilder Choice(
            string text,
            string? goesTo = null,
            Func<Player, bool>? onlyIf = null,
            Action<Player>? effect = null)
        {
            CurrentLine(nameof(Choice)).Choices.Add(new DialogueChoice
            {
                Text = text,
                Next = goesTo,
                Condition = onlyIf,
                Effect = effect
            });

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
}
