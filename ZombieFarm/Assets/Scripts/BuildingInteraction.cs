using UnityEngine;
using UnityEngine.InputSystem;

public enum BuildingType { Shop, BattleGate }

/// Click detection on scene buildings. Opens the corresponding UI page via UIManager.
/// Farm input is blocked while a UI page is open (handled by UIManager).
public class BuildingInteraction : MonoBehaviour
{
    [SerializeField] private BuildingType buildingType;
    [SerializeField] private UIManager uiManager;

    private Camera cam;
    private Collider2D col2D;

    private void Awake()
    {
        cam = Camera.main;
        col2D = GetComponent<Collider2D>();
        if (uiManager == null) uiManager = FindFirstObjectByType<UIManager>();
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || cam == null || col2D == null) return;
        if (uiManager == null) return;
        if (!mouse.leftButton.wasPressedThisFrame) return;

        Vector3 world = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        world.z = transform.position.z;

        if (col2D.OverlapPoint(world))
        {
            if (buildingType == BuildingType.Shop)
                uiManager.OpenShop();
            else if (buildingType == BuildingType.BattleGate)
                uiManager.OpenBattle();
        }
    }
}
