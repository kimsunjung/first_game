using FirstGame.Data;

namespace FirstGame.Core.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(int damage);
        // 속성 인식 데미지. 기본 구현은 element 무시하고 int 오버로드로 전달.
        void TakeDamage(int damage, ElementType element) => TakeDamage(damage);

        // 상태이상 부여. 기본 구현은 no-op — 구현체가 자체 Stats.ApplyStatus 위임.
        // 플레이어 스킬이 적에게, 적 공격이 플레이어에게 상태이상을 거는 통합 경로.
        void ApplyStatusEffect(StatusEffect status, float duration) { }
    }
}
