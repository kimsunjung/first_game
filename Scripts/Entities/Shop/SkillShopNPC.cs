using Godot;
using FirstGame.Core;
using FirstGame.Data;
using FirstGame.UI;

namespace FirstGame.Entities.Shop
{
	public partial class SkillShopNPC : BaseInteractable
	{
		// 0 = 게이트 없음(town 기본기 상점). town 외 허브는 .tscn에서
		// 권장값 지정(field_outpost 8 / harbor_village 8 / mountain_refuge 12).
		[Export] public int MinPlayerLevel { get; set; } = 0;
		[Export] public string ShopTitle { get; set; } = "스킬 상점";

		protected override void OnInteract()
		{
			if (UIPauseManager.IsPaused) return;

			if (TryOpenQuestDialog()) return;
			if (!CheckLevelGate(MinPlayerLevel, ShopTitle)) return;

			var skillShopUI = GetNodeOrNull<SkillShopUI>("SkillShopUI");
			if (skillShopUI != null)
				skillShopUI.OpenShop();
			else
				GD.PrintErr("SkillShopNPC: SkillShopUI 노드를 찾을 수 없음!");
		}
	}
}
