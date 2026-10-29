
using UnityEngine;
using UnityEngine.EventSystems;

public class GridPosition : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Click!" + x + "," + y);
        }
    }
}
