using Godot;
using System;

namespace FirstGame.Data
{
	[GlobalClass]
	public partial class CharacterStats : Resource
	{
        // Event: current, max
		public event Action<int, int> OnHealthChanged;

		[Export] public float MoveSpeed { get; set; } = 300.0f;
        [Export] public float AttackRange { get; set; } = 80.0f; // Added for attack logic
		[Export] public int MaxHealth { get; set; } = 100;
        
        private int _currentHealth = 100;
		[Export] 
        public int CurrentHealth 
        { 
            get => _currentHealth;
            set
            {
                // Clamp health between 0 and MaxHealth
                _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
                OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
            }
        }
	}
}
