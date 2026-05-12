using Godot;
using FirstGame.Core;
using FirstGame.Data;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController
	{
		private const string PlayerFramesPath = "res://Resources/Generated/GPT/Characters/Player/player_sprite_frames.tres";

		// ─── 애니메이션 설정 (GPT 4방향 스프라이트 시트) ──────────────
		private void SetupAnimations()
		{
			_animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (_animSprite == null) { GD.PrintErr("PlayerController: AnimatedSprite2D 없음"); return; }

			var frames = GD.Load<SpriteFrames>(PlayerFramesPath);
			if (frames == null) { GD.PrintErr($"PlayerController: SpriteFrames 로드 실패 - {PlayerFramesPath}"); return; }

			_animSprite.SpriteFrames = frames;
			_animSprite.Play("idle_down");
			_animSprite.AnimationFinished += OnAnimationFinished;
		}

		// 대각선 입력 시 X/Y 우세 축으로 4방향 중 하나 선택. 정확히 같으면 X 우선.
		private string GetDirSuffix()
		{
			Vector2 d = _facingDirection;
			if (Mathf.Abs(d.X) >= Mathf.Abs(d.Y))
				return d.X >= 0 ? "right" : "left";
			return d.Y > 0 ? "down" : "up";
		}

		// ─── 애니메이션 업데이트 ────────────────────────────────────
		private void UpdateAnimation()
		{
			if (_animSprite == null || _isAnimLocked) return;

			bool isMoving = Velocity != Vector2.Zero;
			string dir = GetDirSuffix();
			string target = isMoving ? $"walk_{dir}" : $"idle_{dir}";
			PlayAnim(target);

			// 걷기 바운스 효과
			if (isMoving && (_walkBounceTween == null || !_walkBounceTween.IsRunning()))
			{
				StartWalkBounce();
			}
			else if (!isMoving && _walkBounceTween != null && _walkBounceTween.IsRunning())
			{
				_walkBounceTween.Kill();
				_walkBounceTween = null;
				if (_animSprite != null)
					_animSprite.Position = new Vector2(_animSprite.Position.X, 0);
			}
		}

		private void StartWalkBounce()
		{
			if (_animSprite == null) return;
			_walkBounceTween?.Kill();
			_walkBounceTween = CreateTween();
			_walkBounceTween.SetLoops();
			_walkBounceTween.TweenProperty(_animSprite, "position:y", -1.5f, 0.15f)
				.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
			_walkBounceTween.TweenProperty(_animSprite, "position:y", 0f, 0.15f)
				.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		}

		private void PlayAnim(string animName)
		{
			if (_animSprite != null && _animSprite.Animation != animName)
			{
				if (_animSprite.SpriteFrames.HasAnimation(animName))
					_animSprite.Play(animName);
			}
		}

		// ─── 공격 애니메이션 (attack_<dir> 시트 재생) ──────────────
		private void PlayAttackAnimation()
		{
			if (_animSprite == null) return;
			_isAnimLocked = true;
			Velocity = Vector2.Zero;
			_walkBounceTween?.Kill();
			// 바운스 효과로 position이 어긋난 상태에서 attack 들어올 수 있어 원점 복귀
			_animSprite.Position = Vector2.Zero;

			string dir = GetDirSuffix();
			string animName = $"attack_{dir}";
			if (_animSprite.SpriteFrames != null && _animSprite.SpriteFrames.HasAnimation(animName))
			{
				_animSprite.Play(animName);
				// loop=false라 끝나면 OnAnimationFinished에서 lock 해제
			}
			else
			{
				// fallback: 애니 없으면 짧은 timer 후 lock 해제
				GetTree().CreateTimer(0.25).Timeout += () =>
				{
					_isAnimLocked = false;
					UpdateAnimation();
				};
			}
		}

		// ─── 피격 애니메이션 ────────────────────────────────────────
		private void PlayHitAnimation()
		{
			if (_animSprite == null || IsDead) return;
			_isAnimLocked = true;
			_isHitFlashing = true;

			_animSprite.Modulate = new Color(10f, 10f, 10f, 1f);
			var hitTween = CreateTween();
			hitTween.TweenProperty(_animSprite, "position:x", 3f, 0.03f);
			hitTween.TweenProperty(_animSprite, "position:x", -3f, 0.03f);
			hitTween.TweenProperty(_animSprite, "position:x", 2f, 0.03f);
			hitTween.TweenProperty(_animSprite, "position:x", 0f, 0.03f);
			hitTween.TweenProperty(_animSprite, "modulate", new Color(1f, 1f, 1f, 1f), 0.08f);
			hitTween.TweenCallback(Callable.From(() =>
			{
				_isHitFlashing = false;
				_isAnimLocked = false;
				UpdateAnimation();
			}));
		}

		// ─── 사망 애니메이션 ────────────────────────────────────────
		private void PlayDeathAnimation()
		{
			if (_animSprite == null) return;
			_isAnimLocked = true;
			_walkBounceTween?.Kill();

			var tween = CreateTween();
			tween.TweenProperty(_animSprite, "modulate:a", 0f, 0.5f);
			tween.Parallel().TweenProperty(_animSprite, "scale", new Vector2(0.3f, 0.3f), 0.5f);
		}

		// ─── 애니메이션 완료 콜백 ───────────────────────────────────
		private void OnAnimationFinished()
		{
			// attack_<dir>만 loop=false. 끝나면 lock 풀고 walk/idle로 복귀.
			// 단, 피격 Tween(_isHitFlashing) 또는 사망 중에는 lock 유지 — 해당 콜백이 풀어줌.
			if (IsDead || _isHitFlashing) return;
			string anim = _animSprite?.Animation.ToString() ?? "";
			if (anim.StartsWith("attack_"))
			{
				_isAnimLocked = false;
				UpdateAnimation();
			}
		}
	}
}
