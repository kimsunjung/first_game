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

		// ─── 공격 애니메이션 (프로그래밍 효과) ──────────────────────
		private void PlayAttackAnimation()
		{
			if (_animSprite == null) return;
			_isAnimLocked = true;
			Velocity = Vector2.Zero;
			_walkBounceTween?.Kill();

			// 공격 방향으로 lunge + scale 펄스
			Vector2 lungeDir = _facingDirection.Normalized() * 8f;
			var tween = CreateTween();
			tween.TweenProperty(_animSprite, "scale", new Vector2(1.3f, 1.3f), 0.07f);
			tween.Parallel().TweenProperty(_animSprite, "position",
				new Vector2(lungeDir.X, lungeDir.Y), 0.07f);
			tween.TweenProperty(_animSprite, "scale", Vector2.One, 0.08f);
			tween.Parallel().TweenProperty(_animSprite, "position", Vector2.Zero, 0.08f);
			tween.TweenCallback(Callable.From(() =>
			{
				// 공격 후 경직 (값을 늘려 공격 속도 조절)
				GetTree().CreateTimer(0.25).Timeout += () =>
				{
					_isAnimLocked = false;
					UpdateAnimation();
				};
			}));
		}

		// ─── 피격 애니메이션 ────────────────────────────────────────
		private void PlayHitAnimation()
		{
			if (_animSprite == null || IsDead) return;
			_isAnimLocked = true;

			_animSprite.Modulate = new Color(10f, 10f, 10f, 1f);
			var hitTween = CreateTween();
			hitTween.TweenProperty(_animSprite, "position:x", 3f, 0.03f);
			hitTween.TweenProperty(_animSprite, "position:x", -3f, 0.03f);
			hitTween.TweenProperty(_animSprite, "position:x", 2f, 0.03f);
			hitTween.TweenProperty(_animSprite, "position:x", 0f, 0.03f);
			hitTween.TweenProperty(_animSprite, "modulate", new Color(1f, 1f, 1f, 1f), 0.08f);
			hitTween.TweenCallback(Callable.From(() =>
			{
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
			// walk_<dir>는 loop라 완료 안 됨. 비전이 효과는 Tween으로 처리.
		}
	}
}
