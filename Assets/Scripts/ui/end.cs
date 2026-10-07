using System.Collections;
using UnityEngine;
using CarTurretGame.Gameplay;

public class EndGameScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarController car;

    [Header("Canvas")]
    [Tooltip("Екран поразки (Level Failed)")]
    [SerializeField] private GameObject screenRoot;
    [Tooltip("Екран перемоги (коли доїхав до кінця)")]
    [SerializeField] private GameObject winScreenRoot;

    [Header("Delay")]
    [SerializeField] private float showDelay = 3f;
    [SerializeField] private float winShowDelay = 0.5f;

    private Coroutine showRoutine;

    private void Awake()
    {
        if (car == null)
            car = FindAnyObjectByType<CarController>();

        if (screenRoot != null)
            screenRoot.SetActive(false);

        if (winScreenRoot != null)
            winScreenRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (car != null)
            car.LevelWon += ShowWin;
    }

    private void OnDisable()
    {
        if (car != null)
            car.LevelWon -= ShowWin;
    }

    public void ShowFail()
    {
        StartShow(screenRoot, showDelay);
    }

    public void ShowWin()
    {
        StartShow(winScreenRoot, winShowDelay);
    }

    private void StartShow(GameObject root, float delay)
    {
        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowAfterDelay(root, delay));
    }

    private IEnumerator ShowAfterDelay(GameObject root, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (root != null)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (showRoutine != null)
            StopCoroutine(showRoutine);

        if (screenRoot != null)
            screenRoot.SetActive(false);

        if (winScreenRoot != null)
            winScreenRoot.SetActive(false);
    }
}