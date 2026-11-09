using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameobject;
    [SerializeField] private GameObject circleArrowGameobject;
    [SerializeField] private GameObject crossYouTextGameobject;
    [SerializeField] private GameObject circleYouTextGameobject;
    [SerializeField] private TextMeshProUGUI playerCrossScoreTextMesh;
    [SerializeField] private TextMeshProUGUI playerCircleScoreTextMesh;

    private void Awake()
    {
        crossArrowGameobject.gameObject.SetActive(false);
        circleArrowGameobject.gameObject.SetActive(false);
        crossYouTextGameobject.gameObject.SetActive(false);
        circleYouTextGameobject.gameObject.SetActive(false);

        playerCrossScoreTextMesh.text = "";
        playerCircleScoreTextMesh.text = "";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnPlayerTypeChanged += GameManager_OnPlayerTypeChanged;
        GameManager.Instance.OnScoreChange += GameManager_OnScoreChange;
    }

    private void GameManager_OnScoreChange(object sender, EventArgs e)
    {
        GameManager.Instance.GetScores(out int playerCrossScore, out int playerCircleScore);
        playerCrossScoreTextMesh.text = playerCrossScore.ToString();
        playerCircleScoreTextMesh.text = playerCircleScore.ToString();
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

        playerCrossScoreTextMesh.text = "0";
        playerCircleScoreTextMesh.text = "0";

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
