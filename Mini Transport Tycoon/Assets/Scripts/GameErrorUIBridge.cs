using UnityEngine;
using UnityEngine.Events;

namespace MiniTransportTycoon
{
    // UI-only bridge so designers can connect error handling through Inspector events.
    public class GameErrorUIBridge : MonoBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private bool tryFindGameDataWhenMissing = true;
        [SerializeField] private bool replayLastErrorOnEnable = true;
        [SerializeField] private UnityEvent<string> onErrorMessageReceived;

        private void Awake()
        {
            EnsureGameDataReference();
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

            onErrorMessageReceived?.Invoke(errorMessage);
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
