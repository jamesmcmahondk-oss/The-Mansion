# The Mansion – Outside: Puzzle Design

The exterior is the game's hub and its tutorial. It shouldn't get much bigger;
it should **interlock**: a clue in one place opens something in another,
characters talk about each other, and doors opened from inside become shortcuts.

> *"This isn't a collection of rooms. Everyone actually lives here."*

## Design rules

- **Stats create shortcuts. Items provide reliable solutions.** A failed roll
  never blocks progress – there is always another way (e.g. Wits *or* a rubbing).
- **No more keys for their own sake.** Rewards should mean something: a
  memory, a reaction, a new route.
- **Don't tell the player where an item goes.** Classic inventory design.
- **Mementos** are objects that stir memories the player doesn't know they
  have. The first one belongs to the player's own mystery, not an NPC's quest.
- Characters reference each other, and **their accounts don't agree**.
- Comedy once; then the world remembers (the rose bush hurts *once*).

## Outdoor map

```mermaid
flowchart TD
    GATE[Front Gate] --> COURT[Courtyard]
    COURT -->|centre| DOOR[Front Door: Quazimodo] --> HALL([Entrance Hall])
    COURT -->|left| GARDEN[Garden] --> STATUE[Statue] & GLASS[Glasshouse]
    GLASS -.later.-> GLASSIN([Glasshouse interior: Grell?])
    COURT -->|right| RIGHT[Right side] --> BENCH[Bench: Ebria] & WELL[Well]
    WELL --> GRAVE[Graveyard] -.later.-> DEEP([Deeper graveyard])
    RIGHT -->|around back| REAR[Rear yard] --> SHED[Tool shed] & KITCHEN([Kitchen]) & CELLAR([Cellar])
    REAR -.later.-> KGARDEN([Kitchen Garden]) -.-> GLASS
```

Rounded boxes don't exist yet.

## The puzzles

### 1. The Graveyard Secret → first Memento ✅ built
1. **Gravestone inscription** – LOOK: worn smooth. ACT: **Wits test** (retry
   allowed; failure says *"There must be another way to recover the
   inscription."*). Or **Charcoal + Paper** (use either while carrying both):
   a rubbing, always works. Reveals:
   **THRICE FOR HEAVEN · ONCE FOR THE HEART · NEVER FOR THE HAND THAT LET GO**
2. **The cross** – three spirals at the centre (up, left, right). Before the
   clue: *"You push the carving. Nothing."* After it, each turns a quarter on
   ACT and says where it points. Solution: **up ×3, left ×1, right ×0**.
3. **The plinth opens** → **Worn Photograph** (Memento): the Mansion as it
   was; a small group; one face decayed away; on the back *"You said you'd
   come back."* Taking it: *the rain stops… you remember sunlight.*
   Who the people are is **deliberately undecided** – it should fit the ending.

### 2. The Well → Cracked Hand Mirror ✅ built
Replaces the Old Key. Ball of String (always) or bucket (Luck, retry).
**Show it to Ebria**: *"No." / "It's a mirror." / "I can see that."*, then
*"Put that away."*, then *Ebria will not look at it.* Never taken – it isn't
hers (and not her sister's either). Pays off much later.

### 3. The Rose Bush → Red Rose ✅ built (shears not placed yet)
First ACT: −2 Health, once. After: *"You've already conducted this
experiment."* At ≤2 Health: *"Something resembling common sense finally
intervenes."* (A rose bush must never kill you.) **Garden Shears** → **Red
Rose**. Where the rose goes is not told. *(Needs: shears in the tool shed;
ideally a picture with one rose missing.)*

### 4. The Faceless Knight → Stone Sword ✅ built
Words on the blade: **HE WHO WOULD TAKE THE BLADE MUST FIRST SURRENDER HIS OWN.**
Holding a weapon: the stone fingers tighten. Put it away (click **WEAPON** in
the stats panel) → GRIND → the sword comes free (+2). The solution is
understanding, not an item. *(Needs: statue picture without the sword.)*

### 5. Characters who know each other ✅ built
- **Hautain** on Ebria: *"THE BENCH-DWELLING MENACE."* She *"DESCENDED FROM THE
  HEAVENS upon my Eastern Provinces."* – Ebria: *"I fell in the pond."*
- **The Gargoyle** (second visit): *"You don't cast rain properly." … "Nothing."*
  The first hint that something is wrong with the player.

### 6. Still to build
| What | Needs |
|---|---|
| ~~Rear yard~~ ✅ built: kitchen door (bolted inside), cellar doors (one knock), pump + trough, drain, The Sock, tool shed (shears, charcoal, rope) | – |
| **Tool shed close-up** (the shed currently opens in the yard) | Art |
| **Kitchen Garden** (rear yard → glasshouse) | Art |
| **Glasshouse interior** (Grell's territory?) | Art + design |
| Paper – where is it found? (Charcoal: tool shed) | Decision |
| Item icons: mirror, photograph, rose, shears, sword, charcoal, paper | Art (or crops) |
| Well close-up picture shows a key, not a mirror | Art |

## What the exterior teaches

| Location | Teaches |
|---|---|
| Front Gate | LOOK / TALK |
| Fountain | Consequences; characters react |
| Courtyard | Navigation (ACT moves on) |
| Garden | Investigation |
| Statue | State / equipment puzzle |
| Glasshouse | Locked places to come back to |
| Right side | ACT reveals hidden things (the bench) |
| Bench | Character discovery |
| Rose bush | Health consequences – once |
| Well | ITEM use |
| Graveyard | Multi-scene puzzle |
| Rear yard | Shortcuts |
