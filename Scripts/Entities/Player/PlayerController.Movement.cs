using Godot;
using FirstGame.Core;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController
	{
		// ─── 이동 입력 ──────────────────────────────────────────────
		private void GetInput(double delta)
		{
			if (IsDead || _isAnimLocked)
			{
				Velocity = Vector2.Zero;
				return;
			}

			// 수동 입력
			Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");

			if (inputDir != Vector2.Zero)
			{
				inputDir = inputDir.Normalized();
				float speed = Stats.MoveSpeed * (_dashActive ? DashSpeedMultiplier : 1.0f);
				Velocity = Velocity.MoveToward(inputDir * speed, Acceleration * (float)delta);
				_facingDirection = inputDir;
			}
			else
			{
				Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
			}

			if (Input.IsActionJustPressed("attack")) Attack();
		}

		private bool IsMoving() => Velocity.Length() > 20f;

		// ─── 대시 ───────────────────────────────────────────────────
		private void UpdateDash(double delta)
		{
			if (!_dashActive) return;
			_dashTimer -= (float)delta;
			if (_dashTimer <= 0f) _dashActive = false;
		}

		// ─── MP 재생 ────────────────────────────────────────────────
		private void RegenMp(double delta)
		{
			if (Stats.CurrentMp >= Stats.MaxMp) return;
			_mpRegenAccum += BalanceData.Regen.MpPerSec * (float)delta;
			if (_mpRegenAccum >= 1f)
			{
				int regen = (int)_mpRegenAccum;
				_mpRegenAccum -= regen;
				Stats.CurrentMp += regen;
			}
		}

		// ─── HP 재생 (비전투 시) ─────────────────────────────────────
		private void RegenHp(double delta)
		{
			if (IsDead || Stats.CurrentHealth >= Stats.MaxHealth) return;
			double elapsed = Time.GetTicksMsec() / 1000.0 - _lastDamageTime;
			if (elapsed < BalanceData.Regen.HpDelayAfterHit) return;
			_hpRegenAccum += BalanceData.Regen.HpPerSec * (float)delta;
			if (_hpRegenAccum >= 1f)
			{
				int regen = (int)_hpRegenAccum;
				_hpRegenAccum -= regen;
				Stats.CurrentHealth += regen;
			}
		}

		// ─── 넉백 감쇠 ─────────────────────────────────────────────
		private void ApplyKnockbackDecay(double delta)
		{
			if (_knockbackVelocity.LengthSquared() < 25f)
			{
				_knockbackVelocity = Vector2.Zero;
				return;
			}
			Velocity += _knockbackVelocity;
			_knockbackVelocity = _knockbackVelocity.MoveToward(Vector2.Zero, 800f * (float)delta);
		}
	}
}
