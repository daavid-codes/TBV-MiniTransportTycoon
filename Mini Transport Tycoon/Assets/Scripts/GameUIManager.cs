using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameData gameData;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI cityText;
    [SerializeField] private TextMeshProUGUI dateText;

    private void OnEnable()
    {
        if (gameData != null)
        {
            gameData.OnDataChanged += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (gameData != null)
        {
            gameData.OnDataChanged -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = gameData.Money.ToString() + "$";

        if (cityText != null)
            cityText.text = gameData.CityName;

        if (dateText != null)
            dateText.text = gameData.CurrentDate.ToString("yyyy. MM. dd. HH:mm");
    }
}