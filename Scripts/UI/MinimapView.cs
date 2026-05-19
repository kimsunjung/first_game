using Godot;
using FirstGame.Core;

namespace FirstGame.UI
{
	// 경량 미니맵 — 현재 맵 경계 안에 플레이어/적/포탈을 점으로 표시.
	// _Draw 기반(노드 생성 없음), HUD._Process가 Refresh()로 매 프레임 QueueRedraw.
	// 월드 좌표는 맵 Background ColorRect 크기를 경계로 사용(없으면 1280×720 폴백).
	public partial class MinimapView : Control
	{
		private const float DefaultW = 1280f, DefaultH = 720f;
		private float _mapW = DefaultW, _mapH = DefaultH;
		private string _boundsScene = "";

		public override void _Ready()
		{
			MouseFilter = MouseFilterEnum.Ignore;
			ResolveBounds();
		}

		public void Refresh()
		{
			if (!IsInsideTree()) return;
			// 숨김 상태에서는 _Draw 가 안 불리므로 경계/가시성 판정을 여기서 한다
			// (허브→필드 전환 시 다시 보이게). 보일 때만 redraw 큐잉.
			ResolveBounds();
			if (Visible) QueueRedraw();
		}

		private void ResolveBounds()
		{
			var scene = GetTree()?.CurrentScene;
			if (scene == null) return;
			if (scene.SceneFilePath == _boundsScene) return;
			_boundsScene = scene.SceneFilePath;
			_mapW = DefaultW; _mapH = DefaultH;
			var bg = scene.GetNodeOrNull<ColorRect>("Background");
			if (bg != null && bg.Size.X > 0 && bg.Size.Y > 0)
			{
				_mapW = bg.Size.X;
				_mapH = bg.Size.Y;
			}
			// 단일 화면(허브 ~640×360)은 맵 전체가 이미 보이므로 미니맵 무가치 +
			// 매 틱 redraw 비용만 발생 → 숨김. 멀티스크린 필드/던전에서만 표시.
			Visible = _mapW > 760f || _mapH > 440f;
		}

		public override void _Draw()
		{
			ResolveBounds();
			Vector2 box = Size;
			if (box.X <= 0 || box.Y <= 0) return;

			// 배경 패널 + 테두리
			DrawRect(new Rect2(Vector2.Zero, box), new Color(0.05f, 0.06f, 0.08f, 0.7f), true);
			DrawRect(new Rect2(Vector2.Zero, box), new Color(0.6f, 0.65f, 0.75f, 0.8f), false, 1.0f);

			float scale = Mathf.Min(box.X / _mapW, box.Y / _mapH);
			Vector2 pad = (box - new Vector2(_mapW, _mapH) * scale) * 0.5f;
			Vector2 ToMap(Vector2 world) => pad + new Vector2(
				Mathf.Clamp(world.X, 0, _mapW),
				Mathf.Clamp(world.Y, 0, _mapH)) * scale;

			var gm = GameManager.Instance;
			if (gm == null) return;

			// 포탈(청록) — 현재 씬에서 Portal 그룹/타입 탐색.
			var scene = GetTree()?.CurrentScene;
			if (scene != null)
			{
				foreach (var n in scene.GetChildren())
				{
					if (n is FirstGame.Objects.Portal p && p.IsInsideTree())
						DrawRect(new Rect2(ToMap(p.GlobalPosition) - new Vector2(2, 2), new Vector2(4, 4)),
							new Color(0.3f, 0.85f, 0.95f));
				}
			}

			// 적(빨강)
			foreach (var e in gm.ActiveEnemies)
			{
				if (e != null && IsInstanceValid(e) && e.IsInsideTree())
					// 모바일 가시성 — 1.6px는 폰 화면에서 거의 안 보여 2.2px로.
					DrawCircle(ToMap(e.GlobalPosition), 2.2f, new Color(0.95f, 0.3f, 0.3f));
			}

			// 플레이어(연두, 약간 크게)
			if (gm.Player is Node2D pn && IsInstanceValid(pn) && pn.IsInsideTree())
			{
				Vector2 pp = ToMap(pn.GlobalPosition);
				DrawCircle(pp, 3.0f, new Color(0.4f, 1f, 0.45f));
				DrawCircle(pp, 3.0f, new Color(0, 0, 0, 0.8f));
				DrawCircle(pp, 2.2f, new Color(0.4f, 1f, 0.45f));
			}
		}
	}
}
