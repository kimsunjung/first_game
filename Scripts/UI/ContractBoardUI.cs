using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	// 사냥 계약 보드 — 권역별 수락 가능 계약 + 진행 중 계약(완료/포기).
	// 메인 QuestBoardUI와 별개. 동시 3개 수락, 완료 후 재수락 가능.
	// 보상/완료는 HuntingContractManager가 GameTransaction+SaveGame으로 안전 처리.
	public partial class ContractBoardUI : BaseUIWindow
	{
		private Label _titleLabel;
		private Label _statusLabel;
		private VBoxContainer _list;
		private Label _messageLabel;
		private Button _closeButton;

		private IPlayer _player;
		private string _region = "town_region";

		protected override void OnReadyInternal()
		{
			_titleLabel = GetNode<Label>("%ContractTitleLabel");
			_statusLabel = GetNode<Label>("%ContractStatusLabel");
			_list = GetNode<VBoxContainer>("%ContractList");
			_messageLabel = GetNode<Label>("%ContractMessageLabel");
			_closeButton = GetNode<Button>("%ContractCloseButton");
			_closeButton.Pressed += Close;
			_messageLabel.Visible = false;
		}

		public void OpenBoard(string region, string title)
		{
			var player = GameManager.Instance?.Player;
			if (player == null) return;
			_player = player;
			if (!string.IsNullOrEmpty(region)) _region = region;
			ContractsData.EnsureLoaded();
			if (_titleLabel != null && !string.IsNullOrEmpty(title)) _titleLabel.Text = title;
			if (Visible) OnOpened();
			else Open();
		}

		protected override void OnOpened() => Rebuild();

		protected override void OnExitTreeInternal()
		{
			if (_closeButton != null) _closeButton.Pressed -= Close;
		}

		private HuntingContractManager Mgr => GameManager.Instance?.ContractManager;

		private static string TypeLabel(ContractData c) => c.Type switch
		{
			ContractType.Kill => $"처치: {c.TargetEnemyType}",
			ContractType.Gather => "납품(완료 시 소모)",
			ContractType.BossKill => $"보스: {c.TargetBossId}",
			ContractType.Mining => "채광",
			_ => ""
		};

		private static string RewardLabel(ContractData c)
		{
			string s = $"{c.GoldReward}G  EXP +{c.ExpReward}";
			if (!string.IsNullOrEmpty(c.RewardItemPath) && c.RewardItemQuantity > 0)
			{
				var it = GD.Load<ItemData>(c.RewardItemPath);
				if (it != null) s += $"  +{it.ItemName} x{c.RewardItemQuantity}";
			}
			return s;
		}

		private void Rebuild()
		{
			if (_list == null) return;
			foreach (Node ch in _list.GetChildren()) ch.QueueFree();
			var mgr = Mgr;
			if (mgr == null) return;

			// 보드를 열 때마다 현재 인벤 기준으로 납품형 Gather 진행을 재계산.
			// 재료를 창고/제작/상점으로 빼면 TurnInReady가 내려가 stale "완료" 버튼이
			// 남지 않는다(완료 가능 여부와 화면 표시가 항상 일치).
			mgr.RecomputeGatherProgress();

			// ── 진행 중 ──
			AddHeader($"진행 중 ({mgr.Active.Count}/{HuntingContractManager.MaxActive})");
			if (mgr.Active.Count == 0)
				AddInfo("진행 중인 계약이 없습니다.");
			foreach (var prog in mgr.Active)
			{
				var def = ContractsData.Find(prog.ContractId);
				if (def == null) continue;
				bool ready = prog.TurnInReady;
				AddRow(
					$"{def.Title}  [{prog.Progress}/{def.Goal}]",
					$"{TypeLabel(def)}\n보상: {RewardLabel(def)}",
					ready ? "완료" : "포기",
					ready ? new Color(0.4f, 1f, 0.5f) : new Color(0.9f, 0.6f, 0.5f),
					() => { if (ready) OnComplete(def); else OnAbandon(def); });
			}

			// ── 수락 가능 ──
			AddHeader("수락 가능");
			var avail = mgr.AvailableForRegion(_region);
			if (avail.Count == 0)
				AddInfo("이 권역에서 수락 가능한 계약이 없습니다.");
			bool full = mgr.Active.Count >= HuntingContractManager.MaxActive;
			foreach (var c in avail)
			{
				AddRow(
					$"{c.Title}  (Lv.{c.RecommendedLevel}~)",
					$"{c.Description}\n{TypeLabel(c)}  목표 {c.Goal}\n보상: {RewardLabel(c)}",
					full ? "가득참" : "수락",
					new Color(1f, 0.85f, 0.4f),
					full ? (System.Action)null : () => OnAccept(c));
			}

			if (_statusLabel != null)
				_statusLabel.Text = full
					? "계약 한도(3개) 도달 — 완료/포기 후 수락 가능"
					: $"동시 최대 {HuntingContractManager.MaxActive}개까지 수락";
		}

		private void AddHeader(string text)
		{
			var l = new Label { Text = text };
			l.AddThemeFontSizeOverride("font_size", 12);
			l.AddThemeColorOverride("font_color", new Color(0.7f, 0.85f, 1f));
			_list.AddChild(l);
		}

		private void AddInfo(string text)
		{
			var l = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.Word };
			l.AddThemeFontSizeOverride("font_size", 9);
			l.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
			l.CustomMinimumSize = new Vector2(390, 0);
			_list.AddChild(l);
		}

		private void AddRow(string title, string body, string btnText, Color titleColor, System.Action onPressed)
		{
			var panel = new PanelContainer { CustomMinimumSize = new Vector2(395, 0) };
			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.12f, 0.12f, 0.15f, 0.95f),
				BorderColor = new Color(0.4f, 0.38f, 0.22f, 0.9f),
			};
			style.SetBorderWidthAll(1);
			style.SetCornerRadiusAll(4);
			style.SetContentMarginAll(5);
			panel.AddThemeStyleboxOverride("panel", style);

			var hbox = new HBoxContainer();
			hbox.AddThemeConstantOverride("separation", 8);
			panel.AddChild(hbox);

			var vbox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			hbox.AddChild(vbox);

			var t = new Label { Text = title, AutowrapMode = TextServer.AutowrapMode.Word };
			t.AddThemeFontSizeOverride("font_size", 11);
			t.AddThemeColorOverride("font_color", titleColor);
			t.CustomMinimumSize = new Vector2(285, 0);
			vbox.AddChild(t);

			var b = new Label { Text = body, AutowrapMode = TextServer.AutowrapMode.Word };
			b.AddThemeFontSizeOverride("font_size", 8);
			b.AddThemeColorOverride("font_color", new Color(0.82f, 0.82f, 0.82f));
			b.CustomMinimumSize = new Vector2(285, 0);
			vbox.AddChild(b);

			var btn = new Button
			{
				Text = btnText,
				CustomMinimumSize = new Vector2(64, 0),
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
				Disabled = onPressed == null
			};
			btn.AddThemeFontSizeOverride("font_size", 10);
			if (onPressed != null) btn.Pressed += () => onPressed();
			hbox.AddChild(btn);

			_list.AddChild(panel);
		}

		private void OnAccept(ContractData c)
		{
			if (Mgr?.Accept(c.Id) == true)
			{
				ShowMessage($"계약 수락: {c.Title}");
				Rebuild();
			}
			else ShowMessage("수락할 수 없습니다 (한도 초과 또는 이미 진행 중).");
		}

		private void OnAbandon(ContractData c)
		{
			if (Mgr?.Abandon(c.Id) == true)
			{
				ShowMessage($"계약 포기: {c.Title}");
				Rebuild();
			}
		}

		private void OnComplete(ContractData c)
		{
			var mgr = Mgr;
			if (mgr != null && mgr.Complete(c.Id, _player, out bool rewardDeferred))
			{
				AudioManager.Instance?.PlaySFX("craft_success.wav");
				ShowMessage(rewardDeferred
					? $"계약 완료! 가방이 가득 차 보상 아이템은 보관함에 보류됨: {c.Title}"
					: $"계약 완료! 보상을 지급했습니다: {c.Title}");
				Rebuild();
			}
			else ShowMessage(c.Type == ContractType.Gather
				? "완료할 수 없습니다 (납품 재료가 부족합니다)."
				: "완료할 수 없습니다.");
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
