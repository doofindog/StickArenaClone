using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private RectTransform crosshairRect;
    
    public void Update()
    {
        Vector2 mousePosition = Input.mousePosition;

        int cameraRefX = CameraController.Instance._pixelPerfectCamera.refResolutionX;
        int cameraRefY = CameraController.Instance._pixelPerfectCamera.refResolutionY;

        int screenResX = Screen.width;
        int screenResY = Screen.height;

        float newPositionX = ConvertRange(screenResX, screenResY, cameraRefX, cameraRefY, mousePosition.x);
        float newPositionY = ConvertRange(screenResX, screenResY, cameraRefX, cameraRefY, mousePosition.y);

        crosshairRect.anchoredPosition = new Vector2(newPositionX, newPositionY);
        
        Debugger.Log("[CROSSHAIR] mousePosition : " + mousePosition);
        Debugger.Log("[CROSSHAIR] CrosshairPosition : " + crosshairRect.anchoredPosition);
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
