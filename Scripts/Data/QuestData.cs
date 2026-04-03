using Godot;

namespace FirstGame.Data
{
	public enum QuestType
	{
		Kill,       // 몬스터 N마리 처치
		Gather,     // 아이템 N개 수집
		Explore     // 특정 장소 도달
	}

	[GlobalClass]
	public partial class QuestData : Resource
	{
		[Export] public string QuestId { get; set; } = "";
		[Export] public string QuestTitle { get; set; } = "퀘스트";
		[Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
		[Export] public QuestType Type { get; set; } = QuestType.Kill;
		[Export] public int TargetCount { get; set; } = 5;

		// Kill 퀘스트
		[ExportGroup("Kill Quest")]
		[Export] public string TargetEnemyType { get; set; } = "Orc";

		// Gather 퀘스트
		[ExportGroup("Gather Quest")]
		[Export] public ItemData TargetItem { get; set; }

		// Explore 퀘스트
		[ExportGroup("Explore Quest")]
		[Export] public string TargetScene { get; set; } = "";

		// 보상
		[ExportGroup("Rewards")]
		[Export] public int GoldReward { get; set; } = 100;
		[Export] public int ExpReward { get; set; } = 0;
		[Export] public ItemData RewardItem { get; set; }
	}
}
