using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using MiniTransportTycoon;

namespace MiniTransportTycoon
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TMP_InputField cityNameInput;

        public void ResumeGame()
        {
            int latestSlot = SaveReader.GetLatestSlot();
            if (latestSlot == -1)
            {
                UnityEngine.Debug.LogWarning("No saves found!");
                return;
            }
            GameSession.SlotToLoad = latestSlot;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void ExitToDesktop()
        {
            UnityEngine.Debug.Log("QuitToDesktop");
            Application.Quit();
        }

        public void StartNewGame(string cityName)
        {
            GameSession.SlotToLoad = -1;
            GameSession.CityName = cityName;
            UnityEngine.Debug.Log("City name set to " + cityName);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void ConfirmNewGame()
        {
            string cityName = cityNameInput.text;

            if (string.IsNullOrEmpty(cityName))
            {
                UnityEngine.Debug.LogWarning("The city name Can't be empty!");
                return;
            }
            StartNewGame(cityName);
        }
    }
}
