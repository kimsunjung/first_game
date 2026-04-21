using Godot;
using System;
using System.Collections.Generic;

namespace FirstGame.Maps
{
	/// <summary>
	/// 필드맵 자동 생성기 (Tool 스크립트, 필드 전용).
	///
	/// 씬 설정:
	///   1. 씬 루트에 Node2D 추가 → 이름 MapGenerator, 이 스크립트 연결
	///   2. 자식 TileMapLayer 2개: GroundLayer (z_index=-1), ObstacleLayer (z_index=0)
	///   3. 두 레이어 TileSet = res://Resources/Tilesets/field_tileset.tres
	///   4. Inspector → Regenerate = true → Ctrl+S 저장
	///
	/// 타일 크기 16px 기준: 1280×720 씬 = MapWidth=80, MapHeight=45
	///
	/// 레이어 구조:
	///   Background (ColorRect, z=-10): 잔디 색상 → GroundLayer 빈 셀에서 비침
	///   GroundLayer (TileMapLayer, z=-1): 흙 패치 + 포탈 주변 석재만 그림
	///   ObstacleLayer (TileMapLayer, z=0): 나무, 울타리 벽, 물 등
	///
	/// Source 0 (Floors_Tiles.png) rows 13-15: 솔리드 flat 타일 (sparse 아님, 0-9 전체 등록)
	///   cols 0-4: 회색/돌 계열
	///   cols 5-9: 오렌지/흙 계열
	/// Source 2 (Wall_Tiles.png): WallTile = (4,11), physics 확인됨
	/// Source 4 (Water_tiles.png): WaterTile = (2,7), physics 확인됨
	/// </summary>
	[Tool]
	public partial class MapGenerator : Node2D
	{
		// ─── 맵 크기 ───────────────────────────────────────────────
		[ExportGroup("Map Size")]
		[Export] public int MapWidth  { get; set; } = 80;
		[Export] public int MapHeight { get; set; } = 45;

		// ─── 바닥 타일 (Source 0) ─────────────────────────────────
		// 잔디: GroundLayer에 그리지 않음 → 씬의 Background ColorRect(녹색)가 보임
		// 흙/석재: rows 13-15 솔리드 flat 타일 사용 (blob 타일 아님)
		[ExportGroup("Ground (Source 0)")]
		[Export] public int      GroundSourceId { get; set; } = 0;
		/// <summary>흙 패치 타일 (5,14) — Source 0 row 14 솔리드 flat, 오렌지/흙 계열</summary>
		[Export] public Vector2I DirtTile       { get; set; } = new(6, 14);
		/// <summary>흙 패치 비율 0=없음, 1=전부. 0.25 권장.</summary>
		[Export(PropertyHint.Range, "0,1,0.05")] public float DirtCoverage { get; set; } = 0.25f;
		/// <summary>흙 패치 노이즈 주파수. 낮을수록 큰 덩어리.</summary>
		[Export(PropertyHint.Range, "0.01,0.1,0.005")] public float DirtFrequency { get; set; } = 0.035f;
		/// <summary>포탈 주변 석재 바닥 타일 (0,14) — Source 0 row 14 솔리드 flat, 회색/돌 계열</summary>
		[Export] public Vector2I StoneTile      { get; set; } = new(1, 14);

		// ─── 울타리/테두리 (Source 2: Wall_Tiles.png) ─────────────
		[ExportGroup("Obstacles (Source 2)")]
		[Export] public int      ObstacleSourceId { get; set; } = 2;
		/// <summary>벽 타일 (4,11) — 등록+충돌 확인</summary>
		[Export] public Vector2I WallTile         { get; set; } = new(4, 11);

		// ─── 호수 (Source 4: Water_tiles.png) ────────────────────
		[ExportGroup("Lake (Source 4)")]
		[Export] public bool     HasLake       { get; set; } = true;
		[Export] public int      WaterSourceId { get; set; } = 4;
		/// <summary>솔리드 물 + 충돌 (2,7) — 확인됨</summary>
		[Export] public Vector2I WaterTile     { get; set; } = new(2, 7);
		[Export] public Vector2I LakeCenter    { get; set; } = new(55, 22);
		[Export] public int      LakeRadius    { get; set; } = 6;
		/// <summary>
		/// Shore 타일: Water_tiles.png Source 4, 섬 blob 그룹 0 (cols 0-5, rows 0-4).
		/// 각 방향 = 잔디+물 경계 타일. 6×5 blob 외곽 위치.
		///   ShoreN  = 호수가 북(위)에 → 타일 상단=물, 하단=잔디 → blob row 0 top center
		///   ShoreS  = 호수가 남(아래) → 타일 상단=잔디, 하단=물 → blob row 4 bottom center
		///   ShoreW  = 호수가 서(왼) → 타일 왼=물, 오=잔디 → blob col 0 left center
		///   ShoreE  = 호수가 동(오) → 타일 왼=잔디, 오=물 → blob col 5 right center
		/// </summary>
		[Export] public Vector2I ShoreN  { get; set; } = new(2, 0);  // blob 상단 중앙
		[Export] public Vector2I ShoreS  { get; set; } = new(2, 4);  // blob 하단 중앙
		[Export] public Vector2I ShoreE  { get; set; } = new(5, 2);  // blob 우측 중앙
		[Export] public Vector2I ShoreW  { get; set; } = new(0, 2);  // blob 좌측 중앙
		[Export] public Vector2I ShoreNE { get; set; } = new(5, 0);  // blob 우상 코너
		[Export] public Vector2I ShoreNW { get; set; } = new(0, 0);  // blob 좌상 코너
		[Export] public Vector2I ShoreSE { get; set; } = new(5, 4);  // blob 우하 코너
		[Export] public Vector2I ShoreSW { get; set; } = new(0, 4);  // blob 좌하 코너

		// ─── 나무 ─────────────────────────────────────────────────
		[ExportGroup("Trees")]
		[Export(PropertyHint.Range, "0.01,0.15,0.01")]
		public float TreeDensity { get; set; } = 0.04f;

		private static readonly (int src, int sc, int sr, int tw, int th)[] _treeTypes =
		{
			(9, 0, 0, 4, 6),   // 침엽수-1 (Source 9, Model_02/Size_02)
			(9, 4, 0, 4, 6),   // 침엽수-2
			(5, 0, 0, 4, 4),   // 낙엽수-1 (Source 5, Model_01/Size_02)
			(5, 4, 0, 4, 4),   // 낙엽수-2
		};

		// ─── 포탈 클리어 존 ────────────────────────────────────────
		[ExportGroup("Portal Paths")]
		[Export] public Godot.Collections.Array<Vector2I> PortalPixelPositions { get; set; } = new();

		// ─── 생성 ──────────────────────────────────────────────────
		[ExportGroup("Generation")]
		[Export] public int Seed { get; set; } = 0;

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
		private int          _usedSeed;

		private HashSet<Vector2I> _lakeTiles    = new();
		private HashSet<Vector2I> _clearZones   = new();

		public override void _Ready()
		{
			if (Engine.IsEditorHint()) return;

			_groundLayer   = GetNodeOrNull<TileMapLayer>("GroundLayer");
			_obstacleLayer = GetNodeOrNull<TileMapLayer>("ObstacleLayer");

			if (_groundLayer == null || _obstacleLayer == null)
			{
				GD.PrintErr("MapGenerator: GroundLayer / ObstacleLayer 자식 노드가 없습니다.");
				return;
			}

			GenerateMap();
		}

		private void RunGenerate()
		{
			_groundLayer   = GetNodeOrNull<TileMapLayer>("GroundLayer");
			_obstacleLayer = GetNodeOrNull<TileMapLayer>("ObstacleLayer");

			if (_groundLayer == null || _obstacleLayer == null)
			{
				GD.PrintErr("MapGenerator: GroundLayer / ObstacleLayer 자식 노드가 없습니다.");
				_regenerate = false;
				return;
			}

			GenerateMap();
			_regenerate = false;
			NotifyPropertyListChanged();
			GD.Print("MapGenerator: 완료. Ctrl+S로 씬 저장하세요.");
		}

		public void GenerateMap()
		{
			_usedSeed = Seed != 0 ? Seed : (int)(GD.Randi() % 99999);
			GD.Print($"[MapGenerator] seed={_usedSeed}  크기={MapWidth}×{MapHeight}");

			_groundLayer.Clear();
			_obstacleLayer.Clear();
			_lakeTiles.Clear();
			_clearZones.Clear();

			GetNodeOrNull("FenceCollision")?.QueueFree();

			Step1_FillGround();        // 잔디 + 흙 패치
			Step2_Border();            // 맵 테두리 벽
			Step3_PortalClearZones();  // 포탈 주변 석재
			if (HasLake) Step4_Lake(); // 호수 (선택)
			Step5_Trees();             // 나무
			Step6_SetupPhysics();      // 충돌 설정
			Step7_FenceCollision();    // StaticBody2D 보완
		}

		// ── Step 1: 흙 패치만 GroundLayer에 그리기 ──────────────────
		// 잔디: GroundLayer에 아무것도 그리지 않음 → 씬 Background(녹색 ColorRect)가 보임.
		// 흙 패치: Simplex 노이즈로 자연스러운 덩어리 생성 → DirtTile을 GroundLayer에 그림.
		// DirtTile = Source 0 rows 13-15의 솔리드 flat 타일 (blob 타일 아님).
		private void Step1_FillGround()
		{
			var dirtNoise = MakeNoise(FastNoiseLite.NoiseTypeEnum.Simplex, _usedSeed + 2, DirtFrequency);

			// threshold: coverage 0.25 → 상위 25%를 흙 패치로
			float threshold = 1f - 2f * Mathf.Clamp(DirtCoverage, 0f, 1f);

			for (int x = 0; x < MapWidth; x++)
				for (int y = 0; y < MapHeight; y++)
				{
					float n = dirtNoise.GetNoise2D(x, y);
					if (n > threshold)
						_groundLayer.SetCell(new Vector2I(x, y), GroundSourceId, DirtTile);
					// 잔디 셀: 비워둠 → Background ColorRect(녹색)가 비쳐 보임
				}
		}

		// ── Step 2: 맵 테두리 (2타일 두께 벽) ───────────────────────
		private void Step2_Border()
		{
			for (int x = 0; x < MapWidth; x++)
				for (int y = 0; y < MapHeight; y++)
					if (x < 2 || x >= MapWidth - 2 || y < 2 || y >= MapHeight - 2)
					{
						var pos = new Vector2I(x, y);
						_obstacleLayer.SetCell(pos, ObstacleSourceId, WallTile);
						_clearZones.Add(pos);
					}
		}

		// ── Step 3: 포탈 주변 석재 바닥 + 클리어 존 ─────────────────
		// 경계 벽 타일 위치라도 포탈 주변은 지움 → 포탈이 씬 가장자리에 있어도 접근 가능.
		// 씬 외부 충돌은 Walls CollisionShape2D(씬 파일)가 담당.
		private void Step3_PortalClearZones()
		{
			const int clearRadius = 4;

			foreach (var pixelPos in PortalPixelPositions)
			{
				int cx = pixelPos.X / 16;
				int cy = pixelPos.Y / 16;

				for (int dx = -clearRadius; dx <= clearRadius; dx++)
					for (int dy = -clearRadius; dy <= clearRadius; dy++)
					{
						int px = cx + dx, py = cy + dy;
						// 타일맵 범위 안쪽만 처리 (경계 벽 포함 — 포탈은 가장자리에 배치 가능)
						if (px < 0 || px >= MapWidth || py < 0 || py >= MapHeight) continue;
						var pos = new Vector2I(px, py);
						_groundLayer.SetCell(pos, GroundSourceId, StoneTile);
						_obstacleLayer.EraseCell(pos);
						_clearZones.Add(pos);
					}
			}
		}

		// ── Step 4: 호수 + 경계 타일 ─────────────────────────────────
		// 호수 내부: ObstacleLayer에 솔리드 물 (2,7).
		// 호수 경계: GroundLayer에 Shore 타일 (Source 4, rows 0-4).
		//   → 타일 좌표는 Inspector에서 확인 후 조정하세요.
		private void Step4_Lake()
		{
			var noise = MakeNoise(FastNoiseLite.NoiseTypeEnum.Simplex, _usedSeed + 100, 0.13f);
			int rx = LakeRadius;
			int ry = Mathf.RoundToInt(LakeRadius * 0.72f);

			// Pass 1: 모든 호수 타일 솔리드 물로 채우기
			for (int x = LakeCenter.X - rx - 2; x <= LakeCenter.X + rx + 2; x++)
				for (int y = LakeCenter.Y - ry - 2; y <= LakeCenter.Y + ry + 2; y++)
				{
					if (x < 3 || x >= MapWidth-3 || y < 3 || y >= MapHeight-3) continue;
					var pos = new Vector2I(x, y);
					if (_clearZones.Contains(pos)) continue;

					float dx = (float)(x - LakeCenter.X) / rx;
					float dy = (float)(y - LakeCenter.Y) / ry;
					float edgeNoise = noise.GetNoise2D(x * 5f, y * 5f) * 0.28f;

					if (dx*dx + dy*dy + edgeNoise < 1.0f)
					{
						_obstacleLayer.SetCell(pos, WaterSourceId, WaterTile);
						_lakeTiles.Add(pos);
						_clearZones.Add(pos);
					}
				}

			// Pass 2: 호수 경계 → 인접 잔디 타일을 Shore 타일로 교체 (GroundLayer)
			foreach (var waterPos in _lakeTiles)
			{
				foreach (var adj in GetCardinalNeighbors(waterPos))
				{
					if (_lakeTiles.Contains(adj)) continue;
					if (adj.X < 0 || adj.X >= MapWidth || adj.Y < 0 || adj.Y >= MapHeight) continue;
					if (_clearZones.Contains(adj) && !_lakeTiles.Contains(adj)) continue;

					// 물 기준으로 잔디 타일이 어느 방향에 있는지 판단
					// → 잔디 쪽에서 보면 water가 특정 방향에 있음
					bool waterN = _lakeTiles.Contains(adj + new Vector2I( 0, -1));
					bool waterS = _lakeTiles.Contains(adj + new Vector2I( 0,  1));
					bool waterE = _lakeTiles.Contains(adj + new Vector2I( 1,  0));
					bool waterW = _lakeTiles.Contains(adj + new Vector2I(-1,  0));

					Vector2I shoreAtlas = PickShoreAtlas(waterN, waterS, waterE, waterW);
					_groundLayer.SetCell(adj, WaterSourceId, shoreAtlas);
				}
			}

			GD.Print($"[호수] {_lakeTiles.Count}타일");
		}

		/// <summary>
		/// 인접한 물 방향에 따라 Shore 타일 좌표 선택.
		/// Inspector의 Shore* 값으로 조정 가능.
		/// </summary>
		private Vector2I PickShoreAtlas(bool n, bool s, bool e, bool w)
		{
			// 단순 4방향 → 코너 조합
			if (n && e && !s && !w) return ShoreNE;
			if (n && w && !s && !e) return ShoreNW;
			if (s && e && !n && !w) return ShoreSE;
			if (s && w && !n && !e) return ShoreSW;
			if (n && !s) return ShoreN;
			if (s && !n) return ShoreS;
			if (e && !w) return ShoreE;
			if (w && !e) return ShoreW;
			// 여러 방향이면 N 우선
			if (n) return ShoreN;
			if (s) return ShoreS;
			if (e) return ShoreE;
			if (w) return ShoreW;
			return ShoreN;
		}

		private static IEnumerable<Vector2I> GetCardinalNeighbors(Vector2I pos)
		{
			yield return pos + new Vector2I( 0, -1);
			yield return pos + new Vector2I( 0,  1);
			yield return pos + new Vector2I( 1,  0);
			yield return pos + new Vector2I(-1,  0);
		}

		// ── Step 5: 나무 배치 ────────────────────────────────────────
		private void Step5_Trees()
		{
			var noise = MakeNoise(FastNoiseLite.NoiseTypeEnum.Cellular, _usedSeed + 1, 0.025f);
			var rng   = new RandomNumberGenerator();
			rng.Seed  = (ulong)_usedSeed;

			float threshold = 0.35f - (TreeDensity * 2.0f);

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
			var (src, sc, sr, w, h) = _treeTypes[typeIdx];

			for (int dx = 0; dx < w; dx++)
				for (int dy = 0; dy < h; dy++)
				{
					var pos = new Vector2I(mx + dx, my + dy);
					if (pos.X < 3 || pos.X >= MapWidth-3) return;
					if (pos.Y < 3 || pos.Y >= MapHeight-3) return;
					if (_clearZones.Contains(pos)) return;
					if (_obstacleLayer.GetCellSourceId(pos) != -1) return;
				}

			var atlasSource = _obstacleLayer.TileSet?.GetSource(src) as TileSetAtlasSource;
			for (int dx = 0; dx < w; dx++)
				for (int dy = 0; dy < h; dy++)
				{
					var atlasCoords = new Vector2I(sc + dx, sr + dy);
					if (atlasSource != null && !atlasSource.HasTile(atlasCoords)) continue;
					_obstacleLayer.SetCell(new Vector2I(mx + dx, my + dy), src, atlasCoords);
				}
		}

		// ── Step 6: Physics ──────────────────────────────────────────
		private void Step6_SetupPhysics()
		{
			var tileSet = _obstacleLayer?.TileSet;
			if (tileSet == null) return;

			if (tileSet.GetPhysicsLayersCount() == 0)
			{
				tileSet.AddPhysicsLayer();
				tileSet.SetPhysicsLayerCollisionLayer(0, 4);
				tileSet.SetPhysicsLayerCollisionMask(0, 0);
			}

			var fullPoly = new Vector2[] { new(-8,-8), new(8,-8), new(8,8), new(-8,8) };

			AddTilePhysics(tileSet, ObstacleSourceId, WallTile, fullPoly);
			for (int dx = 0; dx < 8; dx++)
			{
				AddTilePhysics(tileSet, 9, new Vector2I(dx, 4), fullPoly);
				AddTilePhysics(tileSet, 9, new Vector2I(dx, 5), fullPoly);
			}
			for (int dx = 0; dx < 8; dx++)
				AddTilePhysics(tileSet, 5, new Vector2I(dx, 3), fullPoly);
			if (HasLake)
				AddTilePhysics(tileSet, WaterSourceId, WaterTile, fullPoly);

			if (Engine.IsEditorHint() && !string.IsNullOrEmpty(tileSet.ResourcePath))
				ResourceSaver.Save(tileSet, tileSet.ResourcePath);
		}

		private void AddTilePhysics(TileSet tileSet, int sourceId, Vector2I atlasCoords, Vector2[] poly)
		{
			var src = tileSet.GetSource(sourceId) as TileSetAtlasSource;
			if (src == null || !src.HasTile(atlasCoords)) return;
			var data = src.GetTileData(atlasCoords, 0);
			if (data == null || data.GetCollisionPolygonsCount(0) > 0) return;
			data.AddCollisionPolygon(0);
			data.SetCollisionPolygonPoints(0, 0, poly);
		}

		// ── Step 7: StaticBody2D 보완 충돌 ──────────────────────────
		private void Step7_FenceCollision()
		{
			var body = new StaticBody2D
			{
				Name = "FenceCollision",
				CollisionLayer = 4,
				CollisionMask  = 0
			};
			AddChild(body);

			int shapeCount = 0;
			for (int x = 0; x < MapWidth; x++)
				for (int y = 0; y < MapHeight; y++)
				{
					var pos   = new Vector2I(x, y);
					int srcId = _obstacleLayer.GetCellSourceId(pos);
					if (srcId == -1) continue;
					var atlas = _obstacleLayer.GetCellAtlasCoords(pos);
					if (srcId != ObstacleSourceId || atlas != WallTile) continue;
					AddBoxShape(body, x, y);
					shapeCount++;
				}

			if (HasLake && _lakeTiles.Count > 0)
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
				cs.Shape    = new RectangleShape2D { Size = new Vector2((lx2-lx1+1)*16f, (ly2-ly1+1)*16f) };
				cs.Position = new Vector2((lx1+lx2+1)*8f, (ly1+ly2+1)*8f);
				body.AddChild(cs);
			}

			GD.Print($"[충돌체] {shapeCount}개 border shape");
		}

		private void AddBoxShape(StaticBody2D body, int tileX, int tileY)
		{
			var cs = new CollisionShape2D();
			cs.Shape    = new RectangleShape2D { Size = new Vector2(16, 16) };
			cs.Position = new Vector2(tileX*16+8, tileY*16+8);
			body.AddChild(cs);
		}

		// ─── 헬퍼 ──────────────────────────────────────────────────
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
