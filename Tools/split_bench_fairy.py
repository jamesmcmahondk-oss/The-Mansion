"""
Splits the painted-over 'Ebria under the bench' picture into:
  - Assets/Backgrounds/Mansion/Sources/bench_closeup_clean_source.png  (bench, no fairy)
  - Assets/Characters/Ebria_UnderBench.png                              (fairy, transparent)

How: the picture is a zoomed crop of the original bench painting
(Sources/bench_closeup_source.png) that was repainted with the fairy in it.
The two are aligned with SIFT feature matching. The fairy is cut out with
GrabCut. The space behind her is filled with the matching part of the original
painting, re-lit to match the repaint's shadows.

Usage (from the repo root; needs opencv-python-headless and numpy):
    python Tools/split_bench_fairy.py path/to/ebria_composite.webp
Prints where to place the sprite (background pixels) for Data/Scenes.cs.
"""
import sys
import cv2
import numpy as np

COMPOSITE = sys.argv[1]
ORIGINAL = "Assets/Backgrounds/Mansion/Sources/bench_closeup_source.png"
OUT_BG = "Assets/Backgrounds/Mansion/Sources/bench_closeup_clean_source.png"
OUT_SPRITE = "Assets/Characters/Ebria_UnderBench.png"
KEEP_ROWS = 875           # rows below this are the game's menu frame in the screenshot

up = cv2.imread(COMPOSITE)
src = cv2.imread(ORIGINAL)
H, W = up.shape[:2]

# 1. Align the original painting to the composite.
sift = cv2.SIFT_create(8000)
k1, d1 = sift.detectAndCompute(cv2.cvtColor(up, cv2.COLOR_BGR2GRAY), None)
k2, d2 = sift.detectAndCompute(cv2.cvtColor(src, cv2.COLOR_BGR2GRAY), None)
good = [a for a, b in cv2.BFMatcher().knnMatch(d1, d2, k=2) if a.distance < 0.75 * b.distance]
p1 = np.float32([k1[g.queryIdx].pt for g in good])
p2 = np.float32([k2[g.trainIdx].pt for g in good])
A, _ = cv2.estimateAffinePartial2D(p1, p2, method=cv2.RANSAC, ransacReprojThreshold=3)
orig = cv2.warpAffine(src, cv2.invertAffineTransform(A), (W, H), flags=cv2.INTER_CUBIC).astype(np.float32)

# 2. Cut out the fairy (GrabCut, seeded by hand-drawn hints).
def grabcut(mask):
    bgd = np.zeros((1, 65)); fgd = np.zeros((1, 65))
    cv2.grabCut(up, mask, None, bgd, fgd, 8, cv2.GC_INIT_WITH_MASK)
    fg = np.where((mask == 1) | (mask == 3), 255, 0).astype(np.uint8)
    n, lab, st, _ = cv2.connectedComponentsWithStats(fg)
    return np.where(lab == 1 + np.argmax(st[1:, cv2.CC_STAT_AREA]), 255, 0).astype(np.uint8)

core = np.zeros((H, W), np.uint8)
core[315:785, 415:830] = cv2.GC_PR_BGD
cv2.ellipse(core, (640, 560), (110, 190), 0, 0, 360, cv2.GC_PR_FGD, -1)
cv2.rectangle(core, (600, 600), (740, 740), cv2.GC_FGD, -1)
cv2.circle(core, (600, 380), 35, cv2.GC_FGD, -1)
cv2.rectangle(core, (790, 320), (830, 560), cv2.GC_BGD, -1)
cv2.rectangle(core, (415, 315), (440, 600), cv2.GC_BGD, -1)
body = grabcut(core)

outline = np.array([(448, 330), (520, 318), (565, 318), (640, 322), (700, 368), (770, 365), (778, 405),
                    (778, 500), (765, 560), (790, 690), (815, 745), (760, 775), (600, 770), (440, 762),
                    (432, 720), (452, 640), (440, 500), (442, 400)], np.int32)
full = np.full((H, W), cv2.GC_BGD, np.uint8)
cv2.fillPoly(full, [outline], cv2.GC_PR_FGD)
full[body > 0] = cv2.GC_FGD
fg = cv2.morphologyEx(grabcut(full), cv2.MORPH_CLOSE, np.ones((5, 5), np.uint8))

# 3. Fill behind her with the original painting, re-lit like the repaint.
hole = cv2.dilate(fg, np.ones((11, 11), np.uint8))
for x0, y0, x1, y1 in [(446, 322, 560, 420), (468, 575, 550, 712), (422, 690, 485, 780),
                       (540, 318, 700, 345), (448, 570, 482, 652), (770, 700, 832, 748),
                       (742, 560, 780, 715), (462, 405, 510, 476)]:
    hole[y0:y1, x0:x1] = 255
known = (cv2.dilate(hole, np.ones((9, 9), np.uint8)) == 0).astype(np.float32)
upf = up.astype(np.float32)
ratio = np.log((upf + 4) / (orig + 4))
num = cv2.GaussianBlur(ratio * known[..., None], (0, 0), 30)
den = cv2.GaussianBlur(known, (0, 0), 30)[..., None]
fill = np.clip(orig * np.exp(num / np.maximum(den, 1e-4)), 0, 255)
alpha = cv2.GaussianBlur(hole.astype(np.float32) / 255, (0, 0), 3)[..., None]
alpha = np.maximum(alpha, (cv2.erode(hole, np.ones((5, 5), np.uint8)) > 0)[..., None].astype(np.float32))
clean = np.clip(upf * (1 - alpha) + fill * alpha, 0, 255).astype(np.uint8)
cv2.imwrite(OUT_BG, clean[:KEEP_ROWS])

# 4. The fairy as a transparent sprite, soft-edged and cropped.
a = cv2.GaussianBlur(cv2.erode(fg, np.ones((3, 3), np.uint8)).astype(np.float32), (0, 0), 1.2)
rgba = np.dstack([up, np.clip(a, 0, 255).astype(np.uint8)])
x, y, w, h = cv2.boundingRect(fg)
cv2.imwrite(OUT_SPRITE, rgba[y:y + h, x:x + w])
print(f"sprite at composite pixels x={x} y={y} w={w} h={h}")
