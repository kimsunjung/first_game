using Godot;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class QuestData : Resource
	{
		[Export] public string QuestId { get; set; } = "";
		[Export] public string QuestTitle { get; set; } = "퀘스트";
		[Export] public string Description { get; set; } = "";
		[Export] public string TargetEnemyType { get; set; } = "Orc"; // EnemyTypeName과 매칭
		[Export] public int TargetCount { get; set; } = 5;
		[Export] public int GoldReward { get; set; } = 100;
	}
}
