using UnityEngine;

namespace ArcadeShooter.Weapons
{
    [CreateAssetMenu(menuName = "ArcadeShooter/Weapon Data", fileName = "NewWeapon")]
    public class WeaponData : ScriptableObject
    {
        [Header("ID")]
        public string displayName = "Weapon";
        public Sprite icon;

        [Header("Stats")]
        public float fireRate = 0.5f;       // seconds between shots
        public int damage = 1;
        public int projectilesPerShot = 1;  // >1 = shotgun spread
        public float spreadAngle = 0f;      // cone in degrees
        public float projectileSpeed = 20f;

        [Header("References")]
        public GameObject projectilePrefab;
        public AudioClip fireSound;
        public GameObject muzzleFlashPrefab;
    }
}
