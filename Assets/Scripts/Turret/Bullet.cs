using UnityEngine;
using CarTurretGame.Gameplay.Health;

namespace CarTurretGame.Gameplay.Turret
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 20f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private float hitRadius = 0.15f;

        private Vector3 _lastPosition;

        private void Start()
        {
            _lastPosition = transform.position;
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            _lastPosition = transform.position;
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

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
                    Destroy(gameObject);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}