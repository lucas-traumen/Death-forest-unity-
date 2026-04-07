#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HollowManor.EditorTools
{
    [InitializeOnLoad]
    public static class ExternalGhostMaBootstrap
    {
        private const string ExternalAssetsRoot = "Assets/Resources/ExternalAssets";
        private const string ModelPath = ExternalAssetsRoot + "/ma.fbx";
        private const string PrefabPath = ExternalAssetsRoot + "/DF_External_Ghost_Ma.prefab";
        private static bool queued;

        static ExternalGhostMaBootstrap()
        {
            QueueEnsure();
        }

        [MenuItem("Death Forest/External Assets/Ensure Ghost Ma Prefab")]
        public static void EnsureGhostMaPrefabMenu()
        {
            EnsureGhostMaPrefab(true);
        }

        private static void QueueEnsure()
        {
            if (queued)
            {
                return;
            }

            queued = true;
            EditorApplication.delayCall += () =>
            {
                queued = false;
                EnsureGhostMaPrefab(false);
            };
        }

        public static bool EnsureGhostMaPrefab(bool interactive)
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!interactive)
                {
                    QueueEnsure();
                }
                return false;
            }

            Directory.CreateDirectory(ExternalAssetsRoot);
            AssetDatabase.Refresh();

            if (!File.Exists(ModelPath))
            {
                if (interactive)
                {
                    EditorUtility.DisplayDialog(
                        "Death Forest",
                        "Khong tim thay model ma.fbx tai:\n" + ModelPath,
                        "OK");
                }
                return false;
            }

            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existingPrefab != null)
            {
                ExternalAssetCatalogBuilder.RebuildCatalog(false);
                if (interactive)
                {
                    EditorUtility.DisplayDialog(
                        "Death Forest",
                        "Da co san prefab ghost Ma. Catalog da duoc rebuild lai.",
                        "OK");
                }
                return true;
            }

            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (modelAsset == null)
            {
                if (interactive)
                {
                    EditorUtility.DisplayDialog(
                        "Death Forest",
                        "Unity chua import duoc ma.fbx thanh model asset. Hay cho Unity import xong roi chay lai menu Ensure Ghost Ma Prefab.",
                        "OK");
                }
                return false;
            }

            GameObject root = new GameObject("DF_External_Ghost_Ma");
            try
            {
                ExternalAssetPrefabAdapter adapter = root.AddComponent<ExternalAssetPrefabAdapter>();
                adapter.role = ExternalAssetPrefabAdapter.AssetRole.Ghost;
                adapter.applyOnAwake = true;
                adapter.keepExistingColliders = false;
                adapter.normalizeGhostHeight = true;
                adapter.targetGhostHeight = 2.45f;

                GameObject modelInstance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
                if (modelInstance == null)
                {
                    modelInstance = Object.Instantiate(modelAsset);
                }

                modelInstance.name = "ma";
                modelInstance.transform.SetParent(root.transform, false);
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
                modelInstance.transform.localScale = Vector3.one;

                RemoveEditorOnlyNodes(root.transform);
                DisableGhostHelperRenderers(root.transform);

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ExternalAssetCatalogBuilder.RebuildCatalog(false);

            if (interactive)
            {
                EditorUtility.DisplayDialog(
                    "Death Forest",
                    "Da tao prefab ghost moi tai:\n" + PrefabPath + "\n\nCatalog runtime da duoc rebuild.",
                    "OK");
            }

            return true;
        }

        private static void DisableGhostHelperRenderers(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                string lowered = renderer.transform.name.ToLowerInvariant();
                if (lowered == "cube" || lowered.StartsWith("cube.") || lowered == "plane" || lowered.StartsWith("plane."))
                {
                    renderer.enabled = false;
                }
            }
        }

        private static void RemoveEditorOnlyNodes(Transform root)
        {
            List<GameObject> toDestroy = new List<GameObject>();
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform node = all[i];
                if (node == root)
                {
                    continue;
                }

                string lowered = node.name.ToLowerInvariant();
                if (lowered == "camera" || lowered.StartsWith("camera.") || lowered == "light" || lowered.StartsWith("light."))
                {
                    toDestroy.Add(node.gameObject);
                }
            }

            for (int i = 0; i < toDestroy.Count; i++)
            {
                if (toDestroy[i] != null)
                {
                    Object.DestroyImmediate(toDestroy[i]);
                }
            }
        }
    }
}
#endif
