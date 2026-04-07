Death Forest v26 - Editor safe destroy + material/shader fix

Da sua cac van de chinh sau:
1. Khong con goi Destroy(...) trong Edit Mode o cac doan scene authoring/build map.
   - Thay bang UnityCompatibility.DestroyObject(...)
   - Tu dong dung DestroyImmediate khi dang o Editor va chua Play.

2. Khong con sua mau bang renderer.material trong cac duong di co the cham den Editor.
   - Dung sharedMaterial trong Edit Mode.
   - Co helper SetRendererColor de tranh tao material instance bi leak vao scene.

3. Them bo chon shader tuong thich:
   - uu tien Universal Render Pipeline/Lit
   - sau do Standard
   - sau do Lit / Legacy / Sprites fallback
   Muc tieu la giam tinh trang vat the mau hong do shader khong hop pipeline.

4. Demo external asset materials se duoc repair shader/cau hinh khi rebuild.

File chinh da sua:
- Assets/Scripts/UnityCompatibility.cs
- Assets/Scripts/LevelFactory.cs
- Assets/Scripts/ExternalAssetPrefabAdapter.cs
- Assets/Scripts/FuseBoxInteractable.cs
- Assets/Scripts/PatrolEnemy.cs
- Assets/Scripts/PowerControlledLight.cs
- Assets/Scripts/JumpScareDirector.cs
- Assets/Scripts/Editor/ExternalAssetImportTools.cs

Nen lam sau khi mo project:
- Death Forest/External Assets/Rebuild Demo Pack
- Death Forest/Scene Authoring/Create Or Refresh Current Scene
- Neu van con mau hong o asset cu: chon material/prefab do va reimport, hoac rebuild lai scene authored.
