using System;
using System.Collections.Generic;
using Yarn.Unity;

namespace Model
{
    public class YarnModel
    {
        private SaveData _saveData;
        private InMemoryVariableStorage _yarnStorage;
        
        public YarnModel( InMemoryVariableStorage yarnStorage)
        {
            _yarnStorage = yarnStorage;
        }

        public void SaveVariablesToSaveData(SaveData saveData)
        {
            saveData.yarn.Clear();

            // Yarnから現在の全変数を取得
            var (floats, strings, bools) = _yarnStorage.GetAllVariables();

            foreach (var kvp in floats)
                saveData.yarn.Add(new YarnVariable { key = kvp.Key, value = kvp.Value.ToString(), type = "Float" });

            foreach (var kvp in strings)
                saveData.yarn.Add(new YarnVariable { key = kvp.Key, value = kvp.Value, type = "String" });

            foreach (var kvp in bools)
                saveData.yarn.Add(new YarnVariable { key = kvp.Key, value = kvp.Value.ToString(), type = "Bool" });
        }

        // --- ロード時の処理 ---
        public void LoadVariablesFromSaveData(SaveData saveData)
        {
            _yarnStorage.Clear();

            var floats = new Dictionary<string, float>();
            var strings = new Dictionary<string, string>();
            var bools = new Dictionary<string, bool>();

            // 保存されたリストから型ごとに復元
            foreach (var variable in saveData.yarn)
            {
                if (variable.type == "Float" && float.TryParse(variable.value, out float fVal))
                    floats[variable.key] = fVal;
                else if (variable.type == "String")
                    strings[variable.key] = variable.value;
                else if (variable.type == "Bool" && bool.TryParse(variable.value, out bool bVal))
                    bools[variable.key] = bVal;
            }

            // Yarnに一括でセット
            _yarnStorage.SetAllVariables(floats, strings, bools);
        }
        
    }
}