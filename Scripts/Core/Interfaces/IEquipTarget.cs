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
		// 일시 buff — duration초 동안 MoveSpeed +moveDelta, AttackSpeed +atkDelta 적용 후 자동 복귀.
		void ApplyBuff(float moveDelta, float atkDelta, float durationSec);
	}
}
