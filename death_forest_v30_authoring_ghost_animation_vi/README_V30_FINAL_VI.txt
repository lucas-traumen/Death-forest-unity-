DEATH FOREST V30 - ON DINH CO CHE, MA AUTHORING, OUTRO, TIENG VIET

Nhung thay doi chinh:
- Ma duoc doi sang huong authoring 1 khoi:
  - root ma giu AI
  - child ModelRoot giu model/animator
  - child PatrolRoute giu cac point tuan tra
- Them script GhostAuthoring.cs
- PatrolEnemy tu doc route authored va neu scene cu chua co route thi tu tao fallback route de ma van di chuyen
- PatrolEnemy co the dieu khien Animator:
  - Speed
  - IsMoving
  - State
  - Alertness
  - Capture
- ExternalAssetPrefabAdapter giu Animator cua ghost thay vi tat het nhu truoc
- EndingSequencePoints co them vehicleRoot + driveAwayDuration + moveVehicleDuringOutro
- GameManager outro thang co the cho xe chay ra khoi rung theo ExitPathEnd
- HUD/menu/prompt chuyen sang tieng Viet nhieu hon
- Them menu authoring cho ghost:
  - Death Forest/Gameplay Authoring/Refresh All Ghost Authoring
  - Death Forest/Gameplay Authoring/Refresh Selected Ghost Authoring

Cach dung khuyen nghi:
1. Mo project trong Unity
2. Chay:
   Death Forest -> Scene Authoring -> Create Or Refresh And Save Default Scene
3. Chay:
   Death Forest -> Gameplay Authoring -> Refresh All Pickup Spawn Points
4. Chay:
   Death Forest -> Gameplay Authoring -> Refresh All Ghost Authoring
5. Chay:
   Death Forest -> Gameplay Authoring -> Rebuild Ending Point Anchors
6. Save scene

Chinh ma:
- Chon root ma trong Hierarchy
- Model moi dat vao child ModelRoot
- Patrol route dat trong child PatrolRoute
- Neu model co Animator, PatrolEnemy se co gang drive param:
  Speed, IsMoving, State, Alertness, Capture
- Neu animator controller cua ban dung ten param khac, doi ten trong Inspector cua PatrolEnemy

Chinh ending:
- Chon EndingSequenceRoot
- Chinh InteractPoint, DoorApproachPoint, SeatPoint, SeatFacingPoint, CarAlignPoint, ExitPathEnd
- Neu co xe that trong scene, EndingSequencePoints se co gang tim vehicleRoot gan nhat
- Co the tu gan vehicleRoot tay trong Inspector de on dinh hon

Luu y:
- Mình khong mo duoc Unity trong moi truong nay nen van can ban compile/test trong Editor de xac nhan cuoi cung.
- Neu con loi Console, chup dong loi do dau tien de sua tiep chinh xac.
