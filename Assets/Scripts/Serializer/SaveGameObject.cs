using System;
using System.IO;
using StateManager;
using UnityEngine;

namespace Serializer
{
    public class SaveGameObject : MonoBehaviour
    {
        private Type queuedState;
        private bool _skipSaveOnQuit;

        public void Awake()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "save1.json");

            if (!File.Exists(savePath))
            {
                Debug.Log("Save file doesn't exist");
                return;
            }

            try
            {
                string json = File.ReadAllText(savePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogWarning("Save file exists but is empty. Ignoring it.");
                    return;
                }

                queuedState = SaveFile.FromJSON(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to read save file: {ex.Message}");
            }
        }

        public void DeleteSave()
        {
            _skipSaveOnQuit = true;
            string savePath = Path.Combine(Application.persistentDataPath, "save1.json");

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Save file deleted: " + savePath);
            }
            else
            {
                Debug.Log("No save file to delete.");
            }
        }

        public void Start()
        {
            Debug.Log("Queued state : " + queuedState);

            if (queuedState != null)
            {
                GameStateManager.Instance.Change(queuedState);
                queuedState = null;
            }
        }

        public void Save()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "save1.json");

            string save;
            try
            {
                save = SaveFile.currentJSON;
                if (string.IsNullOrWhiteSpace(save))
                {
                    save = SaveFile.ToJSON();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to build save data: {ex.Message}");
                return;
            }

            if (string.IsNullOrWhiteSpace(save))
            {
                Debug.LogError("Save data was empty. Aborting save.");
                return;
            }

            try
            {
                File.WriteAllText(savePath, save);
                Debug.Log("Saved " + savePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write save file: {ex.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            if (_skipSaveOnQuit)
                return;

            Save();
        }
    }
}
