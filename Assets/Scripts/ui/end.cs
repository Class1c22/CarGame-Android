using UnityEngine;

public class EndGameScreen : MonoBehaviour
{
    [Header("Canvas")]
    [Tooltip("Канвас/об'єкт екрану (наприклад 'loose').")]
    [SerializeField] private GameObject screenRoot;

    private void Awake()
    {
        if (screenRoot != null)
            screenRoot.SetActive(false);
    }

    public void ShowFail()
    {
        if (screenRoot != null)
            screenRoot.SetActive(true);
    }


    public void Hide()
    {
        if (screenRoot != null)
            screenRoot.SetActive(false);
    }
}