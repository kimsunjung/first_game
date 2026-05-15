using Godot;

namespace FirstGame.Entities
{
	/// <summary>
	/// 일반 시민 NPC. 챕터 대사 + 사이드 퀘스트 give/turn-in을 BaseInteractable이 자동 처리한다.
	/// OnInteract는 BaseInteractable의 기본 dialogue/quest 흐름에 위임.
	/// </summary>
	public partial class CivilianNpc : BaseInteractable
	{
		protected override void OnInteract()
		{
			// 퀘스트 보드 NPC는 별도 QuestBoardUI 사용 — 사이드 퀘스트 목록 표시.
			if (NpcId == "quest_board")
			{
				var ui = GetTree()?.CurrentScene?.GetNodeOrNull<FirstGame.UI.QuestBoardUI>("QuestBoardUI");
				var player = FirstGame.Core.GameManager.Instance?.Player;
				if (ui != null && player != null) ui.OpenForPlayer(player);
				return;
			}
			// 일반 시민 NPC — 챕터 대사는 BaseInteractable이 자동 처리. 퀘스트 수락/완료 시도.
			TryOpenQuestDialog();
		}
	}
}
