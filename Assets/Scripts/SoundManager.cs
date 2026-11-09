using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSFXPrefab;
    [SerializeField] private Transform winSFXPrefab;
    [SerializeField] private Transform loseSFXPrefab;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject += GameManager_OnPlacedObject;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == e.winPlayerType)
        {
            Transform winSFXTransform = Instantiate(winSFXPrefab);
            Destroy(winSFXTransform.gameObject, 5f);
        }
        else
        {
            Transform loseSFXTransform = Instantiate(loseSFXPrefab);
            Destroy(loseSFXTransform.gameObject, 5f);
        }
    }

    private void GameManager_OnPlacedObject(object sender, EventArgs e)
    {
        Transform placeSFXTransform = Instantiate(placeSFXPrefab);
        Destroy(placeSFXTransform.gameObject, 5f);
    }
}
