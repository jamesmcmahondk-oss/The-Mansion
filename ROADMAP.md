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
      `right_closeup.png` / `right_open.png` (a side door)
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
- [ ] Glasshouse interior – *needs art + design*.
- [x] Art: photograph (icon + full-size view, right-click in the inventory),
      red rose and mirror icons; statue without its sword; cross with the open
      plinth.
- [ ] Art still wanted: icons for shears, sword, charcoal, rope, paper; rose bush
      with one rose cut. (Well close-up with the mirror: done. Darker bench
      close-up: done.)
- [x] Right side of the mansion: new view (roses, bench, arch to the well, path
      round the back). One rose is painted out once you've cut it. The
      servants' entrance is gone (the kitchen door in the rear yard covers it).

## Step 3 – Feel and feedback
- [x] **Show what's clickable** – the hotspot's name follows the cursor, and
      holding Space outlines and names every hotspot in the scene.
- [x] **Less repetitive replies** – variants of the fallback replies
      (Data/Replies.cs), wrong-verb lines for ~40 objects, `.OnOtherItems` on
      hotspots and `misuse:` lines on every item.
- [ ] **Sound** – rain, footsteps, creaks, dice rattle, music.
- [x] **Scene transitions** – each new scene fades in from black.
- [ ] Faster verbs (e.g. left-click = sensible action, right-click = look).

## Step 4 – Player safety nets
- [x] **Autosave** on entering each scene (not mid-conversation or fight);
      "AUTOSAVE" is the first entry on every Load screen.
- [x] Inventory holds more than 10 items: ◀ ▶ pages of 9 (also mouse wheel
      or the Left/Right keys).
- [x] **Journal** (J, or the Esc menu): 17 entries that write themselves from
      story flags (Data/Journal.cs), with a "New journal entry" notice.
      Add entries as the inside of the Mansion grows.
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
