using UnityEngine;
using UnityEngine.UI;

namespace CarTurretGame.Gameplay.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CarController car;

        [Header("Bar images (Image Type = Filled)")]
        [SerializeField] private Image fullBarImage;
        [SerializeField] private Image middleBarImage;
        [SerializeField] private Image lowBarImage;

        [Header("Thresholds")]
        [Range(0f, 1f)] [SerializeField] private float middleThreshold = 0.66f;
        [Range(0f, 1f)] [SerializeField] private float lowThreshold = 0.33f;

        [Header("Smoothing")]
        [SerializeField] private float lerpSpeed = 2f; // одиниць ratio за секунду

        private float targetRatio = 1f;
        private float displayedRatio = 1f;

        private void Awake()
        {
            if (car == null)
                car = FindObjectOfType<CarController>();
        }

        private void Start()
        {
            if (car != null)
            {
                car.HealthChanged += OnHealthChanged;

                targetRatio = car.MaxHealth > 0f ? car.CurrentHealth / car.MaxHealth : 0f;
                displayedRatio = targetRatio;
                UpdateBarVisuals(displayedRatio);
            }
        }

        private void OnDisable()
        {
            if (car != null)
                car.HealthChanged -= OnHealthChanged;
        }

        private void Update()
        {
            if (Mathf.Approximately(displayedRatio, targetRatio))
                return;

            displayedRatio = Mathf.MoveTowards(displayedRatio, targetRatio, lerpSpeed * Time.deltaTime);
            UpdateBarVisuals(displayedRatio);
        }

        private void OnHealthChanged(float current, float max)
        {
            targetRatio = max > 0f ? current / max : 0f;
        }

        private void UpdateBarVisuals(float ratio)
        {
            bool showFull = ratio > middleThreshold;
            bool showMiddle = ratio <= middleThreshold && ratio > lowThreshold;
            bool showLow = ratio <= lowThreshold;

            SetActiveImage(fullBarImage, showFull, ratio);
            SetActiveImage(middleBarImage, showMiddle, ratio);
            SetActiveImage(lowBarImage, showLow, ratio);
        }

        private void SetActiveImage(Image image, bool isActive, float fillAmount)
        {
            if (image == null) return;

            image.gameObject.SetActive(isActive);
            if (isActive)
                image.fillAmount = fillAmount;
        }
    }
}