using System;
using FirstGame.Data;
using Xunit;

namespace FirstGame.Tests
{
    // 이 값들은 세이브 파일에 정수/비트마스크/문자열로 영속화된다.
    // 재배치/개명하면 기존 세이브가 조용히 깨지므로(잘못된 클래스·상태·진행도 로드)
    // 상수로 고정 검증한다. 의도적으로 바꿀 땐 마이그레이션 + 이 테스트 동시 갱신.
    public class EnumStabilityTests
    {
        [Fact]
        public void PlayerClass_IdsAreStable()
        {
            Assert.Equal(0, (int)PlayerClass.Warrior);
            Assert.Equal(1, (int)PlayerClass.Mage);
            Assert.Equal(2, (int)PlayerClass.Archer);
        }

        [Fact]
        public void StatusEffect_BitsAreStable_AndFlags()
        {
            Assert.Equal(0, (int)StatusEffect.None);
            Assert.Equal(1, (int)StatusEffect.Poison);
            Assert.Equal(2, (int)StatusEffect.Freeze);
            Assert.Equal(4, (int)StatusEffect.Curse);
            Assert.Equal(8, (int)StatusEffect.Burn);
            Assert.Equal(16, (int)StatusEffect.Shock);

            // [Flags] 조합이 비트 OR 로 동작하는지 (저장된 복합 상태 무결성)
            var combo = StatusEffect.Poison | StatusEffect.Shock;
            Assert.True(combo.HasFlag(StatusEffect.Poison));
            Assert.True(combo.HasFlag(StatusEffect.Shock));
            Assert.False(combo.HasFlag(StatusEffect.Freeze));
            Assert.Equal(17, (int)combo);

            Assert.NotNull(typeof(StatusEffect).GetCustomAttributes(typeof(FlagsAttribute), false));
            Assert.NotEmpty(typeof(StatusEffect).GetCustomAttributes(typeof(FlagsAttribute), false));
        }

        [Fact]
        public void ElementType_IdsAreStable()
        {
            Assert.Equal(0, (int)ElementType.None);
            Assert.Equal(1, (int)ElementType.Fire);
            Assert.Equal(2, (int)ElementType.Ice);
            Assert.Equal(3, (int)ElementType.Lightning);
            Assert.Equal(4, (int)ElementType.Holy);
            Assert.Equal(5, (int)ElementType.Dark);
            Assert.Equal(6, (int)ElementType.Poison);
        }

        // Chapter 는 상점/스킬 게이팅에서 (int) 비교로 쓰이고 진행도가 세이브된다.
        // 순서가 곧 진행 단조성이므로 값 고정.
        [Fact]
        public void Chapter_OrderIsStable()
        {
            Assert.Equal(0, (int)Chapter.Prologue);
            Assert.Equal(1, (int)Chapter.Chapter1);
            Assert.Equal(2, (int)Chapter.Chapter2);
            Assert.Equal(3, (int)Chapter.Chapter3);
            Assert.Equal(4, (int)Chapter.Final);
            Assert.Equal(5, (int)Chapter.Ending);
            Assert.True((int)Chapter.Prologue < (int)Chapter.Ending);
        }

        // ChapterFlags 문자열은 세이브의 ChapterFlags 리스트에 그대로 저장된다.
        // 개명 시 v9→v10 backfill/진행도 판정이 깨지므로 리터럴 고정.
        [Fact]
        public void ChapterFlags_StringsAreStable()
        {
            Assert.Equal("flag_outpost_entered", ChapterFlags.OutpostEntered);
            Assert.Equal("flag_orc_warlord_killed", ChapterFlags.OrcWarlordKilled);
            Assert.Equal("flag_skeleton_king_killed", ChapterFlags.SkeletonKingKilled);
            Assert.Equal("flag_abyss_unsealed", ChapterFlags.AbyssUnsealed);
            Assert.Equal("flag_lich_killed", ChapterFlags.LichKilled);
        }
    }
}
