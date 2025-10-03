using UnityEngine;

public class ClickDebugger : MonoBehaviour
{
    public LayerMask clickableLayer;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayer))
            {
                Debug.Log($"Raycast hit: {hit.collider.name}");
            }
            else
            {
                Debug.Log("Raycast missed all objects!");
            }
        }
    }
}
