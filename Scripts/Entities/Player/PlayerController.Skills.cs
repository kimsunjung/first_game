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
			for (int i = 0; i < _skillCooldowns.Length; i++)
			{
				if (_skillCooldowns[i] > 0f)
					_skillCooldowns[i] = Mathf.Max(0f, _skillCooldowns[i] - (float)delta);
			}
		}

		private void UseSkillSlot(int slot)
		{
			var skills = Stats.LearnedSkills;
			if (slot >= skills.Count) { GD.Print($"슬롯 {slot + 1}에 스킬이 없습니다."); return; }

			var skill = skills[slot];
			if (_skillCooldowns[slot] > 0f) { GD.Print($"{skill.SkillName} 쿨타임: {_skillCooldowns[slot]:F1}s"); return; }
			if (Stats.CurrentMp < skill.MpCost) { GD.Print("MP 부족!"); return; }

			Stats.CurrentMp -= skill.MpCost;
			_skillCooldowns[slot] = skill.Cooldown;
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

		// 모바일 UI용 쿨타임/MP 정보 조회
		public float GetSkillCooldownRemaining(int slot)
			=> slot < _skillCooldowns.Length ? _skillCooldowns[slot] : 0f;
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
		public void SetPowerStrikeActive(bool active) => _powerStrikeActive = active;
		public void ActivateDash(float duration)
		{
			_dashActive = true;
			_dashTimer = duration;
		}
	}
}
