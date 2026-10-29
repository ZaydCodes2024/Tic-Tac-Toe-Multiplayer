
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
            GameManager.Instance.ClickedOnGridPosition(x,y);
        }
    }
}
