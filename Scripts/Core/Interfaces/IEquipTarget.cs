using FirstGame.Data;

namespace FirstGame.Core.Interfaces
{
	/// <summary>
	/// 장비 장착/해제 시 스탯 변경을 위한 인터페이스.
	/// Inventory(Data)가 PlayerStats(Data)를 직접 참조하지 않도록 함.
	/// </summary>
	public interface IEquipTarget
	{
		void ModifyBaseDamage(int delta);
		void ModifyMaxHealth(int delta);
		void ModifyDefense(int delta);
		void ModifyMaxMp(int delta);
		void ModifyCritRate(float delta);
		void ModifyMoveSpeed(float delta);
		void ModifyAttackSpeed(float delta);
		void ModifyLifesteal(float delta);
		void Heal(int amount);
		void RestoreMp(int amount);
		bool LearnSkill(SkillData skill);
		// 일시 buff — duration초 동안 MoveSpeed/AttackSpeed/BaseDamage/Defense/CritRate 적용 후 자동 복귀.
		void ApplyBuff(float moveDelta, float atkDelta, float durationSec);
		// 확장 buff — Damage/Defense/Crit 추가. 기본 구현은 좁은 ApplyBuff로 fallback.
		void ApplyBuffEx(float moveDelta, float atkDelta, int dmgDelta, int defDelta, float critDelta, float durationSec)
			=> ApplyBuff(moveDelta, atkDelta, durationSec);
		// 상태이상 해제 — mask 비트: 1=독, 2=빙결, 4=저주. 기본 no-op.
		void CureStatuses(int mask) { }
	}
}
