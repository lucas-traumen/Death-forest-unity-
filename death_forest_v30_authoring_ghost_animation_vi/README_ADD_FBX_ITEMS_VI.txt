THEM ITEM FBX VAO GAME

Nen dung FBX.
- Unity import FBX on dinh hon.
- File .blend chi nen dung neu may mo project co cai Blender va Unity nhan duoc Blender local.

Cach them item moi:
1. Chep file .fbx vao Assets/Resources/ExternalAssets/Items/
2. Chep texture vao Assets/Resources/ExternalAssets/Items/Textures/
3. Mo script Assets/Scripts/ExternalItemVisualLibrary.cs
4. Them mot mapping moi trong PickupSpecs:
   - model path, texture path, rotation, target size, offset, mau fallback
5. Trong LevelFactory.cs, doi item can dung model moi sang ItemType tuong ung.

Ban patch nay da ho tro san cac file:
- battery.fbx
- v_belt.fbx
- spark_plug.fbx
- tire_wheel.fbx
- jerry_can.fbx

Gameplay moi:
- Shift ton the luc
- Space de nhay
- F de bat/tat den pin
- Ma lan theo diem tieng dong vua phat ra
- Nghe radio static khi ma den gan
