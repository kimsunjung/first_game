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
		void Heal(int amount);
		bool LearnSkill(SkillData skill);
	}
}
