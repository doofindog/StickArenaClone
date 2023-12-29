using UnityEngine;
using UnityEngine.InputSystem;

public class GameUtilt
{
    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = Camera.main.farClipPlane * 0.5f;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        
        return mouseWorldPos;
    }
}
