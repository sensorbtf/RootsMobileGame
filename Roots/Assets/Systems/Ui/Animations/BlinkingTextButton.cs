using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlinkingTextButton : MonoBehaviour
{
    public TextMeshProUGUI textMesh; 
    public Button buttonToWatch; 

    private float _blinkInterval = .4f; 
    private bool isBlinking = false;
    private Coroutine blinkCoroutine;

    private void Update()
    {
        if (isBlinking)
        {
            if (!buttonToWatch.interactable)
            {
                StopBlinking();
            }
        }
        else
        {
            if (buttonToWatch.interactable)
            {
                blinkCoroutine = StartCoroutine(BlinkText());
                isBlinking = true;
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
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            isBlinking = false;
        }
    }
}
