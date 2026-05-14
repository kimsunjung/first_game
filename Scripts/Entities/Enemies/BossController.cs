using Godot;
using System.Collections.Generic;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Entities.Enemies
{
	/// <summary>
	/// EnemyController를 상속. IsBoss=true 또는 명명 미니보스에 사용.
	/// Stats.Patterns 배열에서 랜덤 패턴을 선택해 Telegraph → 실행 순서로 처리.
	/// HP에 따른 3-페이즈 전환: >60% 기본 / 30~60% 빠름 / <30% 빨강 + 최고 빠름.
	/// </summary>
	public partial class BossController : EnemyController
	{
		private enum BossPhase { Phase1, Phase2, Phase3 }
		private BossPhase _currentPhase = BossPhase.Phase1;
		private bool _phase2Entered = false;
		private bool _phase3Entered = false;

		// 패턴 쿨다운 추적 (패턴 인덱스 → 남은 쿨다운)
		private float[] _patternCooldowns;
		private bool _isPerformingPattern = false;
		private float _patternTimer = 0f;
		private BossPatternData _activePattern = null;
		private Telegraph _activeTelegraph = null;
		private enum PatternStep { None, Telegraph, Cast, Done }
		private PatternStep _patternStep = PatternStep.None;

		// 소환된 미니언 추적
		private readonly List<Node> _summonedMinions = new();

		public override void _Ready()
		{
			base._Ready();
			if (Stats?.Patterns != null)
			{
				_patternCooldowns = new float[Stats.Patterns.Length];
				// 첫 패턴 발동 전 초기 딜레이 랜덤화 (보스 입장 직후 즉시 패턴 막기)
				for (int i = 0; i < _patternCooldowns.Length; i++)
					_patternCooldowns[i] = (float)GD.RandRange(2.0, 5.0);
			}
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			UpdatePhase();
			if (Stats?.Patterns != null && Stats.Patterns.Length > 0)
				UpdatePatterns((float)delta);
		}

		private void UpdatePhase()
		{
			if (Stats == null) return;
			float hpRatio = (float)Stats.CurrentHealth / Stats.MaxHealth;
			var newPhase = hpRatio > 0.6f ? BossPhase.Phase1
				: hpRatio > 0.3f ? BossPhase.Phase2
				: BossPhase.Phase3;

			if (newPhase == _currentPhase) return;
			_currentPhase = newPhase;

			if (newPhase == BossPhase.Phase2 && !_phase2Entered)
			{
				_phase2Entered = true;
				// 페이즈 2: 패턴 쿨다운 단축 + Telegraph 시간 단축
				EventManager.TriggerBossPhaseChanged(2);
				GD.Print($"[Boss] {Stats.EnemyTypeName} Phase 2!");
			}
			else if (newPhase == BossPhase.Phase3 && !_phase3Entered)
			{
				_phase3Entered = true;
				// 페이즈 3: modulate 빨강 + 더 빠름
				Modulate = new Color(1.4f, 0.3f, 0.3f, 1f);
				EventManager.TriggerBossPhaseChanged(3);
				GD.Print($"[Boss] {Stats.EnemyTypeName} Phase 3 — 격노!");
			}
		}

		private float GetPhaseSpeedMul() => _currentPhase switch
		{
			BossPhase.Phase2 => 1.3f,
			BossPhase.Phase3 => 1.6f,
			_ => 1.0f
		};

		private float GetPhaseTelegraphMul() => _currentPhase switch
		{
			BossPhase.Phase2 => 0.8f,
			BossPhase.Phase3 => 0.6f,
			_ => 1.0f
		};

		private void UpdatePatterns(float delta)
		{
			// 쿨다운 감소
			float speedMul = GetPhaseSpeedMul();
			for (int i = 0; i < _patternCooldowns.Length; i++)
				if (_patternCooldowns[i] > 0f)
					_patternCooldowns[i] -= delta * speedMul;

			if (_isPerformingPattern)
			{
				_patternTimer -= delta;
				if (_patternTimer <= 0f)
					AdvancePatternStep();
				return;
			}

			// 사용 가능한 패턴 선택
			TrySelectPattern();
		}

		private void TrySelectPattern()
		{
			if (Stats.Patterns == null || Stats.Patterns.Length == 0) return;
			// 준비된 패턴 수집
			var candidates = new List<int>();
			for (int i = 0; i < Stats.Patterns.Length; i++)
				if (_patternCooldowns[i] <= 0f)
					candidates.Add(i);
			if (candidates.Count == 0) return;

			int idx = candidates[(int)(GD.Randi() % (uint)candidates.Count)];
			StartPattern(Stats.Patterns[idx], idx);
		}

		private void StartPattern(BossPatternData pattern, int idx)
		{
			_activePattern = pattern;
			_isPerformingPattern = true;
			_patternStep = PatternStep.Telegraph;

			float telegraphTime = pattern.TelegraphDuration * GetPhaseTelegraphMul();
			_patternTimer = telegraphTime;

			// Telegraph 표시
			var player = GameManager.Instance?.Player;
			var targetPos = player != null ? ((Node2D)player).GlobalPosition : GlobalPosition;
			ShowTelegraph(pattern, targetPos, telegraphTime);

			// 쿨다운 재설정
			float speedMul = GetPhaseSpeedMul();
			_patternCooldowns[idx] = pattern.Cooldown / speedMul;
		}

		private void ShowTelegraph(BossPatternData pattern, Vector2 targetPos, float duration)
		{
			switch (pattern.PatternType)
			{
				case BossPatternType.ChargeAttack:
					_activeTelegraph = Telegraph.CreateLine(GetParent(), GlobalPosition,
						pattern.Radius, GlobalPosition.DirectionTo(targetPos).Angle(), duration);
					break;
				case BossPatternType.AoeBurst:
					_activeTelegraph = Telegraph.CreateCircle(GetParent(), GlobalPosition, pattern.Radius, duration);
					break;
				case BossPatternType.BeamSweep:
					_activeTelegraph = Telegraph.CreateLine(GetParent(), GlobalPosition,
						pattern.Radius, GlobalPosition.DirectionTo(targetPos).Angle(), duration);
					break;
				case BossPatternType.ProjectileVolley:
					_activeTelegraph = Telegraph.CreateCircle(GetParent(), GlobalPosition, pattern.Radius * 0.7f, duration);
					break;
				case BossPatternType.SummonMinions:
					_activeTelegraph = Telegraph.CreateCircle(GetParent(), GlobalPosition, 80f, duration);
					break;
				case BossPatternType.Teleport:
					// 텔레포트는 시각 없음 — 짧은 번쩍임으로 대신
					Modulate = _currentPhase == BossPhase.Phase3
						? new Color(1.4f, 0.3f, 0.3f, 0.3f)
						: new Color(1, 1, 1, 0.3f);
					break;
			}
		}

		private void AdvancePatternStep()
		{
			if (_patternStep == PatternStep.Telegraph)
			{
				_patternStep = PatternStep.Cast;
				_patternTimer = _activePattern.CastDuration;
				ExecutePattern(_activePattern);
			}
			else if (_patternStep == PatternStep.Cast)
			{
				_patternStep = PatternStep.Done;
				_isPerformingPattern = false;
				_activePattern = null;
				_activeTelegraph = null;
				Modulate = _currentPhase == BossPhase.Phase3
					? new Color(1.4f, 0.3f, 0.3f, 1f)
					: Colors.White;
			}
		}

		private void ExecutePattern(BossPatternData pattern)
		{
			var player = GameManager.Instance?.Player;
			if (player == null) return;

			int dmg = Mathf.RoundToInt(Stats.BaseDamage * pattern.DamageMul);

			switch (pattern.PatternType)
			{
				case BossPatternType.AoeBurst:
					ExecuteAoeBurst(dmg, pattern.Radius);
					break;
				case BossPatternType.ChargeAttack:
					ExecuteCharge(dmg, (Node2D)player);
					break;
				case BossPatternType.BeamSweep:
					ExecuteBeamSweep(dmg, pattern.Radius);
					break;
				case BossPatternType.ProjectileVolley:
					ExecuteVolley(dmg, pattern.ProjectileCount);
					break;
				case BossPatternType.SummonMinions:
					ExecuteSummon(pattern.SummonCount);
					break;
				case BossPatternType.Teleport:
					ExecuteTeleport((Node2D)player);
					break;
			}
		}

		private void ExecuteAoeBurst(int dmg, float radius)
		{
			var player = GameManager.Instance?.Player as Node2D;
			if (player == null) return;
			float dist = GlobalPosition.DistanceTo(player.GlobalPosition);
			if (dist <= radius)
			{
				var damageable = player as IDamageable;
				damageable?.TakeDamage(dmg, ElementType.None);
			}
			// 폭발 이펙트 (토스트)
			// 폭발 효과 — HitStop으로 시각 피드백
			UIEffectManager.HitStop(0.1f, 0.08f);
		}

		private void ExecuteCharge(int dmg, Node2D player)
		{
			var dir = GlobalPosition.DirectionTo(player.GlobalPosition);
			var tween = CreateTween();
			tween.TweenProperty(this, "global_position", player.GlobalPosition, 0.35f)
				.SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
			// 충돌 데미지는 overlap 체크 대신 단순 거리로
			tween.TweenCallback(Callable.From(() =>
			{
				if (IsInstanceValid(this) && GameManager.Instance?.Player is Node2D p2
					&& GlobalPosition.DistanceTo(p2.GlobalPosition) < 60f)
				{
					(GameManager.Instance?.Player as IDamageable)?.TakeDamage(dmg, ElementType.None);
				}
			}));
		}

		private void ExecuteBeamSweep(int dmg, float length)
		{
			var player = GameManager.Instance?.Player as Node2D;
			if (player == null) return;
			// 범위 내면 피해
			float dist = GlobalPosition.DistanceTo(player.GlobalPosition);
			if (dist <= length)
				(player as IDamageable)?.TakeDamage(dmg, ElementType.None);
		}

		private void ExecuteVolley(int dmg, int count)
		{
			if (Stats.ProjectileTexture == null) return;
			float step = Mathf.Tau / count;
			for (int i = 0; i < count; i++)
			{
				float angle = step * i;
				var proj = new EnemyProjectile();
				proj.Direction = Vector2.Right.Rotated(angle);
				proj.Damage = dmg;
				proj.Speed = 250f;
				proj.Texture = Stats.ProjectileTexture;
				proj.Scale = Vector2.One * Stats.ProjectileScale;
				GetParent().AddChild(proj);
				proj.GlobalPosition = GlobalPosition;
			}
		}

		private void ExecuteSummon(int count)
		{
			// 소환은 단순 표시 — 실제 소환은 스폰 데이터 없이 간단한 노드로 처리
			GD.Print($"[Boss] 미니언 {count}마리 소환 시도 (스폰 구현 필요)");
		}

		private void ExecuteTeleport(Node2D player)
		{
			// 플레이어 근처 무작위 방향으로 텔레포트
			float angle = (float)GD.RandRange(0, Mathf.Tau);
			float dist = (float)GD.RandRange(100, 160);
			GlobalPosition = player.GlobalPosition + Vector2.Right.Rotated(angle) * dist;
			Modulate = _currentPhase == BossPhase.Phase3
				? new Color(1.4f, 0.3f, 0.3f, 1f)
				: Colors.White;
		}
	}

}
