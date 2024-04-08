using Unity.Mathematics;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private RectTransform crosshairRect;
    [SerializeField] private Canvas _canvas;

    public void Update()
    {
        Vector2 mousePosition = Input.mousePosition;

        float cameraRefX = _canvas.GetComponent<RectTransform>().sizeDelta.x;
        float cameraRefY = _canvas.GetComponent<RectTransform>().sizeDelta.y;

        int screenResX = Screen.width;
        int screenResY = Screen.height;

        float pixelOffset = math.abs(Screen.height - _canvas.worldCamera.pixelHeight) * 0.5f;

        float newPositionX = ConvertRange(0, screenResX, 0, cameraRefX, mousePosition.x );
        float newPositionY = ConvertRange(0 + pixelOffset, screenResY - pixelOffset, 0, cameraRefY, mousePosition.y );

        crosshairRect.anchoredPosition = new Vector2(newPositionX, newPositionY);

        string logs = $"mousePosition : {mousePosition} , CrosshairPosition : {crosshairRect.anchoredPosition} ," +
                      $"screen Res : ({screenResX}, {screenResY}) ," +
                      $"Pixel Offset : ({pixelOffset})" +
                      $"camera Ref Res : ({cameraRefX},{cameraRefY}) ," +
                      $"current screen res : {Screen.currentResolution} ," +
                      $"world Camera : ({_canvas.worldCamera.pixelWidth}, {_canvas.worldCamera.pixelHeight})";
    }
    
    
    private float ConvertRange(float originalStart, float originalEnd, float mapStart, float mapEnd, float value)
    {
        float scale = (mapEnd - mapStart) / (originalEnd - originalStart);
        return mapStart + ((value - originalStart) * scale);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = false;
    }
}
