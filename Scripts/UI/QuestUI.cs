using Godot;
using FirstGame.Core;
using FirstGame.Data;

namespace FirstGame.UI
{
	public partial class QuestUI : CanvasLayer
	{
		private Control _questPanel;
		private Label _questTitleLabel;
		private Label _questProgressLabel;
		private Label _questCompleteLabel;

		public override void _Ready()
		{
			_questPanel = GetNodeOrNull<Control>("%QuestPanel");
			_questTitleLabel = GetNodeOrNull<Label>("%QuestTitleLabel");
			_questProgressLabel = GetNodeOrNull<Label>("%QuestProgressLabel");
			_questCompleteLabel = GetNodeOrNull<Label>("%QuestCompleteLabel");

			if (_questPanel != null) _questPanel.Visible = false;
			if (_questCompleteLabel != null) _questCompleteLabel.Visible = false;

			QuestManager.OnQuestAccepted += ShowQuest;
			QuestManager.OnProgressUpdated += UpdateProgress;
			QuestManager.OnQuestCompleted += ShowComplete;
		}

		public override void _ExitTree()
		{
			QuestManager.OnQuestAccepted -= ShowQuest;
			QuestManager.OnProgressUpdated -= UpdateProgress;
			QuestManager.OnQuestCompleted -= ShowComplete;
		}

		private void ShowQuest(QuestData quest)
		{
			if (_questPanel != null) _questPanel.Visible = true;
			if (_questTitleLabel != null) _questTitleLabel.Text = quest.QuestTitle;
			if (_questProgressLabel != null) _questProgressLabel.Text = $"0 / {quest.TargetCount}";
		}

		private void UpdateProgress(int current, int total)
		{
			if (_questProgressLabel != null) _questProgressLabel.Text = $"{current} / {total}";
		}

		private async void ShowComplete(QuestData quest)
		{
			if (_questCompleteLabel != null)
			{
				_questCompleteLabel.Text = $"퀘스트 완료! +{quest.GoldReward}G";
				_questCompleteLabel.Visible = true;
			}
			if (_questPanel != null) _questPanel.Visible = false;

			await ToSignal(GetTree().CreateTimer(3.0), SceneTreeTimer.SignalName.Timeout);
			if (IsInstanceValid(this) && _questCompleteLabel != null)
				_questCompleteLabel.Visible = false;
		}
	}
}
