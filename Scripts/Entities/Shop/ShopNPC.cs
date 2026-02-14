using Godot;
using FirstGame.Data;
using FirstGame.UI;

namespace FirstGame.Entities.Shop
{
    public partial class ShopNPC : Area2D
    {
        [Export] public ItemData[] ShopItems { get; set; }  // 판매 물건 목록
        [Export] public string ShopName { get; set; } = "상점";

        private bool _playerInRange = false;
        private Label _promptLabel;

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;

            _promptLabel = GetNode<Label>("PromptLabel");
            _promptLabel.Visible = false;
        }

        public override void _Process(double delta)
        {
            if (_playerInRange && Input.IsActionJustPressed("interact"))
            {
                // 게임이 이미 일시정지 상태면 무시 (게임오버, 인벤토리, 상점 이미 열림)
                if (GetTree().Paused) return;

                // 씬 구조에 맞춰 ShopUI 찾기 (Main/ShopUI)
                var shopUI = GetTree().Root.GetNodeOrNull<ShopUI>("Main/ShopUI");
                if (shopUI != null)
                {
                    shopUI.OpenShop(ShopItems, ShopName);
                }
                else
                {
                    GD.PrintErr("ShopNPC: Main/ShopUI node not found!");
                }
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body.IsInGroup("Player"))
            {
                _playerInRange = true;
                _promptLabel.Visible = true;
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body.IsInGroup("Player"))
            {
                _playerInRange = false;
                _promptLabel.Visible = false;
            }
        }
    }
}
