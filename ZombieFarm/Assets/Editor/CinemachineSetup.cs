using Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// One-shot editor helper for the Cinemachine follow camera (Cinemachine 2.9.x).
/// Run from: Tools > Zombie Farm > Setup Cinemachine Camera. Editor-only; idempotent.
///
/// It:
///  - adds a CinemachineBrain to the Main Camera (so Cinemachine drives it),
///  - creates a virtual camera "FollowCamera" that follows the Avatar via a Framing Transposer,
///  - creates a "CameraBounds" PolygonCollider2D and wires a CinemachineConfiner2D so the
///    camera view stays inside it (reshape the collider with "Edit Collider"),
///  - wires the CameraController (scroll-zoom) to the virtual camera.
public static class CinemachineSetup
{
    [MenuItem("Tools/Zombie Farm/Setup Cinemachine Camera")]
    public static void Setup()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("[CinemachineSetup] No camera tagged 'MainCamera' found. Aborting.");
            return;
        }

        // 1. Brain on the main camera.
        if (mainCam.GetComponent<CinemachineBrain>() == null)
            Undo.AddComponent<CinemachineBrain>(mainCam.gameObject);

        // 2. Camera bounds collider (reshape later with Edit Collider).
        GameObject boundsGo = GameObject.Find("CameraBounds");
        if (boundsGo == null)
        {
            boundsGo = new GameObject("CameraBounds");
            Undo.RegisterCreatedObjectUndo(boundsGo, "Create CameraBounds");
        }
        PolygonCollider2D bounds = boundsGo.GetComponent<PolygonCollider2D>();
        if (bounds == null)
        {
            bounds = Undo.AddComponent<PolygonCollider2D>(boundsGo);
            bounds.isTrigger = true;
            bounds.points = new[]
            {
                new Vector2(-10f, -6f), new Vector2(10f, -6f),
                new Vector2(10f, 6f), new Vector2(-10f, 6f),
            };
        }

        // 3. Virtual camera following the Avatar.
        GameObject vcamGo = GameObject.Find("FollowCamera");
        if (vcamGo == null)
        {
            vcamGo = new GameObject("FollowCamera");
            Undo.RegisterCreatedObjectUndo(vcamGo, "Create FollowCamera");
        }
        CinemachineVirtualCamera vcam = vcamGo.GetComponent<CinemachineVirtualCamera>();
        if (vcam == null) vcam = Undo.AddComponent<CinemachineVirtualCamera>(vcamGo);

        GameObject avatar = GameObject.Find("Avatar");
        if (avatar != null) vcam.Follow = avatar.transform;
        else Debug.LogWarning("[CinemachineSetup] No 'Avatar' found — run 'Setup Avatar' first, " +
                              "then re-run this to wire Follow.");

        vcam.m_Lens.OrthographicSize = mainCam.orthographicSize > 0f ? mainCam.orthographicSize : 5f;

        // Body: Framing Transposer is the 2D-friendly follow body.
        var transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (transposer == null) transposer = vcam.AddCinemachineComponent<CinemachineFramingTransposer>();
        transposer.m_XDamping = 0.5f;
        transposer.m_YDamping = 0.5f;
        transposer.m_ZDamping = 0f;

        // 4. Confiner2D extension bounded by the collider.
        CinemachineConfiner2D confiner = vcamGo.GetComponent<CinemachineConfiner2D>();
        if (confiner == null) confiner = Undo.AddComponent<CinemachineConfiner2D>(vcamGo);
        confiner.m_BoundingShape2D = bounds;
        // Cinemachine computes the confiner cache on first use. If you later reshape the
        // CameraBounds collider, click "Invalidate Cache" on the Confiner2D component.

        // 5. Wire CameraController (zoom) to the vcam.
        CameraController zoom = mainCam.GetComponent<CameraController>();
        if (zoom != null)
        {
            var so = new SerializedObject(zoom);
            SerializedProperty vcamProp = so.FindProperty("vcam");
            if (vcamProp != null)
            {
                vcamProp.objectReferenceValue = vcam;
                so.ApplyModifiedProperties();
            }
        }

        EditorUtility.SetDirty(mainCam.gameObject);
        EditorUtility.SetDirty(vcamGo);
        EditorUtility.SetDirty(boundsGo);
        EditorSceneManager.MarkSceneDirty(vcamGo.scene);
        Selection.activeGameObject = boundsGo;

        Debug.Log("[CinemachineSetup] Done. Camera now follows the Avatar via Cinemachine. " +
                  "Select 'CameraBounds' and use 'Edit Collider' to shape the camera limits to " +
                  "your farm. Press Play to test, then save the scene (Ctrl+S).");
    }
}
