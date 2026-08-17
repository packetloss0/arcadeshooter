using UnityEngine;
using ArcadeShooter.Weapons;

namespace ArcadeShooter.Player
{
    public class WeaponHolder : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private PlayerController player;
        [SerializeField] private Transform weaponSocket;   // child of the aim pivot
        [SerializeField] private BaseWeapon[] weaponPrefabs;

        public BaseWeapon CurrentWeapon { get; private set; }
        public int CurrentIndex { get; private set; } = -1;

        private void OnEnable()
        {
            input.NextWeaponPressed += EquipNext;
            input.PreviousWeaponPressed += EquipPrevious;
        }

        private void OnDisable()
        {
            input.NextWeaponPressed -= EquipNext;
            input.PreviousWeaponPressed -= EquipPrevious;
        }

        private void Start()
        {
            Equip(0);
        }

        private void Update()
        {
            if (player.MovementLocked) return;

            if (input.FireHeld && CurrentWeapon != null && CurrentWeapon.CanFire())
            {
                CurrentWeapon.Fire(player.AimDirection);
            }
        }

        public void Equip(int index)
        {
            if (weaponPrefabs.Length == 0) return;
            index = (index % weaponPrefabs.Length + weaponPrefabs.Length) % weaponPrefabs.Length;
            if (index == CurrentIndex) return;

            if (CurrentWeapon != null) Destroy(CurrentWeapon.gameObject);

            CurrentIndex = index;
            CurrentWeapon = Instantiate(weaponPrefabs[index], weaponSocket);
            CurrentWeapon.transform.localPosition = Vector3.zero;
            CurrentWeapon.transform.localRotation = Quaternion.identity;
            CurrentWeapon.OwnedByPlayer = true;
            //CurrentWeapon.LastFireTime = Time.time; // it prevented quick swap spam, but sometimes made weapons feel unresponsive. 
        }

        private void EquipNext() => Equip(CurrentIndex + 1);
        private void EquipPrevious() => Equip(CurrentIndex - 1);
    }
}
