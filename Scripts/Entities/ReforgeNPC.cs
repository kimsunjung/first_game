using Godot;
using FirstGame.Core;
using FirstGame.UI;

namespace FirstGame.Entities
{
	// 재련대 NPC/오브젝트 — 상호작용 시 자식 ReforgeUI를 연다.
	public partial class ReforgeNPC : BaseInteractable
	{
		[Export] public string ReforgeTitle { get; set; } = "장신구 재련";

		protected override void OnInteract()
		{
			if (UIPauseManager.IsPaused) return;
			if (TryOpenQuestDialog()) return;
			if (!CheckLevelGate(FirstGame.Data.FeatureGates.Reforge, ReforgeTitle)) return;
			var ui = GetNodeOrNull<ReforgeUI>("ReforgeUI");
			if (ui != null) ui.OpenReforge(ReforgeTitle);
			else GD.PrintErr("ReforgeNPC: ReforgeUI 노드를 찾을 수 없습니다.");
		}
	}
}
