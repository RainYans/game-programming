using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// One-shot editor helper: builds Assets/Scenes/Battle.unity as an L-shaped iso dungeon —
/// three larger rooms connected by locked gates. Room 0 is the entrance (bottom-left); Room 1
/// is to its right (cell-x+, screen up-right via iso), Room 2 is above Room 1 (cell-y+, screen
/// up-left). Walls + gates have visible placeholder sprites so the iso dungeon reads at a
/// glance — swap them out when real wall art lands.
///
/// Run from: Tools > Zombie Farm > Setup Battle Scene. Editor-only; re-running rebuilds.
public static class BattleSceneSetup
{
    private const string ScenePath = "Assets/Scenes/Battle.unity";
    private const string StrainFolder = "Assets/ScriptableObject/Strains";

    // --- Layout (iso cell coords) -----------------------------------------------------------
    // Each room is 9 wide x 9 tall. The wall columns / rows between rooms have a 3-cell-wide
    // door (the middle 3 cells along the wall's axis). Cell -> world via the iso projection
    // ((x-y)*0.5, (x+y)*0.25).

    private struct RoomRect { public int xMin, xMax, yMin, yMax; }
    // Rooms are 13x13 cells now — substantially bigger, with room for pillars + tactical play.
    private static readonly RoomRect Room0 = new RoomRect { xMin = -16, xMax = -4, yMin = -6, yMax =  6 };
    private static readonly RoomRect Room1 = new RoomRect { xMin =  -2, xMax = 10, yMin = -6, yMax =  6 };
    private static readonly RoomRect Room2 = new RoomRect { xMin =  -2, xMax = 10, yMin =  8, yMax = 20 };

    private const int WallA_Column = -3;
    private const int WallA_DoorYMin = -1, WallA_DoorYMax = 1;

    private const int WallB_Row = 7;
    private const int WallB_DoorXMin = 3, WallB_DoorXMax = 5;

    private static readonly Vector2Int SquadSpawnCell = new Vector2Int(-12, 0);
    private static readonly Vector2Int[] EnemySpawnCells =
    {
        new Vector2Int(-9, 0),  // Room 0
        new Vector2Int( 4, 0),  // Room 1
        new Vector2Int( 4, 14), // Room 2
    };

    /// Interior obstacles per room (cell coords). Each becomes a small pillar GameObject
    /// with a sprite + collider so the leader has to navigate around them.
    private static readonly Vector2Int[][] PillarCellsPerRoom =
    {
        new[] { new Vector2Int(-13, -3), new Vector2Int( -7,  3) },
        new[] { new Vector2Int(  1, -3), new Vector2Int(  7,  3) },
        new[] { new Vector2Int(  1, 11), new Vector2Int(  7, 17) },
    };

    // --- Menu -------------------------------------------------------------------------------

    [MenuItem("Tools/Zombie Farm/Setup Battle Scene")]
    public static void SetupBattleScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Sprite placeholder = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        BuildCamera(out BattleCameraFollow follow);
        BuildGlobalLight();
        BuildRoomFloors();

        var walls = new GameObject("Walls").transform;
        var roomRoots = new GameObject("Rooms").transform;

        // Spawn anchors per room.
        Transform squadSpawn = MakeAnchor("SquadSpawn_R0", CellWorld(SquadSpawnCell), roomRoots);
        Transform[] enemySpawns =
        {
            MakeAnchor("EnemySpawn_R0", CellWorld(EnemySpawnCells[0]), roomRoots),
            MakeAnchor("EnemySpawn_R1", CellWorld(EnemySpawnCells[1]), roomRoots),
            MakeAnchor("EnemySpawn_R2", CellWorld(EnemySpawnCells[2]), roomRoots),
        };

        // Inter-room walls + gates. Two different orientations.
        BattleGate gateA = BuildVerticalWallWithGate(walls, placeholder,
            WallA_Column, Room0.yMin, Room0.yMax, WallA_DoorYMin, WallA_DoorYMax, "Gate_1");
        BattleGate gateB = BuildHorizontalWallWithGate(walls, placeholder,
            WallB_Row, Room1.xMin, Room1.xMax, WallB_DoorXMin, WallB_DoorXMax, "Gate_2");

        // Outer walls per room — frames each room as a real iso chamber.
        BuildOuterWalls(walls, placeholder);

        // Interior obstacles (pillars). Force the leader to navigate around them.
        var obstacles = new GameObject("Obstacles").transform;
        BuildPillars(obstacles, placeholder);

        // Leader at the squad spawn.
        GameObject leader = BuildLeader(squadSpawn.position, placeholder);
        follow.SetTarget(leader.transform);

        BuildHud(font, out GameObject canvasGo, out TextMeshProUGUI resultLabel, out GameObject returnGo);
        BuildOperationalUi(canvasGo, font,
            out RectTransform dragBox, out TMP_Text onionLabel, out TMP_Text freezeLabel,
            out GameObject targetingHint, out TMP_Text targetingHintLabel,
            out RectTransform squadHudParent, out RectTransform squadRowTemplate);
        BuildPauseMenu(canvasGo, font,
            out GameObject pausePanel, out Button pauseResumeBtn, out Button pauseReturnBtn);

        // BattleManager.
        var mgrGo = new GameObject("BattleManager");
        var mgr = mgrGo.AddComponent<BattleManager>();
        var so = new SerializedObject(mgr);
        so.FindProperty("leader").objectReferenceValue = leader.transform;
        so.FindProperty("resultLabel").objectReferenceValue = resultLabel;
        so.FindProperty("returnButton").objectReferenceValue = returnGo;
        so.FindProperty("mission").objectReferenceValue = LoadMission();
        SetSquad(so, LoadStrain("Brute"), LoadStrain("Mauler"), LoadStrain("Runner"));
        SetEnemiesFallback(so, ResolveEnemy(), 3);
        SetRooms(so, squadSpawn, enemySpawns, new[] { gateA, gateB });
        so.ApplyModifiedProperties();

        // Mouse selection / right-click commands / Rotten Onion + HUDs (wired with the UI we built).
        var cmdsGo = new GameObject("Commands");
        var ctrl = cmdsGo.AddComponent<BattleCommandController>();
        var ctrlSo = new SerializedObject(ctrl);
        ctrlSo.FindProperty("battleCamera").objectReferenceValue = Object.FindFirstObjectByType<Camera>();
        ctrlSo.FindProperty("manager").objectReferenceValue = mgr;
        ctrlSo.FindProperty("canvas").objectReferenceValue = canvasGo.GetComponent<Canvas>();
        ctrlSo.FindProperty("dragBox").objectReferenceValue = dragBox;
        ctrlSo.FindProperty("onionLabel").objectReferenceValue = onionLabel;
        ctrlSo.FindProperty("freezeLabel").objectReferenceValue = freezeLabel;
        ctrlSo.FindProperty("targetingHint").objectReferenceValue = targetingHint;
        ctrlSo.FindProperty("targetingHintLabel").objectReferenceValue = targetingHintLabel;
        ctrlSo.FindProperty("squadHudParent").objectReferenceValue = squadHudParent;
        ctrlSo.FindProperty("squadRowTemplate").objectReferenceValue = squadRowTemplate;
        ctrlSo.ApplyModifiedProperties();

        // Pause menu.
        var pauseGo = new GameObject("Pause");
        var pauseMenu = pauseGo.AddComponent<BattlePauseMenu>();
        var pauseSo = new SerializedObject(pauseMenu);
        pauseSo.FindProperty("panel").objectReferenceValue = pausePanel;
        pauseSo.FindProperty("resumeButton").objectReferenceValue = pauseResumeBtn;
        pauseSo.FindProperty("returnButton").objectReferenceValue = pauseReturnBtn;
        pauseSo.FindProperty("manager").objectReferenceValue = mgr;
        pauseSo.FindProperty("commandController").objectReferenceValue = ctrl;
        pauseSo.ApplyModifiedProperties();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        EnsureInBuildSettings(ScenePath);
        EnsureInBuildSettings("Assets/Scenes/Farm.unity");

        Debug.Log("[BattleSceneSetup] Built Battle.unity: 3 iso rooms in an L-shape with visible " +
                  "placeholder walls + gates. Deploy from the farm WarCamp; clear a room to open " +
                  "the next gate; clear the final room to reclaim the city.");
    }

    // --- iso math ---------------------------------------------------------------------------

    private static Vector3 CellWorld(int x, int y) => new Vector3((x - y) * 0.5f, (x + y) * 0.25f, 0f);
    private static Vector3 CellWorld(Vector2Int c) => CellWorld(c.x, c.y);
    private static Vector3 CellWorld(float x, float y) => new Vector3((x - y) * 0.5f, (x + y) * 0.25f, 0f);

    /// World distance between two cells whose centers are 1 unit apart on the iso x OR y axis.
    private static float CellStep => Mathf.Sqrt(0.25f + 0.0625f); // ≈ 0.559

    /// Rotation (deg, Z) so the local X axis aligns with the iso-y cell direction in world.
    private static float IsoYAngle()
    {
        Vector2 d = new Vector2(-0.5f, 0.25f);
        return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg; // ≈ 153.43°
    }

    /// Rotation so local X aligns with the iso-x cell direction in world.
    private static float IsoXAngle()
    {
        Vector2 d = new Vector2(0.5f, 0.25f);
        return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg; // ≈ 26.57°
    }

    // --- scene pieces -----------------------------------------------------------------------

    private static void BuildCamera(out BattleCameraFollow follow)
    {
        var camGo = new GameObject("Main Camera", typeof(Camera));
        camGo.tag = "MainCamera";
        var cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f; // bigger view to keep a 13x13 room comfortably framed
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.07f, 0.09f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        follow = camGo.AddComponent<BattleCameraFollow>();
    }

    private static void BuildGlobalLight()
    {
        var lightGo = new GameObject("Global Light 2D");
        var light2d = lightGo.AddComponent<Light2D>();
        light2d.lightType = Light2D.LightType.Global;
        light2d.intensity = 1f;
    }

    private static void BuildRoomFloors()
    {
        var gridGo = new GameObject("BattleGrid", typeof(Grid));
        var grid = gridGo.GetComponent<Grid>();
        grid.cellSize = new Vector3(1f, 0.5f, 1f);
        grid.cellLayout = GridLayout.CellLayout.Isometric;
        grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

        var tmGo = new GameObject("GroundTilemap", typeof(Tilemap), typeof(TilemapRenderer));
        tmGo.transform.SetParent(gridGo.transform, false);
        var tm = tmGo.GetComponent<Tilemap>();
        tm.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
        var tmr = tmGo.GetComponent<TilemapRenderer>();
        tmr.sortOrder = TilemapRenderer.SortOrder.TopRight;
        tmr.sortingOrder = -10;

        var groundTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/GroundTile.asset");
        if (groundTile == null)
        {
            Debug.LogWarning("[BattleSceneSetup] GroundTile missing; rooms will have no floor.");
            return;
        }

        PaintRect(tm, groundTile, Room0);
        PaintRect(tm, groundTile, Room1);
        PaintRect(tm, groundTile, Room2);
        // Doorway floor (cells inside the wall where the door sits).
        for (int y = WallA_DoorYMin; y <= WallA_DoorYMax; y++)
            tm.SetTile(new Vector3Int(WallA_Column, y, 0), groundTile);
        for (int x = WallB_DoorXMin; x <= WallB_DoorXMax; x++)
            tm.SetTile(new Vector3Int(x, WallB_Row, 0), groundTile);
    }

    private static void PaintRect(Tilemap tm, TileBase tile, RoomRect r)
    {
        for (int x = r.xMin; x <= r.xMax; x++)
            for (int y = r.yMin; y <= r.yMax; y++)
                tm.SetTile(new Vector3Int(x, y, 0), tile);
    }

    // --- walls + gates (two orientations) ---------------------------------------------------

    private static BattleGate BuildVerticalWallWithGate(Transform parent, Sprite sprite,
        int columnX, int yMin, int yMax, int doorYMin, int doorYMax, string gateName)
    {
        // Two segments: cells [yMin .. doorYMin-1] (lower) and [doorYMax+1 .. yMax] (upper).
        BuildWallSegmentAlongIsoY(parent, sprite, columnX, yMin, doorYMin - 1);
        BuildWallSegmentAlongIsoY(parent, sprite, columnX, doorYMax + 1, yMax);

        // Gate spans the door cells along iso-y.
        float doorMidY = (doorYMin + doorYMax) * 0.5f;
        Vector3 doorWorld = CellWorld(columnX, doorMidY);
        float doorLength = (doorYMax - doorYMin + 1) * CellStep;
        return BuildGate(parent, sprite, gateName, doorWorld, IsoYAngle(), doorLength);
    }

    private static BattleGate BuildHorizontalWallWithGate(Transform parent, Sprite sprite,
        int rowY, int xMin, int xMax, int doorXMin, int doorXMax, string gateName)
    {
        BuildWallSegmentAlongIsoX(parent, sprite, rowY, xMin, doorXMin - 1);
        BuildWallSegmentAlongIsoX(parent, sprite, rowY, doorXMax + 1, xMax);

        float doorMidX = (doorXMin + doorXMax) * 0.5f;
        Vector3 doorWorld = CellWorld(doorMidX, rowY);
        float doorLength = (doorXMax - doorXMin + 1) * CellStep;
        return BuildGate(parent, sprite, gateName, doorWorld, IsoXAngle(), doorLength);
    }

    private static void BuildWallSegmentAlongIsoY(Transform parent, Sprite sprite, int columnX, int yA, int yB)
    {
        if (yB < yA) return;
        int cells = yB - yA + 1;
        float midY = (yA + yB) * 0.5f;
        Vector3 mid = CellWorld(columnX, midY);
        float length = cells * CellStep;
        BuildWallVisual(parent, sprite, $"Wall_x{columnX}_y{yA}-{yB}", mid, IsoYAngle(), length);
    }

    private static void BuildWallSegmentAlongIsoX(Transform parent, Sprite sprite, int rowY, int xA, int xB)
    {
        if (xB < xA) return;
        int cells = xB - xA + 1;
        float midX = (xA + xB) * 0.5f;
        Vector3 mid = CellWorld(midX, rowY);
        float length = cells * CellStep;
        BuildWallVisual(parent, sprite, $"Wall_y{rowY}_x{xA}-{xB}", mid, IsoXAngle(), length);
    }

    /// One wall segment: a rotated 9-sliced gray bar (visible) + a thinner collider (precise).
    /// `length` is along the wall's local X (iso direction); the bar is 0.6 thick visually,
    /// 0.2 thick physically (so the collider stays clean while the visual reads as a real wall).
    private static void BuildWallVisual(Transform parent, Sprite sprite, string name, Vector3 worldPos,
        float angleDeg, float length)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        // Visual (sliced sprite so its size is exact and independent of the source PPU).
        var visualGo = new GameObject("Visual");
        visualGo.transform.SetParent(go.transform, false);
        var sr = visualGo.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(length, 0.55f);
        sr.color = new Color(0.42f, 0.44f, 0.48f); // stone-ish gray
        sr.sortingOrder = 2;

        // Darker "front face" of the wall, offset slightly down in world to fake iso height.
        var faceGo = new GameObject("Face");
        faceGo.transform.SetParent(go.transform, false);
        faceGo.transform.localPosition = new Vector3(0f, -0.18f, 0f); // local Y: perpendicular to wall
        var face = faceGo.AddComponent<SpriteRenderer>();
        face.sprite = sprite;
        face.drawMode = SpriteDrawMode.Sliced;
        face.size = new Vector2(length, 0.18f);
        face.color = new Color(0.22f, 0.24f, 0.28f);
        face.sortingOrder = 3;

        // Physical collider, thinner so the collider doesn't poke into the rooms.
        var box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(length, 0.2f);
    }

    /// Visible outer wall around every room, skipping the boundary that's already covered by
    /// an inter-room wall (so segments don't overlap).
    private static void BuildOuterWalls(Transform parent, Sprite sprite)
    {
        // Room 0 — east edge is the WallA shared with Room 1.
        BuildWallSegmentAlongIsoX(parent, sprite, Room0.yMax + 1, Room0.xMin, Room0.xMax);
        BuildWallSegmentAlongIsoX(parent, sprite, Room0.yMin - 1, Room0.xMin, Room0.xMax);
        BuildWallSegmentAlongIsoY(parent, sprite, Room0.xMin - 1, Room0.yMin, Room0.yMax);

        // Room 1 — west is WallA, north is WallB (shared with Room 2).
        BuildWallSegmentAlongIsoX(parent, sprite, Room1.yMin - 1, Room1.xMin, Room1.xMax);
        BuildWallSegmentAlongIsoY(parent, sprite, Room1.xMax + 1, Room1.yMin, Room1.yMax);

        // Room 2 — south is WallB.
        BuildWallSegmentAlongIsoX(parent, sprite, Room2.yMax + 1, Room2.xMin, Room2.xMax);
        BuildWallSegmentAlongIsoY(parent, sprite, Room2.xMin - 1, Room2.yMin, Room2.yMax);
        BuildWallSegmentAlongIsoY(parent, sprite, Room2.xMax + 1, Room2.yMin, Room2.yMax);
    }

    private static void BuildPillars(Transform parent, Sprite sprite)
    {
        foreach (Vector2Int[] room in PillarCellsPerRoom)
            foreach (Vector2Int cell in room)
                BuildPillar(parent, sprite, cell);
    }

    private static void BuildPillar(Transform parent, Sprite sprite, Vector2Int cell)
    {
        var go = new GameObject($"Pillar_{cell.x}_{cell.y}");
        go.transform.SetParent(parent, false);
        go.transform.position = CellWorld(cell);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(0.7f, 0.95f);
        sr.color = new Color(0.32f, 0.32f, 0.38f);
        sr.sortingOrder = 5; // share order with agents so iso Y-sort decides who's in front

        var box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.7f, 0.45f); // collider only on the base — easier to navigate iso-wise
    }

    private static BattleGate BuildGate(Transform parent, Sprite sprite, string name,
        Vector3 worldPos, float angleDeg, float length)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(length, 0.45f);
        sr.color = new Color(0.78f, 0.20f, 0.20f, 1f);
        sr.sortingOrder = 4;

        var box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(length, 0.45f);

        var gate = go.AddComponent<BattleGate>();
        var so = new SerializedObject(gate);
        so.FindProperty("sprite").objectReferenceValue = sr;
        so.FindProperty("blocker").objectReferenceValue = box;
        so.ApplyModifiedProperties();
        return gate;
    }

    private static GameObject BuildLeader(Vector3 worldPos, Sprite sprite)
    {
        var leader = new GameObject("Leader");
        leader.transform.position = worldPos;
        var sr = leader.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.95f, 0.9f, 0.45f);
        sr.sortingOrder = 6;
        leader.transform.localScale = Vector3.one * 0.8f;
        var rb = leader.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        leader.AddComponent<CircleCollider2D>().radius = 0.35f;
        leader.AddComponent<AvatarController>();
        leader.AddComponent<LeaderDash>(); // Shift to dash
        return leader;
    }

    private static Transform MakeAnchor(string name, Vector3 pos, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        return go.transform;
    }

    private static void BuildHud(TMP_FontAsset font, out GameObject canvasGo, out TextMeshProUGUI resultLabel, out GameObject returnGo)
    {
        canvasGo = new GameObject("BattleCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        var labelGo = new GameObject("ResultLabel", typeof(RectTransform));
        labelGo.transform.SetParent(canvasGo.transform, false);
        var lrt = labelGo.GetComponent<RectTransform>();
        lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 1f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.anchoredPosition = new Vector2(0f, -40f);
        lrt.sizeDelta = new Vector2(900f, 90f);
        resultLabel = labelGo.AddComponent<TextMeshProUGUI>();
        resultLabel.alignment = TextAlignmentOptions.Center;
        resultLabel.fontSize = 38;
        resultLabel.text = string.Empty;
        if (font != null) resultLabel.font = font;

        if (Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        returnGo = new GameObject("ReturnButton", typeof(RectTransform), typeof(Image), typeof(Button));
        returnGo.transform.SetParent(canvasGo.transform, false);
        var rrt = returnGo.GetComponent<RectTransform>();
        rrt.anchorMin = rrt.anchorMax = rrt.pivot = new Vector2(0.5f, 0f);
        rrt.anchoredPosition = new Vector2(0f, 40f);
        rrt.sizeDelta = new Vector2(260f, 64f);
        returnGo.GetComponent<Image>().color = new Color(0.22f, 0.34f, 0.50f, 1f);
        var rLabelGo = new GameObject("Label", typeof(RectTransform));
        rLabelGo.transform.SetParent(returnGo.transform, false);
        var rlrt = rLabelGo.GetComponent<RectTransform>();
        rlrt.anchorMin = Vector2.zero; rlrt.anchorMax = Vector2.one;
        rlrt.offsetMin = Vector2.zero; rlrt.offsetMax = Vector2.zero;
        var rLabel = rLabelGo.AddComponent<TextMeshProUGUI>();
        rLabel.text = "Return to Farm";
        rLabel.alignment = TextAlignmentOptions.Center;
        rLabel.fontSize = 24;
        if (font != null) rLabel.font = font;
        returnGo.SetActive(false);
    }

    // --- Operational UI (drag box / onion counter / targeting hint / squad HUD) -----------

    private static void BuildOperationalUi(GameObject canvasGo, TMP_FontAsset font,
        out RectTransform dragBox, out TMP_Text onionLabel, out TMP_Text freezeLabel,
        out GameObject targetingHint, out TMP_Text targetingHintLabel,
        out RectTransform squadHudParent, out RectTransform squadRowTemplate)
    {
        // Drag-select box (toggled on while dragging).
        var dragGo = new GameObject("DragBox", typeof(RectTransform), typeof(Image));
        dragGo.transform.SetParent(canvasGo.transform, false);
        dragBox = dragGo.GetComponent<RectTransform>();
        dragBox.anchorMin = dragBox.anchorMax = dragBox.pivot = Vector2.zero;
        dragBox.sizeDelta = Vector2.zero;
        var dragImg = dragGo.GetComponent<Image>();
        dragImg.color = new Color(0.5f, 1f, 0.5f, 0.22f);
        dragImg.raycastTarget = false;
        dragGo.SetActive(false);

        // Onion counter (top-right).
        onionLabel = BuildCornerLabel(canvasGo, font, "OnionCounter",
            "Rotten Onion: 3  [1]", new Vector2(-20f, -20f), new Color(1f, 0.92f, 0.55f));

        // Freeze counter (just below the onion counter).
        freezeLabel = BuildCornerLabel(canvasGo, font, "FreezeCounter",
            "Freeze Canister: 2  [2]", new Vector2(-20f, -64f), new Color(0.65f, 0.90f, 1f));

        // Targeting hint banner: a wrapper GameObject (toggled active) with a TMP child the
        // controller retexts per item.
        var thGo = new GameObject("TargetingHint", typeof(RectTransform));
        thGo.transform.SetParent(canvasGo.transform, false);
        var thRT = thGo.GetComponent<RectTransform>();
        thRT.anchorMin = thRT.anchorMax = new Vector2(0.5f, 1f);
        thRT.pivot = new Vector2(0.5f, 1f);
        thRT.anchoredPosition = new Vector2(0f, -110f);
        thRT.sizeDelta = new Vector2(900f, 36f);

        var thLabelGo = new GameObject("Label", typeof(RectTransform));
        thLabelGo.transform.SetParent(thGo.transform, false);
        var thLabelRT = thLabelGo.GetComponent<RectTransform>();
        thLabelRT.anchorMin = Vector2.zero; thLabelRT.anchorMax = Vector2.one;
        thLabelRT.offsetMin = thLabelRT.offsetMax = Vector2.zero;
        var thLabel = thLabelGo.AddComponent<TextMeshProUGUI>();
        thLabel.text = "Throwing — left-click to throw, Esc / right-click to cancel";
        thLabel.fontSize = 22;
        thLabel.color = new Color(1f, 0.85f, 0.4f);
        thLabel.alignment = TextAlignmentOptions.Center;
        if (font != null) thLabel.font = font;
        thGo.SetActive(false);
        targetingHint = thGo;
        targetingHintLabel = thLabel;

        // Squad HUD container (bottom-left, vertical stack).
        var shGo = new GameObject("SquadHud", typeof(RectTransform));
        shGo.transform.SetParent(canvasGo.transform, false);
        squadHudParent = shGo.GetComponent<RectTransform>();
        squadHudParent.anchorMin = squadHudParent.anchorMax = Vector2.zero;
        squadHudParent.pivot = Vector2.zero;
        squadHudParent.anchoredPosition = new Vector2(20f, 20f);
        squadHudParent.sizeDelta = new Vector2(260f, 220f);
        var vl = shGo.AddComponent<VerticalLayoutGroup>();
        vl.spacing = 4;
        vl.childControlWidth = true;
        vl.childControlHeight = true;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        // Squad row template (cloned per squad member at runtime; keep active here so you can
        // restyle in the editor — BattleCommandController.Awake disables it before cloning).
        var rowGo = new GameObject("SquadRowTemplate", typeof(RectTransform), typeof(Image));
        rowGo.transform.SetParent(shGo.transform, false);
        squadRowTemplate = rowGo.GetComponent<RectTransform>();
        var rowLe = rowGo.AddComponent<LayoutElement>();
        rowLe.minHeight = rowLe.preferredHeight = 38;
        rowLe.flexibleHeight = 0;
        rowGo.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.85f);

        var nameGo = new GameObject("Name", typeof(RectTransform));
        nameGo.transform.SetParent(rowGo.transform, false);
        var nameRT = nameGo.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 0f); nameRT.anchorMax = new Vector2(0.55f, 1f);
        nameRT.offsetMin = new Vector2(10f, 0f); nameRT.offsetMax = Vector2.zero;
        var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
        nameTmp.text = "Strain";
        nameTmp.fontSize = 16;
        nameTmp.color = Color.white;
        nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (font != null) nameTmp.font = font;

        var hpBgGo = new GameObject("HpBg", typeof(RectTransform), typeof(Image));
        hpBgGo.transform.SetParent(rowGo.transform, false);
        var hpBgRT = hpBgGo.GetComponent<RectTransform>();
        hpBgRT.anchorMin = new Vector2(0.55f, 0.28f); hpBgRT.anchorMax = new Vector2(1f, 0.72f);
        hpBgRT.offsetMin = new Vector2(4f, 0f); hpBgRT.offsetMax = new Vector2(-10f, 0f);
        hpBgGo.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.9f);

        var hpFillGo = new GameObject("HpFill", typeof(RectTransform), typeof(Image));
        hpFillGo.transform.SetParent(hpBgGo.transform, false);
        var hpFillRT = hpFillGo.GetComponent<RectTransform>();
        hpFillRT.anchorMin = Vector2.zero; hpFillRT.anchorMax = Vector2.one;
        hpFillRT.offsetMin = hpFillRT.offsetMax = Vector2.zero;
        hpFillRT.pivot = new Vector2(0f, 0.5f); // grow from the left edge
        hpFillGo.GetComponent<Image>().color = new Color(0.42f, 0.85f, 0.42f);
    }

    private static void BuildPauseMenu(GameObject canvasGo, TMP_FontAsset font,
        out GameObject pausePanel, out Button resumeBtn, out Button returnBtn)
    {
        // Wrapper that covers the screen with a dim backdrop; toggled on/off by BattlePauseMenu.
        pausePanel = new GameObject("PausePanel", typeof(RectTransform));
        pausePanel.transform.SetParent(canvasGo.transform, false);
        var prt = pausePanel.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = prt.offsetMax = Vector2.zero;

        var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        dimGo.transform.SetParent(pausePanel.transform, false);
        var drt = dimGo.GetComponent<RectTransform>();
        drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one;
        drt.offsetMin = drt.offsetMax = Vector2.zero;
        dimGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        var dialogGo = new GameObject("Dialog", typeof(RectTransform), typeof(Image));
        dialogGo.transform.SetParent(pausePanel.transform, false);
        var drt2 = dialogGo.GetComponent<RectTransform>();
        drt2.anchorMin = drt2.anchorMax = drt2.pivot = new Vector2(0.5f, 0.5f);
        drt2.sizeDelta = new Vector2(360f, 220f);
        dialogGo.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.18f, 0.98f);

        var titleGo = new GameObject("Title", typeof(RectTransform));
        titleGo.transform.SetParent(dialogGo.transform, false);
        var trt = titleGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 0.65f); trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(16f, 0f); trt.offsetMax = new Vector2(-16f, -12f);
        var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "Paused";
        titleTmp.fontSize = 30;
        titleTmp.color = new Color(1f, 0.95f, 0.75f);
        titleTmp.alignment = TextAlignmentOptions.Center;
        if (font != null) titleTmp.font = font;

        resumeBtn = BuildPauseButton(dialogGo.transform, font, "Resume",
            new Vector2(0.5f, 0.40f), new Color(0.22f, 0.40f, 0.22f));
        returnBtn = BuildPauseButton(dialogGo.transform, font, "Return to Farm",
            new Vector2(0.5f, 0.12f), new Color(0.30f, 0.20f, 0.20f));

        pausePanel.SetActive(false);
    }

    private static Button BuildPauseButton(Transform parent, TMP_FontAsset font, string label,
        Vector2 anchor, Color color)
    {
        var go = new GameObject(label + "Btn", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.sizeDelta = new Vector2(240f, 50f);
        go.GetComponent<Image>().color = color;
        var btn = go.GetComponent<Button>();

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(go.transform, false);
        var lrt = labelGo.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;

        return btn;
    }

    private static TMP_Text BuildCornerLabel(GameObject canvasGo, TMP_FontAsset font, string name,
        string text, Vector2 anchoredPos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(canvasGo.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(280f, 40f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 22;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Right;
        if (font != null) tmp.font = font;
        return tmp;
    }

    // --- BattleManager wiring ---------------------------------------------------------------

    private static void SetSquad(SerializedObject so, params ZombieData[] strains)
    {
        SerializedProperty list = so.FindProperty("testSquad");
        var valid = new List<ZombieData>();
        foreach (ZombieData z in strains) if (z != null) valid.Add(z);
        list.arraySize = valid.Count;
        for (int i = 0; i < valid.Count; i++)
            list.GetArrayElementAtIndex(i).objectReferenceValue = valid[i];
    }

    private static void SetEnemiesFallback(SerializedObject so, ZombieData enemy, int count)
    {
        SerializedProperty list = so.FindProperty("testEnemies");
        if (enemy == null) { list.arraySize = 0; return; }
        list.arraySize = 1;
        SerializedProperty el = list.GetArrayElementAtIndex(0);
        el.FindPropertyRelative("zombie").objectReferenceValue = enemy;
        el.FindPropertyRelative("count").intValue = count;
    }

    private static void SetRooms(SerializedObject so, Transform squadSpawn, Transform[] enemySpawns, BattleGate[] gates)
    {
        SerializedProperty list = so.FindProperty("rooms");
        list.arraySize = enemySpawns.Length;
        for (int i = 0; i < enemySpawns.Length; i++)
        {
            SerializedProperty el = list.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("squadSpawn").objectReferenceValue = i == 0 ? squadSpawn : null;
            el.FindPropertyRelative("enemySpawn").objectReferenceValue = enemySpawns[i];
            el.FindPropertyRelative("entranceGate").objectReferenceValue = (i > 0 && i - 1 < gates.Length) ? gates[i - 1] : null;
        }
    }

    // --- asset lookups ----------------------------------------------------------------------

    private static ZombieData LoadStrain(string displayName)
    {
        var z = AssetDatabase.LoadAssetAtPath<ZombieData>($"{StrainFolder}/Zombie_{displayName}.asset");
        if (z == null) Debug.LogWarning($"[BattleSceneSetup] Missing {displayName} — run Setup Zombie Strains first.");
        return z;
    }

    private static ZombieData ResolveEnemy()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:ZombieData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string n = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (n.Contains("wild")) return AssetDatabase.LoadAssetAtPath<ZombieData>(path);
        }
        return LoadStrain("Runner");
    }

    private static MissionData LoadMission()
    {
        string[] guids = AssetDatabase.FindAssets("t:MissionData");
        string chosen = null;
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            if (chosen == null) chosen = path;
            if (Path.GetFileNameWithoutExtension(path).ToLowerInvariant().Contains("city1")) { chosen = path; break; }
        }
        return chosen != null ? AssetDatabase.LoadAssetAtPath<MissionData>(chosen) : null;
    }

    private static void EnsureInBuildSettings(string path)
    {
        if (!File.Exists(path)) return;
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene s in scenes) if (s.path == path) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
