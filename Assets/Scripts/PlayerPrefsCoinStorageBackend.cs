using UnityEngine;

public class PlayerPrefsCoinStorageBackend : ICoinStorageBackend
{
    private const string SaveKey = "coin_save_data_v1";

    public bool TryLoad(out CoinSaveData data)
    {
        data = null;

        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return false;
        }

        string raw = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        try
        {
            data = JsonUtility.FromJson<CoinSaveData>(raw);
            return data != null;
        }
        catch
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
            PlayerPrefs.SetString(SaveKey, raw);
            PlayerPrefs.Save();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
