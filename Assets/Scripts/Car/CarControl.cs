using System;
using UnityEngine;
using VContainer;
using CarTurretGame.Input;
using CarTurretGame.Gameplay.Health;

namespace CarTurretGame.Gameplay
{
    public enum CarState { WaitingForTap, Moving, Finished }

    public class CarController : MonoBehaviour, IDamageable
    {
        [Header("Movement")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private float levelLength = 300f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;

        public CarState State { get; private set; } = CarState.WaitingForTap;
        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;

        public event Action<float, float> HealthChanged;
        public event Action LevelWon;
        public event Action LevelLost;

        private IInputService _inputService;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
            _inputService.Tapped += OnTapped;
        }

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        private void OnDestroy()
        {
            if (_inputService != null)
                _inputService.Tapped -= OnTapped;
        }

        private void Update()
        {
            if (State != CarState.Moving) return;

            transform.Translate(Vector3.forward * (speed * Time.deltaTime), Space.World);

            if (transform.position.z >= levelLength)
                Win();
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