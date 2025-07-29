using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            savePath = Application.persistentDataPath + "/checkpoint.dat";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveCheckpoint(CheckpointData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public CheckpointData LoadCheckpoint()
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Open);

            CheckpointData data = (CheckpointData)formatter.Deserialize(stream);
            stream.Close();
            return data;
        }

        return null;
    }

    public void DeleteCheckpoint()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }

    public static bool HasCheckpoint()
    {
        string path = Application.persistentDataPath + "/checkpoint.dat";
        return File.Exists(path);
    }
}