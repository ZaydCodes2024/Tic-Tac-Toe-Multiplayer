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
        SpawnObjectRpc(e.x, e.y);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y)
    {
        Debug.Log("SpawnObject");
        Transform spawnCircleTransform = Instantiate(circlePrefab, GetGridWorldPosition(x,y), Quaternion.identity);
        spawnCircleTransform.GetComponent<NetworkObject>().Spawn(true);
    }
    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
