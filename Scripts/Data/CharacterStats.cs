using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class CharacterStats : Resource
	{
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
