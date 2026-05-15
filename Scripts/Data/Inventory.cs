using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Core;
using FirstGame.Core.Interfaces;

namespace FirstGame.Data
{
    public class InventorySlot
    {
        public ItemData Item { get; set; }
        public int Quantity { get; set; }
        public int EnhancementLevel { get; set; } = 0;
        // v7: 인스턴스별 affix. 이번 PR에서는 빈 리스트로 유지 — 후속 PR이 드랍 시점에 채움.
        public List<ItemAffix> Affixes { get; set; } = new();
        // v11+: 장착 상태 — 인벤 슬롯에 머물면서 IsEquipped=true로 표시. 장착 아이템도 인벤에 보임.
        public bool IsEquipped { get; set; } = false;
    }

    public class Inventory
    {
        public const int MaxSlots = 20;

        public List<InventorySlot> Slots { get; private set; } = new();

        // 장비 슬롯 (Equipment Slots)
        public ItemData EquippedWeapon { get; private set; }
        public ItemData EquippedArmor { get; private set; }
        public ItemData EquippedAccessory { get; private set; }
        // 신규 부위별 슬롯 (반지 2개)
        public ItemData EquippedHelmet { get; internal set; }
        public ItemData EquippedBoots { get; internal set; }
        public ItemData EquippedNecklace { get; internal set; }
        public ItemData EquippedRing1 { get; internal set; }
        public ItemData EquippedRing2 { get; internal set; }
        public ItemData EquippedBracelet { get; internal set; }
        // v11: 망토/벨트/장갑 슬롯
        public ItemData EquippedCloak { get; internal set; }
        public ItemData EquippedBelt { get; internal set; }
        public ItemData EquippedGloves { get; internal set; }

        // 장착 중인 장비의 강화 수치.
        // 무기 전용 정책 — Armor/Accessory 강화는 폐기됐지만 SaveData/세이브 호환을 위해 필드는 남기고
        // 항상 0으로만 사용한다. EnhanceUI/Inventory 어디서도 더 이상 변경하지 않음.
        public int EquippedWeaponEnhancement { get; private set; } = 0;
        public int EquippedArmorEnhancement { get; private set; } = 0;
        public int EquippedAccessoryEnhancement { get; private set; } = 0;

        // 장신구 슬롯의 인스턴스별 affix
        public List<ItemAffix> EquippedNecklaceAffixes { get; private set; } = new();
        public List<ItemAffix> EquippedRing1Affixes { get; private set; } = new();
        public List<ItemAffix> EquippedRing2Affixes { get; private set; } = new();
        public List<ItemAffix> EquippedBraceletAffixes { get; private set; } = new();
        public List<ItemAffix> EquippedCloakAffixes { get; private set; } = new();
        public List<ItemAffix> EquippedBeltAffixes { get; private set; } = new();
        public List<ItemAffix> EquippedGlovesAffixes { get; private set; } = new();

        // 무게 합산 — ItemData.EffectiveWeight()가 Type별 기본 처리 (.tres에 Weight 미설정 시 폴백)
        public float CurrentWeight
        {
            get
            {
                float total = 0f;
                foreach (var s in Slots) total += (s.Item?.EffectiveWeight() ?? 1f) * s.Quantity;
                return total;
            }
        }
        public static float GetMaxWeight() => 50f;

        // ─── 강화 헬퍼 ────────────────────────────────────────────
        // 무기 전용 정책. Armor/Accessory는 항상 0 — 호출처 변경 없이 자연스럽게 무효화된다.
        public static (int damage, int health, int defense) GetEnhancementBonuses(ItemData item, int level)
        {
            if (item == null || level <= 0 || item.Type != ItemType.Weapon) return (0, 0, 0);
            return (level * 2, 0, 0);
        }

        public static string GetEnhancedName(ItemData item, int enhancementLevel)
        {
            if (item == null) return "";
            return enhancementLevel > 0 ? $"{item.ItemName} +{enhancementLevel}" : item.ItemName;
        }

        public void NotifyChanged() => OnInventoryChanged?.Invoke();

        // 퀵슬롯 (Quick Slots - 6 slots)
        public ItemData[] QuickSlots { get; private set; } = new ItemData[6];

        // UI 갱신용 이벤트 (Events for UI updates)
        public event Action OnInventoryChanged;
        public event Action<ItemData> OnItemPickedUp;   // HUD 알림용
        public event Action OnEquipmentChanged;
        public event Action OnQuickSlotChanged;

        // --- 인벤토리 조작 ---

        /// <summary>
        /// 인벤토리에 아이템 추가.
        /// </summary>
        /// <param name="fireAcquired">
        /// true: 새 획득(필드 픽업/보상/구매)으로 간주해 OnItemPickedUp 발신.
        /// false: 복원/장착해제/롤백 등 내부 이동이라 픽업 토스트 미발신.
        /// </param>
        public bool AddItem(ItemData item, int amount = 1, int enhancementLevel = 0, bool fireAcquired = true, List<ItemAffix> affixes = null)
        {
            if (!CanAddItem(item, amount)) return false;

            // affix가 있는 인스턴스는 stack 불가 — 슬롯마다 고유 옵션 보존.
            bool hasAffixes = affixes != null && affixes.Count > 0;

            if (item.IsStackable && !hasAffixes)
            {
                int remaining = amount;
                int maxStack = Math.Max(1, item.MaxStack);

                foreach (var existing in Slots)
                {
                    if (!IsSameItem(existing.Item, item)) continue;
                    if (existing.IsEquipped) continue; // 장착 슬롯은 stack 금지
                    if (existing.Affixes != null && existing.Affixes.Count > 0) continue; // affix 있는 슬롯과 stack 금지

                    int space = maxStack - existing.Quantity;
                    if (space <= 0) continue;

                    int toAdd = Math.Min(remaining, space);
                    existing.Quantity += toAdd;
                    remaining -= toAdd;
                    if (remaining <= 0) break;
                }

                while (remaining > 0)
                {
                    int toAdd = Math.Min(remaining, maxStack);
                    Slots.Add(new InventorySlot
                    {
                        Item = item,
                        Quantity = toAdd,
                        EnhancementLevel = enhancementLevel
                    });
                    remaining -= toAdd;
                }
            }
            else
            {
                for (int i = 0; i < amount; i++)
                {
                    Slots.Add(new InventorySlot
                    {
                        Item = item,
                        Quantity = 1,
                        EnhancementLevel = enhancementLevel,
                        Affixes = hasAffixes ? new List<ItemAffix>(affixes) : new List<ItemAffix>()
                    });
                }
            }

            OnInventoryChanged?.Invoke();
            if (fireAcquired) OnItemPickedUp?.Invoke(item);

            // 신규 정책: 물약(소모품) 획득·구매 시 빈 퀵슬롯 첫 번째에 자동 등록.
            // fireAcquired=true(획득/구매)일 때만 — 세이브 복원에는 영향 X.
            if (fireAcquired && item.Type == ItemType.Consumable)
            {
                bool alreadyAssigned = false;
                for (int i = 0; i < QuickSlots.Length; i++)
                    if (QuickSlots[i] != null && IsSameItem(QuickSlots[i], item)) { alreadyAssigned = true; break; }
                if (!alreadyAssigned)
                {
                    for (int i = 0; i < QuickSlots.Length; i++)
                    {
                        if (QuickSlots[i] == null)
                        {
                            QuickSlots[i] = item;
                            OnQuickSlotChanged?.Invoke();
                            break;
                        }
                    }
                }
            }
            return true;
        }

        public bool CanAddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            if (!item.IsStackable)
                return Slots.Count + amount <= MaxSlots;

            int remaining = amount;
            int maxStack = Math.Max(1, item.MaxStack);

            foreach (var slot in Slots)
            {
                if (!IsSameItem(slot.Item, item)) continue;

                int space = maxStack - slot.Quantity;
                if (space <= 0) continue;

                remaining -= Math.Min(remaining, space);
                if (remaining <= 0) return true;
            }

            int emptySlots = MaxSlots - Slots.Count;
            int neededSlots = (remaining + maxStack - 1) / maxStack;
            return neededSlots <= emptySlots;
        }

        private static bool IsSameItem(ItemData a, ItemData b)
        {
            if (a == null || b == null) return false;
            if (!string.IsNullOrEmpty(a.ResourcePath) && !string.IsNullOrEmpty(b.ResourcePath))
                return a.ResourcePath == b.ResourcePath;
            return ReferenceEquals(a, b);
        }

        // 반환값 — 골드 지급 후 제거 실패로 인한 무한 골드 버그 차단용.
        // 판매/소비처럼 mutation 결과에 의존하는 호출자는 반드시 true 확인 후 다음 단계 진행.
        public bool RemoveItem(int slotIndex, int amount = 1)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return false;
            // 장착 슬롯 보호 — 외부 호출자가 실수로 장착 아이템을 제거하지 않게 차단.
            // 장비 해제는 UnequipXxx 경유 필수.
            if (Slots[slotIndex].IsEquipped)
            {
                GD.PrintErr("[Inventory] 장착 중인 아이템은 제거할 수 없습니다 — 먼저 해제하세요.");
                return false;
            }
            Slots[slotIndex].Quantity = Math.Max(0, Slots[slotIndex].Quantity - amount);
            if (Slots[slotIndex].Quantity <= 0)
                Slots.RemoveAt(slotIndex);
            OnInventoryChanged?.Invoke();
            return true;
        }

        // --- 아이템 사용 ---

        public void UseItem(int slotIndex, IEquipTarget target)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];

            if (slot.Item.Type == ItemType.Consumable)
            {
                switch (slot.Item.UseEffect)
                {
                    case ItemUseEffect.ReturnToTown:
                        // 귀환 주문서 트랜잭션 — 막아야 할 4가지.
                        // 1) RemoveItem이 OnInventoryChanged → RequestAutoSave를 트리거해 중간 상태가
                        //    디스크에 박히는 것 → suspendAutoSave (GameTransaction 기본 true)
                        // 2) RemoveItem이 만든 빈 슬롯을 TryClaim이 가로채는 것 + TryClaim의 직접
                        //    SaveGame이 suspend를 우회하는 것 → suspendPendingClaims (기본 true)
                        // 3) AddItem 복구 실패 시 스크롤 손실 → 반환값 체크 + PendingReward fallback
                        // 4) Teleport 성공 후 dispose에서 TryClaim이 실행되면 deferred 큐 상태에서
                        //    BuildSaveData가 old CurrentScene을 캡처해 디스크를 덮는 것 →
                        //    SetClaimAfterDispose(false)로 새 씬에 위임
                        if (!TownReturnHandler.CanUse()) return;
                        var consumed = slot.Item;
                        int consumedLevel = slot.EnhancementLevel;
                        using (var tx = GameTransaction.Begin())
                        {
                            RemoveItem(slotIndex, 1);
                            AudioManager.Instance?.PlaySFX("potion_use.wav");
                            bool teleportSucceeded = TownReturnHandler.Teleport();
                            if (teleportSucceeded)
                            {
                                tx.SetClaimAfterDispose(false); // 새 씬 _Ready → LoadFromSaveData → TryClaim 위임
                            }
                            else
                            {
                                bool restored = AddItem(consumed, 1, consumedLevel, fireAcquired: false);
                                if (!restored)
                                {
                                    GameManager.Instance?.AddPendingReward(consumed, 1, consumedLevel);
                                    GD.PrintErr("귀환 실패 + 인벤 복구 실패 — 주문서를 보류 보상에 보관");
                                }
                                else
                                {
                                    GD.Print("귀환 실패 — 주문서를 돌려받았습니다.");
                                }
                            }
                        }
                        return;

                    case ItemUseEffect.Heal:
                        target.Heal(slot.Item.HealAmount);
                        GD.Print($"{slot.Item.ItemName} 사용! HP +{slot.Item.HealAmount}");
                        AudioManager.Instance?.PlaySFX("potion_use.wav");
                        RemoveItem(slotIndex, 1);
                        return;

                    case ItemUseEffect.RestoreMana:
                        target.RestoreMp(slot.Item.HealAmount);
                        GD.Print($"{slot.Item.ItemName} 사용! MP +{slot.Item.HealAmount}");
                        AudioManager.Instance?.PlaySFX("potion_use.wav");
                        RemoveItem(slotIndex, 1);
                        return;

                    case ItemUseEffect.Buff:
                        target.ApplyBuffEx(
                            slot.Item.BuffMoveSpeed,
                            slot.Item.BuffAttackSpeed,
                            slot.Item.BuffBaseDamage,
                            slot.Item.BuffDefense,
                            slot.Item.BuffCritRate,
                            slot.Item.BuffDurationSec);
                        GD.Print($"{slot.Item.ItemName} 사용! {slot.Item.BuffDurationSec:0}초간 효과 적용");
                        AudioManager.Instance?.PlaySFX("potion_use.wav");
                        RemoveItem(slotIndex, 1);
                        return;

                    case ItemUseEffect.Teleport:
                    {
                        var item = slot.Item;
                        if (string.IsNullOrEmpty(item.TeleportTargetScene)) return;
                        // 방문 이력 확인 — 방문한 씬만 허용
                        var gm = GameManager.Instance;
                        if (gm != null)
                        {
                            bool visited = false;
                            foreach (var s in gm.VisitedScenes)
                                if (s == item.TeleportTargetScene) { visited = true; break; }
                            if (!visited)
                            {
                                GD.Print($"[텔포] {item.TeleportTargetScene}은 아직 방문하지 않은 지역입니다.");
                                return;
                            }
                        }
                        var consumed2 = item;
                        int consumedLevel2 = slot.EnhancementLevel;
                        using (var tx2 = GameTransaction.Begin())
                        {
                            RemoveItem(slotIndex, 1);
                            AudioManager.Instance?.PlaySFX("potion_use.wav");
                            var spawnPos = item.TeleportTargetPos != Godot.Vector2.Zero
                                ? item.TeleportTargetPos : new Godot.Vector2(320, 180);
                            bool ok = Core.SceneManager.Instance?.ChangeScene(item.TeleportTargetScene, spawnPos) == true;
                            if (ok)
                            {
                                tx2.SetClaimAfterDispose(false);
                            }
                            else
                            {
                                bool restored2 = AddItem(consumed2, 1, consumedLevel2, fireAcquired: false);
                                if (!restored2) GameManager.Instance?.AddPendingReward(consumed2, 1, consumedLevel2);
                            }
                        }
                        return;
                    }

                    case ItemUseEffect.CureStatus:
                        target.CureStatuses(slot.Item.HealAmount);
                        GD.Print($"{slot.Item.ItemName} 사용! 상태이상 해제");
                        AudioManager.Instance?.PlaySFX("potion_use.wav");
                        RemoveItem(slotIndex, 1);
                        return;

                    case ItemUseEffect.ReviveOnDeath:
                        // 사용 시 아무 효과 없음 — 사망 시 자동 소비
                        GD.Print($"{slot.Item.ItemName}: 사망 시 자동 발동 아이템입니다.");
                        return;

                    case ItemUseEffect.None:
                    default:
                        return;
                }
            }
            else if (slot.Item.Type.IsEquipment() && slot.Item.Type != ItemType.SkillBook)
            {
                EquipItem(slotIndex, target);
            }
            else if (slot.Item.Type == ItemType.SkillBook)
            {
                if (slot.Item.LearnedSkill == null) return;
                bool learned = target.LearnSkill(slot.Item.LearnedSkill);
                if (learned)
                {
                    AudioManager.Instance?.PlaySFX("skill_learn.wav");
                    RemoveItem(slotIndex, 1);
                }
            }
        }

        // --- 장비 장착/해제 ---

        public void EquipItem(int slotIndex, IEquipTarget target)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];
            var item = slot.Item;
            int enhLevel = slot.EnhancementLevel;

            // 장비 클래스 제한 — 무기·갑옷·모자·신발·장신구 모두 적용.
            if (item.Type.IsEquipment() && !item.AvailableToAllClasses
                && target is PlayerStats ps && item.RequiredClass != ps.PlayerClass)
            {
                GD.Print($"[장착 불가] {item.ItemName}은 {PlayerClassUtil.DisplayName(item.RequiredClass)} 전용입니다 (현재: {PlayerClassUtil.DisplayName(ps.PlayerClass)})");
                return;
            }

            // 이미 장착된 슬롯이면 무시 — 다른 슬롯의 같은 타입을 클릭한 경우는 교체 의도이므로 진행.
            if (slot.IsEquipped) return;

            // 신규 정책: 인벤 슬롯 유지 + IsEquipped 토글.
            // 1) 기존 장착 슬롯 해제 (스탯 차감 + IsEquipped=false)
            // 2) 새 슬롯 IsEquipped=true (인벤에 그대로 남음)
            // MaxSlots 검사 불필요 — 슬롯 카운트 변화 없음.
            if (item.Type == ItemType.Weapon)
            {
                ClearEquippedWeapon(target);
                EquippedWeapon = item;
                EquippedWeaponEnhancement = enhLevel;
                slot.IsEquipped = true;
                var (dmg, _, _) = GetEnhancementBonuses(item, enhLevel);
                target.ModifyBaseDamage(item.BonusDamage + dmg);
            }
            else if (item.Type == ItemType.Armor)
            {
                ClearEquippedArmor(target);
                EquippedArmor = item;
                slot.IsEquipped = true;
                target.ModifyMaxHealth(item.BonusMaxHealth);
                target.ModifyDefense(item.BonusDefense);
            }
            else if (item.Type == ItemType.Accessory)
            {
                ClearEquippedAccessory(target);
                EquippedAccessory = item;
                slot.IsEquipped = true;
                target.ModifyDefense(item.BonusDefense);
                if (item.BonusDamage > 0) target.ModifyBaseDamage(item.BonusDamage);
                if (item.BonusMaxHealth > 0) target.ModifyMaxHealth(item.BonusMaxHealth);
            }
            else if (IsExtraEquipType(item.Type))
            {
                EquipExtraInternal(slotIndex, item, ResolveExtraSlot(item.Type), target);
                OnInventoryChanged?.Invoke();
                OnEquipmentChanged?.Invoke();
                AudioManager.Instance?.PlaySFX("equip.wav");
                GD.Print($"{GetEnhancedName(item, enhLevel)} 장착!");
                return;
            }

            OnInventoryChanged?.Invoke();
            OnEquipmentChanged?.Invoke();
            AudioManager.Instance?.PlaySFX("equip.wav");
            GD.Print($"{GetEnhancedName(item, enhLevel)} 장착!");
        }

        /// <summary>기존 장착 무기 스탯 차감 + 해당 인벤 슬롯의 IsEquipped=false.</summary>
        private void ClearEquippedWeapon(IEquipTarget target)
        {
            if (EquippedWeapon == null) return;
            var (pdmg, _, _) = GetEnhancementBonuses(EquippedWeapon, EquippedWeaponEnhancement);
            target.ModifyBaseDamage(-(EquippedWeapon.BonusDamage + pdmg));
            int idx = FindEquippedSlotIndex(EquippedWeapon);
            if (idx >= 0) Slots[idx].IsEquipped = false;
            EquippedWeapon = null;
            EquippedWeaponEnhancement = 0;
        }

        private void ClearEquippedArmor(IEquipTarget target)
        {
            if (EquippedArmor == null) return;
            target.ModifyMaxHealth(-EquippedArmor.BonusMaxHealth);
            target.ModifyDefense(-EquippedArmor.BonusDefense);
            int idx = FindEquippedSlotIndex(EquippedArmor);
            if (idx >= 0) Slots[idx].IsEquipped = false;
            EquippedArmor = null;
        }

        private void ClearEquippedAccessory(IEquipTarget target)
        {
            if (EquippedAccessory == null) return;
            target.ModifyDefense(-EquippedAccessory.BonusDefense);
            if (EquippedAccessory.BonusDamage > 0) target.ModifyBaseDamage(-EquippedAccessory.BonusDamage);
            if (EquippedAccessory.BonusMaxHealth > 0) target.ModifyMaxHealth(-EquippedAccessory.BonusMaxHealth);
            int idx = FindEquippedSlotIndex(EquippedAccessory);
            if (idx >= 0) Slots[idx].IsEquipped = false;
            EquippedAccessory = null;
        }

        /// <summary>장착된 아이템 데이터에 해당하는 IsEquipped 슬롯의 인덱스. 없으면 -1.</summary>
        private int FindEquippedSlotIndex(ItemData item)
        {
            if (item == null) return -1;
            for (int i = 0; i < Slots.Count; i++)
                if (Slots[i].IsEquipped && IsSameItem(Slots[i].Item, item))
                    return i;
            return -1;
        }

        /// <summary>반지처럼 같은 ItemType이 두 슬롯(Ring1/Ring2)을 가질 때 사용자가
        /// 어느 슬롯에 장착/교체할지 직접 지정. ResolveExtraSlot이 자동으로 빈 칸을 고르는
        /// 기본 EquipItem과 달리, 둘 다 차 있어도 지정한 슬롯으로 교체 가능.</summary>
        public void EquipItemToSlot(int slotIndex, ExtraSlot targetSlot, IEquipTarget target)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];
            var item = slot.Item;
            if (!IsExtraEquipType(item.Type)) return;
            // 슬롯 유형이 아이템과 맞는지 검증 (Ring → Ring1/Ring2만 허용 등)
            if (!IsSlotCompatible(item.Type, targetSlot)) return;
            // 클래스 제한 — EquipItem과 동일 가드.
            if (!item.AvailableToAllClasses && target is PlayerStats psExtra && item.RequiredClass != psExtra.PlayerClass)
            {
                GD.Print($"[장착 불가] {item.ItemName}은 {PlayerClassUtil.DisplayName(item.RequiredClass)} 전용입니다 (현재: {PlayerClassUtil.DisplayName(psExtra.PlayerClass)})");
                return;
            }
            EquipExtraInternal(slotIndex, item, targetSlot, target);
            OnInventoryChanged?.Invoke();
            OnEquipmentChanged?.Invoke();
            AudioManager.Instance?.PlaySFX("equip.wav");
            GD.Print($"{item.ItemName} 장착!");
        }

        private static bool IsSlotCompatible(ItemType t, ExtraSlot s) => (t, s) switch
        {
            (ItemType.Helmet,   ExtraSlot.Helmet)   => true,
            (ItemType.Boots,    ExtraSlot.Boots)     => true,
            (ItemType.Necklace, ExtraSlot.Necklace)  => true,
            (ItemType.Bracelet, ExtraSlot.Bracelet)  => true,
            (ItemType.Ring,     ExtraSlot.Ring1)     => true,
            (ItemType.Ring,     ExtraSlot.Ring2)     => true,
            (ItemType.Cloak,    ExtraSlot.Cloak)     => true,
            (ItemType.Belt,     ExtraSlot.Belt)      => true,
            (ItemType.Gloves,   ExtraSlot.Gloves)    => true,
            _ => false
        };

        private void EquipExtraInternal(int slotIndex, ItemData item, ExtraSlot slotKey, IEquipTarget target)
        {
            var slot = Slots[slotIndex];
            if (slot.IsEquipped) return;

            // 1) 기존 장착 슬롯 해제 (스탯 차감 + IsEquipped=false). 인벤은 유지.
            var prev = GetExtraSlot(slotKey);
            var prevAffixes = GetExtraAffixes(slotKey);
            if (prev != null)
            {
                ApplyItemBonuses(prev, target, -1);
                ApplyAffixBonuses(prevAffixes, target, -1);
                int prevIdx = FindEquippedSlotIndex(prev);
                if (prevIdx >= 0) Slots[prevIdx].IsEquipped = false;
            }

            // 2) 새 슬롯 IsEquipped=true. 인벤 슬롯 유지.
            slot.IsEquipped = true;
            var newAffixes = slot.Affixes != null ? new List<ItemAffix>(slot.Affixes) : new List<ItemAffix>();
            SetExtraSlot(slotKey, item);
            SetExtraAffixes(slotKey, newAffixes);
            ApplyItemBonuses(item, target, +1);
            ApplyAffixBonuses(GetExtraAffixes(slotKey), target, +1);
        }

        public bool UnequipWeapon(IEquipTarget target)
        {
            if (EquippedWeapon == null) return false;
            // 신규 정책: 인벤 슬롯 유지 — IsEquipped=false만. 가방 만석 검사 불필요.
            using (GameTransaction.Begin())
            {
                ClearEquippedWeapon(target);
                OnEquipmentChanged?.Invoke();
                OnInventoryChanged?.Invoke();
            }
            SaveManager.RequestAutoSave();
            return true;
        }

        public bool UnequipArmor(IEquipTarget target)
        {
            if (EquippedArmor == null) return false;
            using (GameTransaction.Begin())
            {
                ClearEquippedArmor(target);
                OnEquipmentChanged?.Invoke();
                OnInventoryChanged?.Invoke();
            }
            SaveManager.RequestAutoSave();
            return true;
        }

        public bool UnequipAccessory(IEquipTarget target)
        {
            if (EquippedAccessory == null) return false;
            using (GameTransaction.Begin())
            {
                ClearEquippedAccessory(target);
                OnEquipmentChanged?.Invoke();
                OnInventoryChanged?.Invoke();
            }
            SaveManager.RequestAutoSave();
            return true;
        }

        // --- 신규 부위별 장비 슬롯 헬퍼 ---

        /// <summary>모자/신발/목걸이/반지/팔찌/망토/벨트/장갑처럼 강화 미지원 신규 부위인지.</summary>
        public static bool IsExtraEquipType(ItemType t) =>
            t == ItemType.Helmet || t == ItemType.Boots || t == ItemType.Necklace ||
            t == ItemType.Ring || t == ItemType.Bracelet ||
            t == ItemType.Cloak || t == ItemType.Belt || t == ItemType.Gloves;

        public enum ExtraSlot { Helmet, Boots, Necklace, Ring1, Ring2, Bracelet, Cloak, Belt, Gloves }

        /// <summary>아이템 타입을 어떤 부위 슬롯에 둘지 결정. 반지는 빈 슬롯 우선.</summary>
        private ExtraSlot ResolveExtraSlot(ItemType t) => t switch
        {
            ItemType.Helmet => ExtraSlot.Helmet,
            ItemType.Boots => ExtraSlot.Boots,
            ItemType.Necklace => ExtraSlot.Necklace,
            ItemType.Bracelet => ExtraSlot.Bracelet,
            ItemType.Ring => EquippedRing1 == null ? ExtraSlot.Ring1 : ExtraSlot.Ring2,
            ItemType.Cloak => ExtraSlot.Cloak,
            ItemType.Belt => ExtraSlot.Belt,
            ItemType.Gloves => ExtraSlot.Gloves,
            _ => ExtraSlot.Helmet
        };

        public ItemData GetExtraSlot(ExtraSlot s) => s switch
        {
            ExtraSlot.Helmet => EquippedHelmet,
            ExtraSlot.Boots => EquippedBoots,
            ExtraSlot.Necklace => EquippedNecklace,
            ExtraSlot.Ring1 => EquippedRing1,
            ExtraSlot.Ring2 => EquippedRing2,
            ExtraSlot.Bracelet => EquippedBracelet,
            ExtraSlot.Cloak => EquippedCloak,
            ExtraSlot.Belt => EquippedBelt,
            ExtraSlot.Gloves => EquippedGloves,
            _ => null
        };

        private void SetExtraSlot(ExtraSlot s, ItemData item)
        {
            switch (s)
            {
                case ExtraSlot.Helmet:   EquippedHelmet  = item; break;
                case ExtraSlot.Boots:    EquippedBoots   = item; break;
                case ExtraSlot.Necklace: EquippedNecklace = item; break;
                case ExtraSlot.Ring1:    EquippedRing1   = item; break;
                case ExtraSlot.Ring2:    EquippedRing2   = item; break;
                case ExtraSlot.Bracelet: EquippedBracelet = item; break;
                case ExtraSlot.Cloak:    EquippedCloak   = item; break;
                case ExtraSlot.Belt:     EquippedBelt    = item; break;
                case ExtraSlot.Gloves:   EquippedGloves  = item; break;
            }
        }

        /// <summary>장신구/신규 슬롯의 affix 페어. Helmet/Boots는 null 반환(미대상).</summary>
        public List<ItemAffix> GetExtraAffixes(ExtraSlot s) => s switch
        {
            ExtraSlot.Necklace => EquippedNecklaceAffixes,
            ExtraSlot.Ring1    => EquippedRing1Affixes,
            ExtraSlot.Ring2    => EquippedRing2Affixes,
            ExtraSlot.Bracelet => EquippedBraceletAffixes,
            ExtraSlot.Cloak    => EquippedCloakAffixes,
            ExtraSlot.Belt     => EquippedBeltAffixes,
            ExtraSlot.Gloves   => EquippedGlovesAffixes,
            _ => null
        };

        private void SetExtraAffixes(ExtraSlot s, List<ItemAffix> affixes)
        {
            var copy = affixes != null ? new List<ItemAffix>(affixes) : new List<ItemAffix>();
            switch (s)
            {
                case ExtraSlot.Necklace: EquippedNecklaceAffixes = copy; break;
                case ExtraSlot.Ring1:    EquippedRing1Affixes    = copy; break;
                case ExtraSlot.Ring2:    EquippedRing2Affixes    = copy; break;
                case ExtraSlot.Bracelet: EquippedBraceletAffixes = copy; break;
                case ExtraSlot.Cloak:    EquippedCloakAffixes    = copy; break;
                case ExtraSlot.Belt:     EquippedBeltAffixes     = copy; break;
                case ExtraSlot.Gloves:   EquippedGlovesAffixes   = copy; break;
                // Helmet/Boots는 affix 미대상 — 무시.
            }
        }

        /// <summary>장비 보너스 일괄 적용 (sign=+1 장착, -1 해제).</summary>
        private static void ApplyItemBonuses(ItemData item, IEquipTarget target, int sign)
        {
            if (item.BonusDamage != 0) target.ModifyBaseDamage(sign * item.BonusDamage);
            if (item.BonusMaxHealth != 0) target.ModifyMaxHealth(sign * item.BonusMaxHealth);
            if (item.BonusDefense != 0) target.ModifyDefense(sign * item.BonusDefense);
            if (item.BonusMaxMp != 0) target.ModifyMaxMp(sign * item.BonusMaxMp);
            if (item.BonusCritRate != 0f) target.ModifyCritRate(sign * item.BonusCritRate);
            if (item.BonusMoveSpeed != 0f) target.ModifyMoveSpeed(sign * item.BonusMoveSpeed);
            if (item.BonusAttackSpeed != 0f) target.ModifyAttackSpeed(sign * item.BonusAttackSpeed);
            if (item.BonusLifesteal != 0f) target.ModifyLifesteal(sign * item.BonusLifesteal);
        }

        /// <summary>affix 합산 적용 (sign=+1 장착, -1 해제). 같은 type이 여러 개면 모두 누적.</summary>
        private static void ApplyAffixBonuses(List<ItemAffix> affixes, IEquipTarget target, int sign)
        {
            if (affixes == null || affixes.Count == 0) return;
            foreach (var a in affixes)
            {
                switch (a.Type)
                {
                    case ItemAffixType.BonusDamage:    target.ModifyBaseDamage(sign * (int)a.Value); break;
                    case ItemAffixType.BonusDefense:   target.ModifyDefense(sign * (int)a.Value); break;
                    case ItemAffixType.BonusMaxHealth: target.ModifyMaxHealth(sign * (int)a.Value); break;
                    case ItemAffixType.BonusMaxMp:     target.ModifyMaxMp(sign * (int)a.Value); break;
                    case ItemAffixType.BonusCritRate:  target.ModifyCritRate(sign * a.Value); break;
                    case ItemAffixType.BonusMoveSpeed: target.ModifyMoveSpeed(sign * a.Value); break;
                    case ItemAffixType.BonusAttackSpeed: target.ModifyAttackSpeed(sign * a.Value); break;
                    case ItemAffixType.BonusLifesteal: target.ModifyLifesteal(sign * a.Value); break;
                }
            }
        }

        /// <summary>세이브에서 신규 부위별 슬롯 복원. 빈 path는 무시.
        /// affixes는 Necklace/Ring/Bracelet 슬롯에만 의미 있음. Helmet/Boots는 null 전달.</summary>
        public void RestoreExtraSlot(ExtraSlot slot, string itemPath, List<ItemAffix> affixes, IEquipTarget target)
        {
            if (string.IsNullOrEmpty(itemPath)) return;
            var item = GD.Load<ItemData>(itemPath);
            if (item == null) return;

            SetExtraSlot(slot, item);
            SetExtraAffixes(slot, affixes);
            ApplyItemBonuses(item, target, +1);
            ApplyAffixBonuses(GetExtraAffixes(slot), target, +1);
            EnsureEquippedSlotPresent(item, 0, affixes);
            OnEquipmentChanged?.Invoke();
        }

        /// <summary>장착된 아이템이 인벤에 IsEquipped 슬롯으로 존재하는지 보장.
        /// 매칭되는 일반 슬롯이 있으면 그 슬롯의 IsEquipped=true, 없으면 신규 추가.
        /// v10 이하 세이브에서 인벤과 분리 보관된 장비를 신규 정책으로 마이그.</summary>
        private void EnsureEquippedSlotPresent(ItemData item, int enhancementLevel, List<ItemAffix> affixes = null)
        {
            if (item == null) return;
            // 이미 IsEquipped 슬롯이 있으면 끝
            int existing = FindEquippedSlotIndex(item);
            if (existing >= 0) return;
            // 없으면 일치하는 미장착 슬롯 찾아 IsEquipped 마킹 (없으면 신규 추가)
            for (int i = 0; i < Slots.Count; i++)
            {
                if (!Slots[i].IsEquipped && IsSameItem(Slots[i].Item, item))
                {
                    Slots[i].IsEquipped = true;
                    Slots[i].EnhancementLevel = enhancementLevel;
                    if (affixes != null && affixes.Count > 0)
                        Slots[i].Affixes = new List<ItemAffix>(affixes);
                    return;
                }
            }
            // 신규 슬롯 추가 (MaxSlots 초과해도 강제 — 세이브 복원은 안전 최우선)
            Slots.Add(new InventorySlot
            {
                Item = item,
                Quantity = 1,
                EnhancementLevel = enhancementLevel,
                Affixes = affixes != null ? new List<ItemAffix>(affixes) : new List<ItemAffix>(),
                IsEquipped = true
            });
        }

        public bool UnequipExtra(ExtraSlot slot, IEquipTarget target)
        {
            var item = GetExtraSlot(slot);
            if (item == null) return false;
            // 신규 정책: 인벤 슬롯 유지 — IsEquipped=false. 가방 만석 검사 불필요.
            using (GameTransaction.Begin())
            {
                var affixes = GetExtraAffixes(slot);
                ApplyItemBonuses(item, target, -1);
                ApplyAffixBonuses(affixes, target, -1);

                int idx = FindEquippedSlotIndex(item);
                if (idx >= 0) Slots[idx].IsEquipped = false;

                SetExtraSlot(slot, null);
                SetExtraAffixes(slot, new List<ItemAffix>());
                OnEquipmentChanged?.Invoke();
                OnInventoryChanged?.Invoke();
            }
            SaveManager.RequestAutoSave();
            return true;
        }

        // --- 퀵슬롯 ---

        public void AssignQuickSlot(int quickSlotIndex, ItemData item)
        {
            if (quickSlotIndex < 0 || quickSlotIndex >= QuickSlots.Length) return;
            QuickSlots[quickSlotIndex] = item;
            OnQuickSlotChanged?.Invoke();
            GD.Print($"퀵슬롯 {quickSlotIndex + 1}번에 {item.ItemName} 등록됨.");
        }

        public void UseQuickSlot(int quickSlotIndex, IEquipTarget target)
        {
            if (quickSlotIndex < 0 || quickSlotIndex >= QuickSlots.Length) return;
            var item = QuickSlots[quickSlotIndex];
            if (item == null) return;

            // ResourcePath 비교 — Resource 캐시가 다른 인스턴스를 반환하더라도 동일 아이템으로 매칭.
            int slotIndex = Slots.FindIndex(s => IsSameItem(s.Item, item));
            if (slotIndex != -1)
            {
                UseItem(slotIndex, target);
            }
            else
            {
                GD.Print($"{item.ItemName}이(가) 인벤토리에 없습니다!");
            }
        }

        /// <summary>세이브 복원용 일괄 퀵슬롯 적용. OnQuickSlotChanged 한 번만 발신.</summary>
        public void RestoreQuickSlots(ItemData[] items)
        {
            if (items == null) return;
            int count = Math.Min(items.Length, QuickSlots.Length);
            for (int i = 0; i < count; i++)
                QuickSlots[i] = items[i];
            OnQuickSlotChanged?.Invoke();
        }

        // --- 크래프팅 지원 ---

        public int CountItem(ItemData item)
        {
            int count = 0;
            foreach (var slot in Slots)
            {
                if (slot.Item.ResourcePath == item.ResourcePath)
                    count += slot.Quantity;
            }
            return count;
        }

        public bool HasItems(ItemData item, int amount)
        {
            return CountItem(item) >= amount;
        }

        public bool ConsumeItems(ItemData item, int amount)
        {
            if (!HasItems(item, amount)) return false;

            int remaining = amount;
            for (int i = Slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (Slots[i].Item.ResourcePath != item.ResourcePath) continue;

                if (Slots[i].Quantity <= remaining)
                {
                    remaining -= Slots[i].Quantity;
                    Slots.RemoveAt(i);
                }
                else
                {
                    Slots[i].Quantity -= remaining;
                    remaining = 0;
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        // --- 강화 지원 (EnhanceUI용) ---

        /// <summary>장착 중인 무기의 강화 수치 변경 (보너스 재계산 포함). 무기 외 ItemType은 무시.</summary>
        public void SetEquippedEnhancement(ItemType type, int newLevel, IEquipTarget target)
        {
            if (type != ItemType.Weapon || EquippedWeapon == null) return;
            var (oldWDmg, _, _) = GetEnhancementBonuses(EquippedWeapon, EquippedWeaponEnhancement);
            target.ModifyBaseDamage(-oldWDmg);
            EquippedWeaponEnhancement = newLevel;
            var (newWDmg, _, _) = GetEnhancementBonuses(EquippedWeapon, newLevel);
            target.ModifyBaseDamage(newWDmg);
            OnEquipmentChanged?.Invoke();
        }

        /// <summary>장착 중인 무기 파괴 (강화 실패 시). 보너스 제거 후 null로. 무기 외 ItemType은 무시.</summary>
        public void DestroyEquippedItem(ItemType type, IEquipTarget target)
        {
            if (type != ItemType.Weapon || EquippedWeapon == null) return;
            var (wDmg, _, _) = GetEnhancementBonuses(EquippedWeapon, EquippedWeaponEnhancement);
            target.ModifyBaseDamage(-(EquippedWeapon.BonusDamage + wDmg));
            EquippedWeapon = null;
            EquippedWeaponEnhancement = 0;
            OnEquipmentChanged?.Invoke();
        }

        /// <summary>
        /// 세이브 데이터의 모든 장비 슬롯을 일괄 복원하는 단일 진입점.
        /// Weapon/Armor/Accessory + Helmet/Boots/Necklace/Ring1/Ring2/Bracelet 9개 슬롯과
        /// v3→v4 Accessory 재분류 마이그까지 한 번에 처리한다.
        /// </summary>
        /// <returns>마이그 결과 (호출자가 사용자 알림에 활용 가능)</returns>
        public EquipmentRestoreReport RestoreFromSaveData(SaveData data, IEquipTarget target)
        {
            var report = new EquipmentRestoreReport();

            var loadedWeapon    = LoadItemPath(data.EquippedWeaponPath);
            var loadedArmor     = LoadItemPath(data.EquippedArmorPath);
            var loadedAccessory = LoadItemPath(data.EquippedAccessoryPath);

            string migrateNecklacePath = data.EquippedNecklacePath;
            string migrateRing1Path = data.EquippedRing1Path;

            // v3→v4: 구 Accessory 슬롯 아이템이 Necklace/Ring으로 재분류된 경우.
            // 신규 슬롯은 강화 미지원 — 강화 +N은 인벤토리로 반환해 보존.
            if (loadedAccessory != null &&
                (loadedAccessory.Type == ItemType.Necklace || loadedAccessory.Type == ItemType.Ring))
            {
                if (data.EquippedAccessoryEnhancement > 0)
                {
                    report.MigratedItem = loadedAccessory;
                    report.MigratedEnhancement = data.EquippedAccessoryEnhancement;
                    if (AddItem(loadedAccessory, 1, data.EquippedAccessoryEnhancement, fireAcquired: false))
                    {
                        report.MigratedToInventory = true;
                        loadedAccessory = null;
                    }
                    else
                    {
                        report.MigratedToInventory = false; // 인벤 가득 — 강화 손실 강제 장착
                    }
                }

                if (loadedAccessory != null)
                {
                    if (loadedAccessory.Type == ItemType.Necklace && string.IsNullOrEmpty(migrateNecklacePath))
                    {
                        migrateNecklacePath = data.EquippedAccessoryPath;
                        loadedAccessory = null;
                    }
                    else if (loadedAccessory.Type == ItemType.Ring && string.IsNullOrEmpty(migrateRing1Path))
                    {
                        migrateRing1Path = data.EquippedAccessoryPath;
                        loadedAccessory = null;
                    }
                }
            }

            // 무기 전용 정책: data.EquippedArmorEnhancement / EquippedAccessoryEnhancement는
            // 의도적으로 무시한다. v6 이하 세이브에 비정상적으로 강화 +N이 박혀 있어도 0으로 클램프.
            RestoreEquipment(loadedWeapon, loadedArmor, target, loadedAccessory,
                data.EquippedWeaponEnhancement);
            // 신규 정책: 장착된 아이템과 매칭되는 인벤 슬롯의 IsEquipped=true 처리.
            // 매칭되는 슬롯이 없으면 인벤에 별도 추가 (v10 이하 세이브 호환).
            EnsureEquippedSlotPresent(loadedWeapon, data.EquippedWeaponEnhancement);
            EnsureEquippedSlotPresent(loadedArmor, 0);
            EnsureEquippedSlotPresent(loadedAccessory, 0);

            RestoreExtraSlot(ExtraSlot.Helmet,   data.EquippedHelmetPath,   null, target);
            RestoreExtraSlot(ExtraSlot.Boots,    data.EquippedBootsPath,    null, target);
            RestoreExtraSlot(ExtraSlot.Necklace, migrateNecklacePath,       data.EquippedNecklaceAffixes,  target);
            RestoreExtraSlot(ExtraSlot.Ring1,    migrateRing1Path,          data.EquippedRing1Affixes,     target);
            RestoreExtraSlot(ExtraSlot.Ring2,    data.EquippedRing2Path,    data.EquippedRing2Affixes,     target);
            RestoreExtraSlot(ExtraSlot.Bracelet, data.EquippedBraceletPath, data.EquippedBraceletAffixes,  target);
            // v11: 망토/벨트/장갑 — 누락 시 빈 문자열이므로 RestoreExtraSlot이 안전하게 무시.
            RestoreExtraSlot(ExtraSlot.Cloak,    data.EquippedCloakPath,    data.EquippedCloakAffixes,     target);
            RestoreExtraSlot(ExtraSlot.Belt,     data.EquippedBeltPath,     data.EquippedBeltAffixes,      target);
            RestoreExtraSlot(ExtraSlot.Gloves,   data.EquippedGlovesPath,   data.EquippedGlovesAffixes,    target);

            return report;
        }

        private static ItemData LoadItemPath(string path)
            => string.IsNullOrEmpty(path) ? null : GD.Load<ItemData>(path);

        public class EquipmentRestoreReport
        {
            /// <summary>v3→v4 마이그로 처리된 강화 Accessory. null이면 마이그 발생 안 함.</summary>
            public ItemData MigratedItem;
            public int MigratedEnhancement;
            /// <summary>true=인벤토리로 안전 반환, false=인벤 가득해 강화 손실하며 신규 슬롯에 강제 장착.</summary>
            public bool MigratedToInventory;
        }

        // 세이브/로드 시 장비 복원 (RestoreFromSaveData 내부에서 호출).
        // 강화는 무기 전용 — Armor/Accessory에는 강화 수치 적용 안 함.
        public void RestoreEquipment(ItemData weapon, ItemData armor, IEquipTarget target,
            ItemData accessory = null,
            int weaponEnhance = 0)
        {
            if (weapon != null)
            {
                EquippedWeapon = weapon;
                EquippedWeaponEnhancement = weaponEnhance;
                var (dmg, _, _) = GetEnhancementBonuses(weapon, weaponEnhance);
                target.ModifyBaseDamage(weapon.BonusDamage + dmg);
            }
            if (armor != null)
            {
                EquippedArmor = armor;
                target.ModifyMaxHealth(armor.BonusMaxHealth);
                target.ModifyDefense(armor.BonusDefense);
            }
            if (accessory != null)
            {
                EquippedAccessory = accessory;
                target.ModifyDefense(accessory.BonusDefense);
                if (accessory.BonusDamage > 0) target.ModifyBaseDamage(accessory.BonusDamage);
                if (accessory.BonusMaxHealth > 0) target.ModifyMaxHealth(accessory.BonusMaxHealth);
            }
            OnEquipmentChanged?.Invoke();
        }
    }
}
