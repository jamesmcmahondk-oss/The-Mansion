# Mirélle, the Portrait Ledger and the Brass House Token

Canonical sequence. BUILT: Mirélle's Parlour (MirelleParlour, through the
gallery's Far Door), her conversation (Dialogues.Mirelle - the scene below),
the token and the locked right door. Still to build: the East Corridor and
the Old Portrait Studio.

```
Portrait Gallery puzzle (medallions)
 -> Portrait Ledger (hidden compartment)
 -> the ledger: Mirélle commissioned and rejected portraits for decades (1798, 1821, 1854)
 -> player confronts Mirélle about it (Mirélle's Parlour, beyond the gallery's Far Door)
 -> Mirélle wants it back
 -> give it, or refuse - both are characterisation, neither is failure
 -> Mirélle gives the Brass House Token either way
 -> Entrance Hall, right door -> East Corridor -> ... -> Old Portrait Studio
```

Map:
```
Entrance Hall
 ├── Left  -> Portrait Gallery -> Mirélle's Parlour
 └── Right -> East Corridor -> ??? -> Old Portrait Studio
```

## Why she wants the ledger

Not vanity, and not "aging" - she would never frame it that way. Every
portrait insists a different person was Mirélle; she rejected them all. She
can dismiss a painting (the painter was incompetent), take it down, order
another. She cannot argue with the dates. The ledger is painfully objective.

## The scene (draft - final wording with the head writer)

Trigger: carrying the Portrait Ledger adds "I found the Portrait Ledger." to
her conversation (offering it with ITEM does the same).

> PLAYER: "I found the Portrait Ledger."
> *Mirélle pauses. Exactly one breath too long.*
> MIRÉLLE: "Did you."
> PLAYER: "I read it."
> MIRÉLLE: "Naturally. People who find ledgers always read them. It is the only thing ledgers are good for."
> PLAYER: "Your name is in it."
> MIRÉLLE: "Allegedly."
> PLAYER: "Several times."
> *Mirélle holds out a gloved hand.*
> MIRÉLLE: "Give it to me. It is mine more than it is anyone else's."

**"Here."** (the kind choice - sets GaveMirelleLedger)
> *Mirélle closes her fingers around it but doesn't open it. She doesn't need to.*
> MIRÉLLE: "Thank you." ... "That was... unexpectedly decent."
> *She produces a small, tarnished brass disc and holds it between two fingers.*
> MIRÉLLE: "Take this."  PLAYER: "What is it?"
> MIRÉLLE: "Something I no longer require."  PLAYER: "That's not an answer."
> MIRÉLLE: "Door on the right of the entrance hall."  PLAYER: "What's through it?"
> MIRÉLLE: "Somewhere else."
> (Brass House Token received.)

**"No. I'm keeping it."**
> *Her extended hand remains perfectly still for a moment. Then she withdraws it.*
> MIRÉLLE: "Of course you are."  PLAYER: "There might be something important in it."
> MIRÉLLE: "Then keep it. Read it in bed. Underline the dates."
> *She drops a small brass disc into the player's palm as though disposing of something unpleasant.*
> MIRÉLLE: "Here."  PLAYER: "What's this?"
> MIRÉLLE: "The door on the right of the entrance hall."  PLAYER: "You're giving me this anyway?"
> MIRÉLLE: "Yes."  PLAYER: "Why?"
> MIRÉLLE: "Because apparently your curiosity requires more room."  PLAYER: "What's through there?"
> MIRÉLLE: "Go and be curious somewhere I am not."
> (Brass House Token received.)

**"Why did you reject all the portraits?"**
> MIRÉLLE: "The painters lacked delicacy."  PLAYER: "All of them?"
> MIRÉLLE: "Remarkable, isn't it?"  PLAYER: "The ledger covers fifty-six years."
> *Mirélle's gaze shifts away.*
> MIRÉLLE: "And yet every one of them made precisely the same mistake."  PLAYER: "What mistake?"
> MIRÉLLE: "They painted a stranger and put my name beneath her." ... "I became tired of correcting them."

Stop there. Do not explain it further.

Alternative short form, if the corridor needs a beat: "What's through there?" -
"Old rooms." - "That's remarkably unhelpful." - "It wasn't intended to be helpful."

## Implementation notes

* Flags: GotBrassToken (either way), GaveMirelleLedger (the kind choice).
* Giving the ledger away must not cost the player anything: before it goes,
  the journal gets **"Portrait Records"** (Puzzles) - Mirélle's dates and
  the gate guardians' names (Dúnán the Patient, Cairn the Steadfast) - and
  LearnedGargoyleName is already set by then if they read that page.
* BUILT: the Brass House Token item and the locked right door (RightDoorCloseup,
  flag RightDoorUnlocked) - Mirélle just needs to give it. Originally planned: used on the
  brass plate of the Entrance Hall's right door (close-up: locked door with a
  round token slot instead of a keyhole; open version with the token seated).
* Journal: "The brass token" (Puzzles) - where it came from, which door it fits.

## Later payoff: the Old Portrait Studio

Mirélle does not come. Dusty, abandoned, virtually untouched. Rejected
canvases turned toward the wall, and one unfinished portrait - 1854. The
player can turn it around. It is not grotesque or corrupted: it is a
perfectly ordinary, honest portrait of Mirélle at that age. That is what she
could not recognise as herself - and the player now understands "They painted
a stranger and put my name beneath her."

Needed art: Mirélle's Parlour, Mirélle full-length (exists:
Assets/Characters/Sources/Mirelle_full.png), right-door close-up (locked/open),
Brass House Token, East Corridor, Old Portrait Studio, the 1854 canvas (back + front).
