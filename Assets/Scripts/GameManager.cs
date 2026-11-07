using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get; private set;}
    public event EventHandler OnGameStarted;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;

    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
    }
    public event EventHandler OnPlayerTypeChanged;
    public event EventHandler<OnClickedWorldPositionEventArgs> OnClickedWorldPosition;
    public class OnClickedWorldPositionEventArgs: EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum Orientation
    {
        Horizontal, Vertical, DiagonalA, DiagonalB
    }
    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }
    public enum PlayerType
    {
        NONE, CROSS, CIRCLE
    }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentLocalPlayerType = new NetworkVariable<PlayerType>();
    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;
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

        lineList = new List<Line>
        {
        
          // Horizontal Lines
          new Line
          {
              gridVector2IntList = new List<Vector2Int>
              {
                  new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0)
              },

              centerGridPosition = new Vector2Int(1,0),

              orientation = Orientation.Horizontal
          },

          new Line
          {
              gridVector2IntList = new List<Vector2Int>
              {
                  new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1)
              },

              centerGridPosition = new Vector2Int(1,1),

              orientation = Orientation.Horizontal
          },

          new Line
          {
              gridVector2IntList = new List<Vector2Int>
              {
                  new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2)
              },

              centerGridPosition = new Vector2Int(1,2),

              orientation = Orientation.Horizontal
          },

          // Vertical Lines
          new Line
          {
              gridVector2IntList = new List<Vector2Int>
              {
                  new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2)
              },

              centerGridPosition = new Vector2Int(0,1),

              orientation = Orientation.Vertical
          },

          new Line
          {
              gridVector2IntList = new List<Vector2Int>
              {
                  new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2)
              },

              centerGridPosition = new Vector2Int(1,1),

              orientation = Orientation.Vertical
          }, 

          new Line
          {
              gridVector2IntList = new List<Vector2Int>
              {
                  new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,2)
              },

              centerGridPosition = new Vector2Int(2,1),

              orientation = Orientation.Vertical
          },  

          // Diagonal Lines

          new Line
          {
              gridVector2IntList = new List<Vector2Int>
              {
                  new Vector2Int(0,0), new Vector2Int(1,1), new Vector2Int(2,2)
              },

              centerGridPosition = new Vector2Int(1,1),

              orientation = Orientation.DiagonalA
          }, 

          new Line
          {
              gridVector2IntList = new List<Vector2Int>
              {
                  new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,0)
              },

              centerGridPosition = new Vector2Int(1,1),
              
              orientation = Orientation.DiagonalB
          },        
        };
    }

    private void TestWinner()
    {

        foreach (Line line in lineList)
        {
            if (TestWinnerLine(line))
            {
                Debug.Log("Winner!");
                currentLocalPlayerType.Value = PlayerType.NONE;

                OnGameWin?.Invoke(this, new OnGameWinEventArgs
                {
                    line = line
                });
                break;
            }
        }
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
        );
    }
    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return
            aPlayerType != PlayerType.NONE &&
            aPlayerType == bPlayerType &&
            bPlayerType == cPlayerType;
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

        TestWinner();
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
