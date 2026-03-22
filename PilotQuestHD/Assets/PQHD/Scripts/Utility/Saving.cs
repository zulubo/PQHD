using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace PQHD
{
    public static class Saving
    {
        public struct SaveState
        {
            public Inventory.SerializedInventory inventory;
            public bool[] plants;
            public Parvina.SerializedParvina parvina;
            public StoreRuntime.SerializedStore luminStore;
            public Settings.SettingsContainer settings;

            public static readonly SaveState Default = new()
            {
                settings = Settings.SettingsContainer.Default()
            };
        }

        public static SaveState State;
        public static bool Busy { get; private set; }

        public static Action<SaveState> OnLoadedState;
        
        private static string SaveDirectory => Application.persistentDataPath;
        private const bool UseEncryption = true;
        private const string EncryptionKey = "Campanella";
        private const string FileName = "save.data";
        private static string GetFilePath(string name) => SaveDirectory + "/" + name;

        private static Queue<Task> taskQueue = new();

        private static void Init()
        {
            if(taskManager == null || taskManager.Status != TaskStatus.Running)
            {
                taskManager = TaskManager();
            }
        }

        public static bool SaveFileExists() => File.Exists(GetFilePath(FileName));

        private static Task taskManager;

        private static async Task TaskManager()
        {
            while(true)
            {
                await Task.Yield();

                while(taskQueue.TryDequeue(out Task task))
                {
                    Busy = true;
                    await task;
                }
                Busy = false;
            }
        }
        
        /// <summary>
        /// Save state to disk.
        /// Adds to a task queue, may not execute immediately.
        /// </summary>
        public static void SaveToDisk()
        {
            Init();
            taskQueue.Enqueue(SaveTask(GetFilePath(FileName)));
        }

        /// <summary>
        /// Save state to disk.
        /// Adds to a task queue, may not execute immediately.
        /// </summary>
        private static async Task SaveTask(string path)
        {
            try
            {
                string serialized = await Task.Run(() => JsonConvert.SerializeObject(State, Formatting.None));
                if (UseEncryption) serialized = await Task.Run(() => Encrypt(serialized, EncryptionKey));
                await File.WriteAllTextAsync(path, serialized);
            }
            catch (Exception e)
            {
                Debug.LogError("Saving to disk failed: " + e);
            }
        }

        public static void LoadFromDisk()
        {
            Init();
            taskQueue.Enqueue(LoadTask(GetFilePath(FileName)));
        }

        
        /// <summary>
        /// Load state from disk.
        /// Adds to a task queue, may not execute immediately.
        /// Subscribe to OnLoadedState action to get results
        /// </summary>
        private static async Task LoadTask(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string serialized = await File.ReadAllTextAsync(path);
                    if (UseEncryption) serialized = await Task.Run(() => Decrypt(serialized, EncryptionKey));
                    State = await Task.Run(() => JsonConvert.DeserializeObject<SaveState>(serialized));
                }
                else
                {
                    State = SaveState.Default;
                }
                
                OnLoadedState?.Invoke(State);
            }
            catch (Exception e)
            {
                Debug.LogError("Loading from disk failed: " + e);
            }
        }


        private const string Salt = "ml6xVjhW9PhIhWHUtNEI1BxY1wZgNXTi";
        /// <summary>
        /// insecure encryption for obscurity only
        /// </summary>
        private static string Encrypt(string plainText, string key)
        {
            using Aes aes = Aes.Create();
            byte[] saltBytes = Encoding.UTF8.GetBytes(Salt);
            var keyBytes = new Rfc2898DeriveBytes(key, saltBytes, 10000);
            aes.Key = keyBytes.GetBytes(32);
            aes.IV = keyBytes.GetBytes(16);
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
                sw.Write(plainText);
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// insecure decryption for obscurity only
        /// </summary>
        public static string Decrypt(string cipherText, string key)
        {
            using Aes aes = Aes.Create();
            byte[] saltBytes = Encoding.UTF8.GetBytes(Salt);
            var keyBytes = new Rfc2898DeriveBytes(key, saltBytes, 10000);
            aes.Key = keyBytes.GetBytes(32);
            aes.IV = keyBytes.GetBytes(16);
            byte[] buffer = Convert.FromBase64String(cipherText);
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (var ms = new MemoryStream(buffer))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }


        /// <summary>
        /// Delete save file.
        /// Added to a task queue, may not execute immediately.
        /// Call LoadFromDisk after deleting if you want it to take effect in-game
        /// </summary>
        public static void DeleteSave(bool keepSettings)
        {
            Init();
            taskQueue.Enqueue(DeleteSaveTask(keepSettings));
        }

        private static async Task DeleteSaveTask(bool keepSettings)
        {
            Settings.SettingsContainer oldSettings = Settings.I.settings; 
            string path = GetFilePath(FileName);
            if(File.Exists(path)) File.Delete(path);
            if(keepSettings)
            {
                State.settings = oldSettings;
                await SaveTask(path);
            }
            await LoadTask(path);
        }
    }
}
