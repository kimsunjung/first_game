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
		private void ApplyCameraBounds()
		{
			if (_camera == null) return;

			// 씬에서 TileMapLayer를 찾아 경계 자동 계산
			var scene = GetTree().CurrentScene;
			TileMapLayer tileMap = FindTileMapLayer(scene);

			if (tileMap != null)
			{
				var used = tileMap.GetUsedRect();
				int tileSize = 16;
				var ts = tileMap.TileSet;
				if (ts != null) tileSize = ts.TileSize.X;

				// 빈 TileMapLayer (타일 없음) → 넓은 기본값 사용
				if (used.Size.X <= 0 || used.Size.Y <= 0)
				{
					_camera.LimitLeft = -500;
					_camera.LimitTop = -500;
					_camera.LimitRight = 2000;
					_camera.LimitBottom = 2000;
					GD.Print("[Camera] TileMap 비어있음 → 기본 경계 사용");
				}
				else
				{
					_camera.LimitLeft = (int)(used.Position.X * tileSize + tileMap.GlobalPosition.X);
					_camera.LimitTop = (int)(used.Position.Y * tileSize + tileMap.GlobalPosition.Y);
					_camera.LimitRight = (int)((used.Position.X + used.Size.X) * tileSize + tileMap.GlobalPosition.X);
					_camera.LimitBottom = (int)((used.Position.Y + used.Size.Y) * tileSize + tileMap.GlobalPosition.Y);
					GD.Print($"[Camera] TileMap 경계 적용: L={_camera.LimitLeft} T={_camera.LimitTop} R={_camera.LimitRight} B={_camera.LimitBottom}");
				}
			}
			else
			{
				// TileMap이 없으면 넓은 기본값
				_camera.LimitLeft = -500;
				_camera.LimitTop = -500;
				_camera.LimitRight = 2000;
				_camera.LimitBottom = 2000;
			}
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
