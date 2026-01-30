# 对话系统测试指南

> 🎉 **新功能**：现在支持**一键自动搭建**，无需手动拖拽组件！
> 
> 使用菜单：`Tools → Dialogue → Auto Setup Scene (一键搭建) ⚡`

## ⚡ 超快速开始（一键搭建）

### 方式 A：全自动（推荐！）

只需两步：

**步骤 1：创建测试数据**
```
Tools → Dialogue → Create Test Data
```

**步骤 2：一键搭建场景**
```
Tools → Dialogue → Auto Setup Scene (一键搭建) ⚡
```

✅ 完成！直接按 **Play** 就能看到对话效果！

---

### 方式 B：手动搭建（如果你想自定义）

#### 1. 创建测试数据
在 Unity 顶部菜单栏选择：
```
Tools → Dialogue → Create Test Data
```
这将自动创建：
- `Resources/TestDialogue.asset`（测试对话脚本）
- `Resources/Actor_Alice.asset`（测试角色定义）

⚠️ **注意**：创建后请在 Inspector 中为 `Actor_Alice` 添加立绘 Sprite（如果需要显示角色图像）。

---

#### 2. 场景搭建

#### A. UI 层级结构
在 Hierarchy 中创建以下结构：

```
Canvas
├── DialoguePanel (Image - 对话框背景)
│   ├── NameText (TextMeshPro - Text)
│   ├── BodyText (TextMeshPro - Text) 
│   │   ├── [挂载] TypewriterEffect
│   │   └── [挂载] TextEffectController
│   └── ContinueIcon (Image - 提示图标，可选)
├── ActorLayer (空物体 - 放置角色立绘)
└── HistoryPanel (隐藏)
    └── ScrollView
        └── Content (挂载 VerticalLayoutGroup)
```

#### B. 脚本挂载

##### DialoguePanel 物体
- 添加 `DialogueView` 组件
- 拖拽引用：
  - Panel → DialoguePanel 自身
  - Name Text → NameText
  - Body Text → BodyText
  - Continue Icon → ContinueIcon
  - Typewriter → BodyText 上的 TypewriterEffect

##### DialogueSystem 物体（新建空物体）
- 添加 `DialogueRunner` 组件
- 添加 `ActorController` 组件
- 添加 `DialogueInputHandler` 组件
- 添加 `DialogueTest` 组件

拖拽引用：
- DialogueRunner:
  - Dialogue View → DialoguePanel 的 DialogueView
  - Actor Controller → 同物体的 ActorController
  - History View → HistoryPanel 的 HistoryView（如果有）
- ActorController:
  - Actor Prefab → 创建一个包含 Image + ActorView 的 Prefab
  - Actor Layer → Canvas/ActorLayer
  - Actor Definitions → 拖入 `Actor_Alice.asset`
- DialogueTest:
  - Test Script → `TestDialogue.asset`

---

#### 3. 运行测试（手动搭建方式）

点击 Unity 的 **Play** 按钮，对话会自动启动。

#### 控制方式：
- **鼠标左键/空格键**：推进对话或跳过打字机
- **H 键**：打开/关闭历史面板
- **R 键**：重新开始对话
- 左上角调试 GUI 也提供按钮控制

---

## 对话脚本标记语法

在对话文本中可以使用以下标记：

| 标记 | 说明 | 示例 |
|------|------|------|
| `[pause=0.5]` | 停顿 0.5 秒 | `你好[pause=1]欢迎` |
| `[spd=0.01]` | 设置打字速度 | `[spd=0.1]说得很慢` |
| `[shake=2]...[/shake]` | 文字抖动 | `[shake=1]危险！[/shake]` |
| `[wave=3]...[/wave]` | 文字波浪 | `[wave=2]~哈哈~[/wave]` |
| `[sfx=sound_name]` | 播放音效 | `[sfx=button_click]点击` |

---

## 命令节点示例

在 `DialogueScriptSO` 的 `nodes` 中添加 `CommandNode`：

```
command: "actor show id=alice portrait=smile x=-300 y=0"
command: "actor hide id=alice"
command: "actor focus id=alice"
command: "wait 1.5"
```

---

## 故障排查

### 问题：脚本无法添加到物体上
- 确认 Console 中没有红色编译错误
- 删除 `Library` 文件夹并重启 Unity

### 问题：对话不显示
- 检查 DialogueRunner 的所有引用是否正确拖拽
- 确认 DialoguePanel 的 Canvas 在场景中激活

### 问题：角色立绘不显示
- 确认 ActorController 的 Actor Prefab 已设置
- 确认 Actor Definitions 中已添加对应角色
- 检查命令中的 `id` 是否与 ActorDefinitionSO 的 `actorId` 匹配

---

## 下一步

测试通过后，你可以：
1. 创建更多的 `DialogueScriptSO` 作为剧情对话
2. 为不同角色创建 `ActorDefinitionSO`
3. 自定义 DialoguePanel 的 UI 样式
4. 扩展 `DialogueRunner` 的命令系统（背景、音乐等）

祝你开发顺利！🎮
