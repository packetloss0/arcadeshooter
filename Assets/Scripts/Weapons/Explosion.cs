using UnityEngine;
using ArcadeShooter.Core;
using ArcadeShooter.Enemies;
using ArcadeShooter.Interfaces;
using ArcadeShooter.FX;

namespace ArcadeShooter.Weapons
{
    public class Explosion : MonoBehaviour
    {
        [SerializeField] private float radius = 2.5f;
        [SerializeField] private int damage = 10;
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private AudioClip explosionSound;
        [SerializeField] private float lifetime = 2f;
        [SerializeField] private bool hitStopOnChain; // apply hitstop when bomber explodes another bomber

        private void Start()
        {
            CameraShake.Instance?.Shake(damage / 30f, 0.3f);
            AudioManager.Instance?.PlaySfx(explosionSound);

            int bombersChained = 0;

            var hits = Physics2D.OverlapCircleAll(transform.position, radius, hitLayers);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponentInParent<Enemy>();
                bool bomberAlive = enemy != null && enemy.IsAlive
                                   && enemy.Data != null && enemy.Data.explodesNearPlayer;

                var damageable = hit.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(damage);

                // It died to this blast, so it is about to blow up as well
                if (bomberAlive && !enemy.IsAlive) bombersChained++;
            }

            if (hitStopOnChain && bombersChained > 0) HitStop.Instance?.Freeze();

            Destroy(gameObject, lifetime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
