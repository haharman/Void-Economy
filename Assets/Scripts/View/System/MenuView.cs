using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View
{
    public class MenuView : MonoBehaviour
    {
        [SerializeField] private GameObject menuObj;
        [SerializeField] private RectTransform menuRect;
        [SerializeField] private float slideInDuration;

        private bool _isTitleScene = false;
        private bool _isOpen = false;

        // ロード
        [SerializeField] private GameObject loadObj;
        [Serializable] private struct SlotComponents
        {
            public Button button;
            public TextMeshProUGUI name;
            public TextMeshProUGUI date;
            public TextMeshProUGUI id;
        }
        [SerializeField] private List<SlotComponents> slotList;
        [SerializeField] private Button newGameButton;
        [SerializeField] private string defaultSlotName;
        [SerializeField] private string defaultSlotId;

        // セーブ
        [SerializeField] private GameObject saveObj;
        [SerializeField] private TextMeshProUGUI savingText;
        [SerializeField] private Button saveOkButton;
        [SerializeField] private GameObject saveOkButtonContainer;

        // 終了
        [SerializeField] private GameObject quitObj;
        [SerializeField] private Button quitToTitleButton;
        [SerializeField] private Button quitToDesktopButton;
        [SerializeField] private Button quitCancelButton;

        // 設定
        [SerializeField] private GameObject settingsObj;

        // ホーム（ルート）
        [SerializeField] private GameObject homeObj;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button settingsButton;

        public event Action<int> OnLoadSlotPressed;
        public event Action OnNewGameSlotPressed;
        public event Action OnSaveStarted;
        public event Action OnQuitToTitle;
        public event Action OnQuitToDesktop;
        public event Action OnSettingsChanged;
        public event Action<int> OnMenuPanelChanged;

        public void Awake()
        {
            SetupPointerClickHandlers();
            SetupButtonColors();

            loadButton.onClick.AddListener(HandleLoadButtonClick);
            saveButton.onClick.AddListener(HandleSaveButtonClick);
            quitButton.onClick.AddListener(HandleQuitButtonClick);
            settingsButton.onClick.AddListener(HandleSettingsButtonClick);
        }

        private void SetupButtonColors()
        {
            SetButtonColors(loadButton);
            SetButtonColors(saveButton);
            SetButtonColors(quitButton);
            SetButtonColors(settingsButton);
            SetButtonColors(quitToTitleButton);
            SetButtonColors(quitToDesktopButton);
            SetButtonColors(quitCancelButton);
            SetButtonColors(saveOkButton);

            foreach (var slot in slotList)
            {
                SetButtonColors(slot.button);
            }
        }

        private void SetButtonColors(Button button)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            colors.selectedColor = new Color(0.5f, 0.8f, 1f, 1f);
            colors.highlightedColor = new Color(0.4f, 0.7f, 1f, 1f);
            colors.pressedColor = new Color(0.3f, 0.6f, 0.9f, 1f);
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            button.colors = colors;
        }

        private void OnDestroy()
        {
            loadButton.onClick.RemoveAllListeners();
            saveButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
        }

        private void SetupPointerClickHandlers()
        {
            AddPointerClickHandler(loadButton);
            AddPointerClickHandler(saveButton);
            AddPointerClickHandler(quitButton);
            AddPointerClickHandler(settingsButton);
            AddPointerClickHandler(quitToTitleButton);
            AddPointerClickHandler(quitToDesktopButton);
            AddPointerClickHandler(quitCancelButton);
            AddPointerClickHandler(saveOkButton);
        }

        private void AddPointerClickHandler(Button button)
        {
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) =>
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            });
            trigger.triggers.Add(entry);
        }
        
        #region メニュー全体の表示/アニメーション
        public void OnEnterTitle()
        {
            Debug.Log("[MenuView] OnEnterTitle");
            _isTitleScene = true;
            _isOpen = true;
            menuObj.SetActive(true);
            homeObj.SetActive(true);
            loadObj.SetActive(false);
            saveObj.SetActive(false);
            quitObj.SetActive(false);
            settingsObj.SetActive(false);
            menuRect.DOKill();
            menuRect.anchoredPosition = Vector2.zero;
            EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
        }

        public void OnExitTitle()
        {
            _isTitleScene = false;
            _isOpen = false;
            menuObj.SetActive(false);
            menuRect.DOKill();
            menuRect.anchoredPosition = new Vector2(-menuRect.rect.width, 0);
        }

        public void OnMenuOpen()
        {
            if (_isTitleScene) return;
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
                    EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
                });
        }

        public void OnMenuClose()
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
                });
        }
        #endregion

        #region Menu ルートボタン
        private void HandleLoadButtonClick()
        {
            EnterLoad(new List<SaveDataInfo>());
            OnMenuPanelChanged?.Invoke(1);
        }

        private void HandleSaveButtonClick()
        {
            EnterSave();
            OnMenuPanelChanged?.Invoke(2);
        }

        private void HandleQuitButtonClick()
        {
            EnterQuit();
            OnMenuPanelChanged?.Invoke(3);
        }

        private void HandleSettingsButtonClick()
        {
            EnterSettings();
            OnMenuPanelChanged?.Invoke(4);
        }

        private void SetupMenuNavigation()
        {
            Navigation nav;

            nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp = settingsButton;
            nav.selectOnDown = saveButton;
            loadButton.navigation = nav;

            nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp = loadButton;
            nav.selectOnDown = quitButton;
            saveButton.navigation = nav;

            nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp = saveButton;
            nav.selectOnDown = settingsButton;
            quitButton.navigation = nav;

            nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp = quitButton;
            nav.selectOnDown = loadButton;
            settingsButton.navigation = nav;
        }
        #endregion

        #region Load 画面
        public void EnterLoad(List<SaveDataInfo> slots)
        {
            Debug.Log("ENTER LOAD");
            SetupMenuNavigation();
            homeObj.SetActive(false);
            loadObj.SetActive(true);
            saveObj.SetActive(false);
            quitObj.SetActive(false);
            settingsObj.SetActive(false);

            int displayCount = Mathf.Min(slots.Count, slotList.Count);
            Debug.Log("[MenuView] displayCount: " + displayCount);

            // スロット情報を表示
            for (int i = 0; i < displayCount; i++)
            {
                slotList[i].name.text = slots[i].name;
                slotList[i].date.text = slots[i].savedAt;
                slotList[i].id.text = slots[i].id.ToString();
            }
            for (int i = displayCount; i < slotList.Count; i++)
            {
                slotList[i].name.text = defaultSlotName;
                slotList[i].date.text = "";
                slotList[i].id.text = defaultSlotId;
            }
            if (displayCount > 0)
            {
                // slotListのナビゲーション設定
                for (int i = 0; i < displayCount; i++)
                {
                    int index = i;
                    slotList[i].button.onClick.AddListener(() =>
                    {
                        OnLoadSlotPressed?.Invoke(index);
                    });

                    Navigation customNavigation = slotList[i].button.navigation;
                    customNavigation.mode = Navigation.Mode.Explicit;
                    if(i != 0)
                        customNavigation.selectOnUp = slotList[i - 1].button;
                    if(i == displayCount - 1)
                        customNavigation.selectOnDown = newGameButton;
                    else
                        customNavigation.selectOnDown = slotList[i + 1].button;
                    slotList[i].button.navigation = customNavigation;
                }
                // newGameButtonのナビゲーション設定
                Navigation newGameButtonNavigation = newGameButton.navigation;
                newGameButtonNavigation.mode = Navigation.Mode.Explicit;
                newGameButtonNavigation.selectOnUp = slotList[displayCount - 1].button;
                newGameButton.navigation = newGameButtonNavigation;
                
                EventSystem.current.SetSelectedGameObject(slotList[0].button.gameObject);
            }
            else
            {
                Navigation newGameButtonNavigation = newGameButton.navigation;
                newGameButtonNavigation.mode = Navigation.Mode.None;
                newGameButton.navigation = newGameButtonNavigation;
                
                EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
            }
            newGameButton.onClick.AddListener(() =>
            {
                OnNewGameSlotPressed?.Invoke();
            });
        }

        public void ExitLoad()
        {
            loadObj.SetActive(false);
            homeObj.SetActive(true);
            foreach (var slot in slotList)
            {
                slot.button.onClick.RemoveAllListeners();
            }
            EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
        }
        #endregion

        #region Save 画面
        public void EnterSave()
        {
            Debug.Log("ENTER SAVE");
            homeObj.SetActive(false);
            loadObj.SetActive(false);
            saveObj.SetActive(true);
            quitObj.SetActive(false);
            settingsObj.SetActive(false);

            savingText.gameObject.SetActive(true);
            saveOkButtonContainer.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void OnSaveComplete()
        {
            savingText.gameObject.SetActive(false);
            saveOkButtonContainer.SetActive(true);
            EventSystem.current.SetSelectedGameObject(saveOkButton.gameObject);

            saveOkButton.onClick.AddListener(() =>
            {
                OnSaveStarted?.Invoke();
                ExitSave();
            });
        }

        public void ExitSave()
        {
            saveObj.SetActive(false);
            homeObj.SetActive(true);
            saveOkButton.onClick.RemoveAllListeners();
            EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
        }
        #endregion

        #region Quit 画面
        public void EnterQuit()
        {
            Debug.Log("ENTER QUIT");
            homeObj.SetActive(false);
            loadObj.SetActive(false);
            saveObj.SetActive(false);
            quitObj.SetActive(true);
            settingsObj.SetActive(false);

            quitObj.SetActive(true);

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

            EventSystem.current.SetSelectedGameObject(quitToTitleButton.gameObject);
        }

        public void ExitQuit()
        {
            quitObj.SetActive(false);
            homeObj.SetActive(true);
            quitToTitleButton.onClick.RemoveAllListeners();
            quitToDesktopButton.onClick.RemoveAllListeners();
            quitCancelButton.onClick.RemoveAllListeners();
            EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
        }
        #endregion

        #region Settings 画面
        public void EnterSettings()
        {
            Debug.Log("ENTER SETTINGS");
            homeObj.SetActive(false);
            loadObj.SetActive(false);
            saveObj.SetActive(false);
            quitObj.SetActive(false);
            settingsObj.SetActive(true);
        }

        public void ExitSettings()
        {
            settingsObj.SetActive(false);
            homeObj.SetActive(true);
            EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
        }
        #endregion
    }
}
