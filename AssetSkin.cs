using System.IO;
using System.Reflection;
using UnityEngine;

namespace Cam
{
    internal static class AssetBundleManager
    {
        private static AssetBundle assetBundle;
        private static AudioClip clickSound;
        private static GUISkin uiSkin;

        public static AssetBundle GetAssetBundle()
        {
            if (assetBundle == null)
            {
                assetBundle = LoadAssetBundle("Cow50Mod.Assets.ui");
            }
            return assetBundle;
        }

        public static AudioClip GetClickSound()
        {
            if (clickSound == null)
            {
                var bundle = GetAssetBundle();
                if (bundle != null)
                {
                    clickSound = bundle.LoadAsset<AudioClip>("click");
                }
            }
            return clickSound;
        }

        public static GUISkin GetUISkin()
        {
            if (uiSkin == null)
            {
                var bundle = GetAssetBundle();
                if (bundle != null)
                {
                    uiSkin = bundle.LoadAsset<GUISkin>("Skin");
                }
            }
            return uiSkin;
        }

        private static AssetBundle LoadAssetBundle(string path)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            if (stream == null)
            {
                Debug.LogError($"Failed to find resource stream for path: {path}");
                return null;
            }

            AssetBundle bundle = AssetBundle.LoadFromStream(stream);
            return bundle;
        }
    }
}