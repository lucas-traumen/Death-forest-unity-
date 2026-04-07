using System.Collections.Generic;
using UnityEngine;

namespace HollowManor
{
    public static class ExternalItemVisualLibrary
    {
        private sealed class ItemVisualSpec
        {
            public readonly string ModelPath;
            public readonly string TexturePath;
            public readonly Vector3 Rotation;
            public readonly float TargetSize;
            public readonly Vector3 Offset;
            public readonly Color Tint;

            public ItemVisualSpec(string modelPath, string texturePath, Vector3 rotation, float targetSize, Vector3 offset, Color tint)
            {
                ModelPath = modelPath;
                TexturePath = texturePath;
                Rotation = rotation;
                TargetSize = targetSize;
                Offset = offset;
                Tint = tint;
            }
        }

        private static readonly Dictionary<ItemType, ItemVisualSpec> PickupSpecs = new Dictionary<ItemType, ItemVisualSpec>
        {
            { ItemType.CarBattery, new ItemVisualSpec("ExternalAssets/Items/battery", "ExternalAssets/Items/Textures/battery_basecolor", new Vector3(-90f, 0f, 0f), 0.75f, new Vector3(0f, 0.05f, 0f), new Color(0.85f, 0.84f, 0.80f)) },
            { ItemType.FanBelt, new ItemVisualSpec("ExternalAssets/Items/v_belt", "ExternalAssets/Items/Textures/v_belt_basecolor", new Vector3(-90f, 0f, 0f), 0.65f, new Vector3(0f, 0.02f, 0f), new Color(0.18f, 0.18f, 0.18f)) },
            { ItemType.SparkPlugKit, new ItemVisualSpec("ExternalAssets/Items/spark_plug", "ExternalAssets/Items/Textures/spark_plug_basecolor", new Vector3(-90f, 0f, 0f), 0.62f, new Vector3(0f, 0.04f, 0f), new Color(0.88f, 0.84f, 0.76f)) },
            { ItemType.SpareWheel, new ItemVisualSpec("ExternalAssets/Items/tire_wheel", "ExternalAssets/Items/Textures/tire_wheel_basecolor", new Vector3(0f, 0f, 90f), 0.82f, new Vector3(0f, 0.18f, 0f), new Color(0.26f, 0.26f, 0.28f)) },
        };

        public static bool TryCreatePickupVisual(Transform root, ItemType itemType, Material fallbackMaterial)
        {
            if (root == null || !PickupSpecs.TryGetValue(itemType, out ItemVisualSpec spec))
            {
                return false;
            }

            return TryInstantiateVisual(root, spec, fallbackMaterial, itemType.ToString());
        }

        public static bool TryCreateJerryCanProp(Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, Material fallbackMaterial)
        {
            if (parent == null)
            {
                return false;
            }

            ItemVisualSpec spec = new ItemVisualSpec(
                "ExternalAssets/Items/jerry_can",
                "ExternalAssets/Items/Textures/jerry_can_basecolor",
                new Vector3(-90f, 0f, 0f),
                1.15f,
                new Vector3(0f, 0.02f, 0f),
                new Color(0.60f, 0.12f, 0.10f));

            GameObject root = new GameObject("JerryCanClue");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.transform.rotation = rotation;
            root.transform.localScale = scale;
            return TryInstantiateVisual(root.transform, spec, fallbackMaterial, "JerryCan") || CreateJerryCanFallback(root.transform, fallbackMaterial);
        }

        private static bool TryInstantiateVisual(Transform root, ItemVisualSpec spec, Material fallbackMaterial, string objectName)
        {
            GameObject modelPrefab = Resources.Load<GameObject>(spec.ModelPath);
            if (modelPrefab == null)
            {
                return false;
            }

            GameObject instance = Object.Instantiate(modelPrefab, root);
            instance.name = objectName;
            instance.transform.localPosition = spec.Offset;
            instance.transform.localRotation = Quaternion.Euler(spec.Rotation);
            instance.transform.localScale = Vector3.one;

            SanitizeImportedModel(instance.transform);
            NormalizeVisualSize(instance.transform, spec.TargetSize);
            ApplyItemMaterial(instance, spec.TexturePath, fallbackMaterial, spec.Tint);
            RemoveColliders(instance.transform);
            return true;
        }

        private static void NormalizeVisualSize(Transform root, float targetSize)
        {
            Bounds bounds;
            if (!TryCalculateRendererBounds(root, out bounds))
            {
                return;
            }

            float maxDim = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (maxDim <= 0.001f)
            {
                return;
            }

            float factor = targetSize / maxDim;
            root.localScale *= factor;

            if (TryCalculateRendererBounds(root, out bounds))
            {
                Vector3 shift = new Vector3(0f, -bounds.min.y + 0.02f, 0f);
                root.localPosition += shift;
            }
        }

        private static void ApplyItemMaterial(GameObject instance, string texturePath, Material fallbackMaterial, Color tint)
        {
            Texture2D texture = Resources.Load<Texture2D>(texturePath);
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material material = UnityCompatibility.EnsureRendererHasMaterial(renderer, fallbackMaterial);
                if (material == null)
                {
                    continue;
                }

                if (texture != null)
                {
                    material.mainTexture = texture;
                    if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
                    if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
                    UnityCompatibility.WriteColor(material, Color.white);
                }
                else
                {
                    UnityCompatibility.WriteColor(material, tint);
                }
            }
        }

        private static void SanitizeImportedModel(Transform root)
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

            Animator[] animators = root.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                if (animators[i] != null)
                {
                    animators[i].applyRootMotion = false;
                    animators[i].enabled = false;
                }
            }
        }

        private static void RemoveColliders(Transform root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    UnityCompatibility.DestroyObject(colliders[i]);
                }
            }
        }

        private static bool TryCalculateRendererBounds(Transform root, out Bounds bounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bounds = default;
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private static bool CreateJerryCanFallback(Transform root, Material material)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "JerryCanBody";
            body.transform.SetParent(root, false);
            body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            body.transform.localScale = new Vector3(0.48f, 0.68f, 0.18f);
            Renderer r = body.GetComponent<Renderer>();
            if (r != null && material != null) r.sharedMaterial = material;
            UnityCompatibility.DestroyObject(body.GetComponent<Collider>());
            return true;
        }
    }
}
