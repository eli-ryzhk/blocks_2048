using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using YG;

public class GameOverChecker : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highScoreText;
    public int columns = 8;

    public void CheckGameOver()
    {
        Transform gridParent = GetGridParent();
        if (gridParent == null) return;

        List<Transform> allCells = new List<Transform>();
        foreach (Transform child in gridParent) allCells.Add(child);

        for (int i = 0; i < allCells.Count; i++)
        {
            Block script = allCells[i].GetComponentInChildren<Block>();
            if (script == null) continue;
            int currentNumber = script.GetNumber();

            int[] offsets = { -columns - 1, -columns, -columns + 1, -1, 1, columns - 1, columns, columns + 1 };
            foreach (int offset in offsets)
            {
                int neighborIndex = i + offset;
                if (neighborIndex >= 0 && neighborIndex < allCells.Count &&
                    IsNeighborValid(i, neighborIndex, columns))
                {
                    Block neighborScript = allCells[neighborIndex].GetComponentInChildren<Block>();
                    if (neighborScript != null && neighborScript.GetNumber() == currentNumber)
                        return; 
                }
            }
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            int currentScore = ScoreManager.instance != null ? ScoreManager.instance.GetCurrentScore() : 0;
            int highScore = ScoreManager.instance != null ? ScoreManager.instance.GetHighScore() : 0;

            if (currentScoreText != null)
                currentScoreText.text = "Очки: " + currentScore;

            if (highScoreText != null)
                highScoreText.text = "Рекорд: " + highScore;
        }
    }

    public void RestartGame()
    {
        YG2.InterstitialAdvShow();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private bool IsNeighborValid(int myIndex, int neighborIndex, int columns)
    {
        int myRow = myIndex / columns;
        int neighborRow = neighborIndex / columns;
        int myCol = myIndex % columns;
        int neighborCol = neighborIndex % columns;
        return Mathf.Abs(myRow - neighborRow) <= 1 && Mathf.Abs(myCol - neighborCol) <= 1;
    }

    private Transform GetGridParent()
    {
        Transform t = transform;
        while (t != null)
        {
            if (t.GetComponent<UnityEngine.UI.GridLayoutGroup>() != null)
                return t;
            t = t.parent;
        }
        return null;
    }
}
