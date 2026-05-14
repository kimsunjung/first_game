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
		private static float StrDamageBonus => BalanceData.Progression.StrAtkBonus;
		private static float ConHealthBonus => BalanceData.Progression.ConHpBonus;
		private static float IntMpBonus => BalanceData.Progression.IntMpBonus;
		// DEX(궁수 핵심) — 공격력·크리·이속에 동시에 영향. balance json에서 로드.
		private static float DexDamageBonus => BalanceData.Progression.DexAtkBonus;
		private static float DexCritBonus => BalanceData.Progression.DexCritBonus;
		private static float DexSpeedBonus => BalanceData.Progression.DexSpeedBonus;

		// ─── IEquipTarget 구현 ───────────────────────────────────────
		public void ModifyBaseDamage(int delta) => BaseDamage += delta;
		public void ModifyMaxHealth(int delta)
		{
			MaxHealth += delta;
			if (delta > 0) CurrentHealth += delta;
			else if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
		}
		public void ModifyDefense(int delta) => Defense += delta;
		public void ModifyMaxMp(int delta)
		{
			MaxMp += delta;
			if (delta > 0) CurrentMp += delta;
			else if (CurrentMp > MaxMp) CurrentMp = MaxMp;
		}
		public void ModifyCritRate(float delta) => CritRate += delta;
		public void ModifyMoveSpeed(float delta) => MoveSpeed += delta;
		public void Heal(int amount) => CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
		public void RestoreMp(int amount) => CurrentMp = Math.Min(CurrentMp + amount, MaxMp);

		// ─── 레벨/경험치 ─────────────────────────────────────────────
		private int _level = 1;
		public int Level => _level;

		private int _exp = 0;
		public int Exp => _exp;

		private int _statPoints = 0;
		private int _strPoints = 0;
		private int _conPoints = 0;
		private int _intPoints = 0;
		private int _dexPoints = 0;

		public int StatPoints => _statPoints;
		public int StrPoints => _strPoints;
		public int ConPoints => _conPoints;
		public int IntPoints => _intPoints;
		public int DexPoints => _dexPoints;

		// 캐릭터 클래스 — 신규 게임 시 선택. 세이브에 정수로 저장. 기본 Warrior(전사).
		public PlayerClass PlayerClass { get; set; } = PlayerClass.Warrior;

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
			// 결정론적 리셋 — 매 로드마다 베이스로 되돌려 ApplyStatPointBonuses += 누적 차단.
			// CritRate/MoveSpeed/Defense는 레벨 보너스가 없으므로 베이스값으로 직접 복귀.
			MaxHealth = 100 + levelsGained * LevelUpHealthBonus;
			BaseDamage = 10 + levelsGained * LevelUpDamageBonus;
			MaxMp = 50 + levelsGained * LevelUpMpBonus;
			CritRate = 0.1f;       // CharacterStats 베이스 — affix/장비/DEX는 후속에 더해짐
			MoveSpeed = 120.0f;    // CharacterStats 베이스
			Defense = 0;           // 장비 보너스만 더해짐
			_exp = Mathf.Max(0, exp);
		}

		// ─── 스탯 포인트 ─────────────────────────────────────────────
		public bool AllocateStat(string stat)
		{
			if (_statPoints <= 0) return false;
			switch (stat.ToUpperInvariant())
			{
				case "STR":
					_strPoints++;
					int atkDelta = Mathf.RoundToInt(_strPoints * StrDamageBonus) - Mathf.RoundToInt((_strPoints - 1) * StrDamageBonus);
					BaseDamage += atkDelta;
					break;
				case "CON":
					_conPoints++;
					int hpDelta = Mathf.RoundToInt(_conPoints * ConHealthBonus) - Mathf.RoundToInt((_conPoints - 1) * ConHealthBonus);
					MaxHealth += hpDelta;
					CurrentHealth = Mathf.Min(CurrentHealth + hpDelta, MaxHealth);
					break;
				case "INT":
					_intPoints++;
					int mpDelta = Mathf.RoundToInt(_intPoints * IntMpBonus) - Mathf.RoundToInt((_intPoints - 1) * IntMpBonus);
					MaxMp += mpDelta;
					CurrentMp = Mathf.Min(CurrentMp + mpDelta, MaxMp);
					break;
				case "DEX":
					_dexPoints++;
					int dexAtkDelta = Mathf.RoundToInt(_dexPoints * DexDamageBonus) - Mathf.RoundToInt((_dexPoints - 1) * DexDamageBonus);
					BaseDamage += dexAtkDelta;
					CritRate += DexCritBonus;
					MoveSpeed += DexSpeedBonus;
					break;
				default: return false;
			}
			_statPoints--;
			OnStatPointsChanged?.Invoke(_statPoints);
			return true;
		}

		public void SetStatPointsFromSave(int sp, int str, int con, int intel, int dex = 0)
		{
			_statPoints = sp;
			_strPoints = str;
			_conPoints = con;
			_intPoints = intel;
			_dexPoints = dex;
		}

		/// <summary>로드 시 SetStatPointsFromSave 후 호출. STR/CON/INT/DEX 보너스를 스탯에 재적용한다.</summary>
		public void ApplyStatPointBonuses()
		{
			BaseDamage += Mathf.RoundToInt(_strPoints * StrDamageBonus);
			MaxHealth += Mathf.RoundToInt(_conPoints * ConHealthBonus);
			MaxMp += Mathf.RoundToInt(_intPoints * IntMpBonus);
			BaseDamage += Mathf.RoundToInt(_dexPoints * DexDamageBonus);
			CritRate += _dexPoints * DexCritBonus;
			MoveSpeed += _dexPoints * DexSpeedBonus;
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
			// 클래스 제한 — AvailableToAllClasses=true면 통과, 아니면 RequiredClass 일치 필수.
			if (!skill.AvailableToAllClasses && skill.RequiredClass != PlayerClass)
			{
				GD.Print($"{skill.SkillName}은 {PlayerClassUtil.DisplayName(skill.RequiredClass)} 전용 스킬입니다.");
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
			if (skills == null) return;
			// 직접 변조된 세이브가 다른 클래스 전용 스킬을 잔류시키는 케이스 차단.
			// 정상 경로(LearnSkill)는 이미 체크되므로 일관성 보장.
			foreach (var sk in skills)
			{
				if (sk == null) continue;
				if (!sk.AvailableToAllClasses && sk.RequiredClass != PlayerClass)
				{
					GD.PrintErr($"[Load] {sk.SkillName} 스킬이 {PlayerClassUtil.DisplayName(PlayerClass)} 클래스와 불일치 — 제거");
					continue;
				}
				_learnedSkills.Add(sk);
			}
		}

		/// <summary>스킬 슬롯 위치 교체 — Q/W/E/R 슬롯 순서를 사용자가 수동으로 재배치할 때 사용.</summary>
		public bool SwapSkillSlots(int indexA, int indexB)
		{
			if (indexA == indexB) return false;
			if (indexA < 0 || indexB < 0) return false;
			if (indexA >= _learnedSkills.Count || indexB >= _learnedSkills.Count) return false;

			(_learnedSkills[indexA], _learnedSkills[indexB]) = (_learnedSkills[indexB], _learnedSkills[indexA]);
			return true;
		}
	}
}
