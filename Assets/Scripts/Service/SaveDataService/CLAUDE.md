# セーブ＆ロード機能

## 概要

プレイヤー位置・オーレリー（惑星運行）の経過時間などのゲーム進行状態をローカルJSONファイルに永続化し、後のセッションで復元する機能。

**スロットシステム：**
- 手動セーブスロット：1 個（新しい手動セーブで上書き）
- オートセーブスロット：5 個（満杯時は最古を削除）
- ID 採番：手動・オートを区別せず全体で連番（0, 1, 2, ...）

**現在バージョン：** `"0.30"`（`SaveDataService.CurrentVersion`）

---

## アーキテクチャ

### 層構造と責務分離

```
Presenter層（ISaveDataWriter / ISaveDataReader を実装）
    ↑↓ 登録
Presenter層（ISaveDataService を利用。Root/MenuPresenterから呼び出し）
    ↓ Save/Load 要求
Service層（SaveDataService + IDataStorage）
    ↓ 永続化
ファイルシステム（Application.persistentDataPath）
```

- **`ISaveable` のような単一インターフェースは存在しない。** 「書く」と「読む」で2つのインターフェースに分かれている。
- **実装するのは Presenter 層であり、Model 層ではない。** Model はセーブ/ロードを意識せず、公開プロパティ（`ReactiveProperty` など）と、外部から状態を復元するための狭いメソッド（`Init()` / `ReadFrom()` など）を持つだけ。

### インターフェース定義

#### `ISaveDataService`（`Presenter/ISaveDataService.cs`）
```csharp
namespace Presenter
{
    public interface ISaveDataService
    {
        public void Save(bool auto);
        public void Load(int id, bool newGame);
        public List<SaveDataInfo> GetSaveSlots();
        public void RegisterWriter(ISaveDataWriter writer);
        public void RegisterReader(ISaveDataReader reader);
        public void UnregisterWriter(ISaveDataWriter writer);
        public void UnregisterReader(ISaveDataReader reader);
    }
}
```
**実装：** `Service/SaveDataService/SaveDataService.cs`
**利用者：** `MenuPresenter`（Save/Loadボタン）、各シーンRoot（Register/Unregister）

#### `ISaveDataWriter` / `ISaveDataReader`（`Presenter/ISaveDataWriter.cs` / `ISaveDataReader.cs`）
```csharp
namespace Presenter
{
    public interface ISaveDataWriter { void WriteTo(SaveData data); }
    public interface ISaveDataReader { void ReadFrom(SaveData data); }
}
```
**実装者（実装済み）：** `PlayerPresenter`（`data.player`）、`OrreryPresenter`（`data.orrery`）

```csharp
// Presenter/Orrery/PlayerPresenter.cs
public void WriteTo(SaveData data)
{
    var d = new PlayerSaveData();
    d.coordSystem = _model.CoordPos.CurrentValue.CoordSystem;
    d.position = _model.CoordPos.CurrentValue.Position;
    d.velocity = _model.CoordPos.CurrentValue.Velocity;
    d.forward = _model.CoordPos.CurrentValue.Forward;
    data.player = d;
}

public void ReadFrom(SaveData data)
{
    PlayerSaveData d = data.player;
    _model.ReadFrom(new CoordPos(d.coordSystem, d.position, d.velocity, d.forward));
}
```

#### `IDataStorage`（`Service/SaveDataService/IDataStorage.cs`）
```csharp
namespace Service
{
    public interface IDataStorage
    {
        List<SaveDataInfo> GetSaveDataInfoList();
        bool ManualSave(SaveData saveData);
        bool AutoSave(SaveData saveData);
        bool TryLoad(int id, out SaveData saveData);
    }
}
```
**役割：** ファイルI/O、スロット管理、基本検証
**実装：** `JsonDataStorage`（JSON、`Application.persistentDataPath`）

### 登録の流れ（Root層の責務）

Presenter は自分から `SaveDataService` に登録しにいかない。**「誰が誰を登録するか」は Root 層が決める。**

```csharp
// Service/OrreryRoot.cs Bootstrap() 内
_orreryModel = new OrreryModel(orrerySettingsSo);
_jetpackModel = new JetpackModel(_entityModel, _orreryModel);
_playerPresenter = new PlayerPresenter(_jetpackModel, playerView, inputService, ...);
_orreryPresenter = new OrreryPresenter(_orreryModel);

_saveDataService.RegisterReader(_orreryPresenter);
_saveDataService.RegisterWriter(_orreryPresenter);
_saveDataService.RegisterReader(_playerPresenter);
_saveDataService.RegisterWriter(_playerPresenter);
```

`OrreryRoot.Terminate()` で対称的に `Unregister` する。`SaveDataService` 本体は `SystemRoot`（シーンを跨いで永続する最上位Root）が1つだけ生成し、`OrreryRoot.Bootstrap(...)` に参照を渡している。

`PlanetRoot` は `ISaveDataService` の参照を `Bootstrap()` で受け取るが、**現状は何も登録していない**（後述「既知の問題」）。

---

## データフロー

### Save

1. `MenuPresenter.HandleSaveStarted()` → `ISaveDataService.Save(auto: false)`
2. `SaveDataService.Save()` が新しい `SaveData` を生成し、`info.version` / `savedAt` / `savedAtUnixMs` を設定
3. 登録済みの全 `ISaveDataWriter` を走査して `WriteTo(data)` を呼び出す（例外時は `Debug.LogError` して次のWriterへ継続。1つの失敗が他を巻き込まない）
4. `IDataStorage.ManualSave(data)` / `AutoSave(data)` に永続化を委譲
5. `JsonDataStorage` がスロット制限（手動1・オート5）を適用し、`<id>_manual.json` / `<id>_auto.json` として書き出す

### Load

1. `MenuPresenter.HandleLoadSlotPressed()` → `ISaveDataService.Load(id, newGame: false)`
2. `IDataStorage.TryLoad(id, out data)` でJSONを読み込む（失敗時は `InvalidOperationException`）
3. `data.info.version != CurrentVersion` ならマイグレーションを実行（失敗時は例外を再送出。これは致命的なので握り潰さない）
4. 登録済みの全 `ISaveDataReader` を走査して `ReadFrom(data)` を呼び出す（例外時は `Debug.LogError` して次のReaderへ継続）

**⚠️ 実際の呼び出し順序に注意：** `MenuPresenter` はTitleシーンで動作しており、この時点では `PlayerPresenter`/`OrreryPresenter` はまだ存在しない（`OrreryRoot.Bootstrap()` はOrreryシーンのロード完了後に呼ばれる）。そのため手順1の `Load(id, false)` は登録済みReaderが0件の状態で実行され、実質的に何もModelへ反映しない。詳細は「既知の問題」を参照。

---

## データ構造（`Core/SaveData.cs`）

```csharp
[Serializable]
public class SaveData
{
    public SaveDataInfo info;
    public GlobalStateSaveData global;
    public OrrerySaveData orrery;
    //public EconomySaveData economy;      // フィールド自体コメントアウト、未配線
    //public InventorySaveData inventory;  // フィールド自体コメントアウト、未配線
    public PlayerSaveData player;
    public List<YarnVariable> yarn;

    public SaveData DeepCopy()
    {
        // JSON往復による深いコピー。参照型フィールドも含めて完全に独立する
        string json = JsonUtility.ToJson(this);
        return JsonUtility.FromJson<SaveData>(json);
    }
}
```

| 構造 | 内容 | 実装状況 |
|------|------|--------|
| `SaveDataInfo` | id, name, savedAt, autoSave, savedAtUnixMs, screenshotFileName, version, sceneName | ✅ |
| `PlayerSaveData` | coordSystem, position, velocity, forward | ✅（`PlayerPresenter`経由） |
| `OrrerySaveData` | orreryCumulativeSeconds | ✅（`OrreryPresenter`経由） |
| `GlobalStateSaveData` | sceneType | ⚠️ フィールドはあるが読み書きするPresenterが存在しない |
| `YarnVariable` | key, value, type | ⚠️ フィールドはあるが読み書きするPresenterが存在しない |
| `EconomySaveData` / `InventorySaveData` / `EntitySaveData` | （空） | ❌ 構造定義のみ、`SaveData` 本体ではコメントアウト |

---

## セーブスロット管理（`JsonDataStorage`）

- ファイル命名：`<id>_manual.json` / `<id>_auto.json`（`Application.persistentDataPath` 直下）
- ID採番：`GetNextSaveId()` が既存の全スロット（手動+オート）から `max(id) + 1` を計算。手動・オートで系列は分かれない
- `ManualSave()`：既存の手動セーブを全削除 → 新規作成
- `AutoSave()`：オートスロットが5個以上なら `savedAtUnixMs` が最小のものを削除 → 新規作成
- `TryLoad(id, out data)`：`<id>_manual.json` を先に探し、無ければ `<id>_auto.json` を探す
- 検証は「`JsonUtility.FromJson` の結果が `null` でないか」「`info.id >= 0`」のみ。JSON破損時の詳細なリカバリーはない

---

## バージョン管理とマイグレーション

```csharp
// SaveDataService.cs
private readonly string CurrentVersion = "0.30";
private readonly Dictionary<string, Action<SaveData>> _migrations = new() { /* 現状空 */ };

private void Migrate(SaveData data, string fromVersion, string toVersion)
{
    var versions = new[] { "0.20", "0.25", "0.30" }; // ハードコード配列
    int fromIdx = Array.IndexOf(versions, fromVersion);
    int toIdx = Array.IndexOf(versions, toVersion);
    if (fromIdx == -1 || toIdx == -1)
        throw new InvalidOperationException($"Unknown version: {fromVersion} or {toVersion}");

    for (int i = fromIdx; i < toIdx; i++)
        if (_migrations.TryGetValue(versions[i], out var migration))
            migration(data);
}
```

新しいバージョンを追加する際は **2箇所**を同時に更新する必要がある：
1. `Migrate()` 内の `versions` 配列にバージョン文字列を追加
2. `_migrations` 辞書に `{ "旧バージョン", 変換処理 }` を追加

どちらか一方だけ更新すると、登録した変換処理が黙って実行されない、または未知バージョンとして例外になる。

---

## 拡張ポイント：新しいModelをセーブ対象に追加する

1. **`SaveData` にフィールドを追加**（`Core/SaveData.cs`）
   ```csharp
   [Serializable]
   public struct EconomySaveData { public int balance; }
   // SaveData クラスに public EconomySaveData economy; を追加
   ```
2. **担当Presenterに `ISaveDataReader`/`ISaveDataWriter` を実装**
   ```csharp
   public void WriteTo(SaveData data) => data.economy = new EconomySaveData { balance = _model.Balance.CurrentValue };
   public void ReadFrom(SaveData data) => _model.ReadFrom(data.economy.balance);
   ```
3. **生成した箇所（Root）で登録**
   ```csharp
   _saveDataService.RegisterReader(_walletPresenter);
   _saveDataService.RegisterWriter(_walletPresenter);
   // Terminate() 側で Unregister も忘れずに
   ```

`SaveDataService` 自体の変更は不要。

---

## 既知の問題

- **🔴 ロードスロット選択が実質機能しない。** `MenuPresenter.HandleLoadSlotPressed()`（`Presenter/System/MenuPresenter.cs:79`）は `_saveDataService.Load(slots[slotIndex].id, false)` を呼ぶが、この時点ではTitleシーンにおり `PlayerPresenter`/`OrreryPresenter` はまだ登録されていない（登録は `OrreryRoot.Bootstrap()` 内、シーン遷移後）。その後 `SystemRoot.OnLoadingCompleted()`（`Service/SystemRoot.cs:126`）がOrreryシーンのロード完了時に**常に** `_saveDataService.Load(newGame:true)` を呼び直すため、選択したスロットのデータではなく `DefaultSaveDataSo` の初期状態で上書きされる。「Load」を押しても常に New Game 相当の状態でOrreryシーンに入る状態。修正するには、どのスロットを開くつもりか（あるいは新規ゲームか）を `SystemRoot` まで伝搬し、`OnLoadingCompleted` 側の `Load()` 呼び出しをそれに応じて切り替える必要がある。
- **`EconomySaveData` / `InventorySaveData` / `EntitySaveData` は未配線。** `SaveData` 本体ではフィールドがコメントアウトされたままで、構造体定義のみ存在する。
- **`GlobalStateSaveData`（シーン情報）と `YarnVariable`（ダイアログ変数）を読み書きするPresenterが見つからない。** フィールドは `SaveData` にあるが、対応する `ISaveDataWriter`/`ISaveDataReader` 実装が現状ない。
- **`PlanetRoot` は `ISaveDataService` を受け取るが登録していない**（`Service/PlanetRoot.cs:46` のコメント「まだ使わないが、ロードするならこの関数内で登録する必要がある」）。
- **マイグレーション対象バージョンが2箇所に分散**（`Migrate()`内の配列 と `_migrations`辞書）。追加時に片方だけ更新すると壊れる。

---

## 参考資料

- `Core/SaveData.cs` — 全データ構造
- `Presenter/ISaveDataService.cs`, `ISaveDataWriter.cs`, `ISaveDataReader.cs`
- `Service/SaveDataService/IDataStorage.cs`, `SaveDataService.cs`, `JsonDataStorage.cs`, `DefaultSaveDataSo.cs`
- `Presenter/Orrery/PlayerPresenter.cs`, `Presenter/Orrery/OrreryPresenter.cs` — Writer/Reader実装例
- `Service/OrreryRoot.cs` — 登録の実例、`Service/SystemRoot.cs` — `SaveDataService`の生成元
- `product-docs/srs_機能要件/load-save_ロードとセーブ.md` — アーキテクチャ全体像・クラス図
