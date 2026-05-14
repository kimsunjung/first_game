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
			data.DayTime = FirstGame.Core.DayNightCycle.NormalizedTime;
			data.GameDay = FirstGame.Core.DayNightCycle.Day;

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
			data.DexPoints = Stats.DexPoints;
			data.PlayerClassId = (int)Stats.PlayerClass;

			// 인벤토리 — 슬롯별 affix까지 함께 직렬화 (장신구 인스턴스 보존).
			data.InventoryItems = new List<SavedItemSlot>();
			foreach (var invSlot in Inventory.Slots)
			{
				if (invSlot.Item == null || string.IsNullOrEmpty(invSlot.Item.ResourcePath))
					continue;
				data.InventoryItems.Add(new SavedItemSlot
				{
					ItemPath = invSlot.Item.ResourcePath,
					Quantity = invSlot.Quantity,
					EnhancementLevel = invSlot.EnhancementLevel,
					Affixes = invSlot.Affixes != null ? new List<ItemAffix>(invSlot.Affixes) : new List<ItemAffix>(),
					IsEquipped = invSlot.IsEquipped
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

			// 장신구 슬롯의 affix
			data.EquippedNecklaceAffixes = new List<ItemAffix>(Inventory.EquippedNecklaceAffixes);
			data.EquippedRing1Affixes    = new List<ItemAffix>(Inventory.EquippedRing1Affixes);
			data.EquippedRing2Affixes    = new List<ItemAffix>(Inventory.EquippedRing2Affixes);
			data.EquippedBraceletAffixes = new List<ItemAffix>(Inventory.EquippedBraceletAffixes);

			// v11: 망토/벨트/장갑 슬롯
			data.EquippedCloakPath   = Inventory.EquippedCloak?.ResourcePath   ?? "";
			data.EquippedBeltPath    = Inventory.EquippedBelt?.ResourcePath     ?? "";
			data.EquippedGlovesPath  = Inventory.EquippedGloves?.ResourcePath   ?? "";
			data.EquippedCloakAffixes  = new List<ItemAffix>(Inventory.EquippedCloakAffixes);
			data.EquippedBeltAffixes   = new List<ItemAffix>(Inventory.EquippedBeltAffixes);
			data.EquippedGlovesAffixes = new List<ItemAffix>(Inventory.EquippedGlovesAffixes);

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

			// 절차적 필드맵 seed (씬 경로 → seed)
			var gm = GameManager.Instance;
			if (gm != null)
			{
				data.FieldSeeds = new Dictionary<string, int>();
				foreach (var kv in gm.FieldSeeds)
					data.FieldSeeds[kv.Key] = kv.Value;

				// 방문 이력 — 텔레포트 NPC 활성화 기준.
				data.VisitedScenes = new List<string>(gm.VisitedScenes);
				// 현재 씬은 무조건 포함 (저장 직전 ChangeScene이 아직 발생하지 않았을 수도).
				string cur = GetTree().CurrentScene.SceneFilePath;
				if (!string.IsNullOrEmpty(cur) && !data.VisitedScenes.Contains(cur))
					data.VisitedScenes.Add(cur);

				// 채광 완료된 광산 노드 — 씬 재진입 시 부활 차단.
				data.MinedNodes = new List<string>(gm.MinedNodes);

				// 메인 스토리 챕터 플래그.
				data.ChapterFlags = new List<string>(gm.ChapterFlags);
			}
		}

		public void ReadSaveData(SaveData data)
		{
			LoadFromSaveData(data);
		}
	}
}
