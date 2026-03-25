using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Model.Economy;
using Model.Physics;

namespace Model
{
    /// <summary>
    /// データの永続化（ローカルファイルへのSave/Load）を担うクラス
    /// </summary>
    public class DataPersister
    {
        // New Game時の初期セーブデータ
        private SaveData _defaultSaveData;
        
        // 保存するファイル名
        private const string SaveFileName = "savedata.json";
        
        // セーブファイルのパス
        private string saveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public DataPersister(SaveData defaultSaveData)
        {
            _defaultSaveData = defaultSaveData;
        }

        /// <summary>
        /// 各クラスからデータを集約する
        /// SaveDataをJSON化してファイルに保存する
        /// </summary>
        /// <param name="data">各Modelから集約されたセーブデータ</param>
        public void Save(SaveData data)
        {
            Debug.Log("[DataPersister] Save開始");
            try
            {
                // true→PrettyPrintでJSON出力
                string json = JsonUtility.ToJson(data, true);
                
                // 同名ファイルに書き込み
                File.WriteAllText(saveFilePath, json);
                
                Debug.Log($"[DataPersister] Save成功: {saveFilePath}");
            }
            catch (Exception e)
            {
                // ファイル書き込み中のエラー（容量不足やアクセス権限など）をキャッチ
                Debug.LogError($"[DataPersister] Save失敗:\n{e.Message}");
            }
        }

        /// <summary>
        /// ファイルからJSONを読み込む
        /// SaveDataとして復元する
        /// 各クラスに配信する
        /// </summary>
        public SaveData Load()
        {
            if (File.Exists(saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    SaveData loadedData = JsonUtility.FromJson<SaveData>(json);
                    
                    Debug.Log("[DataPersister] Load成功");
                    return loadedData;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DataPersister] Load失敗:\n{e.Message}");
                }
            }

            return null;
        }
        
        /// <summary>
        /// セーブデータを削除
        /// </summary>
        public void DeleteSaveData()
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                Debug.Log("[DataPersister] セーブデータを削除しました。");
            }
        }

        
    }
}   