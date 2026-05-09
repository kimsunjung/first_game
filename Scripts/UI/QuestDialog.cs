using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	/// <summary>
	/// NPC와 상호작용 시 퀘스트 부여/진행/완료를 처리하는 다이얼로그.
	/// 상태에 따라 [수락]/[완료]/[닫기] 버튼 노출.
	/// </summary>
	public partial class QuestDialog : BaseUIWindow
	{
		private Label _titleLabel;
		private Label _descLabel;
		private Label _progressLabel;
		private Button _primaryBtn;
		private Button _closeBtn;

		private enum DialogMode { Offer, InProgress, ReadyToComplete, NoQuest }
		private DialogMode _mode;
		private QuestData _quest;
		private IPlayer _player;

		protected override void OnReadyInternal()
		{
			_titleLabel = GetNodeOrNull<Label>("%TitleLabel");
			_descLabel = GetNodeOrNull<Label>("%DescLabel");
			_progressLabel = GetNodeOrNull<Label>("%ProgressLabel");
			_primaryBtn = GetNodeOrNull<Button>("%PrimaryBtn");
			_closeBtn = GetNodeOrNull<Button>("%CloseBtn");

			if (_primaryBtn != null) _primaryBtn.Pressed += OnPrimaryPressed;
			if (_closeBtn != null) _closeBtn.Pressed += () => Close();
		}

		public void OpenForNpc(string npcId, IPlayer player)
		{
			_player = player;
			var qm = GameManager.Instance?.QuestManager;
			if (qm == null) return;

			// 1) 진행 중인 퀘스트가 이 NPC와 관련 있나?
			if (qm.HasActiveQuest)
			{
				_quest = qm.ActiveQuest;
				// Deliver 타겟이면 즉시 도착 알림 → 완료 가능
				if (_quest.Type == QuestType.Deliver && _quest.TargetNpcId == npcId)
					qm.NotifyNpcTalked(npcId, player);

				bool isGiver = _quest.GiverNpcId == npcId;
				if (qm.IsActiveQuestComplete && isGiver)
					_mode = DialogMode.ReadyToComplete;
				else if (isGiver)
					_mode = DialogMode.InProgress;
				else
				{
					// 다른 NPC 퀘스트 진행 중 — 안내만
					_mode = DialogMode.NoQuest;
				}
			}
			else
			{
				// 2) 이 NPC가 줄 다음 퀘스트 검색
				_quest = qm.FindNextQuestForNpc(npcId);
				_mode = _quest != null ? DialogMode.Offer : DialogMode.NoQuest;
			}

			RefreshUi(npcId);
			// Open()을 직접 사용 — Toggle()은 이미 열린 상태에서 재호출 시 닫혀 버려
			// 같은 NPC에게 빠르게 두 번 말 걸면 다이얼로그가 사라지는 문제가 발생.
			Open();
		}

		private void RefreshUi(string npcId)
		{
			switch (_mode)
			{
				case DialogMode.Offer:
					_titleLabel.Text = $"새 퀘스트: {_quest.QuestTitle}";
					_descLabel.Text = _quest.Description + "\n\n" + RewardSummary(_quest);
					_progressLabel.Text = $"목표: {ObjectiveSummary(_quest)}";
					_primaryBtn.Text = "수락";
					_primaryBtn.Visible = true;
					break;
				case DialogMode.InProgress:
					_titleLabel.Text = $"진행 중: {_quest.QuestTitle}";
					_descLabel.Text = _quest.Description;
					_progressLabel.Text = $"진행도: {GameManager.Instance.QuestManager.Progress} / {_quest.TargetCount}";
					_primaryBtn.Visible = false;
					break;
				case DialogMode.ReadyToComplete:
					_titleLabel.Text = $"완료: {_quest.QuestTitle}";
					_descLabel.Text = _quest.Description + "\n\n" + RewardSummary(_quest);
					_progressLabel.Text = "목표 달성! 보상을 받으세요.";
					_primaryBtn.Text = "보상 받기";
					_primaryBtn.Visible = true;
					break;
				default:
					_titleLabel.Text = "할 말이 없네요.";
					_descLabel.Text = GameManager.Instance.QuestManager.HasActiveQuest
						? "다른 NPC의 퀘스트가 진행 중입니다."
						: "지금은 부여할 퀘스트가 없습니다.";
					_progressLabel.Text = "";
					_primaryBtn.Visible = false;
					break;
			}
		}

		private void OnPrimaryPressed()
		{
			var qm = GameManager.Instance?.QuestManager;
			if (qm == null || _quest == null) return;

			switch (_mode)
			{
				case DialogMode.Offer:
					qm.StartQuest(_quest);
					Close();
					break;
				case DialogMode.ReadyToComplete:
					if (qm.CompleteQuest(_player))
					{
						Close();
					}
					else
					{
						// 인벤 공간 부족 또는 재료 사라짐 — 창 유지하고 사용자에게 안내.
						_progressLabel.Text = "가방 공간이 부족하거나 재료가 사라졌습니다. 정리 후 다시 시도하세요.";
						_progressLabel.AddThemeColorOverride("font_color", new Color(1f, 0.5f, 0.4f));
					}
					break;
			}
		}

		private static string ObjectiveSummary(QuestData q) => q.Type switch
		{
			QuestType.Kill => $"{q.TargetEnemyType} {q.TargetCount}마리 처치",
			QuestType.Gather => $"{q.TargetItem?.ItemName ?? "?"} {q.TargetCount}개 수집",
			QuestType.Deliver => $"{q.TargetNpcId}에게 전달",
			QuestType.Explore => $"{q.TargetScene}로 이동",
			_ => "?"
		};

		private static string RewardSummary(QuestData q)
		{
			string s = $"보상: {q.GoldReward}G";
			if (q.ExpReward > 0) s += $", EXP +{q.ExpReward}";
			if (q.RewardItem != null) s += $", {q.RewardItem.ItemName}×{System.Math.Max(1, q.RewardItemQuantity)}";
			return s;
		}
	}
}
