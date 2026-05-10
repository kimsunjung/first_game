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

        // 장착된 장비의 강화 수치
        public int EquippedWeaponEnhancement { get; private set; } = 0;
        public int EquippedArmorEnhancement { get; private set; } = 0;
        public int EquippedAccessoryEnhancement { get; private set; } = 0;
        // 신규 슬롯은 강화 미지원 — 항상 0 유지 (이후 단계에서 확장)

        // ─── 강화 헬퍼 ────────────────────────────────────────────
        public static (int damage, int health, int defense) GetEnhancementBonuses(ItemData item, int level)
        {
            if (item == null || level <= 0) return (0, 0, 0);
            return item.Type switch
            {
                ItemType.Weapon => (level * 2, 0, 0),
                ItemType.Armor => (0, 0, level * 1),
                ItemType.Accessory => item.BonusDamage > 0
                    ? (level * 1, 0, 0)
                    : (0, 0, level * 1),
                _ => (0, 0, 0)
            };
        }

        public static string GetEnhancedName(ItemData item, int enhancementLevel)
        {
            if (item == null) return "";
            return enhancementLevel > 0 ? $"{item.ItemName} +{enhancementLevel}" : item.ItemName;
        }

        public void NotifyChanged() => OnInventoryChanged?.Invoke();

        // 퀵슬롯 (Quick Slots - 4 slots)
        public ItemData[] QuickSlots { get; private set; } = new ItemData[4];

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
        public bool AddItem(ItemData item, int amount = 1, int enhancementLevel = 0, bool fireAcquired = true)
        {
            if (!CanAddItem(item, amount)) return false;

            if (item.IsStackable)
            {
                int remaining = amount;
                int maxStack = Math.Max(1, item.MaxStack);

                foreach (var existing in Slots)
                {
                    if (!IsSameItem(existing.Item, item)) continue;

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
                        EnhancementLevel = enhancementLevel
                    });
                }
            }

            OnInventoryChanged?.Invoke();
            if (fireAcquired) OnItemPickedUp?.Invoke(item);
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

        public void RemoveItem(int slotIndex, int amount = 1)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            Slots[slotIndex].Quantity = Math.Max(0, Slots[slotIndex].Quantity - amount);
            if (Slots[slotIndex].Quantity <= 0)
                Slots.RemoveAt(slotIndex);
            OnInventoryChanged?.Invoke();
        }

        // --- 아이템 사용 ---

        public void UseItem(int slotIndex, IEquipTarget target)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];

            if (slot.Item.Type == ItemType.Consumable)
            {
                target.Heal(slot.Item.HealAmount);
                GD.Print($"{slot.Item.ItemName} 사용! HP +{slot.Item.HealAmount}");
                AudioManager.Instance?.PlaySFX("potion_use.wav");
                RemoveItem(slotIndex, 1);
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

            // 기존 장비를 해당 슬롯 자리에 in-place로 되돌려놓음으로써 가방이 꽉 차도
            // 교체 가능하게 한다. UnequipXxx → AddItem 경로는 빈 슬롯을 요구해서
            // MaxSlots 도달 시 거부됐음.
            if (item.Type == ItemType.Weapon)
            {
                var prevItem = EquippedWeapon;
                int prevEnh = EquippedWeaponEnhancement;
                if (prevItem != null)
                {
                    var (pdmg, _, _) = GetEnhancementBonuses(prevItem, prevEnh);
                    target.ModifyBaseDamage(-(prevItem.BonusDamage + pdmg));
                    slot.Item = prevItem;
                    slot.Quantity = 1;
                    slot.EnhancementLevel = prevEnh;
                }
                else
                {
                    Slots.RemoveAt(slotIndex);
                }
                EquippedWeapon = item;
                EquippedWeaponEnhancement = enhLevel;
                var (dmg, _, _) = GetEnhancementBonuses(item, enhLevel);
                target.ModifyBaseDamage(item.BonusDamage + dmg);
            }
            else if (item.Type == ItemType.Armor)
            {
                var prevItem = EquippedArmor;
                int prevEnh = EquippedArmorEnhancement;
                if (prevItem != null)
                {
                    var (_, _, pdef) = GetEnhancementBonuses(prevItem, prevEnh);
                    target.ModifyMaxHealth(-prevItem.BonusMaxHealth);
                    target.ModifyDefense(-(prevItem.BonusDefense + pdef));
                    slot.Item = prevItem;
                    slot.Quantity = 1;
                    slot.EnhancementLevel = prevEnh;
                }
                else
                {
                    Slots.RemoveAt(slotIndex);
                }
                EquippedArmor = item;
                EquippedArmorEnhancement = enhLevel;
                var (_, _, def) = GetEnhancementBonuses(item, enhLevel);
                target.ModifyMaxHealth(item.BonusMaxHealth);
                target.ModifyDefense(item.BonusDefense + def);
            }
            else if (item.Type == ItemType.Accessory)
            {
                var prevItem = EquippedAccessory;
                int prevEnh = EquippedAccessoryEnhancement;
                if (prevItem != null)
                {
                    var (pdmg, _, pdef) = GetEnhancementBonuses(prevItem, prevEnh);
                    target.ModifyDefense(-(prevItem.BonusDefense + pdef));
                    if (prevItem.BonusDamage > 0) target.ModifyBaseDamage(-(prevItem.BonusDamage + pdmg));
                    if (prevItem.BonusMaxHealth > 0) target.ModifyMaxHealth(-prevItem.BonusMaxHealth);
                    slot.Item = prevItem;
                    slot.Quantity = 1;
                    slot.EnhancementLevel = prevEnh;
                }
                else
                {
                    Slots.RemoveAt(slotIndex);
                }
                EquippedAccessory = item;
                EquippedAccessoryEnhancement = enhLevel;
                var (dmgBonus, _, defBonus) = GetEnhancementBonuses(item, enhLevel);
                target.ModifyDefense(item.BonusDefense + defBonus);
                if (item.BonusDamage > 0) target.ModifyBaseDamage(item.BonusDamage + dmgBonus);
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
            EquipExtraInternal(slotIndex, item, targetSlot, target);
            OnInventoryChanged?.Invoke();
            OnEquipmentChanged?.Invoke();
            AudioManager.Instance?.PlaySFX("equip.wav");
            GD.Print($"{item.ItemName} 장착!");
        }

        private static bool IsSlotCompatible(ItemType t, ExtraSlot s) => (t, s) switch
        {
            (ItemType.Helmet, ExtraSlot.Helmet) => true,
            (ItemType.Boots, ExtraSlot.Boots) => true,
            (ItemType.Necklace, ExtraSlot.Necklace) => true,
            (ItemType.Bracelet, ExtraSlot.Bracelet) => true,
            (ItemType.Ring, ExtraSlot.Ring1) => true,
            (ItemType.Ring, ExtraSlot.Ring2) => true,
            _ => false
        };

        private void EquipExtraInternal(int slotIndex, ItemData item, ExtraSlot slotKey, IEquipTarget target)
        {
            var slot = Slots[slotIndex];
            var prev = GetExtraSlot(slotKey);
            if (prev != null)
            {
                ApplyItemBonuses(prev, target, -1);
                slot.Item = prev;
                slot.Quantity = 1;
                slot.EnhancementLevel = 0;
            }
            else
            {
                Slots.RemoveAt(slotIndex);
            }
            SetExtraSlot(slotKey, item);
            ApplyItemBonuses(item, target, +1);
        }

        public bool UnequipWeapon(IEquipTarget target)
        {
            if (EquippedWeapon == null) return false;
            if (Slots.Count >= MaxSlots)
            {
                GD.Print("가방이 꽉 차서 무기를 해제할 수 없습니다!");
                return false;
            }

            var (dmg, _, _) = GetEnhancementBonuses(EquippedWeapon, EquippedWeaponEnhancement);
            target.ModifyBaseDamage(-(EquippedWeapon.BonusDamage + dmg));
            bool added = AddItem(EquippedWeapon, 1, EquippedWeaponEnhancement, fireAcquired: false);
            if (!added) return false;

            EquippedWeapon = null;
            EquippedWeaponEnhancement = 0;
            OnEquipmentChanged?.Invoke();
            return true;
        }

        public bool UnequipArmor(IEquipTarget target)
        {
            if (EquippedArmor == null) return false;
            if (Slots.Count >= MaxSlots)
            {
                GD.Print("가방이 꽉 차서 방어구를 해제할 수 없습니다!");
                return false;
            }

            var (_, _, def) = GetEnhancementBonuses(EquippedArmor, EquippedArmorEnhancement);
            target.ModifyMaxHealth(-EquippedArmor.BonusMaxHealth);
            target.ModifyDefense(-(EquippedArmor.BonusDefense + def));
            bool added = AddItem(EquippedArmor, 1, EquippedArmorEnhancement, fireAcquired: false);
            if (!added) return false;

            EquippedArmor = null;
            EquippedArmorEnhancement = 0;
            OnEquipmentChanged?.Invoke();
            return true;
        }

        public bool UnequipAccessory(IEquipTarget target)
        {
            if (EquippedAccessory == null) return false;
            if (Slots.Count >= MaxSlots)
            {
                GD.Print("가방이 꽉 차서 악세서리를 해제할 수 없습니다!");
                return false;
            }

            var (dmgBonus, _, defBonus) = GetEnhancementBonuses(EquippedAccessory, EquippedAccessoryEnhancement);
            target.ModifyDefense(-(EquippedAccessory.BonusDefense + defBonus));
            if (EquippedAccessory.BonusDamage > 0) target.ModifyBaseDamage(-(EquippedAccessory.BonusDamage + dmgBonus));
            if (EquippedAccessory.BonusMaxHealth > 0) target.ModifyMaxHealth(-EquippedAccessory.BonusMaxHealth);
            bool added = AddItem(EquippedAccessory, 1, EquippedAccessoryEnhancement, fireAcquired: false);
            if (!added) return false;

            EquippedAccessory = null;
            EquippedAccessoryEnhancement = 0;
            OnEquipmentChanged?.Invoke();
            return true;
        }

        // --- 신규 부위별 장비 슬롯 헬퍼 ---

        /// <summary>모자/신발/목걸이/반지/팔찌처럼 강화 미지원 신규 부위인지.</summary>
        public static bool IsExtraEquipType(ItemType t) =>
            t == ItemType.Helmet || t == ItemType.Boots || t == ItemType.Necklace ||
            t == ItemType.Ring || t == ItemType.Bracelet;

        public enum ExtraSlot { Helmet, Boots, Necklace, Ring1, Ring2, Bracelet }

        /// <summary>아이템 타입을 어떤 부위 슬롯에 둘지 결정. 반지는 빈 슬롯 우선.</summary>
        private ExtraSlot ResolveExtraSlot(ItemType t) => t switch
        {
            ItemType.Helmet => ExtraSlot.Helmet,
            ItemType.Boots => ExtraSlot.Boots,
            ItemType.Necklace => ExtraSlot.Necklace,
            ItemType.Bracelet => ExtraSlot.Bracelet,
            ItemType.Ring => EquippedRing1 == null ? ExtraSlot.Ring1 : ExtraSlot.Ring2,
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
            _ => null
        };

        private void SetExtraSlot(ExtraSlot s, ItemData item)
        {
            switch (s)
            {
                case ExtraSlot.Helmet: EquippedHelmet = item; break;
                case ExtraSlot.Boots: EquippedBoots = item; break;
                case ExtraSlot.Necklace: EquippedNecklace = item; break;
                case ExtraSlot.Ring1: EquippedRing1 = item; break;
                case ExtraSlot.Ring2: EquippedRing2 = item; break;
                case ExtraSlot.Bracelet: EquippedBracelet = item; break;
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
        }

        /// <summary>세이브에서 신규 부위별 슬롯 복원. 빈 path는 무시.</summary>
        public void RestoreExtraSlot(ExtraSlot slot, string itemPath, IEquipTarget target)
        {
            if (string.IsNullOrEmpty(itemPath)) return;
            var item = GD.Load<ItemData>(itemPath);
            if (item == null) return;

            SetExtraSlot(slot, item);
            ApplyItemBonuses(item, target, +1);
            OnEquipmentChanged?.Invoke();
        }

        public bool UnequipExtra(ExtraSlot slot, IEquipTarget target)
        {
            var item = GetExtraSlot(slot);
            if (item == null) return false;
            if (Slots.Count >= MaxSlots)
            {
                GD.Print("가방이 꽉 차서 장비를 해제할 수 없습니다!");
                return false;
            }

            ApplyItemBonuses(item, target, -1);
            bool added = AddItem(item, 1, 0, fireAcquired: false);
            if (!added) return false;

            SetExtraSlot(slot, null);
            OnEquipmentChanged?.Invoke();
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

        /// <summary>장착 중인 장비의 강화 수치 변경 (보너스 재계산 포함)</summary>
        public void SetEquippedEnhancement(ItemType type, int newLevel, IEquipTarget target)
        {
            switch (type)
            {
                case ItemType.Weapon when EquippedWeapon != null:
                    var (oldWDmg, _, _) = GetEnhancementBonuses(EquippedWeapon, EquippedWeaponEnhancement);
                    target.ModifyBaseDamage(-oldWDmg);
                    EquippedWeaponEnhancement = newLevel;
                    var (newWDmg, _, _) = GetEnhancementBonuses(EquippedWeapon, newLevel);
                    target.ModifyBaseDamage(newWDmg);
                    break;
                case ItemType.Armor when EquippedArmor != null:
                    var (_, _, oldADef) = GetEnhancementBonuses(EquippedArmor, EquippedArmorEnhancement);
                    target.ModifyDefense(-oldADef);
                    EquippedArmorEnhancement = newLevel;
                    var (_, _, newADef) = GetEnhancementBonuses(EquippedArmor, newLevel);
                    target.ModifyDefense(newADef);
                    break;
                case ItemType.Accessory when EquippedAccessory != null:
                    var (oldAccDmg, _, oldAccDef) = GetEnhancementBonuses(EquippedAccessory, EquippedAccessoryEnhancement);
                    if (EquippedAccessory.BonusDamage > 0) target.ModifyBaseDamage(-oldAccDmg);
                    else target.ModifyDefense(-oldAccDef);
                    EquippedAccessoryEnhancement = newLevel;
                    var (newAccDmg, _, newAccDef) = GetEnhancementBonuses(EquippedAccessory, newLevel);
                    if (EquippedAccessory.BonusDamage > 0) target.ModifyBaseDamage(newAccDmg);
                    else target.ModifyDefense(newAccDef);
                    break;
            }
            OnEquipmentChanged?.Invoke();
        }

        /// <summary>장착 중인 장비 파괴 (강화 실패 시). 모든 보너스 제거 후 null로.</summary>
        public void DestroyEquippedItem(ItemType type, IEquipTarget target)
        {
            switch (type)
            {
                case ItemType.Weapon when EquippedWeapon != null:
                    var (wDmg, _, _) = GetEnhancementBonuses(EquippedWeapon, EquippedWeaponEnhancement);
                    target.ModifyBaseDamage(-(EquippedWeapon.BonusDamage + wDmg));
                    EquippedWeapon = null;
                    EquippedWeaponEnhancement = 0;
                    break;
                case ItemType.Armor when EquippedArmor != null:
                    var (_, _, aDef) = GetEnhancementBonuses(EquippedArmor, EquippedArmorEnhancement);
                    target.ModifyMaxHealth(-EquippedArmor.BonusMaxHealth);
                    target.ModifyDefense(-(EquippedArmor.BonusDefense + aDef));
                    EquippedArmor = null;
                    EquippedArmorEnhancement = 0;
                    break;
                case ItemType.Accessory when EquippedAccessory != null:
                    var (accDmg, _, accDef) = GetEnhancementBonuses(EquippedAccessory, EquippedAccessoryEnhancement);
                    target.ModifyDefense(-(EquippedAccessory.BonusDefense + accDef));
                    if (EquippedAccessory.BonusDamage > 0) target.ModifyBaseDamage(-(EquippedAccessory.BonusDamage + accDmg));
                    if (EquippedAccessory.BonusMaxHealth > 0) target.ModifyMaxHealth(-EquippedAccessory.BonusMaxHealth);
                    EquippedAccessory = null;
                    EquippedAccessoryEnhancement = 0;
                    break;
            }
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

            RestoreEquipment(loadedWeapon, loadedArmor, target, loadedAccessory,
                data.EquippedWeaponEnhancement, data.EquippedArmorEnhancement, data.EquippedAccessoryEnhancement);

            RestoreExtraSlot(ExtraSlot.Helmet, data.EquippedHelmetPath, target);
            RestoreExtraSlot(ExtraSlot.Boots, data.EquippedBootsPath, target);
            RestoreExtraSlot(ExtraSlot.Necklace, migrateNecklacePath, target);
            RestoreExtraSlot(ExtraSlot.Ring1, migrateRing1Path, target);
            RestoreExtraSlot(ExtraSlot.Ring2, data.EquippedRing2Path, target);
            RestoreExtraSlot(ExtraSlot.Bracelet, data.EquippedBraceletPath, target);

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

        // 세이브/로드 시 장비 복원 (RestoreFromSaveData 내부에서 호출)
        public void RestoreEquipment(ItemData weapon, ItemData armor, IEquipTarget target,
            ItemData accessory = null,
            int weaponEnhance = 0, int armorEnhance = 0, int accessoryEnhance = 0)
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
                EquippedArmorEnhancement = armorEnhance;
                var (_, _, def) = GetEnhancementBonuses(armor, armorEnhance);
                target.ModifyMaxHealth(armor.BonusMaxHealth);
                target.ModifyDefense(armor.BonusDefense + def);
            }
            if (accessory != null)
            {
                EquippedAccessory = accessory;
                EquippedAccessoryEnhancement = accessoryEnhance;
                var (dmgBonus, _, defBonus) = GetEnhancementBonuses(accessory, accessoryEnhance);
                target.ModifyDefense(accessory.BonusDefense + defBonus);
                if (accessory.BonusDamage > 0) target.ModifyBaseDamage(accessory.BonusDamage + dmgBonus);
                if (accessory.BonusMaxHealth > 0) target.ModifyMaxHealth(accessory.BonusMaxHealth);
            }
            OnEquipmentChanged?.Invoke();
        }
    }
}
