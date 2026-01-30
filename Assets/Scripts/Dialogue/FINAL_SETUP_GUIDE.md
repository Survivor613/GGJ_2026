# 对话系统最终设置指南（原生Text版本）

> ⚠️ **重要**：此指南适用于使用 Unity 原生 Text 的版本，完美支持中文！

---

## 🎯 需要用到的工具（仅4个）

在 Unity 顶部菜单 `Tools → Dialogue` 中：

1. **Create Test Data** - 创建测试数据
2. **Auto Setup Scene** - 一键搭建场景
3. **Convert to Unity Text** - 转换到原生Text
4. **Switch to Chinese (Unity Text)** - 切换中文模式

---

## 📋 完整设置步骤（按顺序执行）

### 步骤 1：清理旧场景（如果有的话）
在 Hierarchy 中删除：
- `DialogueSystem` 物体
- `Canvas` 下的 `DialoguePanel`、`ActorLayer`、`HistoryPanel`

### 步骤 2：创建测试数据
```
Tools → Dialogue → Create Test Data
```
✅ 会创建测试对话脚本和角色定义

### 步骤 3：一键搭建场景
```
Tools → Dialogue → Auto Setup Scene (一键搭建) ⚡
```
✅ 自动创建所有 UI 和组件

### 步骤 4：转换到原生 Text
```
Tools → Dialogue → Convert to Unity Text (原生Text，完美中文) 📝
```
✅ 将 TextMeshPro 替换为原生 Text（完美支持中文）

### 步骤 5：切换到中文模式
```
Tools → Dialogue → Switch to Chinese (Unity Text) 🇨🇳
```
✅ 加载中文测试数据

### 步骤 6：测试运行
点击 Unity 的 **Play ▶️** 按钮！

---

## ✨ 应该看到的效果

- ✅ 清晰的中文对话（不再是口口口或模糊）
- ✅ 打字机效果
- ✅ 对话自动开始
- ✅ 点击屏幕或按空格推进对话
- ✅ 按 H 键打开历史面板
- ✅ 按 R 键重新开始

---

## 🎮 测试对话内容

```
爱丽丝: 你好！欢迎来到对话系统测试。
爱丽丝: 我可以说话很快！或者说得很慢...
旁白: 这是一段旁白文字，没有角色高亮。
```

---

## ❓ 常见问题

### Q: 还是显示口口口？
A: 确保已执行步骤 4（转换到原生 Text）

### Q: 点击没反应？
A: 检查场景中是否有 `DialogueInputHandler` 组件

### Q: 文字太小？
A: 在 Hierarchy 找到 `DialoguePanel → BodyText`，调整 Font Size

---

## 🚀 下一步

系统运行正常后，你可以：

1. **创建自己的对话脚本**
   - 复制 `Resources/TestDialogue.asset`
   - 修改对话内容
   
2. **添加角色立绘**
   - 将立绘图片导入 Unity
   - 在 `Resources/Actor_Alice.asset` 中添加

3. **自定义 UI 样式**
   - 修改 `DialoguePanel` 的背景、颜色、大小
   
4. **编写更多对话命令**
   - 在 `DialogueRunner.cs` 中扩展命令系统

---

## 📝 支持的对话标记

```
[pause=0.5]        # 停顿 0.5 秒
[spd=0.05]         # 改变打字速度
[sfx=sound_name]   # 播放音效
<color=#FF0000>红色文字</color>  # 改变颜色
```

---

## 💡 提示

- 原生 Text 不支持顶点特效（shake/wave），但保留了所有核心功能
- 如果需要炫酷特效，可以考虑使用 TextMeshPro + 高质量中文字体
- 当前配置已经是最简单、最稳定的中文解决方案

---

**祝你开发顺利！** 🎉
