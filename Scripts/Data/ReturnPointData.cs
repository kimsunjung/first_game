using Godot;

namespace FirstGame.Data
{
	/// <summary>
	/// 귀환 주문서/리스폰 등 "기본 귀환 지점" 데이터. 마을 씬 경로 + 도착 스폰 좌표.
	/// 마을 구조 변경 시 코드 수정 없이 .tres만 교체.
	/// </summary>
	[GlobalClass]
	public partial class ReturnPointData : Resource
	{
		[Export] public string ScenePath { get; set; } = "res://Scenes/Maps/town.tscn";
		[Export] public Vector2 SpawnPosition { get; set; } = new Vector2(128, 240);
	}
}
