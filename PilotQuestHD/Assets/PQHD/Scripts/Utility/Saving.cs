using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PQHD
{
    public static class Saving
    {
        public struct SaveState
        {
            public Inventory.SerializedInventory inventory;
            public bool[] plants;
        }

        public static SaveState State;
        public static bool Busy { get; private set; }

        public static Action<SaveState> OnLoadedState;
        
        private static string SaveDirectory => Application.persistentDataPath;
        private const bool UseEncryption = true;
        private const string EncryptionKey = "Campanella";
        private const string FileName = "save.data";
        private static string GetFilePath(string name) => SaveDirectory + "/" + name;
        
        /// <summary>
        /// Save state to disk.
        /// Threaded.
        /// </summary>
        public static void SaveToDisk()
        {
            SaveTask(GetFilePath(FileName));
        }

        /// <summary>
        /// Load state from disk.
        /// Threaded.
        /// Subscribe to OnLoadedState action to get results
        /// </summary>
        private static async Task SaveTask(string path)
        {
            while (Busy) await Task.Yield();
            
            Busy = true;
            try
            {
                string serialized = await Task.Run(() => JsonConvert.SerializeObject(State, Formatting.None));
                if (UseEncryption) serialized = await Task.Run(() => Encrypt(serialized, EncryptionKey));
                await File.WriteAllTextAsync(path, serialized);
                
                Busy = false;
            }
            catch (Exception e)
            {
                Debug.LogError("Saving to disk failed: " + e);
                Busy = false;
            }
        }

        public static void LoadFromDisk()
        {
            LoadThreaded(GetFilePath(FileName));
        }
        
        private static async Task LoadThreaded(string path)
        {
            while (Busy) await Task.Yield();

            Busy = true;
            try
            {
                if (!File.Exists(path))
                {
                    State = default;
                    Busy = false;
                    return;
                }

                string serialized = await File.ReadAllTextAsync(path);
                if (UseEncryption) serialized = await Task.Run(()=> Decrypt(serialized, EncryptionKey));
                State = await Task.Run(() => JsonConvert.DeserializeObject<SaveState>(serialized));
                
                Busy = false;
                OnLoadedState?.Invoke(State);
            }
            catch (Exception e)
            {
                Debug.LogError("Loading from disk failed: " + e);
                Busy = false;
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
    }
}
