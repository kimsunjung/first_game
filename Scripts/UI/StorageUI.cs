using Godot;
using System.Collections.Generic;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	// 공유 창고 v1 — 인벤 ↔ 창고 전체 슬롯 이동. 장착 슬롯은 보관 불가.
	// 이동은 GameTransaction으로 오토세이브 중간상태 차단 + 종료 후 명시 SaveGame.
	public partial class StorageUI : BaseUIWindow
	{
		private Label _titleLabel;
		private VBoxContainer _invList;
		private VBoxContainer _storageList;
		private Label _messageLabel;
		private Button _closeButton;

		private IPlayer _player;
		private Inventory _inventory;

		protected override void OnReadyInternal()
		{
			_titleLabel = GetNode<Label>("%StorageTitleLabel");
			_invList = GetNode<VBoxContainer>("%StorageInvList");
			_storageList = GetNode<VBoxContainer>("%StorageStoreList");
			_messageLabel = GetNode<Label>("%StorageMessageLabel");
			_closeButton = GetNode<Button>("%StorageCloseButton");
			_closeButton.Pressed += Close;
			_messageLabel.Visible = false;
		}

		public void OpenStorage(string title)
		{
			var player = GameManager.Instance?.Player;
			if (player == null) return;
			_player = player;
			_inventory = player.Inventory;
			if (_titleLabel != null && !string.IsNullOrEmpty(title)) _titleLabel.Text = title;
			if (Visible) OnOpened();
			else Open();
		}

		protected override void OnOpened() => RefreshBoth();

		protected override void OnExitTreeInternal()
		{
			if (_closeButton != null) _closeButton.Pressed -= Close;
		}

		private void RefreshBoth()
		{
			RebuildInv();
			RebuildStorage();
		}

		private static string AffixTag(List<ItemAffix> affixes) =>
			affixes != null && affixes.Count > 0 ? $" (옵션{affixes.Count})" : "";

		private void RebuildInv()
		{
			foreach (Node c in _invList.GetChildren()) c.QueueFree();
			if (_inventory == null) return;
			for (int i = 0; i < _inventory.Slots.Count; i++)
			{
				var slot = _inventory.Slots[i];
				if (slot.Item == null) continue;
				// 장착 중인 슬롯은 절대 보관 불가 — 목록에서 제외.
				if (slot.IsEquipped) continue;
				int idx = i;
				string label = Inventory.GetEnhancedName(slot.Item, slot.EnhancementLevel)
					+ (slot.Quantity > 1 ? $" x{slot.Quantity}" : "") + AffixTag(slot.Affixes);
				_invList.AddChild(MakeRow(label, "보관", () => Deposit(idx)));
			}
		}

		private void RebuildStorage()
		{
			foreach (Node c in _storageList.GetChildren()) c.QueueFree();
			var store = GameManager.Instance?.Storage;
			if (store == null) return;
			for (int i = 0; i < store.Count; i++)
			{
				var s = store[i];
				var item = GD.Load<ItemData>(s.ItemPath);
				string nm = item != null ? item.ItemName : s.ItemPath;
				int idx = i;
				string label = (s.EnhancementLevel > 0 ? $"+{s.EnhancementLevel} " : "") + nm
					+ (s.Quantity > 1 ? $" x{s.Quantity}" : "") + AffixTag(s.Affixes);
				_storageList.AddChild(MakeRow(label, "꺼내기", () => Withdraw(idx)));
			}
		}

		private PanelContainer MakeRow(string text, string btnText, System.Action onPressed)
		{
			// 420px 패널 안 2열 구조 — 행 폭은 한 열(약 185px)에 맞춰 오버플로 방지.
			var panel = new PanelContainer { CustomMinimumSize = new Vector2(185, 22) };
			panel.MouseFilter = Control.MouseFilterEnum.Pass;
			var hbox = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Pass };
			panel.AddChild(hbox);
			var lbl = new Label { Text = text, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			lbl.AddThemeFontSizeOverride("font_size", 10);
			lbl.ClipText = true;
			lbl.MouseFilter = Control.MouseFilterEnum.Ignore;
			hbox.AddChild(lbl);
			var btn = new Button { Text = btnText };
			btn.AddThemeFontSizeOverride("font_size", 10);
			btn.MouseFilter = Control.MouseFilterEnum.Pass;
			btn.Pressed += () => onPressed();
			hbox.AddChild(btn);
			return panel;
		}

		// 인벤 → 창고. 슬롯 전체(수량+강화+affix) 이동. 장착 슬롯 거부.
		private void Deposit(int slotIndex)
		{
			if (_inventory == null || slotIndex < 0 || slotIndex >= _inventory.Slots.Count) return;
			var slot = _inventory.Slots[slotIndex];
			if (slot.IsEquipped) { ShowMessage("장착 중인 아이템은 보관할 수 없습니다."); return; }
			if (slot.Item == null || string.IsNullOrEmpty(slot.Item.ResourcePath)) return;

			var saved = new SavedItemSlot
			{
				ItemPath = slot.Item.ResourcePath,
				Quantity = slot.Quantity,
				EnhancementLevel = slot.EnhancementLevel,
				Affixes = slot.Affixes != null ? new List<ItemAffix>(slot.Affixes) : new List<ItemAffix>(),
				IsEquipped = false
			};
			using (GameTransaction.Begin())
			{
				// 반드시 제거 성공 후에만 창고 적재 — 복제/소실 차단.
				if (!_inventory.RemoveItem(slotIndex, slot.Quantity))
				{
					ShowMessage("보관할 수 없습니다.");
					return;
				}
				GameManager.Instance?.AddStorageSlot(saved);
			}
			SaveManager.SaveGame();
			ShowMessage("창고에 보관했습니다.");
			RefreshBoth();
		}

		// 창고 → 인벤. 인벤 공간 부족 시 실패(창고 데이터 보존).
		private void Withdraw(int storageIndex)
		{
			var store = GameManager.Instance?.Storage;
			if (store == null || storageIndex < 0 || storageIndex >= store.Count) return;
			var s = store[storageIndex];
			var item = GD.Load<ItemData>(s.ItemPath);
			if (item == null) { ShowMessage("아이템 로드 실패."); return; }
			if (!_inventory.CanAddItem(item, s.Quantity))
			{
				ShowMessage("가방 공간이 부족합니다.");
				return;
			}
			using (GameTransaction.Begin())
			{
				bool added = _inventory.AddItem(item, s.Quantity, s.EnhancementLevel,
					fireAcquired: false, s.Affixes);
				if (!added)
				{
					ShowMessage("꺼낼 수 없습니다 (공간 부족).");
					return; // 창고 슬롯 미제거 — 데이터 보존
				}
				GameManager.Instance?.RemoveStorageAt(storageIndex);
			}
			SaveManager.SaveGame();
			ShowMessage("창고에서 꺼냈습니다.");
			RefreshBoth();
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
