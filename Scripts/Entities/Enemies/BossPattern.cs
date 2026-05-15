using Godot;

namespace FirstGame.Entities.Enemies
{
	public enum BossPatternType
	{
		ChargeAttack,      // 돌진 — 직선 고속 이동 후 충돌 데미지
		AoeBurst,          // 광역 폭발 — 보스 중심 원형 범위 피해
		SummonMinions,     // 소환 — 미니언 N마리 소환
		BeamSweep,         // 레이저 회전 — 회전 빔
		ProjectileVolley,  // 일제 투사체 — N방향 동시 발사
		Teleport           // 순간이동 — 플레이어 근처 재출현
	}

	[GlobalClass]
	public partial class BossPatternData : Resource
	{
		[Export] public BossPatternType PatternType { get; set; } = BossPatternType.AoeBurst;
		[Export] public float TelegraphDuration { get; set; } = 1.0f; // 경보 표시 시간 (초)
		[Export] public float CastDuration { get; set; } = 0.8f;      // 실제 발동 시간 (초)
		[Export] public float Cooldown { get; set; } = 6.0f;          // 패턴 쿨다운 (초)
		[Export] public float DamageMul { get; set; } = 1.5f;         // 기본 BaseDamage에 곱할 배율
		[Export] public float Radius { get; set; } = 120.0f;          // AoeBurst/BeamSweep 범위 (px)
		[Export] public int ProjectileCount { get; set; } = 8;        // ProjectileVolley 투사체 수
		[Export] public int SummonCount { get; set; } = 2;            // SummonMinions 소환 수
	}
}
