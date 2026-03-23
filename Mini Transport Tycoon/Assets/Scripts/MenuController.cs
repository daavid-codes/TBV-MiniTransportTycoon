using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject vehicleSubMenu;
    public GameObject bridgeSubMenu;

    public void ToggleVehicleMenu()
    {
        if (vehicleSubMenu != null)
        {
            bool isCurrentlyActive = vehicleSubMenu.activeSelf;
            bridgeSubMenu.SetActive(false);
            vehicleSubMenu.SetActive(!isCurrentlyActive);
        }
    }

    public void ToggleBridgeMenu()
    {
        if (bridgeSubMenu != null)
        {
            bool isCurrentlyActive = bridgeSubMenu.activeSelf;
            vehicleSubMenu.SetActive(false);
            bridgeSubMenu.SetActive(!isCurrentlyActive);
        }
    }

    //Scene switch
    public void LoadNextScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneName);
    }
    
}