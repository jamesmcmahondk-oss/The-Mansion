"""
Builds the fight menu buttons (ATTACK and FLEE, plus *_glow hover versions)
in Assets/UI/ from the existing ACT button, so they match the HUD style.
ATTACK gets a bone die as its icon; FLEE gets the pointing hand flipped.
Run from the repo root after Tools/build_dice.py:
    python Tools/build_fight_buttons.py
Requires Pillow and numpy.
"""
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageOps

FONT = None
for path in ("C:/Windows/Fonts/georgia.ttf",
             "/usr/share/fonts/truetype/liberation/LiberationSerif-Regular.ttf",
             "/usr/share/fonts/truetype/dejavu/DejaVuSerif.ttf"):
    try:
        ImageFont.truetype(path, 10)
        FONT = path
        break
    except OSError:
        pass

SCALE = 4  # draw text at 4x for smooth edges


def clean_plaque(src):
    """Copy of the button with icon and lettering painted over with plain plaque."""
    img = src.copy()
    a = np.asarray(img).astype(np.float32)
    patch = a[14:49, 62:84]                       # plain texture between hand and text
    tile = np.concatenate([patch, patch[:, ::-1]], axis=1)
    for x0, x1 in ((13, 62), (80, 158)):
        width = x1 - x0
        reps = int(np.ceil(width / tile.shape[1]))
        fill = np.concatenate([tile] * reps, axis=1)[:, :width]
        a[14:49, x0:x1] = fill
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8))


def gold_text(text, height, glow):
    font = ImageFont.truetype(FONT, int(height * SCALE * 1.38))
    tmp = Image.new("L", (1, 1))
    box = ImageDraw.Draw(tmp).textbbox((0, 0), text, font=font)
    w, h = box[2] - box[0] + 8 * SCALE, box[3] - box[1] + 8 * SCALE
    mask = Image.new("L", (w, h), 0)
    ImageDraw.Draw(mask).text((4 * SCALE - box[0], 4 * SCALE - box[1]), text, font=font, fill=255)

    # Vertical gold gradient
    top = np.array([236, 204, 140] if glow else [214, 182, 122], np.float32)
    bot = np.array([196, 146, 74] if glow else [160, 120, 64], np.float32)
    t = np.linspace(0, 1, h)[:, None, None]
    grad = top * (1 - t) + bot * t
    rgb = np.broadcast_to(grad, (h, w, 3))
    layer = np.dstack([rgb, np.asarray(mask)]).astype(np.uint8)
    text_img = Image.fromarray(layer, "RGBA")

    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    shadow = Image.new("RGBA", (w, h), (10, 6, 2, 255))
    shadow.putalpha(mask.filter(ImageFilter.GaussianBlur(1.5 * SCALE)).point(lambda v: v * 0.9))
    out.alpha_composite(shadow, (SCALE, SCALE))
    if glow:
        g = Image.new("RGBA", (w, h), (255, 200, 110, 255))
        g.putalpha(mask.filter(ImageFilter.GaussianBlur(5 * SCALE)).point(lambda v: v * 0.8))
        out.alpha_composite(g)
    out.alpha_composite(text_img)
    return out.resize((w // SCALE, h // SCALE), Image.LANCZOS)


def glow_layer():
    """The ACT button's hover glow (act_glow - act). Inside the icon/lettering
    area it is replaced by the glow of the plain gap between them, blended
    into the frame glow near the edges, so the new icon/text don't inherit
    the old ones' glow."""
    normal = np.asarray(Image.open("Assets/UI/act.png").convert("RGB")).astype(np.float32)
    glowing = np.asarray(Image.open("Assets/UI/act_glow.png").convert("RGB")).astype(np.float32)
    diff = glowing - normal

    y0, y1, x0, x1 = 12, 51, 12, 160
    profile = diff[y0:y1, 62:80].mean(axis=1)              # glow in the plain gap
    region = np.repeat(profile[:, None, :], x1 - x0, axis=1)

    ramp = 14
    left = diff[y0:y1, x0 - 1]
    right = diff[y0:y1, x1]
    for i in range(ramp):
        t = i / ramp
        region[:, i] = left * (1 - t) + region[:, i] * t
        region[:, -1 - i] = right * (1 - t) + region[:, -1 - i] * t

    out = diff.copy()
    out[y0:y1, x0:x1] = region
    return out


GLOW = glow_layer()


def build(name, icon, glow):
    src = Image.open("Assets/UI/act.png").convert("RGBA")
    btn = clean_plaque(src)

    if glow:
        a = np.asarray(btn).astype(np.float32)
        a[..., :3] += GLOW
        btn = Image.fromarray(np.clip(a, 0, 255).astype(np.uint8), "RGBA")

    # Icon centred where the hand was (x ~19-56, y ~16-46).
    ix = 37 - icon.width // 2
    iy = 31 - icon.height // 2
    btn.alpha_composite(icon, (ix, iy))

    # Lettering centred in the text area (x ~70-165).
    txt = gold_text(name, 20, glow)
    btn.alpha_composite(txt, (117 - txt.width // 2, 31 - txt.height // 2))
    return btn.convert("RGB")


die = Image.open("Assets/UI/Dice/die_6.png").convert("RGBA").resize((38, 38), Image.LANCZOS)
hand_src = Image.open("Assets/UI/act.png").convert("RGBA").crop((14, 14, 62, 49))
hand_glow_src = Image.open("Assets/UI/act_glow.png").convert("RGBA").crop((14, 14, 62, 49))


def hand_icon(crop):
    # The hand with its own plaque background, flipped, with soft edges so
    # it blends into the cleaned plaque.
    flipped = ImageOps.mirror(crop)
    mask = Image.new("L", flipped.size, 0)
    ImageDraw.Draw(mask).rounded_rectangle((3, 3, flipped.width - 4, flipped.height - 4), 6, fill=255)
    flipped.putalpha(mask.filter(ImageFilter.GaussianBlur(2.5)))
    return flipped


for glow in (False, True):
    suffix = "_glow" if glow else ""
    build("ATTACK", die, glow).save(f"Assets/UI/attack{suffix}.png")
    build("FLEE", hand_icon(hand_glow_src if glow else hand_src), glow).save(f"Assets/UI/flee{suffix}.png")
    print("wrote attack/flee", suffix or "(normal)")
