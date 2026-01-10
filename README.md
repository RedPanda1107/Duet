# Duet-like Unity 游戏设置指南

这是一个面向PC的Duet风格游戏原型，包含菜单和游戏场景、基础旋转控制、坠落障碍物和碰撞检测。

## 项目结构

```
Assets/
├── Scenes/
│   ├── Menu.unity (待创建)
│   └── Game.unity (待创建)
├── Scripts/
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   ├── InputManager.cs
│   │   ├── UIManager.cs
│   │   └── PoolManager.cs
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   └── Dot.cs
│   ├── Obstacles/
│   │   ├── Obstacle.cs
│   │   └── ObstacleSpawner.cs
│   ├── GameInitializer.cs
│   └── PrefabCreator.cs
├── Prefabs/
│   ├── Dot.prefab (待创建)
│   └── Obstacle.prefab (待创建)
└── Resources/
```

## 设置步骤

### 1. 创建场景

1. 在Unity编辑器中，前往 File > New Scene
2. 保存为 `Assets/Scenes/Menu.unity`
3. 重复操作并保存为 `Assets/Scenes/Game.unity`

### 2. 创建预制件

1. 将 `PrefabCreator.cs` 添加到场景中的任意GameObject
2. 右键点击该组件，选择"Create Dot Prefab"和"Create Obstacle Prefab"
3. 创建预制件后移除PrefabCreator组件

### 3. 设置菜单场景

1. 创建一个Canvas (UI > Canvas)
2. 作为子对象添加以下UI元素：
   - 文本: "Duet Game" (作为标题)
   - 按钮: "Start Game" (分配给UIManager.startButton)
   - 按钮: "Quit" (分配给UIManager.quitButton)

3. 为名为"GameInitializer"的GameObject添加GameInitializer组件
4. 将创建的预制件分配给GameInitializer.dotPrefab和.obstaclePrefab

### 4. 设置游戏场景

1. **创建游戏UI Canvas**：
   - 创建一个新的Canvas (UI > Canvas)
   - 命名为"GameCanvas"

2. **创建游戏HUD**：
   - 在GameCanvas下创建Panel，命名为"GameHUD" (分配给UIManager.gameHUD)
   - 在GameHUD下添加Text: "Score: 0"

3. **创建GameOver面板**：
   - 在GameCanvas下创建Panel，命名为"GameOverPanel" (分配给UIManager.gameOverPanel)
   - 在GameOverPanel下添加：
     - Text: "Game Over!"
     - Button: "Restart" (分配给UIManager.restartButton)
     - Button: "Menu" (分配给UIManager.menuButton)

4. **设置UIManager引用**：
   - 找到Managers对象上的UIManager组件
   - 将GameHUD拖到gameHUD字段
   - 将GameOverPanel拖到gameOverPanel字段
   - 将Restart按钮拖到restartButton字段
   - 将Menu按钮拖到menuButton字段

5. **添加GameInitializer**：
   - 创建空GameObject，命名为"GameInitializer"
   - 添加GameInitializer组件
   - 分配Dot和Obstacle预制件

6. **相机设置**：
   - 将主相机定位在(0, 0, -10)
   - 设置Projection为Orthographic

### 5. 配置构建设置

1. 前往 File > Build Settings
2. 将Menu.unity添加为第一个场景，Game.unity添加为第二个场景
3. 将Menu设置为启动场景

### 6. 标签设置

确保这些标签存在 (Edit > Project Settings > Tags and Layers):
- Player
- Obstacle

## 控制方式

- A / 左方向键：逆时针旋转玩家
- D / 右方向键：顺时针旋转玩家

## 游戏流程

1. 从菜单场景开始
2. 点击"Start Game"加载游戏场景
3. 玩家使用A/D键旋转以避开坠落的障碍物
4. 碰撞触发游戏结束画面
5. 可以重新开始或返回菜单

## 调试信息

### UI设置检查器
添加UISetupChecker组件到任意GameObject，它会在启动时检查所有UI引用是否正确设置。

### 调试UI（可选）
如果游戏无法正常运行，可以在游戏场景添加调试UI：

1. 创建一个Text UI元素
2. 将其分配给DebugManager组件的debugText字段
3. DebugManager会显示：
   - 当前游戏状态
   - 输入方向
   - 运行时间

## 备注

- 所有核心机制都已实现
- 没有粒子效果或难度调整（如要求）
- 使用对象池以优化性能
- 简单的碰撞检测和游戏结束状态

## 常见问题

**Q: DontDestroyOnLoad错误**
A: 确保Managers GameObject是场景的根对象（没有父对象）

**Q: GameHUD is null in UIManager**
A: 按以下步骤设置UI引用：
1. 在Game场景中创建GameHUD Panel
2. 找到Managers对象上的UIManager组件
3. 将GameHUD Panel拖到UIManager的gameHUD字段
4. 同样设置gameOverPanel等其他UI元素

**Q: 游戏状态显示Menu**
A: 确保在Game场景中调用了GameManager.Instance.SetGameStateToPlaying()

**Q: GameOver面板一直显示**
A: 检查UIManager的gameHUD和gameOverPanel是否都已正确分配UI元素

**Q: 游戏没有响应输入**
A: 检查GameManager状态是否正确设置为Playing，使用调试UI确认状态

**Q: 玩家无法旋转**
A: 确保PlayerController组件已添加到PlayerPivot对象上
