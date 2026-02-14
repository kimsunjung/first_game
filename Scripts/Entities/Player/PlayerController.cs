using Godot;
using System;
using FirstGame.Data;
using FirstGame.Core;
using FirstGame.Core.Interfaces;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController : CharacterBody2D, IDamageable
	{
		[Export] public PlayerStats Stats { get; set; }
		public Inventory Inventory { get; private set; } // 인벤토리 추가 (Inventory added)

		public bool IsDead { get; private set; } = false;
		private Vector2 _facingDirection = Vector2.Right; // 바라보는 방향 (Facing direction)

		public void TakeDamage(int damage)
		{
			if (IsDead) return;

			Stats.CurrentHealth -= damage;
			GD.Print($"Player took {damage} damage. HP: {Stats.CurrentHealth}/{Stats.MaxHealth}");
			
			if (Stats.CurrentHealth <= 0)
			{
				Die();
			}
		}

		private void Die()
		{
			IsDead = true;
			GD.Print("플레이어 사망! (Player Died!)");
			EventManager.TriggerPlayerDeath();
			SetPhysicsProcess(false); // 이동 비활성화 (Disable movement)
		}

		public override void _Ready()
		{
			// Stats가 null이면 초기화 (Initialize if Stats is null)
			// 공유 문제 방지를 위해 복제 (Duplicate to prevent shared resource issues)
			if (Stats != null)
				Stats = (PlayerStats)Stats.Duplicate();
			else
				Stats = new PlayerStats();

			Inventory = new Inventory(); // 인벤토리 초기화 (Initialize Inventory)

			if (SaveManager.PendingLoadData != null)
			{
				// 저장 데이터 적용 (Apply loaded data)
				var data = SaveManager.PendingLoadData;
				GlobalPosition = new Vector2(data.PlayerPosX, data.PlayerPosY);
				Stats.MaxHealth = data.PlayerMaxHealth;    // MaxHealth 먼저 설정 (Set MaxHealth first)
				Stats.CurrentHealth = data.PlayerHealth;   // 그 다음 CurrentHealth (Then CurrentHealth)
				GameManager.Instance.PlayerGold = data.PlayerGold; // 골드 복원 (Restore Gold)

				// 인벤토리 복원 (Restore Inventory)
				if (data.InventoryItems != null)
				{
					foreach (var savedSlot in data.InventoryItems)
					{
						var item = GD.Load<ItemData>(savedSlot.ItemPath);
						if (item != null)
							Inventory.AddItem(item, savedSlot.Quantity);
					}
				}

				// 장비 복원 (스탯 보너스 없이 슬롯만 설정 - 저장된 MaxHealth/BaseDamage에 이미 포함됨)
				ItemData loadedWeapon = null;
				ItemData loadedArmor = null;

				if (!string.IsNullOrEmpty(data.EquippedWeaponPath))
					loadedWeapon = GD.Load<ItemData>(data.EquippedWeaponPath);
				if (!string.IsNullOrEmpty(data.EquippedArmorPath))
					loadedArmor = GD.Load<ItemData>(data.EquippedArmorPath);

				Inventory.RestoreEquipment(loadedWeapon, loadedArmor);

				SaveManager.PendingLoadData = null;        // 적용 후 초기화 (Reset after applying)
			}
			else if (!SaveManager.HasSave())
			{
				// 최초 실행 시 초기 자동저장 생성 (Create initial auto-save on first run)
				SaveManager.SaveGame();
			}

			IsDead = false; // 부활 시 초기화 (Reset on respawn)
			
			GD.Print("플레이어 초기화됨 (Player Initialized)");
		}

		public override void _PhysicsProcess(double delta)
		{
			GetInput();
			MoveAndSlide();
		}

		// 퀵슬롯 사용 (Use Quick Slots with 1-4 keys)
		public override void _UnhandledInput(InputEvent @event)
		{
			if (IsDead) return;
			if (@event is InputEventKey k && k.Pressed && !k.Echo)
			{
				var key = k.Keycode != Key.None ? k.Keycode : k.PhysicalKeycode;
				if (key == Key.Key1) Inventory.UseQuickSlot(0, this);
				else if (key == Key.Key2) Inventory.UseQuickSlot(1, this);
				else if (key == Key.Key3) Inventory.UseQuickSlot(2, this);
				else if (key == Key.Key4) Inventory.UseQuickSlot(3, this);
			}
		}

		private void GetInput()
		{
			if (IsDead) return;

			// 이동 (Movement)
			Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
			Velocity = inputDir * Stats.MoveSpeed;

			// 이동 시 바라보는 방향 업데이트 (Update facing direction when moving)
			if (inputDir != Vector2.Zero)
				_facingDirection = inputDir.Normalized();

			// 공격 (Attack)
			if (Input.IsActionJustPressed("attack"))
			{
				Attack();
			}
		}

		private void Attack()
		{
			GD.Print("플레이어 공격! (Player Attacked!)");

			var enemies = GetTree().GetNodesInGroup("Enemy");
			foreach (Node2D enemyNode in enemies)
			{
				if (enemyNode is IDamageable damageableEnemy && enemyNode is Node2D node)
				{
					float distance = GlobalPosition.DistanceTo(node.GlobalPosition);
					if (distance <= Stats.AttackRange)
					{
						// 바라보는 방향의 적만 공격 (120도 부채꼴)
						Vector2 dirToEnemy = (node.GlobalPosition - GlobalPosition).Normalized();
						float dot = _facingDirection.Dot(dirToEnemy);
						if (dot > 0.5f) // cos(60°) = 0.5 → 좌우 60도씩 총 120도 범위
						{
							damageableEnemy.TakeDamage(Stats.BaseDamage);
						}
					}
				}
			}
		}
	}
}
