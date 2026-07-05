using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Yarn.Unity;

namespace Model
{
    public class YarnModel
    {
        private DialogueRunner _dialogueRunner;
        private InMemoryVariableStorage _inMemoryVariableStorage;
        private GlobalStateModel _globalStateModel;
        
        // ダイアログ開始前の_globalStateModel.CurrentInputState.Valueを保持する
        private InputState _previousInputState;
        
        
        public YarnModel(DialogueRunner dialogueRunner, InMemoryVariableStorage inMemoryVariableStorage, GlobalStateModel globalStateModel)
        {
            _dialogueRunner = dialogueRunner;
            _inMemoryVariableStorage = inMemoryVariableStorage;
            _globalStateModel = globalStateModel;
            
            _dialogueRunner.onDialogueStart.AddListener(OnDialogueStart);
            _dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }

        public bool IsDialogueRunning => _dialogueRunner.IsDialogueRunning;
        
        public void StartDialogue(string nodeName)
        {
            _dialogueRunner.StartDialogue(nodeName);
        }

        [YarnCommand("Sample")]
        public static void Sample()
        {
            Debug.Log("[YarnModel] Sample Commandが呼ばれました");
        }
        
        private void OnDialogueStart()
        {
            Debug.Log("[YarnModel] OnDialogueStart");
            _previousInputState = _globalStateModel.CurrentInputState.Value;
            _globalStateModel.CurrentInputState.Value = InputState.Dialogue;
            _globalStateModel.InvokeOnDialogueStarted();
        }

        private void OnDialogueComplete()
        {
            Debug.Log("[YarnModel] OnDialogueComplete");
            _globalStateModel.CurrentInputState.Value = _previousInputState;
            _globalStateModel.InvokeOnDialogueCompleted();
        }

        #region セーブとロード
        public List<YarnVariable> SaveVariablesToSaveData()
        {
            List<YarnVariable> yarnVariables = new List<YarnVariable>();

            // Yarnから現在の全変数を取得
            var (floats, strings, bools) = _inMemoryVariableStorage.GetAllVariables();

            foreach (var kvp in floats)
                yarnVariables.Add(new YarnVariable { key = kvp.Key, value = kvp.Value.ToString(), type = "Float" });

            foreach (var kvp in strings)
                yarnVariables.Add(new YarnVariable { key = kvp.Key, value = kvp.Value, type = "String" });

            foreach (var kvp in bools)
                yarnVariables.Add(new YarnVariable { key = kvp.Key, value = kvp.Value.ToString(), type = "Bool" });
            
            return yarnVariables;
        }
        
        public void LoadVariablesFromSaveData(List<YarnVariable> yarnVariables)
        {
            _inMemoryVariableStorage.Clear();

            var floats = new Dictionary<string, float>();
            var strings = new Dictionary<string, string>();
            var bools = new Dictionary<string, bool>();

            // 保存されたリストから型ごとに復元
            foreach (var variable in yarnVariables)
            {
                if (variable.type == "Float" && float.TryParse(variable.value, out float fVal))
                    floats[variable.key] = fVal;
                else if (variable.type == "String")
                    strings[variable.key] = variable.value;
                else if (variable.type == "Bool" && bool.TryParse(variable.value, out bool bVal))
                    bools[variable.key] = bVal;
            }

            // Yarnに一括でセット
            _inMemoryVariableStorage.SetAllVariables(floats, strings, bools);
        }
        #endregion
    }
}