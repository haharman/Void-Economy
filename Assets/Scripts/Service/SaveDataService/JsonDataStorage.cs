using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Core;

namespace Service
{
    public class JsonDataStorage : IDataStorage
    {
        private const int MaxManualSaveSlots = 1;
        private const int MaxAutoSaveSlots = 5;

        private readonly string _savePath = Application.persistentDataPath;

        public List<SaveDataInfo> GetSaveDataInfoList()
        {
            var saveDataInfoList = new List<SaveDataInfo>();

            if (!Directory.Exists(_savePath))
            {
                return saveDataInfoList;
            }

            var files = Directory.GetFiles(_savePath, "*.json");

            foreach (var filePath in files)
            {
                if (!TryLoadInfo(filePath, out var info))
                {
                    continue;
                }
                saveDataInfoList.Add(info);
            }

            // Unix timestamp でソート（新しい順）
            saveDataInfoList.Sort((a, b) => b.savedAtUnixMs.CompareTo(a.savedAtUnixMs));

            return saveDataInfoList;
        }

        public bool ManualSave(SaveData saveData)
        {
            try
            {
                if (!Directory.Exists(_savePath))
                {
                    Directory.CreateDirectory(_savePath);
                }

                // 既存の手動セーブを削除
                DeleteExistingManualSaves();

                // 新しい ID を決定（全スロット中で最大の ID + 1）
                int newId = GetNextSaveId();
                saveData.info.id = newId;

                // JSON ファイルに保存
                return SaveToJson(saveData, false);
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonDataStorage] ManualSave failed: {e}");
                return false;
            }
        }

        public bool AutoSave(SaveData saveData)
        {
            try
            {
                if (!Directory.Exists(_savePath))
                {
                    Directory.CreateDirectory(_savePath);
                }

                // オートセーブスロットが満杯か確認
                var autoSaves = GetAutoSaveList();
                if (autoSaves.Count >= MaxAutoSaveSlots)
                {
                    // 最古のオートセーブを削除
                    DeleteOldestAutoSave();
                }

                // 新しい ID を決定
                int newId = GetNextSaveId();
                saveData.info.id = newId;

                // JSON ファイルに保存
                return SaveToJson(saveData, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonDataStorage] AutoSave failed: {e}");
                return false;
            }
        }

        public bool TryLoad(int id, out SaveData saveData)
        {
            saveData = null;

            try
            {
                // 手動セーブを探す
                var manualPath = GetSaveFilePath(id, false);
                if (File.Exists(manualPath))
                {
                    return TryLoadFromJson(manualPath, out saveData);
                }

                // オートセーブを探す
                var autoPath = GetSaveFilePath(id, true);
                if (File.Exists(autoPath))
                {
                    return TryLoadFromJson(autoPath, out saveData);
                }

                Debug.LogError($"[JsonDataStorage] Save slot {id} not found");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonDataStorage] TryLoad failed: {e}");
                return false;
            }
        }

        private bool SaveToJson(SaveData saveData, bool isAutoSave)
        {
            try
            {
                string filePath = GetSaveFilePath(saveData.info.id, isAutoSave);

                // 既存ファイルを削除（エラー耐性）
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // JSON に変換して保存
                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(filePath, json);

                Debug.Log($"[JsonDataStorage] Saved to {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonDataStorage] SaveToJson failed: {e}");
                return false;
            }
        }

        private bool TryLoadFromJson(string filePath, out SaveData saveData)
        {
            saveData = null;

            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                string json = File.ReadAllText(filePath);
                saveData = JsonUtility.FromJson<SaveData>(json);

                // 基本検証
                if (saveData == null)
                {
                    Debug.LogError($"[JsonDataStorage] Failed to deserialize {filePath}");
                    return false;
                }

                // SaveDataInfo の有効性確認
                if (saveData.info.id < 0)
                {
                    Debug.LogError($"[JsonDataStorage] Invalid save data: negative id");
                    return false;
                }

                Debug.Log($"[JsonDataStorage] Loaded from {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonDataStorage] TryLoadFromJson failed: {e}");
                return false;
            }
        }

        private bool TryLoadInfo(string filePath, out SaveDataInfo info)
        {
            info = new SaveDataInfo();

            try
            {
                if (!TryLoadFromJson(filePath, out SaveData saveData))
                {
                    return false;
                }

                info = saveData.info;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void DeleteExistingManualSaves()
        {
            var manualSaves = GetManualSaveList();
            foreach (var info in manualSaves)
            {
                var filePath = GetSaveFilePath(info.id, false);
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        Debug.Log($"[JsonDataStorage] Deleted {filePath}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[JsonDataStorage] Failed to delete {filePath}: {e}");
                }
            }
        }

        private void DeleteOldestAutoSave()
        {
            var autoSaves = GetAutoSaveList();
            if (autoSaves.Count == 0)
            {
                return;
            }

            // Unix timestamp が最小のものが最古
            var oldest = autoSaves.OrderBy(x => x.savedAtUnixMs).First();
            var filePath = GetSaveFilePath(oldest.id, true);

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[JsonDataStorage] Deleted oldest auto-save: {filePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonDataStorage] Failed to delete {filePath}: {e}");
            }
        }

        private List<SaveDataInfo> GetManualSaveList()
        {
            var list = new List<SaveDataInfo>();

            if (!Directory.Exists(_savePath))
            {
                return list;
            }

            var files = Directory.GetFiles(_savePath, "*_manual.json");
            foreach (var filePath in files)
            {
                if (TryLoadInfo(filePath, out var info))
                {
                    list.Add(info);
                }
            }

            return list;
        }

        private List<SaveDataInfo> GetAutoSaveList()
        {
            var list = new List<SaveDataInfo>();

            if (!Directory.Exists(_savePath))
            {
                return list;
            }

            var files = Directory.GetFiles(_savePath, "*_auto.json");
            foreach (var filePath in files)
            {
                if (TryLoadInfo(filePath, out var info))
                {
                    list.Add(info);
                }
            }

            return list;
        }

        private int GetNextSaveId()
        {
            var allSaves = GetSaveDataInfoList();
            if (allSaves.Count == 0)
            {
                return 0;
            }

            return allSaves.Max(x => x.id) + 1;
        }

        private string GetSaveFilePath(int id, bool isAutoSave)
        {
            string fileName = isAutoSave ? $"{id}_auto.json" : $"{id}_manual.json";
            return Path.Combine(_savePath, fileName);
        }
    }
}