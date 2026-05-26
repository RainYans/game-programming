using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// One-shot editor helper: builds the farm avatar in the currently open scene and wires the
/// Main Camera to follow it. Run from the menu: Tools > Zombie Farm > Setup Avatar.
///
/// Editor-only (lives under an /Editor folder, so it is NOT compiled into game builds).
/// Idempotent: running it again reuses the existing "Avatar" object instead of duplicating.
public static class AvatarSetup
{
    private const string AvatarName = "Avatar";

    [MenuItem("Tools/Zombie Farm/Setup Avatar")]
    public static void SetupAvatar()
    {
        // 1. Find or create the Avatar GameObject.
        GameObject avatar = GameObject.Find(AvatarName);
        if (avatar == null)
        {
            avatar = new GameObject(AvatarName);
            Undo.RegisterCreatedObjectUndo(avatar, "Create Avatar");
            avatar.transform.position = Vector3.zero; // farm origin; move it in the scene as needed
        }

        // 2. SpriteRenderer with a visible placeholder sprite.
        SpriteRenderer sr = avatar.GetComponent<SpriteRenderer>();
        if (sr == null) sr = Undo.AddComponent<SpriteRenderer>(avatar);
        if (sr.sprite == null)
        {
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(0.30f, 0.60f, 1f); // bluish, so it stands out from the ground
        }
        // Quick visibility choice: draw above the ground tilemap. For proper isometric
        // "walk in front of / behind" depth later, match this to the crops' sorting layer
        // & order and let the Transparency Sort Axis (0,1,0) handle front/back by Y.
        sr.sortingOrder = 5;

        // 3. Physics body + collider so the avatar collides with solid objects (buildings).
        Rigidbody2D rb = avatar.GetComponent<Rigidbody2D>();
        if (rb == null) rb = Undo.AddComponent<Rigidbody2D>(avatar);
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (avatar.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D cc = Undo.AddComponent<CircleCollider2D>(avatar);
            cc.radius = 0.3f;
        }

        // 4. Movement + interaction controllers (defaults: moveSpeed 4, isoYScale 0.5).
        if (avatar.GetComponent<AvatarController>() == null)
            Undo.AddComponent<AvatarController>(avatar);
        if (avatar.GetComponent<AvatarInteraction>() == null)
            Undo.AddComponent<AvatarInteraction>(avatar);

        // 5. Mark dirty + select so the change is saved with the scene.
        //    Camera follow is set up separately via "Setup Cinemachine Camera".
        EditorUtility.SetDirty(avatar);
        EditorSceneManager.MarkSceneDirty(avatar.scene);
        Selection.activeGameObject = avatar;

        Debug.Log("[AvatarSetup] Done. Avatar created with WASD movement + E-to-interact. " +
                  "Run 'Setup Cinemachine Camera' to make the camera follow it. Press Play: " +
                  "WASD to move, walk onto a field tile, press E to plant/harvest.");
    }
}
