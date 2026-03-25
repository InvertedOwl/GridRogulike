using System;
using System.IO;
using UnityEngine;

namespace Serializer
{
    public class SaveGameObject : MonoBehaviour
    {
        public void Awake()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "save1.json");
            if (File.Exists(savePath))
            {
                SaveFile.FromJSON(File.ReadAllText(savePath));
            }
            else
            {
                Debug.Log("Save file doesn't exist");
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