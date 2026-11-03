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
* `.Sprite("Assets/Characters/Ebria_Drinking.png", X, Y, height)` puts a
  transparent picture in the scene (e.g. a character sitting under a bench).
  Optional `onlyWhen: p => …` hides it while false. Add a `.Hotspot(...)` in the
  same place to make it clickable.
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
        .Portrait("Assets/Characters/Portraits/Hautain_portrait_framed.png")   // close-up (optional)
        .Character("Assets/Characters/Hautain.png")   // full-body picture in the scene (optional)
        .StandAt(495, 205, 285)                       // where the Character stands (X, Y, height)
        .StartAt(p => p.HasFlag("StoleCoins") ? "again" : "start")   // optional

    .Line("start", "...HALT, THIEF! Those coins belong to the Crown!")
        .Choice("King? You're a frog on a lily pad.", "insult")
        .Choice("Surely the Crown can spare a coin?", "flattered",
                onlyIf: p => p.Wits >= 12,
                effect: p => p.Give("Royal Gold Coin"))
        .Choice("Finders keepers!", "grab",
                effect: p => p.SetFlag("StoleCoins"))
        .Choice("Goodbye.")                        // no goesTo = conversation ends

    .Line("insult", "A FROG?!", character: "Assets/Characters/Hautain_Attack.png")
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
* `.OnItem("Item Name", "lineId")` – what happens when the player **offers an
  item** during the conversation (see below).
* Keep each line to about **two lines of text** so the replies fit.
* **Two pictures, both optional:**
  * `.Portrait(...)` – the **close-up framed portrait**. Always shown in the same
    spot on the left of the scene (background pixels X 60, Y 350, height 400).
    Portraits live in `Assets/Characters/Portraits` (built by
    `Tools/build_portraits.py`).
  * `.Character(...)` – the **full-body picture** standing in the scene, placed
    with `.StandAt(X, Y, height)`. Without `.StandAt` it stands on the right.
* Swap either one for a single line: `portrait: "...Beaten_portrait_framed.png"`
  (e.g. after a fight) or `character: "...Hautain_Attack.png"` (an angry pose).
* The scene darkens only when nothing is standing in it (no `.StandAt`
  character and no portrait). Force it with `.DimScene(true)` or `.DimScene(false)`.
* A typo in a `goesTo` stops the game at start-up with a message naming the
  line, so mistakes are caught straight away.

### Offering items during a conversation

While talking, the **ITEM** button stays lit. Clicking it swaps the
conversation for the inventory; picking an item offers it to the character.
Put these near the top of a dialogue (after `.StartAt`):

```csharp
.OnItem("Gold Coins", "returned",                 // jump to the "returned" line
        removeItem: true,                         // the character keeps it
        effect: p => p.ClearFlag("StoleCoins"))   // optional
.OnItem("Royal Gold Coin", "keepit")              // react, but the player keeps it
.OnItem("Cake", "eats", onlyIf: p => p.HasFlag("PipIsHungry"))   // only sometimes
.OnOtherItems("\"What would I want with a {item}?\"")         // anything else
```

* Leave out the line name (e.g. `.OnItem("Rose")`) to end the conversation.
* Items with no `OnItem` get the `OnOtherItems` reply (or
  "*Speaker* shows no interest in the *item*."), then a **(Back)** choice
  returns to the line the player was on.
* Esc (or ITEM again) closes the inventory and returns to the conversation.

## 5. Items

```csharp
public static Item RustyKey { get; } =
    NewItem("Rusty Key",
            "An old iron key, orange with rust.",
            "Assets/Items/rusty_key.png");     // optional 128x128 icon
```

Icons: add a crop to `Tools/build_item_icons.py` and run it, or draw your own
128×128 PNG. Items without an icon show their initials.

## 6. The car trunk (starting item)

After the character roll, the player picks **one** item from their car trunk
(`Assets/UI/trunk.png`). The choices are in `Data/Trunk.cs`:

```csharp
Add("Party Hat", 850, 325, 245, 205),                    // item, X, Y, width, height in trunk.png
Add("Fork", 1050, 505, 210, 240, onTake: p => p.Weapon = "Fork"),   // optional effect
```

* Each name must also exist in `Data/Items.cs` (that's where the description
  and icon come from).
* Positions are pixels of `trunk.png`; only Y 80–944 is visible on screen.
* Where boxes overlap, the one listed **last** is on top.

## 7. Fights

Fighting Fantasy style. Each round both sides roll two bone dice:

| | Attack Strength |
|---|---|
| Player | 2d6 + Strength ÷ 2 + weapon bonus (no weapon = −1) |
| Enemy | 2d6 + Skill |

* Higher total hits for **10**. A tie: both miss.
* **Double six** = critical hit for **20**. **Double one** = fumble, the round is lost.
* **ATTACK** rolls a round. **ITEM** uses an item (the enemy gets a free attack;
  items that can't help don't use up your turn). **FLEE** always works but
  costs a parting blow (10), unless the enemy has `.NoFleeing()`.
* Lose all Health → *You have died*, with **Try the fight again** (back to how
  you were when the fight began) or **Start again**.

Enemies live in `Data/Enemies.cs`:

```csharp
public static Enemy AngryHautain { get; } =
    NewEnemy("AngryHautain", "King Hautain", skill: 4, health: 30)
        .Portrait("Assets/Characters/Portraits/Hautain_portrait_framed.png")        // close-up
        .BeatenPortrait("Assets/Characters/Portraits/Hautain_Beaten_portrait_framed.png") // shown when you win
        .Character("Assets/Characters/Hautain_Attack.png")   // full-body, in the scene
        .StandAt(495, 205, 285)
        .Intro("King Hautain leaps from his lily pad!")
        .NoFleeing()                                // optional
        .OnWin("He paddles away into the reeds.",
               p => p.SetFlag("DefeatedHautain"));
```

**Skill:** 3 feeble · 5 fair · 7 tough · 9 deadly. **Health:** 10 per hit.
A typical hero (Strength 12, no weapon) has +5, so Skill 5 is an even fight.

Start a fight from a conversation answer or by ACT on a hotspot:
```csharp
.Choice("(Stand your ground and fight!)", startsFight: "AngryHautain")
.Hotspot("Rat", 100, 600, 80, 50).ActStartsFight("GiantRat")
```

**Weapons and fight items** are set on items in `Data/Items.cs`:
```csharp
NewItem("Fork", "...", "Assets/Items/fork.png", weaponBonus: 1)   // when p.Weapon = "Fork"

NewItem("Empty Bottle", "...", "Assets/Items/empty_bottle.png",
        inFight: (p, fight) => { fight.HurtEnemy(10); return "You hurl the bottle! (-10)"; },
        usedUpInFight: true)
```

**Testing:** F1 → *Start a fight* lists every enemy.

## 8. Luck and Wits tests

For traps, chance events and puzzles (not fights). Roll 3d6 – equal to or
under the stat succeeds (10 = 50%, 12 = 74%, 7 = 16%):

```csharp
.OnAct("You reach into the dark hole...", p =>
{
    if (p.TestLuck()) p.Give("Silver Key");
    else p.Health -= 10;          // something bites! (0 Health = game over)
})

effect: p => { if (p.TestWits()) p.SetFlag("SolvedRiddle"); }
```

## 9. Saving

Players save and load from the **Esc** menu (3 slots), and load from the title
screen or the *You have died* screen. Saves are small files in
`%AppData%\The Mansion\Saves`.

A save stores the player – stats, weapon, status, **inventory and story
flags** – plus the current scene. So:

* Keep story progress in **flags and items** (`p.SetFlag(...)`, `p.Give(...)`)
  and it is saved automatically. Hotspots (`.OnlyWhen`), backgrounds
  (`.BackgroundWhen`) and conversations (`.StartAt`) rebuild themselves from
  them after loading.
* **Renaming a scene** breaks saves made in it (the game will say so).
* Saving isn't allowed during conversations or fights.

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

// Dialogues.cs – .Character(...) + .StandAt(...) puts him on the lily pad
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

### Only ask about people the player has met
Give every character's conversation a met flag – it is set the moment the
conversation starts (after `.StartAt` has picked the first line, so first
meetings still work):
```csharp
NewDialogue("HautainCoins", "King Hautain")
    .MetFlag("MetHautain")
```
Then, in anyone else's conversation, hide questions that name them:
```csharp
.Choice("Tell me about Hautain.", "hautain", onlyIf: p => p.HasFlag("MetHautain"))
```
Current flags: `MetGargoyle`, `MetHautain`, `MetQuazimodo`. Characters not in
the game yet are already referred to as `MetPip`, `MetEbria`,
`MetMagnusAndSeverin`, `MetVelvet`, `MetSola` and `MetGrell` – use those
names with `.MetFlag(...)` when you write their conversations.

Keep at least one answer **without** `onlyIf` on every line, so the player
always has a way on.

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
