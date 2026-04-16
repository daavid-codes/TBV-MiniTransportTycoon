using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniTransportTycoon
{
    public class MenuController : MonoBehaviour
    {
    public GameObject vehicleSubMenu;

    public void ToggleVehicleMenu()
    {
        if (vehicleSubMenu != null)
        {
            bool isCurrentlyActive = vehicleSubMenu.activeSelf;
            vehicleSubMenu.SetActive(!isCurrentlyActive);
        }
    }


    //Scene switch
    public void LoadNextScene(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings) {
            SceneManager.LoadScene(sceneIndex);
        }
        else {
            Debug.LogWarning($"Scene index {sceneIndex} is out of range!");
        }
    }
    }
}