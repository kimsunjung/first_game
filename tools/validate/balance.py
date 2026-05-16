#!/usr/bin/env python3
"""
밸런스 정합성 분석기 (Godot 불필요).

  B1  스폰 가능한 적은 PossibleDrops 가 비어있지 않아야 한다 (DropChance>0)
  B2  zone 난이도(hpMul*atkMul) 대비 보상(expMul) 강한 역전 금지
        — A 가 B 보다 1.5배 이상 어려운데 expMul(A) < expMul(B) 면 결함
  B3  zone 의 hpMul/atkMul/expMul 은 모두 양수 (validate.py R4 와 중복 방어)

종료 코드: 결함 0건이면 0, 아니면 1. (경고는 종료코드에 영향 없음)
"""
import glob
import json
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
HUBS = {"town", "harbor_village", "mountain_refuge", "field_outpost"}  # 허브/전이 진입 — 곡선 검사 제외

errors = []
warnings = []


def field(text, key):
    m = re.search(rf'^{key}\s*=\s*(.+)$', text, re.M)
    return m.group(1).strip() if m else None


def check_drops():
    for f in sorted(glob.glob(os.path.join(ROOT, "Resources", "Enemies", "*.tres"))):
        t = open(f, encoding="utf-8").read()
        name = os.path.basename(f)
        pm = re.search(r'PossibleDrops\s*=\s*Array\[Resource\]\(\[([^\]]*)\]\)', t)
        n = len(re.findall(r'ExtResource\("', pm.group(1))) if pm else 0
        dc = field(t, "DropChance")
        dc = float(dc) if dc else 0.0
        if n == 0:
            errors.append(("B1", f"{name}: PossibleDrops 비어있음 (사냥해도 드랍 없음)"))
        elif dc <= 0.0:
            errors.append(("B1", f"{name}: DropChance={dc} (드랍 테이블 있으나 절대 안 떨어짐)"))


def check_curve():
    bal = json.load(open(os.path.join(ROOT, "Resources", "Balance", "game_balance.json"),
                       encoding="utf-8"))
    z = bal["zones"]
    diff = {}
    for k, v in z.items():
        if k in HUBS:
            continue
        hp, atk, exp = v.get("hpMul"), v.get("atkMul"), v.get("expMul")
        if not all(isinstance(x, (int, float)) and x > 0 for x in (hp, atk, exp)):
            errors.append(("B3", f"zone '{k}' mul 값 비정상: hp={hp} atk={atk} exp={exp}"))
            continue
        diff[k] = (hp * atk, exp)

    items = sorted(diff.items(), key=lambda kv: kv[1][0])  # 난이도 오름차순
    for i in range(len(items)):
        ka, (da, ea) = items[i]
        for j in range(i):
            kb, (db, eb) = items[j]
            # A(난이도 큼)가 B보다 1.5배 이상 어려운데 보상이 더 적다 → 강한 역전
            if da >= db * 1.5 and ea < eb - 1e-6:
                errors.append(("B2",
                    f"보상 역전: '{ka}'(난이도 {da:.2f}, exp {ea}) 가 "
                    f"'{kb}'(난이도 {db:.2f}, exp {eb}) 보다 어렵지만 보상이 낮음"))
            # 완만한 역전은 경고만
            elif da > db and ea < eb - 1e-6:
                warnings.append(
                    f"[완만한 역전] '{ka}'(난이도 {da:.2f}, exp {ea}) ≥ "
                    f"'{kb}'(난이도 {db:.2f}, exp {eb})")


def check_bosses():
    """B4: 씬이 BossStatVariant 로 가리키는 보스는 반드시 BossId 가 있어야
    GameManager.RecordBossDefeat 에 기록된다 (first-kill/재출현 게이트·권역 졸업).
    BossId 누락이면 coast/mountain 보스 처치가 영구히 유실됨 → 결함.
    참조 안 된 IsBoss 리소스는 사장 데이터 → 경고."""
    import glob as _g
    bosses = set()
    for f in _g.glob(os.path.join(ROOT, "Resources", "Enemies", "*.tres")):
        if re.search(r'^IsBoss\s*=\s*true', open(f, encoding="utf-8").read(), re.M):
            bosses.add(os.path.basename(f)[:-5])
    referenced = set()
    for s in _g.glob(os.path.join(ROOT, "Scenes", "Maps", "*.tscn")):
        t = open(s, encoding="utf-8").read()
        m = re.search(r'BossStatVariant\s*=\s*ExtResource\("([^"]+)"\)', t)
        if not m:
            continue
        rid = m.group(1)
        em = re.search(
            rf'\[ext_resource[^\]]*path="res://Resources/Enemies/([^"]+)\.tres"[^\]]*id="{re.escape(rid)}"\]',
            t)
        if not em:
            continue
        stem = em.group(1)
        referenced.add(stem)
        if not re.search(r'BossId\s*=\s*"[^"]+"', t):
            errors.append(("B4",
                f"{os.path.basename(s)}: 보스 '{stem}' 참조하나 BossId 누락 "
                f"(처치 기록 불가 → 권역 진행 유실)"))
    for orphan in sorted(bosses - referenced):
        warnings.append(f"[사장 보스 리소스] {orphan}.tres 가 어떤 씬에도 "
                        f"BossStatVariant 로 연결되지 않음 (레거시 추정)")


def main():
    check_drops()
    check_curve()
    check_bosses()

    if warnings:
        print(f"⚠️  경고 {len(warnings)}건 (종료코드 무관, 검토 권장)")
        for w in warnings:
            print(f"  - {w}")
        print()

    if not errors:
        print("✅ 밸런스 분석기 통과 — 결함 0건")
        return 0

    by = {}
    for c, m in errors:
        by.setdefault(c, []).append(m)
    print(f"❌ 밸런스 결함 {len(errors)}건\n")
    for c in sorted(by):
        print(f"[{c}] {len(by[c])}건")
        for m in by[c]:
            print(f"  - {m}")
        print()
    return 1


if __name__ == "__main__":
    sys.exit(main())
