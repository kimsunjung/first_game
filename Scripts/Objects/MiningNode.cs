using Godot;
using FirstGame.Core;
using FirstGame.Data;
using FirstGame.Entities;

namespace FirstGame.Objects
{
	public partial class MiningNode : BaseInteractable
	{
		[Export] public ItemData OreItem { get; set; }
		[Export] public int Quantity { get; set; } = 1;
		/// <summary>씬 재진입 시 채광 상태에서 부활할 확률(0~1). 0이면 영구 비활성 — 강화석 같은 희귀 자원용.</summary>
		[Export] public float RespawnChanceOnReentry { get; set; } = 0f;

		// NodeId 형식: "{SceneFilePath}/{NodeName}" — 씬 안에서 노드명이 고유하므로 충돌 없음.
		// 별도 Export 없이 자동 추출 — 씬에서 노드명 바꾸면 저장 데이터가 깨지지만 광맥은 stable.
		private string GetNodeId() => $"{GetTree().CurrentScene.SceneFilePath}/{Name}";

		protected override void OnReady()
		{
			if (GameManager.Instance != null && GameManager.Instance.IsNodeMined(GetNodeId()))
			{
				// 광종별 부활 — RespawnChance > 0이면 씬 재진입 시 확률로 다시 활성.
				// 부활 결정 시 MinedNodes에서 즉시 제거 → 다음 자동저장에 영속화.
				if (RespawnChanceOnReentry > 0f && GD.Randf() < RespawnChanceOnReentry)
				{
					GameManager.Instance.UnmineNode(GetNodeId());
					// 정상 흐름으로 진행 — 라벨 세팅 후 채광 가능 상태.
				}
				else
				{
					QueueFree();
					return;
				}
			}

			var label = GetNodeOrNull<Label>("PromptLabel");
			if (label != null && OreItem != null)
				label.Text = $"[F] {OreItem.ItemName} 채광";

			// 광종별 Sprite2D는 OreItem.Icon에서 자동 추출 — .tscn texture override 불필요.
			var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
			if (sprite != null && OreItem?.Icon != null)
				sprite.Texture = OreItem.Icon;
		}

		protected override void OnInteract()
		{
			if (OreItem == null) return;
			var inv = GameManager.Instance?.Player?.Inventory;
			if (inv == null) return;

			// 가방이 꽉 차면 노드를 그대로 두고 알림만 — 인벤 정리 후 재시도 가능.
			if (!inv.CanAddItem(OreItem, Quantity))
			{
				AudioManager.Instance?.PlaySFX("error.wav");
				GD.Print("가방이 가득 차서 채광할 수 없습니다.");
				return;
			}

			// 트랜잭션 격리 — AddItem이 OnInventoryChanged → RequestAutoSave를 트리거해
			// "아이템은 들어왔지만 채광 기록은 아직 없는" 중간 상태가 디스크에 박히는 결함 차단.
			// 강화석 같은 희귀 자원이 OS kill 직후 부활하는 race를 막는다.
			using (GameTransaction.Begin())
			{
				inv.AddItem(OreItem, Quantity);
				GameManager.Instance.RecordNodeMined(GetNodeId());
			}
			SaveManager.SaveGame();

			AudioManager.Instance?.PlaySFX("pickup.wav");
			QueueFree();
		}
	}
}
