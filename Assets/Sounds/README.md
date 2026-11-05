# Sounds

Put sound files here as `name.wav` or `name.mp3`. They are copied next to the
game when it is built; a missing file is simply silent. Keep effects short
(under ~2 s) and loops seamless.

| File name | Plays when |
|---|---|
| `stone_grind` | a spiral disc in the cross puzzle turns |
| `stone_stuck` | a disc won't move (gravestone not read yet) |
| `puzzle_click` | the cross puzzle is solved and the discs lock |
| `stone_slide` | the plinth slides open |
| `dice_roll` | the dice roll in a fight |
| `hit` | someone is hit in a fight |
| `door_knock` | the front door knocker |
| `door_creak` | a door opens (tool shed, front doors) |
| `journal_open` | the journal opens (page turn) |
| `journal_entry` | a new journal entry is written (soft chime) |
| `rain_loop` | outdoors, looping |
| `hall_ambience` | inside the Entrance Hall, looping (clock, candles) |
| `Title_music` | title screen, backstory, character roll and trunk (loops) |

Licences: use only sounds you may use in a game (e.g. CC0 from freesound.org
or Kenney.nl) and note where each came from in `CREDITS.md`.

## Levels

Put originals in `Assets/SoundSources`, then run `python Tools/normalize_sounds.py`
(needs ffmpeg). It writes levelled copies here: effects all equally loud,
background loops quieter, the title music in between, nothing clipping.
