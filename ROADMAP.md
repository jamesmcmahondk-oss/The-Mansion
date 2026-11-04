# The Mansion – Roadmap

The list of improvements we're working through. Tick items off (`[x]`) as
they're done; add new ones at the bottom of the right section.

## Step 1 – Safety net (in progress)
- [x] **Build check on GitHub** – `.github/workflows/build.yml` builds the game
      (Debug + Release) on Windows for every push and pull request.
- [x] **Start-up data check** – `Data/GameValidator.cs` lists broken references
      (scenes, items, conversations, fights, pictures) when a Debug build starts.
- [x] **Clean-up** – removed the stray `.whl` file, the empty `Character` class,
      the unused `LookBtn_MouseEnter` method and `Assets/UI/old`.
- [x] Fix whatever the first GitHub build finds. (It caught a mistake in the
      clean-up – the verb buttons' hover handler had been removed. Fixed;
      the game now builds on GitHub.)
- [x] Tidy the 5 compiler warnings (unused `_CurrentInteractionCursor` field;
      `Scene.Name/BackgroundImage/Description` without defaults).
- [x] Delete unused art: `Ebria_Drinking.png`, the old `Fountain.png`,
      `left.png`, `right.png` and the first right-side painting. Kept for later:
      `right_closeup.png` / `right_open.png` (side door – servants' entrance?)
      and the characters not in the game yet.
- [x] **Git LFS** for the images (`*.png`, `*.jpg`, `*.jpeg`, `*.webp`). Current
      images moved to LFS in a new commit (history not rewritten, so old
      versions still take up space there). The build check downloads them,
      cached between builds, and fails if any image is missing.

## Step 2 – Story and puzzles (see `PUZZLES.md`)
- [x] **Puzzle design for the outside** – `PUZZLES.md` (design rules, map,
      puzzles). Decided: Worn Photograph memento, Cracked Hand Mirror (Old Key
      retired), Wits-or-rubbing gravestone, rose hurts once, Faceless Knight.
- [x] Graveyard Secret: gravestone clue → three spirals → plinth → Worn Photograph.
- [x] Well: Cracked Hand Mirror; Ebria refuses it.
- [x] Rose bush: hurts once; Garden Shears → Red Rose.
- [x] Faceless Knight: put your weapon away (click WEAPON) → Stone Sword.
- [x] Hautain ↔ Ebria cross-reference; the Gargoyle notices the rain.
- [x] **Scene list** – `SCENES.md`, regenerate with `python Tools/list_scenes.py`.
- [x] **Rear yard** (from *Path to the Back*): kitchen door (bolted inside),
      cellar doors (one knock), hand pump + trough, drain, clothesline and The
      Sock, tool shed with Garden Shears, Charcoal and Rope.
- [ ] Tool shed close-up – *needs art* (the shed opens in the yard for now).
- [x] **Kitchen Garden** (rear yard → door in the wall → Garden): the outdoor loop is closed.
- [ ] Where Paper is found (Charcoal: tool shed).
- [ ] **Entrance Hall** – *needs art* (the data check reports it missing).
- [ ] Servants' entrance as a shortcut from inside.
- [ ] Glasshouse interior – *needs art + design*.
- [x] Art: photograph (icon + full-size view, right-click in the inventory),
      red rose and mirror icons; statue without its sword; cross with the open
      plinth.
- [ ] Art still wanted: icons for shears, sword, charcoal, rope, paper; rose bush
      with one rose cut. (Well close-up with the mirror: done.)

## Step 3 – Feel and feedback
- [ ] **Show what's clickable** – hotspot highlight / name next to the cursor,
      and a key that briefly shows every hotspot.
- [ ] **Less repetitive replies** – random variants of "You can't do that.",
      custom wrong-verb lines for important objects, item-specific misuse lines.
- [ ] **Sound** – rain, footsteps, creaks, dice rattle, music.
- [ ] **Scene transitions** – short fades instead of hard cuts.
- [ ] Faster verbs (e.g. left-click = sensible action, right-click = look).

## Step 4 – Player safety nets
- [ ] **Autosave** on entering each scene.
- [ ] **Journal** of clues and conversations ("…KEEPER OF THE KEYS…", Ebria's key).
- [ ] Healing (rest spots, food, potions) so Health matters outside fights.
- [ ] Limit retries on Luck checks (e.g. the bucket) so they aren't a grind.

## Step 5 – Code structure (when the story starts growing fast)
- [ ] **Split `MainWindow.xaml.cs`** (3,800 lines) into a game engine (scenes,
      verbs, dialogue, fights, inventory, saving) and the UI.
- [ ] **Unit tests** for fight maths, save/load and conversation links.
- [ ] **Conversations**: one file per character; consider the Ink scripting
      language instead of C# for writing them.
- [ ] Constants for scene / item / flag names (`SceneIds`, `Flags`) so typos
      can't happen and renaming a scene doesn't break saves.
- [ ] Cache and preload pictures for smoother scene changes.
