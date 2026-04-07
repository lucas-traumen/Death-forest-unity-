DEATH FOREST V29 - STABLE GAMEPLAY AUTHORING
===========================================

Muc tieu ban nay:
- Giam viec sua code moi khi them asset / doi vi tri vat pham / doi diem ending
- Chuyen gameplay point quan trong sang scene-authored
- Giu asset pipeline tach khoi gameplay logic de project on dinh hon

NHUNG GI DA DOI
---------------
1. Item pickup khong con dat truc tiep bang hard-code trong LevelFactory nua.
   - Moi diem dat vat pham gio la 1 ItemSpawnPoint trong scene.
   - Moi spawn point giu itemType, displayName, visualType.
   - Co the gan visual prefab rieng hoac asset id tu External Asset Catalog.
   - Them asset moi khong can sua code pickup.

2. Ending / escape da co cum point authored rieng.
   - EndingSequencePoints giu:
     + InteractPoint
     + DoorApproachPoint
     + SeatPoint
     + SeatFacingPoint
     + CarAlignPoint
     + ExitPathStart
     + ExitPathEnd
     + EndingCameraPoint
     + GhostRevealPoint
   - CarEscapeInteractable uu tien doc diem ending tu scene thay vi offset hard-code.

3. Scene authoring tools moi.
   - Death Forest/Gameplay Authoring/Refresh All Pickup Spawn Points
   - Death Forest/Gameplay Authoring/Refresh Selected Pickup Spawn Point
   - Death Forest/Gameplay Authoring/Rebuild Ending Point Anchors

4. BootstrapController legacy se tu tat neu scene authored da co DeathForestSceneRoot / GameManager.
   - Giam kha nang scene bi build chong len nhau.

5. External asset catalog builder infer role tu ca folder path + file name.
   - Dat prefab vao folder hop ly duoi Assets/Resources/ExternalAssets se de phan loai hon.

CACH DUNG DE ON DINH NHAT
-------------------------
1. Mo project.
2. Chay menu:
   Death Forest/Scene Authoring/Create Or Refresh And Save Default Scene
3. Chay tiep:
   Death Forest/Gameplay Authoring/Refresh All Pickup Spawn Points
4. Chay tiep:
   Death Forest/Gameplay Authoring/Rebuild Ending Point Anchors
5. Ctrl+S de luu scene.

THEM ASSET MOI MA KHONG SUA CODE
--------------------------------
A. Them prop / environment asset
- Import prefab vao Assets/Resources/ExternalAssets/
- Chay Death Forest/Rebuild External Asset Catalog
- Mo Scene Palette hoac keo prefab vao SceneExternalAssets

B. Them visual moi cho pickup
- Import prefab item vao project
- Chon ItemSpawnPoint can doi
- Gan Visual Prefab Override hoac External Visual Asset Id
- Chay Refresh Selected Pickup Spawn Point

C. Doi vi tri vat pham
- Keo ItemSpawnPoint trong scene
- Refresh lai spawn point
- Save scene

D. Doi diem ending
- Chon EndingSequenceRoot
- Di chuyen cac child point trong scene
- Save scene

KET QUA KIEN TRUC
-----------------
- Them asset moi chu yeu la thao tac Scene / Inspector / Catalog
- Giam phu thuoc vao toa do hard-code
- Giam viec sua LevelFactory cho moi lan bo sung content
- Phu hop hon de mo rong map, item va outro sau nay
