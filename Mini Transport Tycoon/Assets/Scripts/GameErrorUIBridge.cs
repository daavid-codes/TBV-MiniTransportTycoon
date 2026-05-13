using UnityEngine;
using TMPro;

namespace MiniTransportTycoon
{
    // Minimal bridge: show error panel, set TMP text, auto-hide after delay.
    public class GameErrorUIBridge : MonoBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private bool tryFindGameDataWhenMissing = true;
        [SerializeField] private bool replayLastErrorOnEnable = true;
        [SerializeField] private GameObject errorPanelParent;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private float clearAfterSeconds = 2.5f;
        [SerializeField] private bool hidePanelOnAwake = true;
        private CanvasGroup selfCanvasGroup;
        private float hideAtTime = -1f;

        private void Awake()
        {
            EnsureGameDataReference();

            if (errorPanelParent == null)
                errorPanelParent = gameObject;

            if (errorText == null)
                errorText = errorPanelParent.GetComponentInChildren<TMP_Text>(true);

            if (errorPanelParent == gameObject)
            {
                selfCanvasGroup = errorPanelParent.GetComponent<CanvasGroup>();
                if (selfCanvasGroup == null)
                    selfCanvasGroup = errorPanelParent.AddComponent<CanvasGroup>();
            }

            if (hidePanelOnAwake)
                HideErrorNow();
        }

        private void Update()
        {
            if (hideAtTime > 0f && Time.unscaledTime >= hideAtTime)
            {
                if (errorText != null)
                    errorText.text = string.Empty;

                HideErrorNow();

                hideAtTime = -1f;
            }
        }

        private void OnEnable()
        {
            EnsureGameDataReference();

            if (gameData == null)
                return;

            gameData.OnErrorMessage += ForwardErrorMessageToUI;

            if (replayLastErrorOnEnable && !string.IsNullOrWhiteSpace(gameData.LastErrorMessage))
            {
                ForwardErrorMessageToUI(gameData.LastErrorMessage);
            }
        }

        private void OnDisable()
        {
            if (gameData == null)
                return;

            gameData.OnErrorMessage -= ForwardErrorMessageToUI;
        }

        public void SetGameData(GameData newGameData)
        {
            if (gameData == newGameData)
                return;

            if (isActiveAndEnabled && gameData != null)
            {
                gameData.OnErrorMessage -= ForwardErrorMessageToUI;
            }

            gameData = newGameData;

            if (isActiveAndEnabled && gameData != null)
            {
                gameData.OnErrorMessage += ForwardErrorMessageToUI;

                if (replayLastErrorOnEnable && !string.IsNullOrWhiteSpace(gameData.LastErrorMessage))
                {
                    ForwardErrorMessageToUI(gameData.LastErrorMessage);
                }
            }
        }

        public void ForwardErrorMessageToUI(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                return;

            ShowErrorNow();

            if (errorText != null)
                errorText.text = errorMessage;

            hideAtTime = Time.unscaledTime + Mathf.Max(0.5f, clearAfterSeconds);
        }

        private void ShowErrorNow()
        {
            if (errorPanelParent == null)
                return;

            // If the bridge is on the same object, keep object active and fade via CanvasGroup.
            if (errorPanelParent == gameObject)
            {
                if (selfCanvasGroup != null)
                {
                    selfCanvasGroup.alpha = 1f;
                    selfCanvasGroup.interactable = true;
                    selfCanvasGroup.blocksRaycasts = true;
                }

                return;
            }

            errorPanelParent.SetActive(true);
        }

        private void HideErrorNow()
        {
            if (errorPanelParent == null)
                return;

            if (errorPanelParent == gameObject)
            {
                if (selfCanvasGroup != null)
                {
                    selfCanvasGroup.alpha = 0f;
                    selfCanvasGroup.interactable = false;
                    selfCanvasGroup.blocksRaycasts = false;
                }

                return;
            }

            errorPanelParent.SetActive(false);
        }

        private void EnsureGameDataReference()
        {
            if (gameData != null || !tryFindGameDataWhenMissing)
                return;

            gameData = GameData.Instance;

            if (gameData == null)
            {
                gameData = FindObjectOfType<GameData>();
            }
        }
    }
}
