"""
Builds the Garden backgrounds from two ChatGPT pictures of the same view:
one with the statue holding its sword, one without.

ChatGPT redraws the whole picture slightly between versions, so only the
statue patch is taken from the sword-less picture (feathered) and pasted
into the other. Nothing else shifts when the sword is taken.

Writes Assets/Backgrounds/Mansion/garden.png and garden_nosword.png.
Usage: python Tools/build_garden.py with_sword.png without_sword.png
"""
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageFilter

OUT = Path(__file__).resolve().parent.parent / "Assets" / "Backgrounds" / "Mansion"

# Around the statue and its sword (x0, y0, x1, y1).
STATUE_BOX = (755, 330, 900, 580)


def main():
    with_sword = np.asarray(Image.open(sys.argv[1]).convert("RGB")).astype(np.float32)
    without = np.asarray(Image.open(sys.argv[2]).convert("RGB")).astype(np.float32)

    x0, y0, x1, y1 = STATUE_BOX
    hard = np.zeros(with_sword.shape[:2], dtype=np.uint8)
    hard[y0:y1, x0:x1] = 255
    mask = Image.fromarray(hard).filter(ImageFilter.GaussianBlur(10))
    mask = np.asarray(mask).astype(np.float32)[..., None] / 255.0

    no_sword = with_sword * (1 - mask) + without * mask

    Image.fromarray(with_sword.astype(np.uint8)).save(OUT / "garden.png", optimize=True)
    Image.fromarray(no_sword.clip(0, 255).astype(np.uint8)).save(OUT / "garden_nosword.png", optimize=True)
    print("wrote garden.png and garden_nosword.png")


if __name__ == "__main__":
    main()
