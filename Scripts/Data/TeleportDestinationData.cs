using Godot;

namespace FirstGame.Data
{
	/// <summary>
	/// 텔레포트 NPC가 노출할 목적지 1건. 씬 경로/표시명/비용/도착 스폰 좌표를 데이터로 분리.
	/// 새 맵 추가 시 .tres 파일 추가만 하면 되며, 코드 변경/빌드 불필요.
	/// </summary>
	[GlobalClass]
	public partial class TeleportDestinationData : Resource
	{
		[Export] public string ScenePath { get; set; } = "";
		[Export] public string DisplayName { get; set; } = "";
		[Export] public int Cost { get; set; } = 0;
		[Export] public Vector2 SpawnPosition { get; set; } = Vector2.Zero;
	}
}
