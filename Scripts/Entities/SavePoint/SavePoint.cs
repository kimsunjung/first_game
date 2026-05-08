using Godot;
using FirstGame.Core;

namespace FirstGame.Entities
{
    public partial class SavePoint : BaseInteractable
    {
        protected override void OnInteract()
        {
            // 퀘스트 다이얼로그가 가능하면 우선 (저장은 다이얼로그 닫고 다시 누르면 됨)
            if (TryOpenQuestDialog()) return;

            SaveManager.SaveGame("manual");
            AudioManager.Instance?.PlaySFX("save.wav");
        }
    }
}
