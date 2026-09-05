using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarTurretGame.Gameplay.Environment
{
    public enum RandomRotationAxis
    {
        None,
        Y,
        Z
    }

    [Serializable]
    public class WeightedDecorationPrefab
    {
        public GameObject prefab;
        [Min(0.01f)] public float weight = 1f;
        public RandomRotationAxis randomRotationAxis = RandomRotationAxis.Y;
        public float heightOffset = 0f;
    }

    public class RoadsideDecorationSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private WeightedDecorationPrefab[] decorationPrefabs;

        [Header("Ground Segments")]
        [SerializeField] private Transform[] groundSegments;
        [SerializeField] private float roadHalfWidth = 3f;
        [SerializeField] private float decorationZoneWidth = 15f;

        [Header("Spawn Settings")]
        [SerializeField] private int spawnCountPerSegment = 10;
        [SerializeField] private float minDistanceBetweenObjects = 1.5f;
        [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.3f);

        [Header("Ground Raycast")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float raycastHeight = 50f;

        [Header("Parent")]
        [SerializeField] private Transform decorationsParent;

        private readonly List<Vector3> _placedPositions = new List<Vector3>();
        private float _totalWeight;

        private void Start()
        {
            SpawnDecorations();
        }

        [ContextMenu("Spawn Decorations")]
        public void SpawnDecorations()
        {
            if (decorationPrefabs == null || decorationPrefabs.Length == 0)
            {
                Debug.LogWarning("RoadsideDecorationSpawner: немає префабів для спавну.");
                return;
            }

            if (groundSegments == null || groundSegments.Length == 0)
            {
                Debug.LogWarning("RoadsideDecorationSpawner: не призначено жодного ground-сегмента.");
                return;
            }

            CalculateTotalWeight();
            ClearDecorations();

            foreach (Transform segment in groundSegments)
            {
                if (segment == null) continue;
                SpawnForSegment(segment);
            }
        }

        private void CalculateTotalWeight()
        {
            _totalWeight = 0f;
            foreach (var entry in decorationPrefabs)
            {
                if (entry.prefab != null)
                    _totalWeight += entry.weight;
            }
        }

        private WeightedDecorationPrefab PickRandomEntry()
        {
            float roll = UnityEngine.Random.Range(0f, _totalWeight);
            float cumulative = 0f;

            foreach (var entry in decorationPrefabs)
            {
                if (entry.prefab == null) continue;

                cumulative += entry.weight;
                if (roll <= cumulative)
                    return entry;
            }

            return decorationPrefabs[decorationPrefabs.Length - 1];
        }

        private void SpawnForSegment(Transform segment)
        {
            Renderer rend = segment.GetComponentInChildren<Renderer>();
            if (rend == null)
            {
                Debug.LogWarning($"RoadsideDecorationSpawner: у сегмента {segment.name} немає Renderer.", segment);
                return;
            }

            Bounds bounds = rend.bounds;

            int spawned = 0;
            int attempts = 0;
            int maxAttempts = spawnCountPerSegment * 20;

            while (spawned < spawnCountPerSegment && attempts < maxAttempts)
            {
                attempts++;

                Vector3 point = GetRandomPointOnSegmentSide(segment, bounds);

                if (!IsFarEnoughFromOthers(point))
                    continue;

                if (!TryProjectToGround(point, out Vector3 groundPoint))
                    continue;

                SpawnAt(groundPoint);
                _placedPositions.Add(groundPoint);
                spawned++;
            }
        }

        private Vector3 GetRandomPointOnSegmentSide(Transform segment, Bounds bounds)
        {
            float zLocal = UnityEngine.Random.Range(-0.5f, 0.5f) * bounds.size.z;

            bool leftSide = UnityEngine.Random.value < 0.5f;
            float sign = leftSide ? -1f : 1f;
            float xLocal = sign * UnityEngine.Random.Range(roadHalfWidth, roadHalfWidth + decorationZoneWidth);

            Vector3 localOffset = new Vector3(xLocal, 0f, zLocal);
            return segment.position + segment.TransformDirection(localOffset);
        }

        private bool IsFarEnoughFromOthers(Vector3 point)
        {
            for (int i = 0; i < _placedPositions.Count; i++)
            {
                if (Vector3.SqrMagnitude(_placedPositions[i] - point) < minDistanceBetweenObjects * minDistanceBetweenObjects)
                    return false;
            }
            return true;
        }

        private bool TryProjectToGround(Vector3 point, out Vector3 result)
        {
            Vector3 rayOrigin = point + Vector3.up * raycastHeight;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                result = hit.point;
                return true;
            }

            result = point;
            return groundLayer.value == 0;
        }

        private void SpawnAt(Vector3 position)
        {
            WeightedDecorationPrefab entry = PickRandomEntry();
            if (entry?.prefab == null) return;

            Quaternion baseRotation = entry.prefab.transform.rotation;
            Quaternion rotation = baseRotation;

            switch (entry.randomRotationAxis)
            {
                case RandomRotationAxis.Y:
                    rotation = baseRotation * Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                    break;
                case RandomRotationAxis.Z:
                    rotation = baseRotation * Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
                    break;
                case RandomRotationAxis.None:
                default:
                    rotation = baseRotation;
                    break;
            }

            Vector3 spawnPosition = position + Vector3.up * entry.heightOffset;

            GameObject instance = Instantiate(entry.prefab, spawnPosition, rotation, decorationsParent != null ? decorationsParent : transform);

            float scale = UnityEngine.Random.Range(scaleRange.x, scaleRange.y);
            instance.transform.localScale *= scale;
        }

        [ContextMenu("Clear Decorations")]
        public void ClearDecorations()
        {
            _placedPositions.Clear();

            Transform parent = decorationsParent != null ? decorationsParent : transform;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}