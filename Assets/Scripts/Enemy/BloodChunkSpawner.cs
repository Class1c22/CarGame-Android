using UnityEngine;

namespace CarTurretGame.Gameplay.Enemies
{
    public class BloodChunkSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject[] chunkPrefabs;

        [Header("Spawn")]
        [SerializeField] private int chunkCount = 7;
        [SerializeField] private float spawnRadius = 0.3f;

        [Header("Explosion Force")]
        [SerializeField] private float minForce = 3f;
        [SerializeField] private float maxForce = 7f;
        [SerializeField] private float upwardForce = 2f;
        [SerializeField] private float torqueStrength = 5f;

        [Header("Scale")]
        [SerializeField] private Vector2 scaleRange = new Vector2(0.5f, 1.2f);

        [Header("Lifetime")]
        [SerializeField] private float lifeTime = 6f;
        [SerializeField] private float freezeAfterSeconds = 2f;

        public void SpawnAt(Vector3 origin)
        {
            if (chunkPrefabs == null || chunkPrefabs.Length == 0) return;

            for (int i = 0; i < chunkCount; i++)
            {
                SpawnChunk(origin);
            }
        }

        private void SpawnChunk(Vector3 origin)
        {
            GameObject prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];

            Vector2 offset2D = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = origin + new Vector3(offset2D.x, 0.3f, offset2D.y);

            GameObject instance = Instantiate(prefab, spawnPos, Random.rotation);

            float scale = Random.Range(scaleRange.x, scaleRange.y);
            instance.transform.localScale *= scale;

            Rigidbody rb = instance.GetComponent<Rigidbody>();
            if (rb == null)
                rb = instance.AddComponent<Rigidbody>();

            if (instance.GetComponent<Collider>() == null)
                instance.AddComponent<SphereCollider>();

            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y); 
            Vector3 force = randomDir * Random.Range(minForce, maxForce) + Vector3.up * upwardForce;

            rb.AddForce(force, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * torqueStrength, ForceMode.Impulse);

            if (freezeAfterSeconds > 0f)
                StartCoroutine(FreezeAfterDelay(rb, freezeAfterSeconds));

            if (lifeTime > 0f)
                Destroy(instance, lifeTime);
        }

        private System.Collections.IEnumerator FreezeAfterDelay(Rigidbody rb, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (rb != null)
            {
                rb.isKinematic = true; 
            }
        }
    }
}