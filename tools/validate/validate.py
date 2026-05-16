#!/usr/bin/env python3
"""
프로젝트 무결성·정합성 검사기 (Godot 런타임 불필요, 순수 텍스트 파싱).

검사 항목:
  R1  모든 res:// 참조가 실제 파일로 해결되는가
  R2  리소스 자체 uid(헤더 uid)가 프로젝트 전역에서 유일한가
  R3  EnemySpawner 표준 API만 사용 / 폐기 API(EnemyStatsList 등) 미사용 / StatVariants 비어있지 않음
  R4  game_balance.json zones ↔ Scenes/Maps 1:1 정합 (town 등 허브 예외)
  R5  skillbook_*.tres → LearnedSkill 이 실존 스킬 리소스를 가리킴
  R6  적 .tres 의 PossibleDrops 개수 == DropWeights 개수
  R7  Resources/Teleport/dest_*.tres 의 ScenePath 가 실존 씬
  R8  사냥터 씬은 EnemySpawner 를 포함 (허브 3곳 제외)

종료 코드: 결함 0건이면 0, 아니면 1.
"""
import json
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))

# 검사 대상에서 제외할 디렉터리 (엔진 캐시 / 빌드 산출물 / 서드파티)
EXCLUDE_DIRS = {".godot", ".git", "android", "addons", "bin", "obj", "tools"}

# 엔진/내장 경로는 res:// 로 와도 파일이 없으므로 무결성 검사에서 제외
RES_IGNORE_PREFIXES = ("res://addons/",)

# 허브 씬: 적 스포너가 없는 게 정상
HUB_SCENES = {"town", "harbor_village", "mountain_refuge"}

# EnemySpawner 폐기/파손 export (남아 있으면 적이 스폰되지 않음)
DEPRECATED_SPAWNER_TOKENS = ("EnemyStatsList", "SpawnCount", "SpawnAreaSize", "SpawnAreaOffset")

errors = []
warnings = []


def err(category, msg):
    errors.append((category, msg))


def walk_files(exts):
    for dirpath, dirnames, filenames in os.walk(ROOT):
        dirnames[:] = [d for d in dirnames if d not in EXCLUDE_DIRS]
        for fn in filenames:
            if os.path.splitext(fn)[1] in exts:
                yield os.path.join(dirpath, fn)


def res_to_path(res):
    """res://Foo/Bar.tres -> 절대 경로"""
    rel = res[len("res://"):]
    return os.path.join(ROOT, rel)


# ── uid 전역 맵 + 자체 uid 유일성 (R2) ───────────────────────────────
HEADER_UID_RE = re.compile(r'^\[gd_(?:resource|scene)\b[^\]]*\buid="(uid://[^"]+)"')
EXTRES_RE = re.compile(r'^\[ext_resource\b([^\]]*)\]')
ATTR_RE = re.compile(r'(\w+)="([^"]*)"')

uid_to_file = {}          # uid -> 그 uid 를 선언한 리소스 파일 (헤더 uid)
header_uid_owner = {}     # 자체 uid 중복 검출용


def scan_uids():
    for path in walk_files({".tres", ".tscn"}):
        try:
            with open(path, encoding="utf-8") as f:
                first = f.readline()
        except (OSError, UnicodeDecodeError) as e:
            err("R0", f"읽기 실패: {rel(path)} ({e})")
            continue
        m = HEADER_UID_RE.search(first)
        if not m:
            continue
        uid = m.group(1)
        if uid in header_uid_owner:
            err("R2", f"uid 중복 '{uid}': {rel(header_uid_owner[uid])} ↔ {rel(path)}")
        else:
            header_uid_owner[uid] = path
            uid_to_file[uid] = path


def rel(p):
    return os.path.relpath(p, ROOT)


# ── R1: res:// 참조 해결 + R5/R7 보조용 ext_resource 수집 ──────────────
def parse_ext_resources(text):
    """[(type, uid, path, id), ...] 반환. path 없으면 None."""
    out = []
    for line in text.splitlines():
        m = EXTRES_RE.match(line.strip())
        if not m:
            continue
        attrs = dict(ATTR_RE.findall(m.group(1)))
        out.append((attrs.get("type"), attrs.get("uid"), attrs.get("path"), attrs.get("id")))
    return out


# Godot .tres/.tscn 의 res:// 참조는 항상 큰따옴표 안에 있다 (경로에 공백 가능).
RES_REF_RE = re.compile(r'"(res://[^"]+)"')


def check_res_refs():
    for path in walk_files({".tres", ".tscn"}):
        with open(path, encoding="utf-8") as f:
            text = f.read()
        for ref in set(RES_REF_RE.findall(text)):
            if ref.startswith(RES_IGNORE_PREFIXES):
                continue
            target = res_to_path(ref)
            if not os.path.exists(target):
                err("R1", f"{rel(path)} → 미해결 res:// 경로: {ref}")
        # uid-only ext_resource (path 누락) 도 uid 맵으로 해결되는지 확인
        for typ, uid, p, rid in parse_ext_resources(text):
            if p is None and uid is not None and uid not in uid_to_file:
                err("R1", f"{rel(path)} → uid-only ext_resource 해결 불가: {uid}")


# ── R3 / R8: EnemySpawner ────────────────────────────────────────────
def check_spawners():
    maps_dir = os.path.join(ROOT, "Scenes", "Maps")
    for fn in sorted(os.listdir(maps_dir)):
        if not fn.endswith(".tscn"):
            continue
        name = fn[:-5]
        path = os.path.join(maps_dir, fn)
        with open(path, encoding="utf-8") as f:
            text = f.read()
        has_spawner = "Scripts/Entities/Enemies/EnemySpawner.cs" in text
        if name in HUB_SCENES:
            continue
        if not has_spawner:
            err("R8", f"사냥터 씬에 EnemySpawner 없음: {fn}")
            continue
        for tok in DEPRECATED_SPAWNER_TOKENS:
            if re.search(rf'\b{tok}\s*=', text):
                err("R3", f"{fn}: 폐기된 EnemySpawner export 사용 '{tok}' (적 스폰 불가)")
        sv = re.search(r'\bStatVariants\s*=\s*Array\[[^\]]*\]\(\[(.*?)\]\)', text, re.DOTALL)
        if not re.search(r'\bStatVariants\s*=', text):
            err("R3", f"{fn}: StatVariants 누락 (적 스폰 불가)")
        elif sv is None or len(re.findall(r'ExtResource\(', sv.group(1))) == 0:
            err("R3", f"{fn}: StatVariants 가 빈 배열 (적 스폰 불가)")
        # StatWeights 가 있으면 StatVariants 와 항목 수가 같고 합이 0보다 커야 함.
        sw = re.search(r'\bStatWeights\s*=\s*PackedFloat32Array\(([^)]*)\)', text)
        if sw is not None:
            vals = [float(x) for x in sw.group(1).split(',') if x.strip() != ""]
            n_sv = len(re.findall(r'ExtResource\(', sv.group(1))) if sv else 0
            if len(vals) != n_sv:
                err("R3", f"{fn}: StatWeights {len(vals)}개 ≠ StatVariants {n_sv}개")
            elif sum(vals) <= 0:
                err("R3", f"{fn}: StatWeights 합이 0 이하 (가중치 무효)")
        # BossStatVariant 가 있으면 BossId 도 있어야 first-kill 기록이 동작
        if re.search(r'\bBossStatVariant\s*=', text) and not re.search(r'\bBossId\s*=', text):
            err("R3", f"{fn}: BossStatVariant 지정됐으나 BossId 누락 (보스 처치 기록 불가)")


# ── R4: balance zones ↔ scenes ───────────────────────────────────────
def check_balance_zones():
    bal = os.path.join(ROOT, "Resources", "Balance", "game_balance.json")
    with open(bal, encoding="utf-8") as f:
        data = json.load(f)
    zones = set(data.get("zones", {}).keys())
    maps_dir = os.path.join(ROOT, "Scenes", "Maps")
    scenes = {fn[:-5] for fn in os.listdir(maps_dir) if fn.endswith(".tscn")}

    for z in sorted(zones):
        if z not in scenes:
            err("R4", f"balance zone '{z}' 에 대응하는 Scenes/Maps/{z}.tscn 없음")
    # town(중앙 허브)만 zone 없음이 허용. 그 외 모든 맵은 zone 필요.
    for s in sorted(scenes):
        if s == "town":
            continue
        if s not in zones:
            err("R4", f"씬 '{s}.tscn' 에 game_balance.json zone 항목 없음")
    # zones 의 hpMul/atkMul/expMul 양수 검사
    for z, v in data.get("zones", {}).items():
        for k in ("hpMul", "atkMul", "expMul"):
            if k not in v:
                err("R4", f"zone '{z}' 에 '{k}' 누락")
            elif not isinstance(v[k], (int, float)) or v[k] <= 0:
                err("R4", f"zone '{z}'.{k} 가 양수가 아님: {v.get(k)}")


# ── R5: skillbook → LearnedSkill ─────────────────────────────────────
def check_skillbooks():
    items = os.path.join(ROOT, "Resources", "Items")
    for fn in sorted(os.listdir(items)):
        if not (fn.startswith("skillbook_") and fn.endswith(".tres")):
            continue
        path = os.path.join(items, fn)
        with open(path, encoding="utf-8") as f:
            text = f.read()
        m = re.search(r'LearnedSkill\s*=\s*ExtResource\("([^"]+)"\)', text)
        if not m:
            err("R5", f"{fn}: LearnedSkill 미지정")
            continue
        rid = m.group(1)
        # 해당 id 의 ext_resource path/uid 해결
        found = None
        for typ, uid, p, eid in parse_ext_resources(text):
            if eid == rid:
                found = res_to_path(p) if p else uid_to_file.get(uid)
                break
        if not found or not os.path.exists(found):
            err("R5", f"{fn}: LearnedSkill 가 실존 스킬 리소스로 해결되지 않음 (id={rid})")
        elif os.path.sep + "Skills" + os.path.sep not in found:
            err("R5", f"{fn}: LearnedSkill 이 Resources/Skills 가 아닌 곳을 가리킴: {rel(found)}")


# ── R6: PossibleDrops / DropWeights 개수 일치 ────────────────────────
def count_array_items(arr_text):
    return len(re.findall(r'ExtResource\("', arr_text))


def check_drop_tables():
    enemies = os.path.join(ROOT, "Resources", "Enemies")
    for fn in sorted(os.listdir(enemies)):
        if not fn.endswith(".tres"):
            continue
        path = os.path.join(enemies, fn)
        with open(path, encoding="utf-8") as f:
            text = f.read()
        pm = re.search(r'PossibleDrops\s*=\s*Array\[Resource\]\(\[([^\]]*)\]\)', text)
        if not pm:
            continue
        n_drops = count_array_items(pm.group(1))
        wm = re.search(r'DropWeights\s*=\s*PackedFloat32Array\(([^\)]*)\)', text)
        n_weights = len([x for x in wm.group(1).split(",") if x.strip()]) if wm else 0
        if n_weights and n_drops != n_weights:
            err("R6", f"{fn}: PossibleDrops {n_drops}개 ≠ DropWeights {n_weights}개")


# ── R7: teleport dest ScenePath ──────────────────────────────────────
def check_teleport_dests():
    tp = os.path.join(ROOT, "Resources", "Teleport")
    if not os.path.isdir(tp):
        return
    for fn in sorted(os.listdir(tp)):
        if not (fn.startswith("dest_") and fn.endswith(".tres")):
            continue
        path = os.path.join(tp, fn)
        with open(path, encoding="utf-8") as f:
            text = f.read()
        m = re.search(r'ScenePath\s*=\s*"(res://[^"]+)"', text)
        if not m:
            err("R7", f"{fn}: ScenePath 미지정")
            continue
        if not os.path.exists(res_to_path(m.group(1))):
            err("R7", f"{fn}: ScenePath 미해결 {m.group(1)}")


def main():
    scan_uids()
    check_res_refs()
    check_spawners()
    check_balance_zones()
    check_skillbooks()
    check_drop_tables()
    check_teleport_dests()

    by_cat = {}
    for cat, msg in errors:
        by_cat.setdefault(cat, []).append(msg)

    if not errors:
        print("✅ 검사기 통과 — 결함 0건")
        return 0

    print(f"❌ 결함 {len(errors)}건\n")
    for cat in sorted(by_cat):
        print(f"[{cat}] {len(by_cat[cat])}건")
        for m in by_cat[cat]:
            print(f"  - {m}")
        print()
    return 1


if __name__ == "__main__":
    sys.exit(main())
