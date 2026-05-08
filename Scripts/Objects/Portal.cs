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

		protected override void OnInteract()
		{
			if (string.IsNullOrEmpty(TargetScenePath)) return;
			SceneManager.Instance?.ChangeScene(TargetScenePath, TargetSpawnPosition);
		}
	}
}
