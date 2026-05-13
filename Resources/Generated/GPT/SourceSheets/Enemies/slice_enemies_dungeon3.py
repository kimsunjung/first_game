"""
dungeon_3 적 스프라이트 시트 (4 columns × 2 rows) → 8개 PNG 슬라이싱.

- 원본 시트: source_enemies_dungeon3_2026_05_13.png
- 균등 4×2 셀 슬라이싱.
- 배경 제거: 흰 / 체크무늬(연·진 회색 격자) → 알파 0.
- 각 셀의 캐릭터 본체를 추출한 뒤 동일한 정사각 캔버스(128×128)에 중앙 정렬.
- 출력: ../../Enemies/Dungeon3/<name>.png (8개)

사용법:
    python3 slice_enemies_dungeon3.py
"""

from pathlib import Path

from PIL import Image

SCRIPT_DIR = Path(__file__).resolve().parent
SOURCE_SHEET = SCRIPT_DIR / "source_enemies_dungeon3_2026_05_13.png"
OUT_DIR = SCRIPT_DIR.parents[1] / "Enemies" / "Dungeon3"

# 좌→우, 위→아래 순서
NAMES = [
    "abyss_wraith",
    "shadow_assassin",
    "death_knight",
    "cursed_warlock",
    "bone_golem",
    "abyss_hound",
    "ancient_lich",
    "dungeon_guardian",
]

CANVAS = 128
COLS, ROWS = 4, 2


def is_background(r: int, g: int, b: int, a: int) -> bool:
    if a < 8:
        return True
    if r >= 240 and g >= 240 and b >= 240:
        return True
    if abs(r - g) <= 6 and abs(g - b) <= 6 and abs(r - b) <= 6:
        if 175 <= r <= 245:
            return True
    return False


def strip_background(cell: Image.Image) -> Image.Image:
    cell = cell.convert("RGBA")
    pixels = cell.load()
    w, h = cell.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            if is_background(r, g, b, a):
                pixels[x, y] = (0, 0, 0, 0)
    return cell


def keep_largest_blob(img: Image.Image) -> Image.Image:
    """알파>0 픽셀로 connected component를 구해 가장 큰 덩어리만 남기고 나머지는 알파 0.
    이웃 셀에서 흘러들어온 캐릭터 파편을 제거하기 위함."""
    img = img.convert("RGBA")
    px = img.load()
    w, h = img.size
    label = [[0] * w for _ in range(h)]
    next_label = 0
    sizes: dict[int, int] = {}
    bboxes: dict[int, tuple[int, int, int, int]] = {}

    # 4-방향 BFS로 라벨링
    from collections import deque
    for y in range(h):
        for x in range(w):
            if label[y][x] != 0:
                continue
            _, _, _, a = px[x, y]
            if a < 1:
                continue
            next_label += 1
            q = deque([(x, y)])
            label[y][x] = next_label
            count = 0
            min_x = max_x = x
            min_y = max_y = y
            while q:
                cx, cy = q.popleft()
                count += 1
                if cx < min_x: min_x = cx
                if cx > max_x: max_x = cx
                if cy < min_y: min_y = cy
                if cy > max_y: max_y = cy
                for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                    if 0 <= nx < w and 0 <= ny < h and label[ny][nx] == 0:
                        _, _, _, na = px[nx, ny]
                        if na > 0:
                            label[ny][nx] = next_label
                            q.append((nx, ny))
            sizes[next_label] = count
            bboxes[next_label] = (min_x, min_y, max_x + 1, max_y + 1)

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


def trim_to_alpha(img: Image.Image) -> Image.Image:
    bbox = img.getbbox()
    if bbox is None:
        return img
    return img.crop(bbox)


def fit_into_square(img: Image.Image, size: int) -> Image.Image:
    w, h = img.size
    if w == 0 or h == 0:
        return Image.new("RGBA", (size, size), (0, 0, 0, 0))
    scale = min(size / w, size / h)
    new_w = max(1, int(round(w * scale)))
    new_h = max(1, int(round(h * scale)))
    resized = img.resize((new_w, new_h), Image.LANCZOS)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    canvas.paste(resized, ((size - new_w) // 2, (size - new_h) // 2), resized)
    return canvas


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    sheet = Image.open(SOURCE_SHEET).convert("RGBA")
    sw, sh = sheet.size
    cell_w = sw // COLS
    cell_h = sh // ROWS

    print(f"sheet={sw}x{sh}, cell={cell_w}x{cell_h}")

    for idx, name in enumerate(NAMES):
        col = idx % COLS
        row = idx // COLS
        box = (col * cell_w, row * cell_h, (col + 1) * cell_w, (row + 1) * cell_h)
        cell = sheet.crop(box)
        cleaned = strip_background(cell)
        isolated = keep_largest_blob(cleaned)
        trimmed = trim_to_alpha(isolated)
        final = fit_into_square(trimmed, CANVAS)
        out_path = OUT_DIR / f"{name}.png"
        final.save(out_path, "PNG")
        print(f"  [{idx}] {name}.png  src={cell.size}  trimmed={trimmed.size}  -> {final.size}")


if __name__ == "__main__":
    main()
