using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameobject;
    [SerializeField] private GameObject circleArrowGameobject;
    [SerializeField] private GameObject crossYouTextGameobject;
    [SerializeField] private GameObject circleYouTextGameobject;

    private void Awake()
    {
        crossArrowGameobject.gameObject.SetActive(false);
        circleArrowGameobject.gameObject.SetActive(false);
        crossYouTextGameobject.gameObject.SetActive(false);
        circleYouTextGameobject.gameObject.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnPlayerTypeChanged += GameManager_OnPlayerTypeChanged;
    }

    private void GameManager_OnPlayerTypeChanged(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.CROSS)
        {
            crossYouTextGameobject.SetActive(true);
        }
        else
        {
            circleYouTextGameobject.SetActive(true);
        }

        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayerType() == GameManager.PlayerType.CROSS)
        {
            crossArrowGameobject.SetActive(true);
            circleArrowGameobject.SetActive(false);
        }
        else
        {
            crossArrowGameobject.SetActive(false);
            circleArrowGameobject.SetActive(true);
        }
    }
}
