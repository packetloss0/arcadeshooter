using UnityEngine;
using ArcadeShooter.Core;
using ArcadeShooter.Player;

namespace ArcadeShooter.Enemies.States
{
    public class SpawningState : EnemyState
    {
        private const float Duration = 0.5f;
        private float _timer;

        public SpawningState(Enemy enemy) : base(enemy) { }

        public override void Enter()
        {
            _timer = 0f;
            Enemy.Invulnerable = true;
            Enemy.transform.localScale = Vector3.zero;
        }

        public override void Tick(float dt)
        {
            _timer += dt;
            float t = Mathf.Clamp01(_timer / Duration);
            Enemy.transform.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, t);

            if (t >= 1f)
            {
                Enemy.StateMachine.SetState(new ChaseState(Enemy));
            }
        }

        public override void Exit() => Enemy.Invulnerable = false;
    }

    // Solution to enemies stacking up.
    public class ChaseState : EnemyState
    {
        public ChaseState(Enemy enemy) : base(enemy) { }

        public override void FixedTick(float fdt)
        {
            var player = PlayerController.Local;
            if (player == null) return;

            Vector2 toPlayer = (Vector2)(player.transform.position - Enemy.transform.position);
            float distance = toPlayer.magnitude;

            // Just like Minecraft Creepers bombers light their fuse when close enough
            if (Enemy.Data.explodesNearPlayer && distance <= Enemy.Data.explodeRange)
            {
                Enemy.StateMachine.SetState(new FuseState(Enemy));
                return;
            }

            // Spitters stop and attack at range
            if (Enemy.CanAttackAtRange && distance <= Enemy.Data.attackRange)
            {
                Enemy.StateMachine.SetState(new AttackState(Enemy));
                return;
            }

            Vector2 seek = toPlayer.normalized;
            Vector2 separation = Enemy.ComputeSeparation();
            Vector2 desired = (seek + separation * 0.6f).normalized;

            float speed = Enemy.Data.moveSpeed
                          * Enemy.GroupSpeedModifier
                          * (DifficultyManager.Instance?.EnemySpeedMod ?? 1f);

            Enemy.Body.linearVelocity = desired * speed;
            Enemy.FaceDirection(desired);
        }
    }

    public class AttackState : EnemyState
    {
        private float _cooldownTimer;

        public AttackState(Enemy enemy) : base(enemy) { }

        public override void Enter()
        {
            Enemy.Body.linearVelocity = Vector2.zero;
            _cooldownTimer = Enemy.Data.attackCooldown * 0.5f; // first shot comes sooner
        }

        public override void Tick(float dt)
        {
            var player = PlayerController.Local;
            if (player == null) return;

            Vector2 toPlayer = player.transform.position - Enemy.transform.position;
            Enemy.FaceDirection(toPlayer);

            if (toPlayer.magnitude > Enemy.Data.attackRange * 1.25f)
            {
                Enemy.StateMachine.SetState(new ChaseState(Enemy));
                return;
            }

            _cooldownTimer += dt;
            if (_cooldownTimer >= Enemy.Data.attackCooldown)
            {
                _cooldownTimer = 0f;
                Enemy.PerformRangedAttack(toPlayer.normalized);
            }
        }
    }

    // Bomber approaches player, starts to blink and self detonates. Creeper AHH MEN
    public class FuseState : EnemyState
    {
        private float _timer;
        private Vector3 _baseScale;

        public FuseState(Enemy enemy) : base(enemy) { }

        public override void Enter()
        {
            Enemy.Body.linearVelocity = Vector2.zero;
            _baseScale = Enemy.transform.localScale;
        }

        public override void Tick(float dt)
        {
            _timer += dt;
            float t = Mathf.Clamp01(_timer / Mathf.Max(Enemy.Data.fuseTime, 0.05f));

            Enemy.transform.localScale = _baseScale * (1f + 0.4f * t);
            bool blink = Mathf.PingPong(_timer * (4f + 12f * t), 1f) > 0.5f;
            Enemy.SetBlink(blink);

            if (t >= 1f)
            {
                Enemy.StateMachine.SetState(new DeadState(Enemy, awardScore: false));
            }
        }

        public override void Exit() => Enemy.SetBlink(false);
    }

    public class DeadState : EnemyState
    {
        private readonly bool _awardScore;

        public DeadState(Enemy enemy, bool awardScore = true) : base(enemy)
        {
            _awardScore = awardScore;
        }

        public override void Enter()
        {
            Enemy.Invulnerable = true;
            Enemy.Body.linearVelocity = Vector2.zero;
            Enemy.PlayDeathEffects(showScorePopup: _awardScore);
            if (_awardScore)
            {
                GameEvents.RaiseEnemyKilled(Enemy.transform.position, Enemy.Data.scoreValue,
                                            Enemy.Data.displayName);
            }
            Object.Destroy(Enemy.gameObject, 0.05f);
        }
    }
}
