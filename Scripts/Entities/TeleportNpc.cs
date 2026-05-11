using Godot;
using FirstGame.Core;
using FirstGame.Data;
using FirstGame.UI;

namespace FirstGame.Entities
{
	/// <summary>
	/// 마을 텔레포트 NPC. 한 번 다녀온 씬으로 골드를 지불하고 즉시 이동.
	/// 목적지/비용/스폰 좌표는 TeleportDestinationData.tres 배열로 데이터 분리 — 맵 추가 시
	/// 새 .tres 파일을 Destinations에 드래그하면 끝.
	/// </summary>
	public partial class TeleportNpc : BaseInteractable
	{
		[Export] public string NpcName { get; set; } = "텔레포트";
		[Export] public TeleportDestinationData[] Destinations { get; set; } = System.Array.Empty<TeleportDestinationData>();

		protected override void OnInteract()
		{
			if (UIPauseManager.IsPaused) return;
			if (TryOpenQuestDialog()) return;

			var ui = GetNodeOrNull<TeleportUI>("TeleportUI");
			if (ui != null) ui.OpenTeleport(NpcName, Destinations);
			else GD.PrintErr("TeleportNpc: TeleportUI 자식 노드 없음");
		}
	}
}
