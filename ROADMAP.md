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
- [ ] Tidy the 5 compiler warnings (unused `_CurrentInteractionCursor` field;
      `Scene.Name/BackgroundImage/Description` without defaults).
- [ ] Decide: delete unused art (`Assets/Characters/Ebria_Drinking.png`, the old
      right-side painting in `Sources/`) or keep it for later.
- [ ] **Git LFS** for the images (repo is ~180 MB and grows with every picture
      change). Needs Git LFS enabled on GitHub; changes how the repo stores files.

## Step 2 – Story and puzzles
- [ ] **Puzzle dependency chart** – what the player must find/do, in what order,
      to get inside; two or three routes.
- [ ] Give the open hooks a purpose: Old Key, hollow plinth under the cross,
      locked servants' entrance, glasshouse doors, path to the back of the house.
- [ ] **Entrance Hall** scene (Quazimodo's open doors lead to `EntranceHall`,
      which doesn't exist yet – the data check reports it).
- [ ] Back of the house scene (`.GoesTo("BackOfHouse")` is ready in a comment).

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
