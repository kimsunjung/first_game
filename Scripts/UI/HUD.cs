using Godot;
using System;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	public partial class HUD : CanvasLayer
	{
		private ProgressBar _healthBar;
		private Label _healthLabel;
		private Label _goldLabel;
		private IPlayer _player;

		private Control _gameOverPanel;
		private Button _restartButton;

		private Label _saveNotification;
		private Label _itemPickupNotification;

		// 퀵슬롯 UI — 6슬롯
		private const int QuickSlotCount = 6;
		private TextureRect[] _quickSlotIcons = new TextureRect[QuickSlotCount];
		private Label[] _quickSlotQtys = new Label[QuickSlotCount];

		// MP / 레벨 / EXP / 시간 (없을 수 있으므로 null-safe)
		private ProgressBar _mpBar;
		private Label _mpLabel;
		private Label _levelLabel;
		private Label _timeLabel;
		private ProgressBar _expBar;
		private Label _levelUpLabel;
		private Label _questLabel;

		// 상단 이펙트 바(상태이상+버프 아이콘) + 상세 팝업 + 미니맵 — 모두 코드 생성.
		private HBoxContainer _effectsBar;
		private PanelContainer _effectsPopup;
		private VBoxContainer _effectsPopupList;
		private ColorRect _effectsDismiss;
		private string _effectsPopupSig = "";
		private readonly System.Collections.Generic.Dictionary<string, Button> _effectIcons = new();
		private MinimapView _minimap;

		public override void _Ready()
		{
			_healthBar = GetNode<ProgressBar>("%HealthBar");
			_healthLabel = GetNode<Label>("%HealthLabel");
			_goldLabel = GetNode<Label>("%GoldLabel");

			_gameOverPanel = GetNode<Control>("%GameOverPanel");
			_restartButton = GetNode<Button>("%RestartButton");
			_gameOverPanel.Visible = false;

			_saveNotification = GetNode<Label>("%SaveNotification");
			_saveNotification.Visible = false;

			_itemPickupNotification = GetNode<Label>("%ItemPickupNotification");
			_itemPickupNotification.Visible = false;

			for (int i = 0; i < QuickSlotCount; i++)
			{
				_quickSlotIcons[i] = GetNodeOrNull<TextureRect>($"%QuickSlotIcon{i + 1}");
				_quickSlotQtys[i] = GetNodeOrNull<Label>($"%QuickSlotQty{i + 1}");
			}

			// null-safe: 씬에 노드가 없어도 크래시 없음
			_mpBar = GetNodeOrNull<ProgressBar>("%MpBar");
			_mpLabel = GetNodeOrNull<Label>("%MpLabel");
			_levelLabel = GetNodeOrNull<Label>("%LevelLabel");
			_timeLabel = GetNodeOrNull<Label>("%TimeLabel");
			if (_timeLabel != null)
			{
				_timeLabel.Text = DayNightCycle.FormatTime();
				DayNightCycle.OnTimeChanged += UpdateTimeDisplay;
			}
			_expBar = GetNodeOrNull<ProgressBar>("%ExpBar");
			_levelUpLabel = GetNodeOrNull<Label>("%LevelUpLabel");
			if (_levelUpLabel != null) _levelUpLabel.Visible = false;
			_questLabel = GetNodeOrNull<Label>("%QuestLabel");

			_restartButton.Pressed += OnRestartPressed;
			EventManager.OnPlayerDeath += ShowGameOver;
			EventManager.OnLevelUp += ShowLevelUp;
			SaveManager.OnGameSaved += ShowSaveNotification;
			if (GameManager.Instance != null)
			{
				GameManager.Instance.QuestManager.OnQuestStateChanged += UpdateQuestDisplay;
				GameManager.Instance.QuestManager.OnRewardBlocked += OnQuestRewardBlocked;
				GameManager.Instance.OnPendingRewardAdded += OnPendingRewardAdded;
				GameManager.Instance.OnPendingRewardClaimed += OnPendingRewardClaimed;
				GameManager.Instance.OnChapterAdvanced += OnChapterAdvanced;
			}
			UpdateQuestDisplay();

			if (GameManager.Instance != null)
			{
				GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
				UpdateGoldDisplay(GameManager.Instance.PlayerGold);
			}

			// 상단 중앙 이펙트 바(상태이상+버프 아이콘). 클릭 시 상세 팝업 토글.
			_effectsBar = new HBoxContainer { Name = "EffectsBar" };
			_effectsBar.AddThemeConstantOverride("separation", 4);
			_effectsBar.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
			_effectsBar.AnchorLeft = 0.5f; _effectsBar.AnchorRight = 0.5f;
			_effectsBar.GrowHorizontal = Control.GrowDirection.Both;
			_effectsBar.OffsetTop = 4;
			AddChild(_effectsBar);

			// 팝업 바깥 탭 시 닫기용 전체화면 투명 캡처(팝업보다 먼저 추가 → 팝업이 위에 그려짐).
			_effectsDismiss = new ColorRect
			{
				Name = "EffectsDismiss",
				Color = new Color(0, 0, 0, 0),
				Visible = false,
				MouseFilter = Control.MouseFilterEnum.Stop,
			};
			_effectsDismiss.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			_effectsDismiss.GuiInput += ev =>
			{
				bool tap = (ev is InputEventMouseButton mb && mb.Pressed)
					|| (ev is InputEventScreenTouch st && st.Pressed);
				if (tap) { SetEffectsPopupVisible(false); _effectsDismiss.AcceptEvent(); }
			};
			AddChild(_effectsDismiss);

			// 상세 팝업 — 활성 효과 목록 + 남은시간. 기본 숨김.
			_effectsPopup = new PanelContainer { Name = "EffectsPopup", Visible = false };
			_effectsPopup.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
			_effectsPopup.AnchorLeft = 0.5f; _effectsPopup.AnchorRight = 0.5f;
			_effectsPopup.GrowHorizontal = Control.GrowDirection.Both;
			_effectsPopup.OffsetTop = 30; _effectsPopup.CustomMinimumSize = new Vector2(190, 0);
			var pst = new StyleBoxFlat { BgColor = new Color(0.08f, 0.08f, 0.10f, 0.94f) };
			pst.SetBorderWidthAll(1); pst.SetCornerRadiusAll(4); pst.SetContentMarginAll(6);
			pst.BorderColor = new Color(0.5f, 0.5f, 0.6f, 0.9f);
			_effectsPopup.AddThemeStyleboxOverride("panel", pst);
			var pv = new VBoxContainer();
			pv.AddThemeConstantOverride("separation", 3);
			_effectsPopup.AddChild(pv);
			var ph = new Label { Text = "활성 효과" };
			ph.AddThemeFontSizeOverride("font_size", 11);
			ph.AddThemeColorOverride("font_color", new Color(0.8f, 0.9f, 1f));
			pv.AddChild(ph);
			_effectsPopupList = new VBoxContainer();
			_effectsPopupList.AddThemeConstantOverride("separation", 2);
			pv.AddChild(_effectsPopupList);
			AddChild(_effectsPopup);

			// 좌측하단 현재 맵 이름 — 코드 생성(씬마다 hud.tscn 인스턴스라 1곳만 수정).
			var mapName = new Label
			{
				Name = "MapNameLabel",
				Text = FirstGame.Data.MapNames.Get(GetTree().CurrentScene?.SceneFilePath),
				MouseFilter = Control.MouseFilterEnum.Ignore,
			};
			mapName.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
			mapName.AnchorTop = 1; mapName.AnchorBottom = 1;
			mapName.OffsetLeft = 12; mapName.OffsetBottom = -8; mapName.OffsetTop = -30;
			mapName.AddThemeFontSizeOverride("font_size", 13);
			mapName.AddThemeColorOverride("font_color", new Color(0.95f, 0.92f, 0.7f));
			mapName.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 1));
			mapName.AddThemeConstantOverride("outline_size", 4);
			AddChild(mapName);

			// 우측상단 미니맵 — 코드 생성. 맵 경계 안에 플레이어/적/포탈 점 표시.
			_minimap = new MinimapView { Name = "Minimap" };
			_minimap.SetAnchorsPreset(Control.LayoutPreset.TopRight);
			_minimap.AnchorLeft = 1; _minimap.AnchorRight = 1;
			_minimap.GrowHorizontal = Control.GrowDirection.Begin;
			_minimap.CustomMinimumSize = new Vector2(150, 90);
			_minimap.Size = new Vector2(150, 90);
			_minimap.OffsetLeft = -158; _minimap.OffsetTop = 8;
			_minimap.OffsetRight = -8; _minimap.OffsetBottom = 98;
			AddChild(_minimap);

			// 씬 트리 _Ready 순서 보장을 위해 플레이어 바인딩은 지연 실행
			CallDeferred(nameof(BindPlayer));
		}

		// 이펙트 바/미니맵은 매 프레임 갱신할 필요가 없다 — throttle해 전투 중 리스트/노드
		// 재생성 GC hitch를 줄인다. 0.08s(~12.5Hz): 깜빡임이 거칠지 않고, 0.08s 미만으로만
		// 지속되는 극단적 단발 상태가 아니면 칩이 최소 1회는 그려진다(가시성 보장).
		private double _hudRefreshAccum;
		private const double HudRefreshInterval = 0.08;

		public override void _Process(double delta)
		{
			_hudRefreshAccum += delta;
			if (_hudRefreshAccum < HudRefreshInterval) return;
			_hudRefreshAccum = 0;
			RefreshEffects();
			_minimap?.Refresh();
		}

		// 매 프레임 상태이상+버프를 상단 아이콘으로 표시. 클릭 시 팝업. 남은시간 짧으면 깜빡임.
		private const float BlinkThreshold = 3.0f;
		private void RefreshEffects()
		{
			if (_effectsBar == null || _player == null) return;
			if (_player is GodotObject pgo && !IsInstanceValid(pgo)) { _player = null; return; }
			if (_player.Stats == null) return;

			var active = new System.Collections.Generic.List<(string id, string name, float remain, Color col, Texture2D tex)>();
			foreach (var (kind, _, remain) in _player.Stats.GetActiveStatusBars())
			{
				var c = FirstGame.Data.CharacterStats.StatusColor(kind);
				active.Add(("st_" + kind, StatusName(kind), remain, c, LoadStatusIcon(kind)));
			}
			foreach (var (id, name, remain) in _player.Stats.GetActiveBuffs())
			{
				active.Add(("bf_" + id, name, remain, new Color(0.4f, 0.85f, 1f), LoadBuffIcon(id)));
			}

			float blink = 0.45f + 0.55f * Mathf.Abs(Mathf.Sin((float)Time.GetTicksMsec() / 140f));
			var present = new System.Collections.Generic.HashSet<string>();
			foreach (var e in active)
			{
				present.Add(e.id);
				if (!_effectIcons.TryGetValue(e.id, out var btn) || !IsInstanceValid(btn))
				{
					btn = new Button { Flat = true, FocusMode = Control.FocusModeEnum.None };
					btn.CustomMinimumSize = new Vector2(22, 22);
					btn.Pressed += ToggleEffectsPopup;
					if (e.tex != null)
					{
						btn.Icon = e.tex;
						btn.ExpandIcon = true;
					}
					else
					{
						var sb = new StyleBoxFlat { BgColor = e.col };
						sb.SetCornerRadiusAll(3);
						btn.AddThemeStyleboxOverride("normal", sb);
						btn.AddThemeStyleboxOverride("hover", sb);
						btn.AddThemeStyleboxOverride("pressed", sb);
					}
					_effectsBar.AddChild(btn);
					_effectIcons[e.id] = btn;
				}
				btn.TooltipText = $"{e.name}  {e.remain:0.0}s";
				btn.Modulate = e.remain < BlinkThreshold ? new Color(1, 1, 1, blink) : Colors.White;
				btn.Visible = true;
			}
			foreach (var kv in new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, Button>>(_effectIcons))
			{
				if (!present.Contains(kv.Key))
				{
					if (IsInstanceValid(kv.Value)) kv.Value.QueueFree();
					_effectIcons.Remove(kv.Key);
				}
			}

			if (_effectsPopup != null && _effectsPopup.Visible)
			{
				if (active.Count == 0) { SetEffectsPopupVisible(false); }
				else
				{
					// 활성 집합(이름·개수)이 바뀔 때만 행 재생성. 그 외에는 기존
					// Label 의 텍스트/모듈레이트만 갱신(80ms마다 QueueFree+재생성 회피).
					string sig = string.Join("|", active.ConvertAll(e => e.name));
					if (sig != _effectsPopupSig)
					{
						_effectsPopupSig = sig;
						foreach (Node c in _effectsPopupList.GetChildren()) c.QueueFree();
						foreach (var e in active)
						{
							var row = new Label();
							row.AddThemeFontSizeOverride("font_size", 10);
							_effectsPopupList.AddChild(row);
						}
					}
					var rows = _effectsPopupList.GetChildren();
					for (int i = 0; i < active.Count && i < rows.Count; i++)
					{
						if (rows[i] is not Label row) continue;
						var e = active[i];
						row.Text = $"{e.name}  —  {e.remain:0.0}s";
						row.Modulate = e.remain < BlinkThreshold
							? new Color(1f, 0.5f, 0.4f, blink) : Colors.White;
					}
				}
			}
		}

		private void ToggleEffectsPopup()
			=> SetEffectsPopupVisible(_effectsPopup != null && !_effectsPopup.Visible);

		private void SetEffectsPopupVisible(bool visible)
		{
			if (_effectsPopup != null) _effectsPopup.Visible = visible;
			if (_effectsDismiss != null) _effectsDismiss.Visible = visible;
			if (!visible) _effectsPopupSig = ""; // 다음 오픈 시 강제 재구성
		}

		private static string StatusName(FirstGame.Data.StatusEffect k) => k switch
		{
			FirstGame.Data.StatusEffect.Poison => "중독",
			FirstGame.Data.StatusEffect.Freeze => "빙결",
			FirstGame.Data.StatusEffect.Burn => "화상",
			FirstGame.Data.StatusEffect.Shock => "감전",
			FirstGame.Data.StatusEffect.Curse => "저주",
			_ => k.ToString(),
		};

		private static readonly System.Collections.Generic.Dictionary<string, string> _buffIconPaths = new()
		{
			{ "dmg", "res://Resources/Generated/GPT/Icons/Status/attack_up.png" },
			{ "def", "res://Resources/Generated/GPT/Icons/Status/defense_up.png" },
		};

		private static Texture2D LoadBuffIcon(string id)
		{
			if (!_buffIconPaths.TryGetValue(id, out var path)) return null;
			return ResourceLoader.Exists(path) ? GD.Load<Texture2D>(path) : null;
		}

		private static readonly System.Collections.Generic.Dictionary<FirstGame.Data.StatusEffect, string> _statusIconPaths = new()
		{
			{ FirstGame.Data.StatusEffect.Poison, "res://Resources/Generated/GPT/Icons/Status/poison.png" },
			{ FirstGame.Data.StatusEffect.Freeze, "res://Resources/Generated/GPT/Icons/Status/freeze.png" },
			{ FirstGame.Data.StatusEffect.Burn,   "res://Resources/Generated/GPT/Icons/Status/burn.png" },
			{ FirstGame.Data.StatusEffect.Shock,  "res://Resources/Generated/GPT/Icons/Status/shock.png" },
		};

		// 상태 전용 아이콘 텍스처. 매핑/리소스 없으면 null → 호출부가 색상 바로 폴백.
		private static Texture2D LoadStatusIcon(FirstGame.Data.StatusEffect kind)
		{
			if (!_statusIconPaths.TryGetValue(kind, out var path)) return null;
			return ResourceLoader.Exists(path) ? GD.Load<Texture2D>(path) : null;
		}

		private void BindPlayer()
		{
			// CallDeferred로 지연 실행 — 그 사이 빠른 씬 전환으로 이 HUD가 이미
			// 해제됐을 수 있다. freed HUD 메서드를 살아있는 Stats에 구독하면
			// 콜백 시 ObjectDisposedException. 유효성 먼저 확인.
			if (!IsInstanceValid(this) || !IsInsideTree()) return;
			var player = GameManager.Instance?.Player;
			if (player == null) return;

			_player = player;
			_player.Stats.OnHealthChanged += UpdateHealthDisplay;
			_player.Stats.OnMpChanged += UpdateMpDisplay;
			_player.Stats.OnLevelUp += UpdateLevelDisplay;
			_player.Stats.OnExpChanged += UpdateExpDisplay;
			_player.Inventory.OnItemPickedUp += ShowItemPickup;
			_player.Inventory.OnQuickSlotChanged += UpdateQuickSlotDisplay;
			_player.Inventory.OnInventoryChanged += UpdateQuickSlotDisplay;

			UpdateHealthDisplay(_player.Stats.CurrentHealth, _player.Stats.MaxHealth);
			UpdateMpDisplay(_player.Stats.CurrentMp, _player.Stats.MaxMp);
			UpdateLevelDisplay(_player.Stats.Level);
			UpdateExpDisplay(_player.Stats.Exp, _player.Stats.ExpToNextLevel);
			UpdateQuickSlotDisplay();
		}

		private void UpdateTimeDisplay()
		{
			if (_timeLabel != null) _timeLabel.Text = DayNightCycle.FormatTime();
		}

		public override void _ExitTree()
		{
			if (GameManager.Instance != null)
				GameManager.Instance.OnGoldChanged -= UpdateGoldDisplay;

			if (_player is GodotObject playerObj && IsInstanceValid(playerObj))
			{
				_player.Stats.OnHealthChanged -= UpdateHealthDisplay;
				_player.Stats.OnMpChanged -= UpdateMpDisplay;
				_player.Stats.OnLevelUp -= UpdateLevelDisplay;
				_player.Stats.OnExpChanged -= UpdateExpDisplay;
				if (_player.Inventory != null)
				{
					_player.Inventory.OnItemPickedUp -= ShowItemPickup;
					_player.Inventory.OnQuickSlotChanged -= UpdateQuickSlotDisplay;
					_player.Inventory.OnInventoryChanged -= UpdateQuickSlotDisplay;
				}
			}

			EventManager.OnPlayerDeath -= ShowGameOver;
			EventManager.OnLevelUp -= ShowLevelUp;
			SaveManager.OnGameSaved -= ShowSaveNotification;
			DayNightCycle.OnTimeChanged -= UpdateTimeDisplay;
			if (GameManager.Instance != null)
			{
				GameManager.Instance.QuestManager.OnQuestStateChanged -= UpdateQuestDisplay;
				GameManager.Instance.QuestManager.OnRewardBlocked -= OnQuestRewardBlocked;
				GameManager.Instance.OnPendingRewardAdded -= OnPendingRewardAdded;
				GameManager.Instance.OnPendingRewardClaimed -= OnPendingRewardClaimed;
				GameManager.Instance.OnChapterAdvanced -= OnChapterAdvanced;
			}
		}

		// 챕터 진행 시 호출. Ending 진입 시 엔딩 시퀀스 자동 표시.
		private void OnChapterAdvanced(Chapter chapter)
		{
			if (chapter == Chapter.Ending) ShowEndingSequence();
		}

		private async void ShowEndingSequence()
		{
			// 검은 fade-in 패널 + 엔딩 텍스트 + 크레딧. UIPauseManager로 게임 정지.
			UIPauseManager.RequestPause();

			var panel = new ColorRect
			{
				Color = new Color(0, 0, 0, 0),
				// PRESET_FULL_RECT: 풀스크린 입력 영역 확보 — Anchor만으론 Size가 0이라
				// GuiInput 영역이 없어 탭이 안 먹는 결함 차단.
				AnchorLeft = 0, AnchorTop = 0,
				AnchorRight = 1, AnchorBottom = 1,
				OffsetLeft = 0, OffsetTop = 0,
				OffsetRight = 0, OffsetBottom = 0,
				GrowHorizontal = Control.GrowDirection.Both,
				GrowVertical = Control.GrowDirection.Both,
				MouseFilter = Control.MouseFilterEnum.Stop,
				ProcessMode = ProcessModeEnum.Always
			};
			AddChild(panel);

			var fadeTween = panel.CreateTween();
			fadeTween.SetProcessMode(Tween.TweenProcessMode.Idle);
			fadeTween.TweenProperty(panel, "color:a", 1f, 1.5);
			await ToSignal(fadeTween, Tween.SignalName.Finished);

			if (!IsInstanceValid(this)) return;

			var label = new Label
			{
				Text = "— 돌아온 영웅 —\n\n" +
					   "어둠은 다시 봉인되었다.\n" +
					   "새벽이 마을에 돌아오고,\n" +
					   "사람들은 영웅의 이름을 부른다.\n\n" +
					   "카엘.\n" +
					   "그의 이야기는 다음 천 년에 새겨질 것이다.\n\n\n" +
					   "— THE END —",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				AnchorRight = 1, AnchorBottom = 1,
				ProcessMode = ProcessModeEnum.Always,
				Modulate = new Color(1, 1, 1, 0)
			};
			label.AddThemeFontSizeOverride("font_size", 22);
			label.AddThemeColorOverride("font_color", new Color(1f, 0.95f, 0.8f));
			panel.AddChild(label);

			var textTween = label.CreateTween();
			textTween.SetProcessMode(Tween.TweenProcessMode.Idle);
			textTween.TweenProperty(label, "modulate:a", 1f, 1.5);
			await ToSignal(textTween, Tween.SignalName.Finished);

			if (!IsInstanceValid(this)) return;

			var hint = new Label
			{
				Text = "[탭하여 계속 — 자유 탐험 모드]",
				HorizontalAlignment = HorizontalAlignment.Center,
				AnchorLeft = 0, AnchorRight = 1,
				AnchorTop = 0.85f, AnchorBottom = 0.95f,
				ProcessMode = ProcessModeEnum.Always
			};
			hint.AddThemeFontSizeOverride("font_size", 14);
			hint.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
			panel.AddChild(hint);

			panel.GuiInput += (ev) =>
			{
				if (ev is InputEventMouseButton mb && mb.Pressed && IsInstanceValid(panel))
				{
					UIPauseManager.Reset();
					panel.QueueFree();
				}
				if (ev is InputEventScreenTouch st && st.Pressed && IsInstanceValid(panel))
				{
					UIPauseManager.Reset();
					panel.QueueFree();
				}
			};
		}

		private async void OnPendingRewardAdded(ItemData item, int qty)
		{
			_itemPickupNotification.Text = $"가방 가득 — {item.ItemName} x{qty} 보류함 보관";
			_itemPickupNotification.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.5), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this))
				_itemPickupNotification.Visible = false;
		}

		private async void OnPendingRewardClaimed(ItemData item, int qty)
		{
			_itemPickupNotification.Text = $"보류함 → {item.ItemName} x{qty} 지급";
			_itemPickupNotification.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this))
				_itemPickupNotification.Visible = false;
		}

		private async void OnQuestRewardBlocked(Data.QuestData quest)
		{
			_itemPickupNotification.Text = $"가방 공간 부족 — {quest.QuestTitle} 보류";
			_itemPickupNotification.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.5), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this))
				_itemPickupNotification.Visible = false;
		}

		private void UpdateQuestDisplay()
		{
			if (_questLabel == null) return;
			var qm = GameManager.Instance?.QuestManager;
			if (qm == null || !qm.HasActiveQuest)
			{
				_questLabel.Visible = false;
				return;
			}
			var q = qm.ActiveQuest;
			string objective = q.Type switch
			{
				QuestType.Kill => $"{q.TargetEnemyType} 처치",
				QuestType.Gather => $"{q.TargetItem?.ItemName ?? "?"} 수집",
				QuestType.Deliver => $"{q.TargetNpcId} 만나기",
				QuestType.Explore => $"{q.TargetScene} 도달",
				_ => ""
			};
			_questLabel.Text = $"[퀘스트] {q.QuestTitle} — {objective} {qm.Progress}/{q.TargetCount}";
			_questLabel.Visible = true;
		}

		private void ShowGameOver()
		{
			AudioManager.Instance?.PlaySFX("player_death.wav");
			_gameOverPanel.Visible = true;
			UIPauseManager.RequestPause();
		}

		private async void ShowSaveNotification()
		{
			_saveNotification.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this))
				_saveNotification.Visible = false;
		}

		private async void ShowItemPickup(ItemData item)
		{
			_itemPickupNotification.Text = $"획득: {item.ItemName}";
			_itemPickupNotification.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this))
				_itemPickupNotification.Visible = false;
		}

		// NPC 대사 토스트 — 챕터별 한 줄 대사를 화면 상단 중앙에 잠시 표시.
		// 호출당 새 라벨을 동적 생성하고 2.5초 + 0.6초 fade out 후 자동 정리.
		private Label _npcDialogueLabel;
		public void ShowNpcDialogue(string npcId, string line)
		{
			if (string.IsNullOrEmpty(line)) return;
			// 이전 라벨을 즉시 트리에서 제거 — QueueFree는 deferred라 같은 프레임 공존으로
			// 깜빡임 발생. RemoveChild로 즉시 분리한 뒤 QueueFree로 메모리 정리.
			if (_npcDialogueLabel != null && IsInstanceValid(_npcDialogueLabel))
			{
				var oldParent = _npcDialogueLabel.GetParent();
				if (oldParent != null) oldParent.RemoveChild(_npcDialogueLabel);
				_npcDialogueLabel.QueueFree();
				_npcDialogueLabel = null;
			}

			// 전용 CanvasLayer(layer=100) — ShopUI/InventoryUI 등 다른 CanvasLayer 위에 출력.
			// 그렇지 않으면 OnInteract가 즉시 fullscreen 본 UI를 열 때 토스트가 가려져 안 보임.
			var scene = GetTree()?.CurrentScene;
			if (scene == null) return;
			var dialogueLayer = scene.GetNodeOrNull<CanvasLayer>("__NpcDialogueLayer");
			if (dialogueLayer == null)
			{
				dialogueLayer = new CanvasLayer { Name = "__NpcDialogueLayer", Layer = 100 };
				dialogueLayer.ProcessMode = ProcessModeEnum.Always;
				scene.AddChild(dialogueLayer);
			}

			var label = new Label
			{
				Text = $"[{NpcLabel(npcId)}] {line}",
				HorizontalAlignment = HorizontalAlignment.Center,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				AnchorLeft = 0.5f, AnchorRight = 0.5f,
				AnchorTop = 0.0f,  AnchorBottom = 0.0f,
				OffsetLeft = -240, OffsetRight = 240,
				OffsetTop = 12,    OffsetBottom = 60,
				GrowHorizontal = Control.GrowDirection.Both,
				Modulate = new Color(1f, 0.95f, 0.7f, 1f),
				ProcessMode = ProcessModeEnum.Always
			};
			label.AddThemeFontSizeOverride("font_size", 14);
			label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.9f));
			label.AddThemeConstantOverride("outline_size", 4);
			dialogueLayer.AddChild(label);
			_npcDialogueLabel = label;

			var tween = label.CreateTween();
			tween.TweenInterval(2.5);
			tween.TweenProperty(label, "modulate:a", 0f, 0.6);
			tween.TweenCallback(Callable.From(() =>
			{
				if (Godot.GodotObject.IsInstanceValid(label)) label.QueueFree();
			}));
		}

		private static string NpcLabel(string npcId) => npcId switch
		{
			"save_point" => "노현자",
			"shop" => "상점 주인",
			"blacksmith" => "대장장이",
			"skill_shop" => "스킬 상인",
			"material_shop" => "재료 상인",
			"teleport" => "텔레포트",
			_ => "주민"
		};

		private async void ShowLevelUp(int newLevel)
		{
			if (_levelUpLabel == null) return;
			_levelUpLabel.Text = $"LEVEL UP! Lv.{newLevel}";
			_levelUpLabel.Visible = true;
			await ToSignal(GetTree().CreateTimer(3.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this) && _levelUpLabel != null)
				_levelUpLabel.Visible = false;
		}

		private void OnRestartPressed()
		{
			_gameOverPanel.Visible = false;
			UIPauseManager.Reset();
			SaveManager.LoadGame();
		}

		private void UpdateHealthDisplay(int currentHp, int maxHp)
		{
			_healthBar.MaxValue = maxHp;
			_healthBar.Value = currentHp;
			_healthLabel.Text = $"{currentHp}/{maxHp}";
		}

		private void UpdateMpDisplay(int currentMp, int maxMp)
		{
			if (_mpBar == null) return;
			_mpBar.MaxValue = maxMp;
			_mpBar.Value = currentMp;
			if (_mpLabel != null)
				_mpLabel.Text = $"{currentMp}/{maxMp}";
		}

		private void UpdateLevelDisplay(int level)
		{
			if (_levelLabel == null) return;
			_levelLabel.Text = $"Lv.{level}";
		}

		private void UpdateExpDisplay(int exp, int expToNext)
		{
			if (_expBar == null) return;
			_expBar.MaxValue = expToNext;
			_expBar.Value = exp;
		}

		private void UpdateGoldDisplay(int gold)
		{
			_goldLabel.Text = $"{gold}G";
		}

		private void UpdateQuickSlotDisplay()
		{
			if (_player == null) return;

			for (int i = 0; i < QuickSlotCount; i++)
			{
				if (_quickSlotIcons[i] == null) continue;
				var item = i < _player.Inventory.QuickSlots.Length ? _player.Inventory.QuickSlots[i] : null;
				if (item != null)
				{
					_quickSlotIcons[i].Texture = item.Icon;
					var slot = _player.Inventory.Slots.Find(s => s.Item.ResourcePath == item.ResourcePath);
					if (_quickSlotQtys[i] != null)
						_quickSlotQtys[i].Text = slot != null ? $"x{slot.Quantity}" : "x0";
				}
				else
				{
					_quickSlotIcons[i].Texture = null;
					if (_quickSlotQtys[i] != null) _quickSlotQtys[i].Text = "";
				}
			}
		}
	}
}
