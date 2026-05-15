using Godot;
using System;
using System.Collections.Generic;

namespace FirstGame.Entities
{
	/// <summary>
	/// 플레이어 접근 시 프롬프트(아이콘+텍스트)를 표시하고 상호작용을 처리하는 Area2D 베이스 클래스.
	/// 상속 후 OnInteract()만 구현하면 됩니다.
	/// 씬에 "PromptLabel" Label을 두면 텍스트 표시, "PromptIcon" TextureRect를 두거나 PromptIcon Export를
	/// 설정하면 아이콘도 함께 표시됩니다.
	/// </summary>
	public abstract partial class BaseInteractable : Area2D
	{
		[Export] public Texture2D PromptIcon { get; set; }

		/// <summary>이 NPC의 식별자. 퀘스트 부여/완료 매칭에 사용. 비워두면 퀘스트 미적용.</summary>
		[Export] public string NpcId { get; set; } = "";

		protected bool PlayerInRange { get; private set; } = false;
		private Label _promptLabel;
		private TextureRect _promptIconRect;

		/// <summary>플레이어 근처의 현재 활성 인터랙터블. UI(모바일 버튼 등)에서 사용.</summary>
		public static BaseInteractable Current { get; private set; }
		public static event Action<BaseInteractable> CurrentChanged;

		// 영역이 겹치는 NPC들 사이에서 빠져나갈 때 여전히 in-range인 인터랙터블을 fallback으로 사용
		private static readonly List<BaseInteractable> _inRange = new();

		public override void _Ready()
		{
			BodyEntered += OnBodyEntered;
			BodyExited += OnBodyExited;

			_promptLabel = GetNodeOrNull<Label>("PromptLabel");
			if (_promptLabel != null)
			{
				if (string.IsNullOrEmpty(_promptLabel.Text))
					_promptLabel.Text = "[F] 상호작용";
				_promptLabel.Visible = false;
				// 프롬프트 위치를 NPC 아래로(요청: "조금 아래로"). 어두운 배경 + 작은 폰트로 가독성.
				_promptLabel.OffsetTop = 18;
				_promptLabel.OffsetBottom = 34;
				_promptLabel.AddThemeFontSizeOverride("font_size", 9);
				_promptLabel.AddThemeColorOverride("font_color", new Color(1f, 0.95f, 0.4f));
				_promptLabel.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 1));
				_promptLabel.AddThemeConstantOverride("outline_size", 4);
			}

			SetupPromptIcon();
			OnReady();
		}

		public override void _ExitTree()
		{
			_inRange.Remove(this);
			if (Current == this)
				SetCurrent(_inRange.Count > 0 ? _inRange[_inRange.Count - 1] : null);
			OnExitTree();
		}

		private void SetupPromptIcon()
		{
			if (PromptIcon == null) return;

			// 씬에 이미 PromptIcon TextureRect가 있으면 그 텍스처만 설정, 없으면 자동 생성
			_promptIconRect = GetNodeOrNull<TextureRect>("PromptIcon");
			if (_promptIconRect == null)
			{
				_promptIconRect = new TextureRect
				{
					Name = "PromptIcon",
					Texture = PromptIcon,
					ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
					CustomMinimumSize = new Vector2(16, 16),
					Size = new Vector2(16, 16),
					Position = new Vector2(-8, -32), // 캐릭터 머리 위
					Visible = false,
				};
				AddChild(_promptIconRect);
			}
			else
			{
				_promptIconRect.Texture = PromptIcon;
				_promptIconRect.Visible = false;
			}
		}

		/// <summary>서브클래스 초기화용. base._Ready() 대신 이 메서드를 오버라이드하세요.</summary>
		protected virtual void OnReady() { }
		protected virtual void OnExitTree() { }

		/// <summary>플레이어가 상호작용 키를 누르면 호출됩니다.</summary>
		protected abstract void OnInteract();

		public override void _UnhandledInput(InputEvent @event)
		{
			// 영역이 겹쳐도 활성 타깃(가장 최근 진입)만 입력 처리 → UI 아이콘과 동작 일치 보장
			if (Current != this) return;
			if (@event.IsActionPressed("interact") && !@event.IsEcho())
			{
				ShowChapterDialogueIfAny();
				OnInteract();
				GetViewport().SetInputAsHandled();
			}
		}

		// 현재 챕터에 맞는 NPC 대사 토스트를 띄운다. ChapterDialogue 미정의 NPC는 no-op.
		// 포탈/광산 노드 등 NpcId 비어있는 인터랙터블도 자동 통과.
		private void ShowChapterDialogueIfAny()
		{
			if (string.IsNullOrEmpty(NpcId)) return;
			var gm = FirstGame.Core.GameManager.Instance;
			if (gm == null) return;
			string line = FirstGame.Data.ChapterDialogue.Get(NpcId, gm.CurrentChapter);
			if (string.IsNullOrEmpty(line)) return;
			var hud = GetTree()?.CurrentScene?.GetNodeOrNull<FirstGame.UI.HUD>("HUD");
			hud?.ShowNpcDialogue(NpcId, line);
		}

		private void OnBodyEntered(Node2D body)
		{
			if (!body.IsInGroup("Player")) return;
			PlayerInRange = true;
			ShowPrompt(true);
			if (!_inRange.Contains(this))
				_inRange.Add(this);
			SetCurrent(this); // 가장 최근 진입한 인터랙터블이 활성
			OnPlayerEntered(body);
		}

		private void OnBodyExited(Node2D body)
		{
			if (!body.IsInGroup("Player")) return;
			PlayerInRange = false;
			ShowPrompt(false);
			_inRange.Remove(this);
			if (Current == this)
				SetCurrent(_inRange.Count > 0 ? _inRange[_inRange.Count - 1] : null);
			OnPlayerExited(body);
		}

		private void ShowPrompt(bool visible)
		{
			if (_promptLabel != null) _promptLabel.Visible = visible;
			if (_promptIconRect != null) _promptIconRect.Visible = visible;
		}

		private static void SetCurrent(BaseInteractable target)
		{
			if (Current == target) return;
			Current = target;
			CurrentChanged?.Invoke(target);
		}

		protected virtual void OnPlayerEntered(Node2D player) { }
		protected virtual void OnPlayerExited(Node2D player) { }

		/// <summary>
		/// NPC가 지금 처리할 퀘스트 액션이 있으면 QuestDialog를 열고 true 반환.
		/// 단순 진행 중 안내나 Deliver 대상 방문은 NPC 본 UI(상점/스킬샵/대장간)를 막지 않는다.
		/// </summary>
		protected bool TryOpenQuestDialog()
		{
			if (string.IsNullOrEmpty(NpcId)) return false;
			var qm = FirstGame.Core.GameManager.Instance?.QuestManager;
			var player = FirstGame.Core.GameManager.Instance?.Player;
			if (qm == null || player == null) return false;

			if (qm.HasActiveQuest)
			{
				var quest = qm.ActiveQuest;

				if (quest.Type == FirstGame.Data.QuestType.Deliver && quest.TargetNpcId == NpcId)
				{
					bool wasComplete = qm.IsActiveQuestComplete;
					qm.NotifyNpcTalked(NpcId, player);
					if (!wasComplete && qm.IsActiveQuestComplete)
						FirstGame.Core.SaveManager.SaveGame();
					return false;
				}

				bool isGiver = quest.GiverNpcId == NpcId;
				if (!isGiver || !qm.IsActiveQuestComplete)
					return false;

				return OpenQuestDialog(player);
			}

			bool hasNextQuest = !qm.HasActiveQuest && qm.FindNextQuestForNpc(NpcId) != null;

			if (!hasNextQuest) return false;

			return OpenQuestDialog(player);
		}

		private bool OpenQuestDialog(FirstGame.Core.Interfaces.IPlayer player)
		{
			var dialog = GetTree()?.CurrentScene?.GetNodeOrNull<FirstGame.UI.QuestDialog>("QuestDialog");
			if (dialog == null)
			{
				GD.PrintErr("BaseInteractable: QuestDialog 노드 없음 (씬에 추가 필요)");
				return false;
			}

			dialog.OpenForNpc(NpcId, player);
			return true;
		}
	}
}
