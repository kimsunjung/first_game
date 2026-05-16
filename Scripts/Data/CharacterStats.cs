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
		// HUD 게이지용 — 각 상태가 마지막으로 부여된 시점의 지속시간(분모).
		private float _poisonMax = 1f, _freezeMax = 1f, _curseMax = 1f, _burnMax = 1f, _shockMax = 1f;

		// 상태이상 저항(0~1). 들어오는 부정 상태이상을 이 확률로 완전 무효화.
		// 장비/버프가 올려줄 수 있는 일반 메커니즘 — ApplyStatus 진입에서 1회 판정.
		public float StatusResist { get; set; } = 0f;

		public bool HasStatus(StatusEffect status) => (ActiveStatuses & status) != 0;

		public void ApplyStatus(StatusEffect status, float duration)
		{
			if (status == StatusEffect.None || duration <= 0f) return;
			// 저항 판정 — 성공 시 부여 자체를 무효화.
			float resist = Mathf.Clamp(StatusResist, 0f, 0.85f);
			if (resist > 0f && GD.Randf() < resist) return;

			// 분모(_xMax)는 "현재 적용 기준 남은 시간" = 갱신된 timer 값.
			// 과거 최대치를 누적하지 않으므로 짧은 재적용도 HUD가 100%에서 시작한다.
			if ((status & StatusEffect.Poison) != 0)
			{ ActiveStatuses |= StatusEffect.Poison; _poisonTimer = Mathf.Max(_poisonTimer, duration); _poisonMax = _poisonTimer; }
			if ((status & StatusEffect.Freeze) != 0)
			{ ActiveStatuses |= StatusEffect.Freeze; _freezeTimer = Mathf.Max(_freezeTimer, duration); _freezeMax = _freezeTimer; }
			if ((status & StatusEffect.Curse) != 0)
			{ ActiveStatuses |= StatusEffect.Curse; _curseTimer = Mathf.Max(_curseTimer, duration); _curseMax = _curseTimer; }
			if ((status & StatusEffect.Burn) != 0)
			{ ActiveStatuses |= StatusEffect.Burn; _burnTimer = Mathf.Max(_burnTimer, duration); _burnMax = _burnTimer; }
			if ((status & StatusEffect.Shock) != 0)
			{ ActiveStatuses |= StatusEffect.Shock; _shockTimer = Mathf.Max(_shockTimer, duration); _shockMax = _shockTimer; }
		}

		// HUD 표시용 — 현재 활성 상태와 남은 비율(0~1). 우선순위 색조 순서와 무관하게 전부 반환.
		public System.Collections.Generic.List<(StatusEffect kind, float frac, float remain)> GetActiveStatusBars()
		{
			var list = new System.Collections.Generic.List<(StatusEffect, float, float)>(5);
			if (_poisonTimer > 0f) list.Add((StatusEffect.Poison, Mathf.Clamp(_poisonTimer / _poisonMax, 0f, 1f), _poisonTimer));
			if (_burnTimer   > 0f) list.Add((StatusEffect.Burn,   Mathf.Clamp(_burnTimer   / _burnMax,   0f, 1f), _burnTimer));
			if (_freezeTimer > 0f) list.Add((StatusEffect.Freeze, Mathf.Clamp(_freezeTimer / _freezeMax, 0f, 1f), _freezeTimer));
			if (_shockTimer  > 0f) list.Add((StatusEffect.Shock,  Mathf.Clamp(_shockTimer  / _shockMax,  0f, 1f), _shockTimer));
			if (_curseTimer  > 0f) list.Add((StatusEffect.Curse,  Mathf.Clamp(_curseTimer  / _curseMax,  0f, 1f), _curseTimer));
			return list;
		}

		public void CureStatuses(int mask)
		{
			if ((mask & (int)StatusEffect.Poison) != 0) { ActiveStatuses &= ~StatusEffect.Poison; _poisonTimer = 0f; _poisonTickAccum = 0f; _poisonMax = 1f; }
			if ((mask & (int)StatusEffect.Freeze) != 0) { ActiveStatuses &= ~StatusEffect.Freeze; _freezeTimer = 0f; _freezeMax = 1f; }
			if ((mask & (int)StatusEffect.Curse)  != 0) { ActiveStatuses &= ~StatusEffect.Curse;  _curseTimer  = 0f; _curseMax = 1f; }
			if ((mask & (int)StatusEffect.Burn)   != 0) { ActiveStatuses &= ~StatusEffect.Burn;   _burnTimer   = 0f; _burnTickAccum = 0f; _burnMax = 1f; }
			if ((mask & (int)StatusEffect.Shock)  != 0) { ActiveStatuses &= ~StatusEffect.Shock;  _shockTimer  = 0f; _shockMax = 1f; }
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
				if (_poisonTimer <= 0f) { ActiveStatuses &= ~StatusEffect.Poison; _poisonTickAccum = 0f; _poisonMax = 1f; }
			}
			if (_burnTimer > 0f)
			{
				_burnTimer -= delta;
				_burnTickAccum += 5f * delta;
				if (_burnTickAccum >= 1f) { int d = (int)_burnTickAccum; dotDmg += d; _burnTickAccum -= d; }
				if (_burnTimer <= 0f) { ActiveStatuses &= ~StatusEffect.Burn; _burnTickAccum = 0f; _burnMax = 1f; }
			}
			if (_freezeTimer > 0f)
			{
				_freezeTimer -= delta;
				if (_freezeTimer <= 0f) { ActiveStatuses &= ~StatusEffect.Freeze; _freezeMax = 1f; }
			}
			if (_curseTimer > 0f)
			{
				_curseTimer -= delta;
				if (_curseTimer <= 0f) { ActiveStatuses &= ~StatusEffect.Curse; _curseMax = 1f; }
			}
			if (_shockTimer > 0f)
			{
				_shockTimer -= delta;
				if (_shockTimer <= 0f) { ActiveStatuses &= ~StatusEffect.Shock; _shockMax = 1f; }
			}
			return dotDmg;
		}

		// 단일 상태 → 대표 색 (HUD 칩/게이지용). GetStatusModulate 색과 일치.
		public static Color StatusColor(StatusEffect s) => s switch
		{
			StatusEffect.Burn   => new Color(1f, 0.55f, 0.25f),
			StatusEffect.Freeze => new Color(0.5f, 0.75f, 1f),
			StatusEffect.Shock  => new Color(1f, 0.95f, 0.35f),
			StatusEffect.Poison => new Color(0.4f, 1f, 0.4f),
			StatusEffect.Curse  => new Color(0.75f, 0.4f, 1f),
			_ => new Color(1f, 1f, 1f),
		};

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
