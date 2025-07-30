using System.Collections.Generic;

[System.Serializable]
public class ChestStateData
{
    public string chestID;
    public bool isOpen;
    public bool wasUnlocked;
    public bool padlockRemoved;
}

[System.Serializable]
public class CheckpointData
{
    public bool hasKey;
    public bool hasDynamite;
    public bool hasLighter;

    public bool hasPlacedDynamite;
    public bool hasExploded;

    public List<ChestStateData> chestStates = new();

    public string sceneName;
    public float playerX;
    public float playerY;
    public float playerZ;

    public bool isEnded;
}