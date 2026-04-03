using Godot;
using System;
using System.Collections.Generic;
using FirstGame.Core;
using FirstGame.Core.Interfaces;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class PlayerStats : CharacterStats, IEquipTarget
	{
		public event Action<int> OnLevelUp;
		public event Action<int, int> OnExpChanged;
		public event Action<int> OnStatPointsChanged;

		[Export] public int BaseDamage { get; set; } = 10;
		[Export] public float AttackRange { get; set; } = 80.0f;

		// ─── 밸런스 (BalanceData.Progression에서 로드) ──────────────
		private static int MaxLevel => BalanceData.Progression.MaxLevel;
		private static float ExpBaseMultiplier => BalanceData.Progression.ExpBase;
		private static float ExpPowerExponent => BalanceData.Progression.ExpExponent;
		private static int LevelUpHealthBonus => BalanceData.Progression.LvHpBonus;
		private static int LevelUpDamageBonus => BalanceData.Progression.LvAtkBonus;
		private static int LevelUpMpBonus => BalanceData.Progression.LvMpBonus;
		private static int LevelUpStatPoints => BalanceData.Progression.LvStatPoints;
		private static int StrDamageBonus => BalanceData.Progression.StrAtkBonus;
		private static int ConHealthBonus => BalanceData.Progression.ConHpBonus;
		private static int IntMpBonus => BalanceData.Progression.IntMpBonus;

		// ─── IEquipTarget 구현 ───────────────────────────────────────
		public void ModifyBaseDamage(int delta) => BaseDamage += delta;
		public void ModifyMaxHealth(int delta)
		{
			MaxHealth += delta;
			if (delta > 0) CurrentHealth += delta;
			else if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
		}
		public void ModifyDefense(int delta) => Defense += delta;
		public void Heal(int amount) => CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);

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

		public int ExpToNextLevel => (int)(ExpBaseMultiplier * Math.Pow(_level, ExpPowerExponent));

		public void AddExp(int amount)
		{
			if (_level >= MaxLevel) return;
			_exp += amount;
			OnExpChanged?.Invoke(_exp, ExpToNextLevel);

			while (_exp >= ExpToNextLevel && _level < MaxLevel)
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
			MaxHealth += LevelUpHealthBonus;
			CurrentHealth = MaxHealth;
			BaseDamage += LevelUpDamageBonus;
			MaxMp += LevelUpMpBonus;
			CurrentMp = MaxMp;
			_statPoints += LevelUpStatPoints;
			OnStatPointsChanged?.Invoke(_statPoints);
		}

		public void SetLevelFromSave(int level, int exp)
		{
			_level = Mathf.Clamp(level, 1, MaxLevel);
			int levelsGained = _level - 1;
			MaxHealth = 100 + levelsGained * LevelUpHealthBonus;
			BaseDamage = 10 + levelsGained * LevelUpDamageBonus;
			MaxMp = 50 + levelsGained * LevelUpMpBonus;
			_exp = Mathf.Max(0, exp);
		}

		// ─── 스탯 포인트 ─────────────────────────────────────────────
		public bool AllocateStat(string stat)
		{
			if (_statPoints <= 0) return false;
			switch (stat.ToUpperInvariant())
			{
				case "STR": _strPoints++; BaseDamage += StrDamageBonus; break;
				case "CON": _conPoints++; MaxHealth += ConHealthBonus; CurrentHealth = Mathf.Min(CurrentHealth + ConHealthBonus, MaxHealth); break;
				case "INT": _intPoints++; MaxMp += IntMpBonus; CurrentMp = Mathf.Min(CurrentMp + IntMpBonus, MaxMp); break;
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
