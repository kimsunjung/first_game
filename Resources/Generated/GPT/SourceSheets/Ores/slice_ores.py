"""Slice the ChatGPT-generated 4x2 ore sheet into 8 transparent ore PNGs.

Source sheet: source_ores_2026_05_13.png (1774x887, 4 cols x 2 rows).
Ordering (left->right, top->bottom):
    Iron, Silver, Gold, Platinum, Copper, Mithril, Ruby, Sapphire.

Background removal:
    - Convert to RGBA.
    - Treat near-white pixels (R,G,B > WHITE_THRESHOLD) as transparent.
    - Treat the checker-grid light gray pixels as transparent. The grid uses
      two near-equal gray tones; we detect by (max(channel)-min(channel)) < 8
      and brightness within GRID_GRAY_RANGE.
    - Conservative thresholds to avoid eating real ore body pixels.

Output:
    Each ore is cropped to its opaque bounding box, then centered onto a
    transparent CANVAS_SIZE x CANVAS_SIZE square (preserving aspect ratio
    via the longer side; resized with LANCZOS).
"""

from __future__ import annotations

import os
import shutil
from pathlib import Path

from PIL import Image

HERE = Path(__file__).resolve().parent
WORKTREE = HERE.parents[4]  # .../agent-xxx (Resources/Generated/GPT/SourceSheets/Ores -> ../../../../..)
ORIGINAL_SHEET = Path("/Users/ksj/personal/project/first_game/ChatGPT Image 2026년 5월 13일 오후 03_49_38.png")
SHEET_COPY = HERE / "source_ores_2026_05_13.png"
OUT_DIR = WORKTREE / "Resources" / "Generated" / "GPT" / "Icons" / "Ores"

COLS = 4
ROWS = 2
CANVAS_SIZE = 128
PADDING = 6  # px padding on each side inside the canvas

WHITE_THRESHOLD = 238  # any channel <= 238 keeps pixel
GRID_GRAY_MIN = 200    # the checker grid is fairly light
GRID_GRAY_MAX = 240
GRID_CHROMA_MAX = 6    # max(R,G,B) - min(R,G,B) must be small for "gray"

ORE_NAMES = [
    "iron_ore",
    "silver_ore",
    "gold_ore",
    "platinum_ore",
    "copper_ore",
    "mithril_ore",
    "ruby_ore",
    "sapphire_ore",
]


def ensure_dirs() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    HERE.mkdir(parents=True, exist_ok=True)


def copy_original() -> None:
    if not SHEET_COPY.exists():
        shutil.copy2(ORIGINAL_SHEET, SHEET_COPY)


def remove_background(cell: Image.Image) -> Image.Image:
    """Return RGBA copy of `cell` with white + checker-grid pixels made transparent."""
    rgba = cell.convert("RGBA")
    px = rgba.load()
    w, h = rgba.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            # Near-white => transparent
            if r >= WHITE_THRESHOLD and g >= WHITE_THRESHOLD and b >= WHITE_THRESHOLD:
                px[x, y] = (r, g, b, 0)
                continue
            # Checker-grid light gray => transparent
            mx, mn = max(r, g, b), min(r, g, b)
            if (mx - mn) <= GRID_CHROMA_MAX and GRID_GRAY_MIN <= mn and mx <= GRID_GRAY_MAX:
                px[x, y] = (r, g, b, 0)
    return rgba


def crop_to_content(img: Image.Image) -> Image.Image:
    bbox = img.getbbox()
    if bbox is None:
        return img
    return img.crop(bbox)


def fit_to_canvas(img: Image.Image, size: int = CANVAS_SIZE, padding: int = PADDING) -> Image.Image:
    target = size - padding * 2
    w, h = img.size
    scale = min(target / w, target / h)
    new_w = max(1, int(round(w * scale)))
    new_h = max(1, int(round(h * scale)))
    resized = img.resize((new_w, new_h), Image.LANCZOS)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    canvas.paste(resized, ((size - new_w) // 2, (size - new_h) // 2), resized)
    return canvas


def slice_sheet() -> list[tuple[str, Path, tuple[int, int]]]:
    img = Image.open(SHEET_COPY).convert("RGBA")
    W, H = img.size
    cw, ch = W // COLS, H // ROWS
    results: list[tuple[str, Path, tuple[int, int]]] = []
    for idx, name in enumerate(ORE_NAMES):
        col = idx % COLS
        row = idx // COLS
        left = col * cw
        upper = row * ch
        right = left + cw
        lower = upper + ch
        cell = img.crop((left, upper, right, lower))
        transparent = remove_background(cell)
        cropped = crop_to_content(transparent)
        final = fit_to_canvas(cropped)
        out_path = OUT_DIR / f"{name}.png"
        final.save(out_path, "PNG")
        results.append((name, out_path, final.size))
    return results


def main() -> None:
    ensure_dirs()
    copy_original()
    results = slice_sheet()
    for name, path, size in results:
        print(f"{name}: {path} {size}")


if __name__ == "__main__":
    main()
