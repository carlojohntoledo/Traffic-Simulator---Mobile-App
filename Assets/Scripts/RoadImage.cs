using UnityEngine;
using UnityEngine.EventSystems;

public class RoadImage : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject roadPrefab;

    public void OnPointerDown(PointerEventData eventData)
    {
        PlacementManager.Instance.StartPlacingRoad(roadPrefab);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PlacementManager.Instance.FinishPlacingRoad();
    }
}
