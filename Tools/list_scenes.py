"""
Writes SCENES.md: every scene and close-up with its objects, characters,
items, exits and how to reach it - read straight from Data/Scenes.cs,
Data/Dialogues.cs and Data/Enemies.cs.

Run from the repo root after changing scenes:   python Tools/list_scenes.py
"""
import re

SCENES = open("Data/Scenes.cs", encoding="utf-8-sig").read()
DIALOGUES = open("Data/Dialogues.cs", encoding="utf-8-sig").read()
ENEMIES = open("Data/Enemies.cs", encoding="utf-8-sig").read()


def strip_comments(src):
    out, i, n = [], 0, len(src)
    while i < n:
        if src.startswith("//", i):
            i = src.find("\n", i)
            i = n if i < 0 else i
        elif src.startswith("/*", i):
            i = src.find("*/", i) + 2
        elif src[i] == '"':
            j = i + 1
            while src[j] != '"':
                j += 2 if src[j] == "\\" else 1
            out.append(src[i:j + 1]); i = j + 1
        else:
            out.append(src[i]); i += 1
    return "".join(out)


def call_args(src, pos):
    """Text inside the parentheses that start at src[pos] == '('."""
    depth, i = 0, pos
    while True:
        c = src[i]
        if c == '"':
            i += 1
            while src[i] != '"':
                i += 2 if src[i] == "\\" else 1
        elif c == "(":
            depth += 1
        elif c == ")":
            depth -= 1
            if depth == 0:
                return src[pos + 1:i]
        i += 1


def strings(args):
    """All string literals in args, joined where they're concatenated with +."""
    parts = re.findall(r'"((?:[^"\\]|\\.)*)"', args)
    return [p.replace('\\"', '"') for p in parts]


def first_text(args):
    # The leading string expression (literals joined by +), before any named argument.
    head = re.split(r",\s*(?:\w+:|p\s*=>)", args)[0]
    return "".join(strings(head))


def calls(src, name):
    for m in re.finditer(r"\." + name + r"\s*\(", src):
        yield call_args(src, m.end() - 1)


SC = strip_comments(SCENES)
speakers = dict(re.findall(r'NewDialogue\(\s*"(\w+)",\s*"([^"]+)"', DIALOGUES))
enemies = dict(re.findall(r'NewEnemy\(\s*"(\w+)",\s*"([^"]+)"', strip_comments(ENEMIES)))

# Split into scenes.
scene_starts = [(m.start(), m) for m in re.finditer(r"NewScene\(", SC)]
scenes = []
for k, (pos, m) in enumerate(scene_starts):
    end = scene_starts[k + 1][0] if k + 1 < len(scene_starts) else len(SC)
    body = SC[pos:end]
    args = call_args(SC, m.end() - 1)
    lits = strings(args)
    if len(lits) < 3:
        continue
    name, bg, desc = lits[0], lits[1], "".join(lits[2:])
    scenes.append(dict(name=name, bg=bg, desc=desc, body=body))

closeup_targets = set()
for s in scenes:
    closeup_targets |= set(strings("".join(calls(s["body"], "CloseUp"))))


def tidy(cond):
    """'p => !p.HasFlag("X")' -> 'not X'; 'p => p.HasFlag("X")' -> 'X'."""
    c = " ".join(cond.split())
    c = re.sub(r"^p\s*=>\s*", "", c)
    c = re.sub(r'!p\.HasFlag\("(\w+)"\)', r"not \1", c)
    c = re.sub(r'p\.HasFlag\("(\w+)"\)', r"\1", c)
    c = re.sub(r'!p\.Has\("([^"]+)"\)', r"without \1", c)
    c = re.sub(r'p\.Has\("([^"]+)"\)', r"has \1", c)
    return c.replace("&&", "and").replace("||", "or")


def parse_hotspots(body):
    marks = [m for m in re.finditer(r"\.(Hotspot|ReturnTo)\s*\(", body)]
    out = []
    for k, m in enumerate(marks):
        end = marks[k + 1].start() if k + 1 < len(marks) else len(body)
        chunk = body[m.start():end]
        args = call_args(body, m.end() - 1)
        if m.group(1) == "ReturnTo":
            lits = strings(args)
            out.append(dict(name="Return arrow", ret=lits[0], retText="".join(lits[1:])))
            continue
        h = dict(name=strings(args)[0])
        for verb in ["Describe", "GoesTo", "CloseUp", "TalkStarts", "ActStarts", "ActStartsFight", "OnTalk"]:
            got = list(calls(chunk, verb))
            if got:
                h[verb] = first_text(got[0])
        acts = list(calls(chunk, "OnAct"))
        if acts:
            a = acts[0]
            text = first_text(a)
            h["OnAct"] = text if text and not a.strip().startswith("p =>") else "(depends on a dice roll / the story - see Data/Scenes.cs)"
            if "Health" in a:
                h["OnAct"] += "  [changes Health]"
            if "Give(" in a:
                h["OnAct"] += "  [gives " + ", ".join(re.findall(r'Give\("([^"]+)"\)', a)) + "]"
        items = []
        for a in calls(chunk, "OnItem"):
            lits = strings(a)
            extra = []
            if "removeItem: true" in a: extra.append("item used up")
            if "Give(" in a: extra.append("gives " + ", ".join(re.findall(r'Give\("([^"]+)"\)', a)))
            if "SetFlag(" in a: extra.append("sets " + ", ".join(re.findall(r'SetFlag\("(\w+)"\)', a)))
            items.append((lits[0], "".join(lits[1:2]), extra))
        if items:
            h["items"] = items
        when = list(calls(chunk, "OnlyWhen"))
        if when:
            h["OnlyWhen"] = tidy(when[0])
        flags = sorted(set(re.findall(r'SetFlag\("(\w+)"\)', "".join(acts))))
        if flags:
            h["sets"] = flags
        out.append(h)
    return out


for s in scenes:
    s["hotspots"] = parse_hotspots(s["body"])
    s["sprites"] = [strings(a)[0] for a in calls(s["body"], "Sprite")]
    s["backgrounds"] = [(tidy(a.rsplit(",", 1)[0]), [x for x in strings(a) if x.endswith((".png", ".jpg"))][-1])
                        for a in calls(s["body"], "BackgroundWhen")]

by_name = {s["name"]: s for s in scenes}

# Incoming links.
incoming = {s["name"]: [] for s in scenes}
for s in scenes:
    for h in s["hotspots"]:
        for key, how in [("GoesTo", "ACT"), ("CloseUp", "LOOK")]:
            t = h.get(key)
            if t:
                incoming.setdefault(t, []).append(f"{s['name']} – {how} on *{h['name']}*")
        if h.get("ret"):
            incoming.setdefault(h["ret"], []).append(f"{s['name']} – Return arrow")


def kind(name):
    return "Close-up" if name in closeup_targets else "Scene"


lines = ["# The Mansion – Scenes and Close-ups", "",
         "Generated from `Data/Scenes.cs` by `Tools/list_scenes.py` – re-run it after",
         "changing scenes. **LOOK** opens close-ups, **ACT** moves on, the Return arrow",
         "works with any verb.", "", "## Map", "", "```mermaid", "flowchart LR"]
for s in scenes:
    shape = f'{s["name"]}(["{s["name"]}"])' if kind(s["name"]) == "Close-up" else f'{s["name"]}["{s["name"]}"]'
    lines.append(f"    {shape}")
missing = set()
for s in scenes:
    for h in s["hotspots"]:
        if h.get("GoesTo"):
            lines.append(f'    {s["name"]} -->|ACT {h["name"]}| {h["GoesTo"]}')
            if h["GoesTo"] not in by_name: missing.add(h["GoesTo"])
        if h.get("CloseUp"):
            lines.append(f'    {s["name"]} -.->|LOOK {h["name"]}| {h["CloseUp"]}')
for m in sorted(missing):
    lines.append(f'    {m}{{{{"{m} – not made yet"}}}}')
lines += ["```", "", "Rounded boxes are close-ups. Dotted arrows = LOOK, solid = ACT.", ""]

# Table of contents
lines += ["## Contents", ""]
for s in scenes:
    lines.append(f"- [{s['name']}](#{s['name'].lower()}) – {kind(s['name'])}")
lines.append("")

for s in scenes:
    lines += [f"## {s['name']}", "", f"**{kind(s['name'])}** · `{s['bg']}`", "", f"> {s['desc']}", ""]
    for cond, img in s["backgrounds"]:
        lines.append(f"- **Picture changes** to `{img}` when **{cond}**")
    for sp in s["sprites"]:
        lines.append(f"- **In the scene:** `{sp}`")
    reach = incoming.get(s["name"], [])
    lines.append("- **Reached from:** " + ("; ".join(reach) if reach else "– (start / debug menu only)"))

    exits = [f"{h['GoesTo']} (ACT *{h['name']}*)" for h in s["hotspots"] if h.get("GoesTo")]
    ups = [f"{h['CloseUp']} (LOOK *{h['name']}*)" for h in s["hotspots"] if h.get("CloseUp")]
    ret = [h["ret"] for h in s["hotspots"] if h.get("ret")]
    if exits: lines.append("- **Exits:** " + "; ".join(exits))
    if ups: lines.append("- **Close-ups:** " + "; ".join(ups))
    if ret: lines.append(f"- **Return arrow →** {ret[0]}")

    people = []
    for h in s["hotspots"]:
        for key, verb in [("TalkStarts", "TALK"), ("ActStarts", "ACT")]:
            if h.get(key):
                people.append(f"{speakers.get(h[key], h[key])} ({verb} *{h['name']}*, conversation `{h[key]}`)")
        if h.get("ActStartsFight"):
            people.append(f"{enemies.get(h['ActStartsFight'], h['ActStartsFight'])} (fight `{h['ActStartsFight']}`)")
    if people:
        lines.append("- **Characters:** " + "; ".join(people))
    lines += ["", "| Object | LOOK | ACT | TALK | Items |", "|---|---|---|---|---|"]

    def cell(t):
        return (t or "–").replace("|", "\\|")

    rows, counts = [], {}
    for h in s["hotspots"]:
        if h.get("ret"):
            continue
        name = h["name"] + (f" *(only when {h['OnlyWhen']})*" if h.get("OnlyWhen") else "")
        look = f"→ **{h['CloseUp']}**" if h.get("CloseUp") else h.get("Describe")
        act = (f"conversation `{h['ActStarts']}`" if h.get("ActStarts") else
               f"fight `{h['ActStartsFight']}`" if h.get("ActStartsFight") else
               h.get("OnAct") or (f"→ **{h['GoesTo']}**" if h.get("GoesTo") else None))
        if h.get("sets"):
            act = (act or "") + f"  [sets {', '.join(h['sets'])}]"
        talk = f"conversation `{h['TalkStarts']}`" if h.get("TalkStarts") else h.get("OnTalk")
        items = "<br>".join(f"**{i}**: {t}" + (f" ({'; '.join(x)})" if x else "") for i, t, x in h.get("items", []))
        row = (name, look, act, talk, items)
        if row not in counts:
            rows.append(row)
        counts[row] = counts.get(row, 0) + 1

    # The same object in several spots (e.g. a lantern on each gatepost) is listed once.
    for row in rows:
        name, look, act, talk, items = row
        if counts[row] > 1:
            name += f" ({counts[row]} places)"
        lines.append(f"| {cell(name)} | {cell(look)} | {cell(act)} | {cell(talk)} | {cell(items)} |")
    lines.append("")

open("SCENES.md", "w", encoding="utf-8").write("\n".join(lines))
print(f"wrote SCENES.md: {len(scenes)} scenes, {sum(len(s['hotspots']) for s in scenes)} hotspots")
