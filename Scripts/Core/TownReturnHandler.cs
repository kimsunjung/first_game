using Godot;
using FirstGame.Data;

namespace FirstGame.Core
{
	/// <summary>
	/// 마을 귀환 주문서 사용 처리. Inventory가 SceneManager에 직접 의존하지 않도록 분리.
	/// 귀환 지점은 ReturnPointData.tres에서 로드 — 마을 구조 변경 시 .tres만 교체하면 됨.
	/// </summary>
	public static class TownReturnHandler
	{
		private const string ReturnPointPath = "res://Resources/ReturnPoints/town_default.tres";
		private static ReturnPointData _cached;

		private static ReturnPointData Point
			=> _cached ??= GD.Load<ReturnPointData>(ReturnPointPath) ?? new ReturnPointData();

		/// <summary>현재 마을이 아니면 사용 가능. 호출자는 CanUse → 인벤에서 1개 제거 → Teleport 순서로
		/// 호출해야 함 (Teleport가 즉시 SaveGame을 일으켜 PendingLoadData를 만들기 때문).</summary>
		public static bool CanUse()
		{
			var sm = SceneManager.Instance;
			if (sm == null) return false;
			string current = sm.GetTree()?.CurrentScene?.SceneFilePath;
			if (current == Point.ScenePath)
			{
				GD.Print("이미 마을에 있습니다.");
				return false;
			}
			return true;
		}

		public static bool Teleport()
		{
			return SceneManager.Instance?.ChangeScene(Point.ScenePath, Point.SpawnPosition) ?? false;
		}
	}
}
