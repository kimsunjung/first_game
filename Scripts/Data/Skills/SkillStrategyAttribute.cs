using System;

namespace FirstGame.Data.Skills
{
	/// <summary>
	/// 스킬 전략 클래스에 적용하여 SkillType과 자동 매핑.
	/// SkillStrategyFactory가 reflection으로 자동 등록하므로
	/// 새 스킬 추가 시 이 어트리뷰트만 붙이면 됨 (OCP 준수).
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class SkillStrategyAttribute : Attribute
	{
		public SkillType Type { get; }
		public SkillStrategyAttribute(SkillType type) => Type = type;
	}
}
