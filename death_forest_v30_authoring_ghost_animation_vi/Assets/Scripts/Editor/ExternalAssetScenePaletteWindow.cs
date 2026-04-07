#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HollowManor.EditorTools
{
    public sealed class ExternalAssetScenePaletteWindow : EditorWindow
    {
        private sealed class AssetItem
        {
            public string assetPath;
            public string name;
        }

        private readonly List<AssetItem> items = new List<AssetItem>();
        private Vector2 scroll;
        private string searchText = string.Empty;

        [MenuItem("Death Forest/External Assets/Open Scene Palette")]
        public static void OpenWindow()
        {
            ExternalAssetScenePaletteWindow window = GetWindow<ExternalAssetScenePaletteWindow>("DF Asset Palette");
            window.minSize = new Vector2(360f, 480f);
            window.RefreshItems();
            window.Show();
        }

        private void OnEnable()
        {
            RefreshItems();
        }

        private void OnGUI()
        {
            GUILayout.Label("Asset ngoai trong Scene", EditorStyles.boldLabel);
            GUILayout.Label("Import vao project xong la co the dat thang trong Scene, khong can Play.");

            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            searchText = EditorGUILayout.TextField("Tim", searchText);
            if (GUILayout.Button("Refresh", GUILayout.Width(90f)))
            {
                RefreshItems();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            if (GUILayout.Button("Rebuild External Asset Catalog", GUILayout.Height(26f)))
            {
                ExternalAssetCatalogBuilder.RebuildCatalog(true);
                RefreshItems();
            }

            GUILayout.Space(8f);
            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < items.Count; i++)
            {
                AssetItem item = items[i];
                if (!MatchesSearch(item))
                {
                    continue;
                }

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(item.name, EditorStyles.boldLabel);
                GUILayout.Label(item.assetPath, EditorStyles.miniLabel);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Dat tai pivot Scene", GUILayout.Height(24f)))
                {
                    SpawnIntoScene(item, SpawnMode.ScenePivot);
                }
                if (GUILayout.Button("Dat vao object dang chon", GUILayout.Height(24f)))
                {
                    SpawnIntoScene(item, SpawnMode.UnderSelection);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }

        private bool MatchesSearch(AssetItem item)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            string token = searchText.Trim().ToLowerInvariant();
            return item.name.ToLowerInvariant().Contains(token) || item.assetPath.ToLowerInvariant().Contains(token);
        }

        private void RefreshItems()
        {
            items.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { ExternalAssetCatalogBuilder.SearchRoot });
            Array.Sort(guids, StringComparer.Ordinal);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    continue;
                }

                items.Add(new AssetItem
                {
                    assetPath = assetPath,
                    name = Path.GetFileNameWithoutExtension(assetPath)
                });
            }
        }

        private enum SpawnMode
        {
            ScenePivot,
            UnderSelection
        }

        private static void SpawnIntoScene(AssetItem item, SpawnMode mode)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(item.assetPath);
            if (prefab == null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                return;
            }

            Transform parent = FindOrCreateExternalAssetRoot();
            Undo.RegisterCreatedObjectUndo(instance, "Spawn External Asset");
            instance.transform.SetParent(parent, true);

            if (mode == SpawnMode.UnderSelection && Selection.activeTransform != null)
            {
                instance.transform.SetParent(Selection.activeTransform, true);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
            }
            else
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                Vector3 pivot = sceneView != null ? sceneView.pivot : Vector3.zero;
                instance.transform.position = new Vector3(pivot.x, Mathf.Max(0f, pivot.y), pivot.z);
                instance.transform.rotation = Quaternion.identity;
            }

            Selection.activeGameObject = instance;
            EditorSceneManager.MarkSceneDirty(instance.scene);
        }

        private static Transform FindOrCreateExternalAssetRoot()
        {
            DeathForestSceneRoot sceneRoot = UnityEngine.Object.FindAnyObjectByType<DeathForestSceneRoot>();
            if (sceneRoot != null && sceneRoot.ExternalAssetRoot != null)
            {
                return sceneRoot.ExternalAssetRoot;
            }

            GameObject root = GameObject.Find("SceneExternalAssets");
            if (root == null)
            {
                root = new GameObject("SceneExternalAssets");
            }

            return root.transform;
        }
    }
}
#endif
