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
		public event Action<int> OnStatPointsChanged;

		[Export] public int BaseDamage { get; set; } = 10;
		[Export] public float AttackRange { get; set; } = 80.0f;

		// ─── 레벨/경험치 ─────────────────────────────────────────────
		private int _level = 1;
		public int Level => _level;

		private int _exp = 0;
		public int Exp => _exp;

		private int _statPoints = 0;
		private int _strPoints = 0;
		private int _conPoints = 0;
		private int _intPoints = 0;

		public int StatPoints => _statPoints;
		public int StrPoints => _strPoints;
		public int ConPoints => _conPoints;
		public int IntPoints => _intPoints;

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
			_statPoints += 3;
			OnStatPointsChanged?.Invoke(_statPoints);
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

		// ─── 스탯 포인트 ─────────────────────────────────────────────
		public bool AllocateStat(string stat)
		{
			if (_statPoints <= 0) return false;
			switch (stat.ToUpperInvariant())
			{
				case "STR": _strPoints++; BaseDamage += 2; break;
				case "CON": _conPoints++; MaxHealth += 5; CurrentHealth = Mathf.Min(CurrentHealth + 5, MaxHealth); break;
				case "INT": _intPoints++; MaxMp += 3; CurrentMp = Mathf.Min(CurrentMp + 3, MaxMp); break;
				default: return false;
			}
			_statPoints--;
			OnStatPointsChanged?.Invoke(_statPoints);
			return true;
		}

		public void SetStatPointsFromSave(int sp, int str, int con, int intel)
		{
			_statPoints = sp;
			_strPoints = str;
			_conPoints = con;
			_intPoints = intel;
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
