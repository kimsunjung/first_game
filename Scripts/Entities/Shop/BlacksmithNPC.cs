using Godot;
using FirstGame.Core;
using FirstGame.UI;

namespace FirstGame.Entities.Shop
{
	public partial class BlacksmithNPC : BaseInteractable
	{
		[Export] public string SmithName { get; set; } = "대장장이";

		protected override void OnInteract()
		{
			if (UIPauseManager.IsPaused) return;

			var enhanceUI = GetParent().GetNodeOrNull<EnhanceUI>("EnhanceUI");
			if (enhanceUI != null)
			{
				enhanceUI.OpenEnhance(SmithName);
			}
			else
			{
				GD.PrintErr("BlacksmithNPC: EnhanceUI node not found!");
			}
		}
	}
}
