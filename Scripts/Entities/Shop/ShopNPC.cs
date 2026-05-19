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
        // 0 = 게이트 없음(여행상인 등 기본 상점). 재료/광산 상점은 .tscn에서
        // Lv.5 지정(초반 정보량 완화 — material_shop_npc.tscn).
        [Export] public int MinPlayerLevel { get; set; } = 0;

        protected override void OnInteract()
        {
            // 다른 UI가 이미 열려있으면 무시
            if (UIPauseManager.IsPaused) return;

            // 퀘스트 부여/진행/완료가 가능하면 다이얼로그 우선
            if (TryOpenQuestDialog()) return;
            if (!CheckLevelGate(MinPlayerLevel, ShopName)) return;

            var shopUI = GetNodeOrNull<ShopUI>("ShopUI");
            if (shopUI != null)
            {
                shopUI.OpenShop(FilterByChapter(ShopItems), ShopName);
            }
            else
            {
                GD.PrintErr("ShopNPC: ShopUI node not found!");
            }
        }

        /// <summary>
        /// GameManager.CurrentChapter < item.MinRequiredChapter 인 아이템은 진열 제외.
        /// 기본값 Prologue(0)이면 항상 진열되므로 기존 .tres 대부분은 영향 없음.
        /// </summary>
        private static ItemData[] FilterByChapter(ItemData[] items)
        {
            if (items == null || items.Length == 0) return items;
            var chapter = GameManager.Instance?.CurrentChapter ?? Chapter.Prologue;
            var filtered = new System.Collections.Generic.List<ItemData>(items.Length);
            foreach (var it in items)
            {
                if (it == null) continue;
                if ((int)chapter >= (int)it.MinRequiredChapter)
                    filtered.Add(it);
            }
            return filtered.ToArray();
        }
    }
}
