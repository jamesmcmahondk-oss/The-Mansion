"""
Turns 16:9 paintings into 1536x1024 scene backgrounds like the others.

The game shows only rows 192-832 of a background (the rest is off screen or
behind the menu). Each painting is scaled to 1536 wide and placed so that
band shows the important part; the unseen top and bottom are filled with
blurred, stretched edges.

SHIFT = how many (scaled) pixels of the painting's top sit above the visible
band - bigger shows more of the bottom of the painting.

Run from the repo root:  python Tools/build_wide_backgrounds.py
"""
from PIL import Image, ImageFilter

W, H = 1536, 1024
VISIBLE_TOP = 192

BACKGROUNDS = [
    # source painting (in Sources/)        output background       SHIFT
    ("courtyard_source.png",               "courtyard.png",          70),
    ("garden_source.png",                  "garden.png",            110),
    ("statue_closeup_source.png",          "statue_closeup.png",     10),
]

for src_name, dst_name, shift in BACKGROUNDS:
    src = Image.open(f"Assets/Backgrounds/Mansion/Sources/{src_name}").convert("RGB")
    scaled = src.resize((W, round(src.height * W / src.width)), Image.LANCZOS)
    top = VISIBLE_TOP - shift

    out = scaled.resize((W, H), Image.LANCZOS).filter(ImageFilter.GaussianBlur(25))
    out.paste(scaled, (0, top))
    out.save(f"Assets/Backgrounds/Mansion/{dst_name}")
    print(f"{dst_name}: painting at y={top}..{top + scaled.height} (visible 192-832)")
