# Lillia First Person — 一人称視点プラグイン

[English](README.md) · [日本語](README.ja.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md)

**Novice Succubus Lillia** に切り替え可能な一人称視点を追加する非公式プラグインです。現在のバージョンは **0.1.28（テスト版）**。ゲーム 1.1、Unity 2022.3.62f2、Windows x64、IL2CPP、BepInEx 6 be.788 で動作確認しています。

## インストール手順とトラブル対処

### 導入済みの構成に応じたダウンロード

対象はゲーム **バージョン 1.1**、Windows x64 です。同梱ローダーは **BepInEx 6 be.788 IL2CPP x64** です。Mono、x86、BepInEx 5 版で代用しないでください。ゲーム本体は含まれません。

- **BepInEx 未導入：** [Releases](https://github.com/shirakawareine/lillia-first-person/releases/tag/v0.1.28) から `FirstPersonTest-0.1.28-bundle-win-x64.zip` をダウンロードします。
- **対応する BepInEx を導入済み：** `FirstPersonTest-0.1.28-release.zip` または `FirstPersonTest.dll` を使用し、既存ローダーと設定を残します。
- **ソース：** `FirstPersonTest-0.1.28-source.zip` はビルド用であり、インストール用ではありません。GitHub 自動生成のソースアーカイブも同様です。

ゲームフォルダーに `BepInEx` や `winhttp.dll` があるか確認してください。既存ローダーや別の変更がある可能性があるため、確認せず上書きしないでください。既存の `BepInEx/LogOutput.log` があれば、BepInEx バージョンと IL2CPP 実行環境を確認できます。

### 同梱セットの導入（BepInEx 未導入）

1. セーブしてゲームを終了します。変更前にセーブと既存設定をバックアップしてください。
2. `C:\Games\Lillia` など短いパスを使用します。必要なら EXE だけでなく**ゲームフォルダー全体**を移動してください。深い階層では BepInEx 無効時もキャラクター読み込みが失敗し、同じファイルを短いパスに置くと成功しました。正確なパス長の上限は未確定です。
3. セットの**全内容**を `NoviceSuccubusLillia.exe` と同じ場所に展開します。圧縮ファイル内から起動したり、Data フォルダーに入れたり、EXE とローダーの間に余分なフォルダーを挟んだりしないでください。
4. 以下の配置と照合します。EXE と Data フォルダーはゲーム本体のもので、セットはローダー、ランタイム、プラグイン、設定を追加します。

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

5. 普段のゲーム EXE を起動します。初回はネット接続で Unity 基礎ライブラリを取得し、`BepInEx/interop` を生成するため、数分かかる場合があります。待機中に複数のゲームを起動しないでください。ログ用コンソールが表示されないのは正常です。
6. ゲームプレイに入ると一人称が初期状態で有効です。ゲームを前面にして **F8** の視点切替、**F9** の髪の表示切替を確認します。プラグインログには `Loaded v0.1.28` が記録されます。読み込み成功だけで全機能の動作が確認できるわけではありません。

### プラグインの導入・更新（BepInEx 導入済み）

1. セーブして終了し、古い DLL と `BepInEx/config/local.lillia.firstpersontest.cfg` を `plugins` の外へバックアップします。
2. `BepInEx/plugins/FirstPersonTest.dll` を置き換えます。サブフォルダーも含めて一つだけにし、バックアップ DLL を残さないでください。既存ローダーと設定は保持します。
3. 単体プラグインの初回導入では一度起動して設定を生成し、終了します。生成直後の `ClipLimits` は空欄ですが、同梱セットには 4 項目が設定済みです。
4. DLL の更新だけでは新しい設定例は**自動追加されません**。`[EventView]` の既存 `ClipLimits` 一行に、下記 4 項目の例を統合し、自分の設定も残します。同じクリップ名は一度だけ記述し、値全体を一行に保ちます。今回の追加は `Fellatio_Female(Gsun)_Loop|3|2|2` です。
5. 保存してゲームを再起動してください。設定は起動時に読み込まれます。今回のセット改訂は設定と説明のみで、0.1.28 DLL は変更していません。

### トラブル対処

| 症状 | 最初に確認すること |
| --- | --- |
| 一人称にならない／F8 が反応しない | ゲームプレイに入り、ウィンドウを前面にします。フォルダー階層と DLL の重複を確認し、ローダー・プラグインログの読み込み結果やエラーを調べます。 |
| 初回起動が止まって見える | `BepInEx/LogOutput.log` でダウンロードや interop 生成が進んでいるか確認します。取得失敗なら具体的なエラーと接続を確認してから再試行し、起動を繰り返さないでください。 |
| `Character content could not be loaded` | Unity ログの `A local absolute bundle path is required` を探します。あれば終了してゲーム全体を短いパスへ移動します。画面の表示だけではパスが原因と断定できません。続く場合はログを保存してください。 |
| イベント視点の可動範囲が広い | `Event view: clip=...` の正確な名前、対応する ClipLimits、再起動の有無を確認します。大文字・小文字を区別し、未一致のクリップには共通範囲が適用されます。 |
| 髪や NPC が視界を遮る | F9 で髪を切り替え、遮られるイベントでは F8 で三人称に戻します。 |

ローダーログ：`BepInEx/LogOutput.log`。プラグインログ：`BepInEx/FirstPersonTest.log`。Unity ログ：`%USERPROFILE%\AppData\LocalLow\PaconeSoft\Novice Succubus Lillia\Player.log`。再起動前に必要なログを保存し、通常の導入手順としてセーブやキャッシュを削除しないでください。

一時停止は F8、アンインストールはゲーム終了後に `BepInEx/plugins/FirstPersonTest.dll` のみを削除します。他の MOD が使うローダーは残してください。旧版へ戻す場合はバックアップした DLL と設定を復元します。ダウンロードの照合には `SHA256SUMS.txt` を使えます。

## スクリーンショット

### 一人称 — 髪を非表示

![一人称 — 髪を非表示](docs/screenshots/first-person-hair-hidden.png)

髪に遮られない一人称の街並み。左側にはゲーム本来のキャラクタープレビューが表示されます。

### 一人称 — 髪を表示

![一人称 — 髪を表示](docs/screenshots/first-person-hair-visible.png)

髪が視界を遮る場合があります。**F9** で一人称視点の髪の表示・非表示を切り替えられます。

### 三人称

![三人称](docs/screenshots/third-person.png)

**F8** で一人称と三人称を切り替えられます。

視点の違いを示す画像であり、同条件での性能比較ではありません。

## 0.1.28 の動作

- 体は視線の方向を向き、W/A/S/D は視線を基準とした移動だけを操作します。
- 走れるのは前方と斜め前方のみです。横、後方、斜め後方への移動は歩行になります。
- アニメーション・IK の後、髪の物理演算の前に頭の姿勢を確定し、メイン画面と隅のキャラクタープレビューで共有します。
- プレビュー専用の 65° 制限、姿勢の差し替え、カメラごとの強制スキニング更新を削除しました。
- 待機中の頭の動きへの視点追従、操作時の待機モーション中断、イベント中のアニメーション基準の視野制限を維持しています。
- 頭部による視界遮蔽の処理、F9 による髪の表示切り替え、一人称中の壁のフェード抑制も維持しています。

歩行アニメーションの移動量を保ったまま、入力方向へ向きを補正します。斜面、衝突、外力はゲーム本来の移動処理に任せます。移動速度、スタミナ、空中移動、重力、ゲーム素材は置き換えません。

**検証状況：** ゲームから生成されたアセンブリを使ってビルドし、方向変換、制御の復元、姿勢更新順序、待機・イベントのシミュレーションテストに合格しています。ゲーム 1.1 での実機確認では正常動作と体感上の滑らかさの改善が報告されています。フレームレートの測定や全シーン・全イベントの確認は行っていません。

## 操作

| 入力 | 動作 |
| --- | --- |
| F8 | 一人称のオン／オフ。オフにするとゲーム本来の操作に戻ります |
| F9 | 一人称中の髪の表示／非表示。その起動中のみ有効です |
| マウス | 上下左右を見る |
| W/A/S/D | 視線を基準に移動。移動入力による体の方向転換は行いません |
| Shift + W、W+A、W+D | 走行を要求。スタミナや走行可否はゲームが判定します |

キーはゲームが前面にあるときだけ反応します。イベント中の自由視点では右クリックを押し続ける必要はありません。カーソルロックや UI イベントシステムは変更しません。

## 設定

ゲーム終了後に `BepInEx/config/local.lillia.firstpersontest.cfg` を編集し、再起動してください。

| セクション / キー | 初期値 | 説明 |
| --- | --- | --- |
| View / EyeHeightAdjustment | -0.01 | 頭のローカル座標での目の高さ補正。-0.03～0.03 |
| View / LookUpDegrees | 75 | 上を向く最大角度。0～89° |
| View / LookDownDegrees | 85 | 下を向く最大角度。0～89° |
| View / PitchSensitivity | 3 | 通常時の上下視点感度 |
| View / InvertY | false | 上下操作を反転 |
| Body / FollowCameraYaw | true | 移動方向と独立して体を視線方向へ向ける。統一した制御のためオンを維持 |
| Body / FollowHeadPitch | true | 頭を視線に合わせる。姿勢の統一のためオンを維持 |
| Body / HideHeadInFirstPerson | true | 検出した頭部遮蔽マテリアルスロットをメイン画面で非表示。プレビューには表示 |
| Body / HideHairOnStart | false | 起動時に髪を非表示。F9 で一時的に切り替え可能 |
| Body / ShowPlayerRenderers | true | 有効な体の Renderer をメイン画面に表示 |
| Body / NearClip | 0.02 | ニアクリップ距離。0.005～0.1。元の値より大きくしません |
| Body / HeadMaterialSlots | 空欄 | 上級設定。正確な RendererName:スロット番号をセミコロンで区切る |
| EventView / DefaultYaw | 75 | イベント中、アニメーションの頭の向きを基準とした左右の範囲 |
| EventView / Sensitivity | 2 | イベント中の自由視点感度 |
| EventView / ClipLimits | 空欄 | アニメーションクリップごとの左右／上／下の制限 |

書式は `ClipName|左右|上|下;OtherClip|左右|上|下`。各角度は 0～89° です。`0|0|0` なら完全にアニメーションに追従します。クリップ別の制限が標準の左右 75° より優先されます。

### ClipLimits の設定例

**DLL 自体にクリップ別の制限は内蔵されておらず、単体版で新規生成する ClipLimits は空欄です。新規導入セットには下記 3 件を設定済みです。** 初期設定では全クリップに左右各 75°、上 75°、下 85° の共通範囲が使われます。設定を変更した場合はその値が適用されます。DLL を入れるだけでは、以下の制限は追加されません。

1. プラグインを入れて一度起動し、設定ファイル生成後に終了します。
2. `BepInEx/config/local.lillia.firstpersontest.cfg` の既存の `[EventView]` を探します。
3. その中の `ClipLimits =` 行を下の例に置き換えます。セクションやキーを重複させず、値は改行せずに半角 `;` と `|` で区切ってください。
4. 保存してゲームを再起動します。

以下はゲーム 1.1 のカタログにある同系統の 4 クリップを対象とする設定です。

```ini
[EventView]
DefaultYaw = 75
ClipLimits = Fellatio_Female_Loop|3|2|2;Fellatio_Female(bellied man)_Loop|3|2|2;Fellatio_Female_End|3|2|2;Fellatio_Female(Gsun)_Loop|3|2|2
```

`3|2|2` はアニメーションの頭の向きに対し左右各 3°、上 2°、下 2° を許容し、ほぼその動きに追従します。この狭い制限は指定した 4 クリップだけに適用され、それ以外は共通範囲のままです。完全に追従させるには、そのクリップの値を `0|0|0` にします。

既存の独自設定がある場合は削除せず、同じ値の中に半角セミコロンで結合してください。同じクリップは一度だけ記載します。名前は大文字・小文字を区別し、ログの `Event view: clip=...` と完全一致が必要です。ゲーム更新で名前が変わった場合は、新しいログに合わせて修正してください。

旧設定の `LookDownClearance`、`LookDownForward`、`LookDownUp`、`HeadPitchWeight` は使われません。目の位置を追加で前にずらす処理はありません。待機アニメーションの視点遷移・回転フィルターは維持し、マウス操作にプレビュー専用の角度制限やフィルターを追加していません。

## 既知の制限

- 使用可能で互換性のある横歩き、後退、その場での旋回アニメーションは見つかっていません。移動方向の補正だけでは足の動きは作れないため、足滑りが生じる場合があります。
- イベント中の体はアニメーションが制御します。NPC で視界が完全に遮られる場合は F8 で三人称に戻してください。
- 極端な角度ではモデル内部が見える場合があります。動作に応じて制限を調整してください。
- ゲーム固有のカメラ名、コントローラー、骨格、更新順序に依存するため、ゲーム更新後は再確認が必要です。
- 髪の物理パラメーターは変更していません。性能のベンチマークは行っていません。

## 動作確認の項目

1. マウスを止めて W/A/D/S と斜め移動を試し、体が入力方向へ回らず正しく移動するか確認。
2. 上下を向いて歩き始め・停止を繰り返し、メイン画面、隅のモデル、髪の整合を確認。
3. Shift+W、Shift+W+A/D は走行可能、Shift+A/D/S は歩行のままか確認。
4. 待機動作を待ってからマウスを動かし、視点追従と待機動作の中断を確認。
5. イベント開始・終了時に体のアニメーション制御、個別制限、終了後の通常移動を確認。
6. F8 のオン／オフやシーン切り替え後に制御が復元されるか確認。

## ログ

通常は `BepInEx/FirstPersonTest.log`、読み込み失敗時は `BepInEx/LogOutput.log` を確認してください。

- `Loaded v0.1.28`：バージョンと起動状態。
- `Body follow: SetInputs observed`：入力コールバックの受信。
- `Locomotion:`：入力方向、アニメーション移動量、補正後のローカル移動量。最大 5 秒に一度。
- `Unified pose:`：姿勢確定の段階。髪の物理演算が有効なら `BeforeHairPhysics`。
- `Pose:`：毎秒 1 フレームを抽出し、体・頭・カメラの更新順序を確認。
- `Idle control:` / `Event view:`：待機中断とイベント制限。
- `disabled after control error` / `Retrying after error`：エラーと復旧。

ログは上限付きのバックグラウンドファイルキューを使い、毎フレーム同期的にコンソール出力しません。

## ソースからビルド

.NET 6 対応 SDK とゲームで生成した BepInEx の `core`・`interop` を使用します。

```powershell
dotnet build .\FirstPersonTest.csproj -c Release '-p:BepInExDir=D:\Games\Lillia\BepInEx'
```

出力先は `bin/Release/net6.0/FirstPersonTest.dll` です。ソース ZIP にゲームのアセンブリ、素材、BepInEx 依存ファイルは含みません。実装メモは [IMPLEMENTATION.ja.md](IMPLEMENTATION.ja.md)を参照してください。

## アンインストール

ゲームを終了してプラグイン DLL を削除します。旧版へ戻す場合は保存済みの DLL に置き換え、設定ファイルを残してください。
