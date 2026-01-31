# 对话系统编辑器指南（给关卡策划）

本指南说明如何在 Unity 内导入与编辑对话。

## 1. CSV 导入流程
1. Excel 中准备 CSV（推荐 UTF-8）
2. 两列格式：

```
speaker,text
,旁白内容
角色名,角色对白
```

3. 在 Unity 菜单：`Tools → Dialogue → Import CSV To DialogueScript`
4. 选择 CSV，确认编码（UTF-8/GBK），导入生成 `DialogueScriptSO`

## 2. 对话节点编辑
在 `DialogueScriptSO` 里可以直接编辑 Nodes：
- 右键节点 → **Insert Line Node After** / **Insert Command Node After**
- 顺序即播放顺序

## 3. 常用标签
```
[pause=0.5]        停顿
[spd=0.05]         速度
[sfx=xxx]          音效
<color=#FF0000>红色</color>
```

## 4. 历史记录
- 按 **H** 打开历史
- 有 `speaker` 就显示名字（黄色），旁白不显示名字
- 历史记录只保存最终文字（去掉控制标记）
  
可调参数（在 HistoryView 上）：
- `Scroll Sensitivity`（滚轮速度）

## 5. 快速测试
场景中挂 `DialogueTest`：
- `testScript` 指向你的 `DialogueScriptSO`
- Play 即可测试

## 6. 对话框字号调整
在 `DialoguePanel` 的 `DialogueViewUniversal` 上：
- `Text Scale`：整体缩放
- `Apply Text Scale` 关闭后可手动固定字号
- `Name/Body Best Fit` 可自动缩放适配边框
