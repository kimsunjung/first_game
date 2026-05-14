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
			// 챕터 대사는 BaseInteractable._UnhandledInput이 ShowChapterDialogueIfAny()를 먼저 호출.
			// 여기서는 사이드 퀘스트 수락/완료 다이얼로그만 시도.
			TryOpenQuestDialog();
		}
	}
}
