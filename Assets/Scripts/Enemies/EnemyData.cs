using UnityEngine;

namespace ArcadeShooter.Enemies
{
    [CreateAssetMenu(menuName = "ArcadeShooter/Enemy Data", fileName = "NewEnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("ID")]
        public string displayName = "Enemy";
        public GameObject enemyPrefab;

        [Header("Stats")]
        public int health = 1;
        public int scoreValue = 100;
        public Color scorePopupColor;
        public float moveSpeed = 3f;
        public int contactDamage = 10;

        [Header("Spawning")] // delay before the next enemy in a group spawns
        public float spawnInterval = 0.25f;

        [Header("AI")] // distance at which a ranged enemy stops chasing and attacks
        public float attackRange = 6f;
        public float attackCooldown = 3f;

        [Header("Bomber")]
        public bool explodesNearPlayer = false; // Captain sparklez, be careful
        public float explodeRange = 2f; // Distance to the player at which the fuse starts
        public float fuseTime = 0.9f;

        [Header("Audio")]
        public AudioClip deathSound;
    }
}
