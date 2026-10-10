# 见习莉莉娅 · 第一人称视角插件

[English](README.md) · [日本語](README.ja.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md)

为《见习莉莉娅》添加可切换的第一人称视角，属于非官方插件。当前为 **0.1.28 测试版**，适配本机游戏 1.1、Unity 2022.3.62f2、Windows x64、IL2CPP、BepInEx 6 be.788。

## 0.1.28 的控制规则

- 身体朝向观察方向；W/A/S/D 只控制相对视线的前进、横移和后退。
- 只允许前进、斜前进奔跑；纯侧向、后退和后斜向保持步行。
- 头部在动画和身体姿态修正之后、头发物理之前统一确定。主画面与角落预览使用同一副骨骼。
- 删除预览专用的 65° 转头限制、预览姿势切换和每个相机强制重算蒙皮的补丁。
- 保留待机头部跟随、操作时退出待机小动作、演出相对动画的观察角度限制。
- 保留头部遮挡材质处理、F9 隐藏头发，以及第一人称期间关闭墙体淡出的功能。

游戏原来按身体朝向施加走路动画位移。本版保留动画产生的步幅，把这段位移转向移动输入的方向，再交回原生马达处理坡面、碰撞与外力。未替换移动速度、体力、空中运动、重力或游戏资源。

**验证状态：** 已使用本机游戏程序集完成编译，并通过方向换算、控制恢复、姿态更新顺序、待机与演出模拟测试。用户已在游戏 1.1 中确认本轮实测效果正常，并反馈运行更流畅；尚未进行帧率测量，也未逐一验证所有场景与演出。

## 下载

从 [Releases](https://github.com/shirakawareine/lillia-first-person/releases/tag/v0.1.28) 下载 `FirstPersonTest-0.1.28-release.zip`，另提供 DLL 与源码 ZIP。

**需要另外安装 BepInEx，这不是一键整合包。** 不包含游戏本体或加载器。

## 安装与升级

1. 安装 BepInEx 6 IL2CPP x64，先运行一次游戏，生成 interop 程序集。
2. 保存并退出游戏。
3. 将包内 `FirstPersonTest.dll` 替换到 `BepInEx/plugins/` 中的原位置。只保留一份同名插件。
4. 保留 `BepInEx/config/local.lillia.firstpersontest.cfg`，重新启动游戏。

启动日志应显示 `Loaded v0.1.28`。已有逐动作 `ClipLimits` 不需要删除或重置。

## 操作

| 操作 | 功能 |
| --- | --- |
| F8 | 开关第一人称；关闭后使用游戏原控制 |
| F9 | 第一人称中隐藏／显示头发，仅保存本次会话 |
| 鼠标 | 左右观察、抬头低头 |
| W/A/S/D | 相对视线移动，方向键不再驱动身体转向 |
| Shift + W 或 W+A/D | 请求奔跑，体力与是否允许奔跑仍由游戏决定 |

按键只在游戏位于前台时响应。演出自由观察无需按住右键；插件不接管鼠标锁定或 UI 事件系统。

## 配置

退出游戏后修改 `BepInEx/config/local.lillia.firstpersontest.cfg`，重启生效。

| 分组 / 配置 | 默认值 | 说明 |
| --- | --- | --- |
| View / EyeHeightAdjustment | -0.01 | 头部局部坐标的眼位高度修正，范围 -0.03～0.03 |
| View / LookUpDegrees | 75 | 最大仰角，范围 0～89° |
| View / LookDownDegrees | 85 | 最大俯角，范围 0～89° |
| View / PitchSensitivity | 3 | 普通视角上下观察灵敏度 |
| View / InvertY | false | 反转上下观察 |
| Body / FollowCameraYaw | true | 身体朝向镜头，移动方向独立；统一控制需保持开启 |
| Body / FollowHeadPitch | true | 头部对齐观察方向；统一姿态需保持开启 |
| Body / HideHeadInFirstPerson | true | 主画面隐藏识别到的头部遮挡材质槽，预览保留 |
| Body / HideHairOnStart | false | 启动时隐藏头发，可用 F9 临时切换 |
| Body / ShowPlayerRenderers | true | 在主画面显示活动的身体渲染器 |
| Body / NearClip | 0.02 | 近裁剪距离，范围 0.005～0.1，且不增大原值 |
| Body / HeadMaterialSlots | 空 | 高级设置：精确填写 RendererName:槽号，以分号分隔 |
| EventView / DefaultYaw | 75 | 普通演出相对动画头部的左右观察范围 |
| EventView / Sensitivity | 2 | 演出自由观察灵敏度 |
| EventView / ClipLimits | 空 | 按动画片段分别覆盖左右／上／下范围 |

`ClipLimits` 格式为 `ClipName|左右|上|下;OtherClip|左右|上|下`。每项 0～89°；`0|0|0` 完全跟随动画。原配置中动作专属的小范围限制优先于默认 75°。

### ClipLimits 配置示例

**新玩家注意：0.1.28 没有内置逐动作限制，首次生成的 `ClipLimits` 为空。** 留空时，所有演出都使用通用范围：左右各 75°、上 75°、下 85°（修改过配置时以你的实际值为准）。需要几乎完全跟随动画的动作，应手动添加以下限制；安装 DLL 不会自动获得这些配置。

1. 安装插件并运行一次游戏，让插件生成配置文件，然后退出游戏。
2. 打开 `BepInEx/config/local.lillia.firstpersontest.cfg`，找到已有的 `[EventView]` 分组。
3. 将该分组中的 `ClipLimits =` 一行替换为下面的示例；不要重复添加 `[EventView]` 分组或同名配置项。整条 `ClipLimits` 保持为一行，使用英文分号和竖线。
4. 保存文件，重新启动游戏。

以下是游戏 1.1 中已使用并验证的三个动作片段配置，可以直接复制：

```ini
[EventView]
DefaultYaw = 75
ClipLimits = Fellatio_Female_Loop|3|2|2;Fellatio_Female(bellied man)_Loop|3|2|2;Fellatio_Female_End|3|2|2
```

`3|2|2` 表示相对动画头部方向，左右各允许 3°、上抬 2°、下压 2°，即几乎完全跟随动画。只有列出的三个片段使用这个小范围；其他片段仍使用通用范围。若希望某个片段完全锁定到动画，将它对应的 `3|2|2` 改为 `0|0|0`。

已有自定义条目时，用英文分号合并到同一个 `ClipLimits` 值里，不必覆盖原有条目。同一个片段只写一次。片段名区分大小写，必须与日志 `Event view: clip=...` 中的名称一致；游戏更新后若动作名改变，旧条目不会匹配，需要按新日志调整。

旧版的 `LookDownClearance`、`LookDownForward`、`LookDownUp` 和 `HeadPitchWeight` 已停用。当前眼位不额外前移。待机动画保留原有的视角过渡与旋转滤波；鼠标观察未增加预览专用的角度限制或滤波。

## 已知限制

- 尚未找到兼容且已加载的横移、倒退和原地转身动画。本版调整移动方向，不会凭空生成对应脚步动作，可能出现滑步。
- 特殊演出中身体交由动画控制；NPC 可能完全挡住视线，建议按 F8 切回第三人称。
- 极端观察角度仍可能穿模，动作限制需要按实际演出调整。
- 依赖本游戏的相机名称、角色控制器、骨骼与更新顺序；游戏更新后需要复核。
- 头发物理参数保持原样，本次没有性能测量。

## 复测

1. 固定鼠标，依次按 W/A/D/S 和斜向：身体不应因方向键转向，位移方向应正确。
2. 分别抬头、低头后起步和停步：主画面、角落模型与头发应保持一致。
3. Shift+W、Shift+W+A/D 可请求奔跑；Shift+A/D/S 保持步行。
4. 静站等待待机小动作，再移动鼠标：保留视角跟随和动作退出。
5. 进入／退出特殊演出，确认身体不被插件转动，专属角度限制仍生效，结束后四向移动正常。
6. 按 F8 关闭并重新开启，再切场景，检查控制和视角恢复。

## 日志

主要日志：`BepInEx/FirstPersonTest.log`；加载失败时查看 `BepInEx/LogOutput.log`。

- `Loaded v0.1.28`：版本与启动状态。
- `Body follow: SetInputs observed`：实际收到游戏输入回调。
- `Locomotion:`：原移动方向、动画位移和调整后的局部位移，每五秒最多一次。
- `Unified pose:`：姿态确定的阶段；有活动头发物理时应为 `BeforeHairPhysics`。
- `Pose:`：每秒抽取一帧，核对身体、头部和各相机的更新顺序。
- `Idle control:`、`Event view:`：待机打断和演出限制。
- `disabled after control error` 或 `Retrying after error`：异常及恢复信息。

日志使用有界后台文件队列，不逐帧同步输出到控制台。

## 从源码构建

使用支持 .NET 6 的 SDK，以及本机游戏已生成的 BepInEx `core` 与 `interop` 依赖：

```powershell
dotnet build .\FirstPersonTest.csproj -c Release '-p:BepInExDir=D:\Games\Lillia\BepInEx'
```

输出为 `bin/Release/net6.0/FirstPersonTest.dll`。项目不分发游戏程序集、资源或 BepInEx 依赖。实现依据见 [IMPLEMENTATION.zh-CN.md](IMPLEMENTATION.zh-CN.md)。

## 卸载

退出游戏后删除插件 DLL。需要回退时，用之前保存的 DLL 替换，保留原配置。
