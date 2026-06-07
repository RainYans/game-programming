# 重建提示词（粘贴进新窗口即可）

> 把下面「PROMPT」整段粘到新会话。它配合 `doc/design/direction.md`（方向准绳）和
> `doc/next-action.md`（进度+技术备忘）使用。

---

## PROMPT

继续做我的 Unity 游戏 **「怪物农场」**（Unity 2022.3 LTS, URP 2D；项目在
`F:\unity_repo\Yanshuo\game-programming\ZombieFarm`）。

**先读这两份再动手**：`doc/design/direction.md`（美术/视图/战斗方向的唯一准绳）、
`doc/next-action.md`（最新进度 + 末尾的技术 gotchas）。

**协作方式**：MCP 已接通、已授权直接驱动 Unity 编辑器（命令行 `unity-mcp-cli run-tool <tool>
--input-file -`）。**所有场景/UI 内容都在编辑器里做成持久化对象、存进 `.unity`，绝不要用运行时
脚本在 Play 时生成场景。** 中文沟通、英文代码。每个 chunk 先按 **M(最小)/T(目标)/P(打磨)** 报三档
验收、等我选档再写代码。

### 现状（已完成，别推翻）
- **美术锁定 = Cute Fantasy（Kenmi）明亮 16px 俯视像素**，全套已拷入
  `Assets/Art/CuteFantasy/Packs/`（983 张，按包分子文件夹：`Cute_Fantasy` 主包 +
  `Cute_Fantasy_Characters`/`_Dungeons`/`_UI`/`_MilitaryCamp`/`_Desert`/`_Volcano`/`_ShroomLands`/
  节日包）。源在 `F:\unity_repo\Yanshuo\像素明亮`。音频从 Ninja Adventure
  (`F:\unity_repo\Yanshuo\像素风\...\Audio`) 借（CF 无音频）。视觉只用 CF，别混 Ninja。
- **代码逻辑全部保留、不要改**：ScriptableObject（`ZombieData`/`CropData`/`MissionData`）、
  `GameConfig`、`SaveManager`/`SaveData`、`BattleAgent` 战斗与被动、`CropInstance` 成长、饥饿
  (`ZombieUnit`)、`Wallet`/`Inventory`/`SeedInventory`/`ItemInventory`、`ShopController`、
  `GridManager`、`FarmActions`、`AvatarController`/`AvatarInteraction` 等。**"僵尸→怪物"只是显示层**
  （strain id 如 brute/mauler/… 与存档结构不变）。
- 视图已从等距翻成**俯视正交**（`isoYScale` 已去掉，编译通过）。

### 任务：完全重建场景 + UI（在保留逻辑之上重新接线）
旧场景只是"能种田"的素颜演示，**全部弃用重建**，做到 Cute Fantasy 风格、像 RPG 示例地图那样
**有布局、有装饰**。

**阶段 0 — 先确认/补齐素材**
- 确认 `Assets/Art/CuteFantasy/Packs` 的像素导入参数都生效（**FilterMode=Point、PPU=16、
  Uncompressed**）；没生效就批量补设。
- **按各资产帧尺寸切片**（地块 16；角色/多数敌人 ~32；大史莱姆 64；哥布林 48；动物 ~32；
  建筑/装饰多为整图或按物件切，不要无脑 16 切）。Tiles 文件夹 16×16 网格切。

**阶段 1 — 重建 Farm 场景**（俯视方格）
- 用 CF 地块铺草地/路/水/农田；CF `Buildings` 摆 **Home/Shop/Lab/WarCamp**（各带碰撞体支持走上去
  按 E 交互）；CF 树/花/栅栏装饰出层次。
- 玩家 = CF `Player`；田地块用 `FieldTile`（保持 `GridManager.IsFarmCell` 判定）。
- 巡场怪/作物成长 = CF 怪（**史莱姆 小→中→大 当 3 个成长阶段**；不同品系用不同种类，造型各异）。
- 重接现有逻辑到新对象：`GridManager`、`FarmActions`、`AvatarController`、`AvatarInteraction`、
  `FarmRoamerSpawner`、`UIManager`、相机（正交 + PixelPerfectCamera，PPU16）。

**阶段 2 — 重建 Battle 场景**（清村）
- 把 `Assets/Editor/BattleSceneSetup.cs` 的等距投影（`CellWorld`、`IsoXAngle`/`IsoYAngle`、
  Grid Isometric）改成**正交俯视**；用 CF `Dungeons`/`MilitaryCamp`/生物群系地块搭**村庄/地牢清剿**
  地图，多个 level。
- 敌人用 CF 怪/哥布林/兽人/骷髅（Boss 用大史莱姆/生物群系怪放大）。
- 重接 `BattleManager`/`BattleAgent`/`BattleCommandController`/`DeployPanel`/`BattleResultApplier`。

**阶段 3 — 重建 UI**
- 用 `Cute_Fantasy_UI`（`UI_Frames`/`UI_Bars`/`UI_Buttons`/`UI_Icons`/`UI_Pop_Up`/`UI_Ribbons` +
  字体）重做 HUD（货币 / 小队+饥饿 / 任务提示）和面板（种子选择 / Shop / Lab / Deploy / 战斗准备 /
  结算）。重接到现有 `UIManager`/`ShopPanelUI`/`DeployPanel`/`SeedPickPopup` 等。

### 验收标准（之前的功能要全部跑通）
1. **走位**：WASD 控 CF 玩家在俯视农场走动，撞建筑/边界停下。
2. **种植循环**：走到田块 E → 选种子 → 种下 → 3 阶段成长 → 收获 → 怪物巡场。
3. **饥饿**：怪物随时间 饱↔饿，且影响战斗强度。
4. **经济**：Shop 能买种子 + ≥1 种战斗道具；单一货币 `Wallet` 正确增减；Lab 入口在。
5. **出征**：WarCamp 打开 Deploy 面板，选关 + 组队(squadCap) + 进入 Battle 场景。
6. **战斗**：俯视小队走位 + 自动攻击 + 框选/右键指挥 + 道具；**清光怪物=胜利**；阵亡=永久损失；
   胜/负结算 + 奖励，返回农场生效。
7. **存档**：整局状态存读正确（`SaveData` 结构不变，旧档可读）。
8. **美术**：全程 Cute Fantasy 单一像素风 + 俯视 + Point 清晰 + 协调；农场/战斗**有布局有装饰、不素颜**。
9. **持久化**：场景内容都是编辑器里建好存进 `.unity` 的对象，非运行时生成。

### 技术 gotchas（见 next-action.md 末尾）
- 场景 Grid 可能有 transform 偏移 → 摆位/相机用 `grid.GetCellCenterWorld(cell)`，别用字面世界坐标。
- `PixelPerfectCamera` 类型在程序集 **`Unity.2D.PixelPerfect`**（不在 URP runtime）。
- 截图用 `screenshot-camera`（`screenshot-game-view` 在编辑模式可能返回旧缓存帧）；为干净截图可临时
  关 `CinemachineBrain`，**截完务必恢复**。
- 改动网格/坐标后，务必 **Play 实测** 走路/种植/进建筑/战斗，确认逻辑没断。

（先从阶段 0 + 阶段 1 Farm 重建做起，做完截图给我看，按 M/T/P 报验收档位等我确认。）
