using UnityEngine;

namespace HollowManor
{
    public sealed class DeathForestSceneRoot : MonoBehaviour
    {
        [SerializeField] private bool authoredInEditor = true;
        [SerializeField] private Transform worldRoot;
        [SerializeField] private Transform propRoot;
        [SerializeField] private Transform interactableRoot;
        [SerializeField] private Transform enemyRoot;
        [SerializeField] private Transform lightRoot;
        [SerializeField] private Transform externalAssetRoot;

        public bool AuthoredInEditor => authoredInEditor;
        public Transform WorldRoot => worldRoot;
        public Transform PropRoot => propRoot;
        public Transform InteractableRoot => interactableRoot;
        public Transform EnemyRoot => enemyRoot;
        public Transform LightRoot => lightRoot;
        public Transform ExternalAssetRoot => externalAssetRoot;

        private void Awake()
        {
            ResolveMissingRoots();
            GameManager manager = GetComponent<GameManager>();
            if (manager != null)
            {
                manager.EnsureSceneBindings();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveMissingRoots();
        }
#endif

        public void BindRoots(Transform world, Transform props, Transform interactables, Transform enemies, Transform lighting, Transform externalAssets)
        {
            worldRoot = world;
            propRoot = props;
            interactableRoot = interactables;
            enemyRoot = enemies;
            lightRoot = lighting;
            externalAssetRoot = externalAssets;
        }

        public void ResolveMissingRoots()
        {
            if (worldRoot == null) worldRoot = transform.Find("World");
            if (propRoot == null) propRoot = transform.Find("Props");
            if (interactableRoot == null) interactableRoot = transform.Find("Interactables");
            if (enemyRoot == null) enemyRoot = transform.Find("Ghosts");
            if (lightRoot == null) lightRoot = transform.Find("Lighting");
            if (externalAssetRoot == null) externalAssetRoot = transform.Find("SceneExternalAssets");
        }
    }
}
