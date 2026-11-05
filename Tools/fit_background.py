"""
Fits a picture into the game's visible band so nothing important is cut off
(only rows 192-832 of a 1536x1024 background show in the game).

    python Tools/fit_background.py in.png out.png LEFT TOP RIGHT BOTTOM

The box (source pixels) is scaled to fit inside the band and centred; the
rest of the screen is a dark, blurred copy of the picture. Prints the scale
and offset, so hotspots measured on the source can be converted:
    new_x = x * scale + dx,   new_y = y * scale + dy
Use the same box for both pictures of a before/after pair.
"""
import sys
from PIL import Image, ImageDraw, ImageEnhance, ImageFilter

W, H = 1536, 1024
BAND_TOP, BAND_BOTTOM = 192, 832


def fit(src, box):
    left, top, right, bottom = box
    scale = min(W / (right - left), (BAND_BOTTOM - BAND_TOP) / (bottom - top))
    cover = max(W / src.width, H / src.height)
    bg = src.resize((round(src.width * cover), round(src.height * cover)), Image.LANCZOS)
    bg = bg.crop(((bg.width - W) // 2, (bg.height - H) // 2, (bg.width - W) // 2 + W, (bg.height - H) // 2 + H))
    bg = ImageEnhance.Brightness(bg.filter(ImageFilter.GaussianBlur(30))).enhance(0.6)
    img = src.resize((round(src.width * scale), round(src.height * scale)), Image.LANCZOS)
    dx = round(W / 2 - (left + right) / 2 * scale)
    dy = round((BAND_TOP + BAND_BOTTOM) / 2 - (top + bottom) / 2 * scale)
    mask = Image.new("L", img.size, 0)
    ImageDraw.Draw(mask).rectangle((30, 30, img.width - 31, img.height - 31), fill=255)
    bg.paste(img, (dx, dy), mask.filter(ImageFilter.GaussianBlur(18)))
    return bg, scale, dx, dy


if __name__ == "__main__":
    out, scale, dx, dy = fit(Image.open(sys.argv[1]).convert("RGB"), [int(v) for v in sys.argv[3:7]])
    out.save(sys.argv[2])
    print(f"wrote {sys.argv[2]}  scale={scale:.4f} dx={dx} dy={dy}")
