using UnityEngine;

namespace HollowManor
{
    public static class UnityCompatibility
    {
        private static Shader cachedLitShader;

        public static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(target);
                return;
            }
#endif

            Object.Destroy(target);
        }

        public static Shader ResolveLitShader()
        {
            if (cachedLitShader != null)
            {
                return cachedLitShader;
            }

            string[] candidates =
            {
                "Universal Render Pipeline/Lit",
                "Standard",
                "Lit",
                "Legacy Shaders/Diffuse",
                "Sprites/Default"
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                Shader shader = Shader.Find(candidates[i]);
                if (shader != null)
                {
                    cachedLitShader = shader;
                    return cachedLitShader;
                }
            }

            return null;
        }

        public static Material CreateLitMaterial(Color color, float metallic, float smoothness, bool transparent)
        {
            Shader shader = ResolveLitShader();
            Material material = shader != null ? new Material(shader) : new Material(Shader.Find("Sprites/Default"));
            ConfigureMaterial(material, color, metallic, smoothness, transparent);
            return material;
        }

        public static Material CloneMaterial(Material source, Color? overrideColor = null)
        {
            if (source == null)
            {
                Color fallbackColor = overrideColor ?? Color.white;
                return CreateLitMaterial(fallbackColor, 0.02f, 0.08f, fallbackColor.a < 0.99f);
            }

            Material clone = new Material(source);
            Color color = overrideColor ?? ReadColor(clone);
            float metallic = clone.HasProperty("_Metallic") ? clone.GetFloat("_Metallic") : 0f;
            float smoothness = clone.HasProperty("_Smoothness")
                ? clone.GetFloat("_Smoothness")
                : (clone.HasProperty("_Glossiness") ? clone.GetFloat("_Glossiness") : 0.2f);

            ConfigureMaterial(clone, color, metallic, smoothness, color.a < 0.99f);
            return clone;
        }

        public static void ConfigureMaterial(Material material, Color color, float metallic, float smoothness, bool transparent)
        {
            if (material == null)
            {
                return;
            }

            Shader litShader = ResolveLitShader();
            if (litShader != null && material.shader != litShader)
            {
                material.shader = litShader;
            }

            WriteColor(material, color);

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (transparent)
            {
                if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
                if (material.HasProperty("_AlphaClip")) material.SetFloat("_AlphaClip", 0f);
                if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 3f);
                if (material.HasProperty("_SrcBlend")) material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (material.HasProperty("_DstBlend")) material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (material.HasProperty("_ZWrite")) material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
            else
            {
                if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 0f);
                if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 0f);
                if (material.HasProperty("_ZWrite")) material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
            }
        }

        public static void SetRendererColor(Renderer renderer, Color color)
        {
            Material material = EnsureRendererHasMaterial(renderer, null);
            if (material == null)
            {
                return;
            }

            WriteColor(material, color);
        }

        public static Material EnsureRendererHasMaterial(Renderer renderer, Material preferredFallback)
        {
            if (renderer == null)
            {
                return null;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Material shared = renderer.sharedMaterial;
                if (shared == null)
                {
                    shared = preferredFallback != null ? preferredFallback : CreateLitMaterial(new Color(0.7f, 0.7f, 0.7f), 0.02f, 0.12f, false);
                    renderer.sharedMaterial = shared;
                }

                return shared;
            }
#endif

            Material runtimeMaterial = renderer.material;
            if (runtimeMaterial == null)
            {
                Material source = renderer.sharedMaterial != null ? renderer.sharedMaterial : preferredFallback;
                runtimeMaterial = source != null ? CloneMaterial(source) : CreateLitMaterial(new Color(0.7f, 0.7f, 0.7f), 0.02f, 0.12f, false);
                renderer.material = runtimeMaterial;
            }

            return runtimeMaterial;
        }

        public static Color ReadColor(Material material)
        {
            if (material == null)
            {
                return Color.white;
            }

            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            if (material.HasProperty("_Color"))
            {
                return material.GetColor("_Color");
            }

            return material.color;
        }

        public static void WriteColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            material.color = color;
        }
    }
}
