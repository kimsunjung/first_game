using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class CharacterStats : Resource
	{
		// ─── 상태이상 (Poison/Freeze/Curse/Burn/Shock) ─────────────────
		public StatusEffect ActiveStatuses { get; private set; } = StatusEffect.None;
		private float _poisonTimer = 0f;
		private float _poisonTickAccum = 0f;
		private float _freezeTimer = 0f;
		private float _curseTimer = 0f;
		private float _burnTimer = 0f;
		private float _burnTickAccum = 0f;
		private float _shockTimer = 0f;

		public bool HasStatus(StatusEffect status) => (ActiveStatuses & status) != 0;

		public void ApplyStatus(StatusEffect status, float duration)
		{
			if ((status & StatusEffect.Poison) != 0)
			{ ActiveStatuses |= StatusEffect.Poison; _poisonTimer = Mathf.Max(_poisonTimer, duration); }
			if ((status & StatusEffect.Freeze) != 0)
			{ ActiveStatuses |= StatusEffect.Freeze; _freezeTimer = Mathf.Max(_freezeTimer, duration); }
			if ((status & StatusEffect.Curse) != 0)
			{ ActiveStatuses |= StatusEffect.Curse; _curseTimer = Mathf.Max(_curseTimer, duration); }
			if ((status & StatusEffect.Burn) != 0)
			{ ActiveStatuses |= StatusEffect.Burn; _burnTimer = Mathf.Max(_burnTimer, duration); }
			if ((status & StatusEffect.Shock) != 0)
			{ ActiveStatuses |= StatusEffect.Shock; _shockTimer = Mathf.Max(_shockTimer, duration); }
		}

		public void CureStatuses(int mask)
		{
			if ((mask & (int)StatusEffect.Poison) != 0) { ActiveStatuses &= ~StatusEffect.Poison; _poisonTimer = 0f; _poisonTickAccum = 0f; }
			if ((mask & (int)StatusEffect.Freeze) != 0) { ActiveStatuses &= ~StatusEffect.Freeze; _freezeTimer = 0f; }
			if ((mask & (int)StatusEffect.Curse)  != 0) { ActiveStatuses &= ~StatusEffect.Curse;  _curseTimer  = 0f; }
			if ((mask & (int)StatusEffect.Burn)   != 0) { ActiveStatuses &= ~StatusEffect.Burn;   _burnTimer   = 0f; _burnTickAccum = 0f; }
			if ((mask & (int)StatusEffect.Shock)  != 0) { ActiveStatuses &= ~StatusEffect.Shock;  _shockTimer  = 0f; }
		}

		// 반환값: 이번 tick에 발생한 DOT 데미지 합 (Poison 3/sec + Burn 5/sec, 방어 무시).
		public int TickStatuses(float delta)
		{
			int dotDmg = 0;
			if (_poisonTimer > 0f)
			{
				_poisonTimer -= delta;
				_poisonTickAccum += 3f * delta;
				if (_poisonTickAccum >= 1f) { int d = (int)_poisonTickAccum; dotDmg += d; _poisonTickAccum -= d; }
				if (_poisonTimer <= 0f) { ActiveStatuses &= ~StatusEffect.Poison; _poisonTickAccum = 0f; }
			}
			if (_burnTimer > 0f)
			{
				_burnTimer -= delta;
				_burnTickAccum += 5f * delta;
				if (_burnTickAccum >= 1f) { int d = (int)_burnTickAccum; dotDmg += d; _burnTickAccum -= d; }
				if (_burnTimer <= 0f) { ActiveStatuses &= ~StatusEffect.Burn; _burnTickAccum = 0f; }
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
			if (_shockTimer > 0f)
			{
				_shockTimer -= delta;
				if (_shockTimer <= 0f) ActiveStatuses &= ~StatusEffect.Shock;
			}
			return dotDmg;
		}

		// 상태에 맞는 색조. 우선순위: Burn > Freeze > Shock > Poison > Curse.
		public Color GetStatusModulate()
		{
			if (HasStatus(StatusEffect.Burn))   return new Color(1f, 0.55f, 0.25f);  // 주황
			if (HasStatus(StatusEffect.Freeze)) return new Color(0.5f, 0.75f, 1f);   // 청
			if (HasStatus(StatusEffect.Shock))  return new Color(1f, 0.95f, 0.35f);  // 노랑
			if (HasStatus(StatusEffect.Poison)) return new Color(0.4f, 1f, 0.4f);    // 녹
			if (HasStatus(StatusEffect.Curse))  return new Color(0.75f, 0.4f, 1f);   // 보라
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
