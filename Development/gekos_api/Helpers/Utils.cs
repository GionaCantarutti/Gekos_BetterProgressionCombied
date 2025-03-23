using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace gekos_api.Helpers
{
    class Utils
    {

        public static AssetBundle LoadBundle(string name)
        {
            var bundlePath = Path.Combine(Plugin.PluginFolder, name);
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                throw new Exception($"Error loading bundle: {bundlePath}");
            }

            return bundle;
        }

        public static GameObject LoadGameObject(string bundleName, string assetName)
        {
            return LoadBundle(bundleName).LoadAsset<GameObject>(assetName);
        }

    }
}
