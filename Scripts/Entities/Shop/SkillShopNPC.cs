using Godot;
using FirstGame.Data;
using FirstGame.UI;

namespace FirstGame.Entities.Shop
{
	public partial class SkillShopNPC : Area2D
	{
		private bool _playerInRange = false;
		private Label _promptLabel;

		public override void _Ready()
		{
			BodyEntered += OnBodyEntered;
			BodyExited += OnBodyExited;
			_promptLabel = GetNode<Label>("PromptLabel");
			_promptLabel.Visible = false;
		}

		public override void _Process(double delta)
		{
			if (_playerInRange && Input.IsActionJustPressed("interact"))
			{
				if (GetTree().Paused) return;
				var skillShopUI = GetTree().Root.GetNodeOrNull<SkillShopUI>("Main/SkillShopUI");
				if (skillShopUI != null)
					skillShopUI.OpenShop();
				else
					GD.PrintErr("SkillShopNPC: Main/SkillShopUI 노드를 찾을 수 없음!");
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (body.IsInGroup("Player"))
			{
				_playerInRange = true;
				_promptLabel.Visible = true;
			}
		}

		private void OnBodyExited(Node2D body)
		{
			if (body.IsInGroup("Player"))
			{
				_playerInRange = false;
				_promptLabel.Visible = false;
			}
		}
	}
}
