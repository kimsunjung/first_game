using FirstGame.Core.Interfaces;

namespace FirstGame.Data.Skills
{
	public interface ISkillStrategy
	{
		void Execute(ISkillTarget target, SkillData skill);
	}
}
