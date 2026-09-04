using UnityEngine;
using VContainer;
using CarTurretGame.Gameplay.Health;

namespace CarTurretGame.Gameplay.Enemies
{

    public class EnemyControl : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 30f;

        [Header("Behaviour")]
        [SerializeField] private float runSpeed = 3f;
        [SerializeField] private float triggerDistance = 15f;
        [SerializeField] private float damageToCarPerHit = 10f;
        [SerializeField] private float damageCooldown = 1f;

        private float _currentHealth;
        private CarController _car;
        private bool _isChasing;
        private float _damageTimer;

        [Inject]
        public void Construct(CarController car)
        {
            _car = car;
        }

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        private void Update()
        {
            if (_car == null || _car.State != CarState.Moving) return;

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
            if (_damageTimer > 0f)
            {
                _damageTimer -= Time.deltaTime;
                return;
            }

            if (other.GetComponentInParent<CarController>() != null)
            {
                _car.TakeDamage(damageToCarPerHit);
                _damageTimer = damageCooldown;
            }
        }
    }
}