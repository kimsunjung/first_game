using Godot;
using FirstGame.Core;
using FirstGame.Data;

namespace FirstGame.Maps
{
	/// <summary>
	/// 씬마다 붙이는 가벼운 날씨/분위기 노드. 새 PNG 없이 코드로
	/// 화면 오버레이 + CPU 파티클 + 약한 상태이상 위험을 만든다.
	/// 저장/로드에 날씨 상태를 저장하지 않는다 — 씬 컨셉 고정 날씨.
	/// DayNightCycle 은 시간 표시 전용으로 유지(여기서 동적 스케줄 없음).
	/// </summary>
	public enum BiomeWeatherKind
	{
		Clear,
		MeadowBreeze,
		ForestMist,
		GraveFog,
		SeaRain,
		SeaStorm,
		Snowfall,
		Blizzard,
		VolcanicAsh,
		Heatwave,
		CrystalDust,
		DungeonGloom
	}

	public partial class BiomeWeatherController : Node2D
	{
		[Export] public BiomeWeatherKind WeatherKind { get; set; } = BiomeWeatherKind.Clear;
		[Export] public string DisplayName { get; set; } = "";
		// 오버레이 알파 배율(0~1). 모바일 가독성을 위해 기본 낮게.
		[Export] public float VisualIntensity { get; set; } = 0.18f;
		[Export] public Color OverlayColor { get; set; } = new Color(1, 1, 1, 0);
		[Export] public Color ParticleColor { get; set; } = new Color(1, 1, 1, 0.5f);
		[Export] public int ParticleCount { get; set; } = 40;
		[Export] public Vector2 Wind { get; set; } = new Vector2(0, 1);
		[Export] public bool HazardEnabled { get; set; } = false;
		[Export] public float HazardInterval { get; set; } = 4.0f;
		[Export] public StatusEffect InflictedStatus { get; set; } = StatusEffect.None;
		[Export] public float StatusChance { get; set; } = 0.05f;
		[Export] public float StatusDuration { get; set; } = 3.0f;

		private float _hazardTimer;
		private CanvasLayer _layer;

		public override void _Ready()
		{
			_hazardTimer = HazardInterval;
			BuildVisuals();
		}

		// 화면 좌표계(CanvasLayer)로 오버레이/파티클 생성 — 카메라 줌/이동과 무관하게 일정.
		private void BuildVisuals()
		{
			var vp = GetViewport().GetVisibleRect().Size;

			_layer = new CanvasLayer { Layer = 0 }; // 월드 위, HUD(기본 layer 1) 아래
			AddChild(_layer);

			if (WeatherKind != BiomeWeatherKind.Clear && OverlayColor.A > 0f)
			{
				var overlay = new ColorRect
				{
					Color = new Color(OverlayColor.R, OverlayColor.G, OverlayColor.B,
						OverlayColor.A * Mathf.Clamp(VisualIntensity, 0f, 1f)),
					MouseFilter = Control.MouseFilterEnum.Ignore,
				};
				overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
				overlay.Size = vp;
				_layer.AddChild(overlay);
			}

			if (WeatherKind != BiomeWeatherKind.Clear && ParticleCount > 0)
			{
				var p = new CpuParticles2D
				{
					Amount = Mathf.Clamp(ParticleCount, 1, 120), // 모바일 보호 상한
					Lifetime = 3.0f,
					Position = new Vector2(vp.X * 0.5f, -16f),
					EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle,
					EmissionRectExtents = new Vector2(vp.X * 0.6f, 8f),
					Direction = Wind.LengthSquared() > 0.001f ? Wind.Normalized() : Vector2.Down,
					Spread = 18f,
					Gravity = new Vector2(Wind.X * 18f, Mathf.Max(40f, Wind.Y * 90f)),
					InitialVelocityMin = 30f,
					InitialVelocityMax = 70f,
					ScaleAmountMin = 1.5f,
					ScaleAmountMax = 3.0f,
					Color = ParticleColor,
					Emitting = true,
				};
				_layer.AddChild(p);
			}
		}

		public override void _Process(double delta)
		{
			if (!HazardEnabled || InflictedStatus == StatusEffect.None) return;
			_hazardTimer -= (float)delta;
			if (_hazardTimer > 0f) return;
			_hazardTimer = HazardInterval;

			if (GD.Randf() > StatusChance) return;
			var stats = GameManager.Instance?.Player?.Stats;
			stats?.ApplyStatus(InflictedStatus, StatusDuration);
		}
	}
}
