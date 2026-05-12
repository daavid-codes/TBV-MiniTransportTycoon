using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using MiniTransportTycoon;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text slotText;
    [SerializeField] private TMP_Text cityNameText;
    [SerializeField] private TMP_Text saveDateText;
    [SerializeField] private Button loadBTN;
    [SerializeField] private Button deleteBTN;

    public void Setup(SaveData data)
    {
        slotText.text = "Slot " + data.slot;
        cityNameText.text = data.cityName;
        saveDateText.text = data.saveDate;
        loadBTN.onClick.AddListener(() => LoadThisSlot(data.slot));
        deleteBTN.onClick.AddListener(() => DeleteThisSlot(data.slot));
    }

    private void LoadThisSlot(int slot)
    {
        GameSession.SlotToLoad = slot;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void DeleteThisSlot(int slot)
    {
        string path = Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

        if (File.Exists(path))
        {
            File.Delete(path);
            Destroy(gameObject);
            UnityEngine.Debug.Log("Deleted save slot " + slot);
        }
    }
}
