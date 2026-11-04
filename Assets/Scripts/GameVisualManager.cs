using System;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform crossPrefab;
    private const float GRID_SIZE = 3.1f;
    private void Start()
    {
        GameManager.Instance.OnClickedWorldPosition += GameManager_OnClickedWorldPosition;
    }

    private void GameManager_OnClickedWorldPosition(object sender, GameManager.OnClickedWorldPositionEventArgs e)
    {
        Debug.Log("GameManager_OnClickedWorldPosition");
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Debug.Log("SpawnObject");
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

        Transform spawnCircleTransform = Instantiate(prefab, GetGridWorldPosition(x,y), Quaternion.identity);
        spawnCircleTransform.GetComponent<NetworkObject>().Spawn(true);
    }
    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
