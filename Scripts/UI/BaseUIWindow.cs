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

		// 창 외부(콘텐츠 패널 영역 밖)를 터치/클릭하면 닫히도록 풀스크린 투명 캡처 영역.
		// 컨텐츠보다 뒤(자식 인덱스 0)에 두어 컨텐츠 패널이 우선 입력을 가져가게 한다.
		private ColorRect _dismissArea;

		public override void _Ready()
		{
			Visible = false;
			AddToGroup(GroupName);
			ProcessMode = ProcessModeEnum.Always;
			EnsureDismissArea();
			OnReadyInternal();
		}

		private void EnsureDismissArea()
		{
			_dismissArea = new ColorRect
			{
				Name = "DismissArea",
				Color = new Color(0, 0, 0, 0), // 완전 투명 — 모달 dim 원하면 알파 조정
				MouseFilter = Control.MouseFilterEnum.Stop
			};
			_dismissArea.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			_dismissArea.GuiInput += OnDismissAreaInput;
			AddChild(_dismissArea);
			MoveChild(_dismissArea, 0); // 가장 뒤로 — 컨텐츠가 위에 그려지고 입력 우선 처리됨
		}

		private void OnDismissAreaInput(InputEvent @event)
		{
			bool dismiss =
				(@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left) ||
				(@event is InputEventScreenTouch st && st.Pressed);
			if (!dismiss) return;
			Close();
			_dismissArea.AcceptEvent();
		}

		public override void _ExitTree()
		{
			if (Visible) UIPauseManager.ReleasePause();
			OnExitTreeInternal();
		}

		/// <summary>ui_cancel(Esc/뒤로가기) 처리 — 보이는 상태면 닫기. 자식이 더 필요하면 오버라이드 후 base 호출.</summary>
		public override void _UnhandledInput(InputEvent @event)
		{
			if (!Visible) return;
			if (@event.IsActionPressed("ui_cancel") && !@event.IsEcho())
			{
				Close();
				GetViewport().SetInputAsHandled();
			}
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
