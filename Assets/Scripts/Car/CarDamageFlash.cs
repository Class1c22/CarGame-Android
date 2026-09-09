using System.Collections;
using UnityEngine;
using CarTurretGame.Gameplay;

namespace CarTurretGame.Gameplay.VFX
{

    public class CarDamageFlash : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CarController car;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material flashMaterial;

        [Header("Timing")]
        [SerializeField] private float flashDuration = 0.1f;

        private Material[][] _originalMaterials;
        private Coroutine _flashRoutine;
        private float _lastHealth = -1f;
        private bool _initialized;

        private void Awake()
        {
            if (car == null)
                car = GetComponentInParent<CarController>();

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>();

            CacheOriginalMaterials();
        }

        private void CacheOriginalMaterials()
        {
            _originalMaterials = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    _originalMaterials[i] = renderers[i].sharedMaterials;
            }
            _initialized = true;
        }

        private void OnEnable()
        {
            if (car != null)
            {
                car.HealthChanged += OnHealthChanged;
                _lastHealth = car.CurrentHealth;
            }
        }

        private void OnDisable()
        {
            if (car != null)
                car.HealthChanged -= OnHealthChanged;

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }
            RestoreMaterials();
        }

        private void OnHealthChanged(float current, float max)
        {
            if (_lastHealth < 0f)
            {
                _lastHealth = current;
                return;
            }

            if (current < _lastHealth)
                Flash();

            _lastHealth = current;
        }

        public void Flash()
        {
            if (!_initialized || flashMaterial == null) return;

            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);

            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            SetWhiteMaterial();
            yield return new WaitForSeconds(flashDuration);
            RestoreMaterials();
            _flashRoutine = null;
        }

        private void SetWhiteMaterial()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;

                int slotCount = renderers[i].sharedMaterials.Length;
                Material[] whiteMats = new Material[slotCount];
                for (int m = 0; m < slotCount; m++)
                    whiteMats[m] = flashMaterial;


                renderers[i].sharedMaterials = whiteMats;
            }
        }

        private void RestoreMaterials()
        {
            if (_originalMaterials == null) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null || _originalMaterials[i] == null) continue;
                renderers[i].sharedMaterials = _originalMaterials[i];
            }
        }
    }
}