"""
Builds Assets/UI/hud_frame.png (exactly 1920x400) from Assets/UI/menu_frame.png.

menu_frame.png is the 2012x782 frame art with transparent surroundings
(made by Tools/cut_menu_frame.py). The game's HUD needs it 1920x400 with the
two side pockets exactly where the verb buttons and the stats panel sit
(MainWindow.xaml):

    left pocket  x 59-234,   right pocket x 1683-1859,   both y 128-373

Squashing the art into that would distort every ornament. Instead:

  * everything is scaled evenly (x SCALE), so gargoyles, skulls, pillars,
    scrolls and the gem keep their shape;
  * the extra width is made by inserting plain columns: inside each pocket
    (between the gargoyle's claws and the scroll) and on the plain bar each
    side of the gem, where the panel's own texture is cross-faded in;
  * the extra height is made by stretching only the plain middle band
    (pillar shafts, pockets, panel, hanging ivy).

Run from the repo root:  python Tools/build_hud_frame.py   (Pillow + numpy)
"""
import numpy as np
from PIL import Image

SRC = "Assets/UI/menu_frame.png"
DST = "Assets/UI/hud_frame.png"

OUT_W, OUT_H = 1920, 400
SCALE = 0.461                    # source pixels -> HUD pixels for the ornaments

src = Image.open(SRC).convert("RGBA")
a = np.asarray(src).astype(np.float32)
SH, SW = a.shape[:2]             # 782 x 2012

WORK_W = round(OUT_W / SCALE)    # width at source scale
EXTRA = WORK_W - SW

# Where to insert, and how many columns (source pixels). The pocket inserts
# go in a step: above STEP_ROW right of the gargoyle's head, below it between
# its claws and the scroll (no single column misses both).
STEP_ROW = 170
POCKET_L_AT_TOP, POCKET_L_AT = 290, 246
POCKET_R_AT_TOP, POCKET_R_AT = SW - 290, SW - 246
POCKET_L = 164
MID_L_AT, MID_R_AT = 650, 1300           # plain bar each side of the gem
MID_L = 913                              # keeps the gem centred
MID_R_AND_POCKET_R = EXTRA - POCKET_L - MID_L
POCKET_R = 169
MID_R = MID_R_AND_POCKET_R - POCKET_R

PLAIN_BAR = (1300, 1338)        # a plain stretch of the top/bottom bar
SEAM_ROW = 240                  # pocket inserts: bar top from PLAIN_BAR above this,
                                # the pocket's own columns below
POCKET_OWN = {"L": (255, 300), "R": (SW - 300, SW - 255)}

TOP, BOT = 266, 696             # rows of the middle panel's texture
TEX_CHUNKS = [(440, 950), (1060, 1570)]   # texture away from the gem's pendant
BLEND = 40

# Vertical: rows [BAND0, BAND1) are stretched so the pockets end at y 373.
BAND0, BAND1 = 330, 620
POCKET_TOP_ROW, POCKET_BOTTOM_ROW = 266, 698
TARGET_POCKET_BOTTOM = 373


def stretch(block, new_w):
    """Resize a block of columns (all rows) to new_w columns."""
    h = block.shape[0]
    img = Image.fromarray(np.clip(block, 0, 255).astype(np.uint8), "RGBA")
    return np.asarray(img.resize((new_w, h), Image.LANCZOS)).astype(np.float32)


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


def blend_edges(block, left_cols, right_cols, rows):
    """Cross-fade the block's ends into the real neighbouring columns."""
    w = np.linspace(1, 0, BLEND)[None, :, None]
    r0, r1 = rows
    block[r0:r1, :BLEND] = left_cols[r0:r1] * w + block[r0:r1, :BLEND] * (1 - w)
    block[r0:r1, -BLEND:] = right_cols[r0:r1] * w[:, ::-1] + block[r0:r1, -BLEND:] * (1 - w[:, ::-1])
    return block


def middle_insert(at, n, chunks):
    block = stretch(a[:, PLAIN_BAR[0]:PLAIN_BAR[1]], n)
    block[TOP:BOT] = texture(n, chunks)
    return blend_edges(block, a[:, at - BLEND:at], a[:, at:at + BLEND], (TOP, BOT))


def pocket_insert(at, n, side):
    upper = stretch(a[:, PLAIN_BAR[0]:PLAIN_BAR[1]], n)
    own = stretch(a[:, POCKET_OWN[side][0]:POCKET_OWN[side][1]], n)
    block = own.copy()
    block[:SEAM_ROW] = upper[:SEAM_ROW]
    # Soften the join between the two sources.
    for i in range(8):
        t = (i + 1) / 9
        r = SEAM_ROW + i
        block[r] = upper[r] * (1 - t) + own[r] * t
    return block


pocket_l = pocket_insert(POCKET_L_AT, POCKET_L, "L")
pocket_r = pocket_insert(POCKET_R_AT, POCKET_R, "R")
mid_l = middle_insert(MID_L_AT, MID_L, TEX_CHUNKS)
mid_r = middle_insert(MID_R_AT, MID_R, TEX_CHUNKS[::-1])


def widen(rows, pocket_l_at, pocket_r_at):
    """The given rows with all four inserts placed."""
    r0, r1 = rows
    inserts = [(pocket_l_at, pocket_l), (MID_L_AT, mid_l), (MID_R_AT, mid_r), (pocket_r_at, pocket_r)]
    parts, x = [], 0
    for at, block in inserts:
        parts += [a[r0:r1, x:at], block[r0:r1]]
        x = at
    parts.append(a[r0:r1, x:])
    return np.concatenate(parts, axis=1)


wide = np.concatenate([
    widen((0, STEP_ROW), POCKET_L_AT_TOP, POCKET_R_AT_TOP),
    widen((STEP_ROW, SH), POCKET_L_AT, POCKET_R_AT),
], axis=0)
assert wide.shape[1] == WORK_W, (wide.shape, WORK_W)

# Vertical: stretch only the plain band so the pockets end where the HUD
# expects (the rows below the band keep the even scale).
band_target = round(TARGET_POCKET_BOTTOM / SCALE
                    - (POCKET_BOTTOM_ROW - BAND1) - BAND0)
band = Image.fromarray(np.clip(wide[BAND0:BAND1], 0, 255).astype(np.uint8), "RGBA")
band = np.asarray(band.resize((WORK_W, band_target), Image.LANCZOS)).astype(np.float32)
tall = np.concatenate([wide[:BAND0], band, wide[BAND1:]], axis=0)

img = Image.fromarray(np.clip(tall, 0, 255).astype(np.uint8), "RGBA")
img = img.resize((OUT_W, round(tall.shape[0] * SCALE)), Image.LANCZOS)
img = img.crop((0, 0, OUT_W, OUT_H))
img.save(DST)
print(f"wrote {DST} {img.size}")
