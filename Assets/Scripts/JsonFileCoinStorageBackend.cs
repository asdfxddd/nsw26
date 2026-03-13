using System;
using System.IO;
using UnityEngine;

public class JsonFileCoinStorageBackend : ICoinStorageBackend
{
    private readonly string filePath;

    public JsonFileCoinStorageBackend(string fileName = "coin_save_data.json")
    {
        filePath = Path.Combine(Application.persistentDataPath, fileName);
    }

    public bool TryLoad(out CoinSaveData data)
    {
        data = null;

        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            string raw = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            data = JsonUtility.FromJson<CoinSaveData>(raw);
            return data != null;
        }
        catch (Exception)
        {
            data = null;
            return false;
        }
    }

    public bool TrySave(CoinSaveData data)
    {
        if (data == null)
        {
            return false;
        }

        try
        {
            string raw = JsonUtility.ToJson(data);
            File.WriteAllText(filePath, raw);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
