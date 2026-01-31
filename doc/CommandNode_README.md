# CommandNode 指令说明

本系统支持在 `CommandNode.command` 中写入简单指令，用于控制角色立绘、等待与场景切换。

## 1. 基本格式
```
<命令组> <子命令> key=value key=value ...
```

示例：
```
actor show id=alice portrait=normal x=-300 y=0
wait 0.5
scene load name=Stage2
```

---

## 2. actor 指令

### 2.1 显示角色
```
actor show id=alice portrait=normal x=-300 y=0
actor show name=祖母 portrait=normal x=-300 y=0
```

参数：
- `id`：角色 ID（可选）
- `name`：角色显示名（可选，若未提供 id 则使用 name 映射）
- `portrait`：立绘 key（可选，若为空则使用默认立绘）
- `x` / `y`：位置（可选）

### 2.2 隐藏角色
```
actor hide id=alice
actor hide name=祖母
```

### 2.3 聚焦角色（高亮）
```
actor focus id=alice
actor focus name=祖母
```

---

## 3. wait 指令
等待指定秒数后进入下一条对话：
```
wait 0.5
```

---

## 4. scene 指令
### 4.1 切换场景
```
scene load name=YourScene
```

或：
```
scene load index=1
```

可选参数：
- `mode=single`（默认）
- `mode=additive`

### 4.2 重新加载当前场景
```
scene reload
```

---

## 5. 自动立绘交替与命令覆盖

系统默认会根据说话者自动显示/高亮角色。  
当你使用 `actor show/hide/focus` 后，会短时间内暂停自动逻辑（避免被下一句自动覆盖）。

覆盖时长可在 `DialogueRunner` Inspector 中调整：
- `Command Override Seconds`（默认 1.5 秒）

---

## 6. 常见示例

```
actor show id=alice portrait=normal x=-300 y=0
actor show id=bob portrait=smile x=300 y=0
wait 0.3
actor focus id=bob
```

