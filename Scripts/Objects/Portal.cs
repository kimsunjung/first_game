using Godot;
using FirstGame.Core;
using FirstGame.Entities;

namespace FirstGame.Objects
{
	public partial class Portal : BaseInteractable
	{
		[Export(PropertyHint.File, "*.tscn")]
		public string TargetScenePath { get; set; } = "";

		[Export]
		public Vector2 TargetSpawnPosition { get; set; } = new Vector2(100, 100);

		/// <summary>비워두면 TargetScenePath 파일명에서 자동 추출 (town/field_N/dungeon_N).</summary>
		[Export]
		public string DestinationName { get; set; } = "";

		protected override void OnReady()
		{
			var label = GetNodeOrNull<Label>("PromptLabel");
			if (label == null) return;

			string name = !string.IsNullOrEmpty(DestinationName)
				? DestinationName
				: DeriveNameFromPath(TargetScenePath);

			if (!string.IsNullOrEmpty(name))
				label.Text = $"[F] {name} 이동";
		}

		protected override void OnInteract()
		{
			if (string.IsNullOrEmpty(TargetScenePath)) return;
			SceneManager.Instance?.ChangeScene(TargetScenePath, TargetSpawnPosition);
		}

		private static string DeriveNameFromPath(string path)
		{
			if (string.IsNullOrEmpty(path)) return "";

			// "res://Scenes/Maps/field_1.tscn" → "field_1"
			int lastSlash = path.LastIndexOf('/');
			string fileName = lastSlash >= 0 ? path.Substring(lastSlash + 1) : path;
			if (fileName.EndsWith(".tscn"))
				fileName = fileName.Substring(0, fileName.Length - 5);

			if (fileName == "town") return "마을로";
			if (fileName.StartsWith("field_")) return $"필드 {fileName.Substring(6)}로";
			if (fileName.StartsWith("dungeon_")) return $"던전 {fileName.Substring(8)}로";
			return fileName;
		}
	}
}
