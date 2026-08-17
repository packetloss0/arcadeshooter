using System.Collections.Generic;

namespace ArcadeShooter.Core
{
    // stats only for the current run
    public static class RunStats
    {
        public static int ScoreFromEnemies;
        public static int ScoreFromCoins;
        public static int EnemiesKilled;
        public static int CoinsCollected;
        public static int PowerUpsCollected;
        public static int ShockwavesUsed;
        public static int HealthHealed;
        public static float LongestNoDamage;

        public static readonly Dictionary<string, int> KillsByType = new();
        public static readonly Dictionary<string, int> KillsByWeapon = new();

        public static void Reset()
        {
            ScoreFromEnemies = 0;
            ScoreFromCoins = 0;
            EnemiesKilled = 0;
            CoinsCollected = 0;
            PowerUpsCollected = 0;
            ShockwavesUsed = 0;
            HealthHealed = 0;
            LongestNoDamage = 0f;

            KillsByType.Clear();
            KillsByWeapon.Clear();
        }

        public static void AddKill(string enemyName, string weaponName)
        {
            EnemiesKilled++;
            Add(KillsByType, enemyName);
            Add(KillsByWeapon, weaponName);
        }

        private static void Add(Dictionary<string, int> counts, string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            counts.TryGetValue(key, out int current);
            counts[key] = current + 1;
        }

        // Weapon with the most kills this run, empty if nothing died (big L)

        // Kills are awarded to the weapon that was held when the enemy died.
        // Soo shockwave etc. will count towards a weapon currently held.
        // Its not enteirly accurate but nobody needs to know that... shush
        public static string BestWeapon(out int kills)
        {
            string best = "";
            kills = 0;

            foreach (var pair in KillsByWeapon)
            {
                if (pair.Value > kills)
                {
                    best = pair.Key;
                    kills = pair.Value;
                }
            }

            return best;
        }
    }
}
