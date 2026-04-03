using Godot;
using FirstGame.Core;

namespace FirstGame.Entities
{
    public partial class SavePoint : BaseInteractable
    {
        protected override void OnInteract()
        {
            SaveManager.SaveGame("manual");
            AudioManager.Instance?.PlaySFX("save.wav");
        }
    }
}
