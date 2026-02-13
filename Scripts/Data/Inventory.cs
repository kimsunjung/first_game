using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Entities.Player;

namespace FirstGame.Data
{
    public class InventorySlot
    {
        public ItemData Item { get; set; }
        public int Quantity { get; set; }
    }

    public class Inventory
    {
        public const int MaxSlots = 20;

        public List<InventorySlot> Slots { get; private set; } = new();

        // 장비 슬롯 (Equipment Slots)
        public ItemData EquippedWeapon { get; private set; }
        public ItemData EquippedArmor { get; private set; }

        // UI 갱신용 이벤트 (Events for UI updates)
        public event Action OnInventoryChanged;
        public event Action<ItemData> OnItemPickedUp;   // HUD 알림용 (For HUD notification)
        public event Action OnEquipmentChanged;

        // --- 인벤토리 조작 (Inventory Manipulation) ---

        public bool AddItem(ItemData item, int amount = 1)
        {
            // 스택 가능한 아이템: 기존 슬롯에 추가 (Stackable: Add to existing slot)
            if (item.IsStackable)
            {
                var existing = Slots.Find(s => s.Item.ResourcePath == item.ResourcePath);
                if (existing != null)
                {
                    existing.Quantity = Math.Min(existing.Quantity + amount, item.MaxStack);
                    OnInventoryChanged?.Invoke();
                    OnItemPickedUp?.Invoke(item);
                    return true;
                }
            }

            // 새 슬롯 필요 (Need new slot)
            if (Slots.Count >= MaxSlots) return false; // 인벤토리 가득 참 (Inventory Full)

            Slots.Add(new InventorySlot { Item = item, Quantity = amount });
            OnInventoryChanged?.Invoke();
            OnItemPickedUp?.Invoke(item);
            return true;
        }

        public void RemoveItem(int slotIndex, int amount = 1)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;

            Slots[slotIndex].Quantity -= amount;
            if (Slots[slotIndex].Quantity <= 0)
                Slots.RemoveAt(slotIndex);

            OnInventoryChanged?.Invoke();
        }

        // --- 아이템 사용 (Use Item) ---

        public void UseItem(int slotIndex, PlayerController player)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];

            if (slot.Item.Type == ItemType.Consumable)
            {
                // 포션 사용: HP 회복 (Use Potion: Heal)
                player.Stats.CurrentHealth += slot.Item.HealAmount;
                GD.Print($"{slot.Item.ItemName} 사용! HP +{slot.Item.HealAmount} (Used {slot.Item.ItemName})");
                RemoveItem(slotIndex, 1);
            }
            else if (slot.Item.Type == ItemType.Weapon || slot.Item.Type == ItemType.Armor)
            {
                EquipItem(slotIndex, player);
            }
        }

        // --- 장비 장착/해제 (Equip/Unequip) ---

        public void EquipItem(int slotIndex, PlayerController player)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count) return;
            var slot = Slots[slotIndex];
            var item = slot.Item;

            if (item.Type == ItemType.Weapon)
            {
                // 기존 무기 해제 → 인벤토리로 (Unequip existing weapon -> To Inventory)
                if (EquippedWeapon != null)
                    UnequipWeapon(player);

                EquippedWeapon = item;
                player.Stats.BaseDamage += item.BonusDamage;
                RemoveItem(slotIndex, 1);
            }
            else if (item.Type == ItemType.Armor)
            {
                if (EquippedArmor != null)
                    UnequipArmor(player);

                EquippedArmor = item;
                player.Stats.MaxHealth += item.BonusMaxHealth;
                player.Stats.CurrentHealth += item.BonusMaxHealth; // 장착 시 추가 HP도 채움 (Add max health to current too)
                RemoveItem(slotIndex, 1);
            }

            OnEquipmentChanged?.Invoke();
            GD.Print($"{item.ItemName} 장착! (Equipped {item.ItemName})");
        }

        public void UnequipWeapon(PlayerController player)
        {
            if (EquippedWeapon == null) return;
            player.Stats.BaseDamage -= EquippedWeapon.BonusDamage;
            bool added = AddItem(EquippedWeapon, 1);
            // 인벤토리가 꽉 찼을 때 처리 필요 (Handle full inventory - implementing basic add for now)
            EquippedWeapon = null;
            OnEquipmentChanged?.Invoke();
        }

        public void UnequipArmor(PlayerController player)
        {
            if (EquippedArmor == null) return;
            player.Stats.MaxHealth -= EquippedArmor.BonusMaxHealth;
            if (player.Stats.CurrentHealth > player.Stats.MaxHealth)
                player.Stats.CurrentHealth = player.Stats.MaxHealth;
            AddItem(EquippedArmor, 1);
            EquippedArmor = null;
            OnEquipmentChanged?.Invoke();
        }
    }
}
