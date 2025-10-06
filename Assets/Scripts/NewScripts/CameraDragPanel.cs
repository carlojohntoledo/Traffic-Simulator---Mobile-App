using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDragPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static bool IsPointerOverCameraPanel = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPointerOverCameraPanel = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPointerOverCameraPanel = false;
    }
}
