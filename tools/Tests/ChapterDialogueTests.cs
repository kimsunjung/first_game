using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FirstGame.Data;
using Xunit;

namespace FirstGame.Tests
{
    // ChapterDialogue 는 NPC 상호작용 대사 사전.
    // 보장 범위를 정확히 한다:
    //   1) 사전에 "등록된" NPC 는 6개 챕터 전부에 비어있지 않은 대사가 있어야 한다(부분 누락 금지).
    //   2) 사전에 등록된 NPC id 는 실제 씬의 NpcId 로 존재해야 한다(개명/삭제로 인한 사전 부패 차단).
    // (모든 씬 NpcId 가 대사를 가져야 한다는 보장은 아님 — 일부 NPC는 의도적으로 무대사.
    //  씬에는 있으나 사전에 없는 NpcId 목록은 진단용으로 노출만 한다.)
    public class ChapterDialogueTests
    {
        private static readonly Chapter[] AllChapters =
            (Chapter[])Enum.GetValues(typeof(Chapter));

        [Fact]
        public void Get_NullOrEmptyNpc_ReturnsEmpty()
        {
            Assert.Equal("", ChapterDialogue.Get(null, Chapter.Prologue));
            Assert.Equal("", ChapterDialogue.Get("", Chapter.Prologue));
        }

        [Fact]
        public void Get_UnknownNpc_ReturnsEmpty()
        {
            Assert.Equal("", ChapterDialogue.Get("___no_such_npc___", Chapter.Prologue));
        }

        [Fact]
        public void Get_KnownEntry_ReturnsConfiguredLine()
        {
            Assert.False(string.IsNullOrWhiteSpace(
                ChapterDialogue.Get("save_point", Chapter.Prologue)));
        }

        // 1) 등록된 NPC 는 전 챕터 대사 완비 (부분 누락 = 회귀)
        [Fact]
        public void RegisteredNpcs_HaveDialogue_ForAllChapters()
        {
            var npcIds = GetDialogueKeys().Select(k => k.Item1).Distinct().OrderBy(x => x).ToList();
            Assert.NotEmpty(npcIds);

            var missing = new List<string>();
            foreach (var npc in npcIds)
                foreach (var ch in AllChapters)
                    if (string.IsNullOrWhiteSpace(ChapterDialogue.Get(npc, ch)))
                        missing.Add($"{npc} @ {ch}");

            Assert.True(missing.Count == 0,
                "등록 NPC 대사 누락 (npc @ chapter):\n" + string.Join("\n", missing));
        }

        // 2) 사전에 등록된 NPC id 는 실제 씬에 NpcId 로 존재해야 한다 (사전 부패 차단)
        [Fact]
        public void RegisteredDialogueNpcs_AllExistInScenes()
        {
            var sceneNpcIds = ScanSceneNpcIds();
            Assert.NotEmpty(sceneNpcIds); // 스캔 자체가 동작하는지 가드

            var registered = GetDialogueKeys().Select(k => k.Item1).Distinct().ToList();
            var rotted = registered.Where(id => !sceneNpcIds.Contains(id)).OrderBy(x => x).ToList();

            Assert.True(rotted.Count == 0,
                "대사 사전이 어떤 씬에도 없는 NpcId 를 가리킴(개명/삭제 의심):\n"
                + string.Join(", ", rotted));
        }

        // 진단용 — 씬엔 있으나 사전에 없는 NpcId. 하드 실패 아님(무대사 NPC 는 정상일 수 있음).
        // 콘텐츠 갭 가시화를 위해 메시지로만 남긴다(항상 통과).
        [Fact]
        public void SceneNpcs_WithoutDialogue_AreReportedNotFailed()
        {
            var sceneNpcIds = ScanSceneNpcIds();
            var registered = GetDialogueKeys().Select(k => k.Item1).Distinct().ToHashSet();
            var noDialogue = sceneNpcIds.Where(id => !registered.Contains(id)).OrderBy(x => x).ToList();
            // 진단 출력만 — 실패시키지 않음.
            Console.WriteLine("[진단] 씬에 존재하나 ChapterDialogue 미등록 NpcId: "
                + (noDialogue.Count == 0 ? "(없음)" : string.Join(", ", noDialogue)));
            Assert.True(true);
        }

        // ── helpers ──────────────────────────────────────────────────
        private static List<(string, Chapter)> GetDialogueKeys()
        {
            var f = typeof(ChapterDialogue).GetField(
                "_lines", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(f);
            var dict = (IDictionary)f.GetValue(null);
            var result = new List<(string, Chapter)>();
            foreach (DictionaryEntry e in dict)
            {
                var t = e.Key.GetType();
                result.Add(((string)t.GetField("Item1").GetValue(e.Key),
                            (Chapter)t.GetField("Item2").GetValue(e.Key)));
            }
            return result;
        }

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "first_game.csproj")))
                dir = dir.Parent;
            Assert.NotNull(dir); // 리포 루트를 못 찾으면 테스트 환경 문제
            return dir.FullName;
        }

        private static readonly Regex NpcIdRe =
            new(@"NpcId\s*=\s*""([^""]+)""", RegexOptions.Compiled);

        private static HashSet<string> ScanSceneNpcIds()
        {
            var root = FindRepoRoot();
            var scenes = Path.Combine(root, "Scenes");
            var ids = new HashSet<string>();
            foreach (var f in Directory.EnumerateFiles(scenes, "*.tscn", SearchOption.AllDirectories))
                foreach (Match m in NpcIdRe.Matches(File.ReadAllText(f)))
                    ids.Add(m.Groups[1].Value);
            return ids;
        }
    }
}
