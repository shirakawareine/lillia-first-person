# 0.1.28 实现依据

[English](IMPLEMENTATION.md) · [日本語](IMPLEMENTATION.ja.md) · [简体中文](IMPLEMENTATION.zh-CN.md) · [繁體中文](IMPLEMENTATION.zh-TW.md)

这些结论来自当前本机游戏 1.1 的元数据、原生方法反汇编与脚本执行顺序，不是根据接口名称推测。下列 RVA 仅用于本地复核；插件使用 IL2CPP interop 与 Harmony，不硬编码这些地址。

## 移动

- `CharacterController.SetInputs`，RVA `0x744450`：将输入轴转换成相机相对的 `_moveInputVector`。`TowardsCamera` 决定 `_lookInputVector` 取相机平面朝向；`TowardsMovement` 则取移动朝向。它还处理动画参数和奔跑状态。
- `OnAnimatorMove`，RVA `0x742930`：将 `Animator.deltaPosition` 乘角色旋转的逆，积累到 `_pendingRootMotionLocalDelta`，普通移动分支不积累局部 Y。
- `UpdateVelocity`，RVA `0x747340`：入口先取出并清空积累位移。稳定地面分支用 `Motor.TransientRotation` 转回世界方向，投影到角色上方向的平面，再沿地面切向施加步幅／时间。最后单独叠加 `_internalVelocityAdd`。空中另有输入加速度、重力和阻力分支。

因此单独启用 TowardsCamera 会让向前的动画位移始终沿视线向前，不能实现横移。0.1.28 在 SetInputs 调用内临时借用该模式，返回或异常退出时恢复原设置；在 UpdateVelocity 消费积累位移前，仅修改该位移的平面方向。原生输入向量、速度、重力和最终马达速度不被替换。

无输入的收步使用最后一个有效移动方向，避免松开侧移键后收步突然向前。禁用、换角色及演出交接时清空这段方向记录。特殊动作、移动锁定、暂停和空中不改根运动。钩子成对安装；其中一个安装失败就撤销，避免只接管身体朝向。

## 姿态

本机 `globalgamemanagers.assets` 中 MonoScript 的执行顺序：FinalIK 的 IKExecutionOrder 为 9996、各 IK 为 9997～9999，FullBodyBipedIK 为 9999，DynamicBone 为 10000。

DynamicBone.LateUpdate 的原生路径依次调用 Prepare、UpdateParticles、ApplyParticlesToTransforms。因此在该 LateUpdate 的前置回调里，每帧第一次确定头部姿态，处于现有动画／IK 之后、头发物理之前。后续 DynamicBone 回调直接被帧号门控跳过。

PlayerLoop 在 ScriptRunBehaviourLateUpdate 之后还有同一入口的回调，用于没有活动 DynamicBone 的帧。它也受同一个帧号门控，不重复计算或重复叠加观察偏移。卸载时从当前循环删除本插件节点，不用旧快照覆盖其他插件后来加入的节点。

头部只有这一处确定最终姿态。下一帧更新前撤销仍由本插件持有的局部旋转，让 Animator 产生新的原始姿态；若其他系统已写入新姿态则保留新值。相机回调只读取骨骼，调整镜头和可见材质，不再改写头骨。故不需要逐相机强制重算蒙皮。

普通观察、待机跟随与演出相对动画观察共用这一出口。待机保留已验证的 Walk 层 ramdomPose 取消和 NormalIdle 过渡；演出保留 ClipLimits，身体始终交由动画控制。

## 验证边界

- 使用实际 BepInEx 与游戏 interop 程序集，开启编译警告视为错误，编译通过。
- 模拟测试覆盖八方向与五种视线角度、身体尚在转向时的位移、收步、倾斜马达坐标、外力保留、空中／暂停／演出避让、关闭和换角色、姿态恢复。
- 模拟更新循环覆盖多个物理组件只提交一次、无物理组件时补齐、保留其他插件节点、安装失败撤销。
- 原有待机与演出角度回归测试通过。
- 用户随后在游戏 1.1 中确认本轮实测效果正常，并反馈更流畅。该反馈不等于所有场景、碰撞与演出均已覆盖；未进行帧率测量。
