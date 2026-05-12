using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniTransportTycoon;

public class SaveSlotPrefab : MonoBehaviour
{
    [SerializeField] private Transform saveSlotContainer;
    [SerializeField] private SaveSlotUI saveSlotPrefab;

    void Start()
    {
        PopulateSaveSlots();
    }

    private void PopulateSaveSlots()
    {
        UnityEngine.Debug.Log("PopulateSaveSlots called, save count: " + SaveReader.GetAllSaves().Count);

        foreach (Transform child in saveSlotContainer)
        {
            Destroy(child.gameObject);
        }

        List<SaveData> saves = SaveReader.GetAllSaves();

        foreach(SaveData save in saves)
        {
            SaveSlotUI slot = Instantiate(saveSlotPrefab, saveSlotContainer);
            slot.Setup(save);
        }
    }
}
