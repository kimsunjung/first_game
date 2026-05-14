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

		// MP / 레벨 / EXP (없을 수 있으므로 null-safe)
		private ProgressBar _mpBar;
		private Label _mpLabel;
		private Label _levelLabel;
		private ProgressBar _expBar;
		private Label _levelUpLabel;
		private Label _questLabel;


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

			// 씬 트리 _Ready 순서 보장을 위해 플레이어 바인딩은 지연 실행
			CallDeferred(nameof(BindPlayer));
		}

		private void BindPlayer()
		{
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
			_healthLabel.Text = $"{currentHp} / {maxHp}";
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
			_goldLabel.Text = $"골드: {gold}";
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
