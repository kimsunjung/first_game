using Godot;

namespace FirstGame.Core
{
	public static class AnimationHelper
	{
		/// <summary>
		/// 스프라이트 시트에서 프레임을 잘라 SpriteFrames에 애니메이션으로 등록합니다.
		/// </summary>
		public static void AddSheetAnimation(SpriteFrames frames, string animName, string fullPath, int frameCount, int fps, bool loop)
		{
			frames.AddAnimation(animName);
			frames.SetAnimationSpeed(animName, fps);
			frames.SetAnimationLoop(animName, loop);

			var texture = GD.Load<Texture2D>(fullPath);
			if (texture == null)
			{
				GD.PrintErr($"AnimationHelper: 텍스처 로드 실패 - {fullPath}");
				return;
			}

			int frameWidth = texture.GetWidth() / frameCount;
			int frameHeight = texture.GetHeight();

			for (int i = 0; i < frameCount; i++)
			{
				var atlas = new AtlasTexture();
				atlas.Atlas = texture;
				atlas.Region = new Rect2(i * frameWidth, 0, frameWidth, frameHeight);
				frames.AddFrame(animName, atlas);
			}
		}

		/// <summary>
		/// 타일맵 아틀라스에서 단일 타일을 잘라 1프레임 애니메이션으로 등록합니다.
		/// Kenney 타일셋처럼 애니메이션 시트가 없는 정적 스프라이트용.
		/// </summary>
		public static void AddSingleTileAnimation(SpriteFrames frames, string animName,
			Texture2D atlasTexture, Vector2I atlasCoords, int tileSize, int fps, bool loop)
		{
			frames.AddAnimation(animName);
			frames.SetAnimationSpeed(animName, fps);
			frames.SetAnimationLoop(animName, loop);

			var atlas = new AtlasTexture();
			atlas.Atlas = atlasTexture;
			atlas.Region = new Rect2(atlasCoords.X * tileSize, atlasCoords.Y * tileSize, tileSize, tileSize);
			frames.AddFrame(animName, atlas);
		}

		/// <summary>
		/// Kenney tilemap_packed.png 텍스처를 캐싱하여 반환합니다.
		/// </summary>
		private static Texture2D _kenneyTilemap;
		public static Texture2D KenneyTilemap
		{
			get
			{
				_kenneyTilemap ??= GD.Load<Texture2D>(KenneyTiles.TilemapPath);
				return _kenneyTilemap;
			}
		}
	}

	/// <summary>
	/// Kenney Tiny Town 타일맵 아틀라스 좌표 상수 정의.
	/// </summary>
	public static class KenneyTiles
	{
		public const string TilemapPath = "res://Resources/Kenney/Tilemap/tilemap_packed.png";
		public const int TileSize = 16;

		// ─── 지면 (Row 0) ──────────────────────────────────────────
		public static readonly Vector2I GrassA = new(0, 0);
		public static readonly Vector2I GrassB = new(1, 0);
		public static readonly Vector2I GrassFlower = new(2, 0);

		// ─── 돌바닥/길 (Row 2) ─────────────────────────────────────
		public static readonly Vector2I StoneA = new(0, 2);
		public static readonly Vector2I StoneB = new(1, 2);
		public static readonly Vector2I StoneC = new(2, 2);

		// ─── 물 (Row 8) ────────────────────────────────────────────
		public static readonly Vector2I WaterA = new(0, 8);
		public static readonly Vector2I WaterB = new(1, 8);
		public static readonly Vector2I WaterC = new(2, 8);

		// ─── 울타리/벽 (Row 3) ─────────────────────────────────────
		public static readonly Vector2I WoodFence = new(0, 3);
		public static readonly Vector2I WoodWallA = new(1, 3);
		public static readonly Vector2I WoodWallB = new(2, 3);

		// ─── 나무 (Rows 0-1, 2×2 멀티타일) ─────────────────────────
		// 초록 나무 1: 상단 (4,0)(5,0), 하단 (4,1)(5,1)
		public static readonly Vector2I TreeGreenTL = new(4, 0);
		public static readonly Vector2I TreeGreenTR = new(5, 0);
		public static readonly Vector2I TreeGreenBL = new(4, 1);
		public static readonly Vector2I TreeGreenBR = new(5, 1);
		// 초록 나무 2: (6,0)(7,0) / (6,1)(7,1)
		public static readonly Vector2I TreeGreen2TL = new(6, 0);
		public static readonly Vector2I TreeGreen2TR = new(7, 0);
		public static readonly Vector2I TreeGreen2BL = new(6, 1);
		public static readonly Vector2I TreeGreen2BR = new(7, 1);
		// 가을 나무: (8,0)(9,0) / (8,1)(9,1)
		public static readonly Vector2I TreeAutumnTL = new(8, 0);
		public static readonly Vector2I TreeAutumnTR = new(9, 0);
		public static readonly Vector2I TreeAutumnBL = new(8, 1);
		public static readonly Vector2I TreeAutumnBR = new(9, 1);

		// ─── 건물 벽 (9-patch, Rows 3-5) ───────────────────────────
		public static readonly Vector2I BWallTL = new(0, 3);
		public static readonly Vector2I BWallT  = new(1, 3);
		public static readonly Vector2I BWallTR = new(2, 3);
		public static readonly Vector2I BWallL  = new(0, 4);
		public static readonly Vector2I BWallC  = new(1, 4);  // 내부
		public static readonly Vector2I BWallR  = new(2, 4);
		public static readonly Vector2I BWallBL = new(0, 5);
		public static readonly Vector2I BWallB  = new(1, 5);
		public static readonly Vector2I BWallBR = new(2, 5);

		// ─── 지붕 (Row 2, 우측) ────────────────────────────────────
		public static readonly Vector2I RoofTL = new(6, 2);
		public static readonly Vector2I RoofTR = new(7, 2);
		public static readonly Vector2I RoofBL = new(6, 3);
		public static readonly Vector2I RoofBR = new(7, 3);

		// ─── 문/창문 (Row 4-5) ──────────────────────────────────────
		public static readonly Vector2I Door = new(3, 4);
		public static readonly Vector2I WindowA = new(4, 4);
		public static readonly Vector2I WindowB = new(5, 4);

		// ─── 캐릭터 (정적 1프레임) ─────────────────────────────────
		public static readonly Vector2I CharPlayer   = new(8, 8);   // 플레이어 (파란 옷)
		public static readonly Vector2I CharNPC      = new(9, 8);   // NPC
		public static readonly Vector2I CharEnemy1   = new(10, 8);  // 적 1 (오크 등)
		public static readonly Vector2I CharEnemy2   = new(11, 8);  // 적 2
		public static readonly Vector2I CharEnemy3   = new(10, 7);  // 적 3 (해골 등)
		public static readonly Vector2I CharEnemy4   = new(11, 7);  // 적 4
		public static readonly Vector2I CharBoss     = new(9, 7);   // 보스 (큰 적)

		// ─── 아이템/도구 (Row 10) ──────────────────────────────────
		public static readonly Vector2I ItemSword    = new(3, 10);
		public static readonly Vector2I ItemSpear    = new(4, 10);
		public static readonly Vector2I ItemShield   = new(5, 10);
		public static readonly Vector2I ItemAxe      = new(7, 10);
		public static readonly Vector2I ItemHammer   = new(8, 10);
		public static readonly Vector2I ItemPickaxe  = new(9, 10);
		public static readonly Vector2I ItemPotion   = new(11, 10);
		public static readonly Vector2I ItemBarrel   = new(10, 10);
		public static readonly Vector2I ItemCoin     = new(9, 7);
		public static readonly Vector2I ItemGem      = new(11, 7);
		public static readonly Vector2I ItemChest    = new(6, 8);
		public static readonly Vector2I ItemKey      = new(7, 8);

		// ─── 장식 (Row 1) ──────────────────────────────────────────
		public static readonly Vector2I BushA  = new(0, 1);
		public static readonly Vector2I BushB  = new(1, 1);
		public static readonly Vector2I FlowerA = new(2, 1);
		public static readonly Vector2I Mushroom = new(3, 1);
	}
}
