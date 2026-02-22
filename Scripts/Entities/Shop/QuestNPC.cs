using Godot;
using FirstGame.Core;
using FirstGame.Data;

namespace FirstGame.Entities.Shop
{
	public partial class QuestNPC : Area2D
	{
		[Export] public QuestData[] AvailableQuests { get; set; }

		private bool _playerInRange = false;
		private Label _promptLabel;
		private int _currentQuestIndex = 0;

		public override void _Ready()
		{
			BodyEntered += OnBodyEntered;
			BodyExited += OnBodyExited;
			_promptLabel = GetNodeOrNull<Label>("PromptLabel");
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!_playerInRange) return;
			if (@event.IsActionPressed("interact") && !@event.IsEcho())
			{
				TryInteract();
			}
		}

		private void TryInteract()
		{
			if (QuestManager.HasActiveQuest)
			{
				// 진행 중인 퀘스트 상태 표시
				var q = QuestManager.CurrentQuest;
				GD.Print($"퀘스트 진행 중: {q.QuestTitle} ({QuestManager.KillProgress}/{q.TargetCount})");
				return;
			}

			if (AvailableQuests == null || AvailableQuests.Length == 0) return;

			// 순서대로 퀘스트 제공 (이미 받은 퀘스트는 건너뜀)
			var quest = AvailableQuests[_currentQuestIndex % AvailableQuests.Length];
			if (QuestManager.AcceptQuest(quest))
			{
				_currentQuestIndex++;
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (!body.IsInGroup("Player")) return;
			_playerInRange = true;
			if (_promptLabel != null) _promptLabel.Visible = true;
		}

		private void OnBodyExited(Node2D body)
		{
			if (!body.IsInGroup("Player")) return;
			_playerInRange = false;
			if (_promptLabel != null) _promptLabel.Visible = false;
		}
	}
}
