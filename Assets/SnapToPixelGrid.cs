using UnityEngine;

public class SnapToPixelGrid : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera ppc;
 
    private void Update()
    {
        Vector3 anchorPosition = GetComponent<RectTransform>().anchoredPosition;
        Vector3 snapPosition = ppc.RoundToPixel(anchorPosition);
        GetComponent<RectTransform>().anchoredPosition = snapPosition;
    }
}
