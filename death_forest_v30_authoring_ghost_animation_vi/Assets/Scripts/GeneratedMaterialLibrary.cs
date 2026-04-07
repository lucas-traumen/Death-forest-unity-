using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace HollowManor
{
    public static class GeneratedMaterialLibrary
    {
        private const string EditorGeneratedFolder = "Assets/Materials/Generated";

        public static Material Get(string key, Color color, float metallic, float smoothness, bool transparent, bool persistentAsset)
        {
#if UNITY_EDITOR
            if (persistentAsset && !Application.isPlaying)
            {
                return GetOrCreateEditorAsset(key, color, metallic, smoothness, transparent);
            }
#endif
            return UnityCompatibility.CreateLitMaterial(color, metallic, smoothness, transparent);
        }

#if UNITY_EDITOR
        private static Material GetOrCreateEditorAsset(string key, Color color, float metallic, float smoothness, bool transparent)
        {
            string safeKey = SanitizeKey(key);
            string assetPath = EditorGeneratedFolder + "/" + safeKey + ".mat";

            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            if (!AssetDatabase.IsValidFolder(EditorGeneratedFolder))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "Generated");
            }

            Material existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing == null)
            {
                existing = UnityCompatibility.CreateLitMaterial(color, metallic, smoothness, transparent);
                AssetDatabase.CreateAsset(existing, assetPath);
            }

            UnityCompatibility.ConfigureMaterial(existing, color, metallic, smoothness, transparent);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static string SanitizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return "GeneratedMaterial";
            }

            string safe = key.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(c, '_');
            }

            return safe.Replace(' ', '_');
        }
#endif
    }
}
