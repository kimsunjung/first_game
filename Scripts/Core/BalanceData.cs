using Godot;
using System;
using System.Text.Json;

namespace FirstGame.Core
{
	public static class BalanceData
	{
		private const string BalancePath = "res://Resources/Balance/game_balance.json";

		public static EnhancementBalance Enhancement { get; private set; } = new();
		public static CombatBalance Combat { get; private set; } = new();
		public static RegenBalance Regen { get; private set; } = new();
		public static EnemyBalance Enemy { get; private set; } = new();
		public static SpawnerBalance Spawner { get; private set; } = new();
		public static ProgressionBalance Progression { get; private set; } = new();

		public static void Load()
		{
			try
			{
				using var file = FileAccess.Open(BalancePath, FileAccess.ModeFlags.Read);
				if (file == null)
				{
					GD.PrintErr($"BalanceData: 파일을 열 수 없음 ({BalancePath}), 기본값 사용");
					return;
				}

				string json = file.GetAsText();
				var doc = JsonDocument.Parse(json);
				var root = doc.RootElement;

				if (root.TryGetProperty("enhancement", out var enh))
					Enhancement = ParseEnhancement(enh);
				if (root.TryGetProperty("combat", out var cmb))
					Combat = ParseCombat(cmb);
				if (root.TryGetProperty("regen", out var reg))
					Regen = ParseRegen(reg);
				if (root.TryGetProperty("enemy", out var enm))
					Enemy = ParseEnemy(enm);
				if (root.TryGetProperty("spawner", out var spn))
					Spawner = ParseSpawner(spn);
				if (root.TryGetProperty("progression", out var prg))
					Progression = ParseProgression(prg);

				GD.Print("BalanceData: 밸런스 데이터 로드 완료");
			}
			catch (Exception e)
			{
				GD.PrintErr($"BalanceData: 로드 실패 ({e.Message}), 기본값 사용");
			}
		}

		private static float GetFloat(JsonElement el, string key, float def)
			=> el.TryGetProperty(key, out var v) ? (float)v.GetDouble() : def;
		private static int GetInt(JsonElement el, string key, int def)
			=> el.TryGetProperty(key, out var v) ? v.GetInt32() : def;

		private static EnhancementBalance ParseEnhancement(JsonElement el)
		{
			var b = new EnhancementBalance
			{
				MaxLevel = GetInt(el, "maxLevel", 10),
				CostBase = GetInt(el, "costBase", 100),
				NoPenaltyMaxLevel = GetInt(el, "noPenaltyMaxLevel", 5),
				DowngradeMaxLevel = GetInt(el, "downgradeMaxLevel", 8),
				DestroyMinLevel = GetInt(el, "destroyMinLevel", 9)
			};
			if (el.TryGetProperty("successRates", out var arr))
			{
				var rates = new float[arr.GetArrayLength()];
				for (int i = 0; i < rates.Length; i++)
					rates[i] = (float)arr[i].GetDouble();
				b.SuccessRates = rates;
			}
			if (el.TryGetProperty("enhanceMaterials", out var mats))
			{
				var matList = new EnhanceMaterialReq[mats.GetArrayLength()];
				for (int i = 0; i < matList.Length; i++)
				{
					var m = mats[i];
					matList[i] = new EnhanceMaterialReq
					{
						ItemPath = m.TryGetProperty("itemPath", out var p) ? p.GetString() : "",
						Quantity = m.TryGetProperty("quantity", out var q) ? q.GetInt32() : 1
					};
				}
				b.MaterialReqs = matList;
			}
			return b;
		}

		private static CombatBalance ParseCombat(JsonElement el) => new()
		{
			PlayerKnockback = GetFloat(el, "playerKnockback", 60f),
			EnemyKnockback = GetFloat(el, "enemyKnockback", 80f),
			AttackRangeRatio = GetFloat(el, "attackRangeRatio", 0.85f)
		};

		private static RegenBalance ParseRegen(JsonElement el) => new()
		{
			HpPerSec = GetFloat(el, "hpPerSec", 1.0f),
			MpPerSec = GetFloat(el, "mpPerSec", 2.0f),
			HpDelayAfterHit = GetFloat(el, "hpDelayAfterHit", 5.0f)
		};

		private static EnemyBalance ParseEnemy(JsonElement el) => new()
		{
			GoldMultiplier = GetInt(el, "goldMultiplier", 5),
			BossKillThreshold = GetInt(el, "bossKillThreshold", 10)
		};

		private static SpawnerBalance ParseSpawner(JsonElement el) => new()
		{
			MaxEnemies = GetInt(el, "maxEnemies", 5),
			SpawnInterval = GetFloat(el, "spawnInterval", 3.0f),
			SpawnRadius = GetFloat(el, "spawnRadius", 300f)
		};

		private static ProgressionBalance ParseProgression(JsonElement el) => new()
		{
			MaxLevel = GetInt(el, "maxLevel", 50),
			ExpBase = GetFloat(el, "expBase", 100.0f),
			ExpExponent = GetFloat(el, "expExponent", 1.5f),
			LvHpBonus = GetInt(el, "lvHpBonus", 10),
			LvAtkBonus = GetInt(el, "lvAtkBonus", 2),
			LvMpBonus = GetInt(el, "lvMpBonus", 5),
			LvStatPoints = GetInt(el, "lvStatPoints", 3),
			StrAtkBonus = GetInt(el, "strAtkBonus", 2),
			ConHpBonus = GetInt(el, "conHpBonus", 5),
			IntMpBonus = GetInt(el, "intMpBonus", 3)
		};
	}

	public class EnhancementBalance
	{
		public int MaxLevel { get; set; } = 10;
		public float[] SuccessRates { get; set; } = { 1f, 1f, 1f, 0.7f, 0.7f, 0.7f, 0.5f, 0.5f, 0.5f, 0.3f };
		public int CostBase { get; set; } = 100;
		public int NoPenaltyMaxLevel { get; set; } = 5;
		public int DowngradeMaxLevel { get; set; } = 8;
		public int DestroyMinLevel { get; set; } = 9;
		public EnhanceMaterialReq[] MaterialReqs { get; set; } = Array.Empty<EnhanceMaterialReq>();
	}

	public struct EnhanceMaterialReq
	{
		public string ItemPath { get; set; }
		public int Quantity { get; set; }
	}

	public class CombatBalance
	{
		public float PlayerKnockback { get; set; } = 60f;
		public float EnemyKnockback { get; set; } = 80f;
		public float AttackRangeRatio { get; set; } = 0.85f;
	}

	public class RegenBalance
	{
		public float HpPerSec { get; set; } = 1.0f;
		public float MpPerSec { get; set; } = 2.0f;
		public float HpDelayAfterHit { get; set; } = 5.0f;
	}

	public class EnemyBalance
	{
		public int GoldMultiplier { get; set; } = 5;
		public int BossKillThreshold { get; set; } = 10;
	}

	public class SpawnerBalance
	{
		public int MaxEnemies { get; set; } = 5;
		public float SpawnInterval { get; set; } = 3.0f;
		public float SpawnRadius { get; set; } = 300f;
	}

	public class ProgressionBalance
	{
		public int MaxLevel { get; set; } = 50;
		public float ExpBase { get; set; } = 100.0f;
		public float ExpExponent { get; set; } = 1.5f;
		public int LvHpBonus { get; set; } = 10;
		public int LvAtkBonus { get; set; } = 2;
		public int LvMpBonus { get; set; } = 5;
		public int LvStatPoints { get; set; } = 3;
		public int StrAtkBonus { get; set; } = 2;
		public int ConHpBonus { get; set; } = 5;
		public int IntMpBonus { get; set; } = 3;
	}
}
