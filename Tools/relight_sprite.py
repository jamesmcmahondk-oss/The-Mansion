"""
Re-lights a sprite to match a new, darker/lighter version of its background.

    python Tools/relight_sprite.py <sprite> <old background> <new background> X Y HEIGHT <out>

X/Y/HEIGHT are where the sprite sits (background pixels, as in .Sprite(...)).
Compares the two backgrounds in a band around the sprite, per colour channel,
and darkens/tints the sprite part of the way, so it sits in the new
lighting without losing its colour. Needs Pillow and numpy.
"""
import sys
import numpy as np
from PIL import Image

sprite_path, old_path, new_path = sys.argv[1:4]
x, y, height = (float(v) for v in sys.argv[4:7])
out_path = sys.argv[7]

sprite = Image.open(sprite_path).convert("RGBA")
old = np.asarray(Image.open(old_path).convert("RGB"), np.float32)
new = np.asarray(Image.open(new_path).convert("RGB"), np.float32)

width = height * sprite.width / sprite.height
pad = 120
x0, y0 = int(max(0, x - pad)), int(max(0, y - pad))
x1, y1 = int(min(old.shape[1], x + width + pad)), int(min(old.shape[0], y + height + pad))

# Leave out the sprite's own area.
mask = np.ones((y1 - y0, x1 - x0), bool)
mask[int(y - y0):int(y - y0 + height), int(x - x0):int(x - x0 + width)] = False

old_mean = old[y0:y1, x0:x1][mask].mean(axis=0)
new_mean = new[y0:y1, x0:x1][mask].mean(axis=0)
raw = (new_mean + 1) / (old_mean + 1)

# Follow the overall brightness change only partly (STRENGTH), and the colour
# change only a little (TINT), so skin and hair keep their warmth.
STRENGTH, TINT = 0.55, 0.2
brightness = float(np.exp(np.log(raw).mean()))
gain = brightness ** STRENGTH * (raw / brightness) ** TINT
print("background change (R, G, B):", np.round(raw, 3), "-> sprite gain:", np.round(gain, 3))

a = np.asarray(sprite).astype(np.float32)
a[..., :3] = np.clip(a[..., :3] * gain, 0, 255)
Image.fromarray(a.astype(np.uint8), "RGBA").save(out_path)
