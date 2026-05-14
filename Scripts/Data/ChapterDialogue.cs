using System.Collections.Generic;

namespace FirstGame.Data
{
	/// <summary>
	/// 메인 스토리 챕터별 NPC 대사 정적 사전.
	/// 각 NPC가 상호작용 시 현재 챕터에 맞는 한 줄 대사를 토스트로 표시.
	/// main_story.md 기준. 진행도 따라 같은 NPC가 다른 말을 함.
	/// </summary>
	public static class ChapterDialogue
	{
		// (npcId, chapter) → 대사. 누락 시 빈 문자열 반환(토스트 표시 안 함).
		private static readonly Dictionary<(string, Chapter), string> _lines = new()
		{
			// ─── save_point (노현자) ────────────────────────────────────
			{("save_point", Chapter.Prologue),  "카엘, 자네는 아직 어리지만 이 마을에 자네만 한 청년이 없네. 외곽을 살펴주게."},
			{("save_point", Chapter.Chapter1),  "잘 돌아왔네. 자네가 가져온 그 부적… 본 적 없는 문양이군."},
			{("save_point", Chapter.Chapter2),  "묘지가… 울고 있다네. 그런 소리는 백 년 만이야."},
			{("save_point", Chapter.Chapter3),  "리치… 천 년 전 영웅들이 봉인했던 그 이름이야. 자네가 그 마지막 영웅이 되겠군."},
			{("save_point", Chapter.Final),     "조심하게, 카엘. 천 년의 어둠이 자네를 기다리고 있어."},
			{("save_point", Chapter.Ending),    "수고했네, 카엘. 자네 이름은 다음 천 년에 새겨질 거야."},

			// ─── shop ─────────────────────────────────────────────────
			{("shop", Chapter.Prologue),  "포션은 넉넉히 가져가게. 공짜는 아니야… 농담이고, 첫 병은 서비스일세."},
			{("shop", Chapter.Chapter1),  "오크들이 험해졌다더군. 상등급 포션으로 바꿔드릴까?"},
			{("shop", Chapter.Chapter2),  "성수 가져가게. 비싸지만 망자에겐 확실해."},
			{("shop", Chapter.Chapter3),  "이번엔 진짜 공짜야. 살아만 돌아오게."},
			{("shop", Chapter.Final),     "행운을 빌어, 영웅이여."},
			{("shop", Chapter.Ending),    "이젠 진짜 단골이 됐군. 다음 잔도 공짜!"},

			// ─── blacksmith ───────────────────────────────────────────
			{("blacksmith", Chapter.Prologue),  "그 검, 내가 손본 거다. 부러뜨려 오면 너부터 갈아버릴 거야."},
			{("blacksmith", Chapter.Chapter1),  "오크 도끼? 녹여서 자네 검에 박아주지."},
			{("blacksmith", Chapter.Chapter2),  "뼈 무기는 가져오지 말게… 농담이야. 가져와."},
			{("blacksmith", Chapter.Chapter3),  "최후의 강화다. 이번이 자네 검의 마지막 불일지도 몰라. 살아 돌아와라."},
			{("blacksmith", Chapter.Final),     "그 검 하나에 마을의 미래가 걸렸어."},
			{("blacksmith", Chapter.Ending),    "그 검, 이젠 박물관 행이지. …그래도 한 번만 더 갈아줄까?"},

			// ─── skill_shop ───────────────────────────────────────────
			{("skill_shop", Chapter.Prologue),  "어린 친구, 첫 스킬은 무료로 가르쳐 주지."},
			{("skill_shop", Chapter.Chapter1),  "오크와 싸워봤다면 이제 진짜 검술이 필요할 걸세. 잘 골라보게."},
			{("skill_shop", Chapter.Chapter2),  "언데드는 신성 마법에 약해. 이 스킬북이 자네에게 어울리겠어."},
			{("skill_shop", Chapter.Chapter3),  "리치를 상대하려면 가진 모든 기술을 익혀두게."},
			{("skill_shop", Chapter.Final),     "지금 자네에게 가르칠 게 더 남았는지 모르겠군."},
			{("skill_shop", Chapter.Ending),    "전설이 된 사람에게 가르칠 게 더 남았을까?"},

			// ─── material_shop ────────────────────────────────────────
			{("material_shop", Chapter.Prologue),  "광물 필요하면 말씀하세요. 신선한 것만 들여옵니다."},
			{("material_shop", Chapter.Chapter1),  "광부들이 광산에서 이상한 검은 결정을 캐 오고 있어. 손대지는 말게."},
			{("material_shop", Chapter.Chapter2),  "검은 결정이 점점 늘어나… 뭔가 이상해."},
			{("material_shop", Chapter.Chapter3),  "검은 결정들… 전부 자네에게 주지. 봉인에 쓰일지도 몰라."},
			{("material_shop", Chapter.Final),     "결정이 떨리고 있어… 그가 깨어나고 있나 봐."},
			{("material_shop", Chapter.Ending),    "검은 결정이 평범한 돌이 됐어. 자네 덕분이야."},

			// ─── teleport ─────────────────────────────────────────────
			{("teleport", Chapter.Prologue),  "아직 자네에겐 먼 길은 무리야. 외곽까진 두 발로 걸어보게."},
			{("teleport", Chapter.Chapter1),  "오크 던전을 보고 왔다면 다음 행선지도 곧 열릴 걸세."},
			{("teleport", Chapter.Chapter2),  "이제 자네도 멀리 갈 자격이 있군. 묘지로 보내주지."},
			{("teleport", Chapter.Chapter3),  "폐허로 가는 좌표는 위험하지만 보내드리지요."},
			{("teleport", Chapter.Final),     "심연 던전 입구까지… 부디 살아 돌아오시오."},
			{("teleport", Chapter.Ending),    "어디든 보내드리지요, 영웅이여."},
		};

		public static string Get(string npcId, Chapter chapter)
		{
			if (string.IsNullOrEmpty(npcId)) return "";
			return _lines.TryGetValue((npcId, chapter), out var line) ? line : "";
		}
	}
}
