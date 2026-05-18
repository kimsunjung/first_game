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
  R9  recipes.json: id 유일 / material·result 경로 실존 / qty>0·gold>=0 /
      레시피 내 material path 중복 금지 / 재료는 Consumable·Material 타입만
  R10 contracts.json: id 유일 / type 유효 / goal>0 / reward gold·exp>=0 /
      rewardItem 경로 실존 / 타입별 타깃 필수 + enemy/boss 타깃 실존
  R11 사냥 계약 보드: 허브 4곳(town/field_outpost/harbor_village/mountain_refuge)에
      ContractBoardNPC 배치
  R12 MiningNode: 씬 내 노드명 유일 / OreItem 지정 / RespawnChanceOnReentry 0~1

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
        # 권역 독립 드랍 — RegionDrop 지정 시 RegionDropChance 는 (0,1] 범위.
        rc = re.search(r'RegionDropChance\s*=\s*([-\d.]+)', text)
        has_region = "RegionDrop = ExtResource(" in text
        if has_region and rc is None:
            err("R6", f"{fn}: RegionDrop 지정됐으나 RegionDropChance 누락")
        elif rc is not None:
            v = float(rc.group(1))
            if v < 0 or v > 1:
                err("R6", f"{fn}: RegionDropChance 범위 밖 ({v})")
            elif v > 0 and not has_region:
                err("R6", f"{fn}: RegionDropChance>0 인데 RegionDrop 미지정")


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


ITEM_TYPE_RE = re.compile(r'^Type\s*=\s*(\d+)', re.M)
# 제작 재료 허용 ItemType: 0=Consumable, 4=Material.
# 장비/스킬북 금지 — Inventory.ConsumeItems 가 IsEquipped/affix 무시하고 path 만으로
# 세므로, 장비를 재료로 허용하면 장착 중인 장비가 제작에 소모될 수 있다.
SAFE_MATERIAL_TYPES = {0, 4}


def _item_type(res_path):
    p = res_to_path(res_path)
    if not os.path.exists(p):
        return None
    with open(p, encoding="utf-8") as f:
        m = ITEM_TYPE_RE.search(f.read())
    return int(m.group(1)) if m else 0  # Type 미기재 = enum 기본값 0(Consumable)


def check_recipes():
    rp = os.path.join(ROOT, "Resources", "Recipes", "recipes.json")
    if not os.path.isfile(rp):
        return
    try:
        with open(rp, encoding="utf-8") as f:
            data = json.load(f)
    except Exception as e:
        err("R9", f"recipes.json 파싱 실패: {e}")
        return
    seen_ids = set()
    for r in data.get("recipes", []):
        rid = r.get("id", "<무-id>")
        if rid in seen_ids:
            err("R9", f"중복 recipe id: {rid}")
        seen_ids.add(rid)

        gold = r.get("gold", 0)
        if not isinstance(gold, int) or gold < 0:
            err("R9", f"{rid}: gold 는 0 이상 정수여야 함 ({gold})")

        result = r.get("result", {})
        rpath = result.get("path", "")
        rqty = result.get("qty", 0)
        if not rpath or not os.path.exists(res_to_path(rpath)):
            err("R9", f"{rid}: result.path 미해결 {rpath}")
        if not isinstance(rqty, int) or rqty <= 0:
            err("R9", f"{rid}: result.qty 는 1 이상이어야 함 ({rqty})")

        mats = r.get("materials", [])
        if not mats:
            err("R9", f"{rid}: materials 비어있음")
        seen_paths = set()
        for m in mats:
            mpath = m.get("path", "")
            mqty = m.get("qty", 0)
            if not mpath or not os.path.exists(res_to_path(mpath)):
                err("R9", f"{rid}: material.path 미해결 {mpath}")
                continue
            if not isinstance(mqty, int) or mqty <= 0:
                err("R9", f"{rid}: material.qty 는 1 이상이어야 함 ({mpath} {mqty})")
            if mpath in seen_paths:
                err("R9", f"{rid}: 같은 material path 중복 기재 {mpath}")
            seen_paths.add(mpath)
            t = _item_type(mpath)
            if t is not None and t not in SAFE_MATERIAL_TYPES:
                err("R9", f"{rid}: 재료에 비안전 타입(Type={t}) {mpath} "
                          "— Consumable/Material 만 허용")


CONTRACT_TYPES = {"Kill", "Gather", "BossKill", "Mining"}
HUB_BOARD_SCENES = ("town", "field_outpost", "harbor_village", "mountain_refuge")


def _collect_enemy_type_names():
    """씬 StatVariants/ext_resource로 *실제 스폰되는* 적 리소스의 EnemyTypeName만 모은다.
    레거시·미참조 리소스는 제외 — 계약 타깃이 실제 사냥터에서 사냥 가능한지 검증하기 위함
    (예: skeleton_base.tres('Skeleton')는 어떤 씬도 안 쓰므로 계약 타깃으로 무효)."""
    names = set()
    maps_dir = os.path.join(ROOT, "Scenes", "Maps")
    referenced = set()
    if os.path.isdir(maps_dir):
        for fn in os.listdir(maps_dir):
            if not fn.endswith(".tscn"):
                continue
            with open(os.path.join(maps_dir, fn), encoding="utf-8") as f:
                for m in re.finditer(r'res://Resources/Enemies/([a-zA-Z0-9_]+)\.tres', f.read()):
                    referenced.add(m.group(1))
    enemies = os.path.join(ROOT, "Resources", "Enemies")
    if not os.path.isdir(enemies):
        return names
    for fn in os.listdir(enemies):
        if not fn.endswith(".tres"):
            continue
        if fn[:-5] not in referenced:
            continue  # 어떤 씬에서도 스폰되지 않는 리소스는 제외
        with open(os.path.join(enemies, fn), encoding="utf-8") as f:
            for m in re.finditer(r'EnemyTypeName\s*=\s*"([^"]+)"', f.read()):
                names.add(m.group(1))
    return names


def _collect_boss_ids():
    ids = set()
    maps_dir = os.path.join(ROOT, "Scenes", "Maps")
    for fn in os.listdir(maps_dir):
        if not fn.endswith(".tscn"):
            continue
        with open(os.path.join(maps_dir, fn), encoding="utf-8") as f:
            for m in re.finditer(r'\bBossId\s*=\s*"([^"]+)"', f.read()):
                ids.add(m.group(1))
    return ids


def check_contracts():
    cp = os.path.join(ROOT, "Resources", "Contracts", "contracts.json")
    if not os.path.isfile(cp):
        return
    try:
        with open(cp, encoding="utf-8") as f:
            data = json.load(f)
    except Exception as e:
        err("R10", f"contracts.json 파싱 실패: {e}")
        return

    enemy_names = _collect_enemy_type_names()
    boss_ids = _collect_boss_ids()
    seen = set()
    for c in data.get("contracts", []):
        cid = c.get("id", "<무-id>")
        if cid in seen:
            err("R10", f"중복 contract id: {cid}")
        seen.add(cid)

        ctype = c.get("type", "")
        if ctype not in CONTRACT_TYPES:
            err("R10", f"{cid}: 잘못된 type '{ctype}'")

        goal = c.get("goal", 0)
        if not isinstance(goal, int) or goal <= 0:
            err("R10", f"{cid}: goal 은 1 이상이어야 함 ({goal})")
        for k in ("goldReward", "expReward"):
            v = c.get(k, 0)
            if not isinstance(v, int) or v < 0:
                err("R10", f"{cid}: {k} 는 0 이상 정수여야 함 ({v})")

        rip = c.get("rewardItemPath", "")
        if rip:
            if not os.path.exists(res_to_path(rip)):
                err("R10", f"{cid}: rewardItemPath 미해결 {rip}")
            rq = c.get("rewardItemQuantity", 1)
            if not isinstance(rq, int) or rq <= 0:
                err("R10", f"{cid}: rewardItemQuantity 는 1 이상이어야 함 ({rq})")

        if ctype == "Kill":
            t = c.get("targetEnemyType", "")
            if not t:
                err("R10", f"{cid}: Kill 계약에 targetEnemyType 누락")
            elif enemy_names and t not in enemy_names:
                err("R10", f"{cid}: targetEnemyType '{t}' 가 씬에서 스폰되는 적 "
                          "EnemyTypeName 과 불일치 (사냥 불가 계약)")
        elif ctype == "BossKill":
            t = c.get("targetBossId", "")
            if not t:
                err("R10", f"{cid}: BossKill 계약에 targetBossId 누락")
            elif boss_ids and t not in boss_ids:
                err("R10", f"{cid}: targetBossId '{t}' 가 어떤 씬 BossId 와도 불일치")
        elif ctype == "Gather":
            t = c.get("targetItemPath", "")
            if not t or not os.path.exists(res_to_path(t)):
                err("R10", f"{cid}: Gather targetItemPath 미해결 {t}")
        elif ctype == "Mining":
            t = c.get("targetOreItemPath", "")
            if not t or not os.path.exists(res_to_path(t)):
                err("R10", f"{cid}: Mining targetOreItemPath 미해결 {t}")


def check_contract_boards():
    maps_dir = os.path.join(ROOT, "Scenes", "Maps")
    for hub in HUB_BOARD_SCENES:
        path = os.path.join(maps_dir, hub + ".tscn")
        if not os.path.isfile(path):
            err("R11", f"허브 씬 없음: {hub}.tscn")
            continue
        with open(path, encoding="utf-8") as f:
            text = f.read()
        if "Scenes/Objects/contract_board_npc.tscn" not in text:
            err("R11", f"{hub}.tscn 에 ContractBoardNPC(사냥 계약 보드) 미배치")


# MiningNode 인스턴스 블록: [node name=".." parent=".." instance=ExtResource("..")]
# 다음 노드 헤더 전까지를 본문으로 본다.
def check_mining_nodes():
    maps_dir = os.path.join(ROOT, "Scenes", "Maps")
    for fn in sorted(os.listdir(maps_dir)):
        if not fn.endswith(".tscn"):
            continue
        path = os.path.join(maps_dir, fn)
        with open(path, encoding="utf-8") as f:
            text = f.read()
        if "Scenes/Objects/mining_node.tscn" not in text:
            continue
        # mining_node.tscn 의 ext_resource id 수집
        mnode_ids = set()
        for typ, uid, p, rid in parse_ext_resources(text):
            if p == "res://Scenes/Objects/mining_node.tscn" and rid:
                mnode_ids.add(rid)
        if not mnode_ids:
            continue
        blocks = re.split(r'^\[node ', text, flags=re.M)
        seen_names = {}
        for blk in blocks:
            mh = re.match(r'name="([^"]+)"[^\]]*instance=ExtResource\("([^"]+)"\)', blk)
            if not mh or mh.group(2) not in mnode_ids:
                continue
            nm = mh.group(1)
            if nm in seen_names:
                err("R12", f"{fn}: MiningNode 노드명 중복 '{nm}' (저장 키 충돌)")
            seen_names[nm] = True
            if not re.search(r'\bOreItem\s*=\s*ExtResource\(', blk):
                err("R12", f"{fn}: MiningNode '{nm}' OreItem 미지정")
            rc = re.search(r'\bRespawnChanceOnReentry\s*=\s*([-\d.]+)', blk)
            if rc is not None:
                v = float(rc.group(1))
                if v < 0 or v > 1:
                    err("R12", f"{fn}: MiningNode '{nm}' RespawnChanceOnReentry 범위 밖 ({v})")
            q = re.search(r'\bQuantity\s*=\s*(-?\d+)', blk)
            if q is not None and int(q.group(1)) <= 0:
                err("R12", f"{fn}: MiningNode '{nm}' Quantity 는 1 이상이어야 함 ({q.group(1)})")


def main():
    scan_uids()
    check_res_refs()
    check_spawners()
    check_balance_zones()
    check_skillbooks()
    check_drop_tables()
    check_teleport_dests()
    check_recipes()
    check_contracts()
    check_contract_boards()
    check_mining_nodes()

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
