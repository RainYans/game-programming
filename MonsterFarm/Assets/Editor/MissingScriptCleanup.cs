using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// One-shot editor helper: removes every "missing script" component from the open scene(s).
/// After deleting the dead prototype scripts (DeployController / BattlePlayer / ...), the
/// components they used to back become "Missing (Mono Script)" placeholders on objects like
/// Systems. This strips those placeholders surgically — it only removes components whose script
/// can't be loaded, and never touches live components or deletes any GameObject.
///
/// Run from: Tools > Monster Farm > Clean Missing Scripts (open scene). Then save (Ctrl+S).
public static class MissingScriptCleanup
{
    [MenuItem("Tools/Monster Farm/Clean Missing Scripts (open scene)")]
    public static void Clean()
    {
        int removed = 0, touched = 0;

        for (int s = 0; s < SceneManager.sceneCount; s++)
        {
            Scene scene = SceneManager.GetSceneAt(s);
            if (!scene.isLoaded) continue;

            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform tf in root.GetComponentsInChildren<Transform>(true))
                {
                    int n = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(tf.gameObject);
                    if (n > 0) { removed += n; touched++; }
                }

            if (removed > 0) EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"[MissingScriptCleanup] Removed {removed} missing-script component(s) from " +
                  $"{touched} object(s) in the open scene(s). Save the scene (Ctrl+S).");
    }
}
