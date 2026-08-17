using System.Collections;
using UnityEngine;
using ArcadeShooter.Enemies;

namespace ArcadeShooter.Spawning
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private float spawnRadius = 1f;

        public IEnumerator SpawnGroup(EnemyGroupData group)
        {
            foreach (var entry in group.enemies)
            {
                if (entry.enemy == null || entry.enemy.enemyPrefab == null) continue;

                for (int i = 0; i < entry.count; i++)
                {
                    Vector2 offset = Random.insideUnitCircle * spawnRadius;
                    Vector3 position = transform.position + (Vector3)offset;

                    var go = Instantiate(entry.enemy.enemyPrefab, position, Quaternion.identity);
                    foreach (var enemy in go.GetComponentsInChildren<Enemy>())
                    {
                        enemy.GroupSpeedModifier = group.speedModifier;
                    }

                    yield return new WaitForSeconds(entry.enemy.spawnInterval / group.speedModifier);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            Gizmos.DrawLine(transform.position, transform.position + transform.right * 1.5f);
        }
    }
}
