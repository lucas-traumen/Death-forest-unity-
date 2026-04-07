#if UNITY_EDITOR
using UnityEditor;

namespace HollowManor.EditorTools
{
    public static class SceneTools
    {
        [MenuItem("Death Forest/Create Empty Play Scene")]
        public static void CreateEmptyPlayScene()
        {
            SceneAuthoringTools.CreateOrRefreshCurrentScene(interactive: true, saveSceneAsset: true);
        }
    }
}
#endif
