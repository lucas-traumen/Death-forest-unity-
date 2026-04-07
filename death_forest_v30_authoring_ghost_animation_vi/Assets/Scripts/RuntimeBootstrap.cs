using UnityEngine;

namespace HollowManor
{
    public static class RuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntimeBootstrap()
        {
#if UNITY_2023_1_OR_NEWER
            if (Object.FindFirstObjectByType<DeathForestSceneRoot>() != null || Object.FindFirstObjectByType<GameManager>() != null)
#else
            if (Object.FindObjectOfType<DeathForestSceneRoot>() != null || Object.FindObjectOfType<GameManager>() != null)
#endif
            {
                return;
            }

            Debug.LogWarning("[Death Forest] Project nay da chuyen sang scene-authoring-first. Scene dang mo chua co DeathForestSceneRoot / GameManager authored, nen se khong tu procedural-build map luc Play nua. Hay mo scene authored hoac chay menu Death Forest/Scene Authoring/Create Or Refresh Current Scene.");
        }
    }
}
