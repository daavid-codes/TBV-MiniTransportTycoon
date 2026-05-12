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
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private float errorMessageDuration = 2.5f;

    public GameObject escapeMenu;
    private float hideErrorAtTime = -1f;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleEscapeMenu();
            }

            if (errorText != null && hideErrorAtTime >= 0f && Time.unscaledTime >= hideErrorAtTime)
            {
                errorText.text = string.Empty;
                hideErrorAtTime = -1f;
            }
        }

        public void Awake()
        {
            if(gameData == null || gameData.Equals(null))
            {
                gameData = GameData.Instance;
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
            gameData.OnErrorMessage += ShowErrorMessage;
        }
    }

    private void OnDisable()
    {
        if (gameData != null)
        {
            gameData.OnDataChanged -= UpdateUI;
            gameData.OnErrorMessage -= ShowErrorMessage;
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

    private void ShowErrorMessage(string message)
    {
        if (errorText == null)
            return;

        errorText.text = message;
        hideErrorAtTime = Time.unscaledTime + Mathf.Max(0.5f, errorMessageDuration);
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