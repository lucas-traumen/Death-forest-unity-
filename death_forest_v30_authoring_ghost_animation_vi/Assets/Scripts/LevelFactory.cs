using System.Collections.Generic;
using UnityEngine;

namespace HollowManor
{
    public static class LevelFactory
    {
        private sealed class MaterialSet
        {
            public readonly Material Ground;
            public readonly Material Path;
            public readonly Material Road;
            public readonly Material Bark;
            public readonly Material Leaves;
            public readonly Material Bush;
            public readonly Material Rock;
            public readonly Material Hut;
            public readonly Material HutRoof;
            public readonly Material Trim;
            public readonly Material Rust;
            public readonly Material CarPaint;
            public readonly Material CarGlass;
            public readonly Material Ghost;
            public readonly Material GhostAccent;
            public readonly Material Shrine;
            public readonly Material Note;
            public readonly Material Highlight;
            public readonly Material RedCloth;

            public MaterialSet(bool persistentAssets)
            {
                Ground = Create("Ground", new Color(0.08f, 0.11f, 0.08f), 0.02f, 0.04f, persistentAssets);
                Path = Create("Path", new Color(0.16f, 0.14f, 0.10f), 0.02f, 0.08f, persistentAssets);
                Road = Create("Road", new Color(0.12f, 0.12f, 0.13f), 0.02f, 0.20f, persistentAssets);
                Bark = Create("Bark", new Color(0.20f, 0.15f, 0.10f), 0.02f, 0.08f, persistentAssets);
                Leaves = Create("Leaves", new Color(0.11f, 0.19f, 0.12f), 0.02f, 0.12f, persistentAssets);
                Bush = Create("Bush", new Color(0.09f, 0.15f, 0.10f), 0.02f, 0.10f, persistentAssets);
                Rock = Create("Rock", new Color(0.21f, 0.23f, 0.24f), 0.02f, 0.18f, persistentAssets);
                Hut = Create("Hut", new Color(0.24f, 0.21f, 0.17f), 0.03f, 0.10f, persistentAssets);
                HutRoof = Create("HutRoof", new Color(0.10f, 0.09f, 0.08f), 0.02f, 0.14f, persistentAssets);
                Trim = Create("Trim", new Color(0.31f, 0.24f, 0.16f), 0.02f, 0.12f, persistentAssets);
                Rust = Create("Rust", new Color(0.34f, 0.18f, 0.10f), 0.06f, 0.18f, persistentAssets);
                CarPaint = Create("CarPaint", new Color(0.17f, 0.20f, 0.24f), 0.18f, 0.34f, persistentAssets);
                CarGlass = Create("CarGlass", new Color(0.54f, 0.76f, 0.82f, 0.65f), 0.00f, 0.42f, persistentAssets);
                Ghost = Create("Ghost", new Color(0.82f, 0.86f, 0.90f), 0.00f, 0.18f, persistentAssets);
                GhostAccent = Create("GhostAccent", new Color(0.78f, 0.16f, 0.22f), 0.02f, 0.20f, persistentAssets);
                Shrine = Create("Shrine", new Color(0.47f, 0.11f, 0.10f), 0.03f, 0.16f, persistentAssets);
                Note = Create("Note", new Color(0.88f, 0.82f, 0.67f), 0.00f, 0.08f, persistentAssets);
                Highlight = Create("Highlight", new Color(0.97f, 0.78f, 0.27f), 0.04f, 0.20f, persistentAssets);
                RedCloth = Create("RedCloth", new Color(0.45f, 0.05f, 0.08f), 0.02f, 0.18f, persistentAssets);
            }

            private static Material Create(string key, Color color, float metallic, float smoothness, bool persistentAssets)
            {
                return GeneratedMaterialLibrary.Get(key, color, metallic, smoothness, color.a < 0.99f, persistentAssets);
            }
        }

        private struct RectXZ
        {
            public float xMin;
            public float xMax;
            public float zMin;
            public float zMax;

            public RectXZ(float xMin, float xMax, float zMin, float zMax)
            {
                this.xMin = xMin;
                this.xMax = xMax;
                this.zMin = zMin;
                this.zMax = zMax;
            }

            public bool Contains(float x, float z, float padding = 0f)
            {
                return x >= xMin - padding && x <= xMax + padding && z >= zMin - padding && z <= zMax + padding;
            }
        }

        private static readonly RectXZ[] WalkableRects =
        {
            new RectXZ(-54f, -34f, -102f, -56f),
            new RectXZ(-46f, -34f, -60f, -10f),
            new RectXZ(-56f, -8f, -12f, 6f),
            new RectXZ(-12f, 8f, -18f, 38f),
            new RectXZ(10f, 56f, 26f, 40f),
            new RectXZ(48f, 64f, 18f, 68f),
            new RectXZ(-58f, -34f, 18f, 74f),
            new RectXZ(-56f, -18f, 62f, 82f),
            new RectXZ(18f, 52f, -8f, 12f),
            new RectXZ(24f, 58f, -30f, -6f),
            new RectXZ(44f, 84f, 36f, 64f),
            new RectXZ(-8f, 18f, 44f, 88f),
            new RectXZ(-74f, -48f, 44f, 84f),
            new RectXZ(64f, 92f, -22f, 12f)
        };

        private static readonly RectXZ[] ClearingRects =
        {
            new RectXZ(-60f, -30f, -104f, -56f),
            new RectXZ(-62f, -28f, -14f, 18f),
            new RectXZ(16f, 60f, -34f, 16f),
            new RectXZ(-62f, -16f, 54f, 88f),
            new RectXZ(40f, 86f, 30f, 68f),
            new RectXZ(-4f, 26f, 44f, 88f)
        };

        private static readonly ItemType[] PartPool =
        {
            ItemType.CarBattery,
            ItemType.FanBelt,
            ItemType.SparkPlugKit,
            ItemType.SpareWheel
        };

        private static ItemType[] randomizedParts = PartPool;
        private static int randomizedPartCursor;
        private static Vector3 lastPlayerSpawnPosition;
        private static Vector3 lastDangerPosition;
        private static bool hasLastDangerPosition;

        private static readonly string[] LegacyLandmarkIds =
        {
            "CrashSite",
            "RangerHut",
            "AbandonedCamp",
            "Shrine",
            "Creek",
            "AmbientClutter"
        };

        public const string AuthoredLandmarkSetResourcePath = "Generated/Landmarks/DeathForestLandmarkSet";
        public const string AuthoredLandmarkSetPrefabAssetPath = "Assets/Resources/Generated/Landmarks/DeathForestLandmarkSet.prefab";

        private static void ResetPickupRandomization()
        {
            randomizedParts = (ItemType[])PartPool.Clone();
            for (int i = 0; i < randomizedParts.Length; i++)
            {
                int swap = Random.Range(i, randomizedParts.Length);
                ItemType temp = randomizedParts[i];
                randomizedParts[i] = randomizedParts[swap];
                randomizedParts[swap] = temp;
            }

            randomizedPartCursor = 0;
        }

        private static ItemType ResolveRandomizedPart(ItemType requested)
        {
            return requested;
        }


        public static void RecordRecentDangerPosition(Vector3 position)
        {
            lastDangerPosition = position;
            hasLastDangerPosition = true;
        }

        public static void EnsureEndingSequenceChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            EnsureChildPoint(root, "InteractPoint", new Vector3(-1.55f, 1.10f, -0.20f), Quaternion.identity);
            EnsureChildPoint(root, "DoorApproachPoint", new Vector3(-0.45f, -0.35f, 0f), Quaternion.identity);
            EnsureChildPoint(root, "SeatPoint", new Vector3(0.72f, 0.18f, -0.18f), Quaternion.identity);
            EnsureChildPoint(root, "SeatFacingPoint", new Vector3(0.72f, 0.18f, -1.18f), Quaternion.identity);
            EnsureChildPoint(root, "CarAlignPoint", new Vector3(0f, 0f, 0f), Quaternion.identity);
            EnsureChildPoint(root, "ExitPathStart", new Vector3(0f, 0f, -4.5f), Quaternion.identity);
            EnsureChildPoint(root, "ExitPathEnd", new Vector3(0f, 0f, -18f), Quaternion.identity);
            EnsureChildPoint(root, "EndingCameraPoint", new Vector3(4.5f, 2.8f, -6.5f), Quaternion.Euler(14f, -148f, 0f));
            EnsureChildPoint(root, "GhostRevealPoint", new Vector3(-6.5f, 0f, 2.4f), Quaternion.identity);

            EndingSequencePoints points = root.GetComponent<EndingSequencePoints>();
            if (points == null)
            {
                points = root.gameObject.AddComponent<EndingSequencePoints>();
            }

            points.ResolveMissingPoints();
        }

        private static string GetPartDisplayName(ItemType type)
        {
            switch (type)
            {
                case ItemType.CarBattery: return "ắc quy";
                case ItemType.FanBelt: return "dây curoa";
                case ItemType.SparkPlugKit: return "bộ bugi";
                case ItemType.SpareWheel: return "bánh dự phòng";
                case ItemType.NotePage: return "trang nhật ký";
                default: return "linh kien";
            }
        }

        public static string GetDefaultItemDisplayName(ItemType type)
        {
            return type == ItemType.NotePage ? "trang nhật ký" : GetPartDisplayName(type);
        }

        public static void RebuildPickupFromSpawnPoint(ItemSpawnPoint spawnPoint, bool persistentAssets)
        {
            if (spawnPoint == null)
            {
                return;
            }

            Transform parent = spawnPoint.transform;
            Transform existing = parent.Find("SpawnedPickup");
            if (existing != null)
            {
                UnityCompatibility.DestroyObject(existing.gameObject);
            }

            MaterialSet materials = new MaterialSet(persistentAssets);
            Vector3 requested = parent.position;
            Vector3 finalPosition = spawnPoint.SnapToGroundWhenRebuilding ? ResolvePickupPlacement(requested) : requested;
            parent.position = finalPosition;
            CreatePickup(parent, materials, spawnPoint.ItemType, spawnPoint.ResolvedDisplayName, Vector3.zero, spawnPoint.VisualType, spawnPoint.VisualPrefabOverride, spawnPoint.ExternalVisualAssetId, spawnPoint.BobAndSpin);
        }

        public static GameObject Build(Transform parent)
        {
            return BuildInternal(parent, runtimeGenerated: true);
        }

        public static GameObject BuildAuthoredSceneRoot()
        {
            return BuildInternal(null, runtimeGenerated: false);
        }

        private static GameObject BuildInternal(Transform parent, bool runtimeGenerated)
        {
            Random.InitState(runtimeGenerated ? System.Environment.TickCount : 16031994);
            if (!runtimeGenerated)
            {
                lastPlayerSpawnPosition = Vector3.zero;
                lastDangerPosition = Vector3.zero;
                hasLastDangerPosition = false;
            }

            MaterialSet materials = new MaterialSet(!runtimeGenerated);
            ResetPickupRandomization();

            GameObject root = new GameObject(runtimeGenerated ? "_GeneratedDeathForest" : "DeathForestSceneRoot");
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            GameManager gameManager = root.AddComponent<GameManager>();
            root.AddComponent<AudioFeedbackController>();
            root.AddComponent<JumpScareDirector>();
            HUDController hud = HUDController.Create(root.transform);
            gameManager.BindHud(hud);

            PlayerMotor player = CreatePlayer(root.transform);
            gameManager.BindPlayer(player);

            Transform worldRoot = CreateRoot(root.transform, "World");
            Transform propRoot = CreateRoot(root.transform, "Props");
            Transform interactableRoot = CreateRoot(root.transform, "Interactables");
            Transform enemyRoot = CreateRoot(root.transform, "Ghosts");
            Transform lightRoot = CreateRoot(root.transform, "Lighting");
            Transform externalAssetRoot = CreateRoot(root.transform, "SceneExternalAssets");

            if (runtimeGenerated)
            {
                root.AddComponent<RuntimeObjectEditor>();
            }
            else
            {
                DeathForestSceneRoot sceneRoot = root.AddComponent<DeathForestSceneRoot>();
                sceneRoot.BindRoots(worldRoot, propRoot, interactableRoot, enemyRoot, lightRoot, externalAssetRoot);
            }

            BuildGround(worldRoot, materials);
            BuildTerrainRelief(worldRoot, materials);
            BuildForestScatter(propRoot, materials);
            BuildLandmarks(propRoot, interactableRoot, materials);
            BuildTraversalObstacles(propRoot, materials);
            TryInstantiateBakedExternalEnvironment(runtimeGenerated ? propRoot : externalAssetRoot);
            BuildGhosts(enemyRoot, materials);
            BuildLighting(lightRoot, materials);

            if (runtimeGenerated)
            {
                gameManager.ShowToast("Nhấn ENTER hoặc SPACE để bắt đầu chuyến đi trong Death Forest.", 999f);
            }

            return root;
        }

        private static PlayerMotor CreatePlayer(Transform parent)
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetParent(parent, false);
            playerObject.transform.localScale = Vector3.one;
            playerObject.transform.position = GetRandomPlayerSpawn();
            lastPlayerSpawnPosition = playerObject.transform.position;

            playerObject.AddComponent<CharacterController>();
            PlayerMotor motor = playerObject.AddComponent<PlayerMotor>();
            motor.walkSpeed = 4.1f;
            motor.sprintSpeed = 6.8f;
            motor.crouchSpeed = 2.1f;
            motor.walkNoiseRadius = 4.8f;
            motor.sprintNoiseRadius = 11.8f;
            playerObject.AddComponent<PlayerInteractor>();

            if (motor.ViewCamera != null)
            {
                motor.ViewCamera.clearFlags = CameraClearFlags.SolidColor;
                motor.ViewCamera.backgroundColor = new Color(0.02f, 0.04f, 0.05f);
                motor.ViewCamera.farClipPlane = 140f;
            }

            return motor;
        }

        private static Vector3 GetRandomPlayerSpawn()
        {
            Vector3[] candidates =
            {
                new Vector3(-44.0f, 0.05f, -93.5f),
                new Vector3(-24.0f, 0.05f, -58.0f),
                new Vector3(-8.0f, 0.05f, -6.0f),
                new Vector3(6.0f, 0.05f, 30.0f),
                new Vector3(40.0f, 0.05f, -8.0f),
                new Vector3(62.0f, 0.05f, 34.0f),
                new Vector3(-60.0f, 0.05f, 54.0f),
                new Vector3(76.0f, 0.05f, 6.0f),
                new Vector3(-70.0f, 0.05f, 70.0f),
                new Vector3(18.0f, 0.05f, 74.0f)
            };

            float bestScore = float.MinValue;
            System.Collections.Generic.List<Vector3> pool = new System.Collections.Generic.List<Vector3>();
            foreach (Vector3 candidate in candidates)
            {
                float score = 0f;
                if (hasLastDangerPosition)
                {
                    score += Vector3.Distance(FlattenXZ(candidate), FlattenXZ(lastDangerPosition)) * 1.35f;
                }
                if (lastPlayerSpawnPosition != Vector3.zero)
                {
                    score += Vector3.Distance(FlattenXZ(candidate), FlattenXZ(lastPlayerSpawnPosition)) * 0.55f;
                }

                if (score > bestScore + 0.1f)
                {
                    pool.Clear();
                    pool.Add(candidate);
                    bestScore = score;
                }
                else if (score >= bestScore - 9f)
                {
                    pool.Add(candidate);
                }
            }

            Vector3 chosen = pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : candidates[Random.Range(0, candidates.Length)];
            return chosen;
        }

        private static void BuildGround(Transform parent, MaterialSet materials)
        {
            CreateBlock(parent, "Ground", new Vector3(0f, -0.5f, -8f), new Vector3(236f, 1f, 248f), materials.Ground, false);
            CreateBlock(parent, "RoadSouth", new Vector3(-44f, 0.02f, -82f), new Vector3(12f, 0.04f, 44f), materials.Road, true);
            CreateBlock(parent, "RoadHook", new Vector3(-40f, 0.02f, -50f), new Vector3(36f, 0.04f, 10f), materials.Path, true);
            CreateBlock(parent, "MainTrail", new Vector3(-10f, 0.02f, -8f), new Vector3(18f, 0.04f, 94f), materials.Path, true);
            CreateBlock(parent, "WestBranch", new Vector3(-46f, 0.02f, 4f), new Vector3(18f, 0.04f, 18f), materials.Path, true);
            CreateBlock(parent, "WestVertical", new Vector3(-52f, 0.02f, 44f), new Vector3(12f, 0.04f, 56f), materials.Path, true);
            CreateBlock(parent, "ShrineTrail", new Vector3(28f, 0.02f, 32f), new Vector3(64f, 0.04f, 14f), materials.Path, true);
            CreateBlock(parent, "EastBranch", new Vector3(46f, 0.02f, -14f), new Vector3(40f, 0.04f, 14f), materials.Path, true);
            CreateBlock(parent, "EastVertical", new Vector3(72f, 0.02f, -2f), new Vector3(14f, 0.04f, 40f), materials.Path, true);
            CreateBlock(parent, "CampClearing", new Vector3(42f, 0.02f, -18f), new Vector3(34f, 0.04f, 26f), materials.Path, true);
            CreateBlock(parent, "CreekClearing", new Vector3(64f, 0.02f, 48f), new Vector3(30f, 0.04f, 22f), materials.Path, true);
            CreateBlock(parent, "HutClearing", new Vector3(-50f, 0.02f, 4f), new Vector3(24f, 0.04f, 26f), materials.Path, true);
            CreateBlock(parent, "CrashClearing", new Vector3(-44f, 0.02f, -88f), new Vector3(28f, 0.04f, 24f), materials.Path, true);
            CreateBlock(parent, "NorthDeadEnd", new Vector3(8f, 0.02f, 70f), new Vector3(16f, 0.04f, 20f), materials.Path, true);
            CreateBlock(parent, "FarWestClearing", new Vector3(-70f, 0.02f, 62f), new Vector3(22f, 0.04f, 22f), materials.Path, true);
            CreateBlock(parent, "FarEastClearing", new Vector3(86f, 0.02f, -8f), new Vector3(18f, 0.04f, 20f), materials.Path, true);
            BuildPerimeter(parent);
        }

        private static void BuildTerrainRelief(Transform parent, MaterialSet materials)
        {
            CreateHill(parent, materials, new Vector3(-72f, -1.5f, 62f), new Vector3(34f, 7.0f, 28f), new Color(0.10f, 0.13f, 0.10f));
            CreateHill(parent, materials, new Vector3(70f, -1.4f, 52f), new Vector3(30f, 6.2f, 26f), new Color(0.10f, 0.12f, 0.11f));
            CreateHill(parent, materials, new Vector3(44f, -1.2f, -40f), new Vector3(34f, 5.4f, 26f), new Color(0.11f, 0.12f, 0.10f));
            CreateHill(parent, materials, new Vector3(-8f, -1.35f, 82f), new Vector3(42f, 6.8f, 22f), new Color(0.09f, 0.11f, 0.10f));
            CreateHill(parent, materials, new Vector3(-24f, -1.25f, -26f), new Vector3(26f, 5.0f, 24f), new Color(0.10f, 0.12f, 0.10f));
            CreateHill(parent, materials, new Vector3(90f, -1.3f, -6f), new Vector3(20f, 4.6f, 18f), new Color(0.10f, 0.12f, 0.10f));
            CreateRaisedPathMound(parent, materials, new Vector3(-8f, 0.16f, 22f), new Vector3(7.4f, 0.5f, 30f), 14f);
            CreateRaisedPathMound(parent, materials, new Vector3(34f, 0.12f, 34f), new Vector3(7.2f, 0.42f, 20f), -18f);
            CreateRaisedPathMound(parent, materials, new Vector3(-46f, 0.10f, 56f), new Vector3(8.8f, 0.35f, 18f), 10f);
            CreateRaisedPathMound(parent, materials, new Vector3(58f, 0.14f, -14f), new Vector3(6.4f, 0.38f, 18f), -26f);
        }

        private static void BuildTraversalObstacles(Transform parent, MaterialSet materials)
        {
            CreateFallenTreeBarrier(parent, materials, new Vector3(-18f, 0f, -18f), 22f, 10.6f);
            CreateFallenTreeBarrier(parent, materials, new Vector3(20.5f, 0f, 22.0f), -34f, 11.4f);
            CreateFallenTreeBarrier(parent, materials, new Vector3(52.0f, 0f, 40.8f), 62f, 9.1f);
            CreateFallenTreeBarrier(parent, materials, new Vector3(-56.0f, 0f, 34.8f), -12f, 9.6f);
            CreateRockBarrier(parent, materials, new Vector3(-34.6f, 0f, 28.4f), 6, 2.6f);
            CreateRockBarrier(parent, materials, new Vector3(44.5f, 0f, 6.6f), 5, 2.4f);
            CreateRockBarrier(parent, materials, new Vector3(14.2f, 0f, 57.4f), 7, 2.4f);
            CreateRockBarrier(parent, materials, new Vector3(80.2f, 0f, -8.4f), 5, 2.5f);
        }

        private static void CreateHill(Transform parent, MaterialSet materials, Vector3 position, Vector3 scale, Color tint)
        {
            GameObject hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hill.name = "Hill";
            hill.transform.SetParent(parent, false);
            hill.transform.position = position;
            hill.transform.localScale = scale;
            SphereCollider collider = hill.GetComponent<SphereCollider>();
            if (collider != null)
            {
                collider.center = new Vector3(0f, 0.08f, 0f);
                collider.radius = 0.50f;
            }
            Renderer renderer = hill.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = UnityCompatibility.CloneMaterial(materials.Ground, tint);
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        private static void CreateRaisedPathMound(Transform parent, MaterialSet materials, Vector3 position, Vector3 scale, float rotationY)
        {
            GameObject mound = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mound.name = "RaisedPathMound";
            mound.transform.SetParent(parent, false);
            mound.transform.position = position;
            mound.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            mound.transform.localScale = scale;
            BoxCollider collider = mound.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.center = Vector3.zero;
                collider.size = Vector3.one;
            }
            Renderer renderer = mound.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = materials.Path;
            }
        }

        private static void CreateFallenTreeBarrier(Transform parent, MaterialSet materials, Vector3 position, float rotationY, float length)
        {
            GameObject barrier = new GameObject("FallenTreeBarrier");
            barrier.transform.SetParent(parent, false);
            barrier.transform.position = position;
            barrier.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            CreateLocalBlock(barrier.transform, "Log", new Vector3(0f, 0.55f, 0f), new Vector3(length, 1.0f, 0.85f), materials.Bark, false, Quaternion.Euler(0f, 0f, 9f));
            CreateLocalBlock(barrier.transform, "BranchA", new Vector3(-length * 0.18f, 1.0f, 0.45f), new Vector3(1.4f, 0.22f, 0.18f), materials.Bark, true, Quaternion.Euler(26f, -18f, 32f));
            CreateLocalBlock(barrier.transform, "BranchB", new Vector3(length * 0.12f, 0.95f, -0.38f), new Vector3(1.1f, 0.20f, 0.18f), materials.Bark, true, Quaternion.Euler(-22f, 16f, -28f));
        }

        private static void CreateRockBarrier(Transform parent, MaterialSet materials, Vector3 center, int count, float spacing)
        {
            for (int i = 0; i < count; i++)
            {
                float t = count <= 1 ? 0f : i / (float)(count - 1);
                Vector3 offset = new Vector3((t - 0.5f) * spacing * count * 0.6f, 0f, Mathf.Sin(t * Mathf.PI) * 1.1f);
                CreateRockGroup(parent, materials, center + offset, Random.Range(0.9f, 1.4f));
            }
        }

        private static void BuildPerimeter(Transform parent)
        {
            CreateInvisibleWall(parent, new Vector3(0f, 2f, -124f), new Vector3(236f, 4f, 1f));
            CreateInvisibleWall(parent, new Vector3(0f, 2f, 108f), new Vector3(236f, 4f, 1f));
            CreateInvisibleWall(parent, new Vector3(-111f, 2f, -8f), new Vector3(1f, 4f, 236f));
            CreateInvisibleWall(parent, new Vector3(111f, 2f, -8f), new Vector3(1f, 4f, 236f));
        }

        private static void BuildForestScatter(Transform parent, MaterialSet materials)
        {
            for (float x = -104f; x <= 104f; x += 4.5f)
            {
                for (float z = -116f; z <= 100f; z += 4.5f)
                {
                    float jitterX = Random.Range(-1.2f, 1.2f);
                    float jitterZ = Random.Range(-1.2f, 1.2f);
                    float px = x + jitterX;
                    float pz = z + jitterZ;

                    if (IsInsideAny(px, pz, 2.8f, WalkableRects) || IsInsideAny(px, pz, 1.6f, ClearingRects))
                    {
                        continue;
                    }

                    float roll = Random.value;
                    Vector3 pos = new Vector3(px, 0f, pz);
                    if (roll < 0.68f)
                    {
                        CreateTree(parent, materials, pos, Random.Range(0.85f, 1.30f));
                    }
                    else if (roll < 0.87f)
                    {
                        CreateBushCluster(parent, materials, pos, Random.Range(0.8f, 1.25f));
                    }
                    else
                    {
                        CreateRockGroup(parent, materials, pos, Random.Range(0.8f, 1.35f));
                    }
                }
            }
        }


        private static void BuildLandmarks(Transform props, Transform interactables, MaterialSet materials)
        {
            if (TryInstantiateAuthoredLandmarkSet(props))
            {
                return;
            }

            Debug.LogError("[Death Forest] Khong tim thay landmark prefab authored tai Resources/" + AuthoredLandmarkSetResourcePath + ". Runtime procedural fallback da bi go bo. Hay rebuild prefab authored bang menu Death Forest/Landmarks/Rebuild Default Authored Landmark Prefab hoac Death Forest/Landmarks/Save Selected Root As Authored Landmark Set.");
        }

        private static void BuildLegacyLandmarks(Transform props, Transform interactables, MaterialSet materials)
        {
            BuildCrashSite(props, interactables, materials);
            BuildRangerHut(props, interactables, materials);
            BuildAbandonedCamp(props, interactables, materials);
            BuildShrine(props, interactables, materials);
            BuildCreek(props, interactables, materials);
            BuildAmbientClutter(props, materials);
        }

        private static bool TryInstantiateAuthoredLandmarkSet(Transform parent)
        {
            GameObject prefab = Resources.Load<GameObject>(AuthoredLandmarkSetResourcePath);
            if (prefab == null)
            {
                return false;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = "AuthoredLandmarkSet";
            return true;
        }

        public static IReadOnlyList<string> GetLegacyLandmarkIdsForAuthoring()
        {
            return LegacyLandmarkIds;
        }

        public static GameObject CreateLegacyLandmarkSetAuthoringRoot()
        {
            MaterialSet materials = new MaterialSet(false);
            GameObject root = new GameObject("DeathForestLandmarkSet");
            Transform props = CreateRoot(root.transform, "Props");
            Transform interactables = CreateRoot(root.transform, "Interactables");
            BuildLegacyLandmarks(props, interactables, materials);
            return root;
        }

        public static GameObject CreateLegacyLandmarkAuthoringRoot(string landmarkId)
        {
            MaterialSet materials = new MaterialSet(false);
            GameObject root = new GameObject(string.IsNullOrWhiteSpace(landmarkId) ? "Landmark" : landmarkId.Trim());
            Transform props = CreateRoot(root.transform, "Props");
            Transform interactables = CreateRoot(root.transform, "Interactables");
            if (!BuildSingleLegacyLandmark(landmarkId, props, interactables, materials))
            {
                Object.DestroyImmediate(root);
                return null;
            }

            return root;
        }

        private static bool BuildSingleLegacyLandmark(string landmarkId, Transform props, Transform interactables, MaterialSet materials)
        {
            switch ((landmarkId ?? string.Empty).Trim())
            {
                case "CrashSite":
                    BuildCrashSite(props, interactables, materials);
                    return true;
                case "RangerHut":
                    BuildRangerHut(props, interactables, materials);
                    return true;
                case "AbandonedCamp":
                    BuildAbandonedCamp(props, interactables, materials);
                    return true;
                case "Shrine":
                    BuildShrine(props, interactables, materials);
                    return true;
                case "Creek":
                    BuildCreek(props, interactables, materials);
                    return true;
                case "AmbientClutter":
                    BuildAmbientClutter(props, materials);
                    return true;
                default:
                    return false;
            }
        }

        private static void BuildCrashSite(Transform props, Transform interactables, MaterialSet materials)
        {
            CreateBrokenCar(props, interactables, materials, new Vector3(-44f, 0f, -83f));
            CreateRoadBarrier(props, materials, new Vector3(-47.8f, 0f, -79.8f), 12f);
            CreateRoadBarrier(props, materials, new Vector3(-40.2f, 0f, -79.8f), -12f);
            CreateBushHideSpot(props, materials, new Vector3(-53.6f, 0f, -78.2f), 25f);
            CreateLogHideSpot(props, materials, new Vector3(-30.4f, 0f, -69.5f), -35f);
            CreatePickupSpawnPoint(props, materials, ItemType.NotePage, "trang nhat ky nha nat", new Vector3(-48.2f, 0.85f, -83.4f), PickupVisualType.Note);
        }

        private static void BuildRangerHut(Transform props, Transform interactables, MaterialSet materials)
        {
            CreateCabin(props, materials, new Vector3(-52f, 0f, 6f), new Vector3(7f, 3.8f, 6f));
            CreateTable(props, materials, new Vector3(-53.8f, 0f, 5.8f), 0f, 1.8f, 0.9f);
            CreateShelf(props, materials, new Vector3(-49.2f, 0f, 4.6f), 90f, 3);
            CreateCrate(props, materials, new Vector3(-54.4f, 0.45f, 8.1f), new Vector3(1.0f, 0.9f, 0.9f));
            CreatePickupSpawnPoint(props, materials, ItemType.SparkPlugKit, "bộ bugi", new Vector3(-53.4f, 0.96f, 1.8f), PickupVisualType.Part);
            CreatePickupSpawnPoint(props, materials, ItemType.NotePage, "bien ban tuan rung", new Vector3(-49.2f, 1.25f, 4.6f), PickupVisualType.Note);
            CreateBushHideSpot(props, materials, new Vector3(-61.2f, 0f, 1.5f), 90f);
            CreateBushHideSpot(props, materials, new Vector3(-60.4f, 0f, 13.7f), 35f);
            CreateLanternLight(interactables, materials, new Vector3(-52.1f, 2.7f, 4.0f), 0.7f);
            CreateClosetHideSpot(props, materials, new Vector3(-50.8f, 0f, 8.1f), 180f);
        }

        private static void BuildAbandonedCamp(Transform props, Transform interactables, MaterialSet materials)
        {
            CreateTent(props, materials, new Vector3(46.5f, 0f, -18.2f), -30f, true);
            CreateTent(props, materials, new Vector3(34.0f, 0f, -10.8f), 25f, true);
            CreateCampfire(props, materials, new Vector3(40.2f, 0f, -14.8f));
            ExternalItemVisualLibrary.TryCreateJerryCanProp(props, new Vector3(38.6f, 0f, -18.6f), Quaternion.Euler(0f, -24f, 0f), Vector3.one, materials.RedCloth);
            CreateTable(props, materials, new Vector3(49.5f, 0f, -9.2f), 180f, 1.8f, 0.8f);
            CreateCrate(props, materials, new Vector3(54.4f, 0.52f, -18.1f), new Vector3(0.9f, 1.0f, 0.9f));
            CreateCrate(props, materials, new Vector3(31.5f, 0.42f, -5.1f), new Vector3(0.8f, 0.8f, 0.8f));
            CreatePickupSpawnPoint(props, materials, ItemType.CarBattery, "ắc quy", new Vector3(49.5f, 1.00f, -9.2f), PickupVisualType.Part);
            CreatePickupSpawnPoint(props, materials, ItemType.NotePage, "anh chup cua nan nhan cu", new Vector3(54.4f, 1.02f, -18.1f), PickupVisualType.Note);
            CreateBushHideSpot(props, materials, new Vector3(58.6f, 0f, -4.8f), -70f);
            CreateLogHideSpot(props, materials, new Vector3(22.8f, 0f, -0.2f), 15f);
        }

        private static void BuildShrine(Transform props, Transform interactables, MaterialSet materials)
        {
            CreateTorii(props, materials, new Vector3(-18.2f, 0f, 30.4f));
            CreateShrineAltar(props, materials, new Vector3(-18.0f, 0f, 34.0f));
            CreatePickupSpawnPoint(props, materials, ItemType.FanBelt, "dây curoa", new Vector3(-18.0f, 0.96f, 28.4f), PickupVisualType.Part);
            CreateRockGroup(props, materials, new Vector3(-25.4f, 0f, 34.0f), 0.82f);
            CreateRockGroup(props, materials, new Vector3(-10.8f, 0f, 33.6f), 0.74f);
            CreateBushHideSpot(props, materials, new Vector3(-25.2f, 0f, 28.8f), 100f);
            CreateBushHideSpot(props, materials, new Vector3(-9.2f, 0f, 35.4f), -90f);
            CreateLanternLight(interactables, materials, new Vector3(-20.2f, 1.2f, 33.6f), 0.55f);
            CreateLanternLight(interactables, materials, new Vector3(-15.8f, 1.2f, 33.6f), 0.55f);
        }

        private static void BuildCreek(Transform props, Transform interactables, MaterialSet materials)
        {
            CreateStream(props, materials, new Vector3(30f, 0.02f, 26f));
            CreateShack(props, materials, new Vector3(34.6f, 0f, 24.6f));
            CreateRockGroup(props, materials, new Vector3(21.4f, 0f, 18.2f), 0.92f);
            CreateRockGroup(props, materials, new Vector3(30.6f, 0f, 34.6f), 0.76f);
            CreatePickupSpawnPoint(props, materials, ItemType.SpareWheel, "bánh dự phòng", new Vector3(37.6f, 0.92f, 22.4f), PickupVisualType.Part);
            CreatePickupSpawnPoint(props, materials, ItemType.NotePage, "manh giay uot ben bo suoi", new Vector3(26.2f, 0.95f, 18.4f), PickupVisualType.Note);
            CreateLogHideSpot(props, materials, new Vector3(19.6f, 0f, 21.8f), 85f);
            CreateBushHideSpot(props, materials, new Vector3(40.2f, 0f, 28.4f), -60f);
            CreateClosetHideSpot(props, materials, new Vector3(33.6f, 0f, 26.8f), 180f);
        }

        private static void BuildAmbientClutter(Transform props, MaterialSet materials)
        {
            CreateSignPost(props, materials, new Vector3(-10.2f, 0f, -16.0f), 180f, "Loi tat");
            CreateSignPost(props, materials, new Vector3(-8.0f, 0f, 2.0f), -70f, "Kiem lam");
            CreateSignPost(props, materials, new Vector3(5.5f, 0f, 8.6f), 75f, "Cam trai");
            CreateSignPost(props, materials, new Vector3(-3.0f, 0f, 22.0f), -65f, "Mieu");
            CreateSignPost(props, materials, new Vector3(7.0f, 0f, 20.0f), 65f, "Suoi");
        }

        private static void BuildGhosts(Transform parent, MaterialSet materials)
        {
            Vector3[] mainRoute =
            {
                new Vector3(8f, 0f, -8f),
                new Vector3(-40f, 0f, -78f),
                new Vector3(-52f, 0f, 6f),
                new Vector3(-18f, 0f, 32f),
                new Vector3(34f, 0f, 23f),
                new Vector3(46f, 0f, -16f),
                new Vector3(12f, 0f, 18f),
                new Vector3(-8f, 0f, 56f),
                new Vector3(-24f, 0f, -8f)
            };

            CreateGhost(parent, materials, "Bong ma rung", mainRoute);
        }

        private static void BuildLighting(Transform parent, MaterialSet materials)
        {
            GameObject moon = new GameObject("MoonLight");
            moon.transform.SetParent(parent, false);
            moon.transform.rotation = Quaternion.Euler(46f, -35f, 0f);
            Light light = moon.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.28f, 0.34f, 0.46f);
            light.intensity = 0.08f;
            light.shadows = LightShadows.Soft;

            RenderSettings.ambientLight = new Color(0.008f, 0.010f, 0.016f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.01f, 0.01f, 0.02f);
            RenderSettings.fogDensity = 0.018f;

            GameObject fill = new GameObject("CrashFill");
            fill.transform.SetParent(parent, false);
            fill.transform.position = new Vector3(-28f, 3.8f, -66f);
            Light fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.range = 9f;
            fillLight.intensity = 0.08f;
            fillLight.color = new Color(0.56f, 0.65f, 0.85f);
        }

        private static void CreateBrokenCar(Transform props, Transform interactables, MaterialSet materials, Vector3 position)
        {
            if (!TryCreateExternalPropByRole(props, position, Quaternion.identity, Vector3.one, "BrokenCarPrefabInstance", ExternalAssetPrefabAdapter.AssetRole.Car))
            {
                GameObject carRoot = new GameObject("BrokenCar");
                carRoot.transform.SetParent(props, false);
                carRoot.transform.position = position;

                CreateLocalBlock(carRoot.transform, "Body", new Vector3(0f, 0.95f, 0f), new Vector3(3.8f, 1.0f, 6.2f), materials.CarPaint, false);
                CreateLocalBlock(carRoot.transform, "Cabin", new Vector3(0f, 1.72f, -0.2f), new Vector3(3.2f, 0.95f, 2.8f), materials.CarPaint, false);
                CreateLocalBlock(carRoot.transform, "Windshield", new Vector3(0f, 1.78f, -1.1f), new Vector3(2.8f, 0.75f, 0.08f), materials.CarGlass, true);
                CreateLocalBlock(carRoot.transform, "RearGlass", new Vector3(0f, 1.78f, 1.05f), new Vector3(2.8f, 0.75f, 0.08f), materials.CarGlass, true);
                CreateLocalBlock(carRoot.transform, "Hood", new Vector3(0f, 1.25f, -2.1f), new Vector3(3.0f, 0.35f, 1.9f), materials.Rust, false);
                CreateLocalBlock(carRoot.transform, "OpenHood", new Vector3(0f, 2.15f, -2.55f), new Vector3(3.0f, 0.10f, 1.9f), materials.Rust, false, Quaternion.Euler(-58f, 0f, 0f));
                CreateWheel(carRoot.transform, materials, new Vector3(-1.7f, 0.55f, -1.75f), true);
                CreateWheel(carRoot.transform, materials, new Vector3(1.7f, 0.55f, -1.75f), true);
                CreateWheel(carRoot.transform, materials, new Vector3(-1.7f, 0.55f, 1.95f), false);
                CreateWheel(carRoot.transform, materials, new Vector3(1.7f, 0.55f, 1.95f), true);
                CreateLocalBlock(carRoot.transform, "MissingWheelHub", new Vector3(-1.7f, 0.55f, 1.95f), new Vector3(0.20f, 0.20f, 0.20f), materials.Highlight, true);
                CreateLocalBlock(carRoot.transform, "EngineGap", new Vector3(0f, 1.38f, -2.0f), new Vector3(2.1f, 0.42f, 1.2f), materials.Rock, true);
            }

            GameObject repair = new GameObject("CarRepairPoint");
            repair.transform.SetParent(interactables, false);
            repair.transform.position = position + new Vector3(-0.75f, 1.15f, -1.05f);
            BoxCollider repairCollider = repair.AddComponent<BoxCollider>();
            repairCollider.size = new Vector3(2.4f, 1.6f, 1.3f);
            repair.AddComponent<CarRepairInteractable>();

            GameObject endingRoot = new GameObject("EndingSequenceRoot");
            endingRoot.transform.SetParent(interactables, false);
            endingRoot.transform.position = position;
            EnsureEndingSequenceChildren(endingRoot.transform);

            Transform interactPoint = endingRoot.transform.Find("InteractPoint");
            GameObject escape = new GameObject("CarEscapePoint");
            escape.transform.SetParent(interactPoint != null ? interactPoint : endingRoot.transform, false);
            escape.transform.localPosition = Vector3.zero;
            BoxCollider escapeCollider = escape.AddComponent<BoxCollider>();
            escapeCollider.size = new Vector3(1.0f, 1.6f, 1.2f);
            escape.AddComponent<CarEscapeInteractable>();
        }

        private static void CreateGhost(Transform parent, MaterialSet materials, string enemyName, params Vector3[] routePoints)
        {
            Vector3 start = routePoints != null && routePoints.Length > 0 ? routePoints[0] : Vector3.zero;
            Vector3[] patrolPoints;
            if (routePoints != null && routePoints.Length > 1)
            {
                patrolPoints = new Vector3[routePoints.Length - 1];
                for (int i = 1; i < routePoints.Length; i++)
                {
                    patrolPoints[i - 1] = routePoints[i];
                }
            }
            else
            {
                patrolPoints = new[] { start };
            }

            GameObject enemy = new GameObject(enemyName);
            enemy.transform.SetParent(parent, false);
            enemy.transform.localScale = Vector3.one;
            enemy.transform.position = start;
            enemy.AddComponent<CharacterController>();
            GhostAuthoring authoring = enemy.AddComponent<GhostAuthoring>();
            PatrolEnemy patrol = enemy.AddComponent<PatrolEnemy>();
            patrol.enemyName = enemyName;
            patrol.patrolSpeed = 2.30f;
            patrol.investigateSpeed = 4.95f;
            patrol.chaseSpeed = 8.65f;
            patrol.viewDistance = 11.5f;
            patrol.viewAngle = 135f;
            patrol.catchDistance = 2.25f;
            patrol.chaseMemory = 15.2f;
            patrol.investigatePause = 0.32f;
            patrol.hearingChaseSignal = 0.09f;
            patrol.captureHoldTime = 0.95f;
            patrol.avoidanceProbeDistance = 4.0f;
            patrol.avoidanceTurnAngle = 58f;
            patrol.wideAvoidanceTurnAngle = 98f;

            Renderer renderer = CreateGhostVisual(enemy.transform, materials);
            patrol.AssignBodyRenderer(renderer);
            Animator animator = enemy.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                patrol.AssignAnimator(animator);
            }

            authoring.ResolveMissingReferences();
            authoring.EnsureRouteChildren(patrolPoints);
            patrol.SetWaypoints(authoring.BuildWaypointPositions(enemy.transform.position));
        }

        private static Renderer CreateGhostVisual(Transform parent, MaterialSet materials)
        {
            Renderer assignedRenderer = null;
            Transform modelRoot = new GameObject("ModelRoot").transform;
            modelRoot.SetParent(parent, false);
            modelRoot.localPosition = new Vector3(0f, 0.1f, 0f);

            assignedRenderer = TryCreateGhostMaDirect(modelRoot, materials);

            if (assignedRenderer == null && ExternalAssetCatalog.TryGetFirstByRole(ExternalAssetPrefabAdapter.AssetRole.Ghost, out ExternalAssetCatalog.Entry ghostEntry))
            {
                GameObject instance = ExternalAssetCatalog.CreateInstance(ghostEntry, ghostEntry.Label, modelRoot);
                if (instance != null)
                {
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    Renderer[] ghostRenderers = instance.GetComponentsInChildren<Renderer>(true);
                    for (int i = 0; i < ghostRenderers.Length; i++)
                    {
                        UnityCompatibility.EnsureRendererHasMaterial(ghostRenderers[i], materials.Ghost);
                    }

                    assignedRenderer = instance.GetComponentInChildren<Renderer>();
                }
            }

            if (assignedRenderer == null)
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "GhostBody";
                body.transform.SetParent(modelRoot, false);
                body.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                body.transform.localScale = new Vector3(1.1f, 1.45f, 1.1f);
                UnityCompatibility.DestroyObject(body.GetComponent<Collider>());
                assignedRenderer = body.GetComponent<Renderer>();
                assignedRenderer.sharedMaterial = materials.Ghost;

                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "GhostHead";
                head.transform.SetParent(modelRoot, false);
                head.transform.localPosition = new Vector3(0f, 2.25f, 0f);
                head.transform.localScale = new Vector3(0.75f, 0.82f, 0.75f);
                UnityCompatibility.DestroyObject(head.GetComponent<Collider>());
                head.GetComponent<Renderer>().sharedMaterial = materials.Ghost;

                GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eyeL.transform.SetParent(modelRoot, false);
                eyeL.transform.localPosition = new Vector3(-0.16f, 2.24f, -0.29f);
                eyeL.transform.localScale = Vector3.one * 0.08f;
                UnityCompatibility.DestroyObject(eyeL.GetComponent<Collider>());
                eyeL.GetComponent<Renderer>().sharedMaterial = materials.GhostAccent;

                GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eyeR.transform.SetParent(modelRoot, false);
                eyeR.transform.localPosition = new Vector3(0.16f, 2.24f, -0.29f);
                eyeR.transform.localScale = Vector3.one * 0.08f;
                UnityCompatibility.DestroyObject(eyeR.GetComponent<Collider>());
                eyeR.GetComponent<Renderer>().sharedMaterial = materials.GhostAccent;
            }

            Light eyeLight = modelRoot.gameObject.AddComponent<Light>();
            eyeLight.type = LightType.Point;
            eyeLight.range = 3.3f;
            eyeLight.intensity = 0.7f;
            eyeLight.color = new Color(1f, 0.25f, 0.35f);
            eyeLight.transform.localPosition = new Vector3(0f, 2.15f, -0.15f);
            BobAndSpin bob = modelRoot.gameObject.AddComponent<BobAndSpin>();
            bob.bobAmplitude = 0.035f;
            bob.bobSpeed = 2.6f;
            bob.spinSpeed = 0f;
            return assignedRenderer;
        }

        private static Renderer TryCreateGhostMaDirect(Transform modelRoot, MaterialSet materials)
        {
            GameObject directPrefab = Resources.Load<GameObject>("ExternalAssets/DF_External_Ghost_Ma");
            if (directPrefab == null)
            {
                directPrefab = Resources.Load<GameObject>("ExternalAssets/ma");
            }

            if (directPrefab == null)
            {
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(directPrefab, modelRoot);
            instance.name = "GhostMa";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * 0.01f;

            SanitizeGhostMaInstance(instance);

            ExternalAssetPrefabAdapter adapter = instance.GetComponent<ExternalAssetPrefabAdapter>();
            if (adapter == null)
            {
                adapter = instance.AddComponent<ExternalAssetPrefabAdapter>();
            }

            adapter.role = ExternalAssetPrefabAdapter.AssetRole.Ghost;
            adapter.applyOnAwake = false;
            adapter.keepExistingColliders = false;
            adapter.normalizeGhostHeight = false;
            adapter.targetGhostHeight = 2.45f;
            adapter.ApplyNow();

            Renderer[] ghostRenderers = instance.GetComponentsInChildren<Renderer>(true);
            Texture2D ghostTexture = Resources.Load<Texture2D>("ExternalAssets/Textures_ghost_ma_basecolor");
            for (int i = 0; i < ghostRenderers.Length; i++)
            {
                Material material = UnityCompatibility.EnsureRendererHasMaterial(ghostRenderers[i], materials.Ghost);
                if (material == null)
                {
                    continue;
                }

                if (ghostTexture != null)
                {
                    material.mainTexture = ghostTexture;
                    if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", ghostTexture);
                    if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", ghostTexture);
                    UnityCompatibility.WriteColor(material, Color.white);
                }
            }

            return instance.GetComponentInChildren<Renderer>();
        }

        private static void SanitizeGhostMaInstance(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            RemoveGhostUtilityNodes(instance.transform);

            Animator[] animators = instance.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                if (animators[i] == null)
                {
                    continue;
                }

                animators[i].applyRootMotion = false;
                animators[i].enabled = false;
            }

            Animation[] legacyAnimations = instance.GetComponentsInChildren<Animation>(true);
            for (int i = 0; i < legacyAnimations.Length; i++)
            {
                if (legacyAnimations[i] != null)
                {
                    legacyAnimations[i].enabled = false;
                }
            }

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
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

        private static void RemoveGhostUtilityNodes(Transform root)
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
                    UnityCompatibility.DestroyObject(toDestroy[i]);
                }
            }
        }

        private static void CreateCabin(Transform parent, MaterialSet materials, Vector3 position, Vector3 size)
        {
            if (TryCreateExternalPropByRole(parent, position, Quaternion.identity, Vector3.one, "ForestCabinPrefabInstance", ExternalAssetPrefabAdapter.AssetRole.Cabin))
            {
                return;
            }

            CreateCabinShell(parent, materials, position, size);
        }

        private static void CreateShack(Transform parent, MaterialSet materials, Vector3 position)
        {
            if (TryCreateExternalPropByKeyword(parent, position, Quaternion.identity, Vector3.one, "ForestShackPrefabInstance", "shack", "hut"))
            {
                return;
            }

            CreateCabinShell(parent, materials, position, new Vector3(4.2f, 2.5f, 3.0f));
            CreateBlock(parent, "ShackDoorPanel", position + new Vector3(-1.0f, 1.1f, -1.58f), new Vector3(1.0f, 2.1f, 0.10f), materials.Rust, true);
        }

        private static void CreateCabinShell(Transform parent, MaterialSet materials, Vector3 position, Vector3 size)
        {
            GameObject cabin = new GameObject("Cabin");
            cabin.transform.SetParent(parent, false);
            cabin.transform.position = position;

            float halfX = size.x * 0.5f;
            float halfZ = size.z * 0.5f;
            float wallThickness = 0.22f;
            float doorWidth = Mathf.Min(1.45f, size.x - 0.8f);
            float doorHeight = Mathf.Min(2.15f, size.y - 0.45f);
            float frontSegmentWidth = Mathf.Max(0.4f, (size.x - doorWidth) * 0.5f);

            CreateLocalBlock(cabin.transform, "BackWall", new Vector3(0f, size.y * 0.5f, halfZ - wallThickness * 0.5f), new Vector3(size.x, size.y, wallThickness), materials.Hut, false);
            CreateLocalBlock(cabin.transform, "LeftWall", new Vector3(-halfX + wallThickness * 0.5f, size.y * 0.5f, 0f), new Vector3(wallThickness, size.y, size.z), materials.Hut, false);
            CreateLocalBlock(cabin.transform, "RightWall", new Vector3(halfX - wallThickness * 0.5f, size.y * 0.5f, 0f), new Vector3(wallThickness, size.y, size.z), materials.Hut, false);
            CreateLocalBlock(cabin.transform, "FrontWallLeft", new Vector3(-(doorWidth * 0.5f + frontSegmentWidth * 0.5f), size.y * 0.5f, -halfZ + wallThickness * 0.5f), new Vector3(frontSegmentWidth, size.y, wallThickness), materials.Hut, false);
            CreateLocalBlock(cabin.transform, "FrontWallRight", new Vector3(doorWidth * 0.5f + frontSegmentWidth * 0.5f, size.y * 0.5f, -halfZ + wallThickness * 0.5f), new Vector3(frontSegmentWidth, size.y, wallThickness), materials.Hut, false);
            CreateLocalBlock(cabin.transform, "FrontLintel", new Vector3(0f, (doorHeight + size.y) * 0.5f, -halfZ + wallThickness * 0.5f), new Vector3(doorWidth, Mathf.Max(0.25f, size.y - doorHeight), wallThickness), materials.Trim, false);
            CreateLocalBlock(cabin.transform, "Roof", new Vector3(0f, size.y + 0.55f, 0f), new Vector3(size.x + 0.35f, 0.35f, size.z + 0.55f), materials.HutRoof, false);
            GameObject doorPivot = new GameObject("DoorPivot");
            doorPivot.transform.SetParent(cabin.transform, false);
            doorPivot.transform.localPosition = new Vector3(-doorWidth * 0.5f + 0.05f, 0f, -halfZ + 0.03f);
            HingedDoorInteractable door = doorPivot.AddComponent<HingedDoorInteractable>();
            door.closedEulerAngles = Vector3.zero;
            door.openEulerAngles = new Vector3(0f, 108f, 0f);
            CreateLocalBlock(doorPivot.transform, "DoorPanel", new Vector3((Mathf.Min(1.25f, doorWidth - 0.15f)) * 0.5f, 1.1f, 0f), new Vector3(Mathf.Min(1.25f, doorWidth - 0.15f), Mathf.Min(2.1f, doorHeight - 0.05f), 0.08f), materials.Trim, false);
            CreateLocalBlock(cabin.transform, "WindowPanel", new Vector3(Mathf.Min(1.8f, halfX - 0.6f), 1.55f, -halfZ + 0.03f), new Vector3(1.1f, 0.92f, 0.06f), materials.CarGlass, true);
        }

        private static void ApplyExternalAdapter(GameObject target, ExternalAssetPrefabAdapter.AssetRole role)
        {
            if (target == null)
            {
                return;
            }

            ExternalAssetPrefabAdapter adapter = target.GetComponent<ExternalAssetPrefabAdapter>();
            if (adapter == null)
            {
                adapter = target.AddComponent<ExternalAssetPrefabAdapter>();
            }

            adapter.role = role;
            adapter.ApplyNow();
        }

        private static void TryInstantiateBakedExternalEnvironment(Transform parent)
        {
            GameObject prefab = Resources.Load<GameObject>("Generated/BakedExternalEnvironment");
            if (prefab == null)
            {
                return;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = "BakedExternalEnvironment";
        }

        private static bool TryCreateExternalPropByRole(Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, string objectName, ExternalAssetPrefabAdapter.AssetRole role)
        {
            if (!ExternalAssetCatalog.TryGetFirstByRole(role, out ExternalAssetCatalog.Entry entry))
            {
                return false;
            }

            return TryCreateExternalProp(parent, position, rotation, scale, objectName, entry);
        }

        private static bool TryCreateExternalPropByKeyword(Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, string objectName, params string[] keywords)
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                if (ExternalAssetCatalog.TryGetFirstByKeyword(keywords[i], out ExternalAssetCatalog.Entry entry))
                {
                    return TryCreateExternalProp(parent, position, rotation, scale, objectName, entry);
                }
            }

            return false;
        }

        private static bool TryCreateExternalProp(Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, string objectName, ExternalAssetCatalog.Entry entry)
        {
            GameObject instance = ExternalAssetCatalog.CreateInstance(entry, objectName, parent);
            if (instance == null)
            {
                return false;
            }

            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = Vector3.Scale(instance.transform.localScale, scale);
            EnsureExternalInstanceHasFallbackMaterials(instance, entry != null ? entry.Role : ExternalAssetPrefabAdapter.AssetRole.GeneralObstacle);
            return true;
        }

        private static void EnsureExternalInstanceHasFallbackMaterials(GameObject instance, ExternalAssetPrefabAdapter.AssetRole role)
        {
            if (instance == null)
            {
                return;
            }

            Material fallback;
            switch (role)
            {
                case ExternalAssetPrefabAdapter.AssetRole.Tree:
                    fallback = GeneratedMaterialLibrary.Get("Fallback_Tree", new Color(0.11f, 0.19f, 0.12f), 0.02f, 0.12f, false, false);
                    break;
                case ExternalAssetPrefabAdapter.AssetRole.Ghost:
                    fallback = GeneratedMaterialLibrary.Get("Fallback_Ghost", new Color(0.82f, 0.86f, 0.90f), 0.00f, 0.18f, true, false);
                    break;
                case ExternalAssetPrefabAdapter.AssetRole.Cabin:
                    fallback = GeneratedMaterialLibrary.Get("Fallback_Cabin", new Color(0.24f, 0.21f, 0.17f), 0.03f, 0.10f, false, false);
                    break;
                case ExternalAssetPrefabAdapter.AssetRole.Car:
                    fallback = GeneratedMaterialLibrary.Get("Fallback_Car", new Color(0.17f, 0.20f, 0.24f), 0.18f, 0.34f, false, false);
                    break;
                default:
                    fallback = GeneratedMaterialLibrary.Get("Fallback_Obstacle", new Color(0.21f, 0.23f, 0.24f), 0.02f, 0.18f, false, false);
                    break;
            }

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                UnityCompatibility.EnsureRendererHasMaterial(renderers[i], fallback);
            }
        }

        private static void CreateTent(Transform parent, MaterialSet materials, Vector3 position, float rotationY, bool canHide)
        {
            GameObject tent = new GameObject("Tent");
            tent.transform.SetParent(parent, false);
            tent.transform.position = position;
            tent.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            CreateLocalBlock(tent.transform, "Canvas", new Vector3(0f, 1.05f, 0f), new Vector3(2.8f, 1.8f, 2.2f), materials.RedCloth, false, Quaternion.Euler(0f, 0f, 0f));
            CreateLocalBlock(tent.transform, "Opening", new Vector3(0f, 0.75f, 1.12f), new Vector3(1.0f, 1.2f, 0.1f), materials.HutRoof, true);

            if (!canHide)
            {
                return;
            }

            HideSpotInteractable hideSpot = tent.AddComponent<HideSpotInteractable>();
            hideSpot.kind = HideSpotInteractable.HideSpotKind.Tent;
            hideSpot.hideLabel = "túp lều đổ";
            hideSpot.visibilityFactorWhenHidden = 0.18f;
            hideSpot.safeEscapeChance = 0.80f;
            hideSpot.captureCheckDistance = 2.35f;

            BoxCollider collider = tent.AddComponent<BoxCollider>();
            collider.size = new Vector3(2.2f, 1.6f, 1.8f);
            collider.center = new Vector3(0f, 0.8f, 0f);

            Transform hideAnchor = new GameObject("HideAnchor").transform;
            hideAnchor.SetParent(tent.transform, false);
            hideAnchor.localPosition = new Vector3(0f, 0f, -0.18f);
            hideAnchor.localRotation = Quaternion.Euler(0f, 180f, 0f);

            Transform exitAnchor = new GameObject("ExitAnchor").transform;
            exitAnchor.SetParent(tent.transform, false);
            exitAnchor.localPosition = new Vector3(0f, 0f, 1.65f);
            exitAnchor.localRotation = Quaternion.identity;

            CreateHideInteriorShell(tent.transform, materials.RedCloth, new Vector3(1.8f, 1.4f, 1.4f), 0.12f, false);
        }

        private static void CreateCampfire(Transform parent, MaterialSet materials, Vector3 position)
        {
            CreateRockGroup(parent, materials, position, 0.75f);
            Light light = new GameObject("CampfireLight").AddComponent<Light>();
            light.transform.SetParent(parent, false);
            light.transform.position = position + Vector3.up * 0.65f;
            light.type = LightType.Point;
            light.range = 7f;
            light.intensity = 1.0f;
            light.color = new Color(1f, 0.42f, 0.16f);
            light.gameObject.AddComponent<FlickerLight>();
        }

        private static void CreateTorii(Transform parent, MaterialSet materials, Vector3 position)
        {
            if (TryCreateExternalPropByKeyword(parent, position, Quaternion.identity, Vector3.one, "Torii", "torii", "gate"))
            {
                return;
            }

            CreateBlock(parent, "ToriiLeft", position + new Vector3(-1.4f, 2.1f, 0f), new Vector3(0.36f, 4.2f, 0.36f), materials.Shrine, false);
            CreateBlock(parent, "ToriiRight", position + new Vector3(1.4f, 2.1f, 0f), new Vector3(0.36f, 4.2f, 0.36f), materials.Shrine, false);
            CreateBlock(parent, "ToriiBeam", position + new Vector3(0f, 4.0f, 0f), new Vector3(3.8f, 0.28f, 0.36f), materials.Shrine, false);
            CreateBlock(parent, "ToriiCap", position + new Vector3(0f, 4.38f, 0f), new Vector3(4.5f, 0.22f, 0.56f), materials.Shrine, false);
        }

        private static void CreateShrineAltar(Transform parent, MaterialSet materials, Vector3 position)
        {
            if (TryCreateExternalPropByKeyword(parent, position, Quaternion.identity, Vector3.one, "ShrineAltar", "altar", "shrine"))
            {
                return;
            }

            CreateBlock(parent, "AltarBase", position + new Vector3(0f, 0.75f, 0f), new Vector3(2.5f, 1.5f, 1.4f), materials.Rock, false);
            CreateBlock(parent, "AltarTop", position + new Vector3(0f, 1.5f, 0f), new Vector3(2.8f, 0.2f, 1.6f), materials.Rock, false);
            CreateBlock(parent, "Cloth", position + new Vector3(0f, 1.55f, -0.35f), new Vector3(1.2f, 0.05f, 0.5f), materials.RedCloth, true);
        }

        private static void CreateStream(Transform parent, MaterialSet materials, Vector3 center)
        {
            if (TryCreateExternalPropByKeyword(parent, center, Quaternion.identity, Vector3.one, "Stream", "stream", "creek", "water"))
            {
                return;
            }

            Material water = UnityCompatibility.CreateLitMaterial(new Color(0.12f, 0.22f, 0.28f, 0.78f), 0.02f, 0.50f, true);
            CreateBlock(parent, "Stream", center + new Vector3(0f, 0.01f, 0f), new Vector3(12f, 0.03f, 5.4f), water, true);
        }

        private static void CreateRoadBarrier(Transform parent, MaterialSet materials, Vector3 position, float rotationY)
        {
            if (TryCreateExternalPropByKeyword(parent, position, Quaternion.Euler(0f, rotationY, 0f), Vector3.one, "Barrier", "barrier", "roadblock"))
            {
                return;
            }

            GameObject barrier = new GameObject("Barrier");
            barrier.transform.SetParent(parent, false);
            barrier.transform.position = position;
            barrier.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            CreateLocalBlock(barrier.transform, "PostL", new Vector3(-0.9f, 0.7f, 0f), new Vector3(0.14f, 1.4f, 0.14f), materials.Bark, false);
            CreateLocalBlock(barrier.transform, "PostR", new Vector3(0.9f, 0.7f, 0f), new Vector3(0.14f, 1.4f, 0.14f), materials.Bark, false);
            CreateLocalBlock(barrier.transform, "Rail", new Vector3(0f, 1.15f, 0f), new Vector3(2.1f, 0.14f, 0.18f), materials.Rust, false);
        }

        private static void CreateTable(Transform parent, MaterialSet materials, Vector3 position, float rotationY, float width, float depth)
        {
            if (TryCreateExternalPropByKeyword(parent, position, Quaternion.Euler(0f, rotationY, 0f), Vector3.one, "Table", "table", "desk"))
            {
                return;
            }

            GameObject table = new GameObject("Table");
            table.transform.SetParent(parent, false);
            table.transform.position = position;
            table.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            CreateLocalBlock(table.transform, "Top", new Vector3(0f, 0.9f, 0f), new Vector3(width, 0.12f, depth), materials.Trim, false);
            CreateLocalBlock(table.transform, "LegA", new Vector3(-width * 0.42f, 0.45f, -depth * 0.38f), new Vector3(0.12f, 0.9f, 0.12f), materials.Trim, false);
            CreateLocalBlock(table.transform, "LegB", new Vector3(width * 0.42f, 0.45f, -depth * 0.38f), new Vector3(0.12f, 0.9f, 0.12f), materials.Trim, false);
            CreateLocalBlock(table.transform, "LegC", new Vector3(-width * 0.42f, 0.45f, depth * 0.38f), new Vector3(0.12f, 0.9f, 0.12f), materials.Trim, false);
            CreateLocalBlock(table.transform, "LegD", new Vector3(width * 0.42f, 0.45f, depth * 0.38f), new Vector3(0.12f, 0.9f, 0.12f), materials.Trim, false);
        }

        private static void CreateShelf(Transform parent, MaterialSet materials, Vector3 position, float rotationY, int levels)
        {
            if (TryCreateExternalPropByKeyword(parent, position, Quaternion.Euler(0f, rotationY, 0f), Vector3.one, "Shelf", "shelf", "rack"))
            {
                return;
            }

            GameObject shelf = new GameObject("Shelf");
            shelf.transform.SetParent(parent, false);
            shelf.transform.position = position;
            shelf.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            CreateLocalBlock(shelf.transform, "Frame", new Vector3(0f, 1.25f, 0f), new Vector3(1.6f, 2.5f, 0.55f), materials.Hut, false);
            for (int i = 0; i < levels; i++)
            {
                CreateLocalBlock(shelf.transform, "Level" + i, new Vector3(0f, 0.5f + i * 0.75f, 0f), new Vector3(1.5f, 0.08f, 0.50f), materials.Trim, false);
            }
        }

        private static void CreateBushHideSpot(Transform parent, MaterialSet materials, Vector3 position, float rotationY)
        {
            GameObject root = new GameObject("BushHideSpot");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            HideSpotInteractable hideSpot = root.AddComponent<HideSpotInteractable>();
            hideSpot.kind = HideSpotInteractable.HideSpotKind.Bush;
            hideSpot.hideLabel = "bụi rậm";
            hideSpot.visibilityFactorWhenHidden = 0.32f;
            hideSpot.safeEscapeChance = 0.84f;
            hideSpot.captureCheckDistance = 2.3f;
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(2.6f, 2.0f, 1.6f);
            collider.center = new Vector3(0f, 1.0f, 0f);
            CreateLocalBlock(root.transform, "BushA", new Vector3(0f, 0.8f, 0f), new Vector3(2.2f, 1.2f, 1.4f), materials.Bush, false);
            CreateLocalBlock(root.transform, "BushB", new Vector3(-0.4f, 1.1f, 0.2f), new Vector3(1.2f, 1.0f, 1.0f), materials.Leaves, true);
            CreateLocalBlock(root.transform, "BushC", new Vector3(0.5f, 1.0f, -0.1f), new Vector3(1.0f, 0.9f, 0.9f), materials.Leaves, true);
            Transform hideAnchor = new GameObject("HideAnchor").transform;
            hideAnchor.SetParent(root.transform, false);
            hideAnchor.localPosition = new Vector3(0f, 0f, -0.1f);
            hideAnchor.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Transform exitAnchor = new GameObject("ExitAnchor").transform;
            exitAnchor.SetParent(root.transform, false);
            exitAnchor.localPosition = new Vector3(0f, 0f, 1.8f);
        }

        private static void CreateLogHideSpot(Transform parent, MaterialSet materials, Vector3 position, float rotationY)
        {
            GameObject root = new GameObject("LogHideSpot");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            HideSpotInteractable hideSpot = root.AddComponent<HideSpotInteractable>();
            hideSpot.kind = HideSpotInteractable.HideSpotKind.Log;
            hideSpot.hideLabel = "thân gỗ rỗng";
            hideSpot.visibilityFactorWhenHidden = 0.22f;
            hideSpot.safeEscapeChance = 0.80f;
            hideSpot.captureCheckDistance = 2.25f;
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(3.0f, 1.8f, 1.5f);
            collider.center = new Vector3(0f, 0.9f, 0f);
            CreateLocalBlock(root.transform, "Log", new Vector3(0f, 0.9f, 0f), new Vector3(2.8f, 1.2f, 1.2f), materials.Bark, false);
            CreateLocalBlock(root.transform, "Hollow", new Vector3(0f, 0.92f, 0.42f), new Vector3(1.9f, 0.7f, 0.5f), materials.Ground, true);
            Transform hideAnchor = new GameObject("HideAnchor").transform;
            hideAnchor.SetParent(root.transform, false);
            hideAnchor.localPosition = new Vector3(0f, 0f, 0.1f);
            hideAnchor.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Transform exitAnchor = new GameObject("ExitAnchor").transform;
            exitAnchor.SetParent(root.transform, false);
            exitAnchor.localPosition = new Vector3(0f, 0f, -1.9f);
            exitAnchor.localRotation = Quaternion.identity;

            CreateHideInteriorShell(root.transform, materials.Bark, new Vector3(1.45f, 0.88f, 1.00f), -0.05f, true);
        }

        private static void CreatePickupSpawnPoint(Transform parent, MaterialSet materials, ItemType itemType, string name, Vector3 position, PickupVisualType visualType)
        {
            GameObject pointObject = new GameObject(itemType + "SpawnPoint");
            pointObject.transform.SetParent(parent, false);
            pointObject.transform.position = ResolvePickupPlacement(position);
            ItemSpawnPoint spawnPoint = pointObject.AddComponent<ItemSpawnPoint>();
            spawnPoint.ItemType = itemType;
            spawnPoint.DisplayName = name;
            spawnPoint.VisualType = visualType;
            spawnPoint.SnapToGroundWhenRebuilding = false;
            spawnPoint.BobAndSpin = true;
            RebuildPickupFromSpawnPoint(spawnPoint, true);
        }

        private static void CreatePickup(Transform parent, MaterialSet materials, ItemType itemType, string name, Vector3 position, PickupVisualType visualType, GameObject visualPrefabOverride = null, string externalVisualAssetId = "", bool bobAndSpin = true)
        {
            itemType = ResolveRandomizedPart(itemType);
            if (visualType == PickupVisualType.Part)
            {
                name = GetPartDisplayName(itemType);
            }
            else if (string.IsNullOrWhiteSpace(name))
            {
                name = GetDefaultItemDisplayName(itemType);
            }

            GameObject pickup = new GameObject("SpawnedPickup");
            pickup.transform.SetParent(parent, false);
            pickup.transform.localPosition = position;
            pickup.transform.localRotation = Quaternion.identity;
            pickup.transform.localScale = Vector3.one;
            PickupInteractable interactable = pickup.AddComponent<PickupInteractable>();
            interactable.itemType = itemType;
            interactable.displayName = name;
            SphereCollider collider = pickup.AddComponent<SphereCollider>();
            collider.radius = 0.35f;
            collider.isTrigger = true;

            Transform visualRoot = new GameObject("Visual").transform;
            visualRoot.SetParent(pickup.transform, false);
            visualRoot.localPosition = Vector3.zero;
            if (bobAndSpin)
            {
                visualRoot.gameObject.AddComponent<BobAndSpin>().bobAmplitude = 0.08f;
            }
            CreatePickupVisual(visualRoot, materials, itemType, visualType, visualPrefabOverride, externalVisualAssetId);
        }

        private static Vector3 ResolvePickupPlacement(Vector3 requestedPosition)
        {
            Vector3[] offsets =
            {
                Vector3.zero,
                new Vector3(0.85f, 0f, 0f),
                new Vector3(-0.85f, 0f, 0f),
                new Vector3(0f, 0f, 0.85f),
                new Vector3(0f, 0f, -0.85f),
                new Vector3(0.65f, 0f, 0.65f),
                new Vector3(-0.65f, 0f, 0.65f),
                new Vector3(0.65f, 0f, -0.65f),
                new Vector3(-0.65f, 0f, -0.65f)
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                Vector3 probe = requestedPosition + offsets[i] + Vector3.up * 3.0f;
                if (Physics.Raycast(probe, Vector3.down, out RaycastHit hit, 8f, ~0, QueryTriggerInteraction.Ignore))
                {
                    Vector3 candidate = hit.point + Vector3.up * 0.45f;
                    string hitName = hit.collider != null ? hit.collider.name.ToLowerInvariant() : string.Empty;
                    if (hitName.Contains("rock"))
                    {
                        continue;
                    }

                    if (!Physics.CheckSphere(candidate, 0.28f, ~0, QueryTriggerInteraction.Ignore))
                    {
                        return candidate;
                    }
                }
            }

            return requestedPosition + Vector3.up * 0.45f;
        }

        private static void CreatePickupVisual(Transform root, MaterialSet materials, ItemType itemType, PickupVisualType visualType, GameObject visualPrefabOverride, string externalVisualAssetId)
        {
            if (visualPrefabOverride != null)
            {
                GameObject instance = Object.Instantiate(visualPrefabOverride, root);
                instance.name = "VisualPrefabOverride";
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                RemoveCollidersFromHierarchy(instance.transform);
                return;
            }

            if (!string.IsNullOrWhiteSpace(externalVisualAssetId) && ExternalAssetCatalog.TryGetEntry(externalVisualAssetId, out ExternalAssetCatalog.Entry entry))
            {
                GameObject instance = ExternalAssetCatalog.CreateInstance(entry, "ExternalPickupVisual", root);
                if (instance != null)
                {
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = entry.DefaultScale == Vector3.zero ? Vector3.one : entry.DefaultScale;
                    RemoveCollidersFromHierarchy(instance.transform);
                    return;
                }
            }

            if (visualType == PickupVisualType.Part && ExternalItemVisualLibrary.TryCreatePickupVisual(root, itemType, materials.Highlight))
            {
                return;
            }

            switch (visualType)
            {
                case PickupVisualType.Part:
                    CreateLocalBlock(root, "PartBody", Vector3.zero, new Vector3(0.44f, 0.22f, 0.30f), materials.Highlight, true, Quaternion.Euler(12f, 24f, 0f));
                    CreateLocalBlock(root, "PartDetail", new Vector3(0f, 0.08f, 0f), new Vector3(0.20f, 0.08f, 0.18f), materials.Trim, true, Quaternion.Euler(0f, 18f, 0f));
                    break;
                case PickupVisualType.Note:
                    CreateLocalBlock(root, "Note", Vector3.zero, new Vector3(0.42f, 0.05f, 0.58f), materials.Note, true, Quaternion.Euler(82f, 14f, 0f));
                    break;
            }
        }

        private static void RemoveCollidersFromHierarchy(Transform root)
        {
            if (root == null)
            {
                return;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    UnityCompatibility.DestroyObject(colliders[i]);
                }
            }
        }

        private static Transform EnsureChildPoint(Transform parent, string childName, Vector3 localPosition, Quaternion localRotation)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                child = new GameObject(childName).transform;
                child.SetParent(parent, false);
            }

            child.localPosition = localPosition;
            child.localRotation = localRotation;
            child.localScale = Vector3.one;
            return child;
        }

        private static void CreateTree(Transform parent, MaterialSet materials, Vector3 position, float scale)
        {
            if (TryCreateExternalPropByRole(parent, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), Vector3.one * scale, "Tree", ExternalAssetPrefabAdapter.AssetRole.Tree))
            {
                return;
            }

            GameObject tree = new GameObject("Tree");
            tree.transform.SetParent(parent, false);
            tree.transform.position = position;
            tree.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            CreateLocalBlock(tree.transform, "Trunk", new Vector3(0f, 2.5f * scale, 0f), new Vector3(0.8f * scale, 5.0f * scale, 0.8f * scale), materials.Bark, false);
            CreateLocalBlock(tree.transform, "LeavesA", new Vector3(0f, 5.4f * scale, 0f), new Vector3(3.6f * scale, 2.8f * scale, 3.6f * scale), materials.Leaves, true);
            CreateLocalBlock(tree.transform, "LeavesB", new Vector3(0.3f * scale, 7.0f * scale, 0.1f * scale), new Vector3(2.7f * scale, 2.2f * scale, 2.7f * scale), materials.Leaves, true);
        }

        private static void CreateBushCluster(Transform parent, MaterialSet materials, Vector3 position, float scale)
        {
            if (TryCreateExternalPropByKeyword(parent, position, Quaternion.identity, Vector3.one * scale, "BushCluster", "bush", "shrub", "foliage"))
            {
                return;
            }

            GameObject bush = new GameObject("BushCluster");
            bush.transform.SetParent(parent, false);
            bush.transform.position = position;
            CreateLocalBlock(bush.transform, "BushA", new Vector3(0f, 0.5f * scale, 0f), new Vector3(1.8f * scale, 1.0f * scale, 1.4f * scale), materials.Bush, false);
            CreateLocalBlock(bush.transform, "BushB", new Vector3(-0.5f * scale, 0.6f * scale, 0.2f * scale), new Vector3(1.1f * scale, 0.8f * scale, 0.9f * scale), materials.Leaves, true);
            CreateLocalBlock(bush.transform, "BushC", new Vector3(0.45f * scale, 0.62f * scale, -0.15f * scale), new Vector3(1.0f * scale, 0.7f * scale, 0.8f * scale), materials.Leaves, true);
        }

        private static void CreateRockGroup(Transform parent, MaterialSet materials, Vector3 position, float scale)
        {
            scale *= 0.72f;
            if (TryCreateExternalPropByKeyword(parent, position, Quaternion.identity, Vector3.one * scale, "RockGroup", "rock", "stone", "boulder"))
            {
                return;
            }

            GameObject rocks = new GameObject("RockGroup");
            rocks.transform.SetParent(parent, false);
            rocks.transform.position = position;
            CreateLocalBlock(rocks.transform, "RockA", new Vector3(0f, 0.35f * scale, 0f), new Vector3(1.4f * scale, 0.7f * scale, 1.2f * scale), materials.Rock, false);
            CreateLocalBlock(rocks.transform, "RockB", new Vector3(0.6f * scale, 0.28f * scale, 0.5f * scale), new Vector3(0.9f * scale, 0.55f * scale, 0.8f * scale), materials.Rock, true);
            CreateLocalBlock(rocks.transform, "RockC", new Vector3(-0.5f * scale, 0.24f * scale, -0.4f * scale), new Vector3(0.8f * scale, 0.5f * scale, 0.7f * scale), materials.Rock, true);
        }

        private static void CreateCrate(Transform parent, MaterialSet materials, Vector3 position, Vector3 scale)
        {
            if (TryCreateExternalPropByKeyword(parent, position, Quaternion.identity, scale, "Crate", "crate", "box"))
            {
                return;
            }

            CreateBlock(parent, "Crate", position, scale, materials.Trim, false);
        }

        private static void CreateWheel(Transform parent, MaterialSet materials, Vector3 position, bool intact)
        {
            if (!intact)
            {
                return;
            }

            CreateLocalBlock(parent, "Wheel", position, new Vector3(0.62f, 0.62f, 0.22f), materials.HutRoof, false);
        }

        private static void CreateSignPost(Transform parent, MaterialSet materials, Vector3 position, float rotationY, string text)
        {
            if (TryCreateExternalPropByKeyword(parent, position, Quaternion.Euler(0f, rotationY, 0f), Vector3.one, "Sign_" + text, "sign", "post"))
            {
                return;
            }

            GameObject sign = new GameObject("Sign_" + text);
            sign.transform.SetParent(parent, false);
            sign.transform.position = position;
            sign.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            CreateLocalBlock(sign.transform, "Post", new Vector3(0f, 0.9f, 0f), new Vector3(0.14f, 1.8f, 0.14f), materials.Bark, false);
            CreateLocalBlock(sign.transform, "Board", new Vector3(0f, 1.55f, 0f), new Vector3(1.2f, 0.45f, 0.10f), materials.Hut, false);
        }

        private static void CreateClosetHideSpot(Transform parent, MaterialSet materials, Vector3 position, float rotationY)
        {
            GameObject root = new GameObject("ClosetHideSpot");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            HideSpotInteractable hideSpot = root.AddComponent<HideSpotInteractable>();
            hideSpot.kind = HideSpotInteractable.HideSpotKind.Closet;
            hideSpot.hideLabel = "tủ gỗ";
            hideSpot.visibilityFactorWhenHidden = 0.10f;
            hideSpot.safeEscapeChance = 0.78f;
            hideSpot.captureCheckDistance = 2.05f;
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.8f, 2.2f, 1.1f);
            collider.center = new Vector3(0f, 1.1f, 0f);

            CreateLocalBlock(root.transform, "ClosetBody", new Vector3(0f, 1.1f, 0f), new Vector3(1.6f, 2.2f, 0.9f), materials.Trim, false);
            GameObject leftDoorPivot = new GameObject("ClosetDoorLeft");
            leftDoorPivot.transform.SetParent(root.transform, false);
            leftDoorPivot.transform.localPosition = new Vector3(-0.38f, 0f, 0.47f);
            HingedDoorInteractable leftDoor = leftDoorPivot.AddComponent<HingedDoorInteractable>();
            leftDoor.closedEulerAngles = Vector3.zero;
            leftDoor.openEulerAngles = new Vector3(0f, -96f, 0f);
            CreateLocalBlock(leftDoorPivot.transform, "Door", new Vector3(0.38f, 1.0f, 0f), new Vector3(0.75f, 2.0f, 0.08f), materials.Hut, false);

            GameObject rightDoorPivot = new GameObject("ClosetDoorRight");
            rightDoorPivot.transform.SetParent(root.transform, false);
            rightDoorPivot.transform.localPosition = new Vector3(0.38f, 0f, 0.47f);
            HingedDoorInteractable rightDoor = rightDoorPivot.AddComponent<HingedDoorInteractable>();
            rightDoor.closedEulerAngles = Vector3.zero;
            rightDoor.openEulerAngles = new Vector3(0f, 96f, 0f);
            CreateLocalBlock(rightDoorPivot.transform, "Door", new Vector3(-0.38f, 1.0f, 0f), new Vector3(0.75f, 2.0f, 0.08f), materials.Hut, false);

            Transform hideAnchor = new GameObject("HideAnchor").transform;
            hideAnchor.SetParent(root.transform, false);
            hideAnchor.localPosition = new Vector3(0f, 0f, -0.05f);
            hideAnchor.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Transform exitAnchor = new GameObject("ExitAnchor").transform;
            exitAnchor.SetParent(root.transform, false);
            exitAnchor.localPosition = new Vector3(0f, 0f, 1.5f);

            CreateHideInteriorShell(root.transform, materials.Hut, new Vector3(1.18f, 1.85f, 0.68f), -0.06f, true);
        }

        private static void CreateHideInteriorShell(Transform parent, Material material, Vector3 innerSize, float centerZOffset, bool openFront)
        {
            float halfX = innerSize.x * 0.5f;
            float halfY = innerSize.y * 0.5f;
            float halfZ = innerSize.z * 0.5f;
            float thickness = 0.06f;
            float centerY = Mathf.Max(0.45f, halfY + 0.12f);

            CreateLocalBlock(parent, "InteriorBack", new Vector3(0f, centerY, centerZOffset - halfZ), new Vector3(innerSize.x, innerSize.y, thickness), material, true);
            CreateLocalBlock(parent, "InteriorLeft", new Vector3(-halfX, centerY, centerZOffset), new Vector3(thickness, innerSize.y, innerSize.z), material, true);
            CreateLocalBlock(parent, "InteriorRight", new Vector3(halfX, centerY, centerZOffset), new Vector3(thickness, innerSize.y, innerSize.z), material, true);
            CreateLocalBlock(parent, "InteriorRoof", new Vector3(0f, centerY + halfY, centerZOffset), new Vector3(innerSize.x, thickness, innerSize.z), material, true);
            CreateLocalBlock(parent, "InteriorFloor", new Vector3(0f, 0.06f, centerZOffset), new Vector3(innerSize.x * 0.96f, 0.08f, innerSize.z * 0.96f), material, true);

            if (!openFront)
            {
                CreateLocalBlock(parent, "InteriorFront", new Vector3(0f, centerY, centerZOffset + halfZ), new Vector3(innerSize.x, innerSize.y, thickness), material, true);
            }
            else
            {
                CreateLocalBlock(parent, "InteriorFrontLeft", new Vector3(-innerSize.x * 0.27f, centerY, centerZOffset + halfZ), new Vector3(innerSize.x * 0.34f, innerSize.y, thickness), material, true);
                CreateLocalBlock(parent, "InteriorFrontRight", new Vector3(innerSize.x * 0.27f, centerY, centerZOffset + halfZ), new Vector3(innerSize.x * 0.34f, innerSize.y, thickness), material, true);
            }
        }

        private static void CreateLanternLight(Transform parent, MaterialSet materials, Vector3 position, float intensity)
        {
            GameObject anchor = new GameObject("LanternLight");
            anchor.transform.SetParent(parent, false);
            anchor.transform.position = position;
            Light light = anchor.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 5.5f;
            light.intensity = intensity;
            light.color = new Color(1.0f, 0.72f, 0.34f);
            anchor.AddComponent<FlickerLight>().variation = 0.12f;
        }

        private static Transform CreateRoot(Transform parent, string name)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            return root.transform;
        }

        private static GameObject CreateBlock(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool noCollider)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.position = position;
            block.transform.localScale = scale;
            if (material != null)
            {
                Renderer renderer = block.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = material;
                }
            }
            if (noCollider)
            {
                UnityCompatibility.DestroyObject(block.GetComponent<Collider>());
            }
            return block;
        }

        private static GameObject CreateLocalBlock(Transform parent, string name, Vector3 localPosition, Vector3 scale, Material material, bool noCollider, Quaternion? localRotation = null)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = localPosition;
            block.transform.localRotation = localRotation ?? Quaternion.identity;
            block.transform.localScale = scale;
            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }
            if (noCollider)
            {
                UnityCompatibility.DestroyObject(block.GetComponent<Collider>());
            }
            return block;
        }

        private static void CreateInvisibleWall(Transform parent, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Boundary";
            wall.transform.SetParent(parent, false);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        private static bool IsInsideAny(float x, float z, float padding, RectXZ[] rects)
        {
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].Contains(x, z, padding))
                {
                    return true;
                }
            }
            return false;
        }

        private static Vector3 FlattenXZ(Vector3 value)
        {
            value.y = 0f;
            return value;
        }

    }
}
