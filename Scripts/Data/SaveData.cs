using System.Collections.Generic; // List 사용을 위해 추가 (Added for List)

namespace FirstGame.Data
{
    public class SavedItemSlot
    {
        public string ItemPath { get; set; }  // "res://Resources/Items/health_potion.tres"
        public int Quantity { get; set; }
    }

    // 저장 데이터 클래스 (JSON 직렬화용)
    public class SaveData
    {
        public float PlayerPosX { get; set; }
        public float PlayerPosY { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerMaxHealth { get; set; }
        public int PlayerGold { get; set; }
        public string Timestamp { get; set; }

        public List<SavedItemSlot> InventoryItems { get; set; } = new(); // 인벤토리 아이템 (Inventory Items)
        public string EquippedWeaponPath { get; set; } // 장착 무기 (Equipped Weapon)
        public string EquippedArmorPath { get; set; } // 장착 방어구 (Equipped Armor)
    }
}
