"""
Makes the HUD frame art transparent around the frame: the wood behind the
gargoyles, spikes and gem, the outside edges, the wood between the hanging
ivy and cloth, and the inside of the two side pockets. The middle text
panel stays solid. Writes Assets/UI/menu_frame.png (then run
Tools/build_hud_frame.py).

Usage (needs opencv-python-headless and numpy):
    python Tools/cut_menu_frame.py path/to/new_menu_frame.png

The numbers are measured on the 2012x782 frame from ChatGPT (frame bars,
pocket insides, spikes); a differently drawn frame needs them re-measured.
"""
import cv2, numpy as np, sys
SRC=sys.argv[1]
img=cv2.imread(SRC)
H,W=img.shape[:2]
GC_BGD,GC_FGD,GC_PR_BGD,GC_PR_FGD=0,1,2,3
m=np.full((H,W),GC_PR_BGD,np.uint8)
# likely ornaments: brightish / saturated pixels outside the frame
hsv=cv2.cvtColor(img,cv2.COLOR_BGR2HSV)
v=hsv[...,2].astype(int); sat=hsv[...,1].astype(int)
m[(v>95)|((sat>150)&(v>60))]=GC_PR_FGD
# frame body: definitely solid
m[200:748,108:1904]=GC_FGD
# pocket interiors: definitely clear (inset from the moulding)
def pocket(x0,x1,y0=268,y1=692,r=22):
    p=np.zeros((H,W),np.uint8); cv2.rectangle(p,(x0+r,y0),(x1-r,y1),255,-1); cv2.rectangle(p,(x0,y0+r),(x1,y1-r),255,-1)
    for cx,cy in [(x0+r,y0+r),(x1-r,y0+r),(x0+r,y1-r),(x1-r,y1-r)]: cv2.circle(p,(cx,cy),r,255,-1)
    return p>0
pl=pocket(138,342); pr=pocket(1671,1868)
m[pl]=GC_BGD; m[pr]=GC_BGD
for cx in (385,1622):
    cv2.fillPoly(m,[np.array([(cx,64),(cx-20,165),(cx-28,200),(cx+28,200),(cx+20,165)],np.int32)],GC_FGD)
# plain wood strips far from ornaments: definitely clear
m[0:12,:]=GC_BGD
m[752:,:]=np.where(m[752:,:]==GC_PR_FGD,GC_PR_FGD,GC_BGD)
for x0,x1 in [(250,320),(610,700),(1310,1400),(1690,1740)]:
    m[0:150,x0:x1]=GC_BGD
bgd=np.zeros((1,65));fgd=np.zeros((1,65))
cv2.grabCut(img,m,None,bgd,fgd,6,cv2.GC_INIT_WITH_MASK)
fg=((m==GC_FGD)|(m==GC_PR_FGD)).astype(np.uint8)*255
fg[pl]=0; fg[pr]=0
# keep only parts connected to the frame; fill small holes
n,lab,st,_=cv2.connectedComponentsWithStats(fg)
main=lab[400,1000]; fg=np.where(lab==main,255,0).astype(np.uint8)
inv=255-fg; n,lab,st,_=cv2.connectedComponentsWithStats(inv)
for i in range(1,n):
    if st[i,cv2.CC_STAT_AREA]<1500: fg[lab==i]=255
fg[pl]=0; fg[pr]=0
# In the hanging ivy/cloth strips at the sides (below the gargoyles), drop
# dark wood between the leaves: keep only bright, red or green pixels.
h=hsv[...,0].astype(int)
keep=(v>=40)|(((h<7)|(h>168))&(v>=20))|((h>=17)&(h<=65)&(v>=26))
keep=cv2.morphologyEx(keep.astype(np.uint8)*255,cv2.MORPH_CLOSE,cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(7,7)))>0
for x0,x1,y0,y1 in [(0,114,300,H),(1898,W,300,H),(1975,W,140,H),(0,22,140,H),(430,600,0,203)]:
    zone=np.zeros((H,W),bool); zone[y0:y1,x0:x1]=True
    fg[zone & ~keep]=0
fg[:,0:4]=0; fg[:,W-4:]=0
n,lab,st,_=cv2.connectedComponentsWithStats(fg)
fg=np.where(lab==lab[400,1000],255,0).astype(np.uint8)
inv=255-fg; n,lab,st,_=cv2.connectedComponentsWithStats(inv)
for i in range(1,n):
    if st[i,cv2.CC_STAT_AREA]<120: fg[lab==i]=255
fg[pl]=0; fg[pr]=0
alpha=cv2.GaussianBlur(fg.astype(np.float32),(0,0),1.0)
rgba=np.dstack([img,np.clip(alpha,0,255).astype(np.uint8)])
cv2.imwrite('Assets/UI/menu_frame.png',rgba)
print('wrote Assets/UI/menu_frame.png', rgba.shape[1], 'x', rgba.shape[0])
