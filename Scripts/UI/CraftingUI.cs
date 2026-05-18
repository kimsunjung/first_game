using Godot;
using System.Collections.Generic;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	// 제작 v1 — 재료+골드 확정 소비 → 결과 확정 지급. 확률/파괴/뽑기 없음.
	// 재료차감+골드차감+결과지급은 GameTransaction은 메모리 자동 rollback이 아니라 autosave/보류보상
		// 클레임 억제용. 실패할 수 있는 검사는 트랜잭션 진입 전에 모두 끝내고,
		// 트랜잭션 안에서는 검증을 통과한 확정 mutation만 수행(부분 차감 방지).
		// 종료 후 명시 SaveGame.
	public partial class CraftingUI : BaseUIWindow
	{
		private Label _titleLabel;
		private VBoxContainer _list;
		private Label _goldLabel;
		private Label _messageLabel;
		private Button _closeButton;

		private IPlayer _player;
		private Inventory _inventory;

		protected override void OnReadyInternal()
		{
			_titleLabel = GetNode<Label>("%CraftTitleLabel");
			_list = GetNode<VBoxContainer>("%CraftList");
			_goldLabel = GetNode<Label>("%CraftGoldLabel");
			_messageLabel = GetNode<Label>("%CraftMessageLabel");
			_closeButton = GetNode<Button>("%CraftCloseButton");
			_closeButton.Pressed += Close;
			_messageLabel.Visible = false;
		}

		public void OpenCrafting(string title)
		{
			var player = GameManager.Instance?.Player;
			if (player == null) return;
			_player = player;
			_inventory = player.Inventory;
			CraftingData.EnsureLoaded();
			if (_titleLabel != null && !string.IsNullOrEmpty(title)) _titleLabel.Text = title;
			if (Visible) OnOpened();
			else Open();
		}

		protected override void OnOpened() => Rebuild();
		protected override void OnExitTreeInternal()
		{
			if (_closeButton != null) _closeButton.Pressed -= Close;
		}

		private void UpdateGold() =>
			_goldLabel.Text = $"보유 골드: {GameManager.Instance.PlayerGold}G";

		private void Rebuild()
		{
			foreach (Node c in _list.GetChildren()) c.QueueFree();
			UpdateGold();
			foreach (var r in CraftingData.Recipes)
			{
				var result = GD.Load<ItemData>(r.ResultPath);
				if (result == null) continue;

				var parts = new List<string>();
				bool haveAll = true;
				foreach (var m in r.Materials)
				{
					var mi = GD.Load<ItemData>(m.Path);
					string mn = mi != null ? mi.ItemName : m.Path;
					int have = mi != null ? _inventory.CountItem(mi) : 0;
					if (have < m.Qty) haveAll = false;
					parts.Add($"{mn} {have}/{m.Qty}");
				}
				bool goldOk = GameManager.Instance.PlayerGold >= r.Gold;
				bool spaceOk = _inventory.CanAddItem(result, r.ResultQty);
				bool canCraft = haveAll && goldOk && spaceOk;

				var panel = new PanelContainer { CustomMinimumSize = new Vector2(360, 30) };
				panel.MouseFilter = Control.MouseFilterEnum.Pass;
				var hbox = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Pass };
				panel.AddChild(hbox);
				var vbox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
				vbox.MouseFilter = Control.MouseFilterEnum.Ignore;
				var nm = new Label { Text = $"{r.Name} → {result.ItemName} x{r.ResultQty}" };
				nm.AddThemeFontSizeOverride("font_size", 11);
				vbox.AddChild(nm);
				var det = new Label { Text = $"재료: {string.Join(", ", parts)}  |  {r.Gold}G" };
				det.AddThemeFontSizeOverride("font_size", 8);
				det.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 1f));
				vbox.AddChild(det);
				hbox.AddChild(vbox);

				var btn = new Button { Text = canCraft ? "제작" : (!spaceOk ? "공간부족" : (!goldOk ? "골드부족" : "재료부족")) };
				btn.Disabled = !canCraft;
				btn.AddThemeFontSizeOverride("font_size", 10);
				btn.MouseFilter = Control.MouseFilterEnum.Pass;
				var captured = r;
				btn.Pressed += () => Craft(captured);
				hbox.AddChild(btn);
				_list.AddChild(panel);
			}
		}

		private void Craft(CraftingData.Recipe r)
		{
			var result = GD.Load<ItemData>(r.ResultPath);
			if (result == null) { ShowMessage("결과 아이템 로드 실패."); return; }

			// 사전 검증 — 하나라도 부족하면 아무것도 차감하지 않는다.
			// 재료를 ResourcePath 기준으로 합산: 같은 재료가 여러 줄로 기재돼도
			// 총량으로 검증/소비된다(줄당 검증 시 누락되는 결함 차단).
			var need = new Dictionary<string, (ItemData item, int qty)>();
			foreach (var m in r.Materials)
			{
				var mi = GD.Load<ItemData>(m.Path);
				if (mi == null) { ShowMessage("재료 로드 실패."); return; }
				if (need.TryGetValue(m.Path, out var cur))
					need[m.Path] = (cur.item, cur.qty + m.Qty);
				else
					need[m.Path] = (mi, m.Qty);
			}
			foreach (var kv in need)
				if (!_inventory.HasItems(kv.Value.item, kv.Value.qty)) { ShowMessage("재료가 부족합니다."); return; }
			if (GameManager.Instance.PlayerGold < r.Gold) { ShowMessage("골드가 부족합니다."); return; }
			if (!_inventory.CanAddItem(result, r.ResultQty)) { ShowMessage("가방 공간이 부족합니다."); return; }

			using (GameTransaction.Begin())
			{
				// 실패 가능 작업(결과 지급)을 가장 먼저 — 여기서 실패하면 재료/골드
				// 차감 전이라 상태 불변으로 안전 종료(부분 차감 없음).
				// 재료 소비/골드 차감은 위 사전 검증을 통과했으므로 실패하지 않는다.
				if (!_inventory.AddItem(result, r.ResultQty, 0, fireAcquired: false))
				{
					ShowMessage("제작 실패 (결과 지급 오류).");
					return;
				}
				foreach (var kv in need)
					_inventory.ConsumeItems(kv.Value.item, kv.Value.qty);
				GameManager.Instance.PlayerGold -= r.Gold;
			}
			SaveManager.SaveGame();
			AudioManager.Instance?.PlaySFX("craft_success.wav");
			ShowMessage($"{result.ItemName} x{r.ResultQty} 제작 완료!");
			Rebuild();
		}

		private async void ShowMessage(string text)
		{
			_messageLabel.Text = text;
			_messageLabel.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this)) _messageLabel.Visible = false;
		}
	}
}
