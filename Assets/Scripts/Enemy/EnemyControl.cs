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

        [Header("Idle Wander")]
        [SerializeField] private float wanderRadius = 1.5f;
        [SerializeField] private float wanderSpeed = 1f;
        [SerializeField] private float wanderPauseMin = 0.5f;
        [SerializeField] private float wanderPauseMax = 2f;

        private float _currentHealth;
        private CarController _car;
        private bool _isChasing;
        private bool _hasHit;

        private Vector3 _spawnPosition;
        private Vector3 _wanderTarget;
        private float _wanderTimer;

        [Inject]
        public void Construct(CarController car)
        {
            _car = car;
        }

        private void Awake()
        {
            _currentHealth = maxHealth;

            var col = GetComponent<Collider>();
            col.isTrigger = true;

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        private void Start()
        {
            _spawnPosition = transform.position;
            PickNewWanderTarget();
        }

        private void Update()
        {
            if (_hasHit) return;

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
            }
            else
            {
                Wander();
            }
        }

        private void Chase()
        {
            var dir = (_car.transform.position - transform.position).normalized;
            dir.y = 0f;
            transform.position += dir * runSpeed * Time.deltaTime;
            transform.LookAt(_car.transform.position);
        }

        private void Wander()
        {
            float dist = Vector3.Distance(transform.position, _wanderTarget);

            if (dist <= 0.1f)
            {
                _wanderTimer -= Time.deltaTime;
                if (_wanderTimer <= 0f)
                {
                    PickNewWanderTarget();
                }
                return;
            }

            var dir = (_wanderTarget - transform.position).normalized;
            dir.y = 0f;
            transform.position += dir * wanderSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(_wanderTarget.x, transform.position.y, _wanderTarget.z));
        }

        private void PickNewWanderTarget()
        {
            Vector2 offset = Random.insideUnitCircle * wanderRadius;
            _wanderTarget = _spawnPosition + new Vector3(offset.x, 0f, offset.y);
            _wanderTimer = Random.Range(wanderPauseMin, wanderPauseMax);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f) return;

            _currentHealth -= amount;
            if (_currentHealth <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (_hasHit) return;

            var car = other.GetComponentInParent<CarController>();
            if (car != null)
            {
                _hasHit = true;
                _car.TakeDamage(damageToCarPerHit);
                Destroy(gameObject);
            }
        }
    }
}