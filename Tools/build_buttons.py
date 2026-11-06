"""
Cuts the verb and fight buttons from the two button sheets
(Assets/UI/Sources/buttons.webp and buttons_glow.webp, 3 x 2 plaques on
black). Writes Assets/UI/<name>.png and <name>_glow.png with transparent
surroundings (the glow keeps its halo as soft transparency).
Usage: python Tools/build_buttons.py   (opencv-python-headless, numpy)
"""
import cv2
import numpy as np

NAMES = [["item", "look", "talk"], ["act", "attack", "flee"]]
COLS, ROWS = [70, 699, 1329], [111, 372]
W, H, M = 602, 200, 16          # plaque size and margin kept around it

normal = cv2.imread("Assets/UI/Sources/buttons.webp")
glow = cv2.imread("Assets/UI/Sources/buttons_glow.webp")


def plaque_mask(img):
    g = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    m = (g > 22).astype(np.uint8) * 255
    m = cv2.morphologyEx(m, cv2.MORPH_CLOSE, np.ones((9, 9), np.uint8))
    # fill holes (dark areas inside the plaque)
    h, w = m.shape
    flood = m.copy()
    cv2.floodFill(flood, np.zeros((h + 2, w + 2), np.uint8), (0, 0), 255)
    return cv2.GaussianBlur(m | cv2.bitwise_not(flood), (0, 0), 1.2)


for r, row in enumerate(NAMES):
    for c, name in enumerate(row):
        x0, y0 = COLS[c] - M, ROWS[r] - M
        box = (slice(y0, y0 + H + 2 * M), slice(x0, x0 + W + 2 * M))
        n = normal[box]
        mask = plaque_mask(n)
        cv2.imwrite(f"Assets/UI/{name}.png", np.dstack([n, mask]))

        gl = glow[box].astype(np.float32)
        halo = gl.max(axis=2) / 255.0                      # glow on black -> transparency
        a = np.maximum(mask.astype(np.float32) / 255.0, np.clip(halo * 1.15, 0, 1))
        col = np.where(a[..., None] > 0.01, gl / np.maximum(a[..., None], 0.01), 0)
        inside = mask.astype(np.float32)[..., None] / 255.0
        col = gl * inside + np.clip(col, 0, 255) * (1 - inside)
        cv2.imwrite(f"Assets/UI/{name}_glow.png", np.dstack([col.astype(np.uint8), (a * 255).astype(np.uint8)]))
        print("wrote", name)
