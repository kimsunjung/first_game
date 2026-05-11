using System;

namespace FirstGame.Core
{
	/// <summary>
	/// 다단계 상태 변경(인벤/장비/보상/씬 전환)을 안전하게 묶는 트랜잭션 헬퍼.
	/// using 블록으로 suspend/resume pair 누락을 컴파일/구조적으로 차단한다.
	///
	/// 막아주는 것:
	/// 1) <c>OnInventoryChanged → RequestAutoSave</c>가 중간 상태를 디스크에 박는 것 (autosave suspend)
	/// 2) <c>OnInventoryChanged → TryClaimPendingRewards</c>가 트랜잭션 중간에 빈 슬롯을 가로채
	///    SaveGame을 호출하는 것 (pending claim suspend)
	///
	/// 사용 예:
	/// <code>
	/// // 인벤 변경 + 명시적 SaveGame (강화/퀘스트 완료):
	/// using (var tx = GameTransaction.Begin(suspendAutoSave: false)) { ... }
	/// SaveManager.SaveGame();
	///
	/// // 인벤 변경 + 씬 전환 (귀환 주문서):
	/// using (var tx = GameTransaction.Begin()) {
	///     RemoveItem(...);
	///     if (Teleport()) tx.SetClaimAfterDispose(false); // 새 씬에 위임
	///     else AddItem(...); // 롤백
	/// }
	/// </code>
	/// </summary>
	public static class GameTransaction
	{
		public static Scope Begin(
			bool suspendAutoSave = true,
			bool suspendPendingClaims = true,
			bool claimAfterDispose = true)
		{
			return new Scope(suspendAutoSave, suspendPendingClaims, claimAfterDispose);
		}

		public sealed class Scope : IDisposable
		{
			private readonly bool _suspendAutoSave;
			private readonly bool _suspendPendingClaims;
			private bool _claimAfter;
			private bool _disposed;

			internal Scope(bool suspendAutoSave, bool suspendPendingClaims, bool claimAfter)
			{
				_suspendAutoSave = suspendAutoSave;
				_suspendPendingClaims = suspendPendingClaims;
				_claimAfter = claimAfter;
				if (_suspendAutoSave) SaveManager.SuspendAutoSave();
				if (_suspendPendingClaims) GameManager.Instance?.SuspendPendingRewardClaims();
			}

			/// <summary>Dispose 시 TryClaim 실행 여부 변경. 씬 전환이 큐잉된 경우 false로 호출 —
			/// 새 씬의 LoadFromSaveData → TryClaim이 책임지게 위임. 그렇지 않으면 deferred 큐
			/// 상태에서 BuildSaveData가 old CurrentScene을 캡처해 디스크를 덮을 수 있다.</summary>
			public void SetClaimAfterDispose(bool value) => _claimAfter = value;

			public void Dispose()
			{
				if (_disposed) return;
				_disposed = true;
				// 순서: pending claim 먼저 풀고 autosave 풀기. claim의 SaveGame 우회가
				// 직접 호출이라 autosave suspend와 무관하지만, 순서를 통일해 향후 회귀를 막는다.
				if (_suspendPendingClaims)
					GameManager.Instance?.ResumePendingRewardClaims(claimNow: _claimAfter);
				if (_suspendAutoSave)
					SaveManager.ResumeAutoSave();
			}
		}
	}
}
