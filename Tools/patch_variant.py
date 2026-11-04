"""
Makes a "before / after" background pair from two ChatGPT pictures of the
same view (e.g. coins in the fountain / coins taken).

ChatGPT redraws the whole picture slightly with every edit, so switching
between its two versions would make everything shimmer. Instead the
"before" picture is kept, and only one box (feathered) is taken from the
"after" picture:

  python Tools/patch_variant.py before.png after.png out_before.png out_after.png x0 y0 x1 y1 [feather]

x0 y0 x1 y1 is the box in image pixels around what changes; feather
(default 10) softens its edge. Check the result - if the two pictures are
shifted relative to each other, align them first with Tools/align_variant.py.
"""
import sys

import numpy as np
from PIL import Image, ImageFilter


def main():
    if len(sys.argv) < 9:
        print(__doc__)
        sys.exit(1)

    before_path, after_path, out_before, out_after = sys.argv[1:5]
    x0, y0, x1, y1 = (int(v) for v in sys.argv[5:9])
    feather = float(sys.argv[9]) if len(sys.argv) > 9 else 10

    before = np.asarray(Image.open(before_path).convert("RGB")).astype(np.float32)
    after = np.asarray(Image.open(after_path).convert("RGB")).astype(np.float32)

    hard = np.zeros(before.shape[:2], dtype=np.uint8)
    hard[y0:y1, x0:x1] = 255
    mask = np.asarray(Image.fromarray(hard).filter(ImageFilter.GaussianBlur(feather)))
    mask = mask.astype(np.float32)[..., None] / 255.0

    patched = before * (1 - mask) + after * mask

    Image.fromarray(before.astype(np.uint8)).save(out_before, optimize=True)
    Image.fromarray(patched.clip(0, 255).astype(np.uint8)).save(out_after, optimize=True)
    print("wrote", out_before, "and", out_after)


if __name__ == "__main__":
    main()
