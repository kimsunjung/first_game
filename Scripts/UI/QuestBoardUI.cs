using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.UI
{
	/// <summary>
	/// 퀘스트 보드 UI — 사이드 퀘스트 목록을 표시.
	/// 수락 가능한 퀘스트만 노출. 진행중인 퀘스트가 있으면 안내만.
	/// quest_board NPC 상호작용 시 OpenForPlayer(player) 호출.
	/// </summary>
	public partial class QuestBoardUI : BaseUIWindow
	{
		private VBoxContainer _questList;
		private Label _statusLabel;
		private Button _closeBtn;
		private IPlayer _player;

		protected override void OnReadyInternal()
		{
			_questList = GetNodeOrNull<VBoxContainer>("%QuestList");
			_statusLabel = GetNodeOrNull<Label>("%StatusLabel");
			_closeBtn = GetNodeOrNull<Button>("%CloseBtn");
			if (_closeBtn != null) _closeBtn.Pressed += () => Close();
		}

		public void OpenForPlayer(IPlayer player)
		{
			_player = player;
			Open();
			Refresh();
		}

		protected override void OnOpened() => Refresh();

		private void Refresh()
		{
			if (_questList == null) return;
			foreach (Node child in _questList.GetChildren()) child.QueueFree();

			var qm = GameManager.Instance?.QuestManager;
			if (qm == null) return;

			if (qm.HasActiveQuest)
			{
				if (_statusLabel != null)
					_statusLabel.Text = $"이미 진행 중: [{qm.ActiveQuest.QuestTitle}] — 완료 후 다시 방문하세요.";
				return;
			}

			var manifest = GD.Load<QuestManifest>("res://Resources/Quests/quest_manifest.tres");
			if (manifest?.SideQuests == null) return;

			int available = 0;
			foreach (var q in manifest.SideQuests)
			{
				if (q == null) continue;
				if (q.GiverNpcId != "quest_board") continue;
				bool done = qm.CompletedQuestIds.Contains(q.QuestId);
				if (done && !q.IsRepeatable) continue;

				AddQuestRow(q, done);
				available++;
			}

			if (_statusLabel != null)
				_statusLabel.Text = available > 0
					? $"수락 가능한 퀘스트: {available}개"
					: "현재 수락 가능한 퀘스트가 없습니다.";
		}

		private void AddQuestRow(QuestData q, bool wasCompleted)
		{
			var row = new PanelContainer();
			row.CustomMinimumSize = new Vector2(0, 56);
			var style = new StyleBoxFlat
			{
				BgColor = new Color(0.12f, 0.12f, 0.15f, 0.95f),
				BorderColor = new Color(0.45f, 0.40f, 0.20f, 0.9f),
			};
			style.SetBorderWidthAll(1);
			style.SetCornerRadiusAll(4);
			row.AddThemeStyleboxOverride("panel", style);

			var hbox = new HBoxContainer();
			hbox.AddThemeConstantOverride("separation", 12);
			row.AddChild(hbox);

			var vbox = new VBoxContainer();
			vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			hbox.AddChild(vbox);

			var title = new Label { Text = q.QuestTitle };
			title.AddThemeFontSizeOverride("font_size", 14);
			title.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f));
			vbox.AddChild(title);

			var desc = new Label { Text = q.Description, AutowrapMode = TextServer.AutowrapMode.Word };
			desc.AddThemeFontSizeOverride("font_size", 11);
			desc.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.85f));
			vbox.AddChild(desc);

			var reward = new Label { Text = $"보상: {q.GoldReward}G  |  EXP +{q.ExpReward}" };
			reward.AddThemeFontSizeOverride("font_size", 10);
			reward.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 0.7f));
			vbox.AddChild(reward);

			var acceptBtn = new Button
			{
				Text = wasCompleted ? "재수락" : "수락",
				CustomMinimumSize = new Vector2(70, 0),
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
			};
			acceptBtn.Pressed += () => OnAcceptPressed(q);
			hbox.AddChild(acceptBtn);

			_questList.AddChild(row);
		}

		private void OnAcceptPressed(QuestData q)
		{
			var qm = GameManager.Instance?.QuestManager;
			if (qm == null || _player == null) return;
			if (qm.HasActiveQuest)
			{
				GD.Print("이미 진행 중인 퀘스트가 있습니다.");
				return;
			}
			// 반복 퀘스트 재수락 시 CompletedQuestIds에서 제거 (재진행 가능하게)
			if (q.IsRepeatable) qm.CompletedQuestIds.Remove(q.QuestId);
			qm.StartQuest(q);
			Close();
		}
	}
}
