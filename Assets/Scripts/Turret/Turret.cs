using UnityEngine;
using VContainer;
using CarTurretGame.Input;

namespace CarTurretGame.Gameplay.Turret
{

    public class TurretController : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float sensitivity = 0.2f;
        [SerializeField] private float minAngle = -60f;
        [SerializeField] private float maxAngle = 60f;

        [Header("Shooting")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private float fireRate = 0.3f;

        [Header("Aim Line")]
        [SerializeField] private LineRenderer aimLine;
        [SerializeField] private float aimLineMaxDistance = 50f;
        [SerializeField] private LayerMask aimLineHitMask = ~0;

        private IInputService _inputService;
        private CarController _car;

        private float _currentYaw;
        private float _fireCooldown;

        [Inject]
        public void Construct(IInputService inputService, CarController car)
        {
            _inputService = inputService;
            _car = car;
            _inputService.DragDelta += OnDragDelta;
        }

        private void OnDestroy()
        {
            if (_inputService != null)
                _inputService.DragDelta -= OnDragDelta;
        }

        private void Update()
        {
            UpdateAimLine();

            if (_car.State != CarState.Moving) return;

            HandleShooting();
        }

        private void UpdateAimLine()
        {
            if (aimLine == null || firePoint == null) return;

            Vector3 origin = firePoint.position;
            Vector3 direction = firePoint.forward;
            Vector3 endPoint = origin + direction * aimLineMaxDistance;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, aimLineMaxDistance, aimLineHitMask))
            {
                endPoint = hit.point;
            }

            aimLine.positionCount = 2;
            aimLine.SetPosition(0, origin);
            aimLine.SetPosition(1, endPoint);
        }

        private void OnDragDelta(Vector2 delta)
        {
            if (_car.State != CarState.Moving) return;

            _currentYaw = Mathf.Clamp(_currentYaw + delta.x * sensitivity, minAngle, maxAngle);
            transform.localRotation = Quaternion.Euler(0f, _currentYaw, 0f);
        }

        private void HandleShooting()
        {
            _fireCooldown -= Time.deltaTime;
            if (_fireCooldown > 0f) return;

            Fire();
            _fireCooldown = fireRate;
        }

        private void Fire()
        {
            if (firePoint == null || bulletPrefab == null) return;

            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}