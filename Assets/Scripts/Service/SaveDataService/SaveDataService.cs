using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Core;
using Model;
using Model.Economy;

using Presenter;

namespace Service
{
    /// <summary>
    /// データの永続化（ローカルファイルへのSave/Load）を担うクラス
    /// </summary>
    public class SaveDataService : ISaveDataService
    {
        private readonly string CurrentVersion = "0.30";
        private readonly SystemType SystemType = SystemType.PC;

        private readonly HashSet<ISaveDataWriter> _writers = new();
        private readonly HashSet<ISaveDataReader> _readers = new();

        private SaveData _defaultSaveData;
        private IDataStorage _dataStorage;
        private SaveData _loadedData;

        private readonly Dictionary<string, Action<SaveData>> _migrations = new()
        {
            // v0.20 → v0.25 → v0.30 のマイグレーションチェーン
        };

        public SaveDataService(SaveData defaultSaveData)
        {
            _defaultSaveData = defaultSaveData;
            switch (SystemType)
            {
                case SystemType.PC:
                    _dataStorage = new JsonDataStorage();
                    break;
                default:
                    Debug.LogError($"[DataService] システムタイプ[{SystemType}]はサポートされていません");
                    break;
            }
        }

        public void RegisterWriter(ISaveDataWriter writer)
        {
            _writers.Add(writer);
        }

        public void RegisterReader(ISaveDataReader reader)
        {
            _readers.Add(reader);
        }

        public void UnregisterWriter(ISaveDataWriter writer)
        {
            _writers.Remove(writer);
        }

        public void UnregisterReader(ISaveDataReader reader)
        {
            _readers.Remove(reader);
        }

        public void Save(bool auto)
        {
            var data = new SaveData();

            // メタデータを設定
            data.info.version = CurrentVersion;
            data.info.autoSave = auto;
            data.info.savedAt = System.DateTime.UtcNow.ToString("o");
            data.info.savedAtUnixMs = new System.DateTimeOffset(System.DateTime.UtcNow).ToUnixTimeMilliseconds();

            // 全 Writer に WriteTo を呼び出し
            foreach (var writer in _writers)
            {
                try
                {
                    writer.WriteTo(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DataService] WriteTo failed: {e}");
                    throw;
                }
            }

            // IDataStorage に永続化を委譲
            bool success = auto ? _dataStorage.AutoSave(data) : _dataStorage.ManualSave(data);

            if (!success)
            {
                Debug.LogError("[DataService] Save failed");
            }
        }

        public void Load(int id = 0, bool newGame = false)
        {
            if (newGame)
            {
                LoadNewGame();
                return;
            }
            if (!_dataStorage.TryLoad(id, out SaveData data))
            {
                Debug.LogError($"[DataService] Failed to load save slot id: {id}");
                throw new System.InvalidOperationException($"Save slot {id} not found");
            }

            // バージョン互換性検証とマイグレーション
            if (data.info.version != CurrentVersion)
            {
                Debug.Log($"[DataService] Migrating from {data.info.version} to {CurrentVersion}");
                try
                {
                    Migrate(data, data.info.version, CurrentVersion);
                    data.info.version = CurrentVersion;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DataService] Migration failed: {e}");
                    throw;
                }
            }
            
            // 全 Reader に ReadFrom を呼び出し
            foreach (var reader in _readers)
            {
                try
                {
                    reader.ReadFrom(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DataService] ReadFrom failed: {e}");
                    throw;
                }
            }

            _loadedData = data;
        }

        private void LoadNewGame()
        {
            var data = _defaultSaveData.DeepCopy();
            
            // 全 Reader に ReadFrom を呼び出し
            foreach (var reader in _readers)
            {
                try
                {
                    reader.ReadFrom(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DataService] ReadFrom failed: {e}");
                    throw;
                }
            }
            
            _loadedData = data;
        }

        public List<SaveDataInfo> GetSaveSlots()
        {
            var slots = _dataStorage.GetSaveDataInfoList();
            // ID を降順でソート
            slots.Sort((a, b) => b.id.CompareTo(a.id));
            return slots;
        }

        private void Migrate(SaveData data, string fromVersion, string toVersion)
        {
            // fromVersion から toVersion へのマイグレーションチェーン実行
            var versions = new[] { "0.20", "0.25", "0.30" };

            int fromIdx = System.Array.IndexOf(versions, fromVersion);
            int toIdx = System.Array.IndexOf(versions, toVersion);

            if (fromIdx == -1 || toIdx == -1)
            {
                throw new System.InvalidOperationException($"Unknown version: {fromVersion} or {toVersion}");
            }

            for (int i = fromIdx; i < toIdx; i++)
            {
                string version = versions[i];
                if (_migrations.TryGetValue(version, out var migration))
                {
                    migration(data);
                }
            }
        }
    }
}   