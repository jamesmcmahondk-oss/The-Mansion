"""
Turns the 16:9 courtyard painting into a 1536x1024 scene background
(Assets/Backgrounds/Mansion/courtyard.png) like the other scenes.

The game shows only rows 192-832 of a background (the rest is off screen or
behind the menu). The painting is scaled to 1536 wide and placed so that
band shows the door, the steps and the fountain; the unseen top and bottom
are filled with blurred, stretched edges.

Run from the repo root:  python Tools/build_courtyard.py
"""
from PIL import Image, ImageFilter

SRC = "Assets/Backgrounds/Mansion/Sources/courtyard_source.png"
DST = "Assets/Backgrounds/Mansion/courtyard.png"
W, H = 1536, 1024
VISIBLE_TOP = 192
SHIFT = 70            # how many (scaled) pixels of the painting's top sit above the visible band

src = Image.open(SRC).convert("RGB")
scaled = src.resize((W, round(src.height * W / src.width)), Image.LANCZOS)
top = VISIBLE_TOP - SHIFT

# Unseen areas: stretch the painting's edges and blur them.
out = scaled.resize((W, H), Image.LANCZOS).filter(ImageFilter.GaussianBlur(25))
out.paste(scaled, (0, top))
out.save(DST)
print("painting placed at y =", top, "to", top + scaled.height, "(visible 192-832)")
