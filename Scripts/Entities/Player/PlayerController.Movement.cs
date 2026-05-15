using Godot;
using FirstGame.Core;
using FirstGame.UI;

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

			// 수동 입력 (키보드 + 가상 조이스틱)
			Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
			if (inputDir == Vector2.Zero)
				inputDir = VirtualInput.JoystickDirection;

			float freezeMul = (Stats?.HasStatus(FirstGame.Data.StatusEffect.Freeze) == true) ? 0.5f : 1.0f;

			if (_dashActive && _dashForcedDir != Vector2.Zero)
			{
				// BackstepShot 강제 대시 — 입력과 무관하게 지정 방향으로 이동
				float forcedSpeed = Stats.MoveSpeed * FirstGame.Core.BalanceData.Movement.PlayerSpeedMultiplier * DashSpeedMultiplier;
				Velocity = _dashForcedDir * forcedSpeed;
			}
			else if (inputDir != Vector2.Zero)
			{
				inputDir = inputDir.Normalized();
				// 무게 페널티 업데이트 (매 이동 프레임마다 — 아이템 pick/drop 즉시 반영)
				Stats.UpdateWeightPenalty(Inventory.CurrentWeight, Data.Inventory.GetMaxWeight());
				float speed = Stats.MoveSpeed * FirstGame.Core.BalanceData.Movement.PlayerSpeedMultiplier
					* Stats.WeightPenaltyMultiplier * freezeMul
					* (_dashActive ? DashSpeedMultiplier : 1.0f);
				Velocity = Velocity.MoveToward(inputDir * speed, Acceleration * (float)delta);
				_facingDirection = inputDir;
				// 이동 시 UI 포커스 해제 (방향키가 슬롯 포커스에 영향주는 것 방지)
				GetViewport().GuiReleaseFocus();
			}
			else
			{
				Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
			}

			if (Input.IsActionJustPressed("attack"))
			{
				// 공격 시 UI 포커스 해제 (Space가 포커스된 버튼 클릭하는 것 방지)
				GetViewport().GuiReleaseFocus();
				Attack();
			}
		}

		private bool IsMoving() => Velocity.Length() > 20f;

		// ─── 대시 ───────────────────────────────────────────────────
		private void UpdateDash(double delta)
		{
			if (!_dashActive) return;
			_dashTimer -= (float)delta;
			if (_dashTimer <= 0f) { _dashActive = false; _dashForcedDir = Vector2.Zero; }
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

		// ─── HP 재생 (비전투 시 baseline + 패시브는 전투 중에도) ─────
		private void RegenHp(double delta)
		{
			if (IsDead || Stats.CurrentHealth >= Stats.MaxHealth) return;
			float perSec = Stats.PassiveHpRegenPerSec; // 패시브는 항상 적용
			double elapsed = Time.GetTicksMsec() / 1000.0 - _lastDamageTime;
			if (elapsed >= BalanceData.Regen.HpDelayAfterHit)
				perSec += BalanceData.Regen.HpPerSec; // baseline은 피격 후 일정 시간 경과 시
			if (perSec <= 0f) return;
			_hpRegenAccum += perSec * (float)delta;
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
