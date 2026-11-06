"""
Builds the game's mouse cursors (Assets/Cursors/*.cur) from the cursor
sheet (Assets/UI/Sources/cursors.webp, 7 x 2, transparent) and the walk
cursor picture (Assets/UI/Sources/cursor_walk.webp).

The sheets have transparent backgrounds (soft shadows and glows are
partly see-through); each cursor is trimmed, scaled to SIZE px and saved as a
32-bit .cur with its hotspot (the click point) at the tip.
Usage: python Tools/build_cursors.py   (Pillow, numpy)
"""
import struct
import numpy as np
from PIL import Image

SIZE = 48
COLS = [195, 460, 730, 995, 1260, 1520, 1785]
ROWS = [(50, 228), (318, 505)]
CELL_W = 250

# name: (row, column, hotspot)  hotspot "tip" = the top-left-most point
CURSORS = {
    "cursor_arrow": (0, 0, "tip"),
    "cursor_talk": (0, 6, "tip"),
    "cursor_act": (1, 0, "tip"),
    "cursor_item": (1, 5, "tip"),
    "cursor_look": (1, 6, (0.38, 0.38)),
}


def unwhite(img):
    """The sheets already have transparent backgrounds; just use them."""
    return img.convert("RGBA")


def fit(img):
    box = img.getchannel("A").point(lambda v: 255 if v > 40 else 0).getbbox()
    img = img.crop(box)
    side = max(img.size)
    sq = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    sq.paste(img, (0, 0))                      # keep the tip at the top-left
    return sq.resize((SIZE, SIZE), Image.LANCZOS)


def hotspot(img, spec):
    if spec != "tip":
        return int(spec[0] * SIZE), int(spec[1] * SIZE)
    a = np.asarray(img.getchannel("A")) > 128
    ys, xs = np.nonzero(a)
    i = np.argmin(xs + ys)                     # the point nearest the top-left corner
    return int(xs[i]), int(ys[i])


def write_cur(img, hot, path):
    w, h = img.size
    px = np.asarray(img)[::-1, :, [2, 1, 0, 3]].tobytes()      # BGRA, bottom-up
    mask = bytes(((w + 31) // 32) * 4 * h)
    header = struct.pack("<IiiHHIIiiII", 40, w, h * 2, 1, 32, 0, len(px) + len(mask), 0, 0, 0, 0)
    data = header + px + mask
    with open(path, "wb") as f:
        f.write(struct.pack("<HHH", 0, 2, 1))
        f.write(struct.pack("<BBBBHHII", w, h, 0, 0, hot[0], hot[1], len(data), 22))
        f.write(data)


sheet = Image.open("Assets/UI/Sources/cursors.webp")
for name, (r, c, spec) in CURSORS.items():
    y0, y1 = ROWS[r]
    cell = sheet.crop((COLS[c] - CELL_W // 2, y0, COLS[c] + CELL_W // 2, y1))
    img = fit(unwhite(cell))
    write_cur(img, hotspot(img, spec), f"Assets/Cursors/{name}.cur")
    img.save(f"Assets/Cursors/{name}.png")
    print("wrote", name, hotspot(img, spec))

walk = fit(unwhite(Image.open("Assets/UI/Sources/cursor_walk.webp")))
write_cur(walk, hotspot(walk, "tip"), "Assets/Cursors/cursor_walk.cur")
walk.save("Assets/Cursors/cursor_walk.png")
print("wrote cursor_walk", hotspot(walk, "tip"))
