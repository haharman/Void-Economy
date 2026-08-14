# MenuView 実装仕様書

## 要件

MenuView のUIを、マウスクリック操作と InputSystem（Gamepad/Keyboard）の両方で操作可能にする。
- EventSystem が全ナビゲーション状態を管理
- マウスクリック時に EventSystem の選択状態を同期
- 動的に変わる UI（Load スロット）のナビゲーションは実行時に再構築

---

## UI構成と画面遷移

### 全体構成

```
Menu（ルート）
  ├── LoadButton
  ├── SaveButton
  ├── QuitButton
  └── SettingsButton
     ↓
Load
  ├── Slot[0]
  ├── Slot[1]
  └── ...
  └── Slot[N-1]（最後のスロット）
     ↓
Save
  ├── 「セーブしています」表示状態
  └── セーブ完了後：OKボタン表示
     ↓
Quit（サブメニュー：Menu から分岐）
  ├── 「タイトルへ」ボタン
  ├── 「デスクトップへ」ボタン
  └── 「キャンセル」ボタン
     ↓
Settings
  ├── マスター音量（スライダー：0-100）
  ├── BGM音量（スライダー：0-100）
  ├── 効果音音量（スライダー：0-100）
  ├── 足音音量（スライダー：0-100）
  ├── 環境音音量（スライダー：0-100）
  ├── Windowモード（選択肢：FullScreen / Borderless / Windowed）
  ├── UIスケール（スライダー：50-200）
  ├── ポストプロセッシング（Boolean toggle）
  ├── 移動キー（スライダー型選択：WASD / 矢印キー）
  ├── Submitキー（リバインド受付状態に進入）
  ├── Cancelキー（リバインド受付状態に進入）
  ├── テキスト送り速度（選択肢：低速 / 中速 / 高速 / アニメーションなし）
  ├── 言語（選択肢：Ja / En）
  └── オートセーブ時間間隔（スライダー：秒単位）
```

### 画面遷移フロー

```
Menu
  ├─（Load ボタンクリック or Submit）→ Load
  │   └─（Cancel）→ Menu
  ├─（Save ボタンクリック or Submit）→ Save
  │   ├─（セーブ完了後 OKボタン表示）
  │   └─（OKボタン Submit）→ Menu
  ├─（Quit ボタンクリック or Submit）→ Quit
  │   ├─（「タイトルへ」Submit）→ タイトルシーン遷移
  │   ├─（「デスクトップへ」Submit）→ ゲーム終了
  │   └─（「キャンセル」Submit or Cancel）→ Menu
  └─（Settings ボタンクリック or Submit）→ Settings
      └─（Cancel）→ Menu

Menu（ゲーム中のみ）
  └─（Cancel InputAction）→ Menu 非表示
```

---

## InputAction 定義

### 使用する InputAction

以下の3つのみを使用：

1. **Move**
   - 説明：上下左右の移動入力
   - キー：W/A/S/D または 矢印キー
   - Gamepad：Left Stick

2. **Submit**
   - 説明：現在選択中の要素を決定
   - キー：Enter
   - Gamepad：Button South（PS: ✕、Xbox: A）

3. **Cancel**
   - 説明：前の画面に戻る、またはメニューを閉じる
   - キー：Escape
   - Gamepad：Button East（PS: ◯、Xbox: B）

### InputActionAsset 自作時の要件

InputActionAsset（`.inputactions`）を新規作成する場合は以下を厳守：

#### Action Map 設定

- **Action Map 名**: `UI`
- **Default Control Scheme**: （未設定でもよい、UI は汎用）
- **Action** 定義:
  ```
  ┌─ UI (Action Map)
  │  ├─ Move
  │  │  ├─ [Keyboard] W/A/S/D
  │  │  ├─ [Keyboard] 上下左右キー
  │  │  └─ [Gamepad] Left Stick
  │  ├─ Submit
  │  │  ├─ [Keyboard] Enter
  │  │  └─ [Gamepad] Button South
  │  └─ Cancel
  │     ├─ [Keyboard] Escape
  │     └─ [Gamepad] Button East
  ```

#### InputSystemUIInputModule への割り当て

EventSystem に アタッチされた `InputSystemUIInputModule` に以下を設定：

```
Move Action: UI/Move
Submit Action: UI/Submit
Cancel Action: UI/Cancel
Point Action: （デフォルト: Pointer/Position）
Click Action: （デフォルト: Pointer/Click）
Scroll Wheel Action: （デフォルト: Pointer/Scroll）
Scroll Sensitivity: 1.0
```

#### Composite の設定

- **Move Action** は以下の Composite を含むこと：
  - `2D Vector（WASD/矢印キー用）`
  - Binding:
    - Up: W / 上キー
    - Down: S / 下キー
    - Left: A / 左キー
    - Right: D / 右キー

#### Control Scheme（オプション）

UI メニュー用なので Control Scheme は不要だが、必要に応じて以下を定義：

```
┌─ Keyboard&Mouse
│  └─ Keyboard, Mouse
└─ Gamepad
   └─ Gamepad
```

---

## EventSystem とナビゲーション設定

### Project Settings

**Player > Active Input Handling:**
- `Input System Package (New)` に設定

### EventSystem GameObject の構成

```
Canvas (EventSystem がアタッチ)
├── EventSystem
└── InputSystemUIInputModule
    ├── Move Action → Move
    ├── Submit Action → Submit
    ├── Cancel Action → Cancel
    └── Pointer/Click Actions → UI 標準（マウス用）
```

### ナビゲーション Mode

#### Menu（ルート）ボタン 4個

- **Navigation Mode:** Explicit
- **接続:**
  ```
  LoadButton
    selectOnUp → SettingsButton
    selectOnDown → SaveButton
    selectOnLeft → 無し
    selectOnRight → 無し

  SaveButton
    selectOnUp → LoadButton
    selectOnDown → QuitButton
    selectOnLeft → 無し
    selectOnRight → 無し

  QuitButton
    selectOnUp → SaveButton
    selectOnDown → SettingsButton
    selectOnLeft → 無し
    selectOnRight → 無し

  SettingsButton
    selectOnUp → QuitButton
    selectOnDown → LoadButton（ループ）
    selectOnLeft → 無し
    selectOnRight → 無し
  ```
- **特徴:** 縦一列のループ。左右キーは無視。

#### Load スロット

- **Navigation Mode:** Explicit（実行時に動的再構築）
- **接続:**
  ```
  Slot[0]
    selectOnUp → Slot[N-1]（ループ）
    selectOnDown → Slot[1]
    selectOnLeft → 無し
    selectOnRight → 無し

  Slot[i]（0 < i < N-1）
    selectOnUp → Slot[i-1]
    selectOnDown → Slot[i+1]
    selectOnLeft → 無し
    selectOnRight → 無し

  Slot[N-1]
    selectOnUp → Slot[N-2]
    selectOnDown → Slot[0]（ループ）
    selectOnLeft → 無し
    selectOnRight → 無し
  ```
- **特徴:** 縦一列のループ。実行時に EnterLoad() 時に再構築。

#### Save 画面

- OKボタンのみ Selectable
- 「セーブしています」表示中は操作対象がない（EventSystem.SetSelectedGameObject(null)）
- セーブ完了後に自動的に OKボタンを SetSelectedGameObject

#### Quit サブメニュー

- **Navigation Mode:** Explicit
- **接続:**
  ```
  「タイトルへ」ボタン
    selectOnUp → 「キャンセル」ボタン（ループ）
    selectOnDown → 「デスクトップへ」ボタン
    selectOnLeft → 無し
    selectOnRight → 無し

  「デスクトップへ」ボタン
    selectOnUp → 「タイトルへ」ボタン
    selectOnDown → 「キャンセル」ボタン
    selectOnLeft → 無し
    selectOnRight → 無し

  「キャンセル」ボタン
    selectOnUp → 「デスクトップへ」ボタン
    selectOnDown → 「タイトルへ」ボタン（ループ）
    selectOnLeft → 無し
    selectOnRight → 無し
  ```
- **特徴:** 縦一列のループ。左右キーは無視。

#### Settings

- **Navigation Mode:** Explicit（実行時に生成）
- **接続:** 上下のみ、無限ループ
- **特徴:**
  - 各設定項目は上下で切り替え
  - 左右キーで値を変更（スライダー ±5、選択肢は前/次）
  - Submit キーの動作：
    - スライダー／Boolean／選択肢の場合：下キー入力と同じ（次の項目へ移動）
    - Submitキー設定項目：リバインド受付状態に進入
    - Cancelキー設定項目：リバインド受付状態に進入

---

## MenuView クラスの実装要件

### フィールド

```csharp
// メニュー全体の表示/アニメーション
[SerializeField] private GameObject menuObj;
[SerializeField] private RectTransform menuRect;
[SerializeField] private float slideInDuration;

private bool _isTitleScene = false;
private bool _isOpen = false;

// パネル状態管理
private enum PanelType
{
    Home,
    Load,
    Save,
    Quit,
    Settings
}
private PanelType _currentPanel;

// ====== Load 画面 ======
[SerializeField] private GameObject loadObj;
[Serializable] private struct SlotComponents
{
    public Button button;          // Navigation 対象
    public TextMeshProUGUI name;
    public TextMeshProUGUI date;
    public TextMeshProUGUI id;
}
[SerializeField] private List<SlotComponents> slotList;
[SerializeField] private string defaultSlotName;
[SerializeField] private string defaultSlotId;
private int _selectedSlot = 0;

// ====== Save 画面 ======
[SerializeField] private GameObject saveObj;
[SerializeField] private TextMeshProUGUI savingText;        // 「セーブしています」
[SerializeField] private Button saveOkButton;
[SerializeField] private GameObject saveOkButtonContainer;  // セーブ完了後に表示

// ====== Quit 画面 ======
[SerializeField] private GameObject quitObj;
[SerializeField] private Button quitToTitleButton;
[SerializeField] private Button quitToDesktopButton;
[SerializeField] private Button quitCancelButton;

// ====== Settings 画面 ======
[SerializeField] private GameObject settingsObj;
// 設定項目は実装で動的に生成または参照

// ====== ボタン（Menu ルート） ======
[SerializeField] private Button loadButton;
[SerializeField] private Button saveButton;
[SerializeField] private Button quitButton;
[SerializeField] private Button settingsButton;

// Presenter 側が購読するイベント
public event Action<int> OnLoadSlotSelected;  // Load 画面でスロットを Submit
public event Action OnSaveStarted;             // Save 画面で OK Submit
public event Action OnQuitToTitle;             // Quit で「タイトルへ」Submit
public event Action OnQuitToDesktop;           // Quit で「デスクトップへ」Submit
public event Action OnSettingsChanged;         // Settings で値が変更された

// セーブ完了イベントは Presenter から呼び出される
public void OnSaveComplete() { ... }
```

### 主要メソッド

#### 1. Awake / OnEnable / OnDisable

```csharp
public void Awake()
{
    // ボタンに EventTrigger で PointerClick を実装（クリック時に SetSelectedGameObject）
    SetupPointerClickHandlers();
    SetupButtonColors();
    
    // ボタンリスナー登録
    loadButton.onClick.AddListener(HandleLoadButtonClick);
    saveButton.onClick.AddListener(HandleSaveButtonClick);
    quitButton.onClick.AddListener(HandleQuitButtonClick);
    settingsButton.onClick.AddListener(HandleSettingsButtonClick);
}

private void OnDestroy()
{
    // リスナー削除
    loadButton.onClick.RemoveAllListeners();
    saveButton.onClick.RemoveAllListeners();
    quitButton.onClick.RemoveAllListeners();
    settingsButton.onClick.RemoveAllListeners();
}
```

#### 2. メニュー全体の表示/非表示

```csharp
public void OnEnterTitle()
{
    // タイトルシーン進入時：メニューを完全表示
    _isTitleScene = true;
    _isOpen = true;
    _currentPanel = PanelType.Home;
    menuObj.SetActive(true);
    homeObj.SetActive(true);
    loadObj.SetActive(false);
    saveObj.SetActive(false);
    quitObj.SetActive(false);
    settingsObj.SetActive(false);
    menuRect.DOKill();
    menuRect.anchoredPosition = Vector2.zero;
    
    // Menu ルートボタンの最初の要素を選択
    EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
}

public void OnExitTitle()
{
    // 他のシーンに遷移時：メニューを非表示
    _isTitleScene = false;
    _isOpen = false;
    menuObj.SetActive(false);
    homeObj.SetActive(false);
    loadObj.SetActive(false);
    saveObj.SetActive(false);
    quitObj.SetActive(false);
    settingsObj.SetActive(false);
    menuRect.DOKill();
    menuRect.anchoredPosition = new Vector2(-menuRect.rect.width, 0);
}

public void ToggleMenu()
{
    // ゲーム中に Escape/Cancel で呼ばれる
    // タイトルシーン中は操作しない
    if (_isTitleScene) return;

    if (_isOpen)
    {
        OnMenuClose();
        _isOpen = false;
    }
    else
    {
        OnMenuOpen();
        _isOpen = true;
    }
}

private void OnMenuOpen()
{
    if (_isTitleScene) return;
    _currentPanel = PanelType.Home;
    menuObj.SetActive(true);
    homeObj.SetActive(true);
    loadObj.SetActive(false);
    saveObj.SetActive(false);
    quitObj.SetActive(false);
    settingsObj.SetActive(false);
    
    menuRect.DOKill();
    float startX = Mathf.Max(-menuRect.rect.width, menuRect.anchoredPosition.x);
    menuRect.DOAnchorPosX(0, slideInDuration)
        .From(new Vector2(startX, 0), true)
        .SetEase(Ease.OutCubic)
        .OnComplete(() => 
        {
            // スライドイン完了後、最初のボタンを選択
            EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
        });
}

private void OnMenuClose()
{
    if (_isTitleScene) return;
    menuRect.DOKill();
    float startX = Mathf.Max(-menuRect.rect.width, menuRect.anchoredPosition.x);
    menuRect.DOAnchorPosX(-menuRect.rect.width, slideInDuration)
        .From(new Vector2(startX, 0), true)
        .SetEase(Ease.InCubic)
        .OnComplete(() =>
        {
            menuObj.SetActive(false);
            homeObj.SetActive(false);
            loadObj.SetActive(false);
            saveObj.SetActive(false);
            quitObj.SetActive(false);
            settingsObj.SetActive(false);
        });
}

// キャンセルボタンの挙動
public void HandleCancelPressed()
{
    switch (_currentPanel)
    {
        case PanelType.Home:
            ToggleMenu();
            break;
        case PanelType.Load:
            ExitLoad();
            break;
        case PanelType.Save:
            ExitSave();
            break;
        case PanelType.Quit:
            ExitQuit();
            break;
        case PanelType.Settings:
            ExitSettings();
            break;
    }
}
```

#### 3. Load 画面

```csharp
public void EnterLoad(List<SaveDataInfo> slots)
{
    // slots はセーブデータのリスト（IDが降順）
    
    _currentPanel = PanelType.Load;
    homeObj.SetActive(false);
    loadObj.SetActive(true);
    saveObj.SetActive(false);
    quitObj.SetActive(false);
    settingsObj.SetActive(false);
    _selectedSlot = 0;
    
    // スロット UI を構築・更新
    int displayCount = Mathf.Min(slots.Count, slotList.Count);
    for (int i = 0; i < displayCount; i++)
    {
        slotList[i].name.text = slots[i].name;
        slotList[i].date.text = slots[i].savedAt;
        slotList[i].id.text = slots[i].id.ToString();
        
        // Button にリスナー追加（Submit 検知）
        int index = i;
        slotList[i].button.onClick.AddListener(() => 
        {
            OnLoadSlotSelected?.Invoke(index);
        });
    }
    
    // 残りのスロット（データがない場合）を初期化
    for (int i = displayCount; i < slotList.Count; i++)
    {
        slotList[i].name.text = defaultSlotName;
        slotList[i].date.text = "";
        slotList[i].id.text = defaultSlotId;
    }
    
    // ナビゲーション再構築（重要）
    RebuildLoadNavigation();
    
    // 最初のスロットを選択
    EventSystem.current.SetSelectedGameObject(slotList[0].button.gameObject);
}

private void RebuildLoadNavigation()
{
    // Load スロットの上下ナビゲーションを再構築（ループ対応）
    int count = slotList.Count;
    for (int i = 0; i < count; i++)
    {
        Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };
        nav.selectOnUp = slotList[(i - 1 + count) % count].button;
        nav.selectOnDown = slotList[(i + 1) % count].button;
        slotList[i].button.navigation = nav;
    }
}

public void ExitLoad()
{
    _currentPanel = PanelType.Home;
    loadObj.SetActive(false);
    homeObj.SetActive(true);
    // ボタンのリスナーをクリア
    foreach (var slot in slotList)
    {
        slot.button.onClick.RemoveAllListeners();
    }
    EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
}
```

#### 4. Save 画面

```csharp
public void EnterSave()
{
    _currentPanel = PanelType.Save;
    homeObj.SetActive(false);
    loadObj.SetActive(false);
    saveObj.SetActive(true);
    quitObj.SetActive(false);
    settingsObj.SetActive(false);
    
    savingText.gameObject.SetActive(true);
    saveOkButtonContainer.SetActive(false);
    
    // 操作対象なし
    EventSystem.current.SetSelectedGameObject(null);
}

public void OnSaveComplete()
{
    // Presenter から呼ばれる（セーブ完了イベント受け取り）
    savingText.gameObject.SetActive(false);
    saveOkButtonContainer.SetActive(true);
    
    // OKボタンを自動選択（View の責務）
    EventSystem.current.SetSelectedGameObject(saveOkButton.gameObject);
    
    // OKボタン Submit リスナー
    saveOkButton.onClick.AddListener(() => 
    {
        OnSaveStarted?.Invoke();
        ExitSave();
    });
}

public void ExitSave()
{
    _currentPanel = PanelType.Home;
    saveObj.SetActive(false);
    homeObj.SetActive(true);
    saveOkButton.onClick.RemoveAllListeners();
    EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
}
```

#### 5. Quit 画面

```csharp
public void EnterQuit()
{
    _currentPanel = PanelType.Quit;
    homeObj.SetActive(false);
    loadObj.SetActive(false);
    saveObj.SetActive(false);
    quitObj.SetActive(true);
    settingsObj.SetActive(false);
    
    // ボタンリスナー
    quitToTitleButton.onClick.AddListener(() => 
    {
        OnQuitToTitle?.Invoke();
        ExitQuit();
    });
    quitToDesktopButton.onClick.AddListener(() => 
    {
        OnQuitToDesktop?.Invoke();
        ExitQuit();
    });
    quitCancelButton.onClick.AddListener(() => 
    {
        ExitQuit();
    });
    
    // ナビゲーション設定（Explicit）
    quitToTitleButton.navigation = new Navigation 
    { 
        mode = Navigation.Mode.Explicit,
        selectOnUp = quitCancelButton,
        selectOnDown = quitToDesktopButton
    };
    quitToDesktopButton.navigation = new Navigation 
    { 
        mode = Navigation.Mode.Explicit,
        selectOnUp = quitToTitleButton,
        selectOnDown = quitCancelButton
    };
    quitCancelButton.navigation = new Navigation 
    { 
        mode = Navigation.Mode.Explicit,
        selectOnUp = quitToDesktopButton,
        selectOnDown = quitToTitleButton
    };
    
    // 最初のボタンを選択
    EventSystem.current.SetSelectedGameObject(quitToTitleButton.gameObject);
}

public void ExitQuit()
{
    _currentPanel = PanelType.Home;
    quitObj.SetActive(false);
    homeObj.SetActive(true);
    quitToTitleButton.onClick.RemoveAllListeners();
    quitToDesktopButton.onClick.RemoveAllListeners();
    quitCancelButton.onClick.RemoveAllListeners();
    EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
}
```

#### 6. Settings 画面

```csharp
public void EnterSettings()
{
    _currentPanel = PanelType.Settings;
    homeObj.SetActive(false);
    loadObj.SetActive(false);
    saveObj.SetActive(false);
    quitObj.SetActive(false);
    settingsObj.SetActive(true);
    
    // Settings アイテムの動的生成・ナビゲーション構築
    // （詳細は別仕様：SettingsView が担当する可能性あり）
    
    // 最初の設定項目を選択
    // EventSystem.current.SetSelectedGameObject(firstSettingItem);
}

public void ExitSettings()
{
    _currentPanel = PanelType.Home;
    settingsObj.SetActive(false);
    homeObj.SetActive(true);
    EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
}
```

### マウスクリック同期の実装

```csharp
// MenuView に以下を追加

private void SetupPointerClickHandlers()
{
    // Menu ルートボタン
    AddPointerClickHandler(loadButton);
    AddPointerClickHandler(saveButton);
    AddPointerClickHandler(quitButton);
    AddPointerClickHandler(settingsButton);
    
    // Quit ボタン
    AddPointerClickHandler(quitToTitleButton);
    AddPointerClickHandler(quitToDesktopButton);
    AddPointerClickHandler(quitCancelButton);
    
    // Save OKボタン
    AddPointerClickHandler(saveOkButton);
}

private void AddPointerClickHandler(Button button)
{
    // EventTrigger で PointerClick イベントを検知
    EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
    EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
    entry.callback.AddListener((data) => 
    {
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    });
    trigger.triggers.Add(entry);
}

private void SetupButtonColors()
{
    // Menu ルートボタン
    SetButtonColors(loadButton);
    SetButtonColors(saveButton);
    SetButtonColors(quitButton);
    SetButtonColors(settingsButton);
    
    // Quit ボタン
    SetButtonColors(quitToTitleButton);
    SetButtonColors(quitToDesktopButton);
    SetButtonColors(quitCancelButton);
    
    // Save OKボタン
    SetButtonColors(saveOkButton);
    
    // Load スロットボタン
    foreach (var slot in slotList)
    {
        SetButtonColors(slot.button);
    }
}

private void SetButtonColors(Button button)
{
    // ボタンの色設定（normal, selected, highlighted, pressed, disabled）
    ColorBlock colors = button.colors;
    colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);        // 黒
    colors.selectedColor = new Color(0.5f, 0.8f, 1f, 1f);        // 明るい青
    colors.highlightedColor = new Color(0.4f, 0.7f, 1f, 1f);     // ホバー時の青
    colors.pressedColor = new Color(0.3f, 0.6f, 0.9f, 1f);       // 押下時の濃い青
    colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);    // グレイアウト
    button.colors = colors;
}
```

---

## MenuPresenter クラスの実装要件

### コンストラクタと初期化

```csharp
public class MenuPresenter
{
    private MenuView _view;
    private ISceneService _sceneService;
    private IDataService _dataService;
    private IQuitService _quitService;
    private InputView _inputView;  // Cancel イベント購読用
    private GlobalStateModel _globalStateModel;

    public MenuPresenter(
        ISceneService sceneService,
        IDataService dataService,
        IQuitService quitService,
        InputView inputView,
        MenuView view,
        GlobalStateModel globalStateModel,
        CancellationToken cancellationToken)
    {
        _sceneService = sceneService;
        _dataService = dataService;
        _quitService = quitService;
        _inputView = inputView;
        _view = view;
        _globalStateModel = globalStateModel;

        // シーン遷移時の処理
        globalStateModel.CurrentSceneState
            .Subscribe(state =>
            {
                if (state == SceneState.Title)
                    _view.OnEnterTitle();
                else if (state == SceneState.Surface || state == SceneState.Space)
                    _view.OnExitTitle();
            })
            .RegisterTo(cancellationToken);

        // View イベント購読
        _view.OnLoadSlotSelected += HandleLoadSlotSelected;
        _view.OnSaveStarted += HandleSaveStarted;
        _view.OnQuitToTitle += HandleQuitToTitle;
        _view.OnQuitToDesktop += HandleQuitToDesktop;
        _view.OnSettingsChanged += HandleSettingsChanged;

        // Input イベント購読
        _inputView.OnCancelPressed += HandleCancelPressed;
    }

    // イベントハンドラ
    private void HandleLoadSlotSelected(int slotIndex)
    {
        // スロットのロード処理
        var slots = _dataService.GetSaveSlots();
        if (slotIndex < slots.Count)
        {
            _dataService.Load(slots[slotIndex].id);
            _sceneService.LoadScene(SceneType.Surface);
        }
        _view.ExitLoad();
    }

    private void HandleSaveStarted()
    {
        // セーブ完了後、Menu に戻る
        _view.ExitSave();
    }

    private void HandleQuitToTitle()
    {
        _view.ExitQuit();
        _sceneService.LoadScene(SceneType.Title);
    }

    private void HandleQuitToDesktop()
    {
        _view.ExitQuit();
        _quitService.QuitGame();
    }

    private void HandleSettingsChanged()
    {
        // 設定値の保存処理など
    }

    private void HandleCancelPressed()
    {
        _view.HandleCancelPressed();
    }
}
```

---

## 実装チェックリスト

### MenuView

- [ ] Menu ルートボタン 4個に EventTrigger でマウスクリック検知
- [ ] Menu ルートボタンのナビゲーション設定（縦一列ループ、上下のみ）
- [ ] Load 画面の EnterLoad() で Slot ナビゲーション動的再構築
- [ ] Load 画面のループナビゲーション確認
- [ ] Save 画面の「セーブしています」と「OKボタン」の状態管理
- [ ] Save 画面の OKボタン完了後の自動フォーカス（View 責務）
- [ ] Quit 画面のナビゲーション（縦一列ループ、上下のみ）
- [ ] Settings 画面のナビゲーション（上下のみ、動的生成予定）
- [ ] Menu 非表示時の Cancel 入力処理（ToggleMenu）
- [ ] EventSystem の SetSelectedGameObject を全ての画面遷移・フォーカス変更で呼び出し確認

### MenuPresenter

- [ ] View のイベント全て購読（OnLoadSlotSelected, OnSaveStarted, OnQuitToTitle, OnQuitToDesktop）
- [ ] InputView の Cancel イベント購読
- [ ] GlobalStateModel の CurrentSceneState 購読（タイトル進入時に OnEnterTitle 呼び出し）
- [ ] Load 選択時の DataService.GetSaveSlots() 呼び出し
- [ ] Quit ボタンの遷移ロジック実装
- [ ] Save 完了イベント（Model / Service から）の購読と View.OnSaveComplete() 呼び出し

### Project Settings

- [ ] Active Input Handling を「Input System Package (New)」に設定
- [ ] InputSystemUIInputModule が EventSystem にアタッチ
- [ ] Move/Submit/Cancel アクションが InputSystemUIInputModule に正しく割り当てられた

---

## 既知テクニック・注意点

### ナビゲーション再構築のタイミング

- Load 画面の EnterLoad() 時、**毎回** RebuildLoadNavigation() を呼ぶこと
- Slot 要素が追加/削除される場合は必ず呼び出し

### マウスクリック + Gamepad の切り替え

- 現状では実装していない（要件なし）
- 将来的に必要な場合：InputSystem.onActionChange で最後に使われたデバイス種別を検出

### EventSystem.SetSelectedGameObject(null)

- Save 画面の「セーブしています」表示時は null を設定
- これにより Submit が反応しなくなる

### Settings 画面の詳細実装

- Settings 画面は複雑なため、**別途 SettingsView, SettingsPresenter の仕様書を作成予定**
- 当仕様書では全体フローのみ記載
