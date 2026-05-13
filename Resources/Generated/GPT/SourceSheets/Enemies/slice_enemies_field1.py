"""
field_1 적 스프라이트 시트 (4 columns × 2 rows) → 8개 PNG 슬라이싱.

- 원본 시트: source_enemies_field1_2026_05_13.png
- 균등 4×2 셀 슬라이싱.
- 배경 제거: 흰 / 체크무늬(연·진 회색 격자) → 알파 0.
- 각 셀의 캐릭터 본체를 추출한 뒤 동일한 정사각 캔버스(128×128)에 중앙 정렬.
- 출력: ../../Enemies/Field1/<name>.png (8개)

사용법:
    python3 slice_enemies_field1.py
"""

from pathlib import Path

from PIL import Image

SCRIPT_DIR = Path(__file__).resolve().parent
SOURCE_SHEET = SCRIPT_DIR / "source_enemies_field1_2026_05_13.png"
OUT_DIR = SCRIPT_DIR.parents[1] / "Enemies" / "Field1"

# 좌→우, 위→아래 순서
NAMES = [
    "slime",
    "wild_wolf",
    "goblin_scout",
    "orc_scout",
    "wild_boar",
    "forest_spider",
    "forest_spirit",
    "hobgoblin_guard",
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
        trimmed = trim_to_alpha(cleaned)
        final = fit_into_square(trimmed, CANVAS)
        out_path = OUT_DIR / f"{name}.png"
        final.save(out_path, "PNG")
        print(f"  [{idx}] {name}.png  src={cell.size}  trimmed={trimmed.size}  -> {final.size}")


if __name__ == "__main__":
    main()
