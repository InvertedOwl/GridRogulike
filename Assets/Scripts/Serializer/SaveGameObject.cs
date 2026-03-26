using System;
using System.IO;
using StateManager;
using UnityEngine;

namespace Serializer
{
    public class SaveGameObject : MonoBehaviour
    {
        private Type queuedState;
        
        public void Awake ()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "save1.json");
            if (File.Exists(savePath))
            {
                queuedState = SaveFile.FromJSON(File.ReadAllText(savePath));
            }
            else
            {
                Debug.Log("Save file doesn't exist");
            }
        }

        public void DeleteSave()
        {
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
            string save = SaveFile.currentJSON;
            File.WriteAllText(savePath, save);
            Debug.Log("Saved " + savePath);
        }
    }
}