using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerSaveData
{
    public float currentHealth;
}

public class SaveSystem : MonoBehaviour
{
    private string savePath => Path.Combine(Application.persistentDataPath, "playerSave.json");

    public void SavePlayer(PlayerState player)
    {
        PlayerSaveData data = new PlayerSaveData
        {
            currentHealth = player.currentHealth
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public PlayerSaveData LoadPlayer()
    {
        if (!File.Exists(savePath))
            return null;

        string json = File.ReadAllText(savePath);
        return JsonUtility.FromJson<PlayerSaveData>(json);
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);
    }
}
