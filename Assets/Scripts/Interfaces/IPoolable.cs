namespace ArcadeShooter.Interfaces
{
    // Recycle! 
    public interface IPoolable
    {
        void OnSpawnedFromPool();
        void OnReturnedToPool();
    }
}
