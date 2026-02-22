using Godot;
using System;
using System.Collections.Generic;

namespace FirstGame.Maps
{
	/// <summary>
	/// 필드맵 자동 생성기 (Tool 스크립트).
	/// 마을 + 호수 + 직선 길 + 멀티타일 나무 + 9-patch 건물 + 충돌 자동 설정.
	/// </summary>
	[Tool]
	public partial class MapGenerator : Node2D
	{
		// ─── 맵 크기 ───────────────────────────────────────────────
		[ExportGroup("Map Size")]
		[Export] public int MapWidth  { get; set; } = 80;
		[Export] public int MapHeight { get; set; } = 60;

		// ─── 바닥 타일 (Source 0: Floors_Tiles.png) ──────────────
		[ExportGroup("Ground (Source 0)")]
		[Export] public int GroundSourceId { get; set; } = 0;
		/// <summary>확인된 순수 단색 잔디: 좌상단 (0,0)</summary>
		[Export] public Vector2I GrassTile  { get; set; } = new(0, 0);
		[Export] public Vector2I GrassTile2 { get; set; } = new(0, 1);
		/// <summary>회색 벽돌 (우상단 ~18-20열): 길 + 마을 바닥</summary>
		[Export] public Godot.Collections.Array<Vector2I> StoneTiles { get; set; } = new()
		{
			new(18, 0), new(19, 0), new(20, 0),
			new(18, 1), new(19, 1), new(20, 1)
		};

		// ─── 물 타일 (Source 4: Water_tiles.png) ─────────────────
		[ExportGroup("Water (Source 4)")]
		[Export] public int     WaterSourceId { get; set; } = 4;
		/// <summary>순수 단색 물: (2,7) 확인됨</summary>
		[Export] public Vector2I WaterTile    { get; set; } = new(2, 7);

		// ─── 울타리/테두리 (Source 2: Wall_Tiles.png) ─────────────
		[ExportGroup("Obstacles (Source 2)")]
		[Export] public int      ObstacleSourceId { get; set; } = 2;
		/// <summary>판자벽 중심 (row 11): 울타리 + 맵 테두리</summary>
		[Export] public Vector2I WallTile         { get; set; } = new(4, 11);

		// ─── 건물 소스 (Source 38, 42) ────────────────────────────
		// Source 38: Buildings/Floors.png  (3×3 타일 그리드, (0,0)-(2,2) 전부 유효)
		// Source 42: Buildings/Walls.png   (9-patch, Group A 다크브라운)
		// 9-patch Group A 확인된 좌표:
		//   Row 0: cols 1,2,3,4 → TL=1, T=2/3, TR=4
		//   Rows 1-3: 전체 cols → L=0, R=5, BL=0, B=2/3, BR=5
		private const int BuildWallSourceId  = 42;
		private const int BuildFloorSourceId = 38;

		private static readonly Vector2I BWallTL = new(1, 0); // top-left corner
		private static readonly Vector2I BWallT  = new(2, 0); // top edge
		private static readonly Vector2I BWallTR = new(4, 0); // top-right corner
		private static readonly Vector2I BWallL  = new(0, 1); // left edge
		private static readonly Vector2I BWallR  = new(5, 1); // right edge
		private static readonly Vector2I BWallBL = new(0, 3); // bottom-left corner
		private static readonly Vector2I BWallB  = new(2, 3); // bottom edge
		private static readonly Vector2I BWallBR = new(5, 3); // bottom-right corner
		private static readonly Vector2I BFloor  = new(0, 0); // interior floor (Source 38)

		// ─── 나무 (Source 5, 9: 개별 PNG, 16×16 그리드) ──────────
		// Source 9: Trees/Model_02/Size_02.png  침엽수 소형
		//   Tree1: atlas (0,0)~(3,5) — top(row0): cols 0,1  middle(1-2): 0-3
		//   Tree2: atlas (4,0)~(7,5) — top(row0): cols 4,5  middle(1-2): 4-7
		//   bottom collision rows: 4,5 (cols 0-7 모두 유효)
		// Source 5: Trees/Model_01/Size_02.png  낙엽수 중형
		//   Tree1: atlas (0,0)~(3,3) — top(row0): cols 2,3  base(2-3): 0-3
		//   Tree2: atlas (4,0)~(7,3) — top(row0): cols 6,7  base(2-3): 4-7
		//   bottom collision row: 3 (cols 0-7 모두 유효)
		private static readonly (int src, int sc, int sr, int tw, int th, string name)[] _treeTypes =
		{
			(9, 0, 0, 4, 6, "침엽수-1"),
			(9, 4, 0, 4, 6, "침엽수-2"),
			(5, 0, 0, 4, 4, "낙엽수-1"),
			(5, 4, 0, 4, 4, "낙엽수-2"),
		};

		// ─── 마을 ───────────────────────────────────────────────────
		[ExportGroup("Village")]
		[Export] public Vector2I VillageCenter { get; set; } = new(15, 12);
		[Export] public int      VillageWidth  { get; set; } = 14;
		[Export] public int      VillageHeight { get; set; } = 10;

		// ─── 호수 ───────────────────────────────────────────────────
		[ExportGroup("Lake")]
		[Export] public Vector2I LakeCenter { get; set; } = new(55, 15);
		[Export] public int      LakeRadius { get; set; } = 7;

		// ─── 생성 옵션 ─────────────────────────────────────────────
		[ExportGroup("Generation")]
		[Export] public int   Seed        { get; set; } = 0;
		[Export(PropertyHint.Range, "0.01,0.15,0.01")] public float TreeDensity { get; set; } = 0.04f;

		private bool _regenerate = false;
		[Export] public bool Regenerate
		{
			get => _regenerate;
			set
			{
				_regenerate = value;
				if (value && Engine.IsEditorHint())
					CallDeferred(nameof(RunGenerate));
			}
		}

		// ─── 내부 상태 ─────────────────────────────────────────────
		private TileMapLayer _groundLayer;
		private TileMapLayer _obstacleLayer;
		private TileMapLayer _decorationLayer;
		private int _usedSeed;

		private HashSet<Vector2I> _pathTiles    = new();
		private HashSet<Vector2I> _villageTiles = new();
		private HashSet<Vector2I> _lakeTiles    = new();

		public override void _Ready() { }

		private void RunGenerate()
		{
			_groundLayer    = GetNodeOrNull<TileMapLayer>("GroundLayer");
			_obstacleLayer  = GetNodeOrNull<TileMapLayer>("ObstacleLayer");
			_decorationLayer = GetNodeOrNull<TileMapLayer>("DecorationLayer");

			if (_groundLayer == null || _obstacleLayer == null)
			{
				GD.PrintErr("MapGenerator: GroundLayer / ObstacleLayer 없음");
				_regenerate = false;
				return;
			}

			GenerateMap();
			_regenerate = false;
			NotifyPropertyListChanged();
			GD.Print("MapGenerator: 완료. Ctrl+S로 씬 저장하세요.");
		}

		// ════════════════════════════════════════════════════════════
		public void GenerateMap()
		{
			_usedSeed = Seed != 0 ? Seed : (int)(GD.Randi() % 99999);
			GD.Print($"[MapGenerator] seed={_usedSeed}");

			_groundLayer.Clear();
			_obstacleLayer.Clear();
			_decorationLayer?.Clear();
			_pathTiles.Clear();
			_villageTiles.Clear();
			_lakeTiles.Clear();

			var oldBody = GetNodeOrNull("FenceCollision");
			oldBody?.QueueFree();

			Step1_FillGround();
			Step2_Border();
			Step3_Village();
			Step4_Buildings();
			Step5_Lake();
			Step6_Roads();
			Step7_Trees();
			Step8_SetupPhysics();    // TileSet physics polygon 설정 (영구 저장)
			Step9_FenceCollision();  // StaticBody2D 보완 충돌체
		}

		// ── Step 1: 전체 잔디 ────────────────────────────────────────
		private void Step1_FillGround()
		{
			var noise = MakeNoise(FastNoiseLite.NoiseTypeEnum.Simplex, _usedSeed, 0.06f);
			for (int x = 0; x < MapWidth; x++)
				for (int y = 0; y < MapHeight; y++)
				{
					float n = noise.GetNoise2D(x * 2f, y * 2f);
					_groundLayer.SetCell(new Vector2I(x, y), GroundSourceId,
						n > 0.35f ? GrassTile2 : GrassTile);
				}
		}

		// ── Step 2: 맵 테두리 (2타일 두께 벽) ───────────────────────
		private void Step2_Border()
		{
			for (int x = 0; x < MapWidth; x++)
				for (int y = 0; y < MapHeight; y++)
					if (x < 2 || x >= MapWidth - 2 || y < 2 || y >= MapHeight - 2)
						_obstacleLayer.SetCell(new Vector2I(x, y), ObstacleSourceId, WallTile);
		}

		// ── Step 3: 마을 (석재 바닥 + 울타리) ───────────────────────
		private void Step3_Village()
		{
			GetVillageBounds(out int vx1, out int vy1, out int vx2, out int vy2);

			// 내부 석재 바닥
			for (int x = vx1; x <= vx2; x++)
				for (int y = vy1; y <= vy2; y++)
				{
					var pos = new Vector2I(x, y);
					_groundLayer.SetCell(pos, GroundSourceId, PickStone(x, y));
					_obstacleLayer.EraseCell(pos);
					_villageTiles.Add(pos);
				}

			// 울타리 (마을 외곽 1칸)
			for (int x = vx1 - 1; x <= vx2 + 1; x++)
				for (int y = vy1 - 1; y <= vy2 + 1; y++)
				{
					bool isFence = (x == vx1-1 || x == vx2+1 || y == vy1-1 || y == vy2+1);
					if (!isFence) continue;
					if (x < 2 || x >= MapWidth-2 || y < 2 || y >= MapHeight-2) continue;

					var pos = new Vector2I(x, y);
					// 남쪽 입구 (중앙 ±1)
					if (y == vy2+1 && Math.Abs(x - VillageCenter.X) <= 1) continue;
					// 동쪽 입구 (중앙 ±1)
					if (x == vx2+1 && Math.Abs(y - VillageCenter.Y) <= 1) continue;

					_obstacleLayer.SetCell(pos, ObstacleSourceId, WallTile);
					_villageTiles.Add(pos);
				}

			GD.Print($"[마을] ({vx1},{vy1})-({vx2},{vy2}), {_villageTiles.Count}타일");
		}

		// ── Step 4: 건물 2채 (마을 내, 9-patch 벽 + 건물 바닥) ─────
		private void Step4_Buildings()
		{
			GetVillageBounds(out int vx1, out int vy1, out _, out _);

			// 건물 1: 마을 좌상단 (상점)
			PlaceBuilding(vx1 + 1, vy1 + 1, 5, 4, doorSouth: true);
			// 건물 2: 마을 우상단 (집)
			PlaceBuilding(vx1 + 8, vy1 + 1, 5, 4, doorSouth: true);
		}

		/// <summary>
		/// 9-patch 건물 배치.
		/// ObstacleLayer: Source 42 (Walls.png) 벽 타일
		/// GroundLayer:   Source 38 (Floors.png) 바닥 타일
		/// </summary>
		private void PlaceBuilding(int bx, int by, int bw, int bh, bool doorSouth)
		{
			int doorX = bx + bw / 2; // 남문 중앙 x

			for (int x = bx; x < bx + bw; x++)
				for (int y = by; y < by + bh; y++)
				{
					if (x < 3 || x >= MapWidth-3 || y < 3 || y >= MapHeight-3) continue;
					var pos = new Vector2I(x, y);

					bool isTop    = (y == by);
					bool isBottom = (y == by + bh - 1);
					bool isLeft   = (x == bx);
					bool isRight  = (x == bx + bw - 1);
					bool isPerimeter = isTop || isBottom || isLeft || isRight;
					bool isDoor   = doorSouth && isBottom && x == doorX;

					// 모든 셀: GroundLayer에 건물 바닥 (Source 38)
					_groundLayer.SetCell(pos, BuildFloorSourceId, BFloor);

					if (isPerimeter && !isDoor)
					{
						// 9-patch 벽 타일 선택 (Source 42)
						Vector2I wallAtlas;
						if      (isTop    && isLeft)  wallAtlas = BWallTL;
						else if (isTop    && isRight) wallAtlas = BWallTR;
						else if (isBottom && isLeft)  wallAtlas = BWallBL;
						else if (isBottom && isRight) wallAtlas = BWallBR;
						else if (isTop)               wallAtlas = BWallT;
						else if (isBottom)            wallAtlas = BWallB;
						else if (isLeft)              wallAtlas = BWallL;
						else                          wallAtlas = BWallR;

						_obstacleLayer.SetCell(pos, BuildWallSourceId, wallAtlas);
					}
					else
					{
						// 내부 또는 출입구: 장애물 제거
						_obstacleLayer.EraseCell(pos);
					}

					_villageTiles.Add(pos);
				}
		}

		// ── Step 5: 호수 ─────────────────────────────────────────────
		private void Step5_Lake()
		{
			var noise = MakeNoise(FastNoiseLite.NoiseTypeEnum.Simplex, _usedSeed + 100, 0.13f);
			int rx = LakeRadius;
			int ry = Mathf.RoundToInt(LakeRadius * 0.72f);

			for (int x = LakeCenter.X - rx - 2; x <= LakeCenter.X + rx + 2; x++)
				for (int y = LakeCenter.Y - ry - 2; y <= LakeCenter.Y + ry + 2; y++)
				{
					if (x < 3 || x >= MapWidth-3 || y < 3 || y >= MapHeight-3) continue;
					var pos = new Vector2I(x, y);
					if (_villageTiles.Contains(pos)) continue;

					float dx = (float)(x - LakeCenter.X) / rx;
					float dy = (float)(y - LakeCenter.Y) / ry;
					float edgeNoise = noise.GetNoise2D(x * 5f, y * 5f) * 0.28f;

					if (dx*dx + dy*dy + edgeNoise < 1.0f)
					{
						// 물: ObstacleLayer에만 (GroundLayer 잔디가 밑에 깔림)
						_obstacleLayer.SetCell(pos, WaterSourceId, WaterTile);
						_lakeTiles.Add(pos);
					}
				}

			GD.Print($"[호수] {_lakeTiles.Count}타일");
		}

		// ── Step 6: 직선 길 (L자형, 3타일 폭) ──────────────────────
		private void Step6_Roads()
		{
			GetVillageBounds(out int vx1, out int vy1, out int vx2, out int vy2);

			var spawn       = new Vector2I(MapWidth / 2, MapHeight - 10);
			var southGate   = new Vector2I(VillageCenter.X, vy2 + 2);
			var eastGate    = new Vector2I(vx2 + 2, VillageCenter.Y);
			var lakeApproach = new Vector2I(LakeCenter.X - LakeRadius - 2, LakeCenter.Y);
			var eastEdge    = new Vector2I(MapWidth - 5, spawn.Y);

			// 길 1: 플레이어 스폰 → 마을 남문
			DrawRoad(spawn, southGate);
			// 길 2: 마을 동문 → 호수 서쪽
			DrawRoad(eastGate, lakeApproach);
			// 길 3: 스폰에서 동쪽 (보조 탐색로)
			DrawRoad(spawn, eastEdge);

			GD.Print($"[길] {_pathTiles.Count}타일");
		}

		/// <summary>L자형 직선 길: 수평 구간 먼저, 그 다음 수직</summary>
		private void DrawRoad(Vector2I from, Vector2I to)
		{
			for (int x = Math.Min(from.X, to.X); x <= Math.Max(from.X, to.X); x++)
				PaveAt(x, from.Y);
			for (int y = Math.Min(from.Y, to.Y); y <= Math.Max(from.Y, to.Y); y++)
				PaveAt(to.X, y);
		}

		private void PaveAt(int cx, int cy)
		{
			for (int dx = -1; dx <= 1; dx++)
				for (int dy = -1; dy <= 1; dy++)
				{
					int px = cx + dx, py = cy + dy;
					if (px < 3 || px >= MapWidth-3 || py < 3 || py >= MapHeight-3) continue;
					var pos = new Vector2I(px, py);
					if (_villageTiles.Contains(pos)) continue;
					if (_lakeTiles.Contains(pos)) continue;
					_groundLayer.SetCell(pos, GroundSourceId, PickStone(px, py));
					_obstacleLayer.EraseCell(pos);
					_pathTiles.Add(pos);
				}
		}

		// ── Step 7: 나무 배치 (Source 5, 9 개별 PNG 멀티타일) ───────
		// Source 9: 4×6 타일, Source 5: 4×4 타일
		// 유효하지 않은 atlas 좌표는 HasTile 체크로 스킵
		private void Step7_Trees()
		{
			var noise = MakeNoise(FastNoiseLite.NoiseTypeEnum.Cellular, _usedSeed + 1, 0.025f);
			var rng = new RandomNumberGenerator();
			rng.Seed = (ulong)_usedSeed;

			float threshold = 0.35f - (TreeDensity * 2.0f);

			// 9×8 격자 기반 배치 (Source 9 최대 크기 4×6 기준 여유 마진)
			for (int x = 4; x < MapWidth - 12; x += 9)
				for (int y = 4; y < MapHeight - 9; y += 8)
				{
					float n = noise.GetNoise2D(x, y);
					if (n < threshold) continue;

					int typeIdx = (int)(rng.Randi() % (uint)_treeTypes.Length);
					int tx = x + (int)(rng.RandiRange(-2, 2));
					int ty = y + (int)(rng.RandiRange(-2, 2));
					TryPlaceTree(tx, ty, typeIdx);
				}
		}

		private void TryPlaceTree(int mx, int my, int typeIdx)
		{
			var (src, sc, sr, w, h, _) = _treeTypes[typeIdx];

			// 배치 가능 여부 사전 체크 (bounding box 전체)
			for (int dx = 0; dx < w; dx++)
				for (int dy = 0; dy < h; dy++)
				{
					var pos = new Vector2I(mx + dx, my + dy);
					if (pos.X < 3 || pos.X >= MapWidth-3) return;
					if (pos.Y < 3 || pos.Y >= MapHeight-3) return;
					if (_villageTiles.Contains(pos)) return;
					if (_lakeTiles.Contains(pos)) return;
					if (_pathTiles.Contains(pos)) return;
					if (_obstacleLayer.GetCellSourceId(pos) != -1) return;
				}

			// 유효한 atlas 타일만 배치 (투명 타일 스킵)
			var atlasSource = _obstacleLayer.TileSet?.GetSource(src) as TileSetAtlasSource;
			for (int dx = 0; dx < w; dx++)
				for (int dy = 0; dy < h; dy++)
				{
					var atlasCoords = new Vector2I(sc + dx, sr + dy);
					if (atlasSource != null && !atlasSource.HasTile(atlasCoords)) continue;
					_obstacleLayer.SetCell(new Vector2I(mx + dx, my + dy), src, atlasCoords);
				}
		}

		// ── Step 8: TileSet Physics Polygon 설정 ────────────────────
		// 소스별 충돌 타일에 물리 polygon 추가 후 TileSet .tres 영구 저장.
		private void Step8_SetupPhysics()
		{
			var tileSet = _obstacleLayer?.TileSet;
			if (tileSet == null) { GD.PrintErr("[Physics] TileSet 없음"); return; }

			if (tileSet.GetPhysicsLayersCount() == 0)
			{
				tileSet.AddPhysicsLayer();
				tileSet.SetPhysicsLayerCollisionLayer(0, 4);
				tileSet.SetPhysicsLayerCollisionMask(0, 0);
				GD.Print("[Physics] PhysicsLayer 0 생성 (layer=4)");
			}

			var fullPoly = new Vector2[] { new(-8,-8), new(8,-8), new(8,8), new(-8,8) };

			// 1. 울타리/테두리 (Source 2, WallTile)
			AddTilePhysics(tileSet, ObstacleSourceId, WallTile, fullPoly);

			// 2. 건물 벽 (Source 42, 9-patch 전체)
			foreach (var wallAtlas in new[] { BWallTL, BWallT, BWallTR, BWallL, BWallR, BWallBL, BWallB, BWallBR })
				AddTilePhysics(tileSet, BuildWallSourceId, wallAtlas, fullPoly);
			// Row 0의 (3,0)도 Top으로 사용 가능하므로 추가
			AddTilePhysics(tileSet, BuildWallSourceId, new Vector2I(3, 0), fullPoly);
			AddTilePhysics(tileSet, BuildWallSourceId, new Vector2I(3, 3), fullPoly);

			// 3. Source 9 침엽수 하단 2행 (row 4-5, cols 0-7 모두 유효)
			for (int dx = 0; dx < 8; dx++)
			{
				AddTilePhysics(tileSet, 9, new Vector2I(dx, 4), fullPoly);
				AddTilePhysics(tileSet, 9, new Vector2I(dx, 5), fullPoly);
			}

			// 4. Source 5 낙엽수 하단 1행 (row 3, cols 0-7 모두 유효)
			for (int dx = 0; dx < 8; dx++)
				AddTilePhysics(tileSet, 5, new Vector2I(dx, 3), fullPoly);

			// 5. 물 (Source 4, WaterTile)
			AddTilePhysics(tileSet, WaterSourceId, WaterTile, fullPoly);

			if (!string.IsNullOrEmpty(tileSet.ResourcePath))
			{
				var err = ResourceSaver.Save(tileSet, tileSet.ResourcePath);
				if (err == Error.Ok)
					GD.Print($"[Physics] TileSet 저장: {tileSet.ResourcePath}");
				else
					GD.PrintErr($"[Physics] TileSet 저장 실패: {err}");
			}
		}

		private void AddTilePhysics(TileSet tileSet, int sourceId, Vector2I atlasCoords, Vector2[] poly)
		{
			var src = tileSet.GetSource(sourceId) as TileSetAtlasSource;
			if (src == null) return;
			if (!src.HasTile(atlasCoords)) return;
			var data = src.GetTileData(atlasCoords, 0);
			if (data == null) return;
			if (data.GetCollisionPolygonsCount(0) > 0) return; // 이미 설정됨
			data.AddCollisionPolygon(0);
			data.SetCollisionPolygonPoints(0, 0, poly);
		}

		// ── Step 9: StaticBody2D 보완 충돌체 ────────────────────────
		// TileSet physics 외에 StaticBody2D로 추가 보장.
		// 울타리(Source2) + 건물벽(Source42) 개별 shape + 호수 bounding box.
		private void Step9_FenceCollision()
		{
			var body = new StaticBody2D
			{
				Name = "FenceCollision",
				CollisionLayer = 4,
				CollisionMask  = 0
			};
			AddChild(body);

			var buildWallSet = new HashSet<Vector2I>
				{ BWallTL, BWallT, new Vector2I(3,0), BWallTR,
				  BWallL,  BWallR,
				  BWallBL, BWallB, new Vector2I(3,3), BWallBR };

			int shapeCount = 0;
			for (int x = 0; x < MapWidth; x++)
				for (int y = 0; y < MapHeight; y++)
				{
					var pos = new Vector2I(x, y);
					int srcId = _obstacleLayer.GetCellSourceId(pos);
					if (srcId == -1) continue;

					var atlas = _obstacleLayer.GetCellAtlasCoords(pos);

					bool isFence   = (srcId == ObstacleSourceId && atlas == WallTile);
					bool isBuildWall = (srcId == BuildWallSourceId && buildWallSet.Contains(atlas));

					if (!isFence && !isBuildWall) continue;

					AddBoxShape(body, x, y);
					shapeCount++;
				}

			// 호수 bounding box
			if (_lakeTiles.Count > 0)
			{
				int lx1 = int.MaxValue, ly1 = int.MaxValue;
				int lx2 = int.MinValue, ly2 = int.MinValue;
				foreach (var t in _lakeTiles)
				{
					if (t.X < lx1) lx1 = t.X;
					if (t.Y < ly1) ly1 = t.Y;
					if (t.X > lx2) lx2 = t.X;
					if (t.Y > ly2) ly2 = t.Y;
				}
				var cs = new CollisionShape2D();
				cs.Shape = new RectangleShape2D
				{
					Size = new Vector2((lx2 - lx1 + 1) * 16f, (ly2 - ly1 + 1) * 16f)
				};
				cs.Position = new Vector2((lx1 + lx2 + 1) * 8f, (ly1 + ly2 + 1) * 8f);
				body.AddChild(cs);
				GD.Print($"[물충돌] bounding box ({lx1},{ly1})-({lx2},{ly2})");
			}

			GD.Print($"[충돌체] {shapeCount}개 shape 생성");
		}

		private void AddBoxShape(StaticBody2D body, int tileX, int tileY)
		{
			var cs = new CollisionShape2D();
			cs.Shape = new RectangleShape2D { Size = new Vector2(16, 16) };
			cs.Position = new Vector2(tileX * 16 + 8, tileY * 16 + 8);
			body.AddChild(cs);
		}

		// ════════════════════════════════════════════════════════════
		// 헬퍼
		// ════════════════════════════════════════════════════════════

		private void GetVillageBounds(out int vx1, out int vy1, out int vx2, out int vy2)
		{
			int hw = VillageWidth / 2, hh = VillageHeight / 2;
			vx1 = Math.Max(3, VillageCenter.X - hw);
			vy1 = Math.Max(3, VillageCenter.Y - hh);
			vx2 = Math.Min(MapWidth - 3, VillageCenter.X + hw);
			vy2 = Math.Min(MapHeight - 3, VillageCenter.Y + hh);
		}

		private Vector2I PickStone(int x, int y)
		{
			if (StoneTiles.Count == 0) return new Vector2I(18, 0);
			uint h = (uint)(x * 17 + y * 31);
			return StoneTiles[(int)(h % StoneTiles.Count)];
		}

		private static FastNoiseLite MakeNoise(FastNoiseLite.NoiseTypeEnum type, int seed, float freq)
		{
			var n = new FastNoiseLite();
			n.NoiseType = type;
			n.Seed      = seed;
			n.Frequency = freq;
			return n;
		}
	}
}
