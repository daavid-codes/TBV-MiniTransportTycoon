using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using MiniTransportTycoon;

public class SaveReader : MonoBehaviour
{
    public static SaveData ReadSlot(int slot)
    {
        string path = Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
        if (!File.Exists(path))
        {
            return null;
        }
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }

    public static List<SaveData> GetAllSaves()
    {
        List<SaveData> saves = new List<SaveData>();

        for (int i = 0; i < 100; i++)
        {
            SaveData data = ReadSlot(i);
            if( data != null)
            {
                saves.Add(data);
            }
        }

        return saves;
    }

    public static int GetLatestSlot()
    {
        List<SaveData> saves = GetAllSaves();

        if(saves.Count == 0)
        {
            return -1;
        }

        SaveData latest = saves[0];

        foreach(SaveData s in saves)
        {
            if(DateTime.Parse(s.saveDate) > DateTime.Parse(latest.saveDate))
            {
                latest = s;
            }
        }

        return latest.slot;
    }

    public static int GetNextSlot()
    {
        int maxslot = -1;

        for (int i = 0;i < 100; i++)
        {
            string path = Path.Combine(Application.persistentDataPath, $"save_{i}.json");
            //Debug.Log("Checking path: " + path + " exists: " + File.Exists(path));
            if (File.Exists(path))
            {
                maxslot = i;
            }
        }

        return maxslot + 1;
    }
}
