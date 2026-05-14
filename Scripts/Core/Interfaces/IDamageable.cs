using FirstGame.Data;

namespace FirstGame.Core.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(int damage);
        // 속성 인식 데미지. 기본 구현은 element 무시하고 int 오버로드로 전달.
        void TakeDamage(int damage, ElementType element) => TakeDamage(damage);
    }
}
