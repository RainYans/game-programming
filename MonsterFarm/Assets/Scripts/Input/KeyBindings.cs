using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// The actions the player can rebind.
public enum BindAction { MoveUp, MoveDown, MoveLeft, MoveRight, Interact, Dash }

/// Rebindable keyboard controls. The game reads input directly via Keyboard.current (no
/// InputActionAsset), so this is a tiny indirection layer: each action maps to a Key, persisted in
/// PlayerPrefs and editable at runtime from the options menu. Defaults = classic WASD + E + Shift.
public static class KeyBindings
{
    private static readonly Key[] Defaults = { Key.W, Key.S, Key.A, Key.D, Key.E, Key.LeftShift };
    private static Key[] keys;

    private static void Ensure()
    {
        if (keys != null) return;
        keys = new Key[Defaults.Length];
        for (int i = 0; i < keys.Length; i++)
            keys[i] = (Key)PlayerPrefs.GetInt("bind_" + i, (int)Defaults[i]);
    }

    public static Key Get(BindAction a) { Ensure(); return keys[(int)a]; }

    public static void Set(BindAction a, Key k)
    {
        Ensure();
        keys[(int)a] = k;
        PlayerPrefs.SetInt("bind_" + (int)a, (int)k);
    }

    public static void ResetDefaults()
    {
        Ensure();
        for (int i = 0; i < keys.Length; i++) Set((BindAction)i, Defaults[i]);
    }

    public static string Label(BindAction a) => Get(a).ToString();

    private static KeyControl Ctrl(BindAction a)
    {
        var kb = Keyboard.current;
        return kb != null ? kb[Get(a)] : null;
    }

    public static bool Held(BindAction a) { var c = Ctrl(a); return c != null && c.isPressed; }
    public static bool Pressed(BindAction a) { var c = Ctrl(a); return c != null && c.wasPressedThisFrame; }
}
