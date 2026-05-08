using System.Collections.Generic;
using System.Linq;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Entities.Player
{
	public partial class PlayerController
	{
		// ─── ISaveable 구현 ─────────────────────────────────────────
		public void WriteSaveData(SaveData data)
		{
			data.PlayerPosX = GlobalPosition.X;
			data.PlayerPosY = GlobalPosition.Y;

			// 월드 상태
			data.CurrentScene = GetTree().CurrentScene.SceneFilePath;
			data.DefeatedBosses = GameManager.Instance?.DefeatedBosses?.ToList() ?? new();
			data.PlayerHealth = Stats.CurrentHealth;
			data.PlayerMaxHealth = Stats.MaxHealth;
			data.PlayerMp = Stats.CurrentMp;
			data.PlayerLevel = Stats.Level;
			data.PlayerExp = Stats.Exp;
			data.StatPoints = Stats.StatPoints;
			data.StrPoints = Stats.StrPoints;
			data.ConPoints = Stats.ConPoints;
			data.IntPoints = Stats.IntPoints;

			// 인벤토리
			data.InventoryItems = new List<SavedItemSlot>();
			foreach (var invSlot in Inventory.Slots)
			{
				if (invSlot.Item == null || string.IsNullOrEmpty(invSlot.Item.ResourcePath))
					continue;
				data.InventoryItems.Add(new SavedItemSlot
				{
					ItemPath = invSlot.Item.ResourcePath,
					Quantity = invSlot.Quantity,
					EnhancementLevel = invSlot.EnhancementLevel
				});
			}

			if (Inventory.EquippedWeapon != null)
				data.EquippedWeaponPath = Inventory.EquippedWeapon.ResourcePath;
			if (Inventory.EquippedArmor != null)
				data.EquippedArmorPath = Inventory.EquippedArmor.ResourcePath;
			if (Inventory.EquippedAccessory != null)
				data.EquippedAccessoryPath = Inventory.EquippedAccessory.ResourcePath;

			// 장비 강화 수치
			data.EquippedWeaponEnhancement = Inventory.EquippedWeaponEnhancement;
			data.EquippedArmorEnhancement = Inventory.EquippedArmorEnhancement;
			data.EquippedAccessoryEnhancement = Inventory.EquippedAccessoryEnhancement;

			// 신규 부위별 슬롯 (v4)
			data.EquippedHelmetPath = Inventory.EquippedHelmet?.ResourcePath ?? "";
			data.EquippedBootsPath = Inventory.EquippedBoots?.ResourcePath ?? "";
			data.EquippedNecklacePath = Inventory.EquippedNecklace?.ResourcePath ?? "";
			data.EquippedRing1Path = Inventory.EquippedRing1?.ResourcePath ?? "";
			data.EquippedRing2Path = Inventory.EquippedRing2?.ResourcePath ?? "";
			data.EquippedBraceletPath = Inventory.EquippedBracelet?.ResourcePath ?? "";

			// 퀘스트 상태
			var qm = GameManager.Instance?.QuestManager;
			if (qm != null)
			{
				data.CurrentQuestPath = qm.ActiveQuestPath;
				data.QuestKillProgress = qm.Progress;
				data.CompletedQuestIds = new List<string>(qm.CompletedQuestIds);
			}

			// 퀵슬롯
			data.QuickSlotPaths = new List<string>();
			foreach (var item in Inventory.QuickSlots)
			{
				data.QuickSlotPaths.Add(item != null ? item.ResourcePath : "");
			}

			// 스킬
			data.LearnedSkillPaths = new List<string>();
			foreach (var skill in Stats.LearnedSkills)
			{
				if (!string.IsNullOrEmpty(skill.ResourcePath))
					data.LearnedSkillPaths.Add(skill.ResourcePath);
			}
		}

		public void ReadSaveData(SaveData data)
		{
			LoadFromSaveData(data);
		}
	}
}
