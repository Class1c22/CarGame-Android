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

        private float _currentHealth;
        private CarController _car;
        private bool _isChasing;
        private bool _hasHit;

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

        private void Update()
        {
            if (_car == null || _car.State != CarState.Moving || _hasHit) return;

            float dist = Vector3.Distance(transform.position, _car.transform.position);

            if (!_isChasing && dist <= triggerDistance)
            {
                _isChasing = true;
            }

            if (_isChasing)
            {
                var dir = (_car.transform.position - transform.position).normalized;
                dir.y = 0f;
                transform.position += dir * runSpeed * Time.deltaTime;
                transform.LookAt(_car.transform.position);
            }
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

            if (other.GetComponentInParent<CarController>() != null)
            {
                _hasHit = true;
                _car.TakeDamage(damageToCarPerHit);
                Destroy(gameObject);
            }
        }
    }
}