using Godot;
using FirstGame.Core;
using FirstGame.UI;

namespace FirstGame.Entities
{
	// 사냥 계약 보드 — 상호작용 시 자식 ContractBoardUI를 연다 (CraftingNPC와 동일 패턴).
	// 메인 퀘스트 보드(quest_board / QuestBoardUI)와 별개. 권역은 허브별로 Export 오버라이드.
	public partial class ContractBoardNPC : BaseInteractable
	{
		[Export] public string Region { get; set; } = "town_region";
		[Export] public string BoardTitle { get; set; } = "사냥 계약 게시판";

		protected override void OnInteract()
		{
			if (UIPauseManager.IsPaused) return;
			var ui = GetNodeOrNull<ContractBoardUI>("ContractBoardUI");
			if (ui != null) ui.OpenBoard(Region, BoardTitle);
			else GD.PrintErr("ContractBoardNPC: ContractBoardUI 노드를 찾을 수 없습니다.");
		}
	}
}
