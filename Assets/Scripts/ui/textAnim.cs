using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LetterPopAnimation : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float delayBetweenLetters = 0.08f;
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnEnable = true;

    [Header("Scale")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float maxScale = 1.4f;

    [SerializeField] private AnimationCurve popCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f)
    );

    private TMP_Text _text;
    private Coroutine _mainRoutine;
    private readonly List<Coroutine> _letterRoutines = new List<Coroutine>();


    private Vector3[][] _baseVertices;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        CacheBaseVertices();
    }

    private void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    private void OnDisable()
    {
        StopAllAnimations();
        RestoreBaseVertices();
    }


    public void CacheBaseVertices()
    {
        _text.ForceMeshUpdate();
        TMP_TextInfo textInfo = _text.textInfo;

        _baseVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            _baseVertices[i] = (Vector3[])textInfo.meshInfo[i].vertices.Clone();
        }
    }

    public void Play()
    {
        StopAllAnimations();
        RestoreBaseVertices();
        _mainRoutine = StartCoroutine(AnimateLetters());
    }

    public void Stop()
    {
        StopAllAnimations();
        RestoreBaseVertices();
    }

    private void StopAllAnimations()
    {
        if (_mainRoutine != null)
        {
            StopCoroutine(_mainRoutine);
            _mainRoutine = null;
        }

        foreach (var routine in _letterRoutines)
        {
            if (routine != null)
                StopCoroutine(routine);
        }
        _letterRoutines.Clear();
    }

    private void RestoreBaseVertices()
    {
        if (_baseVertices == null) return;

        TMP_TextInfo textInfo = _text.textInfo;
        for (int i = 0; i < textInfo.meshInfo.Length && i < _baseVertices.Length; i++)
        {
            _baseVertices[i].CopyTo(textInfo.meshInfo[i].vertices, 0);
        }
        _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    private IEnumerator AnimateLetters()
    {
        TMP_TextInfo textInfo = _text.textInfo;
        int charCount = textInfo.characterCount;

        do
        {
            for (int i = 0; i < charCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                Coroutine c = StartCoroutine(AnimateSingleCharacter(i));
                _letterRoutines.Add(c);
                yield return new WaitForSeconds(delayBetweenLetters);
            }

            yield return new WaitForSeconds(popDuration);
            _letterRoutines.Clear();

        } while (loop);

        _mainRoutine = null;
    }

    private IEnumerator AnimateSingleCharacter(int charIndex)
    {
        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            float curveValue = popCurve.Evaluate(t);
            float scale = Mathf.LerpUnclamped(normalScale, maxScale, curveValue);

            SetCharacterScale(charIndex, scale);
            yield return null;
        }


        SetCharacterScale(charIndex, normalScale);
    }


    private void SetCharacterScale(int charIndex, float scale)
    {
        TMP_TextInfo textInfo = _text.textInfo;

        if (charIndex >= textInfo.characterCount || !textInfo.characterInfo[charIndex].isVisible)
            return;

        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        Vector3[] baseSubmesh = _baseVertices[materialIndex];
        Vector3 charMidBaseline = (baseSubmesh[vertexIndex + 0] + baseSubmesh[vertexIndex + 2]) / 2f;

        Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

        for (int j = 0; j < 4; j++)
        {
            Vector3 originalOffset = baseSubmesh[vertexIndex + j] - charMidBaseline;
            destinationVertices[vertexIndex + j] = charMidBaseline + originalOffset * scale;
        }

        _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}