using Godot;
using FirstGame.Core;
using FirstGame.Data;
using FirstGame.UI;

namespace FirstGame.Entities.Shop
{
    public partial class ShopNPC : BaseInteractable
    {
        [Export] public ItemData[] ShopItems { get; set; }
        [Export] public string ShopName { get; set; } = "상점";

        protected override void OnInteract()
        {
            // 다른 UI가 이미 열려있으면 무시
            if (UIPauseManager.IsPaused) return;

            var shopUI = GetParent().GetNodeOrNull<ShopUI>("ShopUI");
            if (shopUI != null)
            {
                shopUI.OpenShop(ShopItems, ShopName);
            }
            else
            {
                GD.PrintErr("ShopNPC: ShopUI node not found!");
            }
        }
    }
}
