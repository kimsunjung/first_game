"""
오크 Idle 스프라이트 시트를 32x32 → 64x64 프레임으로 패딩.
Run/Death(64x64)와 프레임 크기를 통일하여 애니메이션 전환 시 크기 점프 방지.
"""
from PIL import Image
import os

BASE = os.path.join(
    os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))),
    "Resources", "Tilesets", "Pixel Crawler - Free Pack", "Entities", "Mobs"
)

# 패딩 대상: 모든 몹의 Idle-Sheet.png (32x32 → 64x64)
targets = [
    "Orc Crew/Orc/Idle/Idle-Sheet.png",
    "Orc Crew/Orc - Rogue/Idle/Idle-Sheet.png",
    "Orc Crew/Orc - Shaman/Idle/Idle-Sheet.png",
    "Orc Crew/Orc - Warrior/Idle/Idle-Sheet.png",
    "Skeleton Crew/Skeleton - Base/Idle/Idle-Sheet.png",
    "Skeleton Crew/Skeleton - Mage/Idle/Idle-Sheet.png",
    "Skeleton Crew/Skeleton - Rogue/Idle/Idle-Sheet.png",
    "Skeleton Crew/Skeleton - Warrior/Idle/Idle-Sheet.png",
]

for rel_path in targets:
    src_path = os.path.join(BASE, rel_path)
    if not os.path.exists(src_path):
        print(f"SKIP (not found): {rel_path}")
        continue

    img = Image.open(src_path)
    old_fw = img.width // 4  # 4 frames
    old_fh = img.height
    new_fw, new_fh = 64, 64
    frame_count = 4

    if old_fw == new_fw and old_fh == new_fh:
        print(f"SKIP (already 64x64): {rel_path}")
        continue

    new_img = Image.new("RGBA", (new_fw * frame_count, new_fh), (0, 0, 0, 0))
    for i in range(frame_count):
        frame = img.crop((i * old_fw, 0, (i + 1) * old_fw, old_fh))
        # 중앙 하단 배치
        x_offset = (new_fw - old_fw) // 2
        y_offset = new_fh - old_fh
        new_img.paste(frame, (i * new_fw + x_offset, y_offset))

    out_path = src_path.replace(".png", "_padded.png")
    new_img.save(out_path)
    print(f"OK: {rel_path} ({old_fw}x{old_fh} -> {new_fw}x{new_fh}) => {os.path.basename(out_path)}")

print("\nDone!")
