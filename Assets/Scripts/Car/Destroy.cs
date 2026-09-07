using System.Collections;
using UnityEngine;

namespace CarTurretGame.Gameplay.VFX
{

    public class CarWreckSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CarController car;

        [Header("Wreck Prefabs (3 шматки)")]
        [SerializeField] private GameObject[] wreckPrefabs;

        [Header("Explosion")]
        [SerializeField] private ParticleSystem explosionPrefab;
        [SerializeField] private float explosionLifeTime = 3f;

        [Header("What to hide on the car")]
        [Tooltip("Якщо не заповнено — автоматично візьмуться всі прямі дочірні об'єкти машини")]
        [SerializeField] private GameObject[] carChildrenToHide;

        [Header("Spawn")]
        [SerializeField] private float spawnRadius = 0.5f;

        [Header("Explosion Force")]
        [SerializeField] private float minForce = 3f;
        [SerializeField] private float maxForce = 7f;
        [SerializeField] private float upwardForce = 3f;
        [SerializeField] private float torqueStrength = 5f;

        [Header("Freeze")]
        [SerializeField] private float freezeAfterSeconds = 2f;

        private bool _hasSpawned;

        private void Awake()
        {
            if (car == null)
                car = GetComponentInParent<CarController>();

            if (carChildrenToHide == null || carChildrenToHide.Length == 0)
            {
                Transform root = car != null ? car.transform : transform;
                carChildrenToHide = new GameObject[root.childCount];
                for (int i = 0; i < root.childCount; i++)
                    carChildrenToHide[i] = root.GetChild(i).gameObject;
            }
        }

        private void OnEnable()
        {
            if (car != null)
                car.LevelLost += OnCarBroken;
        }

        private void OnDisable()
        {
            if (car != null)
                car.LevelLost -= OnCarBroken;
        }

        private void OnCarBroken()
        {
            if (_hasSpawned) return;
            _hasSpawned = true;

            SpawnExplosion();
            SpawnWreckPieces();
            HideCar();
        }

        private void SpawnExplosion()
        {
            if (explosionPrefab == null) return;

            Vector3 origin = car != null ? car.transform.position : transform.position;
            ParticleSystem instance = Instantiate(explosionPrefab, origin, Quaternion.identity);
            instance.gameObject.SetActive(true);
            instance.Play();

            Destroy(instance.gameObject, explosionLifeTime);
        }

        private void SpawnWreckPieces()
        {
            if (wreckPrefabs == null || wreckPrefabs.Length == 0) return;

            Vector3 origin = car != null ? car.transform.position : transform.position;

            foreach (var prefab in wreckPrefabs)
            {
                if (prefab == null) continue;
                SpawnPiece(prefab, origin);
            }
        }

        private void SpawnPiece(GameObject prefab, Vector3 origin)
        {
            Vector2 offset2D = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = origin + new Vector3(offset2D.x, 0.3f, offset2D.y);

            GameObject instance = Instantiate(prefab, spawnPos, Random.rotation);

            Rigidbody rb = instance.GetComponent<Rigidbody>();
            if (rb == null)
                rb = instance.AddComponent<Rigidbody>();

            if (instance.GetComponent<Collider>() == null)
                instance.AddComponent<BoxCollider>();

            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y);
            Vector3 force = randomDir * Random.Range(minForce, maxForce) + Vector3.up * upwardForce;

            rb.AddForce(force, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * torqueStrength, ForceMode.Impulse);

            if (freezeAfterSeconds > 0f)
                StartCoroutine(FreezeAfterDelay(rb, freezeAfterSeconds));
        }

        private IEnumerator FreezeAfterDelay(Rigidbody rb, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (rb != null)
                rb.isKinematic = true;
        }

        private void HideCar()
        {
            foreach (var child in carChildrenToHide)
            {
                if (child != null)
                    child.SetActive(false);
            }


            var rootRenderer = GetComponent<Renderer>();
            if (rootRenderer != null) rootRenderer.enabled = false;

            var rootCollider = GetComponent<Collider>();
            if (rootCollider != null) rootCollider.enabled = false;
        }
    }
}