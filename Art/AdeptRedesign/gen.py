from PIL import Image
import math
P={'o':(18,8,26,255),'d':(40,20,56,255),'m':(70,36,96,255),'l':(104,68,130,255),'b':(168,152,118,255),'g':(232,200,106,255),'G':(255,240,176,255)}
HW=[0,1,2,3,3,4,5,5,5,5,6,6,6,6,6]  # half widths rows 0..14
def frame(sway=0.0,bob=0,phase=0,spread=0,glow=0,squash=0,lift=0,trail=0,eyes=1):
    g=[[None]*16 for _ in range(16)]
    top=1+bob+squash-lift
    rows=list(range(1,15))
    for i,r in enumerate(rows):
        y=top+i if i<6 else top+6+int((i-6)*(1-squash*0.12))
        if y<0 or y>15: continue
        hw=HW[r]+(spread if 6<=r<=11 else 0)+(1 if squash and r>9 else 0)
        cx=8+round(sway*(1-i/13.0))  # top sways, hem lags
        for x in range(cx-hw,cx+hw):
            if r>=11:  # ragged hem
                k=(x*7+phase*3+r*5)%6
                if r==11 and k==0: continue
                if r>=12 and k>1+(r-12): continue
            if 0<=x<16: g[y][x]='m'
    # drips below hem
    hemy=max(y for y in range(16) if any(g[y]))
    for x in range(16):
        if g[hemy][x] and (x*5+phase)%4==0:
            for t in range(1,2+((x+phase)%2)+trail):
                if hemy+t<16: g[hemy+t][x]='d'
    # shading
    for y in range(16):
        xs=[x for x in range(16) if g[y][x]]
        if not xs: continue
        mid=(xs[0]+xs[-1])/2
        for x in xs:
            if g[y][x]=='m':
                if x<mid-1 and y<top+9: g[y][x]='l'
                elif x>mid+1: g[y][x]='d'
    # outline boundary
    filled=lambda x,y:0<=x<16 and 0<=y<16 and g[y][x] is not None
    out=[[g[y][x] for x in range(16)] for y in range(16)]
    for y in range(16):
        for x in range(16):
            if g[y][x] and g[y][x]!='d' and not all(filled(x+a,y+b) for a,b in((1,0),(-1,0),(0,1),(0,-1))): out[y][x]='o'
    g=out
    # hood void
    cx=8+round(sway)
    fy=top+3
    for yy in (fy,fy+1,fy+2):
        for x in range(cx-2,cx+1):
            if 0<=yy<16: g[yy][x]='o'
    if eyes and 0<=fy+1<16: g[fy+1][cx-2]='g' if eyes>1 else 'b'; g[fy+1][cx]='g' if eyes>1 else 'b'
    # bone clasp
    if 0<=top+6<16: g[top+6][cx-1]='b'
    if glow:
        hx=min(15,cx+5+spread); hy=top+8
        g[hy][hx-1]='b'
        pts={1:[(0,0)],2:[(0,0),(1,0),(-1,0),(0,1),(0,-1)],3:[(0,-2),(1,-1),(2,0),(1,1),(0,2),(-1,1),(-2,0),(-1,-1),(0,0)]}[glow]
        for a,b in pts:
            x,y=hx+a,hy+b
            if 0<=x<16 and 0<=y<16: g[y][x]='G' if (a,b)==(0,0) else 'g'
    return g
def put(img,g,c,r):
    ox,oy=32*c+8,32*r+16
    for y in range(16):
        for x in range(16):
            if g[y][x]: img.putpixel((ox+x,oy+y),P[g[y][x]])
F={}
F['idle']=[frame(bob=b,phase=p,eyes=e) for b,p,e in [(0,0,1),(0,1,1),(1,2,1),(1,3,2),(0,4,1),(0,5,1)]]
F['walk']=[frame(sway=s,bob=b,phase=i,trail=1) for i,(s,b) in enumerate([(0,0),(1,0),(1,1),(0,1),(-1,0),(-1,1)])]
F['cast']=[frame(spread=sp,glow=gl,eyes=e,phase=i) for i,(sp,gl,e) in enumerate([(0,0,1),(0,1,1),(1,2,2),(1,3,2),(1,2,2),(0,1,1)])]
F['hop']=[frame(squash=1),frame(lift=2,trail=1,phase=1),frame(lift=1,trail=1,phase=2),frame(squash=1,phase=3)]
img=Image.new('RGBA',(192,384))
seq=F['idle']+F['walk']
for i,g in enumerate(seq): put(img,g,i%6,i//6)
for r in range(2,5): # generic rows
    for c in range(6): put(img,frame(sway=[0,1,-1][r%3],phase=c+r,bob=c%2),c,r)
for c in range(6): put(img,F['cast'][c],c,5)
for r in range(6,10):
    for c in range(6): put(img,frame(spread=r%2,phase=c+r,eyes=1+(c==3),glow=(c%3) if r==8 else 0),c,r)
for c in range(4): put(img,F['hop'][c],c,10)
for c in range(4): put(img,frame(phase=c,eyes=0,bob=1),c,11)
img.save('Adept.png'); img.resize((192*6,384*6),Image.NEAREST).save('Adept_x6.png')
for n,fps in [('idle',5),('walk',8),('cast',4),('hop',7)]:
    fr=[]
    for g in F[n]:
        im=Image.new('RGBA',(16,16),(60,60,70,255))
        for y in range(16):
            for x in range(16):
                if g[y][x]: im.putpixel((x,y),P[g[y][x]])
        fr.append(im.resize((128,128),Image.NEAREST).convert('P'))
    fr[0].save(f'{n}.gif',save_all=True,append_images=fr[1:],duration=int(1000/fps),loop=0)
# contact sheet of anim frames
cs=Image.new('RGBA',(16*6,16*4),(60,60,70,255))
for r,n in enumerate(['idle','walk','cast','hop']):
    for c,g in enumerate(F[n]):
        for y in range(16):
            for x in range(16):
                if g[y][x]: cs.putpixel((c*16+x,r*16+y),P[g[y][x]])
cs.resize((96*8,64*8),Image.NEAREST).save('preview_anims_x8.png')
