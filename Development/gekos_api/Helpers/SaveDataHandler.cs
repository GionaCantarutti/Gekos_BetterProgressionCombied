using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gekos_api.Helpers;

namespace gekos_api.Helpers
{
    class SaveDataHandler
    {

        private static void SaveAllData(string fileName, Object data)
        {
            string savePath = Path.Combine(Plugin.PluginFolder, $"{fileName}.json");

            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText(savePath, json);
        }

        private static T LoadAllData<T>(string fileName)
        {
            string savePath = Path.Combine(Plugin.PluginFolder, $"{fileName}.json");

            string json = File.ReadAllText(savePath);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Writes data in the given fileName corresponidng to the current logged profile
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        public static void SaveProfileData<T>(string fileName, T data)
        {
            Dictionary<string, T> oldData;

            try
            {
                oldData = LoadAllData<Dictionary<string, T>>(fileName);
            } catch (FileNotFoundException)
            {
                oldData = new Dictionary<string, T>();
            }

            oldData[Utils.GetPlayerProfile().AccountId] = data;

            SaveAllData(fileName, oldData);
        }

        /// <summary>
        /// Reads data from the given fileName corresponidng to the current logged profile
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns>True if data was found. False if no file of that name was present or if current profile didn't have any data in it</returns>
        public static bool LoadProfileData<T>(string fileName, out T data)
        {
            try
            {
                data = LoadAllData<Dictionary<string, T>>(fileName)[Utils.GetPlayerProfile().AccountId];
                return true;
            } catch (Exception ex) when (ex is KeyNotFoundException || ex is FileNotFoundException)
            {
                data = default;
                return false;
            }
        }

    }
}
