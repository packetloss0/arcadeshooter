using UnityEngine;
using ArcadeShooter.Interfaces;

namespace ArcadeShooter.Player
{
    // Weapon pickups!
    // well its kinda obsolete now, we start with all guns. Something to implement later!
    [RequireComponent(typeof(Collider2D))]
    public class WeaponPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private int weaponIndex;
        [SerializeField] private float respawnTime = 10f;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Collider2D pickupCollider;

        private void Reset()
        {
            pickupCollider = GetComponent<Collider2D>();
        }

        private void Awake()
        {
            pickupCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (Interact())
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        public bool Interact()
        {
            var holder = PlayerController.Local?.GetComponentInChildren<WeaponHolder>()
                         ?? PlayerController.Local?.GetComponent<WeaponHolder>();
            if (holder == null) return false;

            holder.Equip(weaponIndex);
            return true;
        }

        private System.Collections.IEnumerator RespawnRoutine()
        {
            sprite.enabled = false;
            pickupCollider.enabled = false;
            yield return new WaitForSeconds(respawnTime);
            sprite.enabled = true;
            pickupCollider.enabled = true;
        }
    }
}
