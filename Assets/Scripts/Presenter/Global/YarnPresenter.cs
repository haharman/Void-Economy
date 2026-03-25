using Model;
using View;
using Yarn.Unity;
using UnityEngine;

namespace Presenter
{
    public class YarnPresenter
    {
        private DialogueRunner _dialogueRunner;
        private YarnView _yarnView;
        private InventoryModel _inventoryModel;

        public YarnPresenter(YarnView view, InventoryModel inventoryModel)
        {
            _yarnView = view;
            _inventoryModel = inventoryModel;
            _dialogueRunner = view.dialogueRunner;

            // 1. Yarnのアクション(カスタムコマンド)を登録する
            RegisterCommands();

            // 2. 会話開始・終了時のイベントをフックする
            _dialogueRunner.onDialogueComplete.AddListener(OnDialogueEnded);
        }

        private void RegisterCommands()
        {
            // Yarn内で <<give_item "Apple" 1>> と呼ばれた時の処理
            _dialogueRunner.AddCommandHandler<string, int>("give_item", (itemName, amount) =>
            {
                // Modelを更新する (Viewは直接操作しない)
                //_inventoryModel.AddItem(itemName, amount);
                Debug.Log($"[Yarn] アイテム追加: {itemName} x {amount}");
            });

            // Yarn内で <<take_money 100>> と呼ばれた時の処理
            _dialogueRunner.AddCommandHandler<int>("take_money", (amount) =>
            {
                // EconomyEngineやPlayerModelなどを更新
            });
        }

        // 外部（例えばPlayerInputPresenter）から呼ばれる会話開始用メソッド
        public void StartDialogueWithEntity(Entity target)
        {
            if (string.IsNullOrEmpty(target.dialogueNodeName)) return;

            _yarnView.ShowDialogueUI();
            _dialogueRunner.StartDialogue(target.dialogueNodeName);
        }

        private void OnDialogueEnded()
        {
            _yarnView.HideDialogueUI();
        }
    }
}