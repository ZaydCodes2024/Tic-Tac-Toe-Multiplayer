using System;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : MonoBehaviour
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
        Transform spawnCircleTransform = Instantiate(circlePrefab);
        spawnCircleTransform.GetComponent<NetworkObject>().Spawn(true);
        spawnCircleTransform.position = GetGridWorldPosition(e.x,e.y);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
