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

		public static void ReleaseFloatingLabel(FloatingLabel label)
		{
			if (!IsInstanceValid(label)) return;
			label.Visible = false;
			_pool.Enqueue(label);
		}

		private static bool IsInstanceValid(GodotObject obj)
		{
			return obj != null && GodotObject.IsInstanceValid(obj);
		}
	}
}
