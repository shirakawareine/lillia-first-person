# 0.1.28 實作依據

[English](IMPLEMENTATION.md) · [日本語](IMPLEMENTATION.ja.md) · [简体中文](IMPLEMENTATION.zh-CN.md) · [繁體中文](IMPLEMENTATION.zh-TW.md)

這些結論來自所檢查的遊戲 1.1 中繼資料、原生方法反組譯與指令碼執行順序，並非根據介面名稱推測。下列 RVA 僅供本機複核；外掛使用 IL2CPP interop 與 Harmony，沒有將這些位址寫死。

## 移動

- `CharacterController.SetInputs`，RVA `0x744450`：將輸入軸轉換為相對攝影機的 `_moveInputVector`。`TowardsCamera` 讓 `_lookInputVector` 使用攝影機的平面朝向；`TowardsMovement` 則使用移動方向。此方法也處理動畫參數與奔跑狀態。
- `OnAnimatorMove`，RVA `0x742930`：將 `Animator.deltaPosition` 乘上角色旋轉的反旋轉，累積至 `_pendingRootMotionLocalDelta`。一般移動分支不累積區域座標的 Y。
- `UpdateVelocity`，RVA `0x747340`：進入時先取出並清空累積位移。穩定接地分支使用 `Motor.TransientRotation` 轉回世界座標，投影至垂直於角色上方向的平面，再沿地面切線方向套用步幅／時間。最後單獨加上 `_internalVelocityAdd`。空中另有輸入加速度、重力與阻力分支。

因此，只啟用 TowardsCamera 會讓前進動畫的位移始終沿視線向前，無法實現橫移。0.1.28 在 SetInputs 呼叫內暫時使用該模式，正常返回或發生例外時都還原設定；在 UpdateVelocity 消耗累積位移前，只修改該位移的平面方向。不替換原生輸入向量、速度、重力或最終移動控制速度。

鬆開輸入後的最後一步使用最近一次有效移動方向，避免鬆開橫移鍵後突然向前踏步。停用、更換角色及交由演出控制時會清除這段方向紀錄。特殊動作、移動鎖定、暫停與空中狀態不修改根運動。掛鉤成對安裝；其中一個失敗就撤銷，避免只接管身體朝向。

## 姿勢

所檢查的 `globalgamemanagers.assets` 中，MonoScript 執行順序為：FinalIK 的 IKExecutionOrder 是 9996，各 IK 是 9997～9999，FullBodyBipedIK 是 9999，DynamicBone 是 10000。

DynamicBone.LateUpdate 的原生路徑依序呼叫 Prepare、UpdateParticles、ApplyParticlesToTransforms。因此在此 LateUpdate 的前置回呼中，每幀第一次確定頭部姿勢，就會位於既有動畫／IK 之後、頭髮物理運算之前。後續 DynamicBone 回呼以幀號判斷跳過姿勢確定處理。

PlayerLoop 在 ScriptRunBehaviourLateUpdate 之後也有呼叫相同入口的回呼，用於沒有啟用中 DynamicBone 的影格。它共用同一個幀號判斷，不會重複計算或疊加觀察偏移。卸載時從目前的迴圈刪除本外掛節點，不以舊快照覆蓋其他外掛後來加入的節點。

頭部最終姿勢只在此處確定。下一幀更新前，只撤銷仍由本外掛持有的區域旋轉，讓 Animator 產生新的基礎姿勢；若其他系統已寫入新姿勢，就保留新值。攝影機回呼只讀取骨架、調整鏡頭及可見材質，不再改寫頭骨，因此不需要逐攝影機強制重新計算蒙皮。

一般觀察、待機跟隨及演出中相對動畫的觀察共用此最終處理。待機保留已驗證的 Walk 層 ramdomPose 取消與 NormalIdle 過渡；演出保留 ClipLimits，身體始終由動畫控制。

## 驗證範圍

- 使用實際 BepInEx 與遊戲 interop 組件，以警告視為錯誤的設定成功編譯。
- 模擬測試涵蓋八方向與五種視線角度、身體仍在轉向時的位移、停止時的最後一步、傾斜的移動控制座標、外力保留、空中／暫停／演出時略過修改、停用與更換角色，以及姿勢還原。
- 模擬更新迴圈涵蓋多個物理元件只確定一次姿勢、沒有物理元件時補足、保留其他外掛節點，以及安裝失敗時撤銷。
- 原有待機與演出角度回歸測試通過。
- 後續遊戲 1.1 實測回報為正常且感覺較流暢。這不代表所有場景、碰撞與演出均已涵蓋；未進行影格率測量。
