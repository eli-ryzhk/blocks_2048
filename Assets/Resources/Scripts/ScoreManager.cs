using UnityEngine;
using TMPro;
using YG;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    public LeaderboardYG leaderboardYG;

    private int score = 0;
    private int highScore = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Очки: " + score;
        if (highScoreText != null)
        {
            highScoreText.text = "Рекорд: " + highScore;
            YG2.SetLeaderboard(leaderboardYG.nameLB, highScore);
        }
    }

    public int GetCurrentScore() => score;
    public int GetHighScore() => highScore;

    public int CurrentScore => score;
    public int HighScore => highScore;
    public static string ShortenNumber(int number)
    {
        if (number < 10_000)
            return number.ToString();

        if (number < 1_000_000)
            return (number / 1_000) + "к"; // 12000 -> 12к

        return (number / 1_000) + "кк";    // 222000 -> 222кк
    }
}
