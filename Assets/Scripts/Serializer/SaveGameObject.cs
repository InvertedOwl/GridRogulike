using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using StateManager;
using UnityEngine;

namespace Serializer
{
    public class SaveGameObject : MonoBehaviour
    {
        private const string SaveFileName = "save1.sav";
        private const string LegacySaveFileName = "save1.json";
        private const string TempFileExtension = ".tmp";
        private const string BackupFileExtension = ".bak";
        private const int SaveEnvelopeVersion = 1;
        private const string HmacKey = "hexes-love-hot-sauce-and-save-scummers";

        private class SaveEnvelope
        {
            public int version;
            public string payload;
            public string hmac;
        }

        private Type queuedState;
        private bool _skipSaveOnQuit;
        private string _loadedSaveJson;

        public void Awake()
        {
            if (!TryReadSaveJson(out string json, out string error))
            {
                Debug.Log(error);
                return;
            }

            queuedState = SaveFile.FromJSON(json);
            _loadedSaveJson = SaveFile.currentJSON;
        }

        public void DeleteSave()
        {
            _skipSaveOnQuit = true;
            DeleteSaveFiles();
        }

        public void Start()
        {
            Debug.Log("Queued state : " + queuedState);

            if (queuedState != null)
            {
                GameStateManager.Instance.Change(queuedState);
                SaveFile.currentJSON = _loadedSaveJson;
                _loadedSaveJson = null;
                queuedState = null;
            }
        }

        public void Save()
        {
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

            TryWriteSaveJson(save);
        }

        private void OnApplicationQuit()
        {
            if (_skipSaveOnQuit)
                return;

            Save();
        }

        public static bool HasValidSave()
        {
            return TryReadSaveJson(out _, out _);
        }

        public static void PersistCheckpoint()
        {
            string save = SaveFile.currentJSON;
            if (string.IsNullOrWhiteSpace(save))
            {
                save = SaveFile.ToJSON();
            }

            TryWriteSaveJson(save);
        }

        public static bool TryReadSaveJson(out string json, out string error)
        {
            json = null;
            error = null;

            if (TryReadValidSaveAtPath(SavePath, out json, out error))
            {
                return true;
            }

            string primaryError = error;
            if (TryReadValidSaveAtPath(BackupPath, out json, out error))
            {
                Debug.LogWarning($"Primary save was invalid ({primaryError}). Recovered backup save.");
                TryWriteSaveJson(json);
                return true;
            }

            string backupError = error;
            if (TryReadValidSaveAtPath(LegacySavePath, out json, out error))
            {
                Debug.LogWarning("Recovered legacy signed save and rewrote it as .sav.");
                TryWriteSaveJson(json);
                return true;
            }

            string legacyError = error;
            if (TryReadValidSaveAtPath(LegacyBackupPath, out json, out error))
            {
                Debug.LogWarning("Recovered legacy signed backup save and rewrote it as .sav.");
                TryWriteSaveJson(json);
                return true;
            }

            error = $"Save file is invalid. Primary: {primaryError}. Backup: {backupError}. Legacy: {legacyError}. Legacy backup: {error}";
            return false;
        }

        public static bool TryWriteSaveJson(string save)
        {
            if (string.IsNullOrWhiteSpace(save))
            {
                Debug.LogError("Save data was empty. Aborting save.");
                return false;
            }

            if (!SaveFile.TryValidateJSON(save, out string validationError))
            {
                Debug.LogError($"Save data failed validation. Aborting save. {validationError}");
                return false;
            }

            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);

                string savePath = SavePath;
                string tempPath = TempPath;
                string backupPath = BackupPath;

                if (File.Exists(tempPath))
                    File.Delete(tempPath);

                File.WriteAllText(tempPath, WrapPayload(save));

                if (File.Exists(savePath))
                {
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);

                    File.Replace(tempPath, savePath, backupPath, true);
                }
                else
                {
                    File.Move(tempPath, savePath);
                }

                Debug.Log("Saved " + savePath);
                return true;
            }
            catch (Exception ex)
            {
                TryDeleteTempSave();
                Debug.LogError($"Failed to write save file: {ex.Message}");
                return false;
            }
        }

        public static void DeleteSaveFiles()
        {
            bool deletedAny = false;
            deletedAny |= TryDeleteFile(SavePath);
            deletedAny |= TryDeleteFile(TempPath);
            deletedAny |= TryDeleteFile(BackupPath);
            deletedAny |= TryDeleteFile(LegacySavePath);
            deletedAny |= TryDeleteFile(LegacyTempPath);
            deletedAny |= TryDeleteFile(LegacyBackupPath);

            if (deletedAny)
                Debug.Log("Save files deleted.");
            else
                Debug.Log("No save file to delete.");
        }

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        private static string TempPath => SavePath + TempFileExtension;
        private static string BackupPath => SavePath + BackupFileExtension;
        private static string LegacySavePath => Path.Combine(Application.persistentDataPath, LegacySaveFileName);
        private static string LegacyTempPath => LegacySavePath + TempFileExtension;
        private static string LegacyBackupPath => LegacySavePath + BackupFileExtension;

        private static bool TryReadValidSaveAtPath(string path, out string json, out string error)
        {
            json = null;
            error = null;

            if (!File.Exists(path))
            {
                error = $"File does not exist: {path}";
                return false;
            }

            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                error = $"Failed to read {path}: {ex.Message}";
                return false;
            }

            if (!TryUnwrapPayload(json, out string payload, out error))
            {
                error = $"{path}: {error}";
                json = null;
                return false;
            }

            if (!SaveFile.TryValidateJSON(payload, out error))
            {
                error = $"{path}: {error}";
                json = null;
                return false;
            }

            json = payload;

            return true;
        }

        private static string WrapPayload(string payload)
        {
            string compressedPayload = CompressPayload(payload);
            SaveEnvelope envelope = new SaveEnvelope
            {
                version = SaveEnvelopeVersion,
                payload = compressedPayload,
                hmac = ComputeHmac(compressedPayload)
            };

            return JsonConvert.SerializeObject(envelope, Formatting.None);
        }

        private static bool TryUnwrapPayload(string envelopeJson, out string payload, out string error)
        {
            payload = null;
            error = null;

            if (string.IsNullOrWhiteSpace(envelopeJson))
            {
                error = "Save envelope is empty.";
                return false;
            }

            SaveEnvelope envelope;
            try
            {
                envelope = JsonConvert.DeserializeObject<SaveEnvelope>(envelopeJson);
            }
            catch (Exception ex)
            {
                error = $"Save envelope could not be deserialized: {ex.Message}";
                return false;
            }

            if (envelope == null)
            {
                error = "Save envelope deserialized to null.";
                return false;
            }

            if (envelope.version != SaveEnvelopeVersion)
            {
                error = $"Save envelope version {envelope.version} is not supported.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(envelope.payload))
            {
                error = "Save envelope is missing payload.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(envelope.hmac))
            {
                error = "Save envelope is missing HMAC.";
                return false;
            }

            string expectedHmac = ComputeHmac(envelope.payload);
            if (!FixedTimeEquals(envelope.hmac, expectedHmac))
            {
                error = "Save HMAC did not match payload. Save may have been edited or corrupted.";
                return false;
            }

            if (TryDecompressPayload(envelope.payload, out payload, out error))
                return true;

            if (SaveFile.TryValidateJSON(envelope.payload, out _))
            {
                payload = envelope.payload;
                error = null;
                return true;
            }

            error = $"Save payload could not be decompressed: {error}";
            return false;
        }

        private static string CompressPayload(string payload)
        {
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(payloadBytes, 0, payloadBytes.Length);
                }

                return Convert.ToBase64String(output.ToArray());
            }
        }

        private static bool TryDecompressPayload(string compressedPayload, out string payload, out string error)
        {
            payload = null;
            error = null;

            try
            {
                byte[] compressedBytes = Convert.FromBase64String(compressedPayload);
                using (MemoryStream input = new MemoryStream(compressedBytes))
                using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
                using (MemoryStream output = new MemoryStream())
                {
                    gzip.CopyTo(output);
                    payload = Encoding.UTF8.GetString(output.ToArray());
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static string ComputeHmac(string payload)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(HmacKey);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                return Convert.ToBase64String(hmac.ComputeHash(payloadBytes));
            }
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            byte[] leftBytes;
            byte[] rightBytes;

            try
            {
                leftBytes = Convert.FromBase64String(left);
                rightBytes = Convert.FromBase64String(right);
            }
            catch (FormatException)
            {
                return false;
            }

            if (leftBytes.Length != rightBytes.Length)
                return false;

            int difference = 0;
            for (int i = 0; i < leftBytes.Length; i++)
            {
                difference |= leftBytes[i] ^ rightBytes[i];
            }

            return difference == 0;
        }

        private static void TryDeleteTempSave()
        {
            TryDeleteFile(TempPath);
        }

        private static bool TryDeleteFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return false;

                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save file {path}: {ex.Message}");
                return false;
            }
        }
    }
}
