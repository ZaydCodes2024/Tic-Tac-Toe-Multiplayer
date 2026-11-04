using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get; private set;}
    public event EventHandler<OnClickedWorldPositionEventArgs> OnClickedWorldPosition;
    public class OnClickedWorldPositionEventArgs: EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum PlayerType
    {
        NONE, CROSS, CIRCLE
    }

    private PlayerType localPlayerType;
    private PlayerType currentLocalPlayerType;
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
    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        Debug.Log("Clicked on world position " + x + "," + y);

        if (playerType != currentLocalPlayerType)   return;

        OnClickedWorldPosition?.Invoke(this, new OnClickedWorldPositionEventArgs {
           x = x,
           y = y,
           playerType = playerType
        });

        switch (currentLocalPlayerType)
        {
            default:
            case PlayerType.CROSS:
                currentLocalPlayerType = PlayerType.CIRCLE;
                break;

            case PlayerType.CIRCLE:
                currentLocalPlayerType = PlayerType.CROSS;
                break;
        }
    }

    public override void OnNetworkSpawn()
    {
        if(NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.CROSS;
        }
        else
        {
            localPlayerType = PlayerType.CIRCLE;
        }

        if(IsServer)
        {
            currentLocalPlayerType = PlayerType.CROSS;
        }
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }
}
