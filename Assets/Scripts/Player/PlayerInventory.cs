using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasKey = false;
    public bool hasDynamite = false;
    public bool hasLighter = false;

    public void LoadInventoryFromCheckpoint(CheckpointData data)
    {
        hasKey = data.hasKey;
        hasDynamite = data.hasDynamite;
        hasLighter = data.hasLighter;
    }
}