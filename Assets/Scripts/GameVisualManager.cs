using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform lineCompletePrefab;
    private const float GRID_SIZE = 3.1f;
    private List<GameObject> visualGameobjectList;

    private void Awake()
    {
        visualGameobjectList = new List<GameObject>();
    }
    private void Start()
    {
        GameManager.Instance.OnClickedWorldPosition += GameManager_OnClickedWorldPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)     return;


        foreach (GameObject visualGameobject in visualGameobjectList)
        {
            Destroy(visualGameobject);
        }

        visualGameobjectList.Clear();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)     return;

        float eulerZ = 0f;
        switch (e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal:
            eulerZ = 0f;
            break;

            case GameManager.Orientation.Vertical:
            eulerZ = 90f;
            break;

            case GameManager.Orientation.DiagonalA:
            eulerZ = 45f;
            break;

            case GameManager.Orientation.DiagonalB:
            eulerZ = -45f;
            break;
        }

        Transform lineCompleteTransform = Instantiate(lineCompletePrefab, GetGridWorldPosition(e.line.centerGridPosition.x, 
        e.line.centerGridPosition.y), Quaternion.Euler(0,0,eulerZ));
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);

        visualGameobjectList.Add(lineCompleteTransform.gameObject);
    }

    private void GameManager_OnClickedWorldPosition(object sender, GameManager.OnClickedWorldPositionEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        
        Transform prefab;

        switch(playerType)
        {
            default:
                case GameManager.PlayerType.CROSS:
                prefab = crossPrefab;
                break;

                case GameManager.PlayerType.CIRCLE:
                prefab = circlePrefab;
                break;
        }

        Transform spawnTransform = Instantiate(prefab, GetGridWorldPosition(x,y), Quaternion.identity);
        spawnTransform.GetComponent<NetworkObject>().Spawn(true);

        visualGameobjectList.Add(spawnTransform.gameObject);
    }
    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
