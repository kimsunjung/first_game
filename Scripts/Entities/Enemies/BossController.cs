using Godot;
using System.Collections.Generic;
using FirstGame.Core;
using FirstGame.Core.Interfaces;
using FirstGame.Data;

namespace FirstGame.Entities.Enemies
{
	/// <summary>
	/// 보스 패턴 브레인. EnemyController._Ready가 Stats.IsBoss && Patterns != null일 때
	/// 자식 노드로 추가. 부모 EnemyController의 위치/HP/Stats를 참조해 텔레그래프·패턴 실행.
	/// HP에 따른 3-페이즈 전환: >60% 기본 / 30~60% 빠름 / <30% 빨강 + 최고 빠름.
	/// </summary>
	public partial class BossController : Node2D
	{
		private EnemyController _host;
		private EnemyStats Stats => _host?.Stats;

		private enum BossPhase { Phase1, Phase2, Phase3 }
		private BossPhase _currentPhase = BossPhase.Phase1;
		private bool _phase2Entered = false;
		private bool _phase3Entered = false;

		private float[] _patternCooldowns;
		private bool _isPerformingPattern = false;
		private float _patternTimer = 0f;
		private BossPatternData _activePattern = null;
		private Telegraph _activeTelegraph = null;
		private enum PatternStep { None, Telegraph, Cast, Done }
		private PatternStep _patternStep = PatternStep.None;

		public void Attach(EnemyController host)
		{
			_host = host;
		}

		public override void _Ready()
		{
			if (_host == null) _host = GetParent() as EnemyController;
			if (Stats?.Patterns != null && Stats.Patterns.Length > 0)
			{
				_patternCooldowns = new float[Stats.Patterns.Length];
				for (int i = 0; i < _patternCooldowns.Length; i++)
					_patternCooldowns[i] = (float)GD.RandRange(2.0, 5.0);
			}
		}

		public override void _Process(double delta)
		{
			if (_host == null || !IsInstanceValid(_host) || Stats == null) return;
			if (Stats.Patterns == null || Stats.Patterns.Length == 0) return;
			UpdatePhase();
			UpdatePatterns((float)delta);
		}

		private void UpdatePhase()
		{
			float hpRatio = (float)Stats.CurrentHealth / Stats.MaxHealth;
			var newPhase = hpRatio > 0.6f ? BossPhase.Phase1
				: hpRatio > 0.3f ? BossPhase.Phase2
				: BossPhase.Phase3;
			if (newPhase == _currentPhase) return;
			_currentPhase = newPhase;
			if (newPhase == BossPhase.Phase2 && !_phase2Entered)
			{
				_phase2Entered = true;
				EventManager.TriggerBossPhaseChanged(2);
				GD.Print($"[Boss] {Stats.EnemyTypeName} Phase 2!");
			}
			else if (newPhase == BossPhase.Phase3 && !_phase3Entered)
			{
				_phase3Entered = true;
				_host.Modulate = new Color(1.4f, 0.3f, 0.3f, 1f);
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
			float speedMul = GetPhaseSpeedMul();
			for (int i = 0; i < _patternCooldowns.Length; i++)
				if (_patternCooldowns[i] > 0f) _patternCooldowns[i] -= delta * speedMul;

			if (_isPerformingPattern)
			{
				_patternTimer -= delta;
				if (_patternTimer <= 0f) AdvancePatternStep();
				return;
			}
			TrySelectPattern();
		}

		private void TrySelectPattern()
		{
			var candidates = new List<int>();
			for (int i = 0; i < Stats.Patterns.Length; i++)
				if (_patternCooldowns[i] <= 0f) candidates.Add(i);
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
			var player = GameManager.Instance?.Player as Node2D;
			var targetPos = player != null ? player.GlobalPosition : _host.GlobalPosition;
			ShowTelegraph(pattern, targetPos, telegraphTime);
			float speedMul = GetPhaseSpeedMul();
			_patternCooldowns[idx] = pattern.Cooldown / speedMul;
		}

		private void ShowTelegraph(BossPatternData pattern, Vector2 targetPos, float duration)
		{
			var parent = _host.GetParent();
			if (parent == null) return;
			var pos = _host.GlobalPosition;
			switch (pattern.PatternType)
			{
				case BossPatternType.ChargeAttack:
				case BossPatternType.BeamSweep:
					_activeTelegraph = Telegraph.CreateLine(parent, pos, pattern.Radius,
						pos.DirectionTo(targetPos).Angle(), duration);
					break;
				case BossPatternType.AoeBurst:
					_activeTelegraph = Telegraph.CreateCircle(parent, pos, pattern.Radius, duration);
					break;
				case BossPatternType.ProjectileVolley:
					_activeTelegraph = Telegraph.CreateCircle(parent, pos, pattern.Radius * 0.7f, duration);
					break;
				case BossPatternType.SummonMinions:
					_activeTelegraph = Telegraph.CreateCircle(parent, pos, 80f, duration);
					break;
				case BossPatternType.Teleport:
					_host.Modulate = _currentPhase == BossPhase.Phase3
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
				_host.Modulate = _currentPhase == BossPhase.Phase3
					? new Color(1.4f, 0.3f, 0.3f, 1f)
					: Colors.White;
			}
		}

		private void ExecutePattern(BossPatternData pattern)
		{
			var player = GameManager.Instance?.Player as Node2D;
			if (player == null) return;
			int dmg = Mathf.RoundToInt(Stats.BaseDamage * pattern.DamageMul);
			switch (pattern.PatternType)
			{
				case BossPatternType.AoeBurst: ExecuteAoeBurst(dmg, pattern.Radius, player); break;
				case BossPatternType.ChargeAttack: ExecuteCharge(dmg, player); break;
				case BossPatternType.BeamSweep: ExecuteBeamSweep(dmg, pattern.Radius, player); break;
				case BossPatternType.ProjectileVolley: ExecuteVolley(dmg, pattern.ProjectileCount); break;
				case BossPatternType.SummonMinions: ExecuteSummon(pattern.SummonCount); break;
				case BossPatternType.Teleport: ExecuteTeleport(player); break;
			}
		}

		private void ExecuteAoeBurst(int dmg, float radius, Node2D player)
		{
			float dist = _host.GlobalPosition.DistanceTo(player.GlobalPosition);
			if (dist <= radius) (player as IDamageable)?.TakeDamage(dmg, ElementType.None);
			UIEffectManager.HitStop(0.1f, 0.08f);
		}

		private void ExecuteCharge(int dmg, Node2D player)
		{
			var dir = _host.GlobalPosition.DirectionTo(player.GlobalPosition);
			var dest = _host.GlobalPosition + dir * 200f;
			var tween = _host.CreateTween();
			tween.TweenProperty(_host, "global_position", dest, 0.35f)
				.SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
			tween.TweenCallback(Callable.From(() =>
			{
				if (IsInstanceValid(_host) && GameManager.Instance?.Player is Node2D p2
					&& _host.GlobalPosition.DistanceTo(p2.GlobalPosition) < 60f)
				{
					(GameManager.Instance?.Player as IDamageable)?.TakeDamage(dmg, ElementType.None);
				}
			}));
		}

		private void ExecuteBeamSweep(int dmg, float length, Node2D player)
		{
			float dist = _host.GlobalPosition.DistanceTo(player.GlobalPosition);
			if (dist <= length) (player as IDamageable)?.TakeDamage(dmg, ElementType.None);
		}

		private void ExecuteVolley(int dmg, int count)
		{
			if (Stats.ProjectileTexture == null) return;
			var parent = _host.GetParent();
			float step = Mathf.Tau / count;
			for (int i = 0; i < count; i++)
			{
				float angle = step * i;
				var proj = new EnemyProjectile
				{
					Direction = Vector2.Right.Rotated(angle),
					Damage = dmg,
					Speed = 250f,
					Texture = Stats.ProjectileTexture,
					TextureScale = Stats.ProjectileScale
				};
				parent.AddChild(proj);
				proj.GlobalPosition = _host.GlobalPosition;
			}
		}

		private void ExecuteSummon(int count)
		{
			// 미니언 소환 — Stats.PossibleDrops에서 minion 인덱스 사용 대신
			// 간단 스폰: 동일 호스트 씬 인스턴스를 같은 위치에 N개 추가.
			// 이때 미니언은 패턴 없는 일반 적으로 동작 (Patterns 필드 안 박힘).
			var enemyScene = GD.Load<PackedScene>("res://Scenes/Characters/enemy.tscn");
			if (enemyScene == null) return;
			var parent = _host.GetParent();
			for (int i = 0; i < count; i++)
			{
				var minion = enemyScene.Instantiate<EnemyController>();
				// 미니언은 보스 스탯의 25%로 약화된 복제본
				var weakStats = (EnemyStats)Stats.Duplicate();
				weakStats.MaxHealth = Mathf.Max(20, Stats.MaxHealth / 8);
				weakStats.BaseDamage = Mathf.Max(2, Stats.BaseDamage / 3);
				weakStats.IsBoss = false;
				weakStats.Patterns = null;
				minion.Stats = weakStats;
				float angle = (float)GD.RandRange(0, Mathf.Tau);
				float dist = (float)GD.RandRange(60, 120);
				minion.GlobalPosition = _host.GlobalPosition + Vector2.Right.Rotated(angle) * dist;
				minion.AddToGroup("Enemy");
				parent.AddChild(minion);
			}
		}

		private void ExecuteTeleport(Node2D player)
		{
			float angle = (float)GD.RandRange(0, Mathf.Tau);
			float dist = (float)GD.RandRange(100, 160);
			_host.GlobalPosition = player.GlobalPosition + Vector2.Right.Rotated(angle) * dist;
			_host.Modulate = _currentPhase == BossPhase.Phase3
				? new Color(1.4f, 0.3f, 0.3f, 1f)
				: Colors.White;
		}
	}
}
