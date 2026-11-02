# The Mansion – Scene & Story Guide

Everything in the game world is written in three files:

| File | What goes in it |
|---|---|
| `Data/Scenes.cs` | Places, their clickable hotspots, and what each verb does |
| `Data/Dialogues.cs` | Conversations with characters |
| `Data/Items.cs` | Things the player can carry |

Each one uses short **builder chains**: start a thing, then add `.Something(...)`
lines underneath. New scenes, dialogues and items register themselves, so you
only ever add code in these files. **F1** in a Debug build jumps to any scene
for testing.

---

## 1. Scenes

```csharp
public static Scene FountainCloseup { get; } =
    NewScene("FountainCloseup",                                  // name (used by GoesTo / ReturnTo)
             "Assets/Backgrounds/Mansion/fountain_closeup.png",  // 1536x1024 background
             "Lily pads float on the dark water.")               // text shown on arrival

    .Hotspot("Gold Coins", 595, 655, 295, 175)   // name, X, Y, width, height
        .Describe("Several old gold coins rest at the bottom of the fountain.")
        .ActStarts("HautainCoins")

    .ReturnTo("FrontGate", "You step away from the fountain.");
```

* Hotspot positions are **pixels of the background image** (0–1536 across,
  0–1024 down). Only **Y 192–832** is visible in game.
* **Measuring:** F1 → tick *Show hotspot outlines / hotspot editor* → drag a
  box over the object. A ready-made `.Hotspot(...)` line is copied to the
  clipboard; paste it into the scene.
* `.ReturnTo(...)` makes the Return arrow (top left) go back somewhere.
* `.BackgroundWhen(p => …, "image.png")` shows a different background once
  something has happened (see the recipe below). Put it right under `NewScene(...)`.

## 2. What a hotspot does

Add any of these under a `.Hotspot(...)` line:

| Line | Happens when… |
|---|---|
| `.Describe("text")` | it's clicked with LOOK (or a verb with no reaction of its own) |
| `.GoesTo("SceneName")` | it's clicked → move to that scene |
| `.OnTalk("text")` | TALK on it → this reply |
| `.TalkStarts("DialogueId")` | TALK on it → start a conversation |
| `.OnAct("text")` | ACT on it → this reply |
| `.OnAct("text", p => …)` | ACT on it → reply **and** do something |
| `.ActStarts("DialogueId")` | ACT on it → start a conversation |
| `.OnItem("Item Name", "text")` | the player uses that item on it |
| `.OnItem("Item Name", "text", effect: p => …, removeItem: true, startsDialogue: "Id")` | …with extras (all optional) |
| `.OnlyWhen(p => …)` | the hotspot only exists while this is true |

## 3. The player: `p`

Inside any `p => …` you can read and change the player:

```csharp
p.Has("Gold Coins")          // carrying it?
p.Give("Gold Coins")         // add to inventory
p.Take("Gold Coins")         // remove from inventory

p.SetFlag("StoleCoins")      // remember that something happened
p.HasFlag("StoleCoins")      // has it happened?
p.ClearFlag("StoleCoins")    // forget it

p.Health -= 10;   p.Strength += 1;   p.Luck += 1;   p.Wits += 1;
p.Weapon = "Rusty Dagger";
p.StatusEffect = "Poisoned";   // "None" clears it
```

**Flags** are the story's memory: any name you like, e.g. `"MetPip"`,
`"DoorOpen"`, `"StoleCoins"`. The stats panel and hotspots update by
themselves after every effect.

Several changes in one go → use braces:

```csharp
effect: p =>
{
    p.Give("Gold Coins");
    p.SetFlag("StoleCoins");
    p.StatusEffect = "Royal Grudge";
}
```

## 4. Conversations

```csharp
public static Dialogue HautainCoins { get; } =
    NewDialogue("HautainCoins", "King Hautain")       // id, name shown above lines
        .Portrait("Assets/Characters/Hautain.png")    // optional picture
        .StandAt(495, 205, 285)                       // optional: stand IN the scene (X, Y, height)
        .StartAt(p => p.HasFlag("StoleCoins") ? "again" : "start")   // optional

    .Line("start", "...HALT, THIEF! Those coins belong to the Crown!")
        .Choice("King? You're a frog on a lily pad.", "insult")
        .Choice("Surely the Crown can spare a coin?", "flattered",
                onlyIf: p => p.Wits >= 12,
                effect: p => p.Give("Royal Gold Coin"))
        .Choice("Finders keepers!", "grab",
                effect: p => p.SetFlag("StoleCoins"))
        .Choice("Goodbye.")                        // no goesTo = conversation ends

    .Line("insult", "A FROG?!", portrait: "Assets/Characters/Hautain_Attack.png")
        .Choice("Awkward.", "start")

    .Line("again", "YOU again!")
        .Choice("Bye.");
```

* `.Line("id", "text")` – something the character says. The first line is where
  the conversation starts (unless `.StartAt` says otherwise).
* `.Choice("text", "goesTo")` – a player reply that jumps to another line.
  Leave out `goesTo` to end the conversation. Up to **4** are shown.
* `onlyIf:` hides a reply unless true. `effect:` runs when it's picked.
* `.SameChoicesAs("start")` – reuse an earlier line's replies (handy for
  "Anything else?" lines).
* Keep each line to about **two lines of text** so the replies fit.
* Without `.StandAt`, the portrait stands on the right and the scene darkens.
* A typo in a `goesTo` stops the game at start-up with a message naming the
  line, so mistakes are caught straight away.

## 5. Items

```csharp
public static Item RustyKey { get; } =
    NewItem("Rusty Key",
            "An old iron key, orange with rust.",
            "Assets/Items/rusty_key.png");     // optional 128x128 icon
```

Icons: add a crop to `Tools/build_item_icons.py` and run it, or draw your own
128×128 PNG. Items without an icon show their initials.

---

## Recipes

### Change the background after something happens
Quazimodo's conversation sets the `FrontDoorOpen` flag, so the Front Door scene
switches to the open-door picture – straight away, and every visit after:
```csharp
NewScene("FrontDoor", "Assets/Backgrounds/Mansion/Front_door.png", "...")
    .BackgroundWhen(p => p.HasFlag("FrontDoorOpen"),
                    "Assets/Backgrounds/Mansion/Front_door_open.png")
```
* The alternate picture must be **framed the same** as the normal one, so
  hotspots still line up.
* Pair it with `.OnlyWhen(...)` hotspots so the clickable areas change too
  (closed doors → open doors).
* Several `.BackgroundWhen` lines are allowed; the **first** true one wins.
* Add new pictures to the project as a *Resource* (like the other backgrounds
  in `The Mansion.csproj`), or they won't load.

**Testing:** F1 → *Story flags* → type `FrontDoorOpen` → **Set** / **Clear**.
The scene updates immediately.

### Pick something up
```csharp
.Hotspot("Rusty Key", 700, 600, 60, 40)
    .Describe("A rusty key lies in the grass.")
    .OnAct("You pick up the key.", p => p.Give("Rusty Key"))
    .OnlyWhen(p => !p.Has("Rusty Key"))     // gone once taken
```
(The key is painted into the background, so it stays visible in the picture.
Mention it in the scene text, or ask for item sprites when you need them.)

### A locked door that opens with a key
Two hotspots in the same place – only one exists at a time:
```csharp
.Hotspot("Door", 590, 300, 375, 500)
    .Describe("Locked tight.")
    .OnItem("Rusty Key", "The lock turns with a groan. The door swings open!",
            effect: p => p.SetFlag("DoorOpen"),
            removeItem: true)
    .OnlyWhen(p => !p.HasFlag("DoorOpen"))

.Hotspot("Open Door", 590, 300, 375, 500)
    .Describe("The door stands open.")
    .GoesTo("EntranceHall")
    .OnlyWhen(p => p.HasFlag("DoorOpen"))
```

### Someone who appears when you touch something (like Hautain)
```csharp
// Scenes.cs
.Hotspot("Gold Coins", 595, 655, 295, 175)
    .ActStarts("HautainCoins")

// Dialogues.cs – .StandAt(...) puts him on the lily pad
```

### A character who remembers you
```csharp
NewDialogue("Pip", "Pip")
    .StartAt(p => p.HasFlag("MetPip") ? "again" : "start")
.Line("start", "Oh! A visitor!")
    .Choice("Hello, Pip.", effect: p => p.SetFlag("MetPip"))
.Line("again", "You came back!")
    .Choice("Bye, Pip.")
```

### A stat check
```csharp
.Choice("(Force the door)", "forced", onlyIf: p => p.Strength >= 14)
```

### Undo a bad deed
```csharp
.OnItem("Gold Coins", "You put the coins back.",
        removeItem: true,
        effect: p => { p.ClearFlag("StoleCoins"); p.StatusEffect = "None"; })
```
