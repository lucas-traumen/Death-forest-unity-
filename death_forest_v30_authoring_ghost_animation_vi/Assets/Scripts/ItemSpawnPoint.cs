using UnityEngine;

namespace HollowManor
{
    [ExecuteAlways]
    public sealed class ItemSpawnPoint : MonoBehaviour
    {
        [SerializeField] private ItemType itemType = ItemType.None;
        [SerializeField] private string displayName = "vật phẩm";
        [SerializeField] private PickupVisualType visualType = PickupVisualType.Part;
        [SerializeField] private bool snapToGroundWhenRebuilding = true;
        [SerializeField] private bool bobAndSpin = true;
        [SerializeField] private GameObject visualPrefabOverride;
        [SerializeField] private string externalVisualAssetId = string.Empty;

        public ItemType ItemType
        {
            get => itemType;
            set => itemType = value;
        }

        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        public PickupVisualType VisualType
        {
            get => visualType;
            set => visualType = value;
        }

        public bool SnapToGroundWhenRebuilding
        {
            get => snapToGroundWhenRebuilding;
            set => snapToGroundWhenRebuilding = value;
        }

        public bool BobAndSpin
        {
            get => bobAndSpin;
            set => bobAndSpin = value;
        }

        public GameObject VisualPrefabOverride
        {
            get => visualPrefabOverride;
            set => visualPrefabOverride = value;
        }

        public string ExternalVisualAssetId
        {
            get => externalVisualAssetId;
            set => externalVisualAssetId = value ?? string.Empty;
        }

        public string ResolvedDisplayName => string.IsNullOrWhiteSpace(displayName) ? LevelFactory.GetDefaultItemDisplayName(itemType) : displayName.Trim();

        public void RefreshSpawnedPickup(bool persistentAssets)
        {
            LevelFactory.RebuildPickupFromSpawnPoint(this, persistentAssets);
        }

        public void ClearSpawnedPickup()
        {
            Transform existing = transform.Find("SpawnedPickup");
            if (existing != null)
            {
                UnityCompatibility.DestroyObject(existing.gameObject);
            }
        }

        public bool TryGetSpawnedPickup(out PickupInteractable pickup)
        {
            pickup = null;
            Transform existing = transform.Find("SpawnedPickup");
            if (existing == null)
            {
                return false;
            }

            pickup = existing.GetComponent<PickupInteractable>();
            return pickup != null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            gameObject.name = itemType == ItemType.None ? "ItemSpawnPoint" : itemType + "SpawnPoint";
            if (visualType == PickupVisualType.Part && string.IsNullOrWhiteSpace(displayName))
            {
                displayName = LevelFactory.GetDefaultItemDisplayName(itemType);
            }
        }
#endif
    }
}
