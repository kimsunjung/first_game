using Godot;
using System.Collections.Generic;
using FirstGame.UI;

namespace FirstGame.Core
{
	public static class UIEffectManager
	{
		private static readonly PackedScene FloatingLabelScene =
			GD.Load<PackedScene>("res://Scenes/UI/floating_label.tscn");

		private static readonly Queue<FloatingLabel> _pool = new();

		public static void SpawnFloatingLabel(Vector2 worldPos, int damage, bool isCrit, bool isPlayerDamage = false)
		{
			if (FloatingLabelScene == null) return;
			var scene = Engine.GetMainLoop() as SceneTree;
			if (scene?.CurrentScene == null) return;

			FloatingLabel label;
			if (_pool.Count > 0)
			{
				label = _pool.Dequeue();
				if (!IsInstanceValid(label))
				{
					// 풀에서 꺼냈지만 이미 해제된 인스턴스 → 새로 생성
					label = FloatingLabelScene.Instantiate<FloatingLabel>();
					scene.CurrentScene.AddChild(label);
				}
				else
				{
					label.Visible = true;
				}
			}
			else
			{
				label = FloatingLabelScene.Instantiate<FloatingLabel>();
				scene.CurrentScene.AddChild(label);
			}

			label.GlobalPosition = worldPos + new Vector2(0, -20);
			label.Init(damage, isCrit, isPlayerDamage);
		}

		public static void SpawnGoldLabel(Vector2 worldPos, int amount)
		{
			if (FloatingLabelScene == null) return;
			var scene = Engine.GetMainLoop() as SceneTree;
			if (scene?.CurrentScene == null) return;

			FloatingLabel label;
			if (_pool.Count > 0)
			{
				label = _pool.Dequeue();
				if (!IsInstanceValid(label))
				{
					label = FloatingLabelScene.Instantiate<FloatingLabel>();
					scene.CurrentScene.AddChild(label);
				}
				else
				{
					label.Visible = true;
				}
			}
			else
			{
				label = FloatingLabelScene.Instantiate<FloatingLabel>();
				scene.CurrentScene.AddChild(label);
			}

			label.GlobalPosition = worldPos + new Vector2(0, -20);
			label.InitGold(amount);
		}

		public static void ReleaseFloatingLabel(FloatingLabel label)
		{
			if (!IsInstanceValid(label)) return;
			label.Visible = false;
			_pool.Enqueue(label);
		}

		// 히트 스톱(타격감용 짧은 시간정지) — Engine.TimeScale 글로벌 조작이라
		// 동시 호출 시 timer 복원이 엇갈리지 않게 _hitStopActive 가드로 중복 차단.
		// HUD/SettingsUI 같은 UI는 process_mode = Always(3)라 TimeScale 영향 안 받음.
		private static bool _hitStopActive = false;

		public static void HitStop(float duration = 0.06f, float scale = 0.05f)
		{
			if (_hitStopActive) return;
			if (Engine.GetMainLoop() is not SceneTree scene) return;
			_hitStopActive = true;
			Engine.TimeScale = scale;
			var timer = scene.CreateTimer(duration, true, false, true);
			timer.Timeout += () =>
			{
				Engine.TimeScale = 1.0;
				_hitStopActive = false;
			};
		}

		private static bool IsInstanceValid(GodotObject obj)
		{
			return obj != null && GodotObject.IsInstanceValid(obj);
		}
	}
}
