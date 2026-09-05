using UnityEngine;
using CarTurretGame.Gameplay.Health;

namespace CarTurretGame.Gameplay.Turret
{
    [RequireComponent(typeof(TrailRenderer))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 20f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private float hitRadius = 0.15f;
        [SerializeField] private float trailStartDelay = 0.5f;

        private Vector3 _lastPosition;
        private TrailRenderer _trail;
        private float _trailDelayTimer;
        private bool _trailStarted;

        private void Awake()
        {
            _trail = GetComponent<TrailRenderer>();
        }

        private void Start()
        {
            _trail.Clear();
            _trail.emitting = false; // трейл вимкнений на старті

            _trailDelayTimer = trailStartDelay;
            _trailStarted = false;

            _lastPosition = transform.position;
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            _lastPosition = transform.position;
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            if (!_trailStarted)
            {
                _trailDelayTimer -= Time.deltaTime;
                if (_trailDelayTimer <= 0f)
                {
                    _trail.emitting = true;
                    _trailStarted = true;
                }
            }

            CheckHit();
        }

        private void CheckHit()
        {
            Vector3 delta = transform.position - _lastPosition;
            float distance = delta.magnitude;
            if (distance <= 0f) return;

            Vector3 direction = delta / distance;

            if (Physics.SphereCast(_lastPosition, hitRadius, direction, out RaycastHit hit, distance))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var target)
                    || hit.collider.GetComponentInParent<IDamageable>() is { } parentTarget && (target = parentTarget) != null)
                {
                    target.TakeDamage(damage);
                    DestroyBullet();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage);
                DestroyBullet();
            }
        }

        private void DestroyBullet()
        {
            _trail.transform.SetParent(null);
            Destroy(_trail.gameObject, _trail.time);

            Destroy(gameObject);
        }
    }
}