"""
Builds the bone dice faces used in fights: Assets/UI/Dice/die_1.png ... die_6.png
(128x128, transparent background). Old, yellowed bone with carved pips;
the 1 is a little carved skull. Run from the repo root:
    python Tools/build_dice.py
Requires Pillow and numpy.
"""
import os
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageChops

SIZE = 128
S = 4                    # draw at 4x and scale down for smooth edges
W = SIZE * S
OUT = "Assets/UI/Dice"
rng = np.random.default_rng(1347)


def smooth_noise(scale, seed):
    r = np.random.default_rng(seed)
    small = r.random((max(2, W // scale), max(2, W // scale)))
    img = Image.fromarray((small * 255).astype(np.uint8)).resize((W, W), Image.BICUBIC)
    return np.asarray(img).astype(np.float32) / 255.0


def bone_base(seed):
    # Body shape: rounded square, slightly inset to leave room for a shadow.
    pad = 10 * S
    mask = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mask).rounded_rectangle((pad, pad, W - pad - 4 * S, W - pad - 4 * S), 22 * S, fill=255)

    # Bone colour: light ivory top-left -> darker, yellowed bottom-right.
    y, x = np.mgrid[0:W, 0:W].astype(np.float32) / W
    t = (x + y) / 2
    light = np.array([236, 224, 196], np.float32)
    dark = np.array([170, 148, 108], np.float32)
    col = light[None, None] * (1 - t[..., None]) + dark[None, None] * t[..., None]

    # Mottling and grain.
    mott = smooth_noise(24 * S // 4, seed) * 0.6 + smooth_noise(8 * S // 4, seed + 1) * 0.4
    col *= (0.92 + 0.12 * mott)[..., None]
    grain = rng.normal(0, 4, (W, W, 1))
    col += grain

    # Brown age stains.
    stain = np.clip((smooth_noise(30, seed + 2) - 0.62) * 3, 0, 1)
    col = col * (1 - 0.35 * stain[..., None]) + np.array([110, 80, 45]) * 0.35 * stain[..., None]

    img = Image.fromarray(np.clip(col, 0, 255).astype(np.uint8)).convert("RGBA")

    # Hairline cracks.
    d = ImageDraw.Draw(img)
    r = np.random.default_rng(seed + 3)
    # Tiny pores, like real bone.
    for _ in range(140):
        px, py = r.uniform(0.12, 0.88) * W, r.uniform(0.12, 0.88) * W
        rad = r.uniform(0.4, 1.1) * S
        d.ellipse((px - rad, py - rad * 0.7, px + rad, py + rad * 0.7), fill=(120, 95, 65, 90))
    for _ in range(2):
        px, py = r.uniform(0.2, 0.8) * W, r.uniform(0.2, 0.8) * W
        ang = r.uniform(0, 2 * np.pi)
        pts = [(px, py)]
        for _ in range(r.integers(4, 8)):
            ang += r.normal(0, 0.5)
            px += np.cos(ang) * 9 * S
            py += np.sin(ang) * 9 * S
            pts.append((px, py))
        d.line(pts, fill=(105, 80, 55, 85), width=int(0.9 * S))

    # Edge shading: darker toward the rim, bright bevel top-left, shadow bottom-right.
    m = np.asarray(mask).astype(np.float32) / 255.0
    inner = np.asarray(mask.filter(ImageFilter.GaussianBlur(9 * S))).astype(np.float32) / 255.0
    rim = np.clip(m - inner, 0, 1)
    shifted_tl = np.asarray(ImageChops.offset(mask, 3 * S, 3 * S).filter(ImageFilter.GaussianBlur(2 * S))).astype(np.float32) / 255
    shifted_br = np.asarray(ImageChops.offset(mask, -3 * S, -3 * S).filter(ImageFilter.GaussianBlur(2 * S))).astype(np.float32) / 255
    highlight = np.clip(m - shifted_tl, 0, 1)
    shadow = np.clip(m - shifted_br, 0, 1)

    a = np.asarray(img).astype(np.float32)
    a[..., :3] *= (1 - 0.45 * rim)[..., None]
    a[..., :3] += 70 * highlight[..., None]
    a[..., :3] *= (1 - 0.5 * shadow)[..., None]
    a[..., 3] = m * 255
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8)), mask


def carve(img, shape_mask):
    """Carve a dark recess shaped like shape_mask (L image) into the bone."""
    a = np.asarray(img).astype(np.float32)
    m = np.asarray(shape_mask).astype(np.float32) / 255.0
    # Inside: dark, with a shadow on the top-left inner wall.
    inner_shadow = np.asarray(ImageChops.offset(shape_mask, 2 * S, 2 * S)).astype(np.float32) / 255.0
    wall = np.clip(m - inner_shadow, 0, 1)
    pit = np.array([52, 32, 20], np.float32)
    a[..., :3] = a[..., :3] * (1 - m[..., None]) + pit * m[..., None]
    a[..., :3] *= (1 - 0.5 * wall)[..., None]
    # Lip: a little light on the bottom-right edge just outside the hole.
    lip_src = ImageChops.offset(shape_mask, S * 2, S * 2).filter(ImageFilter.GaussianBlur(S))
    lip = np.clip(np.asarray(lip_src).astype(np.float32) / 255.0 - m, 0, 1)
    a[..., :3] += 55 * lip[..., None]
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8))


def pips_mask(points, radius):
    mk = Image.new("L", (W, W), 0)
    d = ImageDraw.Draw(mk)
    for (px, py) in points:
        d.ellipse((px - radius, py - radius, px + radius, py + radius), fill=255)
    return mk.filter(ImageFilter.GaussianBlur(S * 0.6))


def skull_mask():
    """A small carved skull for the 1 face."""
    mk = Image.new("L", (W, W), 0)
    d = ImageDraw.Draw(mk)
    cx, cy = W / 2 - 2 * S, W / 2 - 4 * S
    lw = int(4.5 * S)
    # Cranium + cheeks outline
    d.ellipse((cx - 23 * S, cy - 24 * S, cx + 23 * S, cy + 18 * S), outline=255, width=lw)
    # Jaw
    d.rounded_rectangle((cx - 13 * S, cy + 10 * S, cx + 13 * S, cy + 27 * S), 5 * S, outline=255, width=lw)
    # Eye sockets
    d.ellipse((cx - 16 * S, cy - 6 * S, cx - 3 * S, cy + 7 * S), fill=255)
    d.ellipse((cx + 3 * S, cy - 6 * S, cx + 16 * S, cy + 7 * S), fill=255)
    # Nose
    d.polygon([(cx, cy + 8 * S), (cx - 4 * S, cy + 15 * S), (cx + 4 * S, cy + 15 * S)], fill=255)
    # Teeth
    for tx in (-6, 0, 6):
        d.line((cx + tx * S, cy + 17 * S, cx + tx * S, cy + 25 * S), fill=255, width=int(2.5 * S))
    # Clear the area between cranium outline and jaw so it reads as a skull
    return mk.filter(ImageFilter.GaussianBlur(S * 0.5))


def face_points(n):
    c = W / 2 - 2 * S
    o = 27 * S
    L, M, R = c - o, c, c + o
    T, B = c - o, c + o
    return {
        2: [(L, T), (R, B)],
        3: [(L, T), (M, M), (R, B)],
        4: [(L, T), (R, T), (L, B), (R, B)],
        5: [(L, T), (R, T), (M, M), (L, B), (R, B)],
        6: [(L, T), (R, T), (L, M), (R, M), (L, B), (R, B)],
    }[n]


def with_shadow(img):
    shadow = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    alpha = img.split()[3].filter(ImageFilter.GaussianBlur(4 * S))
    sh = Image.new("RGBA", (W, W), (0, 0, 0, 255))
    sh.putalpha(alpha.point(lambda v: v * 0.6))
    shadow.alpha_composite(sh, (3 * S, 5 * S))
    shadow.alpha_composite(img)
    return shadow


os.makedirs(OUT, exist_ok=True)
for n in range(1, 7):
    base, _ = bone_base(seed=100 + n)
    if n == 1:
        face = carve(base, skull_mask())
    else:
        face = carve(base, pips_mask(face_points(n), 10.5 * S))
    final = with_shadow(face).resize((SIZE, SIZE), Image.LANCZOS)
    path = os.path.join(OUT, f"die_{n}.png")
    final.save(path)
    print("wrote", path)
