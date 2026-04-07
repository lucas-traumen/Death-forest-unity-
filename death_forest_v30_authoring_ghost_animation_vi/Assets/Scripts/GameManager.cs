using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HollowManor
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action StateChanged;

        public ObjectiveStage Stage { get; private set; } = ObjectiveStage.FindCarParts;
        public bool HasCarBattery { get; private set; }
        public bool HasFanBelt { get; private set; }
        public bool HasSparkPlugKit { get; private set; }
        public bool HasSpareWheel { get; private set; }
        public int NotesCollected { get; private set; }
        public int NotesFoundTarget => 4;
        public bool CarRepaired { get; private set; }
        public bool IsEnded => Stage == ObjectiveStage.Win || Stage == ObjectiveStage.Lose;
        public bool IntroActive { get; private set; } = true;
        public bool EscapeSequenceActive { get; private set; }

        public int CarPartsCollected
        {
            get
            {
                int total = 0;
                if (HasCarBattery) total++;
                if (HasFanBelt) total++;
                if (HasSparkPlugKit) total++;
                if (HasSpareWheel) total++;
                return total;
            }
        }

        public int CarPartsRequired => 4;
        public bool HasAllCarParts => CarPartsCollected >= CarPartsRequired;

        // Legacy compatibility fields kept so old scripts still compile.
        public bool HasBlueFuse => false;
        public bool PowerRestored => CarRepaired;
        public int EvidenceCollected => NotesCollected;
        public int EvidenceRequired => NotesFoundTarget;
        public bool HasRedKeycard => false;
        public bool HasWardenSeal => false;
        public bool HasConfessionTape => false;
        public bool ArchiveUnlocked => CarRepaired;

        public PlayerMotor Player { get; private set; }
        public HUDController Hud { get; private set; }
        public float ThreatLevel { get; private set; }
        public string ThreatText { get; private set; } = "YÊN LẶNG";
        public float IntroTimer { get; private set; } = 999f;
        public string CurrentAreaName { get; private set; } = "XE BỊ NẠN";
        public string LoseReason { get; private set; } = string.Empty;

        public bool HasActiveNoiseEvent => activeNoiseEventTimer > 0f;
        public int ActiveNoiseEventId { get; private set; }
        public Vector3 ActiveNoiseEventPosition { get; private set; }
        public float ActiveNoiseEventRadius { get; private set; }
        public string ActiveNoiseEventLabel { get; private set; } = string.Empty;

        private string toastMessage = string.Empty;
        private float toastTimer;
        private float activeNoiseEventTimer;
        private float reportedThreat;
        private float doorNoisePressure;
        private float sceneReloadTimer = -1f;
        private bool sceneReloadQueued;
        private string reportedThreatText = string.Empty;
        private ObjectiveStage announcedStage = (ObjectiveStage)(-1);
        private string lastAreaName = string.Empty;
        private readonly HashSet<string> visitedAreas = new HashSet<string>();
        private readonly List<string> collectedLore = new List<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            EnsureSceneBindings();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (Player == null || Hud == null)
            {
                EnsureSceneBindings();
            }

            if (IsEnded)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    ReloadCurrentScene();
                    return;
                }

                if (Stage == ObjectiveStage.Lose)
                {
                    if (sceneReloadTimer < 0f)
                    {
                        sceneReloadTimer = 3.8f;
                    }
                    else
                    {
                        sceneReloadTimer -= Time.deltaTime;
                        if (sceneReloadTimer <= 0f)
                        {
                            ReloadCurrentScene();
                            return;
                        }
                    }
                }
                else
                {
                    sceneReloadTimer = -1f;
                }
            }
            else
            {
                sceneReloadTimer = -1f;
            }

            if (IntroActive)
            {
                IntroTimer = 999f;
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    BeginRun();
                }
            }
            else if (IntroTimer > 0f)
            {
                IntroTimer = Mathf.Max(0f, IntroTimer - Time.deltaTime);
            }

            if (toastTimer > 0f)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0f)
                {
                    toastMessage = string.Empty;
                    NotifyStateChanged();
                }
            }

            if (activeNoiseEventTimer > 0f)
            {
                activeNoiseEventTimer = Mathf.Max(0f, activeNoiseEventTimer - Time.deltaTime);
                if (activeNoiseEventTimer <= 0f)
                {
                    ActiveNoiseEventRadius = 0f;
                    ActiveNoiseEventLabel = string.Empty;
                }
            }

            doorNoisePressure = Mathf.MoveTowards(doorNoisePressure, 0f, Time.deltaTime * 0.28f);

            if (!IntroActive)
            {
                UpdateAreaTracking();
                AnnounceStageIfNeeded();
            }
        }

        private void LateUpdate()
        {
            if (IsEnded || IntroActive || EscapeSequenceActive)
            {
                ThreatLevel = Mathf.MoveTowards(ThreatLevel, 0f, Time.deltaTime * 2f);
                reportedThreat = 0f;
                reportedThreatText = string.Empty;
                return;
            }

            ThreatLevel = Mathf.MoveTowards(ThreatLevel, reportedThreat, Time.deltaTime * 3.4f);
            float passiveDecay = ThreatLevel > 0.55f ? 0.05f : 0.09f;
            ThreatLevel = Mathf.Max(ThreatLevel - Time.deltaTime * passiveDecay, reportedThreat);
            ThreatText = string.IsNullOrEmpty(reportedThreatText) ? "YÊN LẶNG" : reportedThreatText;
            reportedThreat = 0f;
            reportedThreatText = string.Empty;
        }

        public void BindPlayer(PlayerMotor player)
        {
            Player = player;
            NotifyStateChanged();
        }

        public void BindHud(HUDController hud)
        {
            Hud = hud;
            if (Hud != null)
            {
                Hud.RebindSceneReferences();
            }
            NotifyStateChanged();
        }

        public void EnsureSceneBindings()
        {
            if (Hud == null)
            {
                BindHud(FindSceneObject<HUDController>());
            }
            else
            {
                Hud.RebindSceneReferences();
            }

            if (Player == null)
            {
                BindPlayer(FindSceneObject<PlayerMotor>());
            }
        }

        public void BeginRun()
        {
            if (!IntroActive)
            {
                return;
            }

            IntroActive = false;
            IntroTimer = 0.85f;
            sceneReloadTimer = -1f;
            ShowToast("Tìm 4 linh kiện, bấm F để bật đèn pin. Chạy sẽ tốn thể lực và dễ bị lần theo qua từng tiếng chân.", 3.8f);
            if (Player != null)
            {
                Player.LockGameplayCursor();
            }
            NotifyStateChanged();
        }

        public void RegisterPickup(ItemType itemType, string displayName)
        {
            switch (itemType)
            {
                case ItemType.CarBattery:
                    if (!HasCarBattery)
                    {
                        HasCarBattery = true;
                        collectedLore.Add("Ắc quy vẫn còn ấm. Có người đã tháo nó ra rồi kéo vào khu rừng.");
                        ShowToast("Đã nhặt: ắc quy", 2.2f);
                    }
                    break;
                case ItemType.FanBelt:
                    if (!HasFanBelt)
                    {
                        HasFanBelt = true;
                        collectedLore.Add("Dây curoa dính bùn nước. Dấu vết kéo lê dẫn về một miệng thở không có trên bản đồ.");
                        ShowToast("Đã nhặt: dây curoa", 2.2f);
                    }
                    break;
                case ItemType.SparkPlugKit:
                    if (!HasSparkPlugKit)
                    {
                        HasSparkPlugKit = true;
                        collectedLore.Add("Bộ bugi được đặt gọn trong chòi cũ, như thể có ai đó muốn bạn phải quay lại đây thêm một lần nữa.");
                        ShowToast("Đã nhặt: bộ bugi", 2.2f);
                    }
                    break;
                case ItemType.SpareWheel:
                    if (!HasSpareWheel)
                    {
                        HasSpareWheel = true;
                        collectedLore.Add("Bánh dự phòng kéo theo dấu tay máu cũ. Chủ nhật ký cũ cũng mất ở đây.");
                        ShowToast("Đã nhặt: bánh dự phòng", 2.2f);
                    }
                    break;
                case ItemType.NotePage:
                case ItemType.Evidence:
                    NotesCollected = Mathf.Clamp(NotesCollected + 1, 0, NotesFoundTarget);
                    collectedLore.Add(GetLoreForNote(NotesCollected));
                    ShowToast("Đã nhặt: trang nhật ký " + NotesCollected + "/" + NotesFoundTarget, 2.4f);
                    break;
                default:
                    ShowToast("Đã lấy " + displayName + ".", 2.0f);
                    break;
            }

            EvaluateStage();
        }

        public void RepairCar()
        {
            if (CarRepaired)
            {
                ShowToast("Xe đã được ráp lại.");
                return;
            }

            if (!HasAllCarParts)
            {
                ShowToast("Bạn chưa đủ linh kiện để sửa xe.");
                return;
            }

            CarRepaired = true;
            ShowToast("Tiếng kim loại vang lên giữa rừng. Thứ đó chắc chắn đã nghe thấy.", 4.0f);
            EmitNoise(new Vector3(-20f, 0f, -41f), 18f, 1.2f, "sua xe");
            EvaluateStage();
        }

        public bool CanRepairCar(out string reason)
        {
            if (CarRepaired)
            {
                reason = "Xe đã sẵn sàng.";
                return true;
            }

            List<string> missing = new List<string>();
            if (!HasCarBattery) missing.Add("ắc quy");
            if (!HasFanBelt) missing.Add("dây curoa");
            if (!HasSparkPlugKit) missing.Add("bộ bugi");
            if (!HasSpareWheel) missing.Add("bánh dự phòng");

            if (missing.Count > 0)
            {
                reason = "Thiếu: " + string.Join(", ", missing) + ".";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public void RestorePower() => RepairCar();
        public bool CanUnlockArchive(out string reason) => CanRepairCar(out reason);
        public void UnlockArchive() => RepairCar();

        public void EmitNoise(Vector3 position, float radius, float duration, string label = "")
        {
            if (IsEnded || IntroActive || radius <= 0f || duration <= 0f)
            {
                return;
            }

            string normalizedLabel = (label ?? string.Empty).ToLowerInvariant();
            if (normalizedLabel.Contains("cua"))
            {
                doorNoisePressure = Mathf.Clamp01(doorNoisePressure + 0.34f);
                float pressureScale = Mathf.Lerp(1.12f, 1.70f, doorNoisePressure);
                radius *= pressureScale;
                duration *= Mathf.Lerp(1.05f, 1.42f, doorNoisePressure);
            }
            else if (normalizedLabel.Contains("sua xe") || normalizedLabel.Contains("dong co"))
            {
                radius *= 1.08f;
                duration *= 1.10f;
            }

            ActiveNoiseEventId++;
            ActiveNoiseEventPosition = position;
            ActiveNoiseEventRadius = radius;
            ActiveNoiseEventLabel = label ?? string.Empty;
            activeNoiseEventTimer = duration;
        }

        public void PlayerCaught(string captorName = "Hồn ma")
        {
            if (IsEnded || EscapeSequenceActive)
            {
                return;
            }

            if (Player != null)
            {
                LevelFactory.RecordRecentDangerPosition(Player.transform.position);
            }

            Stage = ObjectiveStage.Lose;
            ThreatLevel = 1f;
            ThreatText = "BỊ NUỐT MẤT";
            LoseReason = string.IsNullOrWhiteSpace(captorName)
                ? "Bạn đã bị một linh hồn trong rừng bắt kịp."
                : captorName + " đã bắt kịp bạn giữa rừng.";
            ShowToast(LoseReason + "\nGame sẽ tự respawn ngẫu nhiên sau vài giây. Bạn vẫn có thể nhấn R để chơi lại ngay.", 999f);
            if (Player != null)
            {
                Player.OnCaught();
            }

            NotifyStateChanged();
        }

        public void PlayerEscaped()
        {
            if (IsEnded)
            {
                return;
            }

            EscapeSequenceActive = false;
            Stage = ObjectiveStage.Win;
            ThreatLevel = 0f;
            ThreatText = "THOÁT KHỎI RỪNG";
            ShowToast("Bạn đã lao lên xe, nổ máy và thoát khỏi khu rừng. Nhấn R để chơi lại.", 999f);
            if (Player != null)
            {
                Player.SetInputBlocked(true);
            }

            NotifyStateChanged();
        }

        public void BeginCarEscapeSequence(EndingSequencePoints points)
        {
            if (points == null)
            {
                return;
            }

            if (IsEnded || EscapeSequenceActive)
            {
                return;
            }

            StartCoroutine(CarEscapeSequenceRoutine(points));
        }

        public void BeginCarEscapeSequence(Vector3 doorPosition, Vector3 seatPosition, Quaternion seatRotation)
        {
            if (IsEnded || EscapeSequenceActive)
            {
                return;
            }

            StartCoroutine(CarEscapeSequenceRoutine(doorPosition, seatPosition, seatRotation));
        }

        private IEnumerator CarEscapeSequenceRoutine(Vector3 doorPosition, Vector3 seatPosition, Quaternion seatRotation)
        {
            EscapeSequenceActive = true;
            ThreatLevel = 0f;
            ThreatText = "NỔ MÁY";
            ShowToast("Bạn lao về phía cửa xe...", 1.1f);
            NotifyStateChanged();

            if (Player != null)
            {
                yield return StartCoroutine(Player.PlayCarEscapeOutro(doorPosition, seatPosition, seatRotation));
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }

            EmitNoise(seatPosition, 20f, 1.8f, "dong co xe");
            ShowToast("Động cơ gào lên. Bạn bám chặt cửa xe và lao khỏi khu rừng...", 1.8f);
            NotifyStateChanged();
            yield return new WaitForSeconds(1.35f);

            PlayerEscaped();
        }

        private IEnumerator CarEscapeSequenceRoutine(EndingSequencePoints points)
        {
            EscapeSequenceActive = true;
            ThreatLevel = 0f;
            ThreatText = "NỔ MÁY";
            ShowToast("Bạn lao về phía cửa xe...", 1.1f);
            NotifyStateChanged();

            Vector3 doorPosition = points.DoorApproachPosition;
            Vector3 seatPosition = points.SeatPosition;
            Quaternion seatRotation = points.SeatRotation;

            if (Player != null)
            {
                yield return StartCoroutine(Player.PlayCarEscapeOutro(doorPosition, seatPosition, seatRotation));
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }

            EmitNoise(seatPosition, 20f, 1.8f, "dong co xe");
            ShowToast("Động cơ gào lên. Bạn bám chặt cửa xe và lao khỏi khu rừng...", 1.8f);
            NotifyStateChanged();

            if (points.MoveVehicleDuringOutro)
            {
                yield return StartCoroutine(PlayVehicleDriveAwayOutro(points));
            }
            else
            {
                yield return new WaitForSeconds(1.35f);
            }

            PlayerEscaped();
        }

        private IEnumerator PlayVehicleDriveAwayOutro(EndingSequencePoints points)
        {
            if (points == null)
            {
                yield return new WaitForSeconds(1.35f);
                yield break;
            }

            Transform vehicle = points.VehicleRoot;
            Transform exitEnd = points.ExitPathEnd;
            if (vehicle == null || exitEnd == null)
            {
                yield return new WaitForSeconds(1.35f);
                yield break;
            }

            if (points.CarAlignPoint != null)
            {
                vehicle.position = points.CarAlignPoint.position;
                vehicle.rotation = points.CarAlignPoint.rotation;
            }

            Vector3 start = vehicle.position;
            Vector3 end = exitEnd.position;
            Quaternion startRotation = vehicle.rotation;
            Vector3 travel = end - start;
            Quaternion endRotation = travel.sqrMagnitude > 0.001f ? Quaternion.LookRotation(new Vector3(travel.x, 0f, travel.z).normalized, Vector3.up) : startRotation;

            Transform playerTransform = Player != null ? Player.transform : null;
            Transform previousParent = playerTransform != null ? playerTransform.parent : null;
            if (playerTransform != null)
            {
                playerTransform.SetParent(vehicle, true);
            }

            float timer = 0f;
            float duration = points.DriveAwayDuration;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                float eased = Mathf.SmoothStep(0f, 1f, t);
                vehicle.position = Vector3.Lerp(start, end, eased);
                vehicle.rotation = Quaternion.Slerp(startRotation, endRotation, eased);
                yield return null;
            }

            if (playerTransform != null)
            {
                playerTransform.SetParent(previousParent, true);
            }
        }

        public void ShowToast(string message, float duration = 2.5f)
        {
            toastMessage = message;
            toastTimer = duration;
            NotifyStateChanged();
        }

        public void ReportThreat(float level, string label)
        {
            if (IsEnded || IntroActive || EscapeSequenceActive)
            {
                return;
            }

            if (level > reportedThreat)
            {
                reportedThreat = Mathf.Clamp01(level);
                reportedThreatText = label;
            }
        }

        public string GetToastMessage() => toastMessage;

        public string GetObjectiveText()
        {
            switch (Stage)
            {
                case ObjectiveStage.FindCarParts:
                    return "Tìm đủ 4 linh kiện rồi quay lại xe.";
                case ObjectiveStage.RepairCar:
                    return "Lắp linh kiện vào xe.";
                case ObjectiveStage.Escape:
                    return "Lên xe và nổ máy.";
                case ObjectiveStage.Win:
                    return "Bạn đã thoát.";
                case ObjectiveStage.Lose:
                    return "Bạn đã bị bắt. Nhấn R để chơi lại.";
                default:
                    return string.Empty;
            }
        }

        public string GetInventoryText()
        {
            return "VẬT PHẨM ĐÃ NHẶT\n" +
                   "- Ắc quy: " + (HasCarBattery ? "Có" : "Chưa") + "\n" +
                   "- Dây curoa: " + (HasFanBelt ? "Có" : "Chưa") + "\n" +
                   "- Bộ bugi: " + (HasSparkPlugKit ? "Có" : "Chưa") + "\n" +
                   "- Bánh dự phòng: " + (HasSpareWheel ? "Có" : "Chưa") + "\n" +
                   "- Nhật ký: " + NotesCollected + "/" + NotesFoundTarget;
        }

        public string GetMenuIntroText()
        {
            return "CHƠI\n\n" +
                   "Bạn vừa tỉnh lại sau một vụ lao xe xuống đồi bùn. Chiếc xe không nổ máy, 4 linh kiện quan trọng bị văng khỏi thân xe và rơi rải rác quanh những điểm người từng dừng chân trong rừng. Thứ đang đi theo bạn không cần thấy mặt - nó lần đến theo từng tiếng động lớn nhất.\n\n" +
                   "- CHƠI: bắt đầu đêm trốn khỏi khu rừng\n" +
                   "- HƯỚNG DẪN: xem điều khiển, mức tiếng ồn và mẹo nấp\n" +
                   "- CỐT TRUYỆN: đọc đoạn mở đầu và lý do khu rừng này không bình thường\n" +
                   "- THOÁT: thoát game hoặc dừng Play Mode trong Editor\n\n" +
                   "Mục tiêu: nhặt đủ 4 linh kiện, quay lại xe, lắp lại và nổ máy trước khi nó tới nơi có tiếng động của bạn.";
        }

        public string GetHowToText()
        {
            return "HƯỚNG DẪN\n\n" +
                   "WASD - di chuyển\n" +
                   "Shift - chạy nhanh, nhanh hơn nhưng để lại dấu vết âm thanh lớn\n" +
                   "Ctrl/C - đi khom, chậm hơn nhưng khó bị nghe thấy hơn\n" +
                   "F - bật/tắt đèn pin\n" +
                   "E - mở cửa, nhặt đồ, trốn, sửa xe, chui vào thân gỗ / tủ gỗ / túp lều đổ\n" +
                   "Esc - thả chuột khi đang chơi\n\n" +
                   "MẸO SỐNG SÓT\n" +
                   "- Mở cửa là mức tiếng ồn dễ bị lần theo nhất. Mở/đóng liên tục sẽ làm độ ồn của cửa tăng dần. Tiếp theo là chạy và đi trên cỏ rậm, sàn gỗ; xuống nước là nhỏ nhất\n" +
                   "- Chạy liên tục giúp bạn kéo giãn khoảng cách, nhưng cũng khiến nó dễ bắt hướng hơn. Dùng lên tốc khi cần cắt đuôi, rồi bẻ góc hoặc nấp sớm\n" +
                   "- Nếu đang bị dí mà bạn chỉ đổi sang đi khom, cảm giác nguy hiểm vẫn còn trong vài giây; đừng coi là đã thoát ngay\n" +
                   "- Khi nấp, hãy im trong 5-10 giây: có 80 phần trăm khả năng nó bỏ qua. Nếu đã bỏ qua thì nó sẽ rút lui và không bắt xuyên tủ/chỗ nấp\n" +
                   "- Game sẽ tự respawn ngẫu nhiên sau khi bị bắt, nên vị trí bạn và ma sẽ không giống nhau mỗi lần\n" +
                   "- Thân gỗ rỗng, tủ gỗ và túp lều đổ cho phép nhìn xoay xung quanh để dò thính hướng.";
        }

        public string GetLoreText()
        {
            return "CỐT TRUYỆN\n\n" +
                   "Bạn đi đường tắt qua con đường kiểm lâm cũ để kịp về trước mưa, nhưng một thân cây đổ ngang đường đã ép xe lao lệch xuống đồi bùn. Chiếc xe không vỡ nát, nhưng ắc quy rơi ra, dây curoa đứt, bộ bugi văng khỏi hộp đồ và bánh sau bị bật mất. Dấu vết và vật dụng quanh các điểm dừng chân cho thấy linh kiện đã bị va đập và kéo đi quanh rừng.\n\n" +
                   "Nơi này từng là loạt điểm nghỉ của kiểm lâm và người đi rừng: chòi gác, túp lều đổ, thân gỗ rỗng và cạnh suối. Nhưng sau nhiều vụ mất tích, ai cũng chỉ nhắc đến Death Forest bằng một quy tắc: đừng gây tiếng động lớn nếu vẫn muốn thấy đường trở ra.\n\n" +
                   "Thứ đang săn bạn không cần phải thấy bạn liên tục. Nó chỉ cần âm thanh: cửa bật mở, bước chân trên gỗ, tiếng đồ sửa xe, và cuối cùng là tiếng động cơ. Những trang nhật ký bỏ lại sẽ nói rõ hơn vì sao khu rừng này chỉ im lặng trước khi một ai đó biến mất.";
        }

        public string GetStoryText()
        {
            return GetHowToText() + "\n\n" + GetLoreText();
        }

        public string GetEndingTitle() => Stage == ObjectiveStage.Win ? "THOÁT KHỎI RỪNG" : "BỊ BẮT";

        public string GetEndingBody()
        {
            if (Stage == ObjectiveStage.Win)
            {
                string loreLine = collectedLore.Count > 0 ? "\n\nManh moi cuoi: " + collectedLore[collectedLore.Count - 1] : string.Empty;
                return "Bạn đã lắp đủ 4 linh kiện, nổ máy và rẽ khỏi con đường tắt trước khi hồn ma kịp kéo tới." + loreLine + "\n\nNhan R de choi lai.";
            }

            if (Stage == ObjectiveStage.Lose)
            {
                return "Tieng thet cuoi cung den sat ben tai truoc khi ban kip ve toi xe.\nLan sau: di khom nhieu hon, dong cua nhanh va nap som khi nghe tieng rit lon dan.\n\nNhan R de choi lai.";
            }

            return string.Empty;
        }

        private void EvaluateStage()
        {
            if (IsEnded)
            {
                return;
            }

            if (!HasAllCarParts)
            {
                Stage = ObjectiveStage.FindCarParts;
            }
            else if (!CarRepaired)
            {
                Stage = ObjectiveStage.RepairCar;
            }
            else
            {
                Stage = ObjectiveStage.Escape;
            }

            if (Player != null)
            {
                Player.SetInputBlocked(IsEnded || IntroActive || EscapeSequenceActive);
            }

            NotifyStateChanged();
        }

        private void UpdateAreaTracking()
        {
            if (Player == null)
            {
                return;
            }

            string area = ResolveAreaName(Player.transform.position);
            CurrentAreaName = area;
            if (area != lastAreaName)
            {
                lastAreaName = area;
                if (visitedAreas.Add(area) && !IsEnded)
                {
                    // Polished v5: keep traversal guidance diegetic through audio instead of frequent on-screen alerts.
                }
                NotifyStateChanged();
            }
        }

        private string ResolveAreaName(Vector3 position)
        {
            if (position.z <= -26f)
            {
                return "XE BỊ NẠN";
            }

            if (position.x <= -18f && position.z >= 14f)
            {
                return "MIẾU CŨ";
            }

            if (position.x >= 20f && position.z >= 12f)
            {
                return "KHE SUỐI";
            }

            if (position.x <= -20f)
            {
                return "CHÒI KIỂM LÂM";
            }

            if (position.x >= 12f)
            {
                return "TRẠI BỎ HOANG";
            }

            return "LỐI MÒN TỐI";
        }

        private static string ResolveAreaHint(string area)
        {
            return string.Empty;
        }

        private void AnnounceStageIfNeeded()
        {
            if (announcedStage == Stage || IsEnded || IntroActive || EscapeSequenceActive)
            {
                return;
            }

            announcedStage = Stage;
            switch (Stage)
            {
                case ObjectiveStage.FindCarParts:
                    ShowToast("Tìm 4 linh kiện xe bị thất lạc trong rừng.", 3.8f);
                    break;
                case ObjectiveStage.RepairCar:
                    ShowToast("Đã đủ linh kiện. Quay lại xe và sửa nhanh.", 3.4f);
                    break;
                case ObjectiveStage.Escape:
                    ShowToast("Lên xe và nổ máy để thoát.", 3.0f);
                    break;
            }
        }

        private static string GetLoreForNote(int index)
        {
            switch (index)
            {
                case 1:
                    return "Trang 1: 'Nếu nghe tiếng trẻ con cười trong rừng, đừng quay lại.'";
                case 2:
                    return "Trang 2: 'Tôi đã trốn trong tủ gỗ và nghe nó đứng trước mặt cánh cửa rất lâu.'";
                case 3:
                    return "Trang 3: 'Nó không một mình. Khu rừng sẽ đưa hình của nó đến sát mặt bạn để làm bạn bỏ chạy.'";
                case 4:
                    return "Trang 4: 'Nếu mở cửa mà thấy bóng nó đứng bên ngoài, đừng lao ra ngay. Đợi cho nó tan rồi mới đi.'";
                default:
                    return "Mảnh giấy ẩm ướt, chữ viết dở dang.";
            }
        }

        private void ReloadCurrentScene()
        {
            if (sceneReloadQueued)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || string.IsNullOrEmpty(activeScene.name))
            {
                Debug.LogWarning("[Death Forest] Không thể reload scene hiện tại vì scene chưa được lưu thành asset.");
                sceneReloadTimer = -1f;
                return;
            }

            sceneReloadQueued = true;
            SceneManager.LoadScene(activeScene.name);
        }

        private static T FindSceneObject<T>() where T : Component
        {
            T[] allObjects = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < allObjects.Length; i++)
            {
                T candidate = allObjects[i];
                if (candidate == null)
                {
                    continue;
                }

                if (!candidate.gameObject.scene.IsValid())
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
