"""광산/던전/필드 환경 오브젝트 시트 (4x2) -> 8개 PNG. 체커보드(거의 흰색) 배경 제거. 출력은 Objects/{Mine,Dungeon,Field}."""
from collections import deque
from pathlib import Path
from PIL import Image

SCRIPT_DIR = Path(__file__).resolve().parent
SOURCE_SHEET = SCRIPT_DIR / "source_environment_objects_mine_dungeon_field_2026_05_15.png"
OBJECTS_ROOT = SCRIPT_DIR.parents[1] / "Objects"

ITEMS = [
    ("Mine", "crystal_vein"),
    ("Mine", "mine_cart"),
    ("Mine", "ore_crates"),
    ("Dungeon", "dungeon_brazier"),
    ("Dungeon", "broken_pillar"),
    ("Dungeon", "rune_stone"),
    ("Field", "field_camp_tent"),
    ("Field", "signpost_waypoint"),
]
CANVAS, COLS, ROWS = 128, 4, 2


def is_bg(r, g, b, a):
    if a < 8:
        return True
    if r >= 235 and g >= 235 and b >= 235 and abs(r - g) <= 8 and abs(g - b) <= 8 and abs(r - b) <= 8:
        return True
    return False


def strip_bg(cell):
    cell = cell.convert("RGBA")
    px = cell.load()
    w, h = cell.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if is_bg(r, g, b, a):
                px[x, y] = (0, 0, 0, 0)
    return cell


def largest_blob(img):
    img = img.convert("RGBA")
    px = img.load()
    w, h = img.size
    label = [[0] * w for _ in range(h)]
    n = 0
    sizes = {}
    for y in range(h):
        for x in range(w):
            if label[y][x] != 0:
                continue
            if px[x, y][3] < 1:
                continue
            n += 1
            q = deque([(x, y)])
            label[y][x] = n
            cnt = 0
            while q:
                cx, cy = q.popleft()
                cnt += 1
                for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                    if 0 <= nx < w and 0 <= ny < h and label[ny][nx] == 0 and px[nx, ny][3] > 0:
                        label[ny][nx] = n
                        q.append((nx, ny))
            sizes[n] = cnt
    if not sizes:
        return img
    keep = max(sizes, key=lambda k: sizes[k])
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    op = out.load()
    for y in range(h):
        for x in range(w):
            if label[y][x] == keep:
                op[x, y] = px[x, y]
    return out


def fit_square(img, size):
    w, h = img.size
    if w == 0 or h == 0:
        return Image.new("RGBA", (size, size), (0, 0, 0, 0))
    scale = min(size / w, size / h)
    nw, nh = max(1, int(round(w * scale))), max(1, int(round(h * scale)))
    r = img.resize((nw, nh), Image.LANCZOS)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    canvas.paste(r, ((size - nw) // 2, (size - nh) // 2), r)
    return canvas


def main():
    sheet = Image.open(SOURCE_SHEET).convert("RGBA")
    sw, sh = sheet.size
    cw, ch = sw // COLS, sh // ROWS
    print(f"sheet={sw}x{sh}, cell={cw}x{ch}")
    for i, (subdir, name) in enumerate(ITEMS):
        col, row = i % COLS, i // COLS
        box = (col * cw, row * ch, (col + 1) * cw, (row + 1) * ch)
        cell = sheet.crop(box)
        cleaned = strip_bg(cell)
        iso = largest_blob(cleaned)
        bbox = iso.getbbox()
        trimmed = iso.crop(bbox) if bbox else iso
        final = fit_square(trimmed, CANVAS)
        out_dir = OBJECTS_ROOT / subdir
        out_dir.mkdir(parents=True, exist_ok=True)
        out_path = out_dir / f"{name}.png"
        if out_path.exists():
            raise SystemExit(f"refuse to overwrite existing {out_path}")
        final.save(out_path, "PNG")
        print(f"  [{i}] {subdir}/{name}.png  trimmed={trimmed.size}")


if __name__ == "__main__":
    main()
