using Godot;
using FirstGame.Data;
using FirstGame.Core;
using FirstGame.Core.Interfaces;

namespace FirstGame.UI
{
	// 스킬 상점: 스킬북 아이템을 판매 (인벤토리에 추가 → 직접 사용해서 스킬 습득)
	public partial class SkillShopUI : BaseUIWindow
	{
		[Export] public ItemData[] SkillBooks { get; set; }

		private VBoxContainer _skillList;
		private Label _goldLabel;
		private Label _messageLabel;
		private Button _closeButton;

		private IPlayer _player;

		protected override void OnReadyInternal()
		{
			_skillList = GetNode<VBoxContainer>("%SkillList");
			_goldLabel = GetNode<Label>("%SkillShopGoldLabel");
			_messageLabel = GetNode<Label>("%SkillShopMessageLabel");
			_closeButton = GetNode<Button>("%SkillShopCloseButton");

			_closeButton.Pressed += Close;
			_messageLabel.Visible = false;
		}

		// --- 스킬 상점 진입점 (NPC 상호작용에서 호출) ---

		public void OpenShop()
		{
			var player = GameManager.Instance?.Player;
			if (player == null) return;
			_player = player;

			if (Visible) OnOpened();
			else Open();
		}

		protected override void OnOpened()
		{
			RefreshList();
			UpdateGold();
		}

		protected override void OnExitTreeInternal()
		{
			if (_closeButton != null) _closeButton.Pressed -= Close;
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
				panel.CustomMinimumSize = new Vector2(260, 40);
				panel.MouseFilter = Control.MouseFilterEnum.Pass;

				var hbox = new HBoxContainer();
				hbox.MouseFilter = Control.MouseFilterEnum.Pass;
				panel.AddChild(hbox);

				// 아이콘
				if (skill.Icon != null)
				{
					var icon = new TextureRect();
					icon.Texture = skill.Icon;
					icon.CustomMinimumSize = new Vector2(24, 24);
					icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
					icon.MouseFilter = Control.MouseFilterEnum.Ignore;
					hbox.AddChild(icon);
				}

				// 스킬 정보
				var vbox = new VBoxContainer();
				vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				vbox.MouseFilter = Control.MouseFilterEnum.Pass;

				var nameLabel = new Label();
				nameLabel.Text = $"{skill.SkillName}";
				nameLabel.AddThemeFontSizeOverride("font_size", 12);
				nameLabel.ClipText = true;
				nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
				vbox.AddChild(nameLabel);

				var descLabel = new Label();
				descLabel.Text = $"{skill.Description}  MP:{skill.MpCost}  쿨:{skill.Cooldown}s";
				descLabel.AddThemeFontSizeOverride("font_size", 10);
				descLabel.ClipText = true;
				descLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
				vbox.AddChild(descLabel);

				var priceLabel = new Label();
				priceLabel.Text = $"{item.Price}G";
				priceLabel.AddThemeFontSizeOverride("font_size", 11);
				priceLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
				vbox.AddChild(priceLabel);

				hbox.AddChild(vbox);

				// 구매 버튼 — PASS: 클릭은 처리되면서 드래그가 ScrollContainer까지 전달됨
				var btn = new Button();
				bool alreadyLearned = _player.Stats.HasSkill(skill.Type);
				bool levelOk = _player.Stats.Level >= skill.RequiredLevel;
				// 클래스 제한 — AvailableToAllClasses=true면 통과, 아니면 플레이어 클래스가 RequiredClass와 일치.
				bool classOk = skill.AvailableToAllClasses || skill.RequiredClass == _player.Stats.PlayerClass;

				btn.MouseFilter = Control.MouseFilterEnum.Pass;
				if (alreadyLearned)
				{
					btn.Text = "학습 완료";
					btn.Disabled = true;
				}
				else if (!classOk)
				{
					btn.Text = $"{FirstGame.Data.PlayerClassUtil.DisplayName(skill.RequiredClass)} 전용";
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
			if (!_player.Inventory.CanAddItem(item, 1))
			{
				ShowMessage("가방이 꽉 찼습니다!");
				return;
			}
			// 골드 먼저 차감 후 AddItem — 역순이면 골드 차감 전 상태가 저장될 수 있음
			GameManager.Instance.PlayerGold -= price;
			_player.Inventory.AddItem(item, 1);
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
