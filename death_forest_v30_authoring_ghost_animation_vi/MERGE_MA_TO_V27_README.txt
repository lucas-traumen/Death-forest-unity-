Ban merged nay da duoc them model ma.fbx vao:
Assets/Resources/ExternalAssets/ma.fbx

Dong thoi them editor bootstrap:
Assets/Scripts/Editor/ExternalGhostMaBootstrap.cs

Khi mo project bang Unity, script se tu dong:
1. Tao prefab Assets/Resources/ExternalAssets/DF_External_Ghost_Ma.prefab neu chua co
2. Xoa node Camera/Light thua trong model
3. Gan ExternalAssetPrefabAdapter voi role Ghost
4. Rebuild Assets/Resources/Generated/ExternalAssetCatalog.json

Neu Unity chua tao prefab tu dong, vao menu:
Death Forest > External Assets > Ensure Ghost Ma Prefab

Luu y: Ban merge nay chi them model ghost Ma vao v27. Neu ban dang bi ket menu, van can mo scene dung va kiem tra GameManager/scene authoring rieng.
