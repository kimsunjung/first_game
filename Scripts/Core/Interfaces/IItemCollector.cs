using System.Collections.Generic;
using FirstGame.Data;

namespace FirstGame.Core.Interfaces
{
	/// <summary>
	/// 아이템을 수집할 수 있는 엔티티가 구현하는 인터페이스.
	/// FieldItem(Objects)이 PlayerController(Entities)를 직접 참조하지 않도록 함.
	/// affixes는 장신구 드롭의 인스턴스별 옵션 — null 또는 빈 리스트는 affix 없음.
	/// </summary>
	public interface IItemCollector
	{
		bool CollectItem(ItemData item, int quantity, List<ItemAffix> affixes = null);
	}
}
