using Godot;

namespace FirstGame.Data
{
	/// <summary>
	/// 메인 퀘스트 chain의 명시적 순서.
	/// DirAccess로 .tres 파일을 열거하던 방식은 Android export에서 .remap 처리와
	/// 충돌할 수 있어 manifest .tres 하나로 순서를 명시한다.
	/// 배열 순서가 곧 chain 진행 순서.
	/// </summary>
	[GlobalClass]
	public partial class QuestManifest : Resource
	{
		[Export] public QuestData[] MainQuests { get; set; }
		// 사이드 퀘스트 — IsRepeatable=true 권장. NPC가 GiverNpcId 매칭으로 부여.
		[Export] public QuestData[] SideQuests { get; set; }
	}
}
