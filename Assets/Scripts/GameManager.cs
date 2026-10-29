using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public event EventHandler<OnClickedWorldPositionEventArgs> OnClickedWorldPosition;
    public class OnClickedWorldPositionEventArgs: EventArgs
    {
        public int x;
        public int y;
    }
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one GameManager instance");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void ClickedOnGridPosition(int x, int y)
    {
        Debug.Log("Clicked on world position " + x + "," + y);
        OnClickedWorldPosition?.Invoke(this, new OnClickedWorldPositionEventArgs {
           x = x,
           y = y
        });
    }
}
