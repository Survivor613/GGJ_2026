# 对话系统说明（面向关卡策划 & 程序）

本系统为 Unity 对话系统（原生 Text 版本），按列表顺序播放，不再使用 id/nextId 跳转。
角色显示逻辑以 **speakerName** 为主（由显示名映射到角色定义）。

## 1. 给关卡策划的使用说明

### 1.1 用 CSV 导入对话（推荐）
准备 CSV（UTF-8 或 GBK 均可），两列即可：

```
speaker,text
,200X年，从齐海大学毕业3年后，我再次回到这条曾经让我无比厌恶的街道。
陌生的客人,你这孩子，发生什么呆？这小女孩你拿钱呢！
祖母,唉，我还是不允许你留在这里……
我,奶奶，那你让我来做什么？
```

- `speaker` 为空 -> 自动当作旁白
- `text` 为正文

导入步骤：
1. 将 CSV 拖入 Unity（例如 `Assets/Dialogue/CSV/`）
2. 菜单 `Tools → Dialogue → Import CSV To DialogueScript`
3. 选择 CSV 文件，点击导入
4. 生成 `DialogueScriptSO` 资产（默认 `Assets/Resources/Dialogue_FromCsv.asset`）

### 1.2 在场景中播放
把 `DialogueScriptSO` 拖给场景里 `DialogueTest` 的 `testScript` 字段即可测试。

### 1.3 角色立绘配置（策划需要知道）
1. 创建 `ActorDefinitionSO`（Create → Dialogue → Actor）
2. 设置：
   - `displayName`（必须与 CSV 的 speaker 完全一致）
   - `defaultSlot`（Left/Right/Center）
   - `defaultPortraitKey`（默认立绘 key）
3. `Portraits` 列表里添加 key 与 sprite
4. 将该 ActorDefinition 加入 `ActorController.actorDefinitions`

### 1.4 文本标签
支持的标记：
- `[pause=0.5]` 停顿
- `[spd=0.05]` 改变打字速度
- `[sfx=xxx]` 播放音效
- `<color=#FF0000>红色</color>` 富文本颜色

不支持（原生 Text 版本）：
- `[shake]`、`[wave]` 字符级动画

---

## 2. 给程序同事的结构说明

### 2.1 核心流程
播放逻辑按 `DialogueScriptSO.nodes` 列表顺序执行：
1. `DialogueRunner.StartDialogue()` -> 从 index 0 开始
2. `LineNode`：显示文本 + 打字机 -> 等待输入
3. `CommandNode`：执行命令 -> 继续下一条

### 2.2 主要脚本
核心控制：
- `DialogueRunner`（顺序播放，忽略 id/nextId）
- `DialogueParser`（解析文本标记）

UI：
- `DialogueViewUniversal`（原生 Text）
- `TypewriterEffectUniversal`（打字机）
- `HistoryView` / `HistoryEntryView`（历史）

Actor：
- `ActorController` / `ActorView`（立绘管理）

编辑工具：
- `DialogueCsvImporter`（CSV 导入）
- `DialogueScriptSOEditor`（右键插入节点）

### 2.3 支持的命令
在 `CommandNode.command` 中使用（支持 name 显示名）：

```
actor show name=祖母 portrait=normal x=-200 y=0
actor hide name=祖母
actor focus name=祖母
wait 0.5
scene load name=YourScene
scene reload
```

---

## 3. 常见问题

### Q: CSV 导入乱码
解决：在导入面板中选择正确编码（UTF-8/GBK）。

### Q: 文本显示“口口口”
确保使用原生 Text，并给 NameText/BodyText 设置支持中文的字体（如思源黑体）。

### Q: 历史记录没有显示角色名
确保 CSV 的 speaker 非空，且与 ActorDefinition 的 displayName 完全一致。

---

## 4. 最佳实践建议

- 大段文本建议使用 CSV 管理，策划维护效率高
- 对话顺序靠节点顺序控制
- 若需要分支/跳转，后续可扩展命令系统

