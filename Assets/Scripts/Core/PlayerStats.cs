using UnityEngine;

namespace ArcadeShooter.Core
{
    public static class PlayerStats
    {
        private const string Prefix = "ArcadeShooter.Stats.";
        private const string KillTypesKey = Prefix + "KillTypes";

        private static int Get(string key) => PlayerPrefs.GetInt(Prefix + key, 0);
        private static void Set(string key, int value) => PlayerPrefs.SetInt(Prefix + key, value);

        public static int GamesPlayed { get => Get("GamesPlayed"); set => Set("GamesPlayed", value); }
        public static int EnemiesKilled { get => Get("EnemiesKilled"); set => Set("EnemiesKilled", value); }
        public static int CoinsCollected { get => Get("CoinsCollected"); set => Set("CoinsCollected", value); }
        public static int PowerUpsCollected { get => Get("PowerUpsCollected"); set => Set("PowerUpsCollected", value); }
        public static int ShockwavesUsed { get => Get("ShockwavesUsed"); set => Set("ShockwavesUsed", value); }
        public static int WavesCleared { get => Get("WavesCleared"); set => Set("WavesCleared", value); }
        public static int BestWave { get => Get("BestWave"); set => Set("BestWave", value); }
        public static int SecondsPlayed { get => Get("SecondsPlayed"); set => Set("SecondsPlayed", value); }

        public static int KillsOf(string enemyName) => Get("Kills." + enemyName);

        public static void AddKill(string enemyName)
        {
            EnemiesKilled++;

            if (string.IsNullOrEmpty(enemyName)) return;
            Set("Kills." + enemyName, KillsOf(enemyName) + 1);
            RegisterKillType(enemyName);
        }

        // Enemy names we have killed at least once, saved as "Chaser,Spitter,Bomber"
        public static string[] KnownKillTypes()
        {
            string raw = PlayerPrefs.GetString(KillTypesKey, "");
            return raw.Length == 0 ? new string[0] : raw.Split(',');
        }

        private static void RegisterKillType(string enemyName)
        {
            foreach (string type in KnownKillTypes())
            {
                if (type == enemyName) return;
            }

            string raw = PlayerPrefs.GetString(KillTypesKey, "");
            PlayerPrefs.SetString(KillTypesKey, raw.Length == 0 ? enemyName : raw + "," + enemyName);
        }

        public static void Save() => PlayerPrefs.Save();

        public static void ResetAll()
        {
            foreach (string type in KnownKillTypes())
            {
                PlayerPrefs.DeleteKey(Prefix + "Kills." + type);
            }
            PlayerPrefs.DeleteKey(KillTypesKey);

            foreach (var key in new[] { "GamesPlayed", "EnemiesKilled", "CoinsCollected",
                                        "PowerUpsCollected", "ShockwavesUsed", "WavesCleared",
                                        "BestWave", "SecondsPlayed" })
            {
                PlayerPrefs.DeleteKey(Prefix + key);
            }
            Save();
        }
    }
}
