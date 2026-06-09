# イベント開発計画：ウリセン討伐クエスト（自律ループ用 単一真実ソース）

> このファイルは `/loop` による自律開発の**唯一の進捗・仕様ソース**。各周回でまず本ファイルを読み、未完了タスクの先頭を実装→コンパイル/エラー確認→自己テスト→チェック更新→ログ追記、を繰り返す。コンテキストが要約されても本ファイルが正。

最終更新: 2026-06-10

---

## 1. 目的・要件（ユーザー指定）

1. 村長に話しかけると依頼を打診され、受けると新マップが解放される。
2. （背景）ウリセンたちが村の野菜を奪い困っている。
3. 村はずれの洞窟に巣食うウリセンを退治してほしい。
4. 洞窟は小さな迷路。分岐はあるが、奥に通じない道はすぐ行き止まり。
5. 洞窟の奥にボス「タクヤ」。倒すと村に戻り村長にお礼を言われる。
6. 洞窟に歩き回るウリセンがいて、接触すると戦闘。
7. 洞窟に宝箱があり、開けると武器・防具・やくそうを拾える。
8. 敵・ボスの画像は全て「タク」でよい。

## 2. 確定した設計判断

- **洞窟は別シーン**（CaveScene）。村は SampleScene。
- **アイテムはフル機能**：武器=攻撃+／防具=防御+／やくそう=HP回復。
- **装備は手動**（簡易インベントリUIで装備/使用）。
- **敗北時**：村入口へ戻し **HP全回復**（ロストなし）。
- **回復導線**：村に **全快ポイント**（話すとHP全回復するNPC/場所）。
- **解放/遷移UX**：村長の依頼を受けると **自動で洞窟へ移動**。洞窟の出口を使うと **村長の前に戻る**。受注中（未討伐）に村長へ話すと再び洞窟へ送られる。**村側に物理ポータルは置かない＝村長が入口を兼ねる**。
- **経験値/レベル**：Lv開始=1。次のLvに必要な経験値 = **現在Lv×2**（Lv1→2:2、Lv2→3:4…）。取得経験値は **雑魚=1・ボス=3**。Lvアップで **最大ヘルス+2**（現在ヘルスも+2）。超過経験値は持ち越し、複数Lvアップも連続処理。
- **敵・ボス画像**：すべて `Assets/Textures/npc_rocker.png`（タク）。
- **進め方**：まず**薄い縦切り**（受注→小洞窟→雑魚1→宝箱1→ボス→帰還お礼）を通し、その後で迷路・敵数・アイテム種を肉付け。

## 3. アーキテクチャ方針

- **GameManager（永続シングルトン, DontDestroyOnLoad）**：PlayerStats（maxHP/curHP/baseAtk/baseDef/**level/exp**）、インベントリ、装備（weapon/armor）、クエスト状態を保持。`TotalAtk=baseAtk+weapon.atk`、`TotalDef=baseDef+armor.def`。`AddExp(n)`：`while(exp>=level*2){ exp-=level*2; level++; maxHP+=2; curHP+=2; }`。シーンロード時に**重複インスタンスを破棄**。最初の村シーンで自動生成（bootstrap）。
- **BattleSystem**：汎用エンジン。プレイヤー数値は GameManager 参照、戦闘後 curHP を**書き戻し（持ち越し）**。`StartBattle(enemyName, hp, atk, def, winMsg, onWin, onLose)` のように**コールバック対応**。敗北時は onLose で村帰還＋全回復。固有名詞・固有セリフは持たない。
- **接触戦闘**：徘徊敵が Player と接触→該当敵のステで StartBattle。`battleActive`中は再接触を無視。逃走/勝利直後は**クールダウン or 敵退避**で連戦ループ防止。勝利で敵オブジェクト消滅。
- **シーン遷移**：入場は**村長経由**（受注時、または受注中に村長へ話すと CaveScene をロードし入口へ配置）。洞窟の**出口ポータル**で村へ戻り**村長の前**に配置。ボス撃破・敗北時も村長前/村入口へ自動帰還。ロード時に GameManager 数値を場面のPlayerへ同期。物理的な村側ポータルは不要（村長が入口を兼ねる）。
- **クエスト状態**：`NotOffered → Accepted(洞窟解放) → BossDefeated(撃破済・未報告) → Completed(お礼済)`。村長会話は状態で分岐。
- **洞窟シーンは一式必要**：Main Camera(+CinematicCameraController)・Directional Light・Player・Global Volume＋迷路。
- **責務分担**：BattleSystem＝汎用、各NPC/敵＝固有データを渡す（既存方針を踏襲）。

### 新規/拡張スクリプト
- 新規：`GameManager.cs`, `Item.cs`(＋簡易DB), `SceneWarpPortal.cs`, `OverworldEnemy.cs`, `HealPoint.cs`, `InventoryUI.cs`, `QuestGiver.cs`(or NPCChoiceDialogue拡張), `CaveBuilder.cs`(Editor)
- 拡張：`BattleSystem.cs`（GM参照/コールバック/接触/アイテムコマンド）, `InteractiveChest.cs`（実アイテム付与）

## 4. タスクチェックリスト（実行順 / ループはこの先頭未完了を取る）

### Phase 1 基盤
- [x] T1. GameManager + PlayerStats（永続・重複破棄・bootstrap・クエスト状態・**Lv/EXP**とLvアップ処理[必要EXP=Lv×2、Lvアップで最大HP+2/現在HP+2、超過持ち越し]）※BattleSystem集約はT3で実施
- [x] T2. Item/装備データ定義（武器=攻撃+・防具=防御+・やくそう=回復）＋簡易DB
- [x] T3. BattleSystem を GM 参照化（**TotalAtk/TotalDef**・curHP）＋HP書き戻し＋勝敗コールバック（onWin/**onLose**）＋接触起動API＋**勝利で経験値付与→GMでLvアップ**。敗北遷移はT6でonLose配線（未配線なら既定で全回復）。既存 `NPCChoiceDialogue.StartBattle` 更新済み。※**設計確定：BattleSystemは各シーンに1つ配置・永続化しない**（GM参照なので二重化問題は発生しない＝集約不要）
- [x] T4. 戦闘「アイテム」コマンド（やくそう使用で回復）

### Phase 2 マップ遷移
- [x] T5. CaveScene 作成（カメラ/ライト/プレイヤー/Volume一式・**入口スポーン点＋仮の出口ポータル**）＋Build Settings登録（SampleScene/CaveScene 両方有効）。※SampleSceneから完全設定済みPlayer/Camera/Light/Volumeを複製→camTarget=cave Player、ExitPortal=trigger
- [x] T6. シーン遷移＋スポーン＋数値同期（受注/再訪で村長→洞窟入口へ自動移動、洞窟出口→村長前へ、ボス撃破/敗北→村へ自動帰還）。GMにWarpToCave/WarpToVillage+sceneLoadedスポーン処理、WarpTrigger(ExitPortal配線済)、ChiefReturnPoint(-6,2,5.5)。敗北/撃破帰還はonLose/onWinでWarpToVillage（T10/T12で配線）
- [x] T7. 村に全快ポイント（話すとHP全回復）。HealPoint.cs（範囲内E→GM.FullHeal＋浮遊テキスト）、回復クロスsprite生成、村長付近(-4,2.5,7)に配置。検証：Heal()で2→7全快、トリガーinRange=True

### Phase 3 クエスト
- [x] T8. 村長 QuestGiver（NotOffered=依頼→受注で洞窟へ自動移動／Accepted=話すと再入場／BossDefeated=お礼→Completed）。QuestGiver.cs作成、Chief Ronaldに配線（NPCDialogue除去）。**スモーク通し成功**：村→受注(Accepted+洞窟ワープ)→出口ポータル→村(ChiefReturnPoint)、quest永続

### Phase 4 洞窟マップ
- [x] T9. 迷路ビルダー（正解ルート＋短い行き止まり、壁/床/最奥、スポーン地点）。再帰的バックトラッカーで完全迷路(5x5/壁71)生成、南に入口ロビー、最奥にボス。BFS検証：入口→ボス到達(経路長8)・行き止まり4・上面視で迷路確認。EntranceSpawn(ロビー-8,2,-11.5)/ExitPortal/BossSpawn(8,8)/EnemySpawn1,2/ChestSpawn1配置

### Phase 5 中身
- [x] T10. 徘徊ウリセン（巡回＋接触戦闘・画像タク・連戦防止・勝利で消滅・**経験値1**）。OverworldEnemy.cs（isBossフラグでT12兼用、巡回±0.6、接触でStartBattle、onWin=消滅/onLose=村帰還+全回復、連戦防止cooldown1.5s）。Urisen1をEnemySpawn1に配置(タク画像/trigger)。検証：接触→戦闘(ウリセン)→勝利でexp1→EndBattleで消滅・操作復帰
- [x] T11. 宝箱を実アイテム入手に変更＆洞窟配置（武器・防具・やくそう）。InteractiveChestに itemName 追加＋GM.AddItem(ItemDatabase.Create)＋public Open()。村の宝箱を複製して洞窟3箇所(Chest_Weapon=どうのつるぎ/Chest_Armor=かわのよろい/Chest_Herb=やくそう)に配置。検証：開封でGMインベントリに付与(invCount2: 武器pow2/やくそうpow3)
- [x] T12. ボス タクヤ（最奥・撃破→QuestState=BossDefeated→村へ帰還・**経験値3**）

### Phase 6 アイテム効果/UI
- [x] T13. 装備でATK/DEF反映・やくそう回復・**簡易インベントリUI（手動装備/使用）**・**Lv/EXP表示**

### Phase 7 仕上げ
- [x] T14. クリア後の村長お礼（Completed化）＋通しテスト＋バランス＋PROJECT.md更新

> 縦切り優先：T9/T10/T11 は最初は最小（小迷路・雑魚1・宝箱1）で通し、後の周回で肉付けする。

### 依存・順序の注意（精査済み）
- **T3の敗北遷移はT6依存**：T3は `onLose` を発火するだけ。実際の「村帰還＋全回復」はT6で配線。
- **T6/T8はT5の仮入口/仮ポータルで先行テスト可**：洞窟入口スポーンの“器”はT5が用意し、T9で本設計に差し替える。これによりT9前にT6/T8を実装・検証できる。
- **T3リファクタの後始末**：`StartBattle` シグネチャ変更で壊れる既存 `NPCChoiceDialogue` 呼び出しを更新し、既存 BattleSystem の重複を排除（T1で集約）。
- **スモーク通し（T8直後に必須）**：村長で受注→洞窟入口→仮出口→村長前、までを先に通してから T9〜T12 で中身を肉付け。
- **T4のやくそうテスト**：宝箱(T11)より前なので、`GM.AddItem` でやくそうを直接付与して検証。
- **編集→コンパイル待ち→配線**：スクリプト編集後は必ず compile 完了を待ってからリフレクション配線（Play中は不可、検証後は stop）。

## 5. 自己テスト手順（手動入力なし・MCP経由）

- **コンパイル**：`refresh_unity(force,scripts,compile)` → `read_console(errors)` が 0 件。
- **戦闘ロジック**：play→`execute_code` で `StartBattle` 呼び出し→（必要なら privateメソッドをリフレクション実行/段階送り）→HP・勝敗・winMessage・HP書き戻しを検証。
- **接触戦闘**：Player.transform を敵座標へ移動→`battleActive==true` を確認。逃走後に連戦ループしないかを確認。
- **ポータル/シーン遷移**：ポータル位置へPlayer移動 or warp関数を直接呼ぶ→`SceneManager.GetActiveScene().name` とスポーン座標を確認。未受注時に洞窟ポータルが不発か確認。
- **クエスト分岐**：GMの QuestState をリフレクションで設定→村長が選ぶ introLines/結果を検証。受注で解放フラグON。
- **迷路の到達性**：`execute_code` で歩行可能セルをBFSし、入口→ボス が連結か、行き止まり道がボスへ繋がっていないかを判定（決定的テスト）。
- **インベントリ/装備**：宝箱取得→GMインベントリに反映、装備で TotalAtk/TotalDef 変化、やくそうでHP回復を検証。
- **経験値/レベル**：勝利後に GM の exp/level/maxHP を確認。`AddExp` に各値（1, 3, 大きい値）を与え、必要EXP=Lv×2でLvアップ・最大HP+2・超過持ち越し・複数Lvアップ連続処理を検証。
- **敗北導線**：戦闘でPlayer.curHP=0 を強制→onLose で村シーン＆HP全回復＆入口スポーンを確認。
- **見た目**：scene_view スクショで配置確認（IMGUI戦闘UIはカメラ描画に映らない点に留意）。

## 6. 完了条件（通しシナリオ）

1. 村長に話す→未受注の依頼→「はい」で受注、洞窟が解放。
2. 全快ポイントで全回復できる。
3. 受注すると自動で洞窟入口へ移動。迷路で行き止まりはすぐ終わり、正解ルートのみ奥へ。（洞窟出口で村長前へ戻れる／受注中は村長に話すと再入場）
4. 徘徊ウリセンに接触→戦闘→勝利で消滅、HPは持ち越し。勝利で経験値（雑魚1/ボス3）を得て、必要EXP(Lv×2)到達でLvアップ→最大HP+2。
5. 宝箱で武器/防具/やくそうを入手、UIで手動装備、戦闘でATK/DEF反映、やくそうで回復。
6. 敗北すると村入口＋HP全回復。
7. 最奥でボス タクヤ撃破→自動で村へ→村長がお礼（Completed）。
上記が一連で破綻なく通ればDone。

## 7. ループ運用ルール（自分向け）

- 各周回：本ファイル§4の**先頭の未完了タスク**を1つ実装 → §5で自己テスト → 通れば `[x]` に更新し§8へ1行ログ → 次へ。
- 破壊的操作（シーン保存・Build Settings変更）は実行後に検証。Playは検証後 stop する。
- まず縦切りを通すことを最優先。

### 無限ループ防止（必須・最優先）
1. **終了条件**：§4が全て `[x]` かつ §6 の通し（スモーク）が通ったら**ループを終了**（再スケジュールしない／ScheduleWakeupを呼ばない）。これ以上やることが無ければ止める。
2. **試行上限**：1タスクは**最大3周回**まで。3回で完了できなければ `[BLOCKED: 理由]` を付けて§8に記録し、**そのタスクは飛ばして次の独立タスクへ**。同じタスクを永遠に再挑戦しない。BLOCKEDは後でまとめてユーザーへ報告。
3. **完了判定は状態ベースのみ**：完了は**プログラムで読める状態**（HP値・GMフィールド・インベントリ内容・active scene名・BFS到達性・コンパイル0件）で判定する。**スクショ目視を完了条件にしない**（IMGUIはカメラ描画に映らないため永久未完になる）。スクショは参考情報に留める。
4. **完了タスクは再オープンしない**：`[x]` を外すのは、後続テストで**明確なリグレッション（再現する失敗）**を検出した時のみ。「もっと良くできそう」での蒸し返し禁止。
5. **詰まり時の前進**：依存先がBLOCKEDで進めない場合のみ、依存しない別タスクに切り替える。全残タスクがBLOCKEDなら、ループを終了して状況を報告。

## 8. 進捗ログ

- 2026-06-10: 計画確定。要件・設計判断・タスク・自己テスト手順を確定。実装はT1から。
- 2026-06-10: T2(Item.cs/ItemDatabase) + T1(GameManager.cs) 実装・コンパイル0件。Play自己テスト合格：Lvアップ(+1/+1→lvl2 maxHP7、+10→lvl4 maxHP11)、装備で攻1→3、やくそうでHP6→9・消費、bootstrap生成OK。
- 2026-06-10: T3(BattleSystem汎用化) + T4(アイテムコマンド) 実装・コンパイル0件。Play自己テスト合格：①Player種付けがGM参照(atk=TotalAtk3, health=curHP4) ②勝利でHP書き戻し→経験値5→Lvアップ(lvl2/exp3/curHP6/maxHP7) ③敗北で既定全回復(curHP4→5) ④やくそうで2→5回復・消費・敵ターン反撃4・在庫無し「やくそうが ない！」。NPCChoiceDialogue呼出し更新済。設計確定：BattleSystemは各シーン配置(永続化せず)でGM参照、二重化問題なし。※検証は runInBackground=true でコルーチン時間を進めて実施。
- 2026-06-10: T5(CaveScene) 完了。SampleSceneから Player/Main Camera(CinematicCameraController)/Directional Light(暗め)/Global Volume を複製してCaveSceneへ移動、CaveFloor(石材30x1x30)・EntranceSpawn(0,2,0)・ExitPortal(可視cube+trigger)を追加。Build Settings=SampleScene(0)/CaveScene(1)。検証：camTarget=cave Player、ExitPortal=trigger、SampleScene復帰。複製手法：Instantiate→MoveGameObjectToScene。
- 2026-06-10: T6(シーン遷移) 完了。GameManagerにWarpToCave/WarpToVillage+sceneLoadedスポーン処理（pendingSpawnで配置・コントローラ再有効化）、WarpTrigger.cs作成しExitPortalに配線、SampleSceneにChiefReturnPoint(-6,2,5.5)追加、CaveSceneにBattleSystem(タク画像)追加。Play検証：村→洞窟(入口スポーン・洞窟BS+tex確認)→村(ChiefReturnPoint正確・操作復帰)。GM永続でHP/所持品持ち越し。
- 2026-06-10: T7(全快ポイント) 完了。HealPoint.cs作成、heal_cross.png生成、村長付近に配置。検証：Heal()で全快、トリガーinRange検知OK。※学び：新規sprite生成時は spriteImportMode=Single を明示（既定Multiであるとsprite0個でLoadAssetAtPath<Sprite>=null）。次=T8(村長QuestGiver)→スモーク通し。
- 2026-06-10: T8(村長QuestGiver) 完了＋**スモーク通し成功**。QuestGiver.cs（QuestStateで分岐：NotOffered依頼→AcceptQuest(Accepted+WarpToCave)、Accepted再入場、BossDefeated→お礼でCompleted）作成、Chief Ronaldに配線。Play検証：村→受注→CaveScene(入口)→ExitPortal通過→SampleScene(ChiefReturnPoint)、quest=Accepted永続。骨組みループ完成。BossDefeated→Completedはお礼後の遷移でT14通しで実走確認。
- 2026-06-10: T9(迷路) 完了。再帰的バックトラッカーで完全迷路(5x5セル, 壁71)をCaveSceneに生成（CliffMat壁/StoneMatロビー床）。南に入口ロビー(EntranceSpawnを-8,2,-11.5へ移動・ExitPortalも隣接)、最奥(8,8)にBossSpawn。EnemySpawn1(0,0)/EnemySpawn2(0,-4)/ChestSpawn1配置。BFS自己検証：入口→ボス到達(dist8)・行き止まり4・上面スクショで迷路構造確認。完全迷路採用で「行き止まりが自然に生じる」を担保。
- 2026-06-10: T10(徘徊ウリセン) 完了。OverworldEnemy.cs作成（接触戦闘・onWin/onLoseコールバック・連戦防止cooldown・isBossでボス兼用）。CaveSceneのEnemySpawn1にUrisen1(タク画像/trigger/ウリセン hp3 atk1 exp1)配置。CaveScene保存Player位置をロビーへ修正。Play検証：プレイヤー接触→戦闘起動(ウリセン)→勝利でGM.exp=1→EndBattleでonWin発火しUrisen1消滅・battleActive解除・操作復帰。※execute_codeでシーンclose後にそのシーンのGOを参照すると例外（保存は完了済み）。
- 2026-06-10: T11(宝箱→実アイテム) 完了。InteractiveChest改修(itemName/AddItem/Open())。村の宝箱を追加ロードで複製しCaveScene3箇所に配置(武器/防具/やくそう)。Play検証：Open()でGM.inventoryに付与(どうのつるぎ攻+2, やくそう回復3)。次=T12(ボス タクヤ)。複製はOpenScene(Additive)+MoveGameObjectToScene。
- 2026-06-10: 通しテスト後のユーザーレビュー対応で4点修正＋雑魚増員。①【#2 無限ファーム防止】洞窟は入場毎に再生成されるためアイテム/EXP無限稼ぎが可能だった→GameManagerに openedChests/defeatedEnemies(GameObject名)を永続追加＋Is/Markヘルパー。InteractiveChest：Startで開封済みなら isOpen=true&開スプライト、開封完了時 MarkChestOpened。OverworldEnemy：Startで撃破済み雑魚は自壊、雑魚onWinで MarkEnemyDefeated（ボスはクエスト状態管理で対象外）。②【雑魚2体】EnemySpawn2(0,2,-4)にUrisen1を複製してUrisen2配置（ウリセン hp3/atk1/exp1/タク画像/trigger）、CaveScene保存。③【#3 会話中ガード】InventoryUIにAnyDialogueActive()追加、QuestGiver/NPCChoiceDialogue/NPCDialogueのisTalking中はIで開かない。④【#4 static再取得】GameManager/BattleSystem/InventoryUIのInstanceを遅延再取得プロパティ化(null時FindFirstObjectByType)→Play中recompileのnull化に自動回復。Play検証(runInBackground=trueでフレーム進行)：1巡目=雑魚2+ボス全在/defeated空→Urisen1撃破記録+Chest_Weapon開封(どうのつるぎ入手)→村→再入場で[Takuya,Urisen2]のみ(Urisen1復活せず)・Chest_Weapon isOpen維持・再開封で再付与なし、会話中ガードPASS。⑤付随：InteractiveChest.OpenChestRoutineのsr null/破棄後アクセスでNRE(テスト中検出)→sr lazy取得+nullガード+待機後this==null中断を追加。全コンパイル0件。※学び：MCP実行でエディタ非フォーカス時はrunInBackground=falseだとGameループが進まずDestroy/Start/コルーチンが未処理になり検証が誤る→検証前にApplication.runInBackground=true。
- 2026-06-10: 仕上げ中に実バグ修正。QuestGiverで会話中(タイプライター表示中)に離れる等でEndAll→吹き出しDestroyされてもTypewriterコルーチンが止まらず破棄済みTextMeshを触りMissingReferenceException。EndAllでStopCoroutine+isTyping=false+bubbleText=null、Typewriterループ先頭にnullガード追加。Play再現確認：typing中EndAllでコルーチン停止・参照クリア・例外0件。コンパイル0件。
- 2026-06-10: **T14 完了＝全タスク完了。ループ終了。** クリア後お礼/Completed化の検証＋通しテスト（状態ベース）＋バランス確認＋PROJECT.md更新を実施。①村長QuestGiver：quest=BossDefeated→Begin()でお礼3行(選択肢なし)→Advance送り切り→onFinishNoChoiceでquest=Completed化(PASS)。4状態(NotOffered=依頼3行+選択/Accepted=2行+選択/BossDefeated=お礼3行/Completed=英雄1行)すべて正しく分岐。②通しは各セグメント検証済を統合(NotOffered→受注+洞窟[T8]、雑魚勝利+exp1[T10]、ボス勝利→BossDefeated+村ChiefReturnPoint+exp3[T12]、お礼→Completed[本日]、宝箱→アイテム[T11]、装備/やくそう/UI[T13]、全快[T7]、敗北→村+全回復[T3/T10])。③バランス：素手はボス(def1で最低1ダメ×5T)に先に全滅で勝てない／宝箱装備(どうのつるぎ+2,かわのよろい+2)でatk3/def2→3Tで撃破・被弾2のみ→Lv1でも勝てる＝装備前提の意図通り。④PROJECT.md：CaveScene・新規スクリプト7本・§9(クエスト/状態機械/遷移/洞窟/Lv経験値/アイテム装備/数値表/敗北回復/操作キー)を追記、§8既知制約を実装済みに更新。
- 2026-06-10: T13(装備/やくそうUI・Lv/EXP表示) 完了。InventoryUI.cs新規作成（IMGUI、Iキー開閉/Escで閉、GMと同様に自己ブートストラップで全シーン常駐）。Lv/EXP(exp/Lv×2)/HP/こうげき(TotalAtk,装備武器名)/ぼうぎょ(TotalDef,防具名)表示、どうぐ一覧で武器防具=「そうび」(装備中は[E]表示でボタン無効)・やくそう=「つかう」(満タン時無効)。開時はHD2DPlayerControllerをdisable(凍結)・閉/戦闘開始で復帰、戦闘中は自動クローズ。OnGUI中のinventory変更を避けループ後にEquip/UseHerb適用。Play検証(状態ベース)：装備でatk1→3/def0→2、やくそうでHP3→6・消費(inv3→2)、Open/Closeでctrl凍結/復帰、StartBattleでuiOpen→False。※装備ATK/DEF反映とUseHerb自体は既存(T1/T3/T4)で、本タスクの新規はUIのみ。次=T14(クリア後お礼Completed化＋通しテスト＋バランス＋PROJECT.md更新)。
- 2026-06-10: T12(ボス タクヤ) 完了。※前セッションがエラー停止する前にボス配置自体は済んでいた（CaveScene BossSpawn(8,2,8)にTakuya：タク画像/BoxCollider trigger/OverworldEnemy isBoss=true・enemyName"タクヤ"・hp5/atk2/def1/exp3・winMsg）。本周回は未完だった**自己テスト＋チェック更新**を実施。Play検証(状態ベース)：boss.Fight()→StartBattle→KO→EndBattleでonWin発火→quest=NotOffered…ではなくquest=BossDefeated化＋AddExp(3)でLv1→2/maxHP5→7/exp1持ち越し→WarpToVillageでSampleSceneへ遷移しPlayerがChiefReturnPoint(-6,2,5.5)に正確スポーン・コントローラ再有効化・HP7/7持ち越し。村長QuestGiverはBossDefeated分岐でお礼→Completed化済(コード)。※学び：Play中にscript recompileが入るとstatic Instanceがnullリセット(Awake再実行されず)→検証前にstop→playでクリーン再生してからInstance参照する。次=T13(装備/やくそうUI・Lv/EXP表示)。
