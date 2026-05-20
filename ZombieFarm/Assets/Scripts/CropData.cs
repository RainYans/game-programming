using UnityEngine;

/// Definition of one plantable engineered-zombie seed. Tunable, art-free (stage colors).
[CreateAssetMenu(fileName = "CropData", menuName = "ZombieFarm/Crop Data")]
public class CropData : ScriptableObject
{
    public string id = "basic";
    public string displayName = "Basic Zombie";

    [Min(0.1f)] public float growSeconds = 10f;
    [Min(1)] public int yieldCount = 1;

    [Header("Placeholder stage colors")]
    public Color seedColor = new Color(0.55f, 0.40f, 0.22f);   // brown
    public Color growingColor = new Color(0.40f, 0.80f, 0.30f); // green
    public Color ripeColor = new Color(0.85f, 0.20f, 0.20f);    // red
}
