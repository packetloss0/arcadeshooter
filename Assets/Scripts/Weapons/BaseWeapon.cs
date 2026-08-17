using UnityEngine;
using ArcadeShooter.Core;
using ArcadeShooter.Player;

namespace ArcadeShooter.Weapons
{
    public class BaseWeapon : MonoBehaviour
    {
        [SerializeField] protected WeaponData data;
        [SerializeField] protected Transform muzzle;
        [SerializeField] protected Animator animator;   // optional "Shoot" trigger

        public WeaponData Data => data;
        public bool OwnedByPlayer { get; set; }
        public float LastFireTime { get; set; } = -1000f;

        private static readonly int ShootParam = Animator.StringToHash("Shoot");

        public bool CanFire()
        {
            float rate = data.fireRate;
            if (OwnedByPlayer && PlayerPowerUps.Local != null)
            {
                rate /= Mathf.Max(PlayerPowerUps.Local.FireRateMultiplier, 0.01f);
            }
            return Time.time - LastFireTime >= rate;
        }

        public virtual void Fire(Vector2 direction)
        {
            if (!CanFire()) return;
            LastFireTime = Time.time;

            AudioManager.Instance?.PlaySfx(data.fireSound);
            if (animator != null) animator.SetTrigger(ShootParam);

            if (data.muzzleFlashPrefab != null && muzzle != null)
            {
                ObjectPool.Instance.Spawn(data.muzzleFlashPrefab, muzzle.position, muzzle.rotation);
            }

            SpawnProjectiles(direction);
        }

        protected virtual void SpawnProjectiles(Vector2 direction)
        {
            if (data.projectilePrefab == null) return;

            Vector3 origin = muzzle != null ? muzzle.position : transform.position;
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            for (int i = 0; i < data.projectilesPerShot; i++)
            {
                float t = data.projectilesPerShot > 1
                    ? (float)i / (data.projectilesPerShot - 1) - 0.5f
                    : 0f;
                float angle = baseAngle + t * data.spreadAngle;
                Quaternion rot = Quaternion.Euler(0, 0, angle);

                var go = ObjectPool.Instance.Spawn(data.projectilePrefab, origin, rot);
                if (go.TryGetComponent<Projectile>(out var projectile))
                {
                    int pierce = 0;
                    int bounces = 0;
                    if (OwnedByPlayer && PlayerPowerUps.Local != null)
                    {
                        pierce = PlayerPowerUps.Local.PierceCount;
                        bounces = PlayerPowerUps.Local.BounceCount;
                    }
                    projectile.Initialize(data.damage, data.projectileSpeed, OwnedByPlayer, pierce, bounces);
                }
            }
        }
    }
}
