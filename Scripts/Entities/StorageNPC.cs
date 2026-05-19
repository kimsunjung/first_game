using Godot;
using FirstGame.Core;
using FirstGame.UI;

namespace FirstGame.Entities
{
	// 허브 창고 NPC — 상호작용 시 자식 StorageUI를 연다 (ShopNPC와 동일 패턴).
	public partial class StorageNPC : BaseInteractable
	{
		[Export] public string StorageTitle { get; set; } = "공유 창고";

		protected override void OnInteract()
		{
			if (UIPauseManager.IsPaused) return;
			if (TryOpenQuestDialog()) return;
			if (!CheckLevelGate(FirstGame.Data.FeatureGates.Storage, StorageTitle)) return;
			var ui = GetNodeOrNull<StorageUI>("StorageUI");
			if (ui != null) ui.OpenStorage(StorageTitle);
			else GD.PrintErr("StorageNPC: StorageUI 노드를 찾을 수 없습니다.");
		}
	}
}
