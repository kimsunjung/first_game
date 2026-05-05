using Godot;
using FirstGame.Core;

namespace FirstGame.UI
{
	/// <summary>
	/// 토글 가능한 UI 창의 공통 베이스. 자동 그룹 등록, 일시정지 카운터, 다른 창 닫기를 처리합니다.
	/// 자식은 OnReadyInternal/OnOpened/OnClosed/CanOpen만 오버라이드하면 됩니다.
	/// </summary>
	public abstract partial class BaseUIWindow : CanvasLayer
	{
		public const string GroupName = "ui_window";

		public override void _Ready()
		{
			Visible = false;
			AddToGroup(GroupName);
			ProcessMode = ProcessModeEnum.Always;
			OnReadyInternal();
		}

		public override void _ExitTree()
		{
			if (Visible) UIPauseManager.ReleasePause();
			OnExitTreeInternal();
		}

		public virtual void Toggle()
		{
			if (Visible) Close();
			else Open();
		}

		public virtual void Open()
		{
			if (Visible) return;
			if (!CanOpen()) return;
			WindowManager.CloseOthers(this);
			// 그룹 외부(상점/대장간 등)가 일시정지를 점유 중이면 열지 않음
			if (UIPauseManager.IsPaused) return;
			Visible = true;
			UIPauseManager.RequestPause();
			OnOpened();
		}

		public virtual void Close()
		{
			if (!Visible) return;
			Visible = false;
			UIPauseManager.ReleasePause();
			OnClosed();
		}

		// 자식이 오버라이드 — 기본은 항상 열기 가능
		protected virtual bool CanOpen() => true;
		protected virtual void OnReadyInternal() {}
		protected virtual void OnExitTreeInternal() {}
		protected virtual void OnOpened() {}
		protected virtual void OnClosed() {}
	}
}
