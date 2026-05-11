using Godot;
using FirstGame.Core;
using FirstGame.Data;

namespace FirstGame.UI
{
	public partial class TeleportUI : BaseUIWindow
	{
		private Label _titleLabel;
		private Label _goldLabel;
		private VBoxContainer _list;
		private Label _messageLabel;
		private Button _closeButton;

		private string _npcName = "텔레포트";
		private TeleportDestinationData[] _destinations = System.Array.Empty<TeleportDestinationData>();

		protected override void OnReadyInternal()
		{
			_titleLabel = GetNode<Label>("%TeleportTitleLabel");
			_goldLabel = GetNode<Label>("%TeleportGoldLabel");
			_list = GetNode<VBoxContainer>("%TeleportList");
			_messageLabel = GetNode<Label>("%TeleportMessageLabel");
			_closeButton = GetNode<Button>("%TeleportCloseButton");

			_closeButton.Pressed += Close;
			_messageLabel.Visible = false;
		}

		public void OpenTeleport(string npcName, TeleportDestinationData[] destinations)
		{
			_npcName = npcName;
			_destinations = destinations ?? System.Array.Empty<TeleportDestinationData>();
			if (Visible) OnOpened();
			else Open();
		}

		protected override void OnOpened()
		{
			_titleLabel.Text = _npcName;
			_messageLabel.Visible = false;
			RefreshGold();
			RefreshList();
		}

		private void RefreshGold()
		{
			int gold = GameManager.Instance?.PlayerGold ?? 0;
			_goldLabel.Text = $"보유 골드: {gold}G";
		}

		private void RefreshList()
		{
			foreach (Node c in _list.GetChildren()) c.QueueFree();

			var gm = GameManager.Instance;
			if (gm == null) return;
			string current = GetTree().CurrentScene.SceneFilePath;

			foreach (var dest in _destinations)
			{
				if (dest == null) continue;
				bool visited = gm.HasVisitedScene(dest.ScenePath);
				bool isCurrent = dest.ScenePath == current;
				bool canAfford = gm.PlayerGold >= dest.Cost;

				var panel = CreateRow(dest, visited, isCurrent, canAfford);
				_list.AddChild(panel);
			}
		}

		private PanelContainer CreateRow(TeleportDestinationData dest, bool visited, bool isCurrent, bool canAfford)
		{
			var panel = new PanelContainer();
			panel.CustomMinimumSize = new Vector2(260, 36);
			panel.MouseFilter = Control.MouseFilterEnum.Pass;

			var hbox = new HBoxContainer();
			hbox.MouseFilter = Control.MouseFilterEnum.Pass;
			panel.AddChild(hbox);

			var nameLabel = new Label
			{
				Text = dest.DisplayName,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			if (isCurrent) nameLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
			else if (!visited) nameLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
			hbox.AddChild(nameLabel);

			var costLabel = new Label
			{
				Text = $"{dest.Cost}G",
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			costLabel.AddThemeColorOverride("font_color",
				canAfford ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 0.4f, 0.4f));
			hbox.AddChild(costLabel);

			var btn = new Button
			{
				Text = isCurrent ? "현재 위치" : !visited ? "미발견" : !canAfford ? "골드 부족" : "이동",
				Disabled = isCurrent || !visited || !canAfford,
				MouseFilter = Control.MouseFilterEnum.Pass,
				CustomMinimumSize = new Vector2(72, 0)
			};
			var captured = dest;
			btn.Pressed += () => OnTeleportPressed(captured);
			hbox.AddChild(btn);

			return panel;
		}

		private void OnTeleportPressed(TeleportDestinationData dest)
		{
			var gm = GameManager.Instance;
			if (gm == null) return;
			if (gm.PlayerGold < dest.Cost)
			{
				ShowMessage("골드가 부족합니다!");
				AudioManager.Instance?.PlaySFX("shop_fail.wav");
				return;
			}
			// 씬 경로를 먼저 검증 — 오타/export 누락 시 골드 차감 자체를 막는다.
			if (!ResourceLoader.Exists(dest.ScenePath))
			{
				ShowMessage("이동할 수 없는 위치입니다.");
				return;
			}
			// 골드를 *먼저* 차감 → ChangeScene이 내부에서 SaveAndSetPending할 때 차감된 골드가
			// 저장에 반영되도록. ChangeScene이 큐잉에 실패하면 SceneManager는 mutation을 하지
			// 않으므로 메모리 골드만 롤백하면 저장 파일은 깨끗히 유지된다.
			gm.PlayerGold -= dest.Cost;
			bool ok = SceneManager.Instance?.ChangeScene(dest.ScenePath, dest.SpawnPosition) ?? false;
			if (!ok)
			{
				gm.PlayerGold += dest.Cost;
				ShowMessage("이동에 실패했습니다.");
				AudioManager.Instance?.PlaySFX("shop_fail.wav");
				return;
			}
			AudioManager.Instance?.PlaySFX("portal_use.wav");
			Close();
		}

		private async void ShowMessage(string text)
		{
			_messageLabel.Text = text;
			_messageLabel.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this)) _messageLabel.Visible = false;
		}

		protected override void OnExitTreeInternal()
		{
			if (_closeButton != null) _closeButton.Pressed -= Close;
		}
	}
}
