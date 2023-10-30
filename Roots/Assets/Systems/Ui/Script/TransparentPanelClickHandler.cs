using UnityEngine;

public class TransparentPanelClickHandler : MonoBehaviour
{
    public static Vector2 LastClickPosition;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) LastClickPosition = Input.mousePosition;
    }
}