using FirstGame.Data;

namespace FirstGame.Core.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(int damage);
        // 속성 인식 데미지. 기본 구현은 element 무시하고 int 오버로드로 전달.
        void TakeDamage(int damage, ElementType element) => TakeDamage(damage);

        // 실제 적용된(속성 보정·방어·잔여 HP 클램프 후) 피해를 반환. 흡혈류 스킬이
        // "가한 피해의 N분의 1" 회복을 정확히 산정하기 위한 경로. 기본 구현은 보정
        // 정보를 모르므로 입력값을 그대로 반환 — EnemyController가 정확값을 오버라이드.
        int TakeDamageReporting(int damage, ElementType element)
        {
            TakeDamage(damage, element);
            return damage;
        }

        // 상태이상 부여. 기본 구현은 no-op — 구현체가 자체 Stats.ApplyStatus 위임.
        // 플레이어 스킬이 적에게, 적 공격이 플레이어에게 상태이상을 거는 통합 경로.
        void ApplyStatusEffect(StatusEffect status, float duration) { }
    }
}
