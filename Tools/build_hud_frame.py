"""
Builds Assets/UI/hud_frame.png (exactly 1920x400) from Assets/UI/menu_frame.png.

menu_frame.png is 1536x596. Squashing it straight into 1920x400 distorts
every ornament (1.25x wider, 0.67x taller). Instead this script scales the
art uniformly (x0.671) and makes up the extra width by widening only the
plain parts:

  * the two side pockets get wider (so the menu buttons fit at their
    natural proportions) by stretching the short bar section above them;
  * the middle text panel gets wider by stretching its plain top/bottom
    molding and filling the inside with more of the panel's own texture,
    cross-faded so there are no visible seams.

Skulls, gargoyles, pillars, scrolls and the centre gem keep their exact
shape. Run from the repo root:  python Tools/build_hud_frame.py
Requires Pillow and numpy.
"""
import numpy as np
from PIL import Image

SRC = "Assets/UI/menu_frame.png"
DST = "Assets/UI/hud_frame.png"

OUT_W, OUT_H = 1920, 400
src = Image.open(SRC).convert("RGBA")
SW, SH = src.size                      # 1536 x 596
work_w = round(OUT_W * SH / OUT_H)     # width at source scale (2861)
extra = work_w - SW

POCKET_EXTRA = 76                      # source px added to each side pocket
mid_extra = extra - 2 * POCKET_EXTRA
MID_EXTRA_L = mid_extra // 2 + mid_extra % 2
MID_EXTRA_R = mid_extra // 2

# Rows of the middle panel: [0, TOP) top molding, [TOP, BOT) texture, [BOT, SH) bottom molding
TOP, BOT = 210, 540
# Texture areas inside the middle panel that are safe to sample from
# (the gap between them is where the centre gem's pendant hangs down).
TEX_CHUNKS = [(345, 735), (805, 1190)]
BLEND = 48                             # cross-fade width at each seam

a = np.asarray(src).astype(np.float32)


def keep(x0, x1):
    return a[:, x0:x1]


def stretch(x0, x1, new_w):
    piece = src.crop((x0, 0, x1, SH)).resize((new_w, SH), Image.LANCZOS)
    return np.asarray(piece).astype(np.float32)


def texture(new_w, chunks):
    """new_w columns of panel texture, joining chunks with cross-fades."""
    tex = a[TOP:BOT, chunks[0][0]:chunks[0][1]]
    i = 1
    while tex.shape[1] < new_w:
        c0, c1 = chunks[i % len(chunks)]
        nxt = a[TOP:BOT, c0:c1]
        w = np.linspace(1, 0, BLEND)[None, :, None]
        seam = tex[:, -BLEND:] * w + nxt[:, :BLEND] * (1 - w)
        tex = np.concatenate([tex[:, :-BLEND], seam, nxt[:, BLEND:]], axis=1)
        i += 1
    return tex[:, :new_w].copy()


def plain_zone(x0, x1, new_w, chunks):
    """Widen a plain section of the middle panel to new_w columns."""
    out = stretch(x0, x1, new_w)                       # molding rows
    tex = texture(new_w, chunks)
    # Cross-fade into the real neighbouring columns at both ends.
    left = a[TOP:BOT, x0:x0 + BLEND]
    right = a[TOP:BOT, x1 - BLEND:x1]
    w = np.linspace(1, 0, BLEND)[None, :, None]
    tex[:, :BLEND] = left * w + tex[:, :BLEND] * (1 - w)
    tex[:, -BLEND:] = right * w[:, ::-1] + tex[:, -BLEND:] * (1 - w[:, ::-1])
    out[TOP:BOT] = tex
    return out


parts = [
    keep(0, 180),                                   # left gargoyle + edge
    stretch(180, 265, 85 + POCKET_EXTRA),           # bar over left pocket
    keep(265, 420),                                 # left skull pillar + scroll
    plain_zone(420, 590, 170 + MID_EXTRA_L, TEX_CHUNKS),
    keep(590, 948),                                 # centre gem ornament
    plain_zone(948, 1117, 169 + MID_EXTRA_R, TEX_CHUNKS[::-1]),
    keep(1117, 1272),                               # right scroll + skull pillar
    stretch(1272, 1350, 78 + POCKET_EXTRA),         # bar over right pocket
    keep(1350, SW),                                 # right gargoyle + edge
]
wide = np.concatenate(parts, axis=1)
assert wide.shape[1] == work_w, wide.shape

img = Image.fromarray(np.clip(wide, 0, 255).astype(np.uint8), "RGBA")
img = img.resize((OUT_W, OUT_H), Image.LANCZOS)
img.save(DST)
print(f"wrote {DST} {img.size}")
