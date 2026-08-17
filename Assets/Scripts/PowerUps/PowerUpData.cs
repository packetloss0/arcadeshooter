using UnityEngine;

namespace ArcadeShooter.PowerUps
{
    public enum PowerUpType
    {
        FireRate,
        PiercingAmmo,
        Heal,
        MoveSpeed,
        BouncingBullets
    }

    // This ain't the best way to do it. But who cares.
    // Pickups give temporary boosts, after expiring leave a small permament residue for the rest of the run.
    [CreateAssetMenu(menuName = "ArcadeShooter/Power Up", fileName = "NewPowerUp")]
    public class PowerUpData : ScriptableObject
    {
        [Header("ID")]
        public PowerUpType type;
        public string displayName = "Power Up";
        [TextArea] public string description = ""; // What is announcer gonna say?
        public Color color = Color.white;

        [Header("Temporary boost")]
        public float duration = 30f; // In seconds.
        public float boostMultiplier = 4f; // for now its for firerate multiplier. 

        [Header("Permanent residue")]
        public float permanentAmount = 1.3f; // Multiply into the permament stack.

        [Header("Heal")]
        public int healAmount = 40;
    }
}
