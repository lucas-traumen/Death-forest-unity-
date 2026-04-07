CAP NHAT V22 - THEM THU NGHIEM ASSET NGOAI VAO PROJECT
=====================================================

Muc tieu
- Khong can fallback procedural nua.
- Van co the mo project va test ngay external asset pipeline.
- Co them menu import asset tu folder ben ngoai vao thang catalog runtime.

Da them
1) Demo external asset pack tu dong tao neu project chua co prefab nao trong:
   Assets/Resources/ExternalAssets

   Goi demo gom:
   - DF_External_PineTree
   - DF_External_StoneLantern
   - DF_External_Shack
   - DF_External_WreckedCar
   - DF_External_Ghost

2) Menu import asset ben ngoai:
   Death Forest/External Assets/Import Folder Into Catalog

   Cach dung:
   - Chon 1 folder asset ben ngoai project
   - Tool se copy toan bo file (bo qua .meta/.tmp) vao:
     Assets/Resources/ExternalAssets/Imported/<ten-folder>
   - Tu rebuild catalog de RuntimeObjectEditor nhin thay prefab moi

3) Menu tao lai demo pack:
   - Death Forest/External Assets/Create Demo Pack
   - Death Forest/External Assets/Rebuild Demo Pack

4) Rebuild catalog khong popup khi bootstrap/editor tool can goi ngam.

CACH TEST NHANH
- Mo project Unity
- Cho Unity import xong
- Vao Play
- Bam F2
- Trong danh sach asset se thay cac muc [EXT] demo
- Click trai de dat asset
- Bam F5 de save layout
- Neu muon bake thanh prefab that: Death Forest/Bake Runtime Layout To Prefab

NEU BAN CO MODEL NGOAI SAN
- Dat file prefab/fbx/obj/texture vao 1 folder ben ngoai
- Chay menu Import Folder Into Catalog
- Sau khi import xong, vao Play -> F2 de dat thu
