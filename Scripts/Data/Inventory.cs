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

        // 장착된 장비의 강화 수치
        public int EquippedWeaponEnhancement { get; private set; } = 0;
        public int EquippedArmorEnhancement { get; private set; } = 0;
        public int EquippedAccessoryEnhancement { get; private set; } = 0;

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

        public bool AddItem(ItemData item, int amount = 1, int enhancementLevel = 0)
        {
            if (item.IsStackable)
            {
                var existing = Slots.Find(s => s.Item.ResourcePath == item.ResourcePath);
                if (existing != null)
                {
                    int space = item.MaxStack - existing.Quantity;
                    if (space > 0)
                    {
                        int toAdd = Math.Min(amount, space);
                        existing.Quantity += toAdd;
                        amount -= toAdd;
                        OnInventoryChanged?.Invoke();
                        OnItemPickedUp?.Invoke(item);
                        if (amount <= 0) return true;
                    }
                }
            }

            if (Slots.Count >= MaxSlots) return false;

            Slots.Add(new InventorySlot { Item = item, Quantity = amount, EnhancementLevel = enhancementLevel });
            OnInventoryChanged?.Invoke();
            OnItemPickedUp?.Invoke(item);
            return true;
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
            else if (slot.Item.Type == ItemType.Weapon || slot.Item.Type == ItemType.Armor || slot.Item.Type == ItemType.Accessory)
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

            if (item.Type == ItemType.Weapon)
            {
                if (EquippedWeapon != null)
                {
                    if (!UnequipWeapon(target)) return;
                }

                EquippedWeapon = item;
                EquippedWeaponEnhancement = enhLevel;
                var (dmg, _, _) = GetEnhancementBonuses(item, enhLevel);
                target.ModifyBaseDamage(item.BonusDamage + dmg);
                RemoveItem(slotIndex, 1);
            }
            else if (item.Type == ItemType.Armor)
            {
                if (EquippedArmor != null)
                {
                    if (!UnequipArmor(target)) return;
                }

                EquippedArmor = item;
                EquippedArmorEnhancement = enhLevel;
                var (_, _, def) = GetEnhancementBonuses(item, enhLevel);
                target.ModifyMaxHealth(item.BonusMaxHealth);
                target.ModifyDefense(item.BonusDefense + def);
                RemoveItem(slotIndex, 1);
            }
            else if (item.Type == ItemType.Accessory)
            {
                if (EquippedAccessory != null)
                {
                    if (!UnequipAccessory(target)) return;
                }

                EquippedAccessory = item;
                EquippedAccessoryEnhancement = enhLevel;
                var (dmgBonus, _, defBonus) = GetEnhancementBonuses(item, enhLevel);
                target.ModifyDefense(item.BonusDefense + defBonus);
                if (item.BonusDamage > 0) target.ModifyBaseDamage(item.BonusDamage + dmgBonus);
                if (item.BonusMaxHealth > 0) target.ModifyMaxHealth(item.BonusMaxHealth);
                RemoveItem(slotIndex, 1);
            }

            OnEquipmentChanged?.Invoke();
            AudioManager.Instance?.PlaySFX("equip.wav");
            GD.Print($"{GetEnhancedName(item, enhLevel)} 장착!");
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
            bool added = AddItem(EquippedWeapon, 1, EquippedWeaponEnhancement);
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
            bool added = AddItem(EquippedArmor, 1, EquippedArmorEnhancement);
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
            bool added = AddItem(EquippedAccessory, 1, EquippedAccessoryEnhancement);
            if (!added) return false;

            EquippedAccessory = null;
            EquippedAccessoryEnhancement = 0;
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

            int slotIndex = Slots.FindIndex(s => s.Item == item);
            if (slotIndex != -1)
            {
                UseItem(slotIndex, target);
            }
            else
            {
                GD.Print($"{item.ItemName}이(가) 인벤토리에 없습니다!");
            }
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

        // 세이브/로드 시 장비 복원
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
