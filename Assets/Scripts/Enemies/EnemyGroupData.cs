using System.Collections.Generic;
using UnityEngine;

namespace ArcadeShooter.Enemies
{
    // Group of enemies that spawn together. controlled by the difficulty tier
    [CreateAssetMenu(menuName = "ArcadeShooter/Enemy Group", fileName = "NewEnemyGroup")]
    public class EnemyGroupData : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public EnemyData enemy;
            public int count;
        }

        public int difficultyTier = 0; // Group will only spawn once the Tier is >= this value
        public float speedModifier = 1f;
        public List<Entry> enemies = new();
    }
}
