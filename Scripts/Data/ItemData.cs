using Godot;

namespace FirstGame.Data
{
    public enum ItemType
    {
        Consumable, // 포션 등 소비 아이템 (Consumable)
        Weapon,     // 무기 (Weapon)
        Armor       // 방어구 (Armor)
    }

    [GlobalClass]
    public partial class ItemData : Resource
    {
        [Export] public string ItemName { get; set; } = "";
        [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
        [Export] public Texture2D Icon { get; set; }
        [Export] public int Price { get; set; } = 0;
        [Export] public ItemType Type { get; set; } = ItemType.Consumable;
        [Export] public bool IsStackable { get; set; } = true;
        [Export] public int MaxStack { get; set; } = 99;

        // 소비 아이템 효과 (Consumable Effects)
        [ExportGroup("Consumable Effects")]
        [Export] public int HealAmount { get; set; } = 0;

        // 장비 보너스 (Equipment Bonuses)
        [ExportGroup("Equipment Bonuses")]
        [Export] public int BonusDamage { get; set; } = 0;
        [Export] public int BonusMaxHealth { get; set; } = 0;
        [Export] public float BonusMoveSpeed { get; set; } = 0f;
    }
}
