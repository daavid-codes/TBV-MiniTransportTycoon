using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniTransportTycoon
{
    public class MainMenu : MonoBehaviour
    {
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitToDesktop()
    {
        UnityEngine.Debug.Log("QuitToDesktop");
        Application.Quit();
    }

    /*
     * Pszeud�k�d a j�v�re tekintettel
     * az�rt "j�v�re" mert n�mely karaktereket nem szereti a visual studio code 2022
     * 
     public void Awake() {
        if (CheckForSaveFiles())
        {
            LoadLatestFile(getLatestFile());
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else {
            disableResumeGameButton();
        }
    }

    bool CheckForSaveFiles() {
        //implement
        return false;
    }

    string getLatestFile() {
        //implement
        return "";
    }

    void LoadLatestFile(string FilePath) {
        //implement
    }
     */

    }
}
