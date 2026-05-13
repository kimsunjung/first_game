"""
광산 적 스프라이트 시트 (4 columns × 2 rows) → 8개 PNG 슬라이싱.

- 원본 시트: source_enemies_mine_2026_05_13.png (worktree 내 사본)
- 균등 4×2 셀 슬라이싱.
- 배경 제거: 흰 / 체크무늬(연·진 회색 격자) → 알파 0.
- 각 셀의 캐릭터 본체를 추출한 뒤 동일한 정사각 캔버스(128×128)에 중앙 정렬.
- 출력: ../../Enemies/Mine/<name>.png (8개)

사용법:
    python3 slice_enemies.py
"""

from pathlib import Path

from PIL import Image

SCRIPT_DIR = Path(__file__).resolve().parent
SOURCE_SHEET = SCRIPT_DIR / "source_enemies_mine_2026_05_13.png"
OUT_DIR = SCRIPT_DIR.parents[1] / "Enemies" / "Mine"

# 좌→우, 위→아래 순서
NAMES = [
    "zombie_basic",
    "zombie_fast",
    "zombie_armored",
    "zombie_brute",
    "cave_bat",
    "rock_golem",
    "skeleton_miner",
    "mine_wraith",
]

CANVAS = 128  # 출력 정사각 캔버스 크기
COLS, ROWS = 4, 2


def is_background(r: int, g: int, b: int, a: int) -> bool:
    """체크무늬/흰색 배경 판정. 보수적 — 캐릭터 본체 픽셀 보호."""
    if a < 8:
        return True
    # 거의 흰색
    if r >= 240 and g >= 240 and b >= 240:
        return True
    # 체크무늬 회색 격자: R≈G≈B, 채도 거의 0, 밝기 175~245 사이
    if abs(r - g) <= 6 and abs(g - b) <= 6 and abs(r - b) <= 6:
        # 무채색 픽셀
        if 175 <= r <= 245:
            return True
    return False


def strip_background(cell: Image.Image) -> Image.Image:
    """배경 픽셀의 알파를 0으로."""
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
    """알파 0 가장자리 잘라내기. 완전 투명이면 원본 반환."""
    bbox = img.getbbox()
    if bbox is None:
        return img
    return img.crop(bbox)


def fit_into_square(img: Image.Image, size: int) -> Image.Image:
    """본체를 정사각 캔버스(size×size)에 비율 유지하며 중앙 정렬."""
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
