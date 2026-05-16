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
		public void ModifyAttackSpeed(float delta) => AttackSpeed += delta;
		// 상태이상 저항 가감 — raw 누적값 유지(장비+버프 합산이 0.85를 넘어도 정보 손실 없음).
		// 실제 면역 상한 clamp(0~0.85)은 사용 시점(CharacterStats.ApplyStatus)에서만 적용해
		// 버프 만료 시 전체 차감이 장비 저항을 깎는 비대칭 버그를 방지한다.
		public void ModifyStatusResist(float delta) => StatusResist += delta;

		// 무게 페널티: 0~80% 정상, 80~100% -10% 이속, 100%+ -30% 이속
		public float WeightPenaltyMultiplier { get; private set; } = 1.0f;
		private bool _overweightWarned = false;

		public void UpdateWeightPenalty(float currentWeight, float maxWeight)
		{
			float ratio = maxWeight > 0f ? currentWeight / maxWeight : 0f;
			float newMul;
			if (ratio >= 1.0f) newMul = 0.7f;
			else if (ratio >= 0.8f) newMul = 0.9f;
			else newMul = 1.0f;

			if (newMul != WeightPenaltyMultiplier)
			{
				WeightPenaltyMultiplier = newMul;
				if (newMul < 1.0f && !_overweightWarned)
				{
					_overweightWarned = true;
					GD.Print("[무게] 과적! 이동속도가 감소합니다.");
				}
				else if (newMul == 1.0f)
				{
					_overweightWarned = false;
				}
			}
		}

		// 임시 buff 추적 — duration초 동안 누적 적용, 만료 시 자동 차감.
		// PlayerController._PhysicsProcess가 TickBuffs(delta) 호출해 timer 진행.
		private float _buffMoveSpeedAmount = 0f;
		private float _buffMoveSpeedRemaining = 0f;
		private float _buffAttackSpeedAmount = 0f;
		private float _buffAttackSpeedRemaining = 0f;
		private int _buffDamageAmount = 0;
		private float _buffDamageRemaining = 0f;
		private int _buffDefenseAmount = 0;
		private float _buffDefenseRemaining = 0f;
		private float _buffCritAmount = 0f;
		private float _buffCritRemaining = 0f;
		private float _buffResistAmount = 0f;
		private float _buffResistRemaining = 0f;

		public void ApplyBuff(float moveDelta, float atkDelta, float durationSec)
			=> ApplyBuffEx(moveDelta, atkDelta, 0, 0, 0f, durationSec);

		public void ApplyBuffEx(float moveDelta, float atkDelta, int dmgDelta, int defDelta, float critDelta, float durationSec, float resistDelta = 0f)
		{
			if (durationSec <= 0f) return;
			// 중복 buff 적용 시 기존 효과를 먼저 제거(누적 차단) → 새 값/시간으로 갱신.
			if (_buffMoveSpeedRemaining > 0f) MoveSpeed -= _buffMoveSpeedAmount;
			if (_buffAttackSpeedRemaining > 0f) AttackSpeed -= _buffAttackSpeedAmount;
			if (_buffDamageRemaining > 0f) BaseDamage -= _buffDamageAmount;
			if (_buffDefenseRemaining > 0f) Defense -= _buffDefenseAmount;
			if (_buffCritRemaining > 0f) CritRate -= _buffCritAmount;
			if (_buffResistRemaining > 0f) ModifyStatusResist(-_buffResistAmount);
			_buffMoveSpeedAmount = moveDelta;
			_buffAttackSpeedAmount = atkDelta;
			_buffDamageAmount = dmgDelta;
			_buffDefenseAmount = defDelta;
			_buffCritAmount = critDelta;
			_buffResistAmount = resistDelta;
			_buffMoveSpeedRemaining   = moveDelta != 0f ? durationSec : 0f;
			_buffAttackSpeedRemaining = atkDelta  != 0f ? durationSec : 0f;
			_buffDamageRemaining      = dmgDelta  != 0  ? durationSec : 0f;
			_buffDefenseRemaining     = defDelta  != 0  ? durationSec : 0f;
			_buffCritRemaining        = critDelta != 0f ? durationSec : 0f;
			_buffResistRemaining      = resistDelta != 0f ? durationSec : 0f;
			MoveSpeed   += moveDelta;
			AttackSpeed += atkDelta;
			BaseDamage  += dmgDelta;
			Defense     += defDelta;
			CritRate    += critDelta;
			if (resistDelta != 0f) ModifyStatusResist(resistDelta);
		}

		public void TickBuffs(float delta)
		{
			if (_manaShieldRemaining > 0f)
			{
				_manaShieldRemaining -= delta;
				if (_manaShieldRemaining <= 0f) IsManaShieldActive = false;
			}
			if (_buffMoveSpeedRemaining > 0f)
			{
				_buffMoveSpeedRemaining -= delta;
				if (_buffMoveSpeedRemaining <= 0f)
				{
					MoveSpeed -= _buffMoveSpeedAmount;
					_buffMoveSpeedAmount = 0f;
				}
			}
			if (_buffAttackSpeedRemaining > 0f)
			{
				_buffAttackSpeedRemaining -= delta;
				if (_buffAttackSpeedRemaining <= 0f)
				{
					AttackSpeed -= _buffAttackSpeedAmount;
					_buffAttackSpeedAmount = 0f;
				}
			}
			if (_buffDamageRemaining > 0f)
			{
				_buffDamageRemaining -= delta;
				if (_buffDamageRemaining <= 0f) { BaseDamage -= _buffDamageAmount; _buffDamageAmount = 0; }
			}
			if (_buffDefenseRemaining > 0f)
			{
				_buffDefenseRemaining -= delta;
				if (_buffDefenseRemaining <= 0f) { Defense -= _buffDefenseAmount; _buffDefenseAmount = 0; }
			}
			if (_buffCritRemaining > 0f)
			{
				_buffCritRemaining -= delta;
				if (_buffCritRemaining <= 0f) { CritRate -= _buffCritAmount; _buffCritAmount = 0f; }
			}
			if (_buffResistRemaining > 0f)
			{
				_buffResistRemaining -= delta;
				if (_buffResistRemaining <= 0f) { ModifyStatusResist(-_buffResistAmount); _buffResistAmount = 0f; }
			}
		}
		public void Heal(int amount) => CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
		public void RestoreMp(int amount) => CurrentMp = Math.Min(CurrentMp + amount, MaxMp);

		// ─── ManaShield ─────────────────────────────────────────────
		public bool IsManaShieldActive { get; private set; } = false;
		private float _manaShieldRemaining = 0f;

		public void ActivateManaShield(float duration)
		{
			IsManaShieldActive = true;
			_manaShieldRemaining = Mathf.Max(_manaShieldRemaining, duration);
		}

		// ─── IEquipTarget.CureStatuses 명시 구현 ─────────────────────
		void Core.Interfaces.IEquipTarget.CureStatuses(int mask) => CureStatuses(mask);

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

		// Lifesteal 누적 — 두 소스 분리해야 ApplyPassiveBonuses 재호출 시 장비 보너스가 깨지지 않음.
		// _passive: 학습한 패시브 스킬 (ApplyPassiveBonuses가 매 로드 재계산)
		// _equip: 장비/affix (Inventory.ApplyItemBonuses/ApplyAffixBonuses가 += / -=)
		private float _passiveLifesteal = 0f;
		private float _equipLifesteal = 0f;
		public float LifestealPercent => _passiveLifesteal + _equipLifesteal;

		// 패시브 HP 재생 — 장비 보너스 없어 분리 불필요.
		public float PassiveHpRegenPerSec { get; private set; } = 0f;

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
			AttackSpeed = 1.0f;    // CharacterStats 베이스 — 장비/affix가 += 누적
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

		// 능동 스킬만 (IsPassive=false). 슬롯 인덱스는 이 목록 기준으로 계산.
		public int ActiveSkillCount
		{
			get { int n = 0; foreach (var s in _learnedSkills) if (!s.IsPassive) n++; return n; }
		}
		public SkillData GetActiveSkillAt(int slot)
		{
			int n = 0;
			foreach (var s in _learnedSkills)
			{
				if (s.IsPassive) continue;
				if (n == slot) return s;
				n++;
			}
			return null;
		}

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
			// 패시브 스킬은 학습 즉시 효과 적용 — 능동 슬롯에는 잡히지 않음.
			if (skill.IsPassive) ApplySinglePassive(skill);
			GD.Print($"스킬 습득: {skill.SkillName}!");
			return true;
		}

		// 패시브 한 개 적용 — LearnSkill 정상 경로와 ApplyPassiveBonuses 재적용 양쪽에서 공유.
		private void ApplySinglePassive(SkillData skill)
		{
			switch (skill.PassiveKind)
			{
				case PassiveType.Lifesteal:  _passiveLifesteal += skill.PassiveValue; break;
				case PassiveType.HpRegen:    PassiveHpRegenPerSec += skill.PassiveValue; break;
				case PassiveType.CritBoost:  CritRate += skill.PassiveValue; break;
				case PassiveType.SpeedBoost: MoveSpeed += skill.PassiveValue; break;
			}
		}

		/// <summary>로드 시 SetLevelFromSave가 베이스 리셋 후 호출 — 학습된 패시브 효과 재적용.
		/// _passiveLifesteal/PassiveHpRegenPerSec만 리셋 — _equipLifesteal은 장비 경로에서 관리.</summary>
		public void ApplyPassiveBonuses()
		{
			_passiveLifesteal = 0f;
			PassiveHpRegenPerSec = 0f;
			foreach (var sk in _learnedSkills)
				if (sk != null && sk.IsPassive) ApplySinglePassive(sk);
		}

		// 장비/affix 흡수 누적/감소 (Inventory.ApplyItemBonuses + ApplyAffixBonuses 경로).
		public void ModifyLifesteal(float delta) => _equipLifesteal += delta;

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
