using Godot;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;
using FirstGame.Data.Skills;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController
	{
		// ─── 스킬 쿨다운 ────────────────────────────────────────────
		private void UpdateSkillCooldowns(double delta)
		{
			// Dictionary는 enumerate 중 수정 불가 — 키 스냅샷 후 갱신
			var keys = new System.Collections.Generic.List<SkillType>(_skillCooldowns.Keys);
			foreach (var key in keys)
			{
				if (_skillCooldowns[key] > 0f)
					_skillCooldowns[key] = Mathf.Max(0f, _skillCooldowns[key] - (float)delta);
			}
		}

		private void UseSkillSlot(int slot)
		{
			var skills = Stats.LearnedSkills;
			if (slot >= skills.Count) { GD.Print($"슬롯 {slot + 1}에 스킬이 없습니다."); return; }

			var skill = skills[slot];
			float remaining = _skillCooldowns.TryGetValue(skill.Type, out var v) ? v : 0f;
			if (remaining > 0f) { GD.Print($"{skill.SkillName} 쿨타임: {remaining:F1}s"); return; }
			if (Stats.CurrentMp < skill.MpCost) { GD.Print("MP 부족!"); return; }

			Stats.CurrentMp -= skill.MpCost;
			_skillCooldowns[skill.Type] = skill.Cooldown;
			ActivateSkill(skill);
		}

		private void ActivateSkill(SkillData skill)
		{
			var strategy = SkillStrategyFactory.Create(skill.Type);
			strategy?.Execute((ISkillTarget)this, skill);
			AudioManager.Instance?.PlaySFX("skill_activate.wav");
		}

		// 모바일 스킬 버튼에서 직접 호출
		public void TriggerSkill(int slot) => UseSkillSlot(slot);

		// 모바일 UI용 쿨타임/MP 정보 조회 — 슬롯 인덱스를 SkillType으로 변환 후 조회
		public float GetSkillCooldownRemaining(int slot)
		{
			var skills = Stats.LearnedSkills;
			if (slot >= skills.Count) return 0f;
			return _skillCooldowns.TryGetValue(skills[slot].Type, out var v) ? v : 0f;
		}
		public float GetSkillMaxCooldown(int slot)
		{
			var skills = Stats.LearnedSkills;
			return slot < skills.Count ? skills[slot].Cooldown : 0f;
		}
		public int GetSkillMpCost(int slot)
		{
			var skills = Stats.LearnedSkills;
			return slot < skills.Count ? skills[slot].MpCost : 0;
		}
		public bool HasSkillInSlot(int slot) => slot < Stats.LearnedSkills.Count;

		// ISkillTarget 메서드 (Strategy에서 호출)
		private Tween _powerStrikeTween;
		public void SetPowerStrikeActive(bool active)
		{
			_powerStrikeActive = active;
			UpdatePowerStrikeVisual();
		}

		private void UpdatePowerStrikeVisual()
		{
			if (_animSprite == null) return;

			if (_powerStrikeTween != null && _powerStrikeTween.IsValid())
				_powerStrikeTween.Kill();

			if (_powerStrikeActive)
			{
				// 황금색 펄스 — 강화된 공격 준비 상태를 시각화
				_powerStrikeTween = CreateTween().SetLoops();
				_powerStrikeTween.TweenProperty(_animSprite, "modulate",
					new Color(1.4f, 1.3f, 0.5f, 1f), 0.25f);
				_powerStrikeTween.TweenProperty(_animSprite, "modulate",
					Colors.White, 0.25f);
			}
			else
			{
				_animSprite.Modulate = Colors.White;
			}
		}
		public void ActivateDash(float duration)
		{
			_dashActive = true;
			_dashTimer = duration;
		}
	}
}
