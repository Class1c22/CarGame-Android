using UnityEngine;

namespace CarTurretGame.Gameplay.VFX
{

    public class CarDamageEffects : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CarController car;
        [SerializeField] private ParticleSystem damageEffect;

        [Header("Damage threshold")]
        [Range(0f, 1f)]
        [SerializeField] private float damageStartRatio = 0.5f; // з якого % HP вмикається ефект

        private void Awake()
        {
            if (car == null)
                car = GetComponentInParent<CarController>();
        }

        private void Start()
        {

            damageEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnEnable()
        {
            if (car != null)
            {
                car.HealthChanged += OnHealthChanged;
                car.LevelWon += StopEffect;
                car.LevelLost += StopEffect;
            }
        }

        private void OnDisable()
        {
            if (car != null)
            {
                car.HealthChanged -= OnHealthChanged;
                car.LevelWon -= StopEffect;
                car.LevelLost -= StopEffect;
            }
        }

        private void OnHealthChanged(float current, float max)
        {
            float ratio = max > 0f ? current / max : 0f;
            bool shouldPlay = ratio < damageStartRatio;

            if (shouldPlay && !damageEffect.isPlaying)
                damageEffect.Play();
            else if (!shouldPlay && damageEffect.isPlaying)
                damageEffect.Stop();
        }

        private void StopEffect()
        {
            damageEffect.Stop();
        }
    }
}