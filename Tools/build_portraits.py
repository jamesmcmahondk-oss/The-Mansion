"""
Builds close-up character portraits from the full-body character art.
Writes two versions per character into Assets/Characters/Portraits/:

  <name>_portrait.png         600x800, dark candle-lit background
  <name>_portrait_framed.png  same, with a brass frame and name plate

Run from the repo root:  python Tools/build_portraits.py
Requires Pillow and numpy.

To add a character: add a line to PORTRAITS with the source image, the
crop box (left, top, right, bottom) around head and shoulders (keep it
3:4, e.g. 600 wide x 800 tall or 450 x 600), the glow position (0-1 across,
0-1 down) and the name plate text.
"""
import os
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

OUT_W, OUT_H = 600, 800
OUT_DIR = "Assets/Characters/Portraits"

PORTRAITS = {
    "Quazimodo": dict(
        source="Assets/Characters/Quazimodo.png",
        crop=(140, 40, 660, 733),
        glow=(0.38, 0.30),
        title="QUAZIMODO",
        subtitle="The Butler"),
    # A picture that already has its own background (no transparency):
    # "opaque" keeps that background and only re-lights it to match.
    "Quazimodo_Beaten": dict(
        source="Assets/Characters/Portraits/Sources/Quazimodo_beaten_source.jpg",
        crop=(235, 105, 655, 665),
        glow=(0.45, 0.28),
        opaque=True,
        title="QUAZIMODO",
        subtitle="The Butler"),
    # Already painted in the portrait style: just resize and frame it.
    "Hautain_Beaten": dict(
        source="Assets/Characters/Portraits/Sources/Hautain_beaten_source.png",
        crop=(0, 0, 768, 1024),
        glow=(0.62, 0.24),
        opaque=True,
        keep_look=True,
        title="HAUTAIN",
        subtitle="The Frog King"),
    "Quazimodo_Bribed": dict(
        source="Assets/Characters/Portraits/Sources/Quazimodo_bribed_source.png",
        crop=(0, 0, 896, 1195),
        glow=(0.45, 0.28),
        opaque=True,
        keep_look=True,
        title="QUAZIMODO",
        subtitle="The Butler"),
    "Hautain": dict(
        source="Assets/Characters/Hautain.png",
        crop=(20, 0, 860, 1120),
        glow=(0.62, 0.24),
        title="HAUTAIN",
        subtitle="The Frog King"),
}

FONT_PATHS = ["C:/Windows/Fonts/georgiab.ttf", "C:/Windows/Fonts/georgia.ttf",
              "/usr/share/fonts/truetype/dejavu/DejaVuSerif-Bold.ttf",
              "/usr/share/fonts/truetype/dejavu/DejaVuSerif.ttf"]


def font(size, bold=True):
    for p in FONT_PATHS if bold else FONT_PATHS[1::2]:
        try:
            return ImageFont.truetype(p, size)
        except OSError:
            continue
    return ImageFont.load_default()


def background(glow_x, glow_y):
    y, x = np.mgrid[0:OUT_H, 0:OUT_W].astype(np.float32)
    # Base: very dark warm brown, a little lighter at the top.
    base = np.array([26, 18, 14], np.float32)
    top = np.array([40, 28, 20], np.float32)
    t = (y / OUT_H)[..., None]
    col = top * (1 - t) + base * t

    # Candle glow behind the head.
    gx, gy = glow_x * OUT_W, glow_y * OUT_H
    d = np.hypot((x - gx) / OUT_W, (y - gy) / OUT_H)
    glow = np.exp(-(d / 0.33) ** 2)[..., None]
    col += np.array([120, 72, 32], np.float32) * glow * 0.9

    # Mottled old-wall texture.
    rng = np.random.default_rng(7)
    small = rng.random((OUT_H // 20, OUT_W // 20))
    tex = np.asarray(Image.fromarray((small * 255).astype(np.uint8)).resize((OUT_W, OUT_H), Image.BICUBIC)) / 255.0
    col *= (0.85 + 0.25 * tex)[..., None]
    col += rng.normal(0, 3, col.shape)
    return Image.fromarray(np.clip(col, 0, 255).astype(np.uint8)).convert("RGBA")


def vignette(img, strength=0.75):
    y, x = np.mgrid[0:OUT_H, 0:OUT_W].astype(np.float32)
    r = np.hypot((x - OUT_W / 2) / (OUT_W / 2), (y - OUT_H * 0.45) / (OUT_H / 2))
    shade = np.clip(1 - strength * np.clip(r - 0.45, 0, 1) ** 1.6, 0, 1)[..., None]
    a = np.asarray(img).astype(np.float32)
    a[..., :3] *= shade
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8))


def warm_light(char, glow_x, glow_y):
    """Tint the character slightly toward the candle and darken the far side."""
    a = np.asarray(char).astype(np.float32)
    h, w = a.shape[:2]
    y, x = np.mgrid[0:h, 0:w].astype(np.float32)
    lit = np.clip(1.1 - np.hypot((x / w - glow_x), (y / h - glow_y)) * 0.9, 0.6, 1.1)[..., None]
    a[..., :3] *= lit
    a[..., :3] = a[..., :3] * np.array([1.06, 1.0, 0.9], np.float32)   # warmer
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8), "RGBA")


def candle_grade(img, glow_x, glow_y):
    """Give a picture with its own background the same warm candle light."""
    a = np.asarray(img).astype(np.float32)
    y, x = np.mgrid[0:OUT_H, 0:OUT_W].astype(np.float32)
    d = np.hypot((x / OUT_W - glow_x), (y / OUT_H - glow_y))
    glow = np.exp(-(d / 0.38) ** 2)[..., None]
    a[..., :3] *= np.array([1.10, 0.98, 0.82], np.float32)       # warm, like candlelight
    a[..., :3] += np.array([70, 40, 16], np.float32) * glow * 0.55   # glow behind the head
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8), "RGBA")


def build(name, spec):
    src = Image.open(spec["source"]).convert("RGBA")
    char = src.crop(spec["crop"]).resize((OUT_W, OUT_H), Image.LANCZOS)
    char = char.filter(ImageFilter.UnsharpMask(radius=1.2, percent=60, threshold=2))

    if spec.get("keep_look"):
        img = char                      # already lit and toned like the others
    elif spec.get("opaque"):
        img = candle_grade(char, *spec["glow"])
    else:
        char = warm_light(char, *spec["glow"])

        # Soft dark shadow behind the figure so it sits in the scene.
        shadow = Image.new("RGBA", char.size, (0, 0, 0, 0))
        sh_alpha = char.split()[3].filter(ImageFilter.GaussianBlur(14)).point(lambda v: v * 0.55)
        shadow.putalpha(sh_alpha)

        img = background(*spec["glow"])
        img.alpha_composite(shadow, (10, 12))
        img.alpha_composite(char)

    if not spec.get("keep_look"):
        img = vignette(img)
    img.convert("RGB").save(os.path.join(OUT_DIR, f"{name}_portrait.png"))

    # Framed version with a name plate.
    framed = img.copy()
    d = ImageDraw.Draw(framed)
    for i, (w, col) in enumerate([(10, (58, 40, 20)), (4, (190, 150, 82)), (2, (90, 64, 30))]):
        off = [0, 8, 13][i]
        d.rectangle((off, off, OUT_W - 1 - off, OUT_H - 1 - off), outline=col, width=w)
    d.rectangle((16, 16, OUT_W - 17, OUT_H - 17), outline=(120, 92, 50), width=1)

    plate_h = 92
    py = OUT_H - 30 - plate_h
    d.rectangle((60, py, OUT_W - 60, py + plate_h), fill=(20, 12, 10, 235), outline=(190, 150, 82), width=3)
    d.rectangle((66, py + 6, OUT_W - 66, py + plate_h - 6), outline=(110, 82, 40), width=1)
    f1, f2 = font(34), font(20, bold=False)
    t1, t2 = spec["title"], spec["subtitle"]
    d.text(((OUT_W - d.textlength(t1, font=f1)) / 2, py + 12), t1, font=f1, fill=(222, 190, 125))
    d.text(((OUT_W - d.textlength(t2, font=f2)) / 2, py + 56), t2, font=f2, fill=(176, 158, 128))
    framed.convert("RGB").save(os.path.join(OUT_DIR, f"{name}_portrait_framed.png"))
    print("wrote", name)


os.makedirs(OUT_DIR, exist_ok=True)
for name, spec in PORTRAITS.items():
    build(name, spec)
