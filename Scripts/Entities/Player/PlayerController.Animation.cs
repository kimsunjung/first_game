using Godot;
using FirstGame.Core;
using FirstGame.Data;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController
	{
		// ─── 애니메이션 설정 (Kenney 정적 타일 + 프로그래밍 효과) ────
		private void SetupAnimations()
		{
			_animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (_animSprite == null) { GD.PrintErr("PlayerController: AnimatedSprite2D 없음"); return; }

			var frames = new SpriteFrames();
			if (frames.HasAnimation("default")) frames.RemoveAnimation("default");

			var tilemap = AnimationHelper.KenneyTilemap;
			if (tilemap == null) { GD.PrintErr("PlayerController: Kenney 타일맵 로드 실패"); return; }

			int ts = KenneyTiles.TileSize;

			// 단일 프레임 애니메이션 등록 (방향 없이 1종씩)
			AnimationHelper.AddSingleTileAnimation(frames, "idle", tilemap, KenneyTiles.CharPlayer, ts, 6, true);
			AnimationHelper.AddSingleTileAnimation(frames, "walk", tilemap, KenneyTiles.CharPlayer, ts, 6, true);
			AnimationHelper.AddSingleTileAnimation(frames, "attack", tilemap, KenneyTiles.CharPlayer, ts, 6, false);
			AnimationHelper.AddSingleTileAnimation(frames, "hit", tilemap, KenneyTiles.CharPlayer, ts, 6, false);
			AnimationHelper.AddSingleTileAnimation(frames, "death", tilemap, KenneyTiles.CharPlayer, ts, 6, false);

			_animSprite.SpriteFrames = frames;
			_animSprite.Play("idle");
			_animSprite.AnimationFinished += OnAnimationFinished;
		}

		// ─── 방향 (8방향 물리 + FlipH 표시) ────────────────────────
		private void UpdateFlipH()
		{
			if (_animSprite == null) return;
			if (Mathf.Abs(_facingDirection.X) > 0.1f)
				_animSprite.FlipH = _facingDirection.X < 0;
		}

		// ─── 애니메이션 업데이트 ────────────────────────────────────
		private void UpdateAnimation()
		{
			if (_animSprite == null || _isAnimLocked) return;
			UpdateFlipH();

			bool isMoving = Velocity != Vector2.Zero;
			string target = isMoving ? "walk" : "idle";
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

			UpdateFlipH();

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
				// 공격 후 짧은 경직
				GetTree().CreateTimer(0.1).Timeout += () =>
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

			// 흰색 플래시 + 흔들림 (기존 코드 패턴 유지)
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
			// 사망 후 처리는 기존 로직에서 담당
		}

		// ─── 애니메이션 완료 콜백 ───────────────────────────────────
		private void OnAnimationFinished()
		{
			// Kenney 정적 타일은 1프레임이므로 즉시 완료됨
			// 실제 애니메이션 타이밍은 Tween으로 제어
		}

		// ─── 하위 호환 (GetDirectionSuffix 제거됨) ──────────────────
		// Combat.cs 등에서 _facingDirection을 직접 사용
	}
}
