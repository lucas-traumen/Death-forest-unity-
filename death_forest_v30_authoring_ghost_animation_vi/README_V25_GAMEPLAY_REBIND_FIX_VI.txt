Death Forest v25 - Gameplay Rebind Fix

Ban nay sua dung loi scene-authoring-first nhung runtime binding chua tu noi lai.

Da sua:
- GameManager tu dong tim va bind lai Player + HUD khi vao Play.
- HUDController tu dong quet lai toan bo child UI trong scene authored.
- Nút PLAY / HOW TO / STORY / QUIT / BACK duoc gan lai listener luc vao Play.
- PLAY se khoa chuot va vao gameplay ngay, khong can click them de bat camera.
- Khi thua game, phim R va auto respawn se reload lai scene authored hien tai.
- DeathForestSceneRoot tu tim lai cac root World / Props / Interactables / Ghosts / Lighting / SceneExternalAssets neu reference bi rong.

Muc tieu cua ban nay:
- mo scene la thay map
- nhan Play la choi duoc
- khong phu thuoc vao runtime-generated listener tam thoi nua

Neu scene hien tai chua duoc luu thanh asset .unity thi tinh nang reload khi thua se can scene duoc save truoc.
