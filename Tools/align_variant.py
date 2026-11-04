"""
Lines up an 'after' picture with the scene painting it was made from, so the
two can be swapped with .BackgroundWhen(...) and all hotspots still fit.

    python Tools/align_variant.py <after picture> <original source> <output>

Finds matching features (SIFT), works out the shift/zoom, warps the after
picture onto the original's frame, and keeps the original wherever the after
picture doesn't reach. Needs opencv-python-headless and numpy.
"""
import sys
import cv2
import numpy as np

after_path, original_path, out_path = sys.argv[1:4]
after = cv2.imread(after_path)
original = cv2.imread(original_path)

sift = cv2.SIFT_create(6000)
k1, d1 = sift.detectAndCompute(cv2.cvtColor(after, cv2.COLOR_BGR2GRAY), None)
k2, d2 = sift.detectAndCompute(cv2.cvtColor(original, cv2.COLOR_BGR2GRAY), None)
good = [a for a, b in cv2.BFMatcher().knnMatch(d1, d2, k=2) if a.distance < 0.7 * b.distance]
p1 = np.float32([k1[g.queryIdx].pt for g in good])
p2 = np.float32([k2[g.trainIdx].pt for g in good])
M, inliers = cv2.estimateAffinePartial2D(p1, p2, method=cv2.RANSAC, ransacReprojThreshold=3)
print(f"{len(good)} matches, {int(inliers.sum())} agree; transform {np.round(M, 3).tolist()}")

H, W = original.shape[:2]
warped = cv2.warpAffine(after, M, (W, H), flags=cv2.INTER_CUBIC)
cover = cv2.warpAffine(np.full(after.shape[:2], 255, np.uint8), M, (W, H))
cover = cv2.erode(cover, np.ones((5, 5), np.uint8))
alpha = (cv2.GaussianBlur(cover, (0, 0), 3).astype(np.float32) / 255)[..., None]
out = (warped * alpha + original * (1 - alpha)).astype(np.uint8)
cv2.imwrite(out_path, out)
