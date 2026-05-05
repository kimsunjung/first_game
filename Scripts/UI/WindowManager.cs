using Godot;

namespace FirstGame.UI
{
	/// <summary>
	/// "ui_window" 그룹에 등록된 BaseUIWindow 인스턴스들을 일괄 제어합니다.
	/// </summary>
	public static class WindowManager
	{
		/// <summary>except를 제외한 열린 모든 BaseUIWindow를 닫습니다.</summary>
		public static void CloseOthers(BaseUIWindow except)
		{
			var tree = Engine.GetMainLoop() as SceneTree;
			if (tree == null) return;

			foreach (Node n in tree.GetNodesInGroup(BaseUIWindow.GroupName))
			{
				if (n is BaseUIWindow w && w != except && w.Visible)
					w.Close();
			}
		}

		/// <summary>현재 열린 첫 BaseUIWindow를 닫습니다 (뒤로가기 버튼용). 닫은 창이 있으면 true.</summary>
		public static bool CloseTop()
		{
			var tree = Engine.GetMainLoop() as SceneTree;
			if (tree == null) return false;

			foreach (Node n in tree.GetNodesInGroup(BaseUIWindow.GroupName))
			{
				if (n is BaseUIWindow w && w.Visible)
				{
					w.Close();
					return true;
				}
			}
			return false;
		}
	}
}
