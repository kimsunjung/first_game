using Godot;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController
	{
		// ─── 카메라 줌 테스트 (F5: 1.5배 / F6: 2.0배 / F7: 2.5배) ───
		private static readonly Vector2[] ZoomPresets = {
			new(1.5f, 1.5f),
			new(2.0f, 2.0f),
			new(2.5f, 2.5f),
		};
		private int _currentZoomIndex = 1; // 기본 2.0배

		private void ApplyCameraZoom()
		{
			if (_camera == null) return;
			_camera.Zoom = ZoomPresets[_currentZoomIndex];
			GD.Print($"[Camera] 줌: {_camera.Zoom.X}x");
		}

		private void HandleZoomInput(InputEvent @event)
		{
			if (_camera == null) return;
			if (@event is InputEventKey k && k.Pressed && !k.Echo)
			{
				if (k.Keycode == Key.F5) { _currentZoomIndex = 0; ApplyCameraZoom(); }
				else if (k.Keycode == Key.F6) { _currentZoomIndex = 1; ApplyCameraZoom(); }
				else if (k.Keycode == Key.F7) { _currentZoomIndex = 2; ApplyCameraZoom(); }
			}
		}

		// ─── 카메라 경계 자동 감지 ──────────────────────────────────
		// Background ColorRect 크기를 기준으로 경계 설정.
		// TileMap 기반은 _Ready() 호출 순서(Player < MapGenerator) 때문에
		// 항상 타일이 비어있어 LimitBottom=2000으로 떨어지는 버그가 있음.
		private void ApplyCameraBounds()
		{
			if (_camera == null) return;

			var scene = GetTree().CurrentScene;

			// Background ColorRect(씬 배경)로 경계 결정 — 가장 신뢰할 수 있는 소스
			var bg = scene.GetNodeOrNull<ColorRect>("Background");
			if (bg != null)
			{
				var size = bg.GetRect().Size;
				_camera.LimitLeft   = 0;
				_camera.LimitTop    = 0;
				_camera.LimitRight  = (int)size.X;
				_camera.LimitBottom = (int)size.Y;
				GD.Print($"[Camera] Background 경계: {size.X}x{size.Y}");
				return;
			}

			// Background 없는 씬(예: 일부 던전) → TileMap 기반으로 대체
			TileMapLayer tileMap = FindTileMapLayer(scene);
			if (tileMap != null)
			{
				var used = tileMap.GetUsedRect();
				int tileSize = 16;
				var ts = tileMap.TileSet;
				if (ts != null) tileSize = ts.TileSize.X;

				if (used.Size.X > 0 && used.Size.Y > 0)
				{
					_camera.LimitLeft   = (int)(used.Position.X * tileSize + tileMap.GlobalPosition.X);
					_camera.LimitTop    = (int)(used.Position.Y * tileSize + tileMap.GlobalPosition.Y);
					_camera.LimitRight  = (int)((used.Position.X + used.Size.X) * tileSize + tileMap.GlobalPosition.X);
					_camera.LimitBottom = (int)((used.Position.Y + used.Size.Y) * tileSize + tileMap.GlobalPosition.Y);
					GD.Print($"[Camera] TileMap 경계: L={_camera.LimitLeft} T={_camera.LimitTop} R={_camera.LimitRight} B={_camera.LimitBottom}");
					return;
				}
			}

			// 최후 폴백: 1280×720 고정
			_camera.LimitLeft   = 0;
			_camera.LimitTop    = 0;
			_camera.LimitRight  = 1280;
			_camera.LimitBottom = 720;
			GD.Print("[Camera] 폴백 경계: 1280x720");
		}

		private TileMapLayer FindTileMapLayer(Node root)
		{
			if (root is TileMapLayer t) return t;
			foreach (var child in root.GetChildren())
			{
				var found = FindTileMapLayer(child);
				if (found != null) return found;
			}
			return null;
		}

		// ─── 화면 흔들림 ────────────────────────────────────────────
		public void TriggerCameraShake(float intensity, float duration)
		{
			if (!FirstGame.Core.GameSettings.ScreenShakeEnabled) return;
			_shakeIntensity = intensity;
			_shakeTimer = duration;
			_shakeDuration = duration;
		}

		private void UpdateCameraShake(double delta)
		{
			if (_shakeTimer <= 0f || _camera == null) return;
			_shakeTimer -= (float)delta;
			if (_shakeTimer <= 0f)
			{
				_shakeTimer = 0f;
				_camera.Offset = Vector2.Zero;
				return;
			}
			float ratio = _shakeDuration > 0f ? _shakeTimer / _shakeDuration : 0f;
			_camera.Offset = new Vector2(
				(float)GD.RandRange(-_shakeIntensity, _shakeIntensity) * ratio,
				(float)GD.RandRange(-_shakeIntensity, _shakeIntensity) * ratio
			);
		}
	}
}
