using Godot;
using FirstGame.Core;
using FirstGame.Data;
using FirstGame.UI;

namespace FirstGame.Entities.Shop
{
	public partial class SkillShopNPC : BaseInteractable
	{
		protected override void OnInteract()
		{
			if (UIPauseManager.IsPaused) return;

			var skillShopUI = GetParent().GetNodeOrNull<SkillShopUI>("SkillShopUI");
			if (skillShopUI != null)
				skillShopUI.OpenShop();
			else
				GD.PrintErr("SkillShopNPC: SkillShopUI 노드를 찾을 수 없음!");
		}
	}
}
