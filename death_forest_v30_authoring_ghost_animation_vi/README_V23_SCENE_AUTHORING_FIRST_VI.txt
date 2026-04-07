DEATH FOREST V23 - SCENE AUTHORING FIRST
=======================================

Ban nay da doi cau truc tu runtime-generated sang scene-authoring-first:

1. Khong can bam Play moi thay map nua.
2. Khi mo project / scene rong, editor se tu tao scene Death Forest authored neu scene hien tai chi la scene mac dinh rong.
3. RuntimeBootstrap khong con procedural-build map nua.
4. Map, player, ghost, lighting, landmark duoc dat san trong Scene/Hierarchy.
5. Asset ngoai duoc them truc tiep trong Scene, khong can RuntimeObjectEditor.

MENU CHINH
----------
- Death Forest/Scene Authoring/Create Or Refresh Current Scene
- Death Forest/Scene Authoring/Create Or Refresh And Save Default Scene
- Death Forest/External Assets/Import Folder Into Catalog
- Death Forest/External Assets/Open Scene Palette
- Death Forest/Rebuild External Asset Catalog

QUY TRINH DUNG DUNG
-------------------
A. Tao / lam moi scene authored
- Chay: Death Forest/Scene Authoring/Create Or Refresh Current Scene
- Neu muon save thanh scene mac dinh trong Build Settings:
  Death Forest/Scene Authoring/Create Or Refresh And Save Default Scene

B. Them asset ngoai vao project
- Chay: Death Forest/External Assets/Import Folder Into Catalog
- Sau do mo: Death Forest/External Assets/Open Scene Palette
- Dat prefab vao Scene tai Scene pivot hoac duoi object dang chon
- Hoac keo-tha prefab truc tiep tu Project vao Hierarchy nhu Unity thong thuong

C. Landmark
- Landmark authored mac dinh duoc bootstrap thanh prefab trong Resources/Generated/Landmarks
- Scene se dung authored landmark prefab thay vi runtime procedural fallback

GHI CHU
-------
- RuntimeObjectEditor van con trong code cho file cu, nhung scene authored moi KHONG gan component nay nua.
- Neu muon sua map, sua truc tiep trong Scene/Hierarchy/Prefab mode.
- Root chinh la DeathForestSceneRoot, co child SceneExternalAssets de chua asset ngoai dat trong scene.
