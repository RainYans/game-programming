using UnityEditor;
using UnityEngine;

/// One-shot editor helper: generates the wild-zombie ZombieData assets used as enemies in
/// combat. Three types so stages can actually vary: a baseline grunt, a fast/fragile runner,
/// and a slow/tough brute. Run from: Tools > Zombie Farm > Setup Wild Zombies. Idempotent.
public static class WildZombiesSetup
{
    private const string Folder = "Assets/ScriptableObject";

    private struct Wild
    {
        public string assetName;
        public string id;
        public string displayName;
        public string role;
        public int hp;
        public int attack;
        public float moveSpeed;
        public Color color;
    }

    private static readonly Wild[] Wilds =
    {
        new Wild { assetName = "WildNormal", id = "wild_normal", displayName = "Wild Zombie",
            role = "Grunt", hp = 8, attack = 2, moveSpeed = 2.0f,
            color = new Color(0.55f, 0.30f, 0.30f) },

        new Wild { assetName = "WildRunner", id = "wild_runner", displayName = "Wild Runner",
            role = "Skirmisher", hp = 5, attack = 2, moveSpeed = 3.6f,
            color = new Color(0.85f, 0.55f, 0.20f) },

        new Wild { assetName = "WildBrute",  id = "wild_brute",  displayName = "Wild Brute",
            role = "Bruiser", hp = 18, attack = 4, moveSpeed = 1.4f,
            color = new Color(0.40f, 0.20f, 0.20f) },
    };

    [MenuItem("Tools/Zombie Farm/Setup Wild Zombies")]
    public static void SetupWildZombies()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "ScriptableObject");

        foreach (Wild w in Wilds)
        {
            string path = $"{Folder}/{w.assetName}.asset";
            var data = AssetDatabase.LoadAssetAtPath<ZombieData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ZombieData>();
                AssetDatabase.CreateAsset(data, path);
            }
            data.id = w.id;
            data.displayName = w.displayName;
            data.role = w.role;
            data.maxHp = w.hp;
            data.attack = w.attack;
            data.moveSpeed = w.moveSpeed;
            data.range = AttackRange.Melee;
            data.passive = Passive.None;
            data.unlockedAtStart = false;
            data.color = w.color;
            EditorUtility.SetDirty(data);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[WildZombiesSetup] Wild zombies ready: WildNormal / WildRunner / WildBrute. " +
                  "Now re-run 'Setup City 1 Stages' so the city uses the mix.");
    }
}
