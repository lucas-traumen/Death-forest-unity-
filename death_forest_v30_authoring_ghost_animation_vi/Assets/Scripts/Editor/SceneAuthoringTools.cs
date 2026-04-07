#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HollowManor.EditorTools
{
    [InitializeOnLoad]
    public static class SceneAuthoringTools
    {
        private const string DefaultScenePath = "Assets/Scenes/DeathForest_Authoring.unity";
        private static bool queuedAutoBuild;

        static SceneAuthoringTools()
        {
            QueueAutoBuildForEmptyScene();
            QueueRepairForOpenedAuthoredScene();
        }

        [MenuItem("Death Forest/Scene Authoring/Create Or Refresh Current Scene")]
        public static void CreateOrRefreshCurrentSceneMenu()
        {
            CreateOrRefreshCurrentScene(interactive: true, saveSceneAsset: false);
        }

        [MenuItem("Death Forest/Scene Authoring/Create Or Refresh And Save Default Scene")]
        public static void CreateOrRefreshAndSaveDefaultSceneMenu()
        {
            CreateOrRefreshCurrentScene(interactive: true, saveSceneAsset: true);
        }

        [MenuItem("Death Forest/Scene Authoring/Repair Missing Materials In Current Scene")]
        public static void RepairMissingMaterialsMenu()
        {
            int repaired = RepairMissingMaterialsInCurrentScene();
            EditorUtility.DisplayDialog("Death Forest", "Da gan lai material cho " + repaired + " renderer thieu material.", "OK");
        }

        [MenuItem("Death Forest/Scene Authoring/Validate Current Scene")]
        public static void ValidateCurrentSceneMenu()
        {
            string report = ValidateCurrentScene();
            Debug.Log(report);
            EditorUtility.DisplayDialog("Death Forest", report, "OK");
        }


        private static void QueueRepairForOpenedAuthoredScene()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                {
                    return;
                }

                DeathForestSceneRoot root = Object.FindAnyObjectByType<DeathForestSceneRoot>();
                if (root == null)
                {
                    return;
                }

                int repaired = RepairMissingMaterialsInCurrentScene();
                if (repaired > 0)
                {
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    Debug.Log("[Death Forest] Auto repaired missing materials in opened authored scene: " + repaired);
                }
            };
        }

        public static void QueueAutoBuildForEmptyScene()
        {
            if (queuedAutoBuild)
            {
                return;
            }

            queuedAutoBuild = true;
            EditorApplication.delayCall += () =>
            {
                queuedAutoBuild = false;
                TryAutoBuildEmptyScene();
            };
        }

        public static void CreateOrRefreshCurrentScene(bool interactive, bool saveSceneAsset)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                QueueAutoBuildForEmptyScene();
                return;
            }

            LandmarkPrefabBootstrap.EnsureAuthoredLandmarkPrefabAvailable();
            ExternalAssetImportTools.EnsureExternalAssetBootstrap();

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                activeScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }

            RemoveLegacyRuntimeRoots();
            RemoveDefaultSceneObjects();

            GameObject root = LevelFactory.BuildAuthoredSceneRoot();
            if (root == null)
            {
                Debug.LogError("[Death Forest] Khong tao duoc authored scene root.");
                return;
            }

            SceneManager.MoveGameObjectToScene(root, activeScene);
            root.transform.SetAsFirstSibling();
            Selection.activeGameObject = root;
            int refreshedPickupPoints = SceneGameplayAuthoringTools.RefreshAllPickupSpawnPoints();
            int endingPointSets = SceneGameplayAuthoringTools.EnsureEndingAnchors();
            int repairedMaterials = RepairMissingMaterialsInCurrentScene();
            if (refreshedPickupPoints > 0)
            {
                Debug.Log("[Death Forest] Refreshed pickup spawn points: " + refreshedPickupPoints);
            }
            if (endingPointSets > 0)
            {
                Debug.Log("[Death Forest] Ensured ending point sets: " + endingPointSets);
            }
            if (repairedMaterials > 0)
            {
                Debug.Log("[Death Forest] Auto repaired missing materials: " + repairedMaterials);
            }
            EditorSceneManager.MarkSceneDirty(activeScene);

            if (saveSceneAsset)
            {
                Directory.CreateDirectory("Assets/Scenes");
                EditorSceneManager.SaveScene(activeScene, DefaultScenePath);
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(DefaultScenePath, true)
                };
                AssetDatabase.Refresh();
            }

            Debug.Log(ValidateCurrentScene());

            if (interactive)
            {
                string suffix = saveSceneAsset ? "\n\nDa luu scene mac dinh tai: " + DefaultScenePath : string.Empty;
                EditorUtility.DisplayDialog(
                    "Death Forest",
                    "Da chuyen project sang scene-authoring-first.\n\nMap, player, ghost, light va landmark da duoc dat san ngay trong Scene, khong can bam Play moi thay nua." + suffix,
                    "OK");
            }
        }

        private static string ValidateCurrentScene()
        {
            List<string> missing = new List<string>();

            DeathForestSceneRoot root = Object.FindAnyObjectByType<DeathForestSceneRoot>();
            if (root == null) missing.Add("DeathForestSceneRoot");
            if (Object.FindAnyObjectByType<GameManager>() == null) missing.Add("GameManager");
            if (Object.FindAnyObjectByType<HUDController>() == null) missing.Add("HUDController");
            if (Object.FindAnyObjectByType<PlayerMotor>() == null) missing.Add("PlayerMotor");
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null) missing.Add("EventSystem");

            if (root != null)
            {
                if (root.WorldRoot == null) missing.Add("World root");
                if (root.PropRoot == null) missing.Add("Props root");
                if (root.InteractableRoot == null) missing.Add("Interactables root");
                if (root.EnemyRoot == null) missing.Add("Ghosts root");
                if (root.LightRoot == null) missing.Add("Lighting root");
                if (root.ExternalAssetRoot == null) missing.Add("SceneExternalAssets root");
            }

            if (UnityEngine.Object.FindAnyObjectByType<ItemSpawnPoint>() == null) missing.Add("ItemSpawnPoint");
            if (UnityEngine.Object.FindAnyObjectByType<EndingSequencePoints>() == null) missing.Add("EndingSequencePoints");

            int missingMaterials = 0;
            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].GetComponentInParent<DeathForestSceneRoot>() != null && renderers[i].sharedMaterial == null)
                {
                    missingMaterials++;
                }
            }

            if (missing.Count == 0 && missingMaterials == 0)
            {
                return "[Death Forest] Scene validation OK. Map authored, gameplay bindings va UI core objects deu co mat.";
            }

            string report = missing.Count > 0 ? "[Death Forest] Scene validation con thieu: " + string.Join(", ", missing) : "[Death Forest] Scene validation: core objects OK.";
            if (missingMaterials > 0)
            {
                report += " Material bi thieu: " + missingMaterials + ".";
            }
            return report;
        }

        private static int RepairMissingMaterialsInCurrentScene()
        {
            int repaired = 0;
            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer.GetComponentInParent<DeathForestSceneRoot>() == null)
                {
                    continue;
                }

                if (renderer.sharedMaterial != null)
                {
                    continue;
                }

                Material fallback = InferFallbackMaterial(renderer.gameObject.name);
                if (fallback == null)
                {
                    fallback = GeneratedMaterialLibrary.Get("Fallback_Default", new Color(0.6f, 0.6f, 0.6f), 0.02f, 0.12f, false, true);
                }

                renderer.sharedMaterial = fallback;
                EditorUtility.SetDirty(renderer);
                repaired++;
            }

            return repaired;
        }

        private static Material InferFallbackMaterial(string objectName)
        {
            string lowered = (objectName ?? string.Empty).ToLowerInvariant();
            if (lowered.Contains("ghost") || lowered.Contains("eye"))
            {
                bool accent = lowered.Contains("eye") || lowered.Contains("mark");
                return GeneratedMaterialLibrary.Get(accent ? "GhostAccent" : "Ghost", accent ? new Color(0.78f, 0.16f, 0.22f) : new Color(0.82f, 0.86f, 0.90f), accent ? 0.02f : 0.00f, accent ? 0.20f : 0.18f, !accent, true);
            }
            if (lowered.Contains("bush") || lowered.Contains("leaf")) return GeneratedMaterialLibrary.Get(lowered.Contains("busha") ? "Bush" : "Leaves", lowered.Contains("busha") ? new Color(0.09f, 0.15f, 0.10f) : new Color(0.11f, 0.19f, 0.12f), 0.02f, lowered.Contains("busha") ? 0.10f : 0.12f, false, true);
            if (lowered.Contains("rock")) return GeneratedMaterialLibrary.Get("Rock", new Color(0.21f, 0.23f, 0.24f), 0.02f, 0.18f, false, true);
            if (lowered.Contains("trunk") || lowered.Contains("log") || lowered.Contains("bark")) return GeneratedMaterialLibrary.Get("Bark", new Color(0.20f, 0.15f, 0.10f), 0.02f, 0.08f, false, true);
            if (lowered.Contains("road")) return GeneratedMaterialLibrary.Get("Road", new Color(0.12f, 0.12f, 0.13f), 0.02f, 0.20f, false, true);
            if (lowered.Contains("path") || lowered.Contains("trail") || lowered.Contains("ground")) return GeneratedMaterialLibrary.Get(lowered.Contains("ground") ? "Ground" : "Path", lowered.Contains("ground") ? new Color(0.08f, 0.11f, 0.08f) : new Color(0.16f, 0.14f, 0.10f), 0.02f, lowered.Contains("ground") ? 0.04f : 0.08f, false, true);
            if (lowered.Contains("hut") || lowered.Contains("wall") || lowered.Contains("cabin")) return GeneratedMaterialLibrary.Get("Hut", new Color(0.24f, 0.21f, 0.17f), 0.03f, 0.10f, false, true);
            if (lowered.Contains("roof")) return GeneratedMaterialLibrary.Get("HutRoof", new Color(0.10f, 0.09f, 0.08f), 0.02f, 0.14f, false, true);
            if (lowered.Contains("trim") || lowered.Contains("leg") || lowered.Contains("shelf") || lowered.Contains("table")) return GeneratedMaterialLibrary.Get("Trim", new Color(0.31f, 0.24f, 0.16f), 0.02f, 0.12f, false, true);
            if (lowered.Contains("rust") || lowered.Contains("hood")) return GeneratedMaterialLibrary.Get("Rust", new Color(0.34f, 0.18f, 0.10f), 0.06f, 0.18f, false, true);
            if (lowered.Contains("glass") || lowered.Contains("window")) return GeneratedMaterialLibrary.Get("CarGlass", new Color(0.54f, 0.76f, 0.82f, 0.65f), 0.00f, 0.42f, true, true);
            if (lowered.Contains("car") || lowered.Contains("wheel")) return GeneratedMaterialLibrary.Get("CarPaint", new Color(0.17f, 0.20f, 0.24f), 0.18f, 0.34f, false, true);
            return null;
        }

        private static void TryAutoBuildEmptyScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                return;
            }

            if (Object.FindAnyObjectByType<DeathForestSceneRoot>() != null)
            {
                return;
            }

            if (!SceneLooksLikeFreshEmptyScene())
            {
                return;
            }

            CreateOrRefreshCurrentScene(interactive: false, saveSceneAsset: true);
        }

        private static bool SceneLooksLikeFreshEmptyScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return true;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            if (roots == null || roots.Length == 0)
            {
                return true;
            }

            int allowedCount = 0;
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null)
                {
                    continue;
                }

                string name = root.name;
                if (name == "Main Camera" || name == "Directional Light" || name == "_DeathForestBootstrap")
                {
                    allowedCount++;
                    continue;
                }

                return false;
            }

            return allowedCount == roots.Length;
        }

        private static void RemoveLegacyRuntimeRoots()
        {
            List<GameObject> toRemove = new List<GameObject>();
            DeathForestSceneRoot existingRoot = Object.FindAnyObjectByType<DeathForestSceneRoot>();
            if (existingRoot != null)
            {
                toRemove.Add(existingRoot.gameObject);
            }

            BootstrapController bootstrap = Object.FindAnyObjectByType<BootstrapController>();
            if (bootstrap != null)
            {
                toRemove.Add(bootstrap.gameObject);
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                if (toRemove[i] != null)
                {
                    Object.DestroyImmediate(toRemove[i]);
                }
            }
        }

        private static void RemoveDefaultSceneObjects()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera == null)
                {
                    continue;
                }

                if (camera.GetComponentInParent<DeathForestSceneRoot>() != null)
                {
                    continue;
                }

                if (camera.gameObject.name == "Main Camera")
                {
                    Object.DestroyImmediate(camera.gameObject);
                }
            }

            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null)
                {
                    continue;
                }

                if (light.GetComponentInParent<DeathForestSceneRoot>() != null)
                {
                    continue;
                }

                if (light.type == LightType.Directional && light.gameObject.name == "Directional Light")
                {
                    Object.DestroyImmediate(light.gameObject);
                }
            }
        }
    }
}
#endif
