using Godot;
using System.Collections.Generic;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	// 재련 v1 — 미장착 장신구의 affix를 재료+골드 확정 소비로 재생성.
	// 장착 중 장비는 대상 제외(스탯 재계산 위험 차단). 확률 파괴 없음.
	public partial class ReforgeUI : BaseUIWindow
	{
		private const string MaterialPath = "res://Resources/Items/enhance_stone.tres";

		private Label _titleLabel;
		private VBoxContainer _list;
		private Label _goldLabel;
		private Label _messageLabel;
		private Button _closeButton;

		private IPlayer _player;
		private Inventory _inventory;

		protected override void OnReadyInternal()
		{
			_titleLabel = GetNode<Label>("%ReforgeTitleLabel");
			_list = GetNode<VBoxContainer>("%ReforgeList");
			_goldLabel = GetNode<Label>("%ReforgeGoldLabel");
			_messageLabel = GetNode<Label>("%ReforgeMessageLabel");
			_closeButton = GetNode<Button>("%ReforgeCloseButton");
			_closeButton.Pressed += Close;
			_messageLabel.Visible = false;
		}

		public void OpenReforge(string title)
		{
			var player = GameManager.Instance?.Player;
			if (player == null) return;
			_player = player;
			_inventory = player.Inventory;
			if (_titleLabel != null && !string.IsNullOrEmpty(title)) _titleLabel.Text = title;
			if (Visible) OnOpened();
			else Open();
		}

		protected override void OnOpened() => Rebuild();
		protected override void OnExitTreeInternal()
		{
			if (_closeButton != null) _closeButton.Pressed -= Close;
		}

		// rarity별 비용 — 상위일수록 비쌈. (gold, enhance_stone 개수)
		private static (int gold, int mat) CostFor(ItemRarity r) => r switch
		{
			ItemRarity.Common    => (60, 1),
			ItemRarity.Uncommon  => (120, 1),
			ItemRarity.Rare      => (200, 2),
			ItemRarity.Epic      => (350, 3),
			ItemRarity.Legendary => (600, 4),
			_                    => (60, 1)
		};

		private static string AffixSummary(List<ItemAffix> affixes)
		{
			if (affixes == null || affixes.Count == 0) return "옵션 없음";
			var p = new List<string>();
			foreach (var a in affixes)
			{
				string n = a.Type switch
				{
					ItemAffixType.BonusDamage => "공격",
					ItemAffixType.BonusDefense => "방어",
					ItemAffixType.BonusMaxHealth => "HP",
					ItemAffixType.BonusMaxMp => "MP",
					ItemAffixType.BonusCritRate => "치명%",
					ItemAffixType.BonusMoveSpeed => "이속",
					ItemAffixType.BonusAttackSpeed => "공속",
					ItemAffixType.BonusLifesteal => "흡혈%",
					_ => "?"
				};
				p.Add($"{n}+{a.Value:0.##}");
			}
			return string.Join(", ", p);
		}

		private void UpdateGold() =>
			_goldLabel.Text = $"보유 골드: {GameManager.Instance.PlayerGold}G";

		private void Rebuild()
		{
			foreach (Node c in _list.GetChildren()) c.QueueFree();
			UpdateGold();
			var mat = GD.Load<ItemData>(MaterialPath);
			int matHave = mat != null ? _inventory.CountItem(mat) : 0;

			for (int i = 0; i < _inventory.Slots.Count; i++)
			{
				var slot = _inventory.Slots[i];
				if (slot.Item == null) continue;
				if (slot.IsEquipped) continue;                       // 장착 중 제외 (안전)
				if (!AffixGenerator.IsJewelry(slot.Item.Type)) continue;

				var (g, m) = CostFor(slot.Item.Rarity);
				bool ok = GameManager.Instance.PlayerGold >= g && matHave >= m;
				int idx = i;

				var panel = new PanelContainer { CustomMinimumSize = new Vector2(360, 30) };
				panel.MouseFilter = Control.MouseFilterEnum.Pass;
				var hbox = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Pass };
				panel.AddChild(hbox);
				var vbox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
				vbox.MouseFilter = Control.MouseFilterEnum.Ignore;
				var nm = new Label { Text = slot.Item.ItemName };
				nm.AddThemeFontSizeOverride("font_size", 11);
				vbox.AddChild(nm);
				var det = new Label { Text = $"{AffixSummary(slot.Affixes)}  |  {g}G + 강화석x{m}" };
				det.AddThemeFontSizeOverride("font_size", 8);
				det.AddThemeColorOverride("font_color", new Color(0.8f, 0.85f, 1f));
				vbox.AddChild(det);
				hbox.AddChild(vbox);
				var btn = new Button { Text = ok ? "재련" : "재료/골드 부족", Disabled = !ok };
				btn.AddThemeFontSizeOverride("font_size", 10);
				btn.MouseFilter = Control.MouseFilterEnum.Pass;
				btn.Pressed += () => Reforge(idx);
				hbox.AddChild(btn);
				_list.AddChild(panel);
			}
		}

		private void Reforge(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= _inventory.Slots.Count) return;
			var slot = _inventory.Slots[slotIndex];
			if (slot.Item == null || slot.IsEquipped || !AffixGenerator.IsJewelry(slot.Item.Type))
			{ ShowMessage("재련할 수 없는 대상입니다."); return; }

			var mat = GD.Load<ItemData>(MaterialPath);
			var (gold, matQty) = CostFor(slot.Item.Rarity);
			if (mat == null || !_inventory.HasItems(mat, matQty)) { ShowMessage("강화석이 부족합니다."); return; }
			if (GameManager.Instance.PlayerGold < gold) { ShowMessage("골드가 부족합니다."); return; }

			using (GameTransaction.Begin())
			{
				if (!_inventory.ConsumeItems(mat, matQty)) { ShowMessage("재료 처리 오류."); return; }
				GameManager.Instance.PlayerGold -= gold;
				// affix만 재생성 — ResourcePath/수량/강화수치 유지. 미장착이라 스탯 재계산 불필요.
				slot.Affixes = AffixGenerator.GenerateForJewelry(slot.Item.Rarity);
				_inventory.NotifyChanged();
			}
			SaveManager.SaveGame();
			AudioManager.Instance?.PlaySFX("craft_success.wav");
			ShowMessage($"{slot.Item.ItemName} 재련 완료! → {AffixSummary(slot.Affixes)}");
			Rebuild();
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
