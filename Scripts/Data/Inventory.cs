using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Core;
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

        // 퀵슬롯 (Quick Slots - 4 slots)
        public ItemData[] QuickSlots { get; private set; } = new ItemData[4];

        // UI 갱신용 이벤트 (Events for UI updates)
        public event Action OnInventoryChanged;
        public event Action<ItemData> OnItemPickedUp;   // HUD 알림용 (For HUD notification)
        public event Action OnEquipmentChanged;
        public event Action OnQuickSlotChanged; // 퀵슬롯 변경 이벤트

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

            // 수량 감소 (Decrease Quantity)
            // 0 이하로 내려가지 않도록 Max 사용 (Prevent negative quantity)
            Slots[slotIndex].Quantity = Math.Max(0, Slots[slotIndex].Quantity - amount);

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
                player.Stats.CurrentHealth += slot.Item.HealAmount;
                GD.Print($"{slot.Item.ItemName} 사용! HP +{slot.Item.HealAmount}");
                AudioManager.Instance?.PlaySFX("potion_use.wav");
                RemoveItem(slotIndex, 1);
            }
            else if (slot.Item.Type == ItemType.Weapon || slot.Item.Type == ItemType.Armor)
            {
                EquipItem(slotIndex, player);
            }
            else if (slot.Item.Type == ItemType.SkillBook)
            {
                if (slot.Item.LearnedSkill == null) return;
                bool learned = player.Stats.LearnSkill(slot.Item.LearnedSkill);
                if (learned)
                {
                    AudioManager.Instance?.PlaySFX("skill_learn.wav");
                    RemoveItem(slotIndex, 1);
                }
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
                // 기존 무기 해제 시도 (Try unequip existing weapon)
                if (EquippedWeapon != null)
                {
                    if (!UnequipWeapon(player)) return; // 해제 실패 시 중단 (Stop if unequip fails)
                }

                EquippedWeapon = item;
                player.Stats.BaseDamage += item.BonusDamage;
                RemoveItem(slotIndex, 1);
            }
            else if (item.Type == ItemType.Armor)
            {
                if (EquippedArmor != null)
                {
                    if (!UnequipArmor(player)) return;
                }

                EquippedArmor = item;
                player.Stats.MaxHealth += item.BonusMaxHealth;
                player.Stats.CurrentHealth += item.BonusMaxHealth; // 장착 시 추가 HP도 채움 (Add max health to current too)
                RemoveItem(slotIndex, 1);
            }

            OnEquipmentChanged?.Invoke();
            AudioManager.Instance?.PlaySFX("equip.wav");
            GD.Print($"{item.ItemName} 장착! (Equipped {item.ItemName})");
        }

        public bool UnequipWeapon(PlayerController player)
        {
            if (EquippedWeapon == null) return false;
            
            // 가방 공간 체크 (Check Inventory Space)
            if (Slots.Count >= MaxSlots) 
            {
                GD.Print("가방이 꽉 차서 무기를 해제할 수 없습니다! (Inventory Full!)");
                return false; 
            }

            player.Stats.BaseDamage -= EquippedWeapon.BonusDamage;
            bool added = AddItem(EquippedWeapon, 1); // AddItem은 성공 여부 반환 (AddItem returns success)
            if (!added) return false; // 혹시라도 실패하면 중단

            EquippedWeapon = null;
            OnEquipmentChanged?.Invoke();
            return true;
        }



        public bool UnequipArmor(PlayerController player)
        {
            if (EquippedArmor == null) return false;
            
            if (Slots.Count >= MaxSlots) 
            {
                GD.Print("가방이 꽉 차서 방어구를 해제할 수 없습니다! (Inventory Full!)");
                return false; 
            }

            player.Stats.MaxHealth -= EquippedArmor.BonusMaxHealth;
            if (player.Stats.CurrentHealth > player.Stats.MaxHealth)
                player.Stats.CurrentHealth = player.Stats.MaxHealth;
            
            bool added = AddItem(EquippedArmor, 1);
            if (!added) return false;

            EquippedArmor = null;
            OnEquipmentChanged?.Invoke();
            return true;
        }

        // --- 퀵슬롯 로직 (Quick Slot Logic) ---

        public void AssignQuickSlot(int quickSlotIndex, ItemData item)
        {
            if (quickSlotIndex < 0 || quickSlotIndex >= QuickSlots.Length) return;

            // 이미 등록된 아이템이면 해제 (Toggle logic or overwrite)
            // 여기서는 덮어쓰기 방식으로 구현
            QuickSlots[quickSlotIndex] = item;
            OnQuickSlotChanged?.Invoke();
            GD.Print($"퀵슬롯 {quickSlotIndex + 1}번에 {item.ItemName} 등록됨.");
        }

        public void UseQuickSlot(int quickSlotIndex, PlayerController player)
        {
            if (quickSlotIndex < 0 || quickSlotIndex >= QuickSlots.Length) return;
            var item = QuickSlots[quickSlotIndex];
            if (item == null) return;

            // 인벤토리에서 해당 아이템 찾기 (Find item in inventory)
            int slotIndex = Slots.FindIndex(s => s.Item == item);

            if (slotIndex != -1)
            {
                UseItem(slotIndex, player); // 아이템 사용 로직 위임
                // 소비 아이템이면 수량이 줄어들었을 테니 UI 갱신됨.
                // 만약 다 써서 없어지면? -> UseItem 내부에서 RemoveItem 호출됨.
                // 퀵슬롯에 등록된 아이템이 인벤토리에 하나도 없게 되면? -> 퀵슬롯은 유지하되 0개로 표시하거나 비활성화 (UI 처리)
            }
            else
            {
                GD.Print($"{item.ItemName}이(가) 인벤토리에 없습니다!");
            }
        }

        // 세이브/로드 시 장비 복원 (스탯 보너스 적용 포함)
        public void RestoreEquipment(ItemData weapon, ItemData armor, PlayerController player)
        {
            if (weapon != null)
            {
                EquippedWeapon = weapon;
                player.Stats.BaseDamage += weapon.BonusDamage;
            }
            if (armor != null)
            {
                EquippedArmor = armor;
                player.Stats.MaxHealth += armor.BonusMaxHealth;
            }
            OnEquipmentChanged?.Invoke();
        }
    }
}
