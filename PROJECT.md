# Unity-dev — HD-2D RPG プロジェクト仕様書

最終更新: 2026-06-10

HD-2D（オクトパストラベラー風）スタイルの探索型RPG。3Dキューブ地形にピクセルアートをテクスチャ貼りし、キャラクターは2Dスプライトのビルボードで表現する。シーンは手作業ではなく**エディタスクリプトでプロシージャル生成**するのが本プロジェクトの最大の特徴。

---

## 1. 環境・技術スタック

| 項目 | 内容 |
|---|---|
| エンジン | Unity 6 (`6000.4.9f1`／実環境は `6000.4.10f1` で動作) |
| レンダリング | URP (Universal Render Pipeline) 17.4.0 |
| 入力 | Input System 1.19.0（新Input System） |
| 主要パッケージ | AI Navigation, Timeline, Visual Scripting, Test Framework |
| AI連携 | `com.coplaydev.unity-mcp`（MCP for Unity）— Claude等から Unity Editor を操作 |
| プラットフォーム | Windows（開発機） |

### MCP連携（AI開発）について
- MCPサーバーは Python 製で `uv` が必要。`Window → MCP for Unity → Start Server` で起動。
- クライアント（Claude Code）は HTTP transport（`http://127.0.0.1:8080/mcp`）で接続。
- 起動手順: ①Unity を開く ②MCPサーバーを Start ③Claude Code をリロード。

---

## 2. ディレクトリ構成

```
Assets/
├── Scenes/
│   ├── SampleScene.unity        … メインシーン（村「HD2D Valley」）
│   └── CaveScene.unity          … ウリセン討伐クエストの洞窟（迷路）→ §9
├── Scripts/                     … ランタイムスクリプト（下記 §4）
│   └── Editor/                  … エディタ拡張（シーン/テクスチャ生成）
├── Materials/                   … マテリアル（SpriteLitMat ほか地形・布など）
├── Textures/                    … コード生成されたピクセルアート（地形/キャラ/敵）
├── Settings/                    … URP アセット（PC/モバイル）, ポストプロセス
└── Screenshots/                 … 動作確認用スクショ出力先
```

---

## 3. シーン構造（SampleScene → "HD2D Valley"）

ルート直下:
- **Main Camera** — `CinematicCameraController`（28度見下ろし・プレイヤー追従）
- **Directional Light** — 太陽光（昼夜サイクルで制御）
- **HD2D Valley** — 村本体（`DayNightCycle` を保持）。主な子オブジェクト:

| グループ | 内容 |
|---|---|
| Blocks | 約5000個のキューブ地形 |
| TierColliders | 段差の当たり判定 |
| RiverWater / Bridges | 川（`WaterFlowAnimator`）と橋 |
| Village_Buildings / Interior_Rooms | 建物5棟・室内4部屋 |
| Trees / LushFlora / FlowerBeds | 木31本・草花161・花壇 |
| Lanterns / Campfire / Crates_And_Barrels / Signposts 等 | 小物 |
| NPC各種 | 商人・冒険者・衛兵・吟遊詩人ほか（会話・巡回） |
| **村長 Chief Ronald** | `QuestGiver`（ウリセン討伐クエストの進行・洞窟入口を兼ねる）→ §9 |
| **全快ポイント** | `HealPoint`（話すとHP全回復）→ §9 |
| **タク** | ロッカーNPC（選択肢会話＋戦闘トリガー）→ §5・§6 |
| Player | プレイヤーキャラ |
| Global Volume | ポストプロセス |
| **BattleSystem** | 戦闘エンジン（各シーンに1つ配置・永続化しない。数値は GameManager 参照）→ §6 |

> **実行時に自動生成される永続オブジェクト（`DontDestroyOnLoad`）**：`GameManager`（プレイヤーデータ/クエスト/シーン遷移）と `InventoryUI`（`I`キーのメニュー）は `[RuntimeInitializeOnLoadMethod]` でシーン非依存にブートストラップされ、村・洞窟をまたいで常駐する（→ §9）。

---

## 4. スクリプト一覧

### ランタイム（[Assets/Scripts/](Assets/Scripts/)）

| スクリプト | 役割 |
|---|---|
| [HD2DPlayerController.cs](Assets/Scripts/HD2DPlayerController.cs) | プレイヤー移動（WASD/矢印）＋8方向スプライトアニメ |
| [HD2DNPCController.cs](Assets/Scripts/HD2DNPCController.cs) | NPCのウェイポイント巡回AI＋会話中はプレイヤーを向く |
| [HD2DBillboard.cs](Assets/Scripts/HD2DBillboard.cs) | 円筒ビルボード（常にカメラを向く）＋風揺れ |
| [CinematicCameraController.cs](Assets/Scripts/CinematicCameraController.cs) | 見下ろし追従カメラ |
| [DayNightCycle.cs](Assets/Scripts/DayNightCycle.cs) | 昼夜サイクル・ランタン点灯（`T`キー） |
| [NPCDialogue.cs](Assets/Scripts/NPCDialogue.cs) | 順送り会話（タイプライター吹き出し、`E`で進行） |
| [NPCChoiceDialogue.cs](Assets/Scripts/NPCChoiceDialogue.cs) | **選択肢分岐会話**（はい/いいえ）＋戦闘フック → §5 |
| [InteractiveChest.cs](Assets/Scripts/InteractiveChest.cs) | 宝箱開封（`E`）＋浮遊テキスト演出 |
| [HouseDoorTransition.cs](Assets/Scripts/HouseDoorTransition.cs) | ドアでの室内テレポート（`Enter`） |
| [WaterFlowAnimator.cs](Assets/Scripts/WaterFlowAnimator.cs) | 水面テクスチャのスクロール |
| [PulsingMistyPrompt.cs](Assets/Scripts/PulsingMistyPrompt.cs) | プロンプトの浮遊・脈動演出 |
| [TriggerForwarder.cs](Assets/Scripts/TriggerForwarder.cs) | 子トリガーの判定を親へ転送 |
| [BattleSystem.cs](Assets/Scripts/BattleSystem.cs) | **ターン制戦闘エンジン**（GM参照・経験値付与・コールバック・アイテムコマンド）→ §6 |
| [GameManager.cs](Assets/Scripts/GameManager.cs) | **永続シングルトン**：PlayerStats(HP/攻/防/**Lv/EXP**)・インベントリ・装備・クエスト状態・シーン遷移&スポーン → §9 |
| [Item.cs](Assets/Scripts/Item.cs) | アイテム定義（武器=攻+/防具=防+/やくそう=回復）＋固定DB `ItemDatabase` |
| [InventoryUI.cs](Assets/Scripts/InventoryUI.cs) | **簡易インベントリ/ステータスUI**（`I`開閉・手動装備/使用・Lv/EXP/HP表示、IMGUI、全シーン常駐） |
| [QuestGiver.cs](Assets/Scripts/QuestGiver.cs) | 村長のクエスト進行役（QuestStateで会話分岐・受注で洞窟へ自動移動・撃破後お礼）→ §9 |
| [OverworldEnemy.cs](Assets/Scripts/OverworldEnemy.cs) | 洞窟の徘徊敵（巡回＋接触戦闘・連戦防止・勝利で消滅・`isBoss`でボス兼用）→ §9 |
| [HealPoint.cs](Assets/Scripts/HealPoint.cs) | 全快ポイント（範囲内`E`でHP全回復＋浮遊テキスト） |
| [WarpTrigger.cs](Assets/Scripts/WarpTrigger.cs) | 洞窟出口ポータル（接触で村長前へ帰還） |

### エディタ拡張（[Assets/Scripts/Editor/](Assets/Scripts/Editor/)）

| スクリプト | メニュー | 役割 |
|---|---|---|
| [HD2DTextureGenerator.cs](Assets/Scripts/Editor/HD2DTextureGenerator.cs) | `Tools/HD-2D/Generate Textures` | 地形・布・キャラ等のピクセルアートをコード生成 |
| [HD2DSceneBuilder.cs](Assets/Scripts/Editor/HD2DSceneBuilder.cs) | `Tools/HD-2D/Build Scene` | 村全体（地形・建物・NPC・小物）をプロシージャル配置 |

> シーンが空/壊れた場合は `Generate Textures` → `Build Scene` の順で再生成できる。

---

## 5. 会話システム（選択肢分岐）

[NPCChoiceDialogue.cs](Assets/Scripts/NPCChoiceDialogue.cs) — 汎用の「はい/いいえ分岐つき会話」コンポーネント。

**操作:** 近づくと頭上に「…」表示 → `E`で開始/送り → 最後に選択肢「`Y` はい / `N` いいえ」→ 結果セリフ → `E`で閉じる。

**主な公開フィールド:**
- `introLines` … 導入セリフ（複数ページ、`E`で送る）
- `choicePrompt` … 選択肢の表示文
- `yesLine` / `noLine` … 通常時の結果セリフ
- `maxCharsPerLine` … 吹き出しの折り返し文字数（CJK対応の文字数折り返し）
- 戦闘フック: `startBattleOnYes`, `enemyName`, `enemyHealth`, `enemyAtk`, `enemyDef`, `winMessage`

**特徴:** 日本語（スペース無し）でも文字数で折り返す `WrapText` を実装。戦闘中は会話入力を抑止。

### 現在の設定NPC: 「タク」
- ロッカー風NPC。画像は [Assets/Textures/npc_rocker.png](Assets/Textures/npc_rocker.png)（コード生成）。
- 暴れ馬の調教を依頼する会話。`はい`で**戦闘開始**、`いいえ`で「困ったなあ」。
- 勝利メッセージ（タク固有）: 「勝利！ 暴れ馬を 調教した！ タクは大よろこびだ！」

---

## 6. 戦闘システム（仕様）

[BattleSystem.cs](Assets/Scripts/BattleSystem.cs) — フロントビュー・ターン制コマンドバトル。**汎用エンジン**（固有のセリフ・固有名詞を持たない）。

### 形式
- **その場で戦闘**（シーン遷移なし）。IMGUI（OnGUI）でオーバーレイ描画 → Canvas不要。
- **黒背景＋敵画像**のフロントビュー（味方は非表示）。敵画像は `enemyTexture` で差し替え可能（現在はタクの画像を流用）。

### 起動
```
StartBattle(enemyName, enemyHealth, eAtk, eDef, expGain, winMsg, onWin=null, onLose=null)
```
- 村のタクは `NPCChoiceDialogue`（暴れ馬戦）、洞窟の徘徊敵・ボスは `OverworldEnemy`（接触戦闘）が呼び出す。
- **プレイヤー数値は `GameManager` から取得**（装備込み `TotalAtk`/`TotalDef`・`curHealth`）。戦闘後の現在ヘルスは GM に書き戻して**持ち越し**。
- 勝利で `expGain` の経験値を付与（GM が Lvアップ処理）。`onWin`/`onLose` で接触戦闘の後処理（敵消滅・村帰還＋全回復・クエスト更新）を配線。

### ステータス
各戦闘員（`Unit`）は **ヘルス（最大/現在）・攻撃力・防御力**を持つ。

### ダメージ計算
```
ダメージ = 攻撃力 − 防御力
```
- 防御力が攻撃力を n（≧1）上回ると、**確率 1 − 1/2ⁿ でミス**
  （超過1→1/2、超過2→3/4、超過3→7/8 …）
- 命中時のダメージは**最低1**を保証。

### コマンド（プレイヤーターン）
- **たたかう** … 上記式でダメージ
- **アイテム** … やくそうを1つ使ってHP回復（1ターン消費。所持品は GM から消費。無いとターン消費なしで「やくそうが ない！」）
- **にげる** … 50%で離脱（失敗時は敵の攻撃を受ける）

### ターン進行・終了条件
- プレイヤー行動 → 敵行動 の交互。
- ヘルス**0で戦闘不能（KO）**。
- **片方の陣営が全員KOで戦闘終了**（内部は List で複数戦闘員に拡張可能。現状は1対1）。
- 結果: 勝利／敗北／逃走 → 「とじる」で村へ復帰（プレイヤー操作を再有効化）。

### 数値設定（村のタク戦）
| 戦闘員 | ヘルス | 攻撃力 | 防御力 |
|---|---|---|---|
| 蓮（プレイヤー Lv1・装備なし） | 5 | 1 | 0 |
| 暴れ馬（敵） | 3 | 1 | 0 |

→ 防御0のため現状ミスは発生せず、毎ターン1ダメージ。先攻の蓮が3ターンで勝利する素直なバランス。プレイヤー数値は GM（Lv/装備）に依存し、敵数値は呼び出し側が指定する。ウリセン討伐クエストの戦闘員数値は §9 を参照。

### 設計上の責務分担
- **BattleSystem** = 汎用戦闘エンジン（固有名詞・固有セリフを持たない。勝利メッセージのデフォルトは「勝利！」）
- **各NPC（NPCChoiceDialogue）** = その戦闘固有のデータ（敵名・ステータス・勝利セリフ）を保持してエンジンに渡す

---

## 7. 操作方法（Play中）

| キー | 動作 |
|---|---|
| WASD / 矢印 | 移動 |
| E | 会話・宝箱・全快ポイント／会話送り |
| Y / N | 会話の選択肢「はい／いいえ」 |
| Enter | ドアで室内へ |
| I | インベントリ/ステータスUI 開閉（Esc でも閉じる）→ §9 |
| T | 昼夜切り替え |
| （戦闘中）マウス | 「たたかう」「アイテム」「にげる」「とじる」ボタン |

---

## 8. 既知の制約・今後の拡張候補

- 戦闘UI・インベントリUIは IMGUI（OnGUI）製。MCPの「カメラ描画」スクショには映らない（実機の Game ビューには表示される）。
- 敗北/逃走セリフは現状共通（「敗北……」「逃走した」）。勝利同様に個別化可能。
- 戦闘は1対1。`Unit` リスト構造のため複数戦闘員・陣営戦に拡張可能。
- アイテム種は固定DB（`ItemDatabase`）の5種のみ。洞窟は最小迷路（5×5）・徘徊敵2体＝縦切り構成で、肉付け余地あり。
- 未実装: セーブ/ロード、サウンド、タイトル/メニューUI。
- 敵・ボス画像はタクの流用（仕様上そのまま）。`enemy_horse.png` は試作・未使用。

---

## 9. ウリセン討伐クエスト（村長 → 洞窟 → ボス）

村長に依頼を受け、村はずれの洞窟（別シーン `CaveScene`）でウリセンを討伐し、最奥のボス「タクヤ」を倒して村へ帰還するクエスト一式。詳細な開発計画・進捗は [EVENT_URISEN_PLAN.md](EVENT_URISEN_PLAN.md)。

### クエスト状態機械（`GameManager.QuestState`）
`NotOffered`（未受注）→ `Accepted`（受注・洞窟解放）→ `BossDefeated`（撃破済・未報告）→ `Completed`（お礼済）。
[QuestGiver.cs](Assets/Scripts/QuestGiver.cs)（村長 Chief Ronald）が状態で会話を分岐：
- **NotOffered**：依頼を打診 → `はい`で受注し**自動で洞窟へワープ**。
- **Accepted**：話すと再び洞窟へ入場（村側に物理ポータルは無く、**村長が入口を兼ねる**）。
- **BossDefeated**：お礼セリフ → `Completed` 化。
- **Completed**：英雄への感謝セリフ。

### シーン遷移（`GameManager`）
受注/再訪で `WarpToCave()`（洞窟入口 `EntranceSpawn` へ）、洞窟出口 [WarpTrigger.cs](Assets/Scripts/WarpTrigger.cs) で `WarpToVillage()`（`ChiefReturnPoint` = 村長前へ）。ボス撃破・戦闘敗北時も村へ自動帰還。`GameManager` は `DontDestroyOnLoad` で数値・所持品を持ち越し、ロード後に Player をスポーン点へ配置して操作を再有効化。

### 洞窟（`CaveScene`）
再帰的バックトラッカーで生成した完全迷路（5×5・壁71）。南に入口ロビー、最奥(8,8)にボス。徘徊ウリセン [OverworldEnemy.cs](Assets/Scripts/OverworldEnemy.cs) **2体**（Urisen1/Urisen2）と宝箱3個（武器/防具/やくそう）を配置。Camera/Light/Player/Global Volume は村から複製済み。完全迷路のため全セルが連結し、宝箱・ボスはいずれも到達可能。

### 再生成と無限ファーム防止
洞窟は入場のたびに `LoadScene` で**まっさらに再生成**される。アイテム・経験値の無限稼ぎを防ぐため、`GameManager` が**開封済み宝箱**(`openedChests`)・**撃破済み雑魚**(`defeatedEnemies`)を GameObject 名で永続記録し、再入場時に消し込む（開封済み宝箱は開封状態のまま再取得不可、撃破済み雑魚は `Start` で自壊）。ボスはクエスト状態で管理（撃破後は再入場不可）。

### レベル / 経験値（`GameManager`）
Lv開始=1。必要EXP = **現在Lv×2**。取得EXP = 雑魚1 / ボス3。Lvアップで**最大HP+2・現在HP+2**、超過EXPは持ち越し（複数Lvアップも連続処理）。

### アイテム / 装備（[Item.cs](Assets/Scripts/Item.cs) / [InventoryUI.cs](Assets/Scripts/InventoryUI.cs)）
武器=`TotalAtk`に加算 / 防具=`TotalDef`に加算 / やくそう=HP回復。`I`キーで開くインベントリUIから**手動で装備・使用**。戦闘内でも「アイテム」コマンドでやくそう使用可。固定DB `ItemDatabase`：きのぼう(攻+1)・どうのつるぎ(攻+2)・ぬののふく(防+1)・かわのよろい(防+2)・やくそう(回復3)。

### 戦闘員の数値（縦切り時点）
| 戦闘員 | HP | 攻 | 防 | EXP |
|---|---|---|---|---|
| 蓮（プレイヤー Lv1） | 5 | 1(+武器) | 0(+防具) | — |
| ウリセン（雑魚・2体） | 3 | 1 | 0 | 各1 |
| タクヤ（ボス） | 5 | 2 | 1 | 3 |

→ **素手ではボスに勝てない**（防御貫通の最低1ダメージで撃破に5ターン要し、先に全滅）。宝箱の どうのつるぎ＋かわのよろい を装備すれば atk3/def2 となり、Lv1でも3ターンで撃破・被弾2のみで勝てる**＝装備前提のバランス**。

### 敗北 / 回復導線
敗北は村入口へ戻し **HP全回復**（ロストなし）。村に全快ポイント [HealPoint.cs](Assets/Scripts/HealPoint.cs)（話すとHP全回復）。

### 操作（クエスト関連の追加キー）
| キー | 動作 |
|---|---|
| I | インベントリ/ステータスUI 開閉（Escでも閉じる） |
| E | 村長と会話・宝箱・全快ポイント |
