using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniTransportTycoon
{
    public class MenuController : MonoBehaviour
    {
        public GameObject vehicleSubMenu;
        public GameObject GarageMenu;
        

        public void ToggleVehicleMenu()
        {
            if (vehicleSubMenu != null)
            {
                if (GarageMenu != null && GarageMenu.activeSelf)
                {
                    GarageMenu.SetActive(false);
                }
                vehicleSubMenu.SetActive(!vehicleSubMenu.activeSelf);
            }
        }

        public void ToggleGarageMenu()
        {
            if (GarageMenu != null)
            {
                if (vehicleSubMenu != null && vehicleSubMenu.activeSelf)
                {
                    vehicleSubMenu.SetActive(false);
                }
                GarageMenu.SetActive(!GarageMenu.activeSelf);
            }
        }

        public void LoadNextScene(int sceneIndex)
        {
            if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(sceneIndex);
            }
            else
            {
                Debug.LogWarning($"Scene index {sceneIndex} is out of range!");
            }
        }
    }
}