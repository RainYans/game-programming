using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// One-shot editor helper: fills the City1 MissionData with three escalating stages of
/// mixed wild zombies (normal / runner / brute) and a city-clear reward, so each stage
/// feels different. Run from: Tools > Zombie Farm > Setup City 1 Stages.
/// Editor-only; re-running overwrites.
public static class CitySetup
{
    [MenuItem("Tools/Zombie Farm/Setup City 1 Stages")]
    public static void SetupCity1()
    {
        MissionData city = FindMission("city1");
        if (city == null) { Debug.LogWarning("[CitySetup] City1 MissionData not found."); return; }

        ZombieData normal = FindWild("normal") ?? FindWild("zombi"); // fall back to old WildZombile typo
        ZombieData runner = FindWild("runner");
        ZombieData brute  = FindWild("brute");

        if (normal == null)
        {
            Debug.LogWarning("[CitySetup] No wild ZombieData found. Run 'Setup Wild Zombies' first.");
            return;
        }

        // Stage 1: a warm-up of grunts.
        // Stage 2: grunts + a couple of runners (you have to deal with mobility).
        // Stage 3: grunts + brutes (heavy hitters — bring CC).
        city.stages = new List<MissionData.Stage>
        {
            Stage((normal, 4)),
            runner != null ? Stage((normal, 3), (runner, 2)) : Stage((normal, 5)),
            brute  != null ? Stage((normal, 4), (brute,  2)) : Stage((normal, 6)),
        };
        city.rewardAmount = 120;

        EditorUtility.SetDirty(city);
        AssetDatabase.SaveAssets();
        Debug.Log($"[CitySetup] {city.cityName}: 3 mixed stages, reward {city.rewardAmount}.");
    }

    private static MissionData.Stage Stage(params (ZombieData zombie, int count)[] spawns)
    {
        var s = new MissionData.Stage();
        foreach (var sp in spawns)
            if (sp.zombie != null)
                s.enemies.Add(new MissionData.EnemySpawn { zombie = sp.zombie, count = sp.count });
        return s;
    }

    private static MissionData FindMission(string nameContains)
    {
        string[] guids = AssetDatabase.FindAssets("t:MissionData");
        MissionData first = null;
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var m = AssetDatabase.LoadAssetAtPath<MissionData>(path);
            if (first == null) first = m;
            if (Path.GetFileNameWithoutExtension(path).ToLowerInvariant().Contains(nameContains)) return m;
        }
        return first;
    }

    /// Find a wild ZombieData whose ASSET NAME contains `kind` ("normal" / "runner" / "brute").
    /// Falls back to the old "WildZombile" typo if "normal" isn't found.
    private static ZombieData FindWild(string kind)
    {
        foreach (string g in AssetDatabase.FindAssets("t:ZombieData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            string n = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (n.StartsWith("wild") && n.Contains(kind)) return AssetDatabase.LoadAssetAtPath<ZombieData>(path);
        }
        return null;
    }
}
