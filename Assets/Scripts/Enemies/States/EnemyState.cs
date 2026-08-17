namespace ArcadeShooter.Enemies.States
{
    public abstract class EnemyState
    {
        protected readonly Enemy Enemy;

        protected EnemyState(Enemy enemy) => Enemy = enemy;

        public virtual void Enter() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void FixedTick(float fixedDeltaTime) { }
        public virtual void Exit() { }
    }

    // Minimal state machine, for future expansion.
    public class EnemyStateMachine
    {
        public EnemyState Current { get; private set; }

        public void SetState(EnemyState next)
        {
            Current?.Exit();
            Current = next;
            Current?.Enter();
        }

        public void Tick(float dt) => Current?.Tick(dt);
        public void FixedTick(float fdt) => Current?.FixedTick(fdt);
    }
}
