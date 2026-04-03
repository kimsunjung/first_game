using Godot;
using System.Collections.Generic;

namespace FirstGame.Core
{
    public partial class AudioManager : Node
    {
        public static AudioManager Instance { get; private set; }

        private const int SfxPoolSize = 12; // 동시 재생 가능한 효과음 수 (Increased pool size)
        private AudioStreamPlayer _bgmPlayer;
        private List<AudioStreamPlayer> _sfxPool = new();
        private Dictionary<string, AudioStream> _cache = new();

        // 볼륨 설정 (0.0 ~ 1.0)
        [Export] public float SfxVolume { get; set; } = 0.8f;
        [Export] public float BgmVolume { get; set; } = 0.5f;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
                return;
            }

            // BGM 플레이어 생성
            _bgmPlayer = new AudioStreamPlayer();
            _bgmPlayer.Bus = "Master";
            AddChild(_bgmPlayer);

            // SFX 풀 생성
            for (int i = 0; i < SfxPoolSize; i++)
            {
                var player = new AudioStreamPlayer();
                player.Bus = "Master";
                AddChild(player);
                _sfxPool.Add(player);
            }
        }

        /// <summary>
        /// 효과음 재생. path는 "res://Resources/Audio/SFX/" 기준 파일명 (확장자 포함)
        /// 예: PlaySFX("player_attack.wav")
        /// </summary>
        public void PlaySFX(string fileName)
        {
            string path = $"res://Resources/Audio/SFX/{fileName}";
            var stream = LoadAudio(path);
            if (stream == null) return;

            // 유휴 플레이어 찾기
            foreach (var player in _sfxPool)
            {
                if (!player.Playing)
                {
                    player.Stream = stream;
                    player.VolumeDb = Mathf.LinearToDb(SfxVolume);
                    player.Play();
                    return;
                }
            }

            // 모든 플레이어가 사용 중이면 첫 번째 것을 재사용 (Oldest reuse)
            // 더 나은 방식: 가장 오래된 것 찾기 (여기서는 단순하게 0번)
            _sfxPool[0].Stream = stream;
            _sfxPool[0].VolumeDb = Mathf.LinearToDb(SfxVolume);
            _sfxPool[0].Play();
        }

        /// <summary>
        /// 배경음 재생. 기존 BGM은 중지됨.
        /// 예: PlayBGM("field_theme.ogg")
        /// </summary>
        public void PlayBGM(string fileName)
        {
            string path = $"res://Resources/Audio/BGM/{fileName}";
            var stream = LoadAudio(path);
            if (stream == null) return;
            
            // 이미 같은 BGM이 재생 중이면 무시 (Avoid restarting same BGM)
            if (_bgmPlayer.Stream == stream && _bgmPlayer.Playing) return;

            _bgmPlayer.Stream = stream;
            _bgmPlayer.VolumeDb = Mathf.LinearToDb(BgmVolume);
            _bgmPlayer.Play();
        }

        public void StopBGM()
        {
            _bgmPlayer?.Stop();
        }

        public void SetBgmVolume(float volume)
        {
            BgmVolume = Mathf.Clamp(volume, 0f, 1f);
            if (_bgmPlayer != null)
                _bgmPlayer.VolumeDb = Mathf.LinearToDb(BgmVolume);
        }

        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp(volume, 0f, 1f);
        }

        private AudioStream LoadAudio(string path)
        {
            if (_cache.TryGetValue(path, out var cached))
                return cached;

            if (!ResourceLoader.Exists(path))
            {
                // 파일이 없으면 에러 로그를 남기지 않고 조용히 리턴 (개발 중 편의)
                // GD.PrintErr($"AudioManager: 오디오 파일 없음 - {path}");
                return null;
            }

            var stream = GD.Load<AudioStream>(path);
            _cache[path] = stream;
            return stream;
        }

        public override void _ExitTree()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
