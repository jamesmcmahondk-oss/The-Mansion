"""
Builds the "gargoyle wearing the party hat" portrait source:
    Assets/Characters/Portraits/Sources/Gargoyle_PartyHat_source.png (600x800)

It lifts the hat and its string from the painted-over portrait
(Sources/Gargoyle_partyhat_edit.jpg, which lines up exactly with the normal
Gargoyle portrait), makes the hat bigger (tilting it a little toward the
moon so its tip still fits in the picture) and puts both onto the clean
Gargoyle_portrait.png. Then build_portraits.py frames it:

    python Tools/build_portraits.py        (makes Gargoyle_portrait.png)
    python Tools/build_gargoyle_hat.py
    python Tools/build_portraits.py        (frames the hat version)

Requires Pillow and numpy.
"""
import math
import numpy as np
from PIL import Image, ImageFilter

HAT_SCALE = 1.3           # how much bigger the hat gets
HAT_TILT = 14            # extra tilt in degrees (positive = leaning toward the moon)
HAT_DROP = 16            # sit the hat this many pixels lower on the head

clean = Image.open("Assets/Characters/Portraits/Gargoyle_portrait.png").convert("RGBA")
edit = Image.open("Assets/Characters/Portraits/Sources/Gargoyle_partyhat_edit.jpg").convert("RGB")
edit = edit.resize(clean.size, Image.LANCZOS).convert("RGBA")
W, H = clean.size

a = np.asarray(edit).astype(np.float32)
b = np.asarray(clean).astype(np.float32)

# Hat and string = warm-coloured pixels (beige paper, red tinsel, tan string)
# that differ from the clean portrait; the stone and sky are grey/blue.
diff = np.abs(a[..., :3] - b[..., :3]).max(axis=2)
r, bl = a[..., 0], a[..., 2]
changed = (diff > 30) & (r - bl > 22) & (r > 55)

hat_area = np.zeros((H, W), bool)
hat_area[22:245, 185:450] = True     # (below the painted frame)

string_area = np.zeros((H, W), bool)
string_area[150:600, 95:190] = True     # left string
string_area[245:600, 95:400] = True     # lower string and the bow
string_area &= ~hat_area


def to_mask(m, close=0, blur=1.0):
    img = Image.fromarray(m.astype(np.uint8) * 255, "L")
    if close:
        img = img.filter(ImageFilter.MaxFilter(close)).filter(ImageFilter.MinFilter(close))
    return img.filter(ImageFilter.GaussianBlur(blur))


# Hat: close the holes left by the darker polka dots so it's solid paper.
hat_mask = to_mask(changed & hat_area, close=21, blur=1.2)
string_mask = to_mask(changed & string_area, close=3, blur=0.8)

hat = edit.copy()
hat.putalpha(hat_mask)
string = edit.copy()
string.putalpha(string_mask)

# Pivot: the middle of the hat's brim (lowest part of the hat).
ys, xs = np.nonzero(np.asarray(hat_mask) > 128)
bottom = ys.max()
brim = xs[ys > bottom - 25]
pivot = (float(brim.mean()), float(bottom))

# Scale and tilt the hat around the pivot (inverse mapping for PIL).
t = math.radians(HAT_TILT)
cos, sin = math.cos(t) / HAT_SCALE, math.sin(t) / HAT_SCALE
px, py = pivot
py_dest = py + HAT_DROP
# source = pivot + R(-t)/s * (dest - pivot)
coeffs = (cos, sin, px - cos * px - sin * py_dest,
          -sin, cos, py + sin * px - cos * py_dest)
big_hat = hat.transform((W, H), Image.AFFINE, coeffs, resample=Image.BICUBIC)

out = clean.copy()
out.alpha_composite(string)
out.alpha_composite(big_hat)
out.convert("RGB").save("Assets/Characters/Portraits/Sources/Gargoyle_PartyHat_source.png")

top = np.nonzero(np.asarray(big_hat.split()[3]).max(axis=1) > 128)[0].min()
print("pivot", pivot, "hat top now at y =", top)
