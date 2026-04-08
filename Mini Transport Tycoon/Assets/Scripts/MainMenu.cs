using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

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
     * Pszeudókód a jövöre tekintettel
     * azért "jövöre" mert némely karaktereket nem szereti a visual studio code 2022
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
