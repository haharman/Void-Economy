# Orrarium 基盤・ツール分析 — 汎用ライブラリ切り出し候補レポート

調査対象: `D:\void-economy\Void Economy`(git ブランチ `feature/npc-trade`、HEAD `5adaef8`)
調査方法: 実コードの直接読解(Read/Grep/Glob/diff)。以下の記述はすべて実際に読んだファイル・行番号を根拠とする。推測で書いた箇所はない。

> **前提の注記:** リポジトリ直下の `CLAUDE.md` は `GlobalRoot`/`GlobalStateModel`/`PlayerModel`/`YarnModel`/`Root.asmdef` 等、現在のコードには存在しないクラス名・アセンブリ構成を記述している(設計初期の構想と実装が分岐した状態)。本レポートは `CLAUDE.md` の記述ではなく、`Assets/Scripts` 配下の実ファイルを読んだ結果に基づく。

---

## タスク1: 現状分析

### 1.1 プロジェクト全体の構成

**アセンブリ定義(`Assets/Scripts/**/*.asmdef`):** 5個。CLAUDE.md が言う `Root` アセンブリは存在せず、Root系スクリプト(`SystemRoot`/`OrreryRoot`/`PlanetRoot`)は `Service.asmdef` に同居している。

| アセンブリ | rootNamespace | 参照 | 主な中身 |
|---|---|---|---|
| `Core` | `Core` | なし | `SpacePosition`/`SurfacePosition`/`CoordPos`/`Enums`/`DeltaTimer`/`Utils`/`SaveData`/`AudioDataSO` |
| `Model` | `Model` | Core, R3.Unity, R3.Unity.TextMeshPro, Unity.Burst, YarnSpinner.Unity, Unity.Localization | `Economy/`(Wallet, Inventory, ItemRegistry)、`Orrery/`(OrreryModel, PlanetModel, JetpackModel, QuestModel, InteractionGuideModel)、`EntityModel` |
| `Presenter` | `Presenter` | GUID参照でCore/Model/Viewの一部/R3等 | `Orrery/`(Player, Wallet, Inventory, Camera, Audio, Dialogue)、`Planet/`、`System/MenuPresenter` |
| `View` | `View` | Core/Model/Presenter系GUID + DOTween.dll | `Orrery/`(Inventory, Trade, Wallet, Player, Yarn, Cinemachine)、`Planet/`、`System/`(Audio, Input, Menu)、`UI/Window` |
| `Service` | `""`(無名) | Core/Model/View/Presenter系GUID + R3/YarnSpinner/DOTween | `SystemRoot`/`OrreryRoot`/`PlanetRoot`(シーンのブートストラップ)、`SaveDataService/`、`SceneService`/`UpdateService`/`InputService`/`ConfigDataService` |

依存方向は `View → Presenter → Model → Core` を概ね維持しているが、`Service` が Presenter/View/Model の全てに依存する形で全レイヤーの配線役を担っている(`CLAUDE.md` の「Root」に相当する層)。

**ディレクトリ規模:** `Assets/Scripts` 配下 `.cs` は87ファイル(Core 9 / Model 24 / Presenter 20 / View 22 / Service 12)。

**テスト:** `Assets/Tests/EditMode` に2アセンブリ(`Economy.Tests.EditMode`, `Inventory.Tests.EditMode`)。詳細は1.6節。

**その他:** `Assets/Editor`(エディタ拡張4ファイル)、`Assets/Scripts/.claude/settings.local.json`(Claude Code のローカル権限設定)、`Assets/Scripts/Service/SaveDataService/CLAUDE.md` と `Assets/Scripts/View/System/MenuView.md` という**ディレクトリ単位のドキュメントが実在**(後述)。`product-docs/`・`notes/` は現時点でリポジトリに存在しない(`CLAUDE.md` が言及するのみ)。

### 1.2 既に実装されている基盤的な仕組み

- **セーブ/ロード(`Service/SaveDataService/`):** `SaveDataService` が `ISaveDataWriter`/`ISaveDataReader` を `HashSet` で登録管理し、`Save()`/`Load()` 時に全登録者へブロードキャストする(SaveDataService.cs:44-62, 96-139)。`JsonDataStorage` がスロット管理(手動1/オート5、JsonDataStorage.cs:12-13)とファイルI/Oを担当。バージョン文字列配列によるマイグレーションチェーンも実装済み(SaveDataService.cs:169-190)。**このサブシステムには専用の `CLAUDE.md` が既に書かれており、既知バグまで文書化されている**(1.7節参照)。
- **手動アップデートディスパッチ(`Service/UpdateService.cs` + `Presenter/IUpdatable.cs`):** `MonoBehaviour.Update()` に頼らず、`IUpdatable` を `HashSet` に登録し、追加/削除を次フレームまで遅延させる形でイテレーション中の例外を回避、かつ `IUpdatable` ごとに `try/catch` で例外を分離している(UpdateService.cs:16-30, 53-67)。ドメイン知識ゼロの完全に汎用的な実装。
- **シーン遷移(`Service/SceneService.cs`):** `ISceneState`/`ISceneLoader` を実装し、`LoadingStarted`/`LoadingCompleted` イベントで非同期ロードを通知する薄いラッパー。
- **入力(`Service/InputService.cs`):** `InputView` からのC#イベントを `ReactiveProperty<InputState>` に変換し、`InputState`(Player/Dialogue/UI)に応じて移動入力をルーティングする。
- **設定管理:** `Service/ConfigDataService.cs` は**中身が空のクラス**(ConfigDataService.cs:1-7)。`SystemRoot.Bootstrap()` 内でもコメントアウトされたまま呼ばれていない(SystemRoot.cs:69-70)。→ 1.4/タスク2で詳述。
- **エディタ拡張:** `Assets/Editor/ButtonEditor.cs` が `[CustomEditor(typeof(MonoBehaviour), true)]` で全MonoBehaviourに `[Button]` 属性メソッド実行ボタンをリフレクションで生やす(ButtonEditor.cs:6-27)。加えて `InspectorReadOnlyDrawer`(読み取り専用フィールド表示)、`ListLabelAttributeDrawer`(リスト要素のラベルを内部フィールド値に置換)という2つの `PropertyDrawer` が既にある。属性自体は `Assets/Scripts/View/ButtonAttribute.cs` 等、ランタイム側(View.asmdef)に定義されている。
- **DI/シングルトン:** DIコンテナは使われていない。`SystemRoot`/`OrreryRoot`/`PlanetRoot` がコンストラクタ引数で手動配線するパターンで統一されており、`static Instance` 型のシングルトンは検出されなかった(全文grep済み、0件)。CLAUDE.mdの「シングルトンなし」方針は実コードでも一貫している。
- **デバッグ機能:** 専用のデバッグウィンドウ等はなし。`[Button]` 属性によるInspector実行ボタンが唯一のランタイムデバッグ支援。ログは全て `Debug.Log`/`Debug.LogError` の手書き。

### 1.3 使用している外部ライブラリとその用途(`Packages/manifest.json` 実読)

| パッケージ | 用途(実コードでの使用箇所) |
|---|---|
| `com.cysharp.r3` | リアクティブプロパティ／イベント。`Model`層の公開状態(`ReadOnlyReactiveProperty<int> Balance` 等)、`Presenter`層の購読(`RegisterTo(cancellationToken)` パターンが31箇所・15ファイルで使用)。 |
| `dev.yarnspinner.unity` | ダイアログ。`DialoguePresenter`/`YarnView`、`Assets/Yarn/*.yarn` |
| `com.unity.localization` | `ItemDefinitionSo.displayName`/`flavorText` の `LocalizedString`、`Assets/Localization/*Table*`(ja/en) |
| `com.unity.cinemachine` | `CameraPresenter`/`CinemachineView` |
| `com.unity.inputsystem` | `InputService`/`InputView`。`Assets/Samples/Input System/.../Visualizers` サンプルも同梱 |
| `com.unity.timeline` | パッケージのみ導入、`Assets/Scripts` 内での参照コードは未検出 |
| `com.unity.2d.*`(animation/aseprite/psdimporter/sprite/spriteshape/tilemap/tooling) | ピクセルアート取り込み・アニメーション(`Graphics`アセット向け) |
| `com.github-glitchenzo.nugetforunity` | NuGetパッケージ導入補助(`Microsoft.Bcl.*`, `System.Threading.Channels` 等が `Assets/Packages/` に展開済み＝R3の依存関係) |
| DOTween(`Assets/Plugins/Demigiant/`、UPM外の有料アセット) | `View`/`Service` 双方で `.DOKill()`/`.DOAnchorPosY()`/`.DOFade()`/`.SetLink(gameObject)` パターンが多用(`InventoryView`, `TradeView`, `AudioView`, `WalletView` 等) |
| `com.unity.render-pipelines.universal`(URP) | レンダリングパイプライン(本レポートのスクリプト読解範囲では直接コードから未参照、Shaderアセット側で使用と推定) |
| `com.unity.addressables` | **パッケージは導入済みだが `Assets/Scripts` 内で `Addressables.`/`AssetReference` の使用は0件**(`Assets/AddressableAssetsData` はデフォルト生成物のみ)。導入されているが未活用、または将来のための先行導入。 |

**UniTask(Cysharp)は導入されていない。** 非同期は素の `CancellationToken` + R3 の `RegisterTo` で統一されており、`Presenter`層のコンストラクタで `CancellationToken` を受け取るパターンが繰り返されている。

### 1.4 同じパターンが繰り返し手書きされている箇所(抽象化されていない箇所)

1. **`TradeView.cs` は `InventoryView.cs` のクラス名以外バイト単位で同一。** `diff` で確認済み(差分は InventoryView.cs:13 と TradeView.cs:13 のクラス名1行のみ、137行中136行が完全一致)。現在の作業ブランチでまさに新規追加されたファイル(`git status`: `AM Assets/Scripts/View/Orrery/TradeView.cs`)であり、「選択バー付きスクロールリスト＋詳細パネル」というUIコンポーネントが未抽出のまま2回目のコピペが発生した瞬間そのもの。
2. **階層的な `IUpdatable` 登録/解除が3階層で手書きされている。** `SystemRoot`(`_updateService`→`_systemUpdateService`、SystemRoot.cs:98-99, 124)、`OrreryRoot`(`_orreryUpdateService` に4Presenterを登録、OrreryRoot.cs:134-137、`Terminate()`で対称的にUnregister、156)、`PlanetRoot`(`_planetUpdateService`、PlanetRoot.cs:56-58)が、Register/Unregisterの対称性を毎回人力で維持している。
3. **`Debug.Log($"[ClassName] ...")` という手動タグ付けログが86箇所・30ファイル**(grep実測)に散在し、クラス名変更に追従できていない箇所がある。例:`SystemRoot.cs` は元 `GlobalRoot` という名前だった痕跡で、現在も `[GlobalRoot]` というタグでログを出し続けている(SystemRoot.cs:45, 53, 67, 90, 115)。
4. **DOTweenの「Kill→再生→SetLink」という定型パターン**が呼び出し側ごとに手書きされている(InventoryView.cs:122-125、WalletView.cs:24-30、AudioView.cs:79-87)。共通ヘルパーは存在しない。
5. **「前の状態を保存して後で復元する」という push/pop パターンが2箇所で個別実装。** `MenuPresenter._previousInputState`(MenuPresenter.cs:28, 133)と `DialoguePresenter._previousInputState`(DialoguePresenter.cs:17, 48)が同じ「`InputState` を退避して戻す」ロジックを独立に持つ。
6. **`ScriptableObject` の重複キー検証がランタイムの `Debug.LogError` のみ。** `ItemRegistry` コンストラクタが `ItemId` 重複を実行時に検出(ItemRegistry.cs:14-19)。エディタタイムでの検証(Inspector上での警告表示やビルド前チェック)は存在しない。
7. **`[SerializeField]` の未設定チェックが呼び出し箇所ごとに手書き。** `OrreryRoot.Bootstrap()`/`Initialize()` 内で `planetRoot == null` チェックが3箇所に分散(OrreryRoot.cs:94, 118, 143)、`PlanetRoot.OnValidate()` は3フィールド分の `GetComponentInChildren` 自動アタッチ＋警告を個別に書いている(PlanetRoot.cs:72-94)。
8. **シェーダー側にも同種の重複がある。** `ProcedualNoice*.shader` が4バリアント(無印/_Sprite/_URP/_URP_Fixed、合計718行)存在し、収束していない試行錯誤の跡が残っている(1.7節・追加検討14参照)。

### 1.5 「TODO」「FIXME」「とりあえず」「仮」等のコメントが集中している領域

全文grep(`TODO|FIXME|とりあえず|仮実装|暫定`)でヒットしたのは実質1件のみ:`AudioView.PlayMusic()` の `fadeDuration` 引数が未実装という明記(AudioView.cs:48)。

ただし、コメント形式のTODOは少ない一方で、**未実装が構造的に露呈している箇所**が複数ある(TODOタグは付いていないが実質的に同種):

- `Model/Orrery/QuestModel.cs`: `MainQuestManager`/`MainQuestFactory`/`TradeQuestFactory` が中身の無い空クラスとして定義されている(QuestModel.cs:12-32)。
- `Service/ConfigDataService.cs`: 空クラス、呼び出し元でコメントアウト(1.2節参照)。
- `Presenter/System/MenuPresenter.cs`: `HandleSettingsChanged()` が空実装(MenuPresenter.cs:123-125)。
- `Assets/Scripts/View/System/MenuView.md`: 音量5種・ウィンドウモード・UIスケール・キーコンフィグ・言語・オートセーブ間隔という設定画面の**仕様書だけが存在し実装コードが1行もない**(Settings関連の実装は `MenuPresenter`/`MenuView.cs` に見当たらない)。
- `View/Orrery/YarnView.cs`: 選択肢フォーカス機能の中身がコメントアウトされたまま空実装(YarnView.cs:26-34)。
- `Presenter/Orrery/PlayerPresenter.cs`: `Dispose()` に「// 未配線」とコメントがあり、実際どこからも呼ばれていない(PlayerPresenter.cs:96-99)。
- `Assets/Shaders/srs_fog_shader.md`: 霧シェーダーの要件定義メモが文の途中で途切れている(追加検討14参照)。
- `View/Planet/WaterSurfaceView.cs`: 対応するシェーダー(`WaterSurfaceURP.shader`)は完成度が高いにもかかわらず、C#側の`SpawnRipple()`が空実装のまま(追加検討14参照)。

### 1.6 テストの有無と範囲

- テストアセンブリは2つ(`Economy.Tests.EditMode`, `Inventory.Tests.EditMode`)、テストファイルは2つのみ。
- **`WalletModelTests.cs`は7ケースの実働テスト**(初期残高、Deposit、TryPay成功/ちょうど/不足、SetBalance、Subscribe経由の変化観測)。`WalletModel` は薄いクラスだが、契約(`GetResult`/`SetResult`スタイル)に沿ったテストの書き方の good example になっている。
- **`InventoryModelTests.cs` は中身が全部コメントアウトされており、実質0テスト**(InventoryModelTests.cs:8-55)。`InventoryModel` は `TryGet`/`Get`/`TrySet`/`Set` という4つの契約的メソッド(`GetResult`/`SetResult` enum付き、InventoryModel.cs:9-23)を持つ、CLAUDE.mdが定義するSDDフロー的にテスト化しやすい設計になっているにもかかわらず未着手。
- `OrreryModel`(重力・座標変換という数式ロジックの塊)、`JetpackModel`(物理更新)、`EntityModel`、`SaveDataService` 等、ロジックの複雑さの割にテストが存在しないクラスが多数。
- Playモードテストは0。

### 1.7 補足:既存ドキュメント・シェーダー資産の状態

- `Assets/Scripts/Service/SaveDataService/CLAUDE.md`: セーブ機能専用のドキュメントが既に存在し、**ロードスロット選択が実質機能しないバグ**(`SystemRoot.OnLoadingCompleted` が常に `Load(newGame:true)` を呼ぶため選択したスロットが無視される、SystemRoot.cs:121)まで明文化されている。
- `Assets/Scripts/View/System/MenuView.md`: 設定画面(音量/キーバインド/言語/UIスケール等)の詳細仕様書。実装コードは存在しない。
- `Assets/Shaders/srs_fog_shader.md`: 霧シェーダーの要件定義メモ(未完成、追加検討14参照)。
- シェーダー資産: `Atmosphere.shader`(151行、霧+砂埃のフルスクリーンポストエフェクト、コメントに「Claudeによって生成されたテストシェーダー」「Fogを調整中」と明記)、`Dither.shader`(50行、Bayerディザ、同じく「テストシェーダー」)、`WaterSurfaceURP.shader`(236行、波紋システム込みの本格実装)、`ProcedualNoice*.shader`(4バリアント、収束していない試作)、`Scanline.shader`+`ScanlineFeature.cs`/`ScanlinePass.cs`(URP ScriptableRendererFeatureによるCRT走査線効果、ドメイン非依存の汎用ポストエフェクト)。星空/starfield関連のコードはリポジトリ全文検索で0件。

---

## タスク2: 欠けているものの洗い出し

いずれも「ゲーム内容」ではなく「基盤・仕組み・ツール」レイヤー。番号は評価表・タスク4と対応。#13・#14はユーザーからの追加依頼を受けて後日追加した候補。

1. **階層的Update配信ライブラリ(Hierarchical Update Dispatcher)** — UpdateService.cs が既に3階層ネストで手書き運用されており(1.4-2節)、優先度順序・一時停止・実行順序の保証がない。ドメイン非依存なので今すぐ切り出せる。
2. **クラス名自動タグ付けロガー** — 86箇所・30ファイルに手書きタグ付きログが散在し、リネーム追従漏れ(`[GlobalRoot]`)が実際に発生済み(1.4-3節)。`[Conditional]`属性でリリースビルドから除去できればGC改善にもなる(追加検討「GC削減」参照)。
3. **汎用マルチライター・セーブフレームワーク** — SaveDataService.cs は Writer/Reader登録・JSON永続化・バージョン移行チェーンを備えるが `SaveData` 型に直結。既存の `Assets/Scripts/Service/SaveDataService/CLAUDE.md` に**ロードスロット選択が実質機能しないバグが明文化**されている。修正と汎用化を同時に行える。
4. **属性ベースInspector拡張キット** — ButtonEditor.cs/`InspectorReadOnlyDrawer`/`ListLabelAttributeDrawer` という3点セットが既に存在するが、複数属性の合成(Unityの「1フィールド1PropertyDrawer」制約)、`[Required]`等の検証系属性が未整備。
5. **リアクティブ設定値ストア(Settings/Preferences)** — ConfigDataService.cs が空クラスのまま放置され、SystemRoot.cs:70 でインスタンス化がコメントアウト。MenuView.md に音量/キーバインド/言語/UIスケール等の詳細仕様が書かれているのに実装コードが存在しない、最も明確な「仕様はあるが基盤がない」ギャップ。
6. **選択式リスト/詳細パネルUIコンポーネント** — InventoryView.cs と TradeView.cs がクラス名以外完全一致(1.4-1節、`diff`で確認済み)。今まさに二重化している。
7. **DOTween⇄R3 ブリッジ** — 「Kill→Tween→SetLink」の定型処理が4箇所以上で個別に手書きされている(1.4-4節)。
8. **ScriptableObject整合性バリデータ** — ItemRegistry.cs:14-19 の重複ID検出が実行時ログのみで、エディタ上での事前検証がない。`AudioDataSO`の `List.Find` も、該当IDが無い場合デフォルト値を無言で返す同種のリスク。
9. **シーンRootライフサイクルヘルパー** — `SystemRoot`/`OrreryRoot`/`PlanetRoot` が `Bootstrap→Initialize→Terminate` の3フェーズと Register/Unregister対称性を個別実装(1.4-2, 1.4-7節)。ただしVContainerの `LifetimeScope` と機能的に衝突するため優先度は低い。
10. **入力モード状態スタック(`InputState` push/pop)** — `MenuPresenter` と `DialoguePresenter` が同じ「前状態退避→復元」パターンを個別実装(1.4-5節)。単体では小さすぎるため他ライブラリへの機能同梱を推奨。
11. **`[SerializeField]` Null検証/自動ワイヤ属性** — `OrreryRoot`/`PlanetRoot` の手書きnullチェック・自動アタッチ(1.4-7節)。4番と統合可能。
12. **未参照コード検出(参考程度)** — OldParallaxView.cs(238行)はシーン・プレハブ・他スクリプトのどこからも参照されていない完全なデッドコード(grep実測、参照0件)。`InventoryModelTests.cs` も実質デッドコード(1.6節)。Unity公式の Project Auditor と機能が重複するため単独ライブラリ化は非推奨。
13. **PlanetParallax(惑星曲面ラップ対応パララックスシステム)** — 追加検討参照。既に高度に実装されているが未整理点(死んだ旧実装、未配線メソッド)がある。
14. **シェーダー集(水面・霧・星空)** — 追加検討参照。水面シェーダーは実装済みだがC#配線が空、霧は試作段階、星空は未着手。

---

## タスク3: ポートフォリオ適性評価

5段階評価(5=最も適性が高い)。個々のスコアは1.1〜1.7の根拠に基づく判断であり、機械的な採点ではない。

| # | 候補 | 切り出し やすさ | 一言性 | 可視性 | 設計の 余地 | 規模の 妥当性 | 実用性 | 志望領域 との接続 | 既存代替 の不在 | 合計 |
|---|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| 13 | **PlanetParallax** | 5 | 5 | 5 | 4 | 4 | 5 | 4 | 5 | **37** |
| 4 | 属性ベースInspector拡張キット | 5 | 5 | 5 | 4 | 4 | 5 | 5 | 3 | 36 |
| 1 | 階層的Update配信ライブラリ | 5 | 5 | 3 | 4 | 5 | 5 | 4 | 4 | 35 |
| 5 | リアクティブ設定値ストア | 5 | 5 | 4 | 4 | 4 | 5 | 3 | 4 | 34 |
| 14 | シェーダー集(水面・霧・星空) | 4 | 4 | 5 | 4 | 3 | 4 | 5 | 3 | 32 |
| 6 | 選択式リストUIコンポーネント | 4 | 5 | 5 | 3 | 3 | 5 | 4 | 3 | 32 |
| 3 | 汎用マルチライター・セーブフレームワーク | 3 | 4 | 3 | 5 | 4 | 5 | 3 | 3 | 30 |
| 11 | SerializeField検証/自動ワイヤ属性 | 4 | 4 | 4 | 3 | 3 | 4 | 5 | 3 | 30 |
| 8 | SOバリデータ | 4 | 4 | 3 | 3 | 3 | 4 | 4 | 3 | 28 |
| 2 | 自動タグ付けロガー(GC対応版) | 5 | 5 | 2 | 3 | 2 | 5 | 3 | 3 | 28 |
| 10 | 入力モード状態スタック | 5 | 4 | 2 | 3 | 1 | 4 | 2 | 4 | 25 |
| 9 | シーンRootライフサイクルヘルパー | 3 | 3 | 2 | 4 | 3 | 4 | 3 | 2 | 24 |
| 7 | DOTween⇄R3ブリッジ | 5 | 4 | 2 | 2 | 1 | 3 | 2 | 2 | 21 |
| 12 | 未参照コード検出 | 3 | 4 | 3 | 2 | 2 | 3 | 3 | 1 | 21 |

**採点上の注記:**
- #4「既存代替の不在」が3止まりなのは、NaughtyAttributes(人気OSS・無料)やOdin Inspector(有料)という直接競合が存在するため。README で明確な差別化(複数属性合成の設計)を書かないとポートフォリオとして弱くなる。
- #13「PlanetParallax」が全候補中最高点なのは、円周ラップ対応の曲面ワールド用パララックスという類似OSSがほとんど無いニッチ(既存代替の不在=5)に、実用性・可視性の高さが重なるため。
- #14「シェーダー集」は志望領域(Unity/リアルタイム描画/HLSL)との接続が全候補中最高だが、水面・霧・星空単体は無料アセットとの競合が最も激しい領域でもある(既存代替の不在=3)。差別化には「曲面ワールド前提」という軸が必要。
- #9 はVContainerの `LifetimeScope` と機能が重なるため意図的に低評価(依頼の評価基準どおり)。
- #10・#2・#7 は単体では規模(200行未満に収まりやすい)・可視性で不利なため、他ライブラリへの機能同梱を推奨し、単独の上位候補からは外した。
- #6 は可視性・実用性で最高点帯だが、EnhancedScroller等の既存UGUIスクロールリスト資産と競合しうる(H=3)ため僅差で5位タイ。
- GC削減ライブラリは単独候補として不成立と判断し採点を割愛した(下記「追加検討」参照)。

---

## タスク4: 推奨(総合評価上位3つ)

### 推奨1: 属性ベースInspector拡張キット(合計36点)

**何を作るか:** Odin Inspector非依存で、複数のカスタム属性(`[Button]`/`[ReadOnly]`/`[Required]`/`[ShowIf]`等)を1フィールドに重ねて使える、合成可能なUnity Inspector拡張ライブラリ。

**解決するOrrariumの課題:**
- ButtonEditor.cs:6 は `[CustomEditor(typeof(MonoBehaviour), true)]` という**全MonoBehaviourを対象にする広すぎるCustomEditor**で実装されており、将来他のCustomEditorを個別クラスに書いた瞬間に衝突する設計上の地雷になっている。
- OrreryRoot.cs:94,118,143 や PlanetRoot.cs:72-94 の手書きnullチェック／自動アタッチは `[Required]`／自動ワイヤ属性で代替でき、Play前にInspector上で赤く警告できる(現状はPlay後にコンソールへ`Debug.LogError`が出るだけ)。
- ItemRegistry.cs:14-19 のようなID重複検証を `[Unique]` 属性としてこの基盤に載せられる。

**想定public API:**
```csharp
// Runtime(UnityEditor非依存アセンブリ)
[AttributeUsage(AttributeTargets.Method)]
public sealed class ButtonAttribute : PropertyAttribute { public string Label; }

[AttributeUsage(AttributeTargets.Field)]
public sealed class ReadOnlyAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public sealed class RequiredAttribute : PropertyAttribute { public string Message; }

[AttributeUsage(AttributeTargets.Field)]
public sealed class ShowIfAttribute : PropertyAttribute
{
    public ShowIfAttribute(string conditionMemberName) { ... }
}

// Editor側:複数属性の合成を担う中核インターフェース
public interface IAttributeDrawer
{
    int Order { get; }
    float GetExtraHeight(SerializedProperty property, PropertyAttribute attribute);
    void OnGUI(ref Rect rect, SerializedProperty property, PropertyAttribute attribute);
}

public static class AttributeDrawerRegistry
{
    public static void Register<TAttr>(IAttributeDrawer drawer) where TAttr : PropertyAttribute;
}
```

**設計上、判断が必要になる論点:**
1. 同一フィールドに複数属性(例: `[Required][ShowIf(...)]`)が付いた場合の描画順序・高さ計算の合成方法(UnityはPropertyDrawerを1属性1つしか自動解決しないため独自ディスパッチが必要)。
2. 現状のような「全MonoBehaviour対象のCustomEditor」を維持するか、属性検出のたびにDrawerを解決するプラガブルな設計にして、利用側が独自CustomEditorを書いても壊れないようにするか。
3. リフレクション(`GetMethods`/`GetCustomAttribute`)を毎GUI呼び出しで行うか、型ごとにキャッシュするか(エディタのフレームレート・大量オブジェクト選択時の負荷に直結)。
4. ランタイム属性定義アセンブリとエディタ専用Drawer実装アセンブリの分離(現状 `ButtonAttribute` 等は `View.asmdef` に同居しており、独立ライブラリ化する際はビルドに含まれないEditor専用asmdefへ切り離す設計判断が要る)。

**READMEでの見せ方:** Before/AfterのInspectorスクリーンショット(未設定必須フィールドが赤くハイライトされる、`[Button]`でPlay中に関数を叩ける)、複数属性を1フィールドに重ねるGIF、属性を1行足すだけのコード例。NaughtyAttributes等との違い(合成ロジック)を明記する。

**想定実装コスト:** 12〜18時間(属性3〜4種＋合成機構＋サンプルシーン＋README)。

---

### 推奨2: 階層的Update配信ライブラリ(合計35点)

**何を作るか:** `MonoBehaviour.Update()` に依存せず、優先度・一時停止・階層グループ化・例外分離を備えた `IUpdatable` 登録ベースのアップデートループ管理ライブラリ。

**解決するOrrariumの課題:**
- 既に UpdateService.cs がドメイン非依存の実装として存在し、`SystemRoot`(SystemRoot.cs:98-99,124)・`OrreryRoot`(OrreryRoot.cs:134-137,151,156)・`PlanetRoot`(PlanetRoot.cs:56-58,65-67)の3階層で同じRegister/Unregisterパターンが手書きされている。
- 優先度順序(現状は `HashSet` の列挙順まかせ)、Register/Unregisterの対称性保証(漏れるとメモリリークや二重更新につながる)、一時停止(ポーズ中のUI操作のみ更新したい等)が未実装。

**想定public API:**
```csharp
public interface IUpdatable { void OnUpdate(float deltaTime); }

public interface ITickGroup : IUpdatable
{
    IDisposable RegisterScoped(IUpdatable updatable, int priority = 0); // 自動Unregister
    void Register(IUpdatable updatable, int priority = 0);
    void Unregister(IUpdatable updatable);
    bool IsPaused { get; set; }
}

public sealed class TickGroup : ITickGroup
{
    public TickGroup(string name = null, IExceptionPolicy exceptionPolicy = null);
}

public interface IExceptionPolicy { void Handle(IUpdatable source, Exception exception); }
```

**設計上、判断が必要になる論点:**
1. 優先度表現(単純なint優先度 vs 明示的フェーズenum vs 依存関係からのトポロジカルソート)。
2. 例外処理ポリシーを固定(現状の `Debug.LogError`)にするか `IExceptionPolicy` としてDI可能にするか。
3. `CancellationToken`(R3の `RegisterTo` と同じ作法)ベースの自動解除と、手動 `Register`/`Unregister` をどう両立させるAPIにするか。
4. `TickGroup` 自体を `IUpdatable` として親に登録できるネスト構造の実行順序保証(親のTick内で子Groupがどのタイミングで呼ばれるか)。

**READMEでの見せ方:** ネスト構成(System→Orrery→Planet相当)を模した小さなデモシーンで、登録順・優先度順にオブジェクトが動くGIF。Before(手書きRegister/Unregisterコード)/After(ライブラリ利用)のコード行数比較。

**想定実装コスト:** 8〜12時間(既存実装がベースにあるため比較的軽い)。

---

### 推奨3: PlanetParallax(合計37点、追加検討により新規トップ)

**何を作るか:** 惑星のような円形ワールドの外周を、シームレスにラップしながら流れる2Dパララックス背景システム。円周長からレイヤー速度を自動逆算し、インスペクターのボタン一発でレイヤー生成まで完了する。

**解決するOrrariumの課題:**
- ParallaxView.cs(203行)が既に核心ロジックを実装済み。`parallaxWidth = planetRadius * 2π` から必要なタイル範囲を再計算する処理(ParallaxView.cs:142-177)、`loopCount` から速度係数を自動逆算する設計(ParallaxView.cs:64)。
- OldParallaxView.cs(238行、参照0件を確認済み)という死んだ旧実装が残っており、1回作り直された経緯がある=設計判断の余地が大きい問題だったことの物的証拠。
- `ParallaxView.UpdateNonLoopLayers()`(ParallaxView.cs:179)が`ParallaxPresenter`から一度も呼ばれておらず未配線(grep実測)。ライブラリ化の前に整理が必要。

**想定public API:**
```csharp
public interface ICurvedParallaxLayer
{
    float Speed { get; }               // 円周に対する相対速度(1=固定, 0=カメラに追従)
    void UpdateTiling(float cameraX, float cameraViewWidth, float circumference);
}

public sealed class CurvedParallaxSurface : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private List<LoopLayerConfig> loopLayers;

    public void RegenerateLayers();          // エディタボタンから呼ぶ生成処理
    public void Tick(float cameraX, float cameraViewWidth);
}

[Serializable]
public struct LoopLayerConfig
{
    public Sprite sprite;
    [Min(1)] public int loopCount;   // 円周を何周分のスプライトで覆うか→速度を自動逆算
    public float offsetY;
}
```

**設計上、判断が必要になる論点:**
1. 円周を割り切れないスプライト幅をどう扱うか(継ぎ目の許容誤差、自動スケーリング vs 警告)。
2. 平坦なワールド(`radius→∞`)も同じAPIで扱える抽象化にするか、円形専用に振り切るか(汎用性 vs 差別化のトレードオフ)。
3. エディタ時生成(現状の`[Button]`方式)と実行時プロシージャル生成のどちらを正とするか、両対応するならAPIをどう共通化するか。
4. `NonLoopLayer`(未配線のまま残っている固定オブジェクトの周回配置)を仕様として残すか削るか。

**READMEでの見せ方:** 惑星を一周歩き続けて背景がシームレスにループするGIF(最も強い訴求)、`loopCount`を変えるだけで速度が変わる比較GIF、インスペクターのボタン1つでレイヤー生成する操作フロー。

**想定実装コスト:** 10〜16時間(死んだ旧実装の整理・未配線メソッドの解消・汎用化・サンプル惑星シーン込み)。

---

## 追加検討(フォローアップ): GC削減ライブラリとシェーダー集の評価詳細

### GC削減ライブラリの検討(結論: 単独候補として非推奨)

毎フレーム実行されるホットパス(`JetpackModel.Tick()`、`UpdateService.OnUpdate()`、`ParallaxView.UpdateLoopLayers()`、`InputView.OnUpdate()`)を実際に読んだが、いずれもLINQ・クロージャ・ボックス化を伴わない書き方が既に徹底されている。実測できる唯一のアロケーション源は InventoryModel.cs:193-225 の `ItemList()`/`TotalMass`/`TotalVolume`(`OrderBy`へのメソッドグループ変換が毎回delegateを生成、`Dictionary.Values`のLINQ列挙は列挙子がボックス化される)だが、これは取引・インベントリ更新イベント時のみ発火し毎フレームではない。

R3自体が低アロケーション設計であり、これを代替するライブラリは依頼の除外基準(R3との衝突)に抵触する。Unityにも`UnityEngine.Pool.ObjectPool<T>`/`ListPool<T>`が標準搭載されており、汎用プールの再発明も同様に既存代替と衝突する。**したがって独立したポートフォリオ用「GC削減ライブラリ」はこのコードベースの実態を裏付けにできず非推奨。**

唯一具体的に正当化できるのは、86箇所ある `Debug.Log($"[ClassName] ...")` (候補2「自動タグ付けロガー」)が呼ばれるたびに文字列補間でヒープ確保している点——これを `[Conditional]` 属性でリリースビルドから完全に除去できる設計にすることが、実測可能な唯一のGC改善である。単独ライブラリ化するなら「GCアロケーション計測ミニツール」(`GC.GetAllocatedBytesForCurrentThread()` でメソッド単位の確保量を計測するエディタ拡張、"減らす"ではなく"測る"ツール)とし、候補4(属性ベースInspector拡張キット)に機能追加する形を推奨する。

### シェーダー集(水面・霧・星空)の評価詳細(合計32点、5位)

**何を作るか:** 曲面ワールド(惑星)を前提にした2Dピクセルアート向け大気表現シェーダー集。水面・霧・星空を、共通の「直交カメラでのワールド座標復元」ユーティリティの上に統一設計で束ねる。

**解決するOrrariumの課題:**
- WaterSurfaceURP.shader(236行)は8方向正弦波合成の環境波と`_RippleData[8]`配列による波紋(時間・距離減衰、疑似法線による太陽グリント)まで実装済みの本格的なシェーダーだが、対応する WaterSurfaceView.cs は`Initialize()`/`SpawnRipple(float x)`が中身空のスタブのまま——シェーダーは完成度が高いのにCPU→GPUへ波紋データを渡す配線だけが欠けている。
- Atmosphere.shader(151行)は直交カメラでの深度復元・距離霧・高さ霧・砂埃をURPのフルスクリーンBlitパスで実装。コメントに「Dustは現状維持、Fogを調整中」と明記されており試作中。
- srs_fog_shader.md という要件定義メモが存在するが「各ピクセルに対して物理演算を行うことで、霧が」で文が途切れており未完成。「画面のピクセル数の4倍で計算を行う」という要件はComputeShaderか高解像度RTでの実装を示唆している。
- 星空/starfield関連のシェーダー・スクリプトはコード全文検索で0件——完全に手つかず。
- `ProcedualNoice*.shader` が4バリアント(無印/_Sprite/_URP/_URP_Fixed、合計718行)存在し、収束していない試行錯誤の跡が残っている。C#側で見つかった重複コード問題(`TradeView`/`InventoryView`)のシェーダー版に相当する。

**想定public API(C#側の薄いコントローラ):**
```csharp
public sealed class WaterSurfaceController : MonoBehaviour
{
    public void SpawnRipple(Vector2 worldPosition, float strength = 1f); // _RippleData配列へ書き込み
}

public sealed class FogVolumeController : MonoBehaviour
{
    public FogVolumeHandle Spawn(Vector3 position, float lifetime);
    public void Despawn(FogVolumeHandle handle);
}

public sealed class StarfieldController : MonoBehaviour
{
    public void SetDensity(float density);
    public void SetTwinkleSpeed(float speed);
}
```

**設計上、判断が必要になる論点:**
1. CPU側のゲームイベント(足音・インタラクション位置)からGPU側の`_RippleData`のような固定長配列へどうデータを詰めるか(最大同時数の設計、古い要素の追い出し戦略)。
2. `Atmosphere.shader`のワールドY復元ロジックを水面・星空でも再利用する共通HLSLインクルードとして切り出すか、各シェーダーに個別実装のまま残すか。
3. 霧の「画面のピクセル数の4倍で計算」という要件をComputeShaderで実装するか、フラグメントシェーダーの高解像度RTで代替するかのパフォーマンストレードオフ。
4. URP ScriptableRendererFeature(Scanline.shaderで既に実績あり)としてポストエフェクト化するか、通常のマテリアル/スプライトシェーダーとして実装するかの使い分け方針。

**READMEでの見せ方:** 水面に波紋が広がるGIF、霧が画面奥から流れてくるGIF、星空の瞬きGIFを1枚のヒーロー画像にまとめる。共通ユーティリティの上に3効果を積む設計図。

**想定実装コスト:** 20〜30時間(水面はC#配線のみで済むため軽いが、霧のComputeShader化と星空の新規実装が大半を占める)。

---

## まとめ

現状分析から見える全体像は「レイヤー構造・DI無しの手動配線・シングルトン排除というアーキテクチャ規律は一貫して守られているが、Model層の周辺(設定・ロギング・Editor拡張・共通UIウィジェット)の抽象化と、View/シェーダー層の重複整理が追いついていない」という状態である。

追加検討の結果、**総合トップはPlanetParallax(37点)**となった。上位3件(PlanetParallax／属性ベースInspector拡張キット／階層的Update配信ライブラリ)は、(a) 既に部分実装がありゼロから作る必要がない、(b) Orrariumの次のマイルストーン(惑星探索・Trade UI)に直結する、(c) ドメイン知識を必要とせず単体リポジトリとして完結する、という3条件を同時に満たしており、依頼の「本体の進行を前に進める」と「ポートフォリオとして機能する」を両立しやすい。リアクティブ設定値ストア(34点)とシェーダー集(32点)も僅差の次点であり、HLSL/描画の比重を高めたいならシェーダー集を優先する判断もありうる。

GC削減については、コードの実態(ホットパスは既にアロケーション意識済み)がこれを単独ライブラリとして裏付けないという結論に至った——要求された項目数を埋めるための無理な追加は行っていない。
