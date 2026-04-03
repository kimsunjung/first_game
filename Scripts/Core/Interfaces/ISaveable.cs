using FirstGame.Data;

namespace FirstGame.Core.Interfaces
{
	/// <summary>
	/// 저장/로드가 필요한 시스템이 구현하는 인터페이스.
	/// SaveManager가 각 시스템의 내부를 알 필요 없이 저장/복원 가능.
	/// </summary>
	public interface ISaveable
	{
		void WriteSaveData(SaveData data);
		void ReadSaveData(SaveData data);
	}
}
