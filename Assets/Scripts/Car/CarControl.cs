using System;
using UnityEngine;
using VContainer;
using CarTurretGame.Input;
using CarTurretGame.Gameplay.Health;

namespace CarTurretGame.Gameplay
{
    public enum CarState { WaitingForTap, Moving, Finished }

    [RequireComponent(typeof(Collider))]
    public class CarController : MonoBehaviour, IDamageable
    {
        [Header("Movement")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private float levelLength = 300f;

        [Header("Side Drift")]
        [SerializeField] private float roadHalfWidth = 2.2f;
        [SerializeField] private float driftChangeIntervalMin = 4f;
        [SerializeField] private float driftChangeIntervalMax = 6f;
        [SerializeField] private float driftSmoothTime = 1.2f;
        [SerializeField] private float maxDriftOffset = 1.5f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;

        public CarState State { get; private set; } = CarState.WaitingForTap;
        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;

        public event Action<float, float> HealthChanged;
        public event Action LevelWon;
        public event Action LevelLost;

        private IInputService _inputService;

        private float _baseX;
        private float _targetOffsetX;
        private float _currentOffsetX;
        private float _driftVelocity;
        private float _driftTimer;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
            _inputService.Tapped += OnTapped;
        }

        private void Awake()
        {
            CurrentHealth = maxHealth;

            var col = GetComponent<Collider>();
            col.isTrigger = false;

            _baseX = transform.position.x;
            PickNewDriftTarget();
        }

        private void OnDestroy()
        {
            if (_inputService != null)
                _inputService.Tapped -= OnTapped;
        }

        private void Update()
        {
            if (State != CarState.Moving) return;

            UpdateDrift();

            Vector3 pos = transform.position;
            pos.z += speed * Time.deltaTime;
            pos.x = _baseX + _currentOffsetX;
            transform.position = pos;

            if (pos.z >= levelLength)
                Win();
        }

        private void UpdateDrift()
        {
            _driftTimer -= Time.deltaTime;
            if (_driftTimer <= 0f)
            {
                PickNewDriftTarget();
            }

            _currentOffsetX = Mathf.SmoothDamp(
                _currentOffsetX,
                _targetOffsetX,
                ref _driftVelocity,
                driftSmoothTime);
        }

        private void PickNewDriftTarget()
        {
            float limit = Mathf.Min(maxDriftOffset, roadHalfWidth);
            _targetOffsetX = UnityEngine.Random.Range(-limit, limit);
            _driftTimer = UnityEngine.Random.Range(driftChangeIntervalMin, driftChangeIntervalMax);
        }

        private void OnTapped()
        {
            switch (State)
            {
                case CarState.WaitingForTap:
                    State = CarState.Moving;
                    break;
                case CarState.Finished:
                    RestartLevel();
                    break;
            }
        }

        public void TakeDamage(float amount)
        {
            if (State != CarState.Moving || amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0f)
                Lose();
        }

        private void Win()
        {
            State = CarState.Finished;
            LevelWon?.Invoke();
        }

        private void Lose()
        {
            State = CarState.Finished;
            LevelLost?.Invoke();
        }

        private void RestartLevel()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}