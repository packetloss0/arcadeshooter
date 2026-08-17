using System.Collections.Generic;
using UnityEngine;
using ArcadeShooter.Interfaces;

namespace ArcadeShooter.Core
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!_pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefab] = queue;
            }

            GameObject instance = queue.Count > 0 ? queue.Dequeue() : null;

            if (instance == null)
            {
                instance = Instantiate(prefab, position, rotation);
                _instanceToPrefab[instance] = prefab;
            }
            else
            {
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.SetActive(true);
            }

            if (instance.TryGetComponent<IPoolable>(out var poolable)) poolable.OnSpawnedFromPool();

            return instance;
        }

        public void Return(GameObject instance)
        {
            if (!_instanceToPrefab.TryGetValue(instance, out var prefab))
            {
                // Not spawned from this pool just destroy it.
                Destroy(instance);
                return;
            }

            if (instance.TryGetComponent<IPoolable>(out var poolable)) poolable.OnReturnedToPool();

            instance.SetActive(false);
            _pools[prefab].Enqueue(instance);
        }
    }
}
