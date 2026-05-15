using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class CharacterStats : Resource
	{
		// ─── 상태이상 (Poison/Freeze/Curse) ─────────────────────────
		public StatusEffect ActiveStatuses { get; private set; } = StatusEffect.None;
		private float _poisonTimer = 0f;
		private float _poisonTickAccum = 0f;
		private float _freezeTimer = 0f;
		private float _curseTimer = 0f;

		public bool HasStatus(StatusEffect status) => (ActiveStatuses & status) != 0;

		public void ApplyStatus(StatusEffect status, float duration)
		{
			if ((status & StatusEffect.Poison) != 0)
			{ ActiveStatuses |= StatusEffect.Poison; _poisonTimer = Mathf.Max(_poisonTimer, duration); }
			if ((status & StatusEffect.Freeze) != 0)
			{ ActiveStatuses |= StatusEffect.Freeze; _freezeTimer = Mathf.Max(_freezeTimer, duration); }
			if ((status & StatusEffect.Curse) != 0)
			{ ActiveStatuses |= StatusEffect.Curse; _curseTimer = Mathf.Max(_curseTimer, duration); }
		}

		public void CureStatuses(int mask)
		{
			if ((mask & (int)StatusEffect.Poison) != 0) { ActiveStatuses &= ~StatusEffect.Poison; _poisonTimer = 0f; _poisonTickAccum = 0f; }
			if ((mask & (int)StatusEffect.Freeze) != 0) { ActiveStatuses &= ~StatusEffect.Freeze; _freezeTimer = 0f; }
			if ((mask & (int)StatusEffect.Curse)  != 0) { ActiveStatuses &= ~StatusEffect.Curse;  _curseTimer  = 0f; }
		}

		// 반환값: 이번 tick에 발생한 독 데미지 (호출자가 방어 우회 직접 적용).
		public int TickStatuses(float delta)
		{
			int poisonDmg = 0;
			if (_poisonTimer > 0f)
			{
				_poisonTimer -= delta;
				_poisonTickAccum += 3f * delta;
				if (_poisonTickAccum >= 1f) { poisonDmg = (int)_poisonTickAccum; _poisonTickAccum -= poisonDmg; }
				if (_poisonTimer <= 0f) { ActiveStatuses &= ~StatusEffect.Poison; _poisonTickAccum = 0f; }
			}
			if (_freezeTimer > 0f)
			{
				_freezeTimer -= delta;
				if (_freezeTimer <= 0f) ActiveStatuses &= ~StatusEffect.Freeze;
			}
			if (_curseTimer > 0f)
			{
				_curseTimer -= delta;
				if (_curseTimer <= 0f) ActiveStatuses &= ~StatusEffect.Curse;
			}
			return poisonDmg;
		}

		// 상태에 맞는 색조 반환 (정상=흰색, 독=초록, 빙결=파랑, 저주=보라).
		public Color GetStatusModulate()
		{
			if (HasStatus(StatusEffect.Poison)) return new Color(0.4f, 1f, 0.4f);
			if (HasStatus(StatusEffect.Freeze)) return new Color(0.5f, 0.75f, 1f);
			if (HasStatus(StatusEffect.Curse))  return new Color(0.75f, 0.4f, 1f);
			return new Color(1f, 1f, 1f);
		}

		// ─── 이벤트 ────────────────────────────────────────────────
		public event Action<int, int> OnHealthChanged; // (현재, 최대)
		public event Action<int, int> OnMpChanged;     // (현재, 최대)

		// ─── 크리티컬 ──────────────────────────────────────────────
		[Export] public float CritRate { get; set; } = 0.1f;         // 10% 기본 크리트 확률
		[Export] public float CritMultiplier { get; set; } = 2.0f;   // 2배 크리트 데미지

		// ─── 기본 스탯 ─────────────────────────────────────────────
		public virtual float MoveSpeed { get; set; } = 120.0f;
		[Export] public int   MaxHealth { get; set; } = 100;
		[Export] public int   Defense { get; set; } = 0;
		// 공격속도 배수 — 1.0이 베이스. 1.10이면 공격 cooldown 10% 단축(10% 더 빠름).
		// 장비/affix/buff가 += 0.05 식으로 누적.
		public float AttackSpeed { get; set; } = 1.0f;

		private int _currentHealth = 100;
		[Export]
		public int CurrentHealth
		{
			get => _currentHealth;
			set
			{
				_currentHealth = Mathf.Clamp(value, 0, MaxHealth);
				OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
			}
		}

		// ─── MP ────────────────────────────────────────────────────
		[Export] public int MaxMp { get; set; } = 50;

		private int _currentMp = 50;
		[Export]
		public int CurrentMp
		{
			get => _currentMp;
			set
			{
				_currentMp = Mathf.Clamp(value, 0, MaxMp);
				OnMpChanged?.Invoke(_currentMp, MaxMp);
			}
		}
	}
}
