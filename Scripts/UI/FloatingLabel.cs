using Godot;
using FirstGame.Core;

namespace FirstGame.UI
{
	public partial class FloatingLabel : Node2D
	{
		public void Init(int damage, bool isCrit, bool isPlayerDamage = false)
		{
			Visible = true;

			var label = GetNode<Label>("Label");
			label.Modulate = new Color(label.Modulate, 1f); // 알파 복원

			if (isCrit)
			{
				label.Text = $"CRIT! {damage}";
				label.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0f));
				label.AddThemeFontSizeOverride("font_size", 18);
			}
			else if (isPlayerDamage)
			{
				label.Text = damage.ToString();
				label.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.3f));
				label.AddThemeFontSizeOverride("font_size", 14);
			}
			else
			{
				label.Text = damage.ToString();
				label.AddThemeColorOverride("font_color", Colors.White);
				label.AddThemeFontSizeOverride("font_size", 14);
			}

			float randX = (float)GD.RandRange(-20.0, 20.0);
			var tween = CreateTween();
			tween.SetParallel(true);
			tween.TweenProperty(this, "position", Position + new Vector2(randX, -60f), 0.8f)
				 .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
			tween.TweenProperty(label, "modulate:a", 0.0f, 0.8f)
				 .SetDelay(0.3f);
			tween.Chain().TweenCallback(Callable.From(ReturnToPool));
		}

		private void ReturnToPool()
		{
			UIEffectManager.ReleaseFloatingLabel(this);
		}
	}
}
