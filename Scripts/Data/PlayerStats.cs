using Godot;
using System;
using System.Collections.Generic;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class PlayerStats : CharacterStats
	{
		public event Action<int> OnLevelUp;
		public event Action<int, int> OnExpChanged;

		[Export] public int BaseDamage { get; set; } = 10;
		[Export] public float AttackRange { get; set; } = 80.0f;

		// ─── 레벨/경험치 ─────────────────────────────────────────────
		private int _level = 1;
		public int Level => _level;

		private int _exp = 0;
		public int Exp => _exp;

		public int ExpToNextLevel => (int)(100 * Math.Pow(_level, 1.5));

		public void AddExp(int amount)
		{
			if (_level >= 50) return;
			_exp += amount;
			OnExpChanged?.Invoke(_exp, ExpToNextLevel);

			while (_exp >= ExpToNextLevel && _level < 50)
			{
				_exp -= ExpToNextLevel;
				_level++;
				ApplyLevelUpBonus();
				OnLevelUp?.Invoke(_level);
				OnExpChanged?.Invoke(_exp, ExpToNextLevel);
			}
		}

		private void ApplyLevelUpBonus()
		{
			MaxHealth += 10;
			CurrentHealth = MaxHealth;
			BaseDamage += 2;
			MaxMp += 5;
			CurrentMp = MaxMp;
		}

		public void SetLevelFromSave(int level, int exp)
		{
			_level = Mathf.Clamp(level, 1, 50);
			int levelsGained = _level - 1;
			MaxHealth = 100 + levelsGained * 10;
			BaseDamage = 10 + levelsGained * 2;
			MaxMp = 50 + levelsGained * 5;
			_exp = Mathf.Max(0, exp);
		}

		// ─── 스킬 시스템 ─────────────────────────────────────────────
		private readonly List<SkillData> _learnedSkills = new();
		public IReadOnlyList<SkillData> LearnedSkills => _learnedSkills;

		public bool LearnSkill(SkillData skill)
		{
			if (skill == null) return false;
			if (_level < skill.RequiredLevel)
			{
				GD.Print($"레벨 {skill.RequiredLevel} 이상이 필요합니다! (현재: Lv.{_level})");
				return false;
			}
			if (_learnedSkills.Exists(s => s.Type == skill.Type))
			{
				GD.Print($"{skill.SkillName}은 이미 배운 스킬입니다.");
				return false;
			}
			_learnedSkills.Add(skill);
			GD.Print($"스킬 습득: {skill.SkillName}!");
			return true;
		}

		public bool HasSkill(SkillType type) =>
			_learnedSkills.Exists(s => s.Type == type);

		public void LoadLearnedSkills(List<SkillData> skills)
		{
			_learnedSkills.Clear();
			if (skills != null)
				_learnedSkills.AddRange(skills);
		}
	}
}
