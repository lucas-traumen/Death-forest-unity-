#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HollowManor.EditorTools
{
    [InitializeOnLoad]
    public static class ExternalAssetImportTools
    {
        private const string ExternalAssetsRoot = "Assets/Resources/ExternalAssets";
        private const string DemoRoot = ExternalAssetsRoot + "/DemoPack";
        private const string DemoMaterialRoot = DemoRoot + "/Materials";
        private static bool queuedEnsure;

        static ExternalAssetImportTools()
        {
            QueueEnsureDemoPack();
        }

        [MenuItem("Death Forest/External Assets/Import Folder Into Catalog")]
        public static void ImportFolderIntoCatalog()
        {
            string sourceFolder = EditorUtility.OpenFolderPanel("Chon folder asset ngoai", "", "");
            if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                return;
            }

            Directory.CreateDirectory(ExternalAssetsRoot);
            string folderName = SanitizeFolderName(new DirectoryInfo(sourceFolder).Name);
            string destinationRoot = Path.Combine(ExternalAssetsRoot, "Imported", folderName).Replace('\\', '/');
            int copiedFiles = CopyDirectoryRecursive(sourceFolder, destinationRoot);

            AssetDatabase.Refresh();
            int prefabCount = ExternalAssetCatalogBuilder.RebuildCatalog(false);
            EditorUtility.DisplayDialog(
                "Death Forest",
                "Da copy " + copiedFiles + " file vao:\n" + destinationRoot + "\n\nCatalog hien co " + prefabCount + " prefab de dat truc tiep trong Scene palette hoac keo-tha vao Hierarchy.",
                "OK");
        }

        [MenuItem("Death Forest/External Assets/Create Demo Pack")]
        public static void CreateDemoPackMenu()
        {
            CreateDemoPack(true, true);
        }

        [MenuItem("Death Forest/External Assets/Rebuild Demo Pack")]
        public static void RebuildDemoPackMenu()
        {
            CreateDemoPack(true, true);
        }

        private static void QueueEnsureDemoPack()
        {
            if (queuedEnsure)
            {
                return;
            }

            queuedEnsure = true;
            EditorApplication.delayCall += () =>
            {
                queuedEnsure = false;
                EnsureDemoPackIfEmpty();
            };
        }

        public static void EnsureExternalAssetBootstrap()
        {
            EnsureDemoPackIfEmpty();
            ExternalAssetCatalogBuilder.RebuildCatalog(false);
        }

        private static void EnsureDemoPackIfEmpty()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                QueueEnsureDemoPack();
                return;
            }

            string[] existingPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { ExternalAssetsRoot });
            if (existingPrefabs != null && existingPrefabs.Length > 0)
            {
                return;
            }

            CreateDemoPack(false, false);
        }

        private static void CreateDemoPack(bool overwrite, bool interactive)
        {
            Directory.CreateDirectory(ExternalAssetsRoot);
            Directory.CreateDirectory(DemoRoot);
            Directory.CreateDirectory(DemoMaterialRoot);

            Material bark = CreateOpaqueMaterial(DemoMaterialRoot + "/M_Bark.mat", new Color(0.23f, 0.15f, 0.10f), overwrite);
            Material leaves = CreateOpaqueMaterial(DemoMaterialRoot + "/M_Leaves.mat", new Color(0.12f, 0.20f, 0.12f), overwrite);
            Material stone = CreateOpaqueMaterial(DemoMaterialRoot + "/M_Stone.mat", new Color(0.45f, 0.46f, 0.48f), overwrite);
            Material wood = CreateOpaqueMaterial(DemoMaterialRoot + "/M_Wood.mat", new Color(0.34f, 0.25f, 0.16f), overwrite);
            Material roof = CreateOpaqueMaterial(DemoMaterialRoot + "/M_Roof.mat", new Color(0.10f, 0.09f, 0.08f), overwrite);
            Material metal = CreateOpaqueMaterial(DemoMaterialRoot + "/M_Metal.mat", new Color(0.22f, 0.25f, 0.28f), overwrite);
            Material glass = CreateTransparentMaterial(DemoMaterialRoot + "/M_Glass.mat", new Color(0.55f, 0.75f, 0.82f, 0.45f), overwrite);
            Material ghost = CreateTransparentMaterial(DemoMaterialRoot + "/M_Ghost.mat", new Color(0.82f, 0.87f, 0.95f, 0.72f), overwrite);
            Material accent = CreateOpaqueMaterial(DemoMaterialRoot + "/M_Accent.mat", new Color(0.58f, 0.15f, 0.10f), overwrite);

            int created = 0;
            created += CreatePrefabIfNeeded(DemoRoot + "/DF_External_PineTree.prefab", overwrite, () => BuildTreePrefab(bark, leaves));
            created += CreatePrefabIfNeeded(DemoRoot + "/DF_External_StoneLantern.prefab", overwrite, () => BuildLanternPrefab(stone, accent));
            created += CreatePrefabIfNeeded(DemoRoot + "/DF_External_Shack.prefab", overwrite, () => BuildShackPrefab(wood, roof, glass));
            created += CreatePrefabIfNeeded(DemoRoot + "/DF_External_WreckedCar.prefab", overwrite, () => BuildCarPrefab(metal, glass, accent));
            created += CreatePrefabIfNeeded(DemoRoot + "/DF_External_Ghost.prefab", overwrite, () => BuildGhostPrefab(ghost, accent));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            int prefabCount = ExternalAssetCatalogBuilder.RebuildCatalog(false);

            if (interactive)
            {
                EditorUtility.DisplayDialog(
                    "Death Forest",
                    "Da tao/goi lai demo external asset pack.\n\nPrefab moi tao: " + created + "\nTong prefab trong catalog: " + prefabCount + "\n\nBan co the mo Death Forest/External Assets/Open Scene Palette de dat asset ngay trong Scene, khong can Play.",
                    "OK");
            }
            else
            {
                Debug.Log("[Death Forest] Da tao demo external asset pack voi " + prefabCount + " prefab.");
            }
        }

        private static int CopyDirectoryRecursive(string sourceRoot, string destinationRoot)
        {
            int copied = 0;
            foreach (string directory in Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories))
            {
                string relative = directory.Substring(sourceRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string targetDir = Path.Combine(destinationRoot, relative).Replace('\\', '/');
                Directory.CreateDirectory(targetDir);
            }

            foreach (string file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(file).ToLowerInvariant();
                if (extension == ".meta" || extension == ".tmp" || extension == ".ds_store")
                {
                    continue;
                }

                string relative = file.Substring(sourceRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string targetPath = Path.Combine(destinationRoot, relative).Replace('\\', '/');
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? destinationRoot);
                FileUtil.CopyFileOrDirectory(file, targetPath);
                copied++;
            }

            return copied;
        }

        private static string SanitizeFolderName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "ImportedPack";
            }

            char[] chars = value.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsLetterOrDigit(chars[i]) || chars[i] == '_' || chars[i] == '-')
                {
                    continue;
                }

                chars[i] = '_';
            }

            return new string(chars);
        }

        private static int CreatePrefabIfNeeded(string assetPath, bool overwrite, System.Func<GameObject> factory)
        {
            if (!overwrite && AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null)
            {
                return 0;
            }

            GameObject root = factory();
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(assetPath) ?? DemoRoot);
                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                return 1;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject BuildTreePrefab(Material bark, Material leaves)
        {
            GameObject root = new GameObject("DF_External_PineTree");
            ExternalAssetPrefabAdapter adapter = root.AddComponent<ExternalAssetPrefabAdapter>();
            adapter.role = ExternalAssetPrefabAdapter.AssetRole.Tree;
            adapter.keepExistingColliders = false;

            GameObject trunk = CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "Trunk", bark, new Vector3(0f, 2.0f, 0f), new Vector3(0.50f, 2.0f, 0.50f));
            trunk.transform.localRotation = Quaternion.identity;
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "LeavesLow", leaves, new Vector3(0f, 3.6f, 0f), new Vector3(2.6f, 1.6f, 2.6f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "LeavesMid", leaves, new Vector3(0f, 4.8f, 0f), new Vector3(2.2f, 1.5f, 2.2f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "LeavesTop", leaves, new Vector3(0f, 5.8f, 0f), new Vector3(1.5f, 1.2f, 1.5f));
            return root;
        }

        private static GameObject BuildLanternPrefab(Material stone, Material accent)
        {
            GameObject root = new GameObject("DF_External_StoneLantern");
            ExternalAssetPrefabAdapter adapter = root.AddComponent<ExternalAssetPrefabAdapter>();
            adapter.role = ExternalAssetPrefabAdapter.AssetRole.GeneralObstacle;
            adapter.keepExistingColliders = false;

            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "Base", stone, new Vector3(0f, 0.18f, 0f), new Vector3(0.7f, 0.18f, 0.7f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "Pillar", stone, new Vector3(0f, 0.95f, 0f), new Vector3(0.22f, 0.78f, 0.22f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "House", stone, new Vector3(0f, 1.78f, 0f), new Vector3(0.78f, 0.58f, 0.78f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Roof", accent, new Vector3(0f, 2.18f, 0f), new Vector3(1.20f, 0.16f, 1.20f));
            return root;
        }

        private static GameObject BuildShackPrefab(Material wood, Material roof, Material glass)
        {
            GameObject root = new GameObject("DF_External_Shack");
            ExternalAssetPrefabAdapter adapter = root.AddComponent<ExternalAssetPrefabAdapter>();
            adapter.role = ExternalAssetPrefabAdapter.AssetRole.Cabin;
            adapter.keepExistingColliders = false;

            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Body", wood, new Vector3(0f, 1.35f, 0f), new Vector3(4.8f, 2.7f, 3.8f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Roof", roof, new Vector3(0f, 2.95f, 0f), new Vector3(5.3f, 0.35f, 4.3f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "DoorFrame", roof, new Vector3(-0.95f, 1.10f, -1.92f), new Vector3(1.2f, 2.1f, 0.08f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Window", glass, new Vector3(1.18f, 1.55f, -1.92f), new Vector3(1.0f, 0.8f, 0.06f));
            return root;
        }

        private static GameObject BuildCarPrefab(Material metal, Material glass, Material accent)
        {
            GameObject root = new GameObject("DF_External_WreckedCar");
            ExternalAssetPrefabAdapter adapter = root.AddComponent<ExternalAssetPrefabAdapter>();
            adapter.role = ExternalAssetPrefabAdapter.AssetRole.Car;
            adapter.keepExistingColliders = false;

            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Body", metal, new Vector3(0f, 0.78f, 0f), new Vector3(3.2f, 0.9f, 1.8f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Cabin", accent, new Vector3(-0.1f, 1.35f, 0f), new Vector3(1.75f, 0.82f, 1.65f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "Windshield", glass, new Vector3(0.22f, 1.32f, 0f), new Vector3(0.12f, 0.62f, 1.28f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "WheelFL", metal, new Vector3(-1.02f, 0.36f, 0.96f), new Vector3(0.36f, 0.18f, 0.36f), Quaternion.Euler(90f, 0f, 0f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "WheelFR", metal, new Vector3(-1.02f, 0.36f, -0.96f), new Vector3(0.36f, 0.18f, 0.36f), Quaternion.Euler(90f, 0f, 0f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "WheelRL", metal, new Vector3(1.08f, 0.36f, 0.96f), new Vector3(0.36f, 0.18f, 0.36f), Quaternion.Euler(90f, 0f, 0f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "WheelRR", metal, new Vector3(1.08f, 0.36f, -0.96f), new Vector3(0.36f, 0.18f, 0.36f), Quaternion.Euler(90f, 0f, 0f));
            return root;
        }

        private static GameObject BuildGhostPrefab(Material ghost, Material accent)
        {
            GameObject root = new GameObject("DF_External_Ghost");
            ExternalAssetPrefabAdapter adapter = root.AddComponent<ExternalAssetPrefabAdapter>();
            adapter.role = ExternalAssetPrefabAdapter.AssetRole.Ghost;
            adapter.keepExistingColliders = false;
            adapter.normalizeGhostHeight = true;
            adapter.targetGhostHeight = 2.7f;

            CreatePrimitivePart(root.transform, PrimitiveType.Capsule, "Body", ghost, new Vector3(0f, 1.25f, 0f), new Vector3(0.85f, 1.25f, 0.85f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "Head", ghost, new Vector3(0f, 2.20f, 0f), new Vector3(0.78f, 0.78f, 0.78f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cube, "TorsoMark", accent, new Vector3(0f, 1.48f, 0.34f), new Vector3(0.32f, 0.60f, 0.06f));
            return root;
        }

        private static GameObject CreatePrimitivePart(Transform parent, PrimitiveType primitiveType, string objectName, Material material, Vector3 localPosition, Vector3 localScale)
        {
            return CreatePrimitivePart(parent, primitiveType, objectName, material, localPosition, localScale, Quaternion.identity);
        }

        private static GameObject CreatePrimitivePart(Transform parent, PrimitiveType primitiveType, string objectName, Material material, Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = objectName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            Collider col = part.GetComponent<Collider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            return part;
        }

        private static Material CreateOpaqueMaterial(string assetPath, Color color, bool overwrite)
        {
            return CreateMaterial(assetPath, color, overwrite, transparent: false);
        }

        private static Material CreateTransparentMaterial(string assetPath, Color color, bool overwrite)
        {
            return CreateMaterial(assetPath, color, overwrite, transparent: true);
        }

        private static Material CreateMaterial(string assetPath, Color color, bool overwrite, bool transparent)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing != null && !overwrite)
            {
                float existingMetallic = existing.HasProperty("_Metallic") ? existing.GetFloat("_Metallic") : (transparent ? 0.0f : 0.05f);
                float existingSmoothness = existing.HasProperty("_Smoothness")
                    ? existing.GetFloat("_Smoothness")
                    : (existing.HasProperty("_Glossiness") ? existing.GetFloat("_Glossiness") : (transparent ? 0.35f : 0.18f));
                UnityCompatibility.ConfigureMaterial(existing, color, existingMetallic, existingSmoothness, transparent);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            Material mat = existing != null
                ? existing
                : UnityCompatibility.CreateLitMaterial(color, transparent ? 0.0f : 0.05f, transparent ? 0.35f : 0.18f, transparent);
            UnityCompatibility.ConfigureMaterial(mat, color, transparent ? 0.0f : 0.05f, transparent ? 0.35f : 0.18f, transparent);

            if (existing == null)
            {
                AssetDatabase.CreateAsset(mat, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(mat);
            }

            return mat;
        }
    }
}
#endif
