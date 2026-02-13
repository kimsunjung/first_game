namespace FirstGame.Data
{
    // 저장 데이터 클래스 (JSON 직렬화용)
    public class SaveData
    {
        public float PlayerPosX { get; set; }
        public float PlayerPosY { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerMaxHealth { get; set; }
        public int PlayerGold { get; set; }
        public string Timestamp { get; set; }
    }
}
