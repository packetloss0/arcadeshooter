namespace ArcadeShooter.Interfaces
{
    // Anything that can take damage.
    public interface IDamageable
    {
        void TakeDamage(int amount);
        bool IsAlive { get; }
    }
}
