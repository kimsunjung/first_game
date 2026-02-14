using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class CharacterStats : Resource
	{
        // 이벤트: 현재체력, 최대체력 (Event: current, max)
		public event Action<int, int> OnHealthChanged;

		[Export] public float MoveSpeed { get; set; } = 300.0f;
		[Export] public int MaxHealth { get; set; } = 100;
        
        private int _currentHealth = 100;
		[Export] 
        public int CurrentHealth 
        { 
            get => _currentHealth;
            set
            {
                // 체력을 0과 최대 체력 사이로 제한 (Clamp health between 0 and MaxHealth)
                _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
                OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
            }
        }
	}
}
