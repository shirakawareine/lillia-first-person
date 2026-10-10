# 見習莉莉婭 · 第一人稱視角外掛

[English](README.md) · [日本語](README.ja.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md)

為《見習莉莉婭》（Novice Succubus Lillia）加入可切換的第一人稱視角，屬於非官方外掛。目前為 **0.1.28 測試版**，已在遊戲 1.1、Unity 2022.3.62f2、Windows x64、IL2CPP、BepInEx 6 be.788 環境測試。

## 0.1.28 的控制規則

- 身體朝向視線方向；W/A/S/D 只控制相對視線的前進、橫移及後退。
- 只有前進及斜前進可以奔跑；純側向、後退及斜後退維持步行。
- 頭部姿勢在動畫與 IK 之後、頭髮物理運算之前統一確定。主畫面與角落預覽共用同一套骨架。
- 移除預覽專用的 65° 轉頭限制、預覽姿勢切換，以及每個攝影機強制重新計算蒙皮的修補。
- 保留待機時跟隨頭部動畫、操作時中斷待機小動作，以及演出中相對動畫的觀察角度限制。
- 保留頭部遮擋材質處理、F9 隱藏頭髮，以及第一人稱期間停用牆壁淡出的功能。

遊戲原本沿身體朝向套用走路動畫的位移。本版保留動畫產生的步幅，將位移轉向輸入方向，再交回遊戲原有的移動控制處理斜坡、碰撞與外力。不替換移動速度、體力、空中移動、重力或遊戲資源。

**驗證狀態：** 已使用遊戲產生的組件完成編譯，並通過方向換算、控制還原、姿勢更新順序、待機與演出模擬測試。遊戲 1.1 的實測回報為正常，且主觀感受較流暢；尚未測量影格率，也未逐一驗證所有場景與演出。

## 下載與安裝

從 [Releases](https://github.com/shirakawareine/lillia-first-person/releases/tag/v0.1.28) 下載 `FirstPersonTest-0.1.28-release.zip`，另提供 DLL 與原始碼 ZIP。

**需要另外安裝 BepInEx，本套件不是一鍵整合安裝包。** 不包含遊戲本體或載入器。

1. 安裝 BepInEx 6 IL2CPP x64，先執行一次遊戲以產生 interop 組件。
2. 存檔並結束遊戲。
3. 將 `FirstPersonTest.dll` 放入 `BepInEx/plugins/`。更新時替換既有 DLL，只保留一份同名外掛。
4. 保留 `BepInEx/config/local.lillia.firstpersontest.cfg`，重新啟動遊戲。

啟動日誌應顯示 `Loaded v0.1.28`。既有的逐動畫 `ClipLimits` 不需要刪除或重設。

## 操作

| 操作 | 功能 |
| --- | --- |
| F8 | 開關第一人稱；關閉後恢復遊戲原有控制 |
| F9 | 第一人稱中隱藏／顯示頭髮，只在本次執行期間有效 |
| 滑鼠 | 左右觀察、抬頭低頭 |
| W/A/S/D | 相對視線移動，不因方向鍵而轉動身體 |
| Shift + W、W+A 或 W+D | 請求奔跑，體力及能否奔跑仍由遊戲決定 |

按鍵只在遊戲位於前景時回應。演出自由觀察不必按住滑鼠右鍵；外掛不接管游標鎖定或 UI 事件系統。

## 設定

結束遊戲後修改 `BepInEx/config/local.lillia.firstpersontest.cfg`，重新啟動後生效。

| 分組 / 設定 | 預設值 | 說明 |
| --- | --- | --- |
| View / EyeHeightAdjustment | -0.01 | 頭部區域座標的眼位高度修正，範圍 -0.03～0.03 |
| View / LookUpDegrees | 75 | 最大仰角，範圍 0～89° |
| View / LookDownDegrees | 85 | 最大俯角，範圍 0～89° |
| View / PitchSensitivity | 3 | 一般遊戲狀態的上下觀察靈敏度 |
| View / InvertY | false | 反轉上下觀察 |
| Body / FollowCameraYaw | true | 身體朝向視線，移動方向獨立；統一控制需保持開啟 |
| Body / FollowHeadPitch | true | 頭部對齊觀察方向；統一姿勢需保持開啟 |
| Body / HideHeadInFirstPerson | true | 主畫面隱藏偵測到的頭部遮擋材質槽，預覽保留 |
| Body / HideHairOnStart | false | 啟動時隱藏頭髮，可用 F9 暫時切換 |
| Body / ShowPlayerRenderers | true | 在主畫面顯示啟用中的身體 Renderer |
| Body / NearClip | 0.02 | 近裁切距離，範圍 0.005～0.1，且不增大原值 |
| Body / HeadMaterialSlots | 空白 | 進階設定：精確填入 RendererName:槽號，以分號分隔 |
| EventView / DefaultYaw | 75 | 一般演出相對動畫頭部的左右觀察範圍 |
| EventView / Sensitivity | 2 | 演出自由觀察靈敏度 |
| EventView / ClipLimits | 空白 | 按動畫片段分別覆寫左右／上／下範圍 |

`ClipLimits` 格式為 `ClipName|左右|上|下;OtherClip|左右|上|下`。每項 0～89°；`0|0|0` 完全跟隨動畫。片段專屬限制優先於預設的左右 75°。

### ClipLimits 設定範例

**新玩家請注意：0.1.28 沒有內建逐動畫限制，首次產生的 ClipLimits 為空白。** 預設情況下，所有演出使用左右各 75°、上 75°、下 85° 的通用範圍；若已修改設定，則以實際設定值為準。只安裝 DLL 不會自動加入以下限制。

1. 安裝外掛並執行一次遊戲，讓外掛產生設定檔，再結束遊戲。
2. 開啟 `BepInEx/config/local.lillia.firstpersontest.cfg`，找到既有的 `[EventView]` 分組。
3. 將其中的 `ClipLimits =` 一行替換成下方範例。不要重複加入分組或同名設定。整個值保持單行，使用半形 `;` 與 `|`。
4. 儲存後重新啟動遊戲。

以下三個動畫片段的設定已在遊戲 1.1 中使用並驗證，可直接複製：

```ini
[EventView]
DefaultYaw = 75
ClipLimits = Fellatio_Female_Loop|3|2|2;Fellatio_Female(bellied man)_Loop|3|2|2;Fellatio_Female_End|3|2|2
```

`3|2|2` 表示相對動畫頭部方向，左右各允許 3°、上抬 2°、下壓 2°，幾乎完全跟隨動畫。只有列出的三個片段使用此小範圍；其他片段仍使用通用範圍。若要完全鎖定至動畫，將該片段的值改為 `0|0|0`。

已有自訂項目時，以半形分號合併至同一個值，不必覆蓋原有項目。同一片段只填一次。片段名稱區分大小寫，必須與日誌 `Event view: clip=...` 完全一致；遊戲更新後若名稱改變，請依新日誌調整。

舊版的 `LookDownClearance`、`LookDownForward`、`LookDownUp` 和 `HeadPitchWeight` 已停用。眼位不會額外前移。待機動畫保留原有的視角過渡與旋轉濾波；滑鼠觀察沒有新增預覽專用的角度限制或濾波。

## 已知限制

- 尚未找到相容且已載入的橫移、後退及原地轉身動畫。本版調整移動方向，不會產生新的腳步動作，可能出現滑步。
- 特殊演出中身體由動畫控制；NPC 可能完全遮住視線，建議按 F8 切回第三人稱。
- 極端角度仍可能穿模，需依實際演出調整限制。
- 依賴遊戲的攝影機名稱、角色控制器、骨架與更新順序；遊戲更新後需要重新確認相容性。
- 頭髮物理參數維持原樣，尚未測量效能改善。

## 複測

1. 固定滑鼠，依序按 W/A/D/S 及斜向：身體不應因方向鍵轉向，位移方向應正確。
2. 抬頭、低頭後開始及停止走路：主畫面、角落模型與頭髮應保持一致。
3. Shift+W、Shift+W+A/D 可請求奔跑；Shift+A/D/S 保持步行。
4. 靜站等待待機小動作，再移動滑鼠，確認視角跟隨與待機中斷。
5. 進入／離開演出，確認身體由動畫控制、專屬限制有效，結束後一般移動恢復正常。
6. F8 關閉再開啟，並切換場景，確認控制與視角還原。

## 日誌

主要日誌為 `BepInEx/FirstPersonTest.log`；載入失敗時查看 `BepInEx/LogOutput.log`。

- `Loaded v0.1.28`：版本與啟動狀態。
- `Body follow: SetInputs observed`：已收到輸入回呼。
- `Locomotion:`：輸入方向、動畫位移及調整後的區域位移，每五秒最多一次。
- `Unified pose:`：姿勢確定階段；有啟用的頭髮物理運算時應為 `BeforeHairPhysics`。
- `Pose:`：每秒抽取一幀，確認身體、頭部與攝影機的更新順序。
- `Idle control:` / `Event view:`：待機中斷與演出限制。
- `disabled after control error` / `Retrying after error`：錯誤與復原。

日誌採用有容量上限的背景檔案佇列，不會每幀同步輸出至主控台。

## 從原始碼建置

使用支援 .NET 6 的 SDK，以及遊戲產生的 BepInEx `core` 與 `interop` 相依檔案：

```powershell
dotnet build .\FirstPersonTest.csproj -c Release '-p:BepInExDir=D:\Games\Lillia\BepInEx'
```

輸出為 `bin/Release/net6.0/FirstPersonTest.dll`。專案不散布遊戲組件、資源或 BepInEx 相依檔案。實作說明見 [IMPLEMENTATION.md](IMPLEMENTATION.md)（簡體中文）。

## 移除

結束遊戲後刪除外掛 DLL。若要退回舊版，以先前保存的 DLL 替換，並保留設定檔。
