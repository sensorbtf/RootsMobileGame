using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlinkingTextButton : MonoBehaviour
{
    private readonly float _blinkInterval = .55f;
    private Coroutine _blinkCoroutine;
    private bool _isBlinking;
    public Button buttonToWatch;
    public TextMeshProUGUI textMesh;

    private void Update()
    {
        if (_isBlinking)
        {
            if (!buttonToWatch.interactable) StopBlinking();
        }
        else
        {
            if (buttonToWatch.interactable)
            {
                _blinkCoroutine = StartCoroutine(BlinkText());
                _isBlinking = true;
            }
        }
    }

    private IEnumerator BlinkText()
    {
        while (true)
        {
            textMesh.enabled = !textMesh.enabled;

            yield return new WaitForSeconds(_blinkInterval);
        }
    }

    private void StopBlinking()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _isBlinking = false;
            textMesh.enabled = true;
        }
    }
}