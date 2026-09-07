using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CarTurretGame.Gameplay.UI
{
    public class DistanceProgressUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CarController car;

        [Header("Bar")]
        [SerializeField] private Image barImage;

        [Header("Track markers ")]
        [SerializeField] private RectTransform trackTop;
        [SerializeField] private RectTransform trackBottom;

        [Header("Elements that ride the fill edge")]
        [SerializeField] private RectTransform carIconRect;
        [SerializeField] private TMP_Text distanceText;

        [Header("Offsets")]
        [SerializeField] private float carVerticalOffset = 0f;

        [Header("Formatting")]
        [SerializeField] private string metersSuffix = "m";

        private void Awake()
        {
            if (barImage != null && barImage.type != Image.Type.Filled)
                
            if (carIconRect != null && trackTop != null && trackBottom != null)
            {
                if (carIconRect.parent != trackTop.parent || carIconRect.parent != trackBottom.parent);
                    
            }
        }

        private void Update()
        {
            if (car == null || barImage == null || car.LevelLength <= 0f) return;

            float ratio = Mathf.Clamp01(car.DistanceTraveled / car.LevelLength);

            UpdateBarFill(ratio);
            UpdateRidingElements(ratio);
            UpdateText(ratio);
        }

        private void UpdateBarFill(float ratio)
        {
            barImage.fillAmount = ratio;
        }

        private void UpdateRidingElements(float ratio)
        {
            if (carIconRect == null || trackTop == null || trackBottom == null) return;

            float y = Mathf.Lerp(trackBottom.localPosition.y, trackTop.localPosition.y, ratio);

            Vector3 localPos = carIconRect.localPosition;
            localPos.y = y + carVerticalOffset;
            carIconRect.localPosition = localPos;
        }

        private void UpdateText(float ratio)
        {
            if (distanceText == null) return;

            int meters = Mathf.RoundToInt(ratio * car.LevelLength);
            distanceText.text = $"{meters}{metersSuffix}";
        }
    }
}