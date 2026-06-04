using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// One-shot editor helper (run with the FARM scene open): stands up the combat-item economy
/// that the MVP gate needs — the Rotten Onion bought in the Shop, carried into a raid, and
/// consumed when thrown.
///
/// It (1) adds an ItemInventory to the Systems object next to Inventory/SeedInventory,
/// (2) ensures GameConfig.itemCatalog contains the Rotten Onion entry, and (3) wires the new
/// `itemInventory` reference on the SaveManager / ShopController / ShopPanelUI / DeployPanel /
/// BattleResultApplier so they persist + carry it. All those also auto-find at runtime, so the
/// wiring is belt-and-suspenders.
///
/// Run from: Tools > Zombie Farm > Setup Item Shop. Idempotent.
public static class ItemShopSetup
{
    private const string OnionDisplayName = "Rotten Onion";
    private const int OnionPrice = 12; // placeholder; tune in the M4 balancing pass (#73)
    private const string FreezeDisplayName = "Freeze Canister";
    private const int FreezePrice = 18;

    [MenuItem("Tools/Zombie Farm/Setup Item Shop")]
    public static void SetupItemShop()
    {
        ItemInventory item = EnsureItemInventory(out GameObject host);
        if (item == null)
        {
            EditorUtility.DisplayDialog("No Systems object",
                "Couldn't find the Inventory / Systems object in the open scene. " +
                "Open the farm scene first.", "OK");
            return;
        }

        EnsureItemCatalog();
        WireRefs(item);

        EditorSceneManager.MarkSceneDirty(host.scene);
        Selection.activeGameObject = host;

        Debug.Log("[ItemShopSetup] ItemInventory added to Systems, Rotten Onion ensured in " +
                  "GameConfig.itemCatalog, and item refs wired (SaveManager / Shop / Deploy / " +
                  "ResultApplier). The Shop now sells the Rotten Onion; bought onions are carried " +
                  "into a raid and consumed when thrown. Save the scene (Ctrl+S).");
    }

    private static ItemInventory EnsureItemInventory(out GameObject host)
    {
        Inventory inv = Object.FindFirstObjectByType<Inventory>();
        host = inv != null ? inv.gameObject : GameObject.Find("Systems");
        if (host == null) return null;

        ItemInventory item = host.GetComponent<ItemInventory>();
        if (item == null) item = host.AddComponent<ItemInventory>();
        return item;
    }

    private static void EnsureItemCatalog()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameConfig");
        if (guids.Length == 0) { Debug.LogWarning("[ItemShopSetup] No GameConfig asset found."); return; }

        var config = AssetDatabase.LoadAssetAtPath<GameConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        if (config == null) return;

        if (config.itemCatalog == null) config.itemCatalog = new List<GameConfig.ItemEntry>();
        Upsert(config, GameConfig.RottenOnionId, OnionDisplayName, OnionPrice);
        Upsert(config, GameConfig.FreezeCanisterId, FreezeDisplayName, FreezePrice);

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
    }

    /// Add the item to the catalog, or refresh its name/price if it's already there (idempotent).
    private static void Upsert(GameConfig config, string id, string displayName, int price)
    {
        var entry = new GameConfig.ItemEntry { id = id, displayName = displayName, price = price };
        int idx = config.itemCatalog.FindIndex(e => e.id == id);
        if (idx >= 0) config.itemCatalog[idx] = entry;
        else config.itemCatalog.Add(entry);
    }

    private static void WireRefs(ItemInventory item)
    {
        WireField(Object.FindFirstObjectByType<SaveManager>(), item);
        WireField(Object.FindFirstObjectByType<ShopController>(), item);
        WireField(Object.FindFirstObjectByType<ShopPanelUI>(), item);
        WireField(Object.FindFirstObjectByType<DeployPanel>(), item);
        WireField(Object.FindFirstObjectByType<BattleResultApplier>(), item);
    }

    /// Set the serialized `itemInventory` field on `target` to `value` if the field exists and
    /// is currently empty. No-op when the component isn't present in the scene.
    private static void WireField(Object target, ItemInventory value)
    {
        if (target == null) return;
        var so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty("itemInventory");
        if (p != null && p.objectReferenceValue == null)
        {
            p.objectReferenceValue = value;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
