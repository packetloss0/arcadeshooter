using UnityEngine;
using ArcadeShooter.FX;

namespace ArcadeShooter.Weapons
{
    public class Pistol : BaseWeapon
    {
        [SerializeField] private float shakeIntensity = 0.05f;

        public override void Fire(Vector2 direction)
        {
            if (!CanFire()) return;
            base.Fire(direction);
            CameraShake.Instance?.Shake(shakeIntensity, 0.1f);
        }
    }
}
