using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Core.Interfaces;

namespace FirstGame.Core
{
	public partial class GameManager : Node
	{
		// 싱글톤 패턴 (Singleton Pattern)
		public static GameManager Instance { get; private set; }

		// 예시: 전역 상태 (골드) (Example Global State)
		private int _playerGold = 0;
		public int PlayerGold 
		{ 
			get => _playerGold;
			set
			{
				_playerGold = value;
				OnGoldChanged?.Invoke(_playerGold);
			}
		}

		// 플레이어 참조 (Player reference — set by PlayerController)
		public IPlayer Player { get; set; }

		// 활성 적 캐시 (Active enemy cache — avoids GetNodesInGroup every frame)
		private readonly List<Node2D> _activeEnemies = new();
		public IReadOnlyList<Node2D> ActiveEnemies => _activeEnemies;
		public void RegisterEnemy(Node2D enemy) => _activeEnemies.Add(enemy);
		public void UnregisterEnemy(Node2D enemy) => _activeEnemies.Remove(enemy);

		// 메인 퀘스트 매니저 (1개씩만 진행)
		public QuestManager QuestManager { get; } = new();

		// 처치한 보스 목록
		private readonly HashSet<string> _defeatedBosses = new();
		public IReadOnlyCollection<string> DefeatedBosses => _defeatedBosses;
		public void RecordBossDefeat(string bossId) => _defeatedBosses.Add(bossId);
		public bool IsBossDefeated(string bossId) => _defeatedBosses.Contains(bossId);
		public void RestoreDefeatedBosses(List<string> bosses)
		{
			_defeatedBosses.Clear();
			foreach (var b in bosses) _defeatedBosses.Add(b);
		}

		// 보류 보상함 — 인벤 가득 등으로 즉시 지급 못한 보스 드랍을 영속 보관.
		// 세이브에 함께 직렬화되어 앱 종료/재시작에도 손실되지 않는다.
		private readonly List<FirstGame.Data.SavedItemSlot> _pendingRewards = new();
		public IReadOnlyList<FirstGame.Data.SavedItemSlot> PendingRewards => _pendingRewards;

		// AddItem이 동기적으로 OnInventoryChanged를 발생시켜 TryClaimPendingRewards가 자기
		// 자신을 재진입할 때 같은 보상이 중복 지급되거나 RemoveAt이 throw하는 결함 차단.
		private bool _claimingPendingRewards = false;
		// QuestManager.CompleteQuest 같은 트랜잭션 중에는 ConsumeItems가 만든 빈 슬롯을
		// pending reward가 가로채면 안 됨. Suspend/Resume API로 명시 격리.
		private int _claimSuspendCount = 0;
		// 세이브 로드 도중에는 인벤/장비/퀘스트가 부분 복원 상태이므로 pending claim 및
		// 그로 인한 SaveGame이 실행되면 안 됨. PlayerController.LoadFromSaveData가
		// BeginRestoreState/EndRestoreState로 전체 복원을 감싸고, 끝나면 1회 TryClaim한다.
		private bool _isRestoringState = false;
		public bool IsRestoringState => _isRestoringState;
		public void BeginRestoreState() => _isRestoringState = true;
		public void EndRestoreState() => _isRestoringState = false;

		public event Action<FirstGame.Data.ItemData, int> OnPendingRewardAdded; // (item, qty)
		public event Action<FirstGame.Data.ItemData, int> OnPendingRewardClaimed; // (item, qty)

		public void AddPendingReward(FirstGame.Data.ItemData item, int qty, int enhancement = 0)
		{
			if (item == null || qty <= 0) return;
			_pendingRewards.Add(new FirstGame.Data.SavedItemSlot
			{
				ItemPath = item.ResourcePath,
				Quantity = qty,
				EnhancementLevel = enhancement
			});
			OnPendingRewardAdded?.Invoke(item, qty);
		}

		public void RestorePendingRewards(List<FirstGame.Data.SavedItemSlot> list)
		{
			_pendingRewards.Clear();
			if (list != null) _pendingRewards.AddRange(list);
		}

		// ─── 절차적 필드맵 seed 보관 (씬 경로 → seed) ─────────────
		// MapGenerator가 첫 진입 시 RecordFieldSeed로 등록, 같은 씬 재진입 시 GetFieldSeed로 재사용.
		// SaveData.FieldSeeds로 직렬화되어 다음 세션에도 같은 지형 유지.
		private readonly Dictionary<string, int> _fieldSeeds = new();
		public IReadOnlyDictionary<string, int> FieldSeeds => _fieldSeeds;
		public bool TryGetFieldSeed(string scenePath, out int seed) => _fieldSeeds.TryGetValue(scenePath, out seed);
		public void RecordFieldSeed(string scenePath, int seed)
		{
			if (string.IsNullOrEmpty(scenePath)) return;
			_fieldSeeds[scenePath] = seed;
		}
		public void RestoreFieldSeeds(Dictionary<string, int> map)
		{
			_fieldSeeds.Clear();
			if (map != null) foreach (var kv in map) _fieldSeeds[kv.Key] = kv.Value;
		}

		// ─── 방문한 씬 — 텔레포트 NPC의 목적지 활성화 기준 ─────
		private readonly HashSet<string> _visitedScenes = new();
		public IReadOnlyCollection<string> VisitedScenes => _visitedScenes;
		public bool HasVisitedScene(string scenePath) => !string.IsNullOrEmpty(scenePath) && _visitedScenes.Contains(scenePath);
		public void RecordSceneVisit(string scenePath)
		{
			if (string.IsNullOrEmpty(scenePath)) return;
			_visitedScenes.Add(scenePath);
		}
		public void RestoreVisitedScenes(List<string> list)
		{
			_visitedScenes.Clear();
			if (list != null) foreach (var s in list) if (!string.IsNullOrEmpty(s)) _visitedScenes.Add(s);
		}

		/// <summary>인벤 트랜잭션(예: QuestManager.CompleteQuest) 동안 보류 보상 클레임 차단.</summary>
		public void SuspendPendingRewardClaims() => _claimSuspendCount++;
		/// <summary>Resume. claimNow=false면 카운터만 풀고 즉시 TryClaim하지 않음 — 귀환 주문서처럼
		/// 트랜잭션 끝에 씬 전환이 큐잉된 경우, 여기서 claim하면 BuildSaveData가 deferred 큐 상태에서
		/// old CurrentScene을 캡처해 디스크의 목적지 정보를 덮어쓴다. 그 경우 호출자가 false로 전달.</summary>
		public void ResumePendingRewardClaims(bool claimNow = true)
		{
			if (_claimSuspendCount > 0) _claimSuspendCount--;
			if (_claimSuspendCount == 0 && claimNow) TryClaimPendingRewards();
		}

		/// <summary>인벤이 비어 자리 생긴 시점에 호출. 가능한 보상부터 순서대로 지급.</summary>
		public void TryClaimPendingRewards()
		{
			if (_claimingPendingRewards) return;       // 재진입 차단
			if (_claimSuspendCount > 0) return;        // 트랜잭션 격리
			if (_isRestoringState) return;             // 세이브 복원 중 차단
			if (_pendingRewards.Count == 0) return;
			var inv = Player?.Inventory;
			if (inv == null) return;

			_claimingPendingRewards = true;
			bool queueMutated = false;
			try
			{
				for (int i = _pendingRewards.Count - 1; i >= 0; i--)
				{
					var pending = _pendingRewards[i];
					var item = Godot.GD.Load<FirstGame.Data.ItemData>(pending.ItemPath);
					if (item == null)
					{
						_pendingRewards.RemoveAt(i); // 깨진 path 제거
						queueMutated = true;
						continue;
					}
					// CanAddItem 사전 확인 후 RemoveAt-먼저 → AddItem 순서.
					// 재진입 가드 외 추가 안전망: AddItem이 OnInventoryChanged를 발생시켜도
					// 이 항목은 이미 큐에서 제거된 상태라 같은 보상이 또 보이지 않는다.
					if (!inv.CanAddItem(item, pending.Quantity)) continue;
					_pendingRewards.RemoveAt(i);
					queueMutated = true;
					if (inv.AddItem(item, pending.Quantity, pending.EnhancementLevel))
						OnPendingRewardClaimed?.Invoke(item, pending.Quantity);
					else
						_pendingRewards.Insert(0, pending); // 매우 드문 race — 큐 끝에 복귀
				}
			}
			finally
			{
				_claimingPendingRewards = false;
			}

			// 보상 지급/깨진 path 제거가 발생했으면 즉시 저장 — 이 호출 후 OS kill 시
			// 파일에 stale pending이 남아 같은 보상이 재지급되는 결함 차단. 보스 보상이라
			// 빈번하지 않으므로 throttle 적용 대상 아님.
			if (queueMutated) SaveManager.SaveGame();
		}

		public void ResetForNewGame()
		{
			PlayerGold = 0;
			_defeatedBosses.Clear();
			_activeEnemies.Clear();
			_pendingRewards.Clear(); // 이전 세션 보류 보상이 새 게임으로 이월되지 않도록
			_fieldSeeds.Clear();
			_visitedScenes.Clear();
			_claimSuspendCount = 0;
			_claimingPendingRewards = false;
			_isRestoringState = false;
			QuestManager.RestoreFromSave("", 0, null);
			EventManager.ResetAll(); // 내부에서 QuestManager.Resubscribe 자동 호출
		}

		// UI 업데이트용 이벤트 (Event for UI updates)
		public event Action<int> OnGoldChanged;

		public override void _Ready()
		{
			if (Instance == null)
			{
				Instance = this;
				BalanceData.Load();
				GD.Print("게임 매니저 초기화됨 (GameManager Initialized)");
			}
			else
			{
				QueueFree();
			}
		}

		public override void _ExitTree()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

		// throttle 만료 시점에 dirty save를 자동 flush (RequestAutoSave 후 추가 행동 없이 앱이
		// 그대로 멈춰도 30초 뒤에는 보존되도록).
		public override void _Process(double delta)
		{
			SaveManager.TickDirty();
		}

		// 모바일 뒤로가기 / 백그라운드 진입 / 포커스 아웃 알림 처리.
		public override void _Notification(int what)
		{
			if (what == NotificationWMGoBackRequest)
			{
				if (FirstGame.UI.WindowManager.CloseTop()) return;
				ShowExitConfirm();
			}
			else if (what == NotificationApplicationPaused
				|| what == NotificationWMWindowFocusOut)
			{
				// 홈 버튼/앱 전환/OS kill 가능성 — 즉시 dirty 무관하게 저장해 진행 손실 차단.
				SaveManager.FlushBeforeExit();
			}
		}

		private ConfirmationDialog _exitDialog;
		private bool _exitDialogPausing = false;

		private void ShowExitConfirm()
		{
			if (_exitDialog == null || !IsInstanceValid(_exitDialog))
			{
				_exitDialog = new ConfirmationDialog
				{
					Title = "종료",
					DialogText = "게임을 종료하시겠습니까?",
					OkButtonText = "예",
					CancelButtonText = "아니오",
					ProcessMode = ProcessModeEnum.Always
				};
				_exitDialog.Confirmed += () =>
				{
					// dirty 여부 무관하게 무조건 저장 — 퀘스트 완료/상점/장비 등 dirty flag
					// 미설정 변경까지 보존.
					SaveManager.FlushBeforeExit();
					GetTree().Quit();
				};
				_exitDialog.VisibilityChanged += OnExitDialogVisibilityChanged;
				GetTree().Root.AddChild(_exitDialog);
			}
			if (_exitDialog.Visible) return;
			_exitDialogPausing = true;
			UIPauseManager.RequestPause();
			_exitDialog.PopupCentered();
		}

		private void OnExitDialogVisibilityChanged()
		{
			if (_exitDialog == null || _exitDialog.Visible) return;
			if (_exitDialogPausing)
			{
				_exitDialogPausing = false;
				UIPauseManager.ReleasePause();
			}
		}
	}
}
