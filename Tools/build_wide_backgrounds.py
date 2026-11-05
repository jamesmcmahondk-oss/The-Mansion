"""
Turns paintings of any shape into 1536x1024 scene backgrounds like the others.

The game shows only rows 192-832 of a background (the rest is off screen or
behind the menu).

WIDE paintings (16:9) are scaled to 1536 wide and placed so the visible band
shows the important part. SHIFT = how many (scaled) pixels of the painting's
top sit above the band - bigger shows more of the bottom.

FITTED paintings (tall, or where everything must show) are scaled so the BOX
(left, top, right, bottom, in source pixels) fits inside the visible band,
centred; the rest of the screen is filled with a dark, blurred copy.

The unseen top and bottom are filled with blurred edges.
Run from the repo root:  python Tools/build_wide_backgrounds.py
(needs Pillow, plus opencv-python-headless and numpy for the well)
"""
from PIL import Image, ImageDraw, ImageEnhance, ImageFilter

W, H = 1536, 1024
BAND_TOP, BAND_BOTTOM = 192, 832
RIGHT_SHIFT = 10          # right side of the Mansion: pixels moved up
SRC_DIR = "Assets/Backgrounds/Mansion/Sources"
OUT_DIR = "Assets/Backgrounds/Mansion"

# Courtyard, garden, statue, glasshouse, cross, graveyard entrance and
# gravestone close-ups now come from
# newer ChatGPT pictures (Nov 2026; statue and cross fitted by
# Tools/fit_background.py) - their old entries are removed so rerunning this
# doesn't put the old pictures back.
WIDE = [
    # source painting                    output background            SHIFT
    # (mansion_right.png now comes from mansion_right_v3_source.png - see the end)
    # (bench_closeup.png now comes from bench_closeup_dark.png - see the end.
    #  Older versions: bench_closeup_clean_source.png, bench_closeup_source.png)
    ("rear_yard_source.png",               "rear_yard.png",             110),
    ("kitchen_garden_source.png",          "kitchen_garden.png",        150),
    ("well_source.png",                    "well.png",                   60),
    # (well_closeup.png now comes from well_closeup_mirror.png - see the end)
]

FITTED = [
    # source painting                    output background            BOX
]


def wide(src, shift):
    scaled = src.resize((W, round(src.height * W / src.width)), Image.LANCZOS)
    out = scaled.resize((W, H), Image.LANCZOS).filter(ImageFilter.GaussianBlur(25))
    out.paste(scaled, (0, BAND_TOP - shift))
    return out


def fitted(src, box):
    left, top, right, bottom = box
    band_h = BAND_BOTTOM - BAND_TOP
    scale = min(W / (right - left), band_h / (bottom - top))

    # Background: the painting blown up to cover the screen, blurred and dark.
    cover = max(W / src.width, H / src.height)
    bg = src.resize((round(src.width * cover), round(src.height * cover)), Image.LANCZOS)
    bg = bg.crop(((bg.width - W) // 2, (bg.height - H) // 2,
                  (bg.width - W) // 2 + W, (bg.height - H) // 2 + H))
    bg = ImageEnhance.Brightness(bg.filter(ImageFilter.GaussianBlur(30))).enhance(0.6)

    img = src.resize((round(src.width * scale), round(src.height * scale)), Image.LANCZOS)
    x = round(W / 2 - (left + right) / 2 * scale)
    y = round((BAND_TOP + BAND_BOTTOM) / 2 - (top + bottom) / 2 * scale)

    # Soft edges so it melts into the background.
    mask = Image.new("L", img.size, 0)
    ImageDraw.Draw(mask).rectangle((30, 30, img.width - 31, img.height - 31), fill=255)
    bg.paste(img, (x, y), mask.filter(ImageFilter.GaussianBlur(18)))
    return bg


for name, dst, shift in WIDE:
    src = Image.open(f"{SRC_DIR}/{name}").convert("RGB")
    wide(src, shift).save(f"{OUT_DIR}/{dst}")
    print("wrote", dst)

for name, dst, box in FITTED:
    src = Image.open(f"{SRC_DIR}/{name}").convert("RGB")
    fitted(src, box).save(f"{OUT_DIR}/{dst}")
    print("wrote", dst)

# The well close-up with the mirror at the bottom. This painting is already in
# the final 1536x1024 layout; the "mirror taken" version paints the mirror out.
import cv2
import numpy as np

src = Image.open(f"{SRC_DIR}/well_closeup_mirror.png").convert("RGB")
# Shift it up 70 px so the mirror clears the menu's centre ornament.
src = wide(src, 192 + 70)
src.save(f"{OUT_DIR}/well_closeup.png")

bgr = cv2.cvtColor(np.asarray(src), cv2.COLOR_RGB2BGR)
mask = np.zeros(bgr.shape[:2], np.uint8)
cv2.ellipse(mask, (803, 728), (30, 22), 0, 0, 360, 255, -1)      # glass
cv2.line(mask, (815, 735), (832, 752), 255, 12)                  # handle
empty = cv2.inpaint(bgr, mask, 9, cv2.INPAINT_TELEA)
Image.fromarray(cv2.cvtColor(empty, cv2.COLOR_BGR2RGB)).save(f"{OUT_DIR}/well_closeup_empty.png")
print("wrote well_closeup.png, well_closeup_empty.png")

# The darker, moodier bench close-up. Already in the final 1536x1024 layout
# and lined up with the earlier version, so Ebria's sprite still fits.
Image.open(f"{SRC_DIR}/bench_closeup_dark.png").convert("RGB").save(f"{OUT_DIR}/bench_closeup.png")
print("wrote bench_closeup.png (dark)")

# The right side of the Mansion (rose walk). Shifted so the bench and roses
# sit in the visible band; the "rose cut" version paints one rose out.
src = Image.open(f"{SRC_DIR}/mansion_right_v3_source.png").convert("RGB")
right = wide(src, 192 + RIGHT_SHIFT)
right.save(f"{OUT_DIR}/mansion_right.png")

# One rose painted out (the one the player cuts with the garden shears).
bgr = cv2.cvtColor(np.asarray(right), cv2.COLOR_RGB2BGR)
mask = np.zeros(bgr.shape[:2], np.uint8)
cv2.ellipse(mask, (1322, 567), (26, 17), 0, 0, 360, 255, -1)
cut = cv2.inpaint(bgr, mask, 7, cv2.INPAINT_TELEA)
# Lay real leaves from just below over the smoothed gap so it isn't a smudge.
# Search nearby for the leafiest patch with the least red (no other rose).
best = None
for py in range(520, 660, 4):
    for px in range(1220, 1420, 4):
        if abs(py - 550) < 40 and abs(px - 1292) < 70:
            continue
        cand = bgr[py:py + 34, px:px + 60].astype(np.float32)
        red = np.clip(cand[..., 2] - cand[..., 1] - 30, 0, None).sum()
        green = cand[..., 1].mean()
        score = red - green * 20
        if best is None or score < best[0]:
            best = (score, py, px)
_, py, px = best
patch = bgr[py:py + 34, px:px + 60].astype(np.float32)
soft = cv2.GaussianBlur(mask[550:584, 1292:1352].astype(np.float32) / 255, (0, 0), 4)[..., None]
region = cut[550:584, 1292:1352].astype(np.float32)
cut[550:584, 1292:1352] = (patch * soft + region * (1 - soft)).astype(np.uint8)
Image.fromarray(cv2.cvtColor(cut, cv2.COLOR_BGR2RGB)).save(f"{OUT_DIR}/mansion_right_rose_cut.png")
print("wrote mansion_right.png, mansion_right_rose_cut.png")
