"""중복 갑옷+신규 4종 시트 (4×2). 상단 4(중복 갑옷) → Unused/Duplicates, 하단 4(신규) → Icons/Equipment."""
from collections import deque
from pathlib import Path
from PIL import Image

SCRIPT_DIR = Path(__file__).resolve().parent
SOURCE_SHEET = SCRIPT_DIR / "source_equipment_partial_duplicates_2026_05_13.png"
EQUIP_DIR = SCRIPT_DIR.parents[1] / "Icons" / "Equipment"
DUP_DIR = SCRIPT_DIR.parents[1] / "Icons" / "Unused" / "Duplicates"
TARGETS = [
    ("dup_chainmail_armor",   DUP_DIR),
    ("dup_knight_plate_armor",DUP_DIR),
    ("dup_ranger_vest",       DUP_DIR),
    ("dup_mystic_robe",       DUP_DIR),
    ("battle_helm",           EQUIP_DIR),
    ("shadow_boots",          EQUIP_DIR),
    ("emerald_ring",          EQUIP_DIR),
    ("sun_amulet",            EQUIP_DIR),
]
CANVAS, COLS, ROWS = 64, 4, 2
LEFT_PAD, RIGHT_PAD = 30, 30


def is_bg(r,g,b,a):
    if a < 8: return True
    if r >= 240 and g >= 240 and b >= 240: return True
    if abs(r-g)<=6 and abs(g-b)<=6 and abs(r-b)<=6 and 175<=r<=245: return True
    return False


def strip_bg(cell):
    cell = cell.convert("RGBA"); px = cell.load(); w,h = cell.size
    for y in range(h):
        for x in range(w):
            r,g,b,a = px[x,y]
            if is_bg(r,g,b,a):
                px[x,y] = (0,0,0,0)
    return cell


def largest_blob(img):
    img = img.convert("RGBA"); px = img.load(); w,h = img.size
    label = [[0]*w for _ in range(h)]; n = 0; sizes = {}
    for y in range(h):
        for x in range(w):
            if label[y][x] != 0: continue
            if px[x,y][3] < 1: continue
            n += 1; q = deque([(x,y)]); label[y][x] = n; cnt = 0
            while q:
                cx,cy = q.popleft(); cnt += 1
                for nx,ny in ((cx+1,cy),(cx-1,cy),(cx,cy+1),(cx,cy-1)):
                    if 0<=nx<w and 0<=ny<h and label[ny][nx]==0 and px[nx,ny][3]>0:
                        label[ny][nx] = n; q.append((nx,ny))
            sizes[n] = cnt
    if not sizes: return img
    keep = max(sizes, key=lambda k: sizes[k])
    out = Image.new("RGBA",(w,h),(0,0,0,0)); op = out.load()
    for y in range(h):
        for x in range(w):
            if label[y][x] == keep: op[x,y] = px[x,y]
    return out


def fit_square(img, size):
    w,h = img.size
    if w==0 or h==0: return Image.new("RGBA",(size,size),(0,0,0,0))
    scale = min(size/w, size/h)
    nw,nh = max(1,int(round(w*scale))), max(1,int(round(h*scale)))
    r = img.resize((nw,nh), Image.LANCZOS)
    canvas = Image.new("RGBA",(size,size),(0,0,0,0))
    canvas.paste(r, ((size-nw)//2,(size-nh)//2), r)
    return canvas


def main():
    EQUIP_DIR.mkdir(parents=True, exist_ok=True)
    DUP_DIR.mkdir(parents=True, exist_ok=True)
    sheet = Image.open(SOURCE_SHEET).convert("RGBA")
    sw,sh = sheet.size; cw,ch = sw//COLS, sh//ROWS
    print(f"sheet={sw}x{sh}, cell={cw}x{ch}")
    for i,(name,outd) in enumerate(TARGETS):
        col, row = i%COLS, i//COLS
        x0 = max(0, col*cw - LEFT_PAD); x1 = min(sw, (col+1)*cw + RIGHT_PAD)
        box = (x0, row*ch, x1, (row+1)*ch)
        cell = sheet.crop(box)
        cleaned = strip_bg(cell); iso = largest_blob(cleaned)
        bbox = iso.getbbox(); trimmed = iso.crop(bbox) if bbox else iso
        final = fit_square(trimmed, CANVAS)
        final.save(outd / f"{name}.png", "PNG")
        print(f"  [{i}] {name}.png  trimmed={trimmed.size}  -> {outd.name}")


if __name__ == "__main__":
    main()
