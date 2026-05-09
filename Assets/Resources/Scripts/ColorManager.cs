using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ColorManager : MonoBehaviour
{
    public static ColorManager instance;
    public Color[] Colors;
    private TextMeshProUGUI numberText;
    private Image backgroundImage;

    void Awake()
    {
        numberText = GetComponentInChildren<TextMeshProUGUI>();
        backgroundImage = GetComponent<Image>();
        instance = this;
    }

    void Start()
    {
       
    }
    public Color GetColorAtIndex(int number)
    {
        Color rezult;
        int infinityIndex = (int)Mathf.Log(number, 2) - 1;
        int index = infinityIndex % Colors.Length;
        rezult = Colors[index];
        return rezult;
    }
    public void UpdateColor()
    {
        if (numberText == null || backgroundImage == null) return;

        if (int.TryParse(numberText.text, out int value))
        {
            switch (value)
            {
                case 2:
                    backgroundImage.color = new Color32(224, 255, 205, 255); // светло-голубой
                    break;
                case 4:
                    backgroundImage.color = new Color32(253, 255, 205, 255); // голубой (Cornflower Blue)
                    break;
                case 8:
                    backgroundImage.color = new Color32(255, 235, 187, 255); // стальной синий
                    break;
                case 16:
                    backgroundImage.color = new Color32(255, 202, 176, 255); // тёмно-синий (Midnight Blue)
                    break;
                default:
                    backgroundImage.color = new Color32(180, 229, 13, 255); // почти чёрно-синий
                    break;
            }
        }
        else
        {
            backgroundImage.color = Color.white;
        }
    }
}
