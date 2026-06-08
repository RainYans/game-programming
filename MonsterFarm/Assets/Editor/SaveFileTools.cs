using System.IO;
using UnityEditor;
using UnityEngine;

/// Editor helpers for the JSON save file SaveManager writes to persistentDataPath. Handy when
/// iterating: delete the save to bootstrap a fresh game (new starting seeds/state), or open the
/// folder to inspect save.json. Run from: Tools > Monster Farm > Save File.
public static class SaveFileTools
{
    // Mirrors SaveManager's default fileName.
    private const string FileName = "save.json";

    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    [MenuItem("Tools/Monster Farm/Save File/Delete Save File")]
    public static void DeleteSaveFile()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play Mode first",
                "Exit Play Mode before deleting the save — the game re-saves on quit.", "OK");
            return;
        }

        if (!File.Exists(SavePath))
        {
            Debug.Log($"[SaveFileTools] No save to delete at {SavePath}");
            return;
        }

        if (EditorUtility.DisplayDialog("Delete save?",
                $"Delete the save file?\n\n{SavePath}\n\nNext launch starts a fresh game.",
                "Delete", "Cancel"))
        {
            File.Delete(SavePath);
            Debug.Log($"[SaveFileTools] Deleted {SavePath}");
        }
    }

    [MenuItem("Tools/Monster Farm/Save File/Open Save Folder")]
    public static void OpenSaveFolder()
    {
        EditorUtility.RevealInFinder(SavePath);
    }
}
