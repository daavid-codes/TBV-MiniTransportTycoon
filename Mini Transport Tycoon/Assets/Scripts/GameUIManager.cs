using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace MiniTransportTycoon
{
    [System.Serializable]
    public class MaterialPriceUIEntry
    {
        public Materials material;
        public TextMeshProUGUI priceText;
    }

    public class GameUIManager : MonoBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private GameController gameController;
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private TextMeshProUGUI cityText;
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI errorText;
        [Header("Material Prices UI")]
        [SerializeField] private List<MaterialPriceUIEntry> materialPriceUIEntries;
        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private float errorMessageDuration = 2.5f;

    public GameObject escapeMenu;
    private float hideErrorAtTime = -1f;
    private Dictionary<Materials, TextMeshProUGUI> _materialPriceUITexts;

        private void Awake()
        {
            _materialPriceUITexts = new Dictionary<Materials, TextMeshProUGUI>();
            if (materialPriceUIEntries != null)
            {
                foreach (var entry in materialPriceUIEntries)
                {
                    if (entry.priceText != null)
                    {
                        _materialPriceUITexts[entry.material] = entry.priceText;
                    }
                }
            }

            //if (gameData == null || gameData.Equals(null))
            gameData = GameData.Instance;
        }

        /*
         * ez azért van itt, mert ezzel is működött, talán még jobban; nem akartam az eredetit törölni
         private void Start()
        {
            gameData = GameData.Instance;
            if (gameController == null) gameController = FindObjectOfType<GameController>();

            gameData.OnDataChanged -= UpdateUI;
            gameData.OnDataChanged += UpdateUI;

            gameData.OnErrorMessage -= ShowErrorMessage;
            gameData.OnErrorMessage += ShowErrorMessage;

            // Az UI azonnali frissítése a kezdeti értékekkel
            UpdateUI();
            UpdateMaterialPricesUI();
        }
         */

        
        private void Start()
        {
            if (gameData == null) gameData = GameData.Instance;
            if (gameController == null) gameController = FindObjectOfType<GameController>();

            // Az UI azonnali frissítése a kezdeti értékekkel
            UpdateUI();
            UpdateMaterialPricesUI();
        }
         

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

        public void ToggleEscapeMenu()
        {
            if (escapeMenu != null)
            {
                escapeMenu.SetActive(!escapeMenu.activeSelf);
                if (!gameData.IsGameOver)
                {
                    TogglePause();
                }
            }
        }

    private void OnEnable()
    {
        if (gameData != null)
        {
            gameData.OnDataChanged += UpdateUI;
            gameData.OnErrorMessage += ShowErrorMessage;
            gameData.OnHourChanged += UpdateMaterialPricesUI;
            gameData.OnGameOver += HandleGameOver;
        }
    }

    private void OnDisable()
    {
        if (gameData != null)
        {
            gameData.OnDataChanged -= UpdateUI;
            gameData.OnErrorMessage -= ShowErrorMessage;
            gameData.OnHourChanged -= UpdateMaterialPricesUI;
            gameData.OnGameOver -= HandleGameOver;
        }
    }

    private void HandleGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
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

    private void UpdateMaterialPricesUI()
    {
        if (gameController == null || _materialPriceUITexts == null) return;

        foreach (var kvp in _materialPriceUITexts)
        {
            Materials material = kvp.Key;
            TextMeshProUGUI textElement = kvp.Value;

            if (textElement != null)
            {
                float price = gameController.GetMaterialPrice(material);
                textElement.text = $"{material}: {price:F2}$";
            }
        }
    }

    public void IncreaseTimeMultiplier()
    {
        if (gameData.IsGameOver) return;
        
        if (Time.timeScale < 4f)
        {
            Time.timeScale *= 2f;
        }
    }

    public void DecreaseTimeMultiplier()
    {
        if (gameData.IsGameOver) return;
        
        if (Time.timeScale > 0.25f)
        {
            Time.timeScale /= 2f;
        }
    }

    public void TogglePause()
    {
        if (gameData.IsGameOver) return;
        
        gameData.IsPaused = !gameData.IsPaused;
        Time.timeScale = gameData.IsPaused ? 0f : 1f;
    }

    }
}