using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get; private set;}
    public event EventHandler OnGameStarted;
    public event EventHandler OnPlayerTypeChanged;
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
    private NetworkVariable<PlayerType> currentLocalPlayerType = new NetworkVariable<PlayerType>();
    private PlayerType[,] playerTypeArray;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one GameManager instance");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerTypeArray = new PlayerType[3,3];
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        Debug.Log("Clicked on world position " + x + "," + y);

        if (playerType != currentLocalPlayerType.Value)   return;

        if (playerTypeArray[x,y] != PlayerType.NONE)
        {
            // Already Occupied
            return;
        }

        playerTypeArray[x,y] = playerType;

        OnClickedWorldPosition?.Invoke(this, new OnClickedWorldPositionEventArgs {
           x = x,
           y = y,
           playerType = playerType
        });

        switch (currentLocalPlayerType.Value)
        {
            default:
            case PlayerType.CROSS:
                currentLocalPlayerType.Value = PlayerType.CIRCLE;
                break;

            case PlayerType.CIRCLE:
                currentLocalPlayerType.Value = PlayerType.CROSS;
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
            currentLocalPlayerType.Value = PlayerType.CROSS;
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentLocalPlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnPlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentLocalPlayerType.Value = PlayerType.CROSS;
            TriggerOnGameStartedRpc();
        }
    }

    public PlayerType GetCurrentPlayerType()
    {
        return currentLocalPlayerType.Value;
    }
    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }
}
