using UnityEngine;
using TMPro;

namespace MiniTransportTycoon
{
    public class UIManager : MonoBehaviour
    {
    [SerializeField] private GameData gameData;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI cityText;
    [SerializeField] private TextMeshProUGUI dateText;

    public GameObject escapeMenu;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleEscapeMenu();
            }
        }

        public void ToggleEscapeMenu()
        {
            if (escapeMenu != null)
            {
                escapeMenu.SetActive(!escapeMenu.activeSelf);
                TogglePause();
                
            }
        }

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

    public void IncreaseTimeMultiplier()
    {
        if (Time.timeScale < 4f)
        {
            Time.timeScale *= 2f;
        }
    }

    public void DecreaseTimeMultiplier()
    {
        if (Time.timeScale > 0.25f)
        {
            Time.timeScale /= 2f;
        }
    }

    public void TogglePause()
    {
        gameData.IsPaused = !gameData.IsPaused;
        Time.timeScale = gameData.IsPaused ? 0f : 1f;
    }

    }
}