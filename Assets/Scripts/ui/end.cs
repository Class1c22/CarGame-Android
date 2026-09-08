using System.Collections;
using UnityEngine;

public class EndGameScreen : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject screenRoot;

    [Header("Delay")]
    [SerializeField] private float showDelay = 3f;

    private Coroutine showRoutine;

    private void Awake()
    {
        if (screenRoot != null)
            screenRoot.SetActive(false);
    }

    public void ShowFail()
    {
        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowAfterDelay());
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(showDelay);

        if (screenRoot != null)
            screenRoot.SetActive(true);
    }

    public void Hide()
    {
        if (showRoutine != null)
            StopCoroutine(showRoutine);

        if (screenRoot != null)
            screenRoot.SetActive(false);
    }
}