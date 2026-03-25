# 開発進捗

## 完了

### 1. アーキテクチャと基盤システム
- [x] **MVPアーキテクチャの導入**: Model, View, Presenterの分離による疎結合な設計基盤の確立
- [x] **グローバルブートストラッパーの実装**: `GlobalBootstrapper` によるゲーム全体の初期化と、各種モデル（Save, Economy, Yarn, Audio, Inventory, Player, Quest）のシングルトン的なライフサイクル管理
- [x] **シーン別ブートストラッパーの実装**: 宇宙空間用の `SpaceBootstrapper` など、シーン固有の初期化処理の分離

### 2. 物理演算システム (Physics System)
- [x] **マルチスレッド物理エンジンの構築**: Unity Job System (`IJobParallelFor`) と `NativeArray` を活用した `PhysicsEngine` の実装
- [x] **大規模シミュレーション対応**: 最大10,000隻の宇宙船の物理演算を同時に処理できる最適化基盤の構築
- [x] **重力・惑星データ構造の定義**: 惑星の質量、半径、大気圏、重力圏 (`PlanetData`) を定義し、計算に組み込む仕組みの構築
- [x] **外部インターフェースの提供**: PresenterやViewが毎フレーム座標を取得・入力を設定するためのAPI (`SetInput`, `GetState`) の実装

### 3. プレイヤー制御と入力 (Player & Input)
- [x] **プレイヤー状態のデータモデル化**: `PlayerModel` における座標、移動入力、インタラクト状態などのデータ一元管理
- [x] **入力イベントのルーティング**: `PlayerInputPresenter` を介した、View(入力)からModelへのデータ更新フローの構築
- [x] **プレイヤーの描画更新処理**: `PlayerView` における、位置と回転 (Transform) の更新ロジックの実装

---

## 実行中

- [ ] 地上でのプレイヤーの移動ロジック

____



## 未完了

### 1. 経済システム (Economy System)
- [ ] **経済エンジンのロジック実装**: 現在空である `EconomyEngine` クラスに、物価変動、取引、需要と供給の計算ロジックを実装する
- [ ] **Job Systemの統合 (推測)**: 物理演算と同様に、多数の取引ノードやNPCの経済活動を並列処理するための `EconomyUpdateJob` の実装

### 2. サブシステムの詳細実装
- [ ] **会話・イベントシステム**: `YarnModel` および `YarnPresenter` を活用したNPCとの会話やシナリオ進行ロジックの構築
- [ ] **インベントリシステム**: `InventoryModel` に対するアイテムの取得、消費、所持品管理ロジックの実装
- [ ] **クエストシステム**: `QuestModel` におけるクエストの受注、進捗管理、達成判定の実装
- [ ] **オーディオ管理**: `AudioModel` と `AudioDataSO` を用いたBGM・SEの再生管理機能の完成

### 3. 物理・ゲームプレイの拡張
- [ ] **当たり判定（Collision）の実装**: 現在の `PhysicsEngine` に、船同士や惑星との衝突判定・ダメージ処理を追加する
- [ ] **NPC・敵AIの実装**: `PhysicsEngine` に入力を与えるための、AIコントローラー（Model/Presenter）の構築
- [ ] **インタラクト機能の完成**: `PlayerModel` の `currentTarget` と `interactFlag` を使用した、具体的な施設やNPCとの相互作用の実装

### 4. UI・データ永続化
- [ ] **ゲームUIの実装**: メニュー画面、インベントリ画面、クエストログ、会話ダイアログのUIビュー構築
- [ ] **セーブ・ロードシステム**: `SaveData` 構造の定義と、`DataPersister` を用いたファイルへの書き出し/読み込み処理の実装
- [ ] **シーン遷移管理**: `SceneSwitcher` を用いた、宇宙空間・惑星表面・屋内などのスムーズな画面遷移処理の実装