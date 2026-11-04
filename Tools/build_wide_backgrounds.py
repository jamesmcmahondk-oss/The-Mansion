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
SRC_DIR = "Assets/Backgrounds/Mansion/Sources"
OUT_DIR = "Assets/Backgrounds/Mansion"

WIDE = [
    # source painting                    output background            SHIFT
    ("courtyard_source.png",               "courtyard.png",              70),
    ("garden_source.png",                  "garden.png",                110),
    ("statue_closeup_source.png",          "statue_closeup.png",         10),
    # After the sword is taken (lined up by Tools/align_variant.py).
    ("statue_closeup_nosword_source.png",  "statue_closeup_nosword.png", 10),
    ("mansion_right_v2_source.png",        "mansion_right.png",         215),
    ("glasshouse_closeup_source.png",      "glasshouse_closeup.png",    230),
    # (bench_closeup.png now comes from bench_closeup_dark.png - see the end.
    #  Older versions: bench_closeup_clean_source.png, bench_closeup_source.png)
    ("rear_yard_source.png",               "rear_yard.png",             110),
    ("kitchen_garden_source.png",          "kitchen_garden.png",        150),
    ("well_source.png",                    "well.png",                   60),
    ("graveyard_entrance_source.png",      "graveyard_entrance.png",    110),
    # (well_closeup.png now comes from well_closeup_mirror.png - see the end)
    ("gravestone_closeup_source.png",      "gravestone_closeup.png",     20),
]

FITTED = [
    # source painting                    output background            BOX
    ("cross_closeup_source.png",           "cross_closeup.png",         (120, 25, 1420, 905)),
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
