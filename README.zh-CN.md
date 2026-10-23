# 见习莉莉娅 · 第一人称视角插件

[English](README.md) · [日本語](README.ja.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md)

为《见习莉莉娅》添加可切换的第一人称视角，属于非官方插件。当前为 **0.1.28 测试版**，适用于游戏 1.1／1.2 版本、Unity 2022.3.62f2、Windows x64、IL2CPP、BepInEx 6 be.788。

**游戏 1.2 兼容情况（2026-10-23）：** 已从 GitHub Releases 下载公开发布的 v0.1.28 整合包，并安装到游戏 1.2 版本。首次初始化完成后，用户实测反馈可以运行，暂未发现异常；本次测试无需修改插件。这是用户实测的运行结果，不代表所有场景、动画或新增游戏机制均已验证。此前的游戏 1.1 测试记录仍保留在下文。

## 安装步骤与常见问题

### 按已安装的组件选择下载

适用环境为游戏 **1.1／1.2 版本**、Windows x64。整合包使用 **BepInEx 6 be.788 IL2CPP x64**；Mono、x86 和 BepInEx 5 版本不能替代。不包含游戏本体。

- **未安装 BepInEx：** 从 [Releases](https://github.com/shirakawareine/lillia-first-person/releases/tag/v0.1.28) 下载 `FirstPersonTest-0.1.28-bundle-win-x64.zip`。
- **已安装兼容的 BepInEx：** 下载 `FirstPersonTest-0.1.28-release.zip` 或 `FirstPersonTest.dll`，保留已有加载器与配置。
- **源码：** `FirstPersonTest-0.1.28-source.zip` 用于编译插件，不是安装包；GitHub 自动生成的源码压缩包也不是安装包。

先查看游戏目录是否已有 `BepInEx` 文件夹或 `winhttp.dll`。存在这些文件说明可能已安装加载器或其他修改，应先核对，不能直接覆盖；可在已有的 `BepInEx/LogOutput.log` 中确认 BepInEx 版本和 IL2CPP 运行环境。

### 整合包安装（未安装 BepInEx）

1. 保存并退出游戏。修改安装目录前，备份存档和已有配置。
2. 建议使用 `C:\Games\Lillia` 等短路径，需要调整时移动**整个游戏文件夹**，不要只移动 EXE。测试中深层目录即使未启用 BepInEx 也无法加载角色，相同文件改放短路径后成功；尚未确定精确的长度上限。
3. 将整合包的**全部内容**解压到 `NoviceSuccubusLillia.exe` 旁边。不要在压缩包内启动，不要解压到 Data 文件夹，也不要让加载器文件与 EXE 之间多套一层整合包文件夹。
4. 对照下列目录。EXE 和 Data 文件夹来自游戏本体；整合包提供加载器、运行时、插件与配置。

```text
C:\Games\Lillia\
  NoviceSuccubusLillia.exe
  NoviceSuccubusLillia_Data\
  winhttp.dll
  doorstop_config.ini
  dotnet\
  BepInEx\
    core\
    plugins\FirstPersonTest.dll
    config\BepInEx.cfg
    config\local.lillia.firstpersontest.cfg
```

5. 启动平时使用的游戏 EXE。首次需要联网下载 Unity 基础库并生成 `BepInEx/interop`，可能需要数分钟。等待时不要重复启动多个游戏进程。没有弹出日志控制台属于正常设置。
6. 进入正式游戏后，第一人称默认开启。在游戏窗口处于前台时，测试 **F8** 切换视角、**F9** 隐藏／显示头发。插件日志应出现 `Loaded v0.1.28`；加载成功不等于所有游戏功能都已经验证。

### 插件安装与更新（已安装 BepInEx）

1. 保存并退出。将旧 DLL 和 `BepInEx/config/local.lillia.firstpersontest.cfg` 备份到 `plugins` 文件夹以外的位置。
2. 替换 `BepInEx/plugins/FirstPersonTest.dll`。包括子文件夹在内只保留一份插件，备份 DLL 不要留在其中。保留已有加载器和配置。
3. 首次单独安装插件时，先运行一次生成配置，再退出。新生成的 `ClipLimits` 为空；整合包则已带四条预设。
4. 替换 DLL **不会**自动合并新增预设。在 `[EventView]` 中，把下方四条示例合并到现有的 `ClipLimits` 一行，保留自定义条目。每个精确动作名只出现一次，整个值保持一行。本次新增的是 `Fellatio_Female(Gsun)_Loop|3|2|2`。
5. 保存并重启游戏，配置在启动时读取。本次整合包修订只更新预设与说明，0.1.28 DLL 未改变。

### 常见问题

| 现象 | 优先检查 |
| --- | --- |
| 没有第一人称，或 F8 无反应 | 先进入正式游戏并让游戏窗口处于前台，核对目录层级和重复 DLL，再查看加载器、插件日志是否成功加载或报错。 |
| 首次启动看似卡住 | 查看 `BepInEx/LogOutput.log` 是否仍在下载或生成 interop；若报告下载失败，按具体错误检查网络后再试，不要反复启动更多进程。 |
| `Character content could not be loaded` | 在 Unity 日志中查找 `A local absolute bundle path is required`；如有，退出并把整个游戏移到短路径。单凭界面提示不能断定是路径问题，仍失败时保留日志。 |
| 演出视角仍能大幅转动 | 核对日志 `Event view: clip=...` 的精确名称、对应 ClipLimits 条目以及是否重启；名称区分大小写，未匹配动作使用通用范围。 |
| 头发或 NPC 挡住视线 | F9 切换头发显示；演出被遮挡时用 F8 切回第三人称。 |

加载器日志：`BepInEx/LogOutput.log`；插件日志：`BepInEx/FirstPersonTest.log`；Unity 日志：`%USERPROFILE%\AppData\LocalLow\PaconeSoft\Novice Succubus Lillia\Player.log`。再次启动前保存需要排查的日志，不要把删除存档或缓存当作常规安装步骤。

临时关闭第一人称可按 F8。卸载插件时，退出游戏，只删除 `BepInEx/plugins/FirstPersonTest.dll`，保留其他 MOD 共用的加载器。回退时恢复备份的 DLL 与配置。下载文件可用 `SHA256SUMS.txt` 核对。

## 截图展示

### 第一人称 · 隐藏头发

![第一人称 · 隐藏头发](docs/screenshots/first-person-hair-hidden.png)

无遮挡的第一人称街道视野，左侧保留游戏原有的角色预览。

### 第一人称 · 显示头发

![第一人称 · 显示头发](docs/screenshots/first-person-hair-visible.png)

头发可能遮挡视野。按 **F9** 隐藏或显示第一人称视野中的头发。

### 第三人称

![第三人称](docs/screenshots/third-person.png)

按 **F8** 随时切换第一／第三人称视角。

截图用于展示视角效果，不代表同条件下的性能对比。

## 0.1.28 的控制规则

- 身体朝向观察方向；W/A/S/D 只控制相对视线的前进、横移和后退。
- 只允许前进、斜前进奔跑；纯侧向、后退和后斜向保持步行。
- 头部在动画和身体姿态修正之后、头发物理之前统一确定。主画面与角落预览使用同一副骨骼。
- 删除预览专用的 65° 转头限制、预览姿势切换和每个相机强制重算蒙皮的补丁。
- 保留待机头部跟随、操作时退出待机小动作、演出相对动画的观察角度限制。
- 保留头部遮挡材质处理、F9 隐藏头发，以及第一人称期间关闭墙体淡出的功能。

游戏原来按身体朝向施加走路动画位移。本版保留动画产生的步幅，把这段位移转向移动输入的方向，再交回原生马达处理坡面、碰撞与外力。未替换移动速度、体力、空中运动、重力或游戏资源。

**验证状态：** 已使用本机游戏程序集完成编译，并通过方向换算、控制恢复、姿态更新顺序、待机与演出模拟测试。用户已在游戏 1.1 版本中确认本轮实测效果正常，并反馈运行更流畅；尚未进行帧率测量，也未逐一验证所有场景与演出。

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

**DLL 本身没有内置逐动作限制，普通插件包首次生成的 `ClipLimits` 为空；整合包已预填下方四个动作的限制。** 留空时，所有演出都使用通用范围：左右各 75°、上 75°、下 85°（修改过配置时以你的实际值为准）。需要几乎完全跟随动画的动作，应手动添加以下限制；安装 DLL 不会自动获得这些配置。

1. 安装插件并运行一次游戏，让插件生成配置文件，然后退出游戏。
2. 打开 `BepInEx/config/local.lillia.firstpersontest.cfg`，找到已有的 `[EventView]` 分组。
3. 将该分组中的 `ClipLimits =` 一行替换为下面的示例；不要重复添加 `[EventView]` 分组或同名配置项。整条 `ClipLimits` 保持为一行，使用英文分号和竖线。
4. 保存文件，重新启动游戏。

以下配置覆盖游戏 1.1 版本资源目录中此系列的四个动作片段，可以直接复制：

```ini
[EventView]
DefaultYaw = 75
ClipLimits = Fellatio_Female_Loop|3|2|2;Fellatio_Female(bellied man)_Loop|3|2|2;Fellatio_Female_End|3|2|2;Fellatio_Female(Gsun)_Loop|3|2|2
```

Gsun 动作的限制（左右各 3°、上下各 2°）已通过游戏内测试，由测试者确认；这不代表所有动作都已穷尽测试。

`3|2|2` 表示相对动画头部方向，左右各允许 3°、上抬 2°、下压 2°，即几乎完全跟随动画。只有列出的四个片段使用这个小范围；其他片段仍使用通用范围。若希望某个片段完全锁定到动画，将它对应的 `3|2|2` 改为 `0|0|0`。

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

输出为 `bin/Release/net6.0/FirstPersonTest.dll`。源码 ZIP 不包含游戏程序集、资源或 BepInEx 依赖。实现依据见 [IMPLEMENTATION.zh-CN.md](IMPLEMENTATION.zh-CN.md)。

## 卸载

退出游戏后删除插件 DLL。需要回退时，用之前保存的 DLL 替换，保留原配置。
