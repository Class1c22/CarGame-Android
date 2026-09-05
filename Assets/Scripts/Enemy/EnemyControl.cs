using System.Collections;
using UnityEngine;
using VContainer;
using CarTurretGame.Gameplay.Health;

namespace CarTurretGame.Gameplay.Enemies
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyControl : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 30f;

        [Header("Behaviour")]
        [SerializeField] private float runSpeed = 3f;
        [SerializeField] private float triggerDistance = 15f;
        [SerializeField] private float damageToCarPerHit = 10f;

        [Header("Death")]
        [SerializeField] private BloodChunkSpawner bloodChunkSpawner;

        [Header("Idle Wander")]
        [SerializeField] private float wanderRadius = 1.5f;
        [SerializeField] private float wanderSpeed = 1f;
        [SerializeField] private float wanderPauseMin = 0.5f;
        [SerializeField] private float wanderPauseMax = 2f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private float hitStunDuration = 1f;
        [SerializeField] private float speedDampTime = 0.1f;

        [Header("Hit Flash")]
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material hitFlashMaterial;
        [SerializeField] private float hitFlashDuration = 0.15f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int HitIndexHash = Animator.StringToHash("HitIndex");

        private float _currentHealth;
        private CarController _car;
        private bool _isChasing;
        private bool _hasHit;
        private float _hitStunTimer;

        private Vector3 _spawnPosition;
        private Vector3 _wanderTarget;
        private float _wanderTimer;

        private Material[][] _originalMaterials;
        private Coroutine _flashRoutine;

        [Inject]
        public void Construct(CarController car)
        {
            _car = car;
        }

        private void Awake()
        {
            _currentHealth = maxHealth;

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>();

            CacheOriginalMaterials();

            var col = GetComponent<Collider>();
            col.isTrigger = true;

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        private void CacheOriginalMaterials()
        {
            _originalMaterials = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                _originalMaterials[i] = renderers[i].materials;
            }
        }

        private void Start()
        {
            _spawnPosition = transform.position;
            PickNewWanderTarget();
        }

        private void Update()
        {
            if (_hasHit) return;

            if (_hitStunTimer > 0f)
            {
                _hitStunTimer -= Time.deltaTime;
                return;
            }

            if (_car != null && _car.State == CarState.Moving)
            {
                float dist = Vector3.Distance(transform.position, _car.transform.position);

                if (!_isChasing && dist <= triggerDistance)
                {
                    _isChasing = true;
                }
            }

            if (_isChasing)
            {
                Chase();
                animator.SetFloat(SpeedHash, 1f, speedDampTime, Time.deltaTime);
            }
            else
            {
                bool isMoving = Wander();
                animator.SetFloat(SpeedHash, isMoving ? 0.5f : 0f, speedDampTime, Time.deltaTime);
            }
        }

        private void Chase()
        {
            var dir = (_car.transform.position - transform.position).normalized;
            dir.y = 0f;
            transform.position += dir * runSpeed * Time.deltaTime;
            transform.LookAt(_car.transform.position);
        }

        private bool Wander()
        {
            float dist = Vector3.Distance(transform.position, _wanderTarget);

            if (dist <= 0.1f)
            {
                _wanderTimer -= Time.deltaTime;
                if (_wanderTimer <= 0f)
                {
                    PickNewWanderTarget();
                }
                return false;
            }

            var dir = (_wanderTarget - transform.position).normalized;
            dir.y = 0f;
            transform.position += dir * wanderSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(_wanderTarget.x, transform.position.y, _wanderTarget.z));
            return true;
        }

        private void PickNewWanderTarget()
        {
            Vector2 offset = Random.insideUnitCircle * wanderRadius;
            _wanderTarget = _spawnPosition + new Vector3(offset.x, 0f, offset.y);
            _wanderTimer = Random.Range(wanderPauseMin, wanderPauseMax);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || _hasHit) return;

            _currentHealth -= amount;

            if (_currentHealth <= 0f)
            {
                Die(transform.position - transform.forward);
                return;
            }

            PlayHitReaction();
        }

        private void PlayHitReaction()
        {
            if (animator != null)
            {
                int hitIndex = Random.Range(0, 3);
                animator.SetInteger(HitIndexHash, hitIndex);
                animator.SetTrigger(HitHash);
            }

            _hitStunTimer = hitStunDuration;

            if (hitFlashMaterial != null)
            {
                if (_flashRoutine != null)
                    StopCoroutine(_flashRoutine);

                _flashRoutine = StartCoroutine(HitFlashRoutine());
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            SetFlashMaterial(true);
            yield return new WaitForSeconds(hitFlashDuration);
            SetFlashMaterial(false);
            _flashRoutine = null;
        }

        private void SetFlashMaterial(bool flashOn)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;

                if (flashOn)
                {
                    var flashMats = new Material[_originalMaterials[i].Length];
                    for (int m = 0; m < flashMats.Length; m++)
                        flashMats[m] = hitFlashMaterial;

                    renderers[i].materials = flashMats;
                }
                else
                {
                    renderers[i].materials = _originalMaterials[i];
                }
            }
        }

        private void Die(Vector3 explosionSource)
        {
            if (_hasHit) return;
            _hasHit = true;

            if (bloodChunkSpawner != null)
            {
                bloodChunkSpawner.SpawnAt(transform.position);
            }

            Destroy(gameObject);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_hasHit) return;

            var car = other.GetComponentInParent<CarController>();
            if (car != null)
            {
                _car.TakeDamage(damageToCarPerHit);
                Die(car.transform.position);
            }
        }
    }
}