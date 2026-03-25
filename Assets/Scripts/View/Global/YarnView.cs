using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

namespace View
{
    
    public class YarnView : MonoBehaviour
    {
        public DialogueRunner dialogueRunner;
        public InMemoryVariableStorage variableStorage;
        //[SerializeField] private OptionListView _optionListView; // YarnのOptionsListViewをアタッチ

        public void ShowDialogueUI() { gameObject.SetActive(true); }
        public void HideDialogueUI() { gameObject.SetActive(false); }
        public bool isPlaying => dialogueRunner.IsDialogueRunning;

        private void Start()
        {
            // 選択肢が表示された時のイベント（Yarnの標準デリゲートなどにフックするか、
            // もしくはOptionsListViewの派生クラスを作る方法があります）
            // 簡易的に実装する場合、Update等で子要素がアクティブになったか監視するか、
            // Yarnの DialogueRunner.onDialogueComplete などを活用します。
        }
        
        /// <summary>
        /// 最初の選択肢にフォーカスする。
        /// マウスを使わずにキーボードやゲームパッドで選択肢を操作するために必要。
        /// </summary>
        public void FocusFirstOption()
        {
            /*var firstButton = _optionListView.GetComponentInChildren<UnityEngine.UI.Button>();
        
            if (firstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            }*/
        }
        
        private void Awake()
        {
            // 別のYarnViewが存在する場合は自身を破棄する
            var existingSystems = FindObjectsOfType<YarnView>();
            if (existingSystems.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}