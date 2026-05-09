using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace MiniTransportTycoon
{
    // UI-only bridge so designers can connect error handling through Inspector events.
    public class GameErrorUIBridge : MonoBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private bool tryFindGameDataWhenMissing = true;
        [SerializeField] private bool replayLastErrorOnEnable = true;
        [SerializeField] private GameObject errorPanelParent;
        [SerializeField] private CanvasGroup errorPanelCanvasGroup;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private float clearAfterSeconds = 2.5f;
        [SerializeField] private bool hidePanelOnAwake = true;
        [SerializeField] private UnityEvent<string> onErrorMessageReceived;
        private float hideAtTime = -1f;

        private void Awake()
        {
            EnsureGameDataReference();

            if (errorPanelParent == null)
                errorPanelParent = gameObject;

            if (errorText == null)
                errorText = GetComponentInChildren<TMP_Text>(true);

            if (errorPanelCanvasGroup == null && errorPanelParent != null)
                errorPanelCanvasGroup = errorPanelParent.GetComponent<CanvasGroup>();

            if (hidePanelOnAwake)
                SetPanelVisible(false);
        }

        private void Update()
        {
            if (hideAtTime > 0f && Time.unscaledTime >= hideAtTime)
            {
                if (errorText != null)
                    errorText.text = string.Empty;

                SetPanelVisible(false);

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

            SetPanelVisible(true);

            if (errorText != null)
                errorText.text = errorMessage;

            hideAtTime = Time.unscaledTime + Mathf.Max(0.5f, clearAfterSeconds);

            onErrorMessageReceived?.Invoke(errorMessage);
        }

        private void SetPanelVisible(bool isVisible)
        {
            if (errorPanelParent == null)
                return;

            // Never deactivate the object that contains this bridge; otherwise it unsubscribes from events.
            if (errorPanelParent == gameObject)
            {
                if (errorPanelCanvasGroup != null)
                {
                    errorPanelCanvasGroup.alpha = isVisible ? 1f : 0f;
                    errorPanelCanvasGroup.interactable = isVisible;
                    errorPanelCanvasGroup.blocksRaycasts = isVisible;
                }

                return;
            }

            errorPanelParent.SetActive(isVisible);
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
