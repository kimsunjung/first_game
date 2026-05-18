using Godot;
using FirstGame.Core;
using FirstGame.UI;

namespace FirstGame.Entities
{
	// 제작대 NPC/오브젝트 — 상호작용 시 자식 CraftingUI를 연다 (ShopNPC와 동일 패턴).
	public partial class CraftingNPC : BaseInteractable
	{
		[Export] public string CraftTitle { get; set; } = "제작대";

		protected override void OnInteract()
		{
			if (UIPauseManager.IsPaused) return;
			if (TryOpenQuestDialog()) return;
			var ui = GetNodeOrNull<CraftingUI>("CraftingUI");
			if (ui != null) ui.OpenCrafting(CraftTitle);
			else GD.PrintErr("CraftingNPC: CraftingUI 노드를 찾을 수 없습니다.");
		}
	}
}
