"""
Builds the inventory item icons in Assets/Items/ (128x128 PNG) by cropping
existing scene art. Run from the repo root:  python Tools/build_item_icons.py
Requires Pillow and numpy.

To add an icon: add a line to ICONS with the source image and the crop box
(left, top, right, bottom) in that image's pixels, then run the script and
point the item's Icon at the new file in Data/Items.cs.
"""
import os
import numpy as np
from PIL import Image, ImageDraw, ImageEnhance, ImageFilter

SIZE = 128
OUT_DIR = "Assets/Items"

ICONS = {
    "gold_coins.png":        ("Assets/Backgrounds/Mansion/fountain_closeup.png", (600, 640, 880, 840)),
    "royal_gold_coin.png":   ("Assets/Backgrounds/Mansion/fountain_closeup.png", (670, 750, 726, 802)),
    "gargoyle_blessing.png": ("Assets/Backgrounds/Mansion/FrontGargoyle.png",    (800, 260, 1140, 600)),

    # Car trunk (pre-game choice)
    "corkscrew.png":         ("Assets/UI/trunk.png", (380, 440, 585, 600)),
    "lighter.png":           ("Assets/UI/trunk.png", (270, 565, 445, 705)),
    "ball_of_string.png":    ("Assets/UI/trunk.png", (630, 505, 790, 670)),
    "party_hat.png":         ("Assets/UI/trunk.png", (850, 325, 1095, 530)),
    "loose_change.png":      ("Assets/UI/trunk.png", (850, 575, 1025, 670)),
    "fork.png":              ("Assets/UI/trunk.png", (1050, 505, 1260, 745)),
    # Item pictures (whole picture, squared to its centre)
    "worn_photograph.png":   ("Assets/Items/Sources/worn_photograph.png",     (230, 0, 1306, 1024)),
    "red_rose.png":          ("Assets/Items/Sources/red_rose.png",            (90, 60, 1190, 1160)),
    "cracked_hand_mirror.png": ("Assets/Items/Sources/cracked_hand_mirror.png", (180, 150, 1060, 1110)),
    "empty_bottle.png":      ("Assets/UI/trunk.png", (1180, 395, 1465, 705)),

    # Outside the Mansion
    "garden_shears.png":     ("Assets/Backgrounds/Mansion/tool_shed_closeup.png", (480, 490, 755, 595)),
    "charcoal.png":          ("Assets/Backgrounds/Mansion/tool_shed_closeup.png", (865, 495, 1050, 590)),
    "rope.png":              ("Assets/Backgrounds/Mansion/rear_yard.png",         (1168, 388, 1234, 496)),
    # Tall and thin: cut out exactly, laid diagonally (third value = degrees,
    # fourth = brightness - the statue is in deep shadow).
    "stone_sword.png":       ("Assets/Backgrounds/Mansion/statue_closeup.png",    (822, 455, 866, 822), -45, 1.9),
}


def make_icon(src_path, box, angle=0, brightness=1.0):
    img = Image.open(src_path).convert("RGB")

    if brightness != 1.0:
        img = ImageEnhance.Brightness(img).enhance(brightness)

    if angle:
        # Cut the box out exactly and turn it, on a dark background.
        img = img.crop(box).rotate(angle, resample=Image.BICUBIC, expand=True,
                                   fillcolor=(14, 11, 10))
        box = (0, 0, img.width, img.height)

    left, top, right, bottom = box
    # Square the crop around its centre.
    side = max(right - left, bottom - top)
    cx, cy = (left + right) / 2, (top + bottom) / 2
    box = (round(cx - side / 2), round(cy - side / 2),
           round(cx + side / 2), round(cy + side / 2))
    icon = img.crop(box).resize((SIZE, SIZE), Image.LANCZOS)

    # Soft dark vignette so icons sit nicely in the brass slots.
    y, x = np.mgrid[0:SIZE, 0:SIZE]
    r = np.hypot(x - SIZE / 2, y - SIZE / 2) / (SIZE / 2)
    shade = np.clip(1.15 - 0.55 * r ** 2, 0.35, 1.0)[..., None]
    arr = np.asarray(icon).astype(np.float32) * shade
    icon = Image.fromarray(np.clip(arr, 0, 255).astype(np.uint8)).convert("RGBA")

    # Rounded corners.
    mask = Image.new("L", (SIZE, SIZE), 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, SIZE - 1, SIZE - 1), 14, fill=255)
    icon.putalpha(mask.filter(ImageFilter.GaussianBlur(0.8)))
    return icon


os.makedirs(OUT_DIR, exist_ok=True)
for name, (src, box, *angle) in ICONS.items():
    make_icon(src, box, *angle).save(os.path.join(OUT_DIR, name))
    print("wrote", os.path.join(OUT_DIR, name))
