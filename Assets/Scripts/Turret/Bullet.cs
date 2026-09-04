using UnityEngine;
using CarTurretGame.Gameplay.Health;

namespace CarTurretGame.Gameplay.Turret
{

    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 20f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float lifeTime = 3f;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
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