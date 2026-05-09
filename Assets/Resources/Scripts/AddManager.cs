using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YG;
using System.Collections;
public class AddManager : MonoBehaviour
{
    public GameObject adOverlay;
    public TextMeshProUGUI adMessageText;
    public TextMeshProUGUI adTimerText;

    public float showEverySeconds = 60f;
    public float adCountdownSeconds = 5f;
    private float countdownRemaining;
    private Coroutine timer;
    private bool isActive = true;


    void  OnEnable()
    {
        if (timer != null)
        {
            StopCoroutine(timer);
        }
        timer = StartCoroutine(ADTimer());
    }

    void ShowAdOverlay()
    {
        Time.timeScale = 0f;
        adOverlay.SetActive(true);
        adMessageText.text = "Скоро появится реклама";
    }

    void HideAdOverlay()
    {
        Time.timeScale = 1f;
        adOverlay.SetActive(false);
    }
    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
    }

    IEnumerator ADTimer()
    {
        while (true)
        {
            while (isActive)
            {
                countdownRemaining = adCountdownSeconds;
                yield return new WaitForSecondsRealtime(showEverySeconds);
                ShowAdOverlay();
                while (countdownRemaining > 0)
                {
                    adTimerText.text = Mathf.CeilToInt(countdownRemaining).ToString();
                    yield return new WaitForSecondsRealtime(1f);
                    countdownRemaining -= 1f;
                }
                YG2.InterstitialAdvShow();
                HideAdOverlay();
            }
        }
    }
}