using System.Text.Json;
using FirstGame.Data;
using Xunit;

namespace FirstGame.Tests
{
	// ContractType 은 코드 분기/매니페스트 파싱에 쓰인다. ContractProgress 는
	// SaveData.ActiveContracts(v13)로 System.Text.Json 직렬화되므로,
	// 값 안정성과 라운드트립을 고정 검증한다.
	public class ContractTests
	{
		[Fact]
		public void ContractType_IdsAreStable()
		{
			Assert.Equal(0, (int)ContractType.Kill);
			Assert.Equal(1, (int)ContractType.Gather);
			Assert.Equal(2, (int)ContractType.BossKill);
			Assert.Equal(3, (int)ContractType.Mining);
		}

		[Fact]
		public void ContractProgress_DefaultsAreSafe()
		{
			var p = new ContractProgress();
			Assert.Equal("", p.ContractId);
			Assert.Equal(0, p.Progress);
			Assert.False(p.TurnInReady);
		}

		[Fact]
		public void ContractProgress_JsonRoundTrips()
		{
			var p = new ContractProgress { ContractId = "town_kill_goblin", Progress = 5, TurnInReady = true };
			string json = JsonSerializer.Serialize(p);
			var back = JsonSerializer.Deserialize<ContractProgress>(json);

			Assert.NotNull(back);
			Assert.Equal("town_kill_goblin", back.ContractId);
			Assert.Equal(5, back.Progress);
			Assert.True(back.TurnInReady);
		}
	}
}
