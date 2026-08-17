using UnityEngine;
using ArcadeShooter.Core;
using ArcadeShooter.FX;

namespace ArcadeShooter.Player
{
    public class ShockwaveAbility : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private ShockwaveFX shockwavePrefab;
        [SerializeField] private int coinsRequired = 25;
        [SerializeField] private AudioClip shockwaveSfx;
        [SerializeField] private AudioClip shockwaveReady;

        public int Charge { get; private set; }
        public bool Ready => Charge >= coinsRequired;

        private void Awake()
        {
            if (input == null) input = GetComponent<PlayerInputHandler>();
        }

        private void OnEnable()
        {
            if (input != null) input.ShockwavePressed += Activate;
            GameEvents.OnGameStarted += ResetCharge;
        }

        private void OnDisable()
        {
            if (input != null) input.ShockwavePressed -= Activate;
            GameEvents.OnGameStarted -= ResetCharge;
        }

        private void Start()
        {
            GameEvents.RaiseShockwaveChargeChanged(Charge, coinsRequired);
        }

        private void ResetCharge()
        {
            Charge = 0;
            GameEvents.RaiseShockwaveChargeChanged(Charge, coinsRequired);
        }

        public void AddCharge(int amount)
        {
            if (Ready) return;

            Charge = Mathf.Min(Charge + amount, coinsRequired);
            GameEvents.RaiseShockwaveChargeChanged(Charge, coinsRequired);

            if (Ready)
            {
                AudioManager.Instance?.PlaySfx(shockwaveReady);
                GameEvents.RaiseAnnouncement("SHOCKWAVE READY");
            }
        }

        private void Activate()
        {
            if (!Ready || shockwavePrefab == null) return;

            Charge = 0;
            GameEvents.RaiseShockwaveChargeChanged(Charge, coinsRequired);
            AudioManager.Instance?.PlaySfx(shockwaveSfx);
            GameEvents.RaiseShockwaveUsed();
            Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        }
    }
}
