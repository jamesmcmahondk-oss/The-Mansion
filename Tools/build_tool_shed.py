"""
Builds the tool shed close-up backgrounds from three ChatGPT pictures:
  full     - shears and charcoal on the workbench
  charcoal - the same with the shears removed
  empty    - the same with both removed

ChatGPT redraws the whole picture a little on every edit, so only the item
patches are taken from the edits (feathered) and pasted into the full
picture. That way nothing else shifts when an item is picked up.

Writes Assets/Backgrounds/Mansion/tool_shed_closeup*.png:
  tool_shed_closeup.png              both items
  tool_shed_closeup_no_shears.png    charcoal only
  tool_shed_closeup_no_charcoal.png  shears only
  tool_shed_closeup_empty.png        neither

Usage: python Tools/build_tool_shed.py full.png charcoal.png empty.png
"""
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageFilter

OUT = Path(__file__).resolve().parent.parent / "Assets" / "Backgrounds" / "Mansion"

# Where each item lies on the workbench (x0, y0, x1, y1), with a margin.
SHEARS_BOX = (470, 490, 765, 600)
CHARCOAL_BOX = (860, 495, 1055, 592)


def load(path):
    return np.asarray(Image.open(path).convert("RGB")).astype(np.float32)


def item_mask(a, b, box):
    """Soft mask of where a and b really differ inside box."""
    x0, y0, x1, y1 = box
    diff = np.abs(a - b).sum(axis=2)
    hard = np.zeros(diff.shape, dtype=np.uint8)
    region = diff[y0:y1, x0:x1] > 60
    hard[y0:y1, x0:x1] = region * 255
    mask = Image.fromarray(hard).filter(ImageFilter.MaxFilter(15)).filter(ImageFilter.GaussianBlur(6))
    return np.asarray(mask).astype(np.float32)[..., None] / 255.0


def paste(base, source, mask):
    return base * (1 - mask) + source * mask


def save(array, name):
    Image.fromarray(array.clip(0, 255).astype(np.uint8)).save(OUT / name, optimize=True)
    print("wrote", name)


def main():
    full, charcoal_only, empty = (load(p) for p in sys.argv[1:4])

    shears_mask = item_mask(full, charcoal_only, SHEARS_BOX)
    charcoal_mask = item_mask(charcoal_only, empty, CHARCOAL_BOX)

    no_shears = paste(full, charcoal_only, shears_mask)
    no_charcoal = paste(full, empty, charcoal_mask)
    neither = paste(no_shears, empty, charcoal_mask)

    save(full, "tool_shed_closeup.png")
    save(no_shears, "tool_shed_closeup_no_shears.png")
    save(no_charcoal, "tool_shed_closeup_no_charcoal.png")
    save(neither, "tool_shed_closeup_empty.png")


if __name__ == "__main__":
    main()
