using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using CarTurretGame.Gameplay;

namespace CarTurretGame.Gameplay.Enemies
{

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private EnemyControl[] enemyPrefabs;

        [Header("Road")]
        [SerializeField] private float roadHalfWidth = 4f;
        [SerializeField] private float minSpacing = 1.2f;

        [Header("Segments")]
        [SerializeField] private float spawnAheadDistance = 60f;
        [SerializeField] private float segmentLength = 20f;
        [SerializeField] private float levelLength = 300f;
        [SerializeField] private float firstSegmentZ = 25f;

        [Header("Density per segment")]
        [SerializeField] private Vector2Int groupsPerSegment = new Vector2Int(2, 4);
        [SerializeField] private Vector2Int enemiesPerSegment = new Vector2Int(6, 14);
        [SerializeField] private float groupRadius = 2.5f;

        private CarController _car;
        private IObjectResolver _resolver;
        private float _nextSpawnZ;
        private readonly List<Vector3> _occupiedInSegment = new List<Vector3>();

        [Inject]
        public void Construct(CarController car, IObjectResolver resolver)
        {
            _car = car;
            _resolver = resolver;
        }

        private void Start()
        {
            _nextSpawnZ = firstSegmentZ;
        }

        private void Update()
        {
            if (_car == null || _car.State != CarState.Moving) return;

            float carZ = _car.transform.position.z;

            while (_nextSpawnZ < levelLength && carZ + spawnAheadDistance >= _nextSpawnZ)
            {
                SpawnSegment(_nextSpawnZ, _nextSpawnZ + segmentLength);
                _nextSpawnZ += segmentLength;
            }
        }

        private void SpawnSegment(float zStart, float zEnd)
        {
            _occupiedInSegment.Clear();

            int groupCount = Random.Range(groupsPerSegment.x, groupsPerSegment.y + 1);
            int totalEnemies = Random.Range(enemiesPerSegment.x, enemiesPerSegment.y + 1);

            for (int g = 0; g < groupCount; g++)
            {
                Vector3 groupCenter = new Vector3(
                    Random.Range(-roadHalfWidth, roadHalfWidth),
                    0f,
                    Random.Range(zStart, zEnd));

                int remainingGroups = groupCount - g;
                int countThisGroup = Mathf.Max(1, Mathf.RoundToInt((float)totalEnemies / remainingGroups));
                totalEnemies -= countThisGroup;

                for (int i = 0; i < countThisGroup; i++)
                {
                    Vector3 pos = FindFreePosition(groupCenter, zStart, zEnd);
                    SpawnEnemyAt(pos);
                }
            }
        }

        private Vector3 FindFreePosition(Vector3 groupCenter, float zStart, float zEnd)
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * groupRadius;
                Vector3 pos = new Vector3(
                    Mathf.Clamp(groupCenter.x + offset.x, -roadHalfWidth, roadHalfWidth),
                    0f,
                    Mathf.Clamp(groupCenter.z + offset.y, zStart, zEnd));

                if (IsFree(pos))
                {
                    _occupiedInSegment.Add(pos);
                    return pos;
                }
            }

            Vector3 fallback = new Vector3(
                Random.Range(-roadHalfWidth, roadHalfWidth), 0f,
                Random.Range(zStart, zEnd));
            _occupiedInSegment.Add(fallback);
            return fallback;
        }

        private bool IsFree(Vector3 pos)
        {
            float minSqr = minSpacing * minSpacing;
            foreach (var p in _occupiedInSegment)
            {
                if ((p - pos).sqrMagnitude < minSqr)
                    return false;
            }
            return true;
        }

        private void SpawnEnemyAt(Vector3 worldPos)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            _resolver.Instantiate(prefab, worldPos, Quaternion.identity, transform);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(-roadHalfWidth, 0, 0), new Vector3(-roadHalfWidth, 0, levelLength));
            Gizmos.DrawLine(new Vector3(roadHalfWidth, 0, 0), new Vector3(roadHalfWidth, 0, levelLength));
        }
#endif
    }
}