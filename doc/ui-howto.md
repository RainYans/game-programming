# 怪物农场 UI 自助手册

> 目标：让你自己在 Unity 编辑器里改 HUD / 面板 / 商店，不用等我盲调。
> 编辑器里改完**直接按 Play 就能实时看到**，不需要截图。

---

## 0. 一句话原则（先记这个）

| 东西 | 怎么改 |
|---|---|
| HUD、面板的框/标题/按钮、商店外壳 | **编辑器里选中物体 → 改 Inspector**，按 Play 看 |
| 商店的**商品卡片** | 卡片是 `ShopPanelUI.cs` **运行时用代码生成**的 → 要改外观**只能改这个脚本**（只改外观，别碰 `shop.Buy` 那几行逻辑） |
| 数字（金币/怪物/种子数） | 是 `WalletCounterUI` 等脚本**运行时**填的 → 编辑器里是占位，**按 Play 才显示真实值** |

---

## 1. 资源在哪（要拖图就来这里找）

根目录：`Assets/Art/CuteFantasy/Packs/Cute_Fantasy_UI/UI/`

| 文件 | 内容 | 我已经切好、可直接拖的 sprite 名字 |
|---|---|---|
| `UI_Frames.png` | 面板/卡片底框 | `cf_panel`(奶白金边·主面板)、`cf_card`(奶白圆角·卡片)、`cf_solid`(奶白实心小块)、`cf_scallop`(花边)、`cf_bolt`(铆钉角)、`cf_grey`(灰·禁用)、`cf_brown`(深棕·HUD木牌) |
| `UI_Buttons.png` | 按钮底 | `cf_btn_cream`、`cf_btn_gold`、`cf_btn_tan`、`cf_btn_green`、`cf_btn_blue`、`cf_btn_red` |
| `UI_Ribbons.png` | 卷轴绶带（标题用） | `cf_ribbon`、`cf_ribbon2` |
| `UI_Bars.png` | 血条/进度条 | （还没切，需要时按第 3 节自己切） |
| `UI_Pop_Up.png` | 弹窗气泡框 | （还没切） |
| `UI_Icons.png` | 一堆小图标（金币/心/星/盾…） | （整张未切，我只单独抠了金币，见下面 UIIcons） |

游戏内图标（物品/资源/食物/工具，16px 网格，描边版更清楚）：
`Assets/Art/CuteFantasy/Packs/Cute_Fantasy/Icons/Outline/`
- `Resources_Icons_Outline.png` 矿石/宝石/木头/种子
- `Food_Icons_Outline.png` 蔬果/肉/鱼（**胡萝卜在第4行第2格**）
- `Other_Icons_Outline.png` 花
- `Tool_Icons_Outline.png` 工具

我给 HUD 抠好的独立图标（已是单张 sprite，直接拖）：
`Assets/Art/UIIcons/` → `icon_coin`(金币)、`icon_seed`(胡萝卜)、`icon_monster`(史莱姆)

怪物本体：`Assets/Art/CuteFantasy/Packs/Cute_Fantasy/Enemies/Slime/...`
字体：`Assets/Fonts/CuteFantasyPixel.asset`（在字体下拉里叫 **CuteFantasyPixel SDF**）

---

## 2. 像素图导入设置（铁律，糊就是这里没设对）

选中任意 `.png` → Inspector 顶部：
- **Texture Type** = `Sprite (2D and UI)`
- **Sprite Mode** = `Single`（单图标）或 `Multiple`（一张表里很多图，要切）
- **Pixels Per Unit** = `16`
- **Filter Mode** = `Point (no filter)` ← 不设这个就糊
- **Compression** = `None`
- 改完点底部 **Apply**

---

## 3. 怎么把一张大图切成多个 sprite（Sprite Editor）

1. 选 png → Sprite Mode 设 `Multiple` → Apply
2. 点 Inspector 里 **Sprite Editor** 按钮
3. 左上 **Slice**：
   - 整齐网格：`Type = Grid By Cell Size`，填 `16 × 16` → Slice
   - 不整齐：`Automatic`，或手动框选
4. 点中某一格可以在右下角**给它命名**（拖的时候认名字）
5. 右上 **Apply**

### 9-slice（让框能拉大而四角不变形）
在 Sprite Editor 里选中那个框，会看到**绿色的边线**，把上下左右四条绿线拖到「角」和「可拉伸中间」的分界处 → Apply。
之后在 Image 组件把 **Image Type 设成 `Sliced`** 才会生效。

---

## 4. 框/按钮拉大后边角太粗或太细？调 PUM

9-slice 的边角厚度公式：

```
屏幕上边角厚度(px) = 边框像素 × 6.25 ÷ (Image 的 Pixels Per Unit Multiplier)
```

> 6.25 是固定的（sprite PPU=16，Canvas 的 Reference Pixels Per Unit=100，100÷16=6.25）

- 在 **Image 组件 → Pixels Per Unit Multiplier** 改这个数。
- **数越大 → 边角越细**；数越小 → 边角越粗。
- 圆角招牌/面板手感：`1.5 ~ 2.1` 之间试。HUD 木牌我用的 **1.5**。

---

## 5. HUD 现在的结构（`Farm.unity` → Canvas → HUD）

```
Canvas
├─ HUD
│  ├─ TitlePlate              Image=cf_ribbon   → "MONSTER FARM" 卷轴
│  │   └─ Title               TMP 文本
│  └─ ResourcePlaque          Image=cf_brown (Sliced, PUM 1.5)  ← 深棕木牌
│      ├─ Icon_ResourceDisplay   Image=icon_coin
│      ├─ ResourceDisplay        TMP + WalletCounterUI     → 金币数
│      ├─ Icon_InventoryDisplay  Image=icon_monster
│      ├─ InventoryDisplay       TMP + InventoryCounterUI  → 怪物数
│      ├─ Icon_SeedDisplay       Image=icon_seed
│      └─ SeedDisplay            TMP + SeedCounterUI       → 种子数
└─ BottomBar
   ├─ ShopBtn   Image=cf_btn_cream
   └─ BattleBtn Image=cf_btn_gold
```

### 想改什么 → 选谁

- **整个木牌大小/位置**：选 `ResourcePlaque` → RectTransform 改 `Pos X/Y`、`Width/Height`。
  换底图就把别的 `cf_xxx` 拖到 Image 的 Source Image。
- **图标大小/位置**：选 `Icon_*` → RectTransform 的 `Width/Height`（现在 coin/seed=48，monster=44）和 `Pos X/Y`。
  换图标：把 `Assets/Art/UIIcons/` 里别的图拖到 Source Image。
- **数字字体/字号/颜色/对齐**：选 `ResourceDisplay`(等) → TextMeshPro 组件：
  - `Font Asset` = CuteFantasyPixel SDF
  - `Font Size`、`Vertex Color`、`Alignment`
- **去掉 "Resources:" 这种前缀**：选同一个物体上的 `WalletCounterUI`(等脚本) → 把 **Prefix** 字段清空（已清空；想加文字就填这里）。
- **三个数字只有 Play 时才出现真实值**，编辑器里是占位/旧值，正常。

### RectTransform 定位小技巧
- 左上角那个**方块图标**=锚点预设。点开后**按住 Alt** 再点某个对齐方式 = 连位置一起对齐。
- HUD 元素都用「左上」锚点，这样不同分辨率下都贴在左上角。

---

## 6. 商店现在的结构 + 怎么改

```
Canvas → ShopPage (平时关着)
└─ Frame                Image=cf_panel  (奶白金边主面板)
   ├─ Title             TMP  "SHOP"
   ├─ ItemGrid          GridLayoutGroup ← 卡片塞在这里(运行时生成)
   └─ CloseBtn          Image=cf_btn_red  "X"
```

### 编辑器里能直接调的
- **面板底图/大小**：选 `Frame` → Image 换 sprite / RectTransform 改大小、PUM。
- **标题**：选 `Title` → 字体换 CuteFantasyPixel SDF、字号、颜色。（想要卷轴标题就在 Frame 下加一个 Image=cf_ribbon 当标题底，再把 Title 放它上面。）
- **关闭按钮**：选 `CloseBtn` → Source Image、大小；**Button 组件 Normal Color 必须是白色**（见第 8 节）。
- **卡片排布**：选 `ItemGrid` → GridLayoutGroup 改 `Cell Size`(卡片大小)、`Spacing`(间距)、`Constraint`(几列)。

### 卡片外观必须改代码（`Assets/Scripts/ShopPanelUI.cs`）
卡片在 `BuildCard()` 里用代码 new 出来，全是写死的深色、没用像素字。**只改外观那几行，别动 `onClick` / `shop.Buy` / `shop.BuyItem`**：

- `bg.color = new Color(0.08f,0.10f,0.16f,1f);`（第 107 行）
  → 想用奶白卡片：给 `bg` 设 sprite 和 Sliced。例：
  ```csharp
  bg.sprite = Resources/AssetDatabase 拿到的 cf_card;
  bg.type = Image.Type.Sliced;
  bg.color = Color.white;
  ```
- `TMP_FontAsset bodyFont = TMP_Settings.defaultFontAsset;`（第 96 行）
  → 换成像素字：加载 `Assets/Fonts/CuteFantasyPixel.asset` 赋给 bodyFont/titleFont。
- BUY 按钮颜色（第 171-178 行）→ 想用 `cf_btn_green` sprite 就给 `btnImg.sprite` 设 Sliced，并把 `cb.normalColor = Color.white`。
- 价格文字 `$"{price} res"`（第 141 行）、`"Owned: {owned}"`（第 219 行）→ 想配金币图标得加一个 Image，稍微多写几行。

> 这部分要写代码，比较绕。**如果你不想碰代码，跟我说一声，我把 `BuildCard()` 的外观部分按之前那张商店预览图改好**（保证不动买卖逻辑）。其余编辑器能拖的你自己来。

---

## 7. 从零拼一个新面板（手把手）

1. Hierarchy 里右键 `Canvas` → **UI → Image**，改名 `Frame`。
2. 选 `Frame` → Image 组件：`Source Image` 拖 `cf_panel`；`Image Type` = `Sliced`；`Pixels Per Unit Multiplier` ≈ 2。
3. RectTransform：锚点选「居中」，设 `Width/Height`（比如 940×580）。
4. **标题绶带**：右键 `Frame` → UI → Image，Source = `cf_ribbon`，Type=Sliced，摆到面板顶部；再右键它 → UI → **Text - TextMeshPro**，Font = CuteFantasyPixel SDF，文字居中。
5. **关闭按钮**：右键 `Frame` → UI → **Button - TextMeshPro**，Source Image=`cf_btn_red`，**Button 组件 Normal Color 设白色**，标签字体换像素字。
6. 里面要放的内容（卡片/列表）按需加 Image / Text / Button，或用 `Vertical/Horizontal Layout Group`、`Grid Layout Group` 自动排版。
7. **Ctrl+S 存场景**。要平时隐藏就把 `Frame` 取消勾选(SetActive false)，由对应脚本打开。

---

## 8. 常见坑（踩了来这查）

| 现象 | 原因 / 解决 |
|---|---|
| 按钮颜色发暗、发灰 | Button 的 **Normal Color 不是白**。Button 组件 → Normal Color 设纯白 `FFFFFF`，Highlighted/Pressed 也调亮。（ColorTint 会把 sprite 颜色乘上去） |
| 图标/字糊成一团 | 该 png 没设 `Filter=Point` + `Compression=None`（第 2 节） |
| 框拉大后四角被拉变形 | Image Type 要 `Sliced`，且该 sprite 在 Sprite Editor 里设过绿色 border |
| 字显示成方块/缺字 | Font Asset 选 `CuteFantasyPixel SDF`；个别字缺会回退到默认字体 |
| 改完看不到效果 | counter 类数字**按 Play 才填**；面板平时是关着的，要在 Hierarchy 勾上才看得到 |
| 拖 sprite 时找不到切好的小块 | 该 png 的 Sprite Mode 要是 `Multiple` 且 Apply 过；展开 png 左边的小三角能看到子 sprite |

---

## 9. 想让我先出"效果图"再动手？

我可以用 Python 把真实素材拼成**高保真预览图**（就是之前商店那张），你看满意了我们再照着搭，避免白搭。需要就说"先出预览图"。
