using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    private bool _isGameOn;
    private Vector3 _offset;

    public void SetPosition(Vector3 p_newPos)
    {
        _isGameOn = true;
        transform.position = p_newPos;
    }

    public void EndMinigame()
    {
        _isGameOn = false;
    }
}