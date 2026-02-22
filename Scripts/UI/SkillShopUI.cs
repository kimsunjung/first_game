using Godot;
using FirstGame.Data;
using FirstGame.Core;
using FirstGame.Entities.Player;

namespace FirstGame.UI
{
	// 스킬 상점: 스킬북 아이템을 판매 (인벤토리에 추가 → 직접 사용해서 스킬 습득)
	public partial class SkillShopUI : CanvasLayer
	{
		[Export] public ItemData[] SkillBooks { get; set; }

		private VBoxContainer _skillList;
		private Label _goldLabel;
		private Label _messageLabel;
		private Button _closeButton;

		private PlayerController _player;
		private bool _justOpened = false;

		public override void _Ready()
		{
			_skillList = GetNode<VBoxContainer>("%SkillList");
			_goldLabel = GetNode<Label>("%SkillShopGoldLabel");
			_messageLabel = GetNode<Label>("%SkillShopMessageLabel");
			_closeButton = GetNode<Button>("%SkillShopCloseButton");

			_closeButton.Pressed += CloseShop;
			_messageLabel.Visible = false;
			Visible = false;
		}

		public override void _Process(double delta)
		{
			if (_justOpened) { _justOpened = false; return; }
			if (Visible && (Input.IsActionJustPressed("interact") || Input.IsActionJustPressed("ui_cancel")))
				CloseShop();
		}

		public void OpenShop()
		{
			var players = GetTree().GetNodesInGroup("Player");
			if (players.Count == 0 || players[0] is not PlayerController player) return;
			_player = player;

			Visible = true;
			GetTree().Paused = true;
			_justOpened = true;
			RefreshList();
			UpdateGold();
		}

		public void CloseShop()
		{
			Visible = false;
			GetTree().Paused = false;
		}

		private void RefreshList()
		{
			foreach (Node child in _skillList.GetChildren())
				child.QueueFree();

			if (SkillBooks == null) return;

			foreach (var item in SkillBooks)
			{
				if (item?.LearnedSkill == null) continue;
				var skill = item.LearnedSkill;

				var panel = new PanelContainer();
				panel.CustomMinimumSize = new Vector2(360, 80);

				var hbox = new HBoxContainer();
				panel.AddChild(hbox);

				// 아이콘
				if (skill.Icon != null)
				{
					var icon = new TextureRect();
					icon.Texture = skill.Icon;
					icon.CustomMinimumSize = new Vector2(48, 48);
					icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
					hbox.AddChild(icon);
				}

				// 스킬 정보
				var vbox = new VBoxContainer();
				vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

				var nameLabel = new Label();
				nameLabel.Text = $"{skill.SkillName}  (Lv.{skill.RequiredLevel} 이상)";
				nameLabel.AddThemeFontSizeOverride("font_size", 14);
				vbox.AddChild(nameLabel);

				var descLabel = new Label();
				descLabel.Text = $"{skill.Description}  MP:{skill.MpCost}  쿨:{skill.Cooldown}s";
				descLabel.AddThemeFontSizeOverride("font_size", 11);
				vbox.AddChild(descLabel);

				var priceLabel = new Label();
				priceLabel.Text = $"가격: {item.Price}G";
				priceLabel.AddThemeFontSizeOverride("font_size", 12);
				vbox.AddChild(priceLabel);

				hbox.AddChild(vbox);

				// 구매 버튼
				var btn = new Button();
				bool alreadyLearned = _player.Stats.HasSkill(skill.Type);
				bool levelOk = _player.Stats.Level >= skill.RequiredLevel;

				if (alreadyLearned)
				{
					btn.Text = "학습 완료";
					btn.Disabled = true;
				}
				else if (!levelOk)
				{
					btn.Text = $"Lv.{skill.RequiredLevel} 필요";
					btn.Disabled = true;
				}
				else
				{
					var capturedItem = item;
					btn.Text = "구매";
					btn.Pressed += () => BuySkillBook(capturedItem);
				}
				hbox.AddChild(btn);

				_skillList.AddChild(panel);
			}
		}

		private void BuySkillBook(ItemData item)
		{
			if (_player == null) return;
			int price = item.Price;
			if (GameManager.Instance.PlayerGold < price)
			{
				ShowMessage("골드가 부족합니다!");
				return;
			}
			bool added = _player.Inventory.AddItem(item, 1);
			if (!added)
			{
				ShowMessage("가방이 꽉 찼습니다!");
				return;
			}
			GameManager.Instance.PlayerGold -= price;
			AudioManager.Instance?.PlaySFX("shop_buy.wav");
			ShowMessage($"{item.ItemName} 구매! 인벤토리에서 사용하세요.");
			UpdateGold();
			RefreshList();
		}

		private void UpdateGold()
		{
			_goldLabel.Text = $"보유 골드: {GameManager.Instance.PlayerGold}G";
		}

		private async void ShowMessage(string text)
		{
			_messageLabel.Text = text;
			_messageLabel.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.5), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this)) _messageLabel.Visible = false;
		}
	}
}
