using UnityEngine;

namespace HollowManor
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PatrolEnemy : MonoBehaviour
    {
        private enum EnemyState
        {
            Patrol,
            Investigate,
            Chase
        }

        [Header("Identity")]
        public string enemyName = "Hồn ma";

        [Header("Movement")]
        public float patrolSpeed = 1.95f;
        public float investigateSpeed = 4.1f;
        public float chaseSpeed = 7.4f;
        [Header("Sight")]
        public float viewDistance = 17.5f;
        public float viewAngle = 135f;
        public float catchDistance = 2.25f;
        [Header("Awareness")]
        public float chaseMemory = 12f;
        public float investigatePause = 0.35f;
        public float hearingChaseSignal = 0.08f;
        [Header("Capture")]
        public float captureHoldTime = 0.95f;
        [Header("Avoidance")]
        public float avoidanceProbeDistance = 4.8f;
        public float avoidanceTurnAngle = 72f;
        public float wideAvoidanceTurnAngle = 128f;
        [Header("Stuck Recovery")]
        public float stuckRecoverAfter = 0.18f;
        public float hardRecoverAfter = 0.55f;

        private CharacterController controller;
        [Header("Authoring")]
        [SerializeField] private GhostAuthoring authoring;
        [SerializeField] private Animator modelAnimator;
        [SerializeField] private string animatorSpeedParameter = "Speed";
        [SerializeField] private string animatorMovingParameter = "IsMoving";
        [SerializeField] private string animatorStateParameter = "State";
        [SerializeField] private string animatorAlertnessParameter = "Alertness";
        [SerializeField] private string animatorCaptureTrigger = "Capture";
        private Renderer bodyRenderer;
        private Light eyeLight;
        private Vector3[] waypoints = new Vector3[0];
        private int waypointIndex;
        private EnemyState state;
        private Vector3 investigatePosition;
        private Vector3 lastKnownPosition;
        private float stateTimer;
        private float awareness;
        private bool hasLastKnownPosition;
        private int lastReactedNoiseEventId = -1;
        private int heardStepCount;
        private float heardStepTimer;
        private float captureTimer;
        private Vector3 previousFlatPosition;
        private float stuckTimer;
        private int preferredAvoidSide = 1;
        private float cornerBiasTimer;
        private float directChaseRefreshTimer;
        private float chaseAfterglowTimer;
        private float animationMoveSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            ConfigureController();
        }

        private void Start()
        {
            ConfigureController();
            ResolveAuthoringReferences();

            if ((waypoints == null || waypoints.Length == 0) && authoring != null)
            {
                waypoints = authoring.BuildWaypointPositions(transform.position);
                waypointIndex = 0;
            }

            if (waypoints == null || waypoints.Length == 0)
            {
                waypoints = BuildFallbackPatrolRoute(transform.position);
                waypointIndex = 0;
            }

            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (modelAnimator == null && authoring != null)
            {
                modelAnimator = authoring.Animator;
            }

            eyeLight = GetComponentInChildren<Light>();
            previousFlatPosition = Flatten(transform.position);
            SetState(EnemyState.Patrol);
            RefreshColor();
            UpdateAnimator();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.IsEnded || GameManager.Instance.IntroActive || GameManager.Instance.EscapeSequenceActive || GameManager.Instance.Player == null)
            {
                animationMoveSpeed = 0f;
                UpdateAnimator();
                return;
            }

            if (!CanUseController())
            {
                return;
            }

            if (chaseAfterglowTimer > 0f)
            {
                chaseAfterglowTimer = Mathf.Max(0f, chaseAfterglowTimer - Time.deltaTime);
            }

            PlayerMotor player = GameManager.Instance.Player;
            bool seesPlayer = CanSeePlayer(player, out float sightFactor);
            float distanceToPlayer = Vector3.Distance(Flatten(transform.position), Flatten(player.transform.position));
            if (heardStepTimer > 0f)
            {
                heardStepTimer = Mathf.Max(0f, heardStepTimer - Time.deltaTime);
                if (heardStepTimer <= 0f)
                {
                    heardStepCount = 0;
                }
            }

            if (cornerBiasTimer > 0f)
            {
                cornerBiasTimer = Mathf.Max(0f, cornerBiasTimer - Time.deltaTime);
            }

            if (player.IsHidden)
            {
                HideSpotInteractable hideSpot = player.CurrentHideSpot;
                float reactionDistance = hideSpot != null ? hideSpot.captureCheckDistance + 1.45f : 2.8f;
                if (player.HideOutcomeSucceeded || (!seesPlayer && distanceToPlayer > reactionDistance))
                {
                    HandleHiddenPlayer();
                }
            }
            else if (seesPlayer)
            {
                awareness = Mathf.Clamp01(awareness + Time.deltaTime * Mathf.Lerp(0.70f, 1.55f, sightFactor));
                lastKnownPosition = player.transform.position;
                hasLastKnownPosition = true;
                heardStepCount = 0;
                heardStepTimer = 0f;
                BeginChase(player.transform.position);
            }
            else
            {
                awareness = Mathf.MoveTowards(awareness, 0f, Time.deltaTime * 0.18f);
                TryHearActiveNoiseEvent();
                UpdateStateTimers();
            }

            animationMoveSpeed = 0f;

            switch (state)
            {
                case EnemyState.Patrol:
                    UpdatePatrol();
                    break;
                case EnemyState.Investigate:
                    UpdateInvestigate();
                    break;
                case EnemyState.Chase:
                    UpdateChase();
                    break;
            }

            UpdateCapture(player, distanceToPlayer, seesPlayer);
            ReportThreat(player, distanceToPlayer, seesPlayer);
            UpdateAnimator();
        }

        public void AssignBodyRenderer(Renderer renderer)
        {
            bodyRenderer = renderer;
            RefreshColor();
        }

        public void AssignAnimator(Animator animator)
        {
            modelAnimator = animator;
            UpdateAnimator();
        }

        public void SetWaypoints(params Vector3[] points)
        {
            if (points == null || points.Length == 0)
            {
                waypoints = BuildFallbackPatrolRoute(transform.position);
                waypointIndex = 0;
                return;
            }

            waypoints = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                waypoints[i] = points[i];
            }
            waypointIndex = 0;
        }

        private void HandleHiddenPlayer()
        {
            awareness = Mathf.MoveTowards(awareness, 0f, Time.deltaTime * 2.0f);
            heardStepCount = 0;
            heardStepTimer = 0f;
            hasLastKnownPosition = false;
            captureTimer = 0f;
            chaseAfterglowTimer = Mathf.Max(chaseAfterglowTimer, 2.25f);

            if (state != EnemyState.Patrol)
            {
                AdvancePatrol();
                stateTimer = 0f;
                SetState(EnemyState.Patrol);
            }
        }

        private void UpdateStateTimers()
        {
            if (state != EnemyState.Chase)
            {
                return;
            }

            stateTimer -= Time.deltaTime;
            if (hasLastKnownPosition && Vector3.Distance(Flatten(transform.position), Flatten(lastKnownPosition)) <= 0.6f)
            {
                stateTimer -= Time.deltaTime * 1.1f;
            }

            if (stateTimer <= 0f)
            {
                Vector3 fallback = hasLastKnownPosition ? lastKnownPosition : transform.position;
                BeginInvestigate(fallback, investigatePause + 0.8f, 0.18f);
            }
        }

        private void SetState(EnemyState newState)
        {
            if (state == newState)
            {
                return;
            }

            state = newState;
            RefreshColor();
        }

        private void BeginInvestigate(Vector3 position, float lingerTime, float minimumAwareness)
        {
            investigatePosition = position;
            stateTimer = lingerTime;
            awareness = Mathf.Max(awareness, minimumAwareness);
            SetState(EnemyState.Investigate);
        }

        private void BeginChase(Vector3 position)
        {
            lastKnownPosition = position;
            hasLastKnownPosition = true;
            stateTimer = chaseMemory * 1.4f;
            directChaseRefreshTimer = 0f;
            chaseAfterglowTimer = Mathf.Max(chaseAfterglowTimer, 3.4f);
            SetState(EnemyState.Chase);
        }

        private void AdvancePatrol()
        {
            if (waypoints != null && waypoints.Length > 0)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
            }
        }

        private void UpdatePatrol()
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                return;
            }

            Vector3 target = waypoints[waypointIndex];
            MoveTowards(target, patrolSpeed);
            if (Vector3.Distance(Flatten(transform.position), Flatten(target)) <= 1.15f)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
                stuckTimer = 0f;
            }
        }

        private void UpdateInvestigate()
        {
            float distanceToPoint = Vector3.Distance(Flatten(transform.position), Flatten(investigatePosition));
            if (distanceToPoint > 0.45f)
            {
                MoveTowards(investigatePosition, investigateSpeed);
                return;
            }

            stateTimer -= Time.deltaTime;
            transform.Rotate(Vector3.up, 105f * Time.deltaTime, Space.World);

            if (stateTimer > 0.45f && hasLastKnownPosition && Random.value < Time.deltaTime * 0.85f)
            {
                Vector3 orbit = Quaternion.Euler(0f, Random.Range(-95f, 95f), 0f) * transform.forward;
                investigatePosition = lastKnownPosition + Flatten(orbit).normalized * Random.Range(1.2f, 2.0f);
            }

            if (stateTimer <= 0f)
            {
                SetState(EnemyState.Patrol);
            }
        }

        private void UpdateChase()
        {
            PlayerMotor player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            Vector3 target = hasLastKnownPosition ? lastKnownPosition : transform.position;
            float currentChaseSpeed = chaseSpeed;

            if (player != null && !player.IsHidden)
            {
                lastKnownPosition = player.transform.position;
                hasLastKnownPosition = true;
                stateTimer = Mathf.Max(stateTimer, chaseMemory * 0.95f);

                float chaseDistance = Vector3.Distance(Flatten(transform.position), Flatten(player.transform.position));
                if (chaseDistance > 5.5f) currentChaseSpeed *= 1.08f;
                if (chaseDistance > 9.5f) currentChaseSpeed *= 1.14f;

                directChaseRefreshTimer -= Time.deltaTime;
                if (directChaseRefreshTimer <= 0f)
                {
                    target = player.transform.position;
                    lastKnownPosition = target;
                    hasLastKnownPosition = true;
                    directChaseRefreshTimer = 0.01f;
                }
                else
                {
                    target = lastKnownPosition;
                }
            }

            MoveTowards(target, currentChaseSpeed);
        }

        private void MoveTowards(Vector3 target, float speed)
        {
            animationMoveSpeed = Mathf.Max(animationMoveSpeed, speed);
            Vector3 flatCurrent = Flatten(transform.position);
            Vector3 flatTarget = Flatten(target);
            Vector3 delta = flatTarget - flatCurrent;
            float remainingDistance = delta.magnitude;
            Vector3 direction = remainingDistance > 0.0001f ? delta / remainingDistance : Vector3.zero;
            Vector3 avoidedDirection = ApplyObstacleAvoidance(direction, remainingDistance);
            if (state == EnemyState.Chase)
            {
                float avoidanceBlend = remainingDistance > 7.5f ? 0.58f : 0.28f;
                direction = remainingDistance <= 3.0f ? direction : Vector3.Slerp(direction, avoidedDirection, avoidanceBlend);
            }
            else
            {
                direction = avoidedDirection;
            }

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            }

            Vector3 movement = direction * speed;
            movement.y = -1.25f;
            CollisionFlags flags = SafeControllerMove(movement * Time.deltaTime);
            UpdateStuckRecovery(direction, speed, target, flags);
        }

        private Vector3 ApplyObstacleAvoidance(Vector3 direction, float remainingDistance)
        {
            if (direction.sqrMagnitude <= 0.0001f || controller == null)
            {
                return direction;
            }

            float probeDistance = Mathf.Clamp(remainingDistance + 0.2f, 0.85f, avoidanceProbeDistance);
            Vector3 origin = transform.position + Vector3.up * Mathf.Max(0.6f, controller.height * 0.45f);
            float radius = Mathf.Max(0.24f, controller.radius * 0.9f);

            float centerClearance = SampleClearance(origin, radius, direction, probeDistance);
            if (centerClearance >= probeDistance * 0.92f)
            {
                return direction;
            }

            Vector3 leftDir = Quaternion.AngleAxis(-avoidanceTurnAngle, Vector3.up) * direction;
            Vector3 rightDir = Quaternion.AngleAxis(avoidanceTurnAngle, Vector3.up) * direction;
            Vector3 wideLeftDir = Quaternion.AngleAxis(-wideAvoidanceTurnAngle, Vector3.up) * direction;
            Vector3 wideRightDir = Quaternion.AngleAxis(wideAvoidanceTurnAngle, Vector3.up) * direction;

            float leftClearance = SampleClearance(origin, radius, leftDir, probeDistance);
            float rightClearance = SampleClearance(origin, radius, rightDir, probeDistance);
            float wideLeftClearance = SampleClearance(origin, radius, wideLeftDir, probeDistance);
            float wideRightClearance = SampleClearance(origin, radius, wideRightDir, probeDistance);

            Vector3 chosen = direction;
            float bestScore = centerClearance * 0.75f;

            void TryCandidate(Vector3 candidate, float clearance, int sideSign, float weight)
            {
                float score = clearance * weight + (sideSign == preferredAvoidSide ? 0.14f : 0f) + (cornerBiasTimer > 0f && sideSign == preferredAvoidSide ? 0.12f : 0f);
                if (score > bestScore)
                {
                    bestScore = score;
                    chosen = candidate;
                }
            }

            TryCandidate(leftDir, leftClearance, -1, 1.00f);
            TryCandidate(rightDir, rightClearance, 1, 1.00f);
            TryCandidate(wideLeftDir, wideLeftClearance, -1, 0.92f);
            TryCandidate(wideRightDir, wideRightClearance, 1, 0.92f);

            preferredAvoidSide = Vector3.SignedAngle(direction, chosen, Vector3.up) < 0f ? -1 : 1;
            cornerBiasTimer = 0.28f;
            return Vector3.Normalize(direction * 0.22f + chosen * 0.78f);
        }

        private float SampleClearance(Vector3 origin, float radius, Vector3 direction, float probeDistance)
        {
            if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, probeDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != null && hit.collider.GetComponentInParent<PlayerMotor>() != null)
                {
                    return probeDistance;
                }

                return hit.distance;
            }

            return probeDistance;
        }

        private void UpdateStuckRecovery(Vector3 direction, float speed, Vector3 target, CollisionFlags flags)
        {
            Vector3 flatCurrent = Flatten(transform.position);
            float movedDistance = Vector3.Distance(flatCurrent, previousFlatPosition);
            bool sideBlocked = (flags & CollisionFlags.Sides) != 0;
            bool almostNotMoving = movedDistance <= Mathf.Max(0.01f, speed * Time.deltaTime * 0.12f);

            if (sideBlocked || almostNotMoving)
            {
                stuckTimer += Time.deltaTime;
            }
            else
            {
                stuckTimer = 0f;
            }

            if (stuckTimer >= stuckRecoverAfter && direction.sqrMagnitude > 0.001f)
            {
                Vector3 sideStep = Vector3.Cross(Vector3.up, direction).normalized * preferredAvoidSide;
                Vector3 recovery = (sideStep * speed * 1.1f) - (direction * speed * 0.25f) + Vector3.down * 1.1f;
                SafeControllerMove(recovery * Time.deltaTime);
            }

            if (stuckTimer >= hardRecoverAfter)
            {
                Vector3 side = Vector3.Cross(Vector3.up, direction).normalized * preferredAvoidSide;
                SafeControllerMove((side * speed * 1.75f + Vector3.down) * Time.deltaTime);

                if (state == EnemyState.Patrol && waypoints != null && waypoints.Length > 1)
                {
                    waypointIndex = (waypointIndex + 1) % waypoints.Length;
                }
                else if (state == EnemyState.Chase && hasLastKnownPosition)
                {
                    lastKnownPosition += side * 1.65f;
                    stateTimer = Mathf.Max(stateTimer, 0.85f);
                }
                else if (state == EnemyState.Investigate)
                {
                    investigatePosition = target + (side * 1.4f);
                    stateTimer = Mathf.Max(stateTimer, 0.6f);
                }

                preferredAvoidSide *= -1;
                cornerBiasTimer = 0.45f;
                stuckTimer = stuckRecoverAfter * 0.45f;
            }

            previousFlatPosition = flatCurrent;
        }

        private void ConfigureController()
        {
            if (controller == null)
            {
                controller = GetComponent<CharacterController>();
            }

            if (controller == null)
            {
                return;
            }

            bool wasEnabled = controller.enabled;
            if (wasEnabled)
            {
                controller.enabled = false;
            }

            transform.localScale = Vector3.one;
            controller.height = 1.9f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.95f, 0f);
            controller.skinWidth = 0.04f;
            controller.minMoveDistance = 0f;
            float maxStepOffset = Mathf.Max(0.05f, controller.height - controller.radius * 2f - 0.02f);
            controller.stepOffset = Mathf.Clamp(0.28f, 0.05f, maxStepOffset);
            controller.slopeLimit = 45f;

            controller.enabled = true;
        }

        private bool CanUseController()
        {
            if (controller == null)
            {
                controller = GetComponent<CharacterController>();
            }

            if (controller == null || !gameObject.activeInHierarchy)
            {
                return false;
            }

            if (!controller.enabled)
            {
                ConfigureController();
            }

            return controller != null && controller.enabled;
        }

        private CollisionFlags SafeControllerMove(Vector3 displacement)
        {
            if (!CanUseController())
            {
                transform.position += displacement;
                return CollisionFlags.None;
            }

            return controller.Move(displacement);
        }

        private bool TryHearActiveNoiseEvent()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null || !gameManager.HasActiveNoiseEvent)
            {
                return false;
            }

            if (gameManager.ActiveNoiseEventId == lastReactedNoiseEventId)
            {
                return false;
            }

            float distanceToNoise = Vector3.Distance(Flatten(transform.position), Flatten(gameManager.ActiveNoiseEventPosition));
            if (distanceToNoise > gameManager.ActiveNoiseEventRadius)
            {
                return false;
            }

            lastReactedNoiseEventId = gameManager.ActiveNoiseEventId;
            float signal = 1f - Mathf.Clamp01(distanceToNoise / Mathf.Max(0.01f, gameManager.ActiveNoiseEventRadius));
            lastKnownPosition = gameManager.ActiveNoiseEventPosition;
            hasLastKnownPosition = true;

            string noiseLabel = (gameManager.ActiveNoiseEventLabel ?? string.Empty).ToLowerInvariant();
            bool isRepairOrEngine = noiseLabel.Contains("sua xe") || noiseLabel.Contains("dong co");
            bool isDoor = noiseLabel.Contains("cua");
            bool isWaterStep = !isRepairOrEngine && noiseLabel.Contains("nuoc");
            bool isWoodStep = !isRepairOrEngine && noiseLabel.Contains("san go");
            bool isGrassStep = !isRepairOrEngine && (noiseLabel.Contains("co ram") || noiseLabel.Contains("khom") || noiseLabel.Contains("di ") || noiseLabel.Contains("chay "));

            if (isWaterStep || isGrassStep || isWoodStep)
            {
                bool isRunningStep = noiseLabel.Contains("chay");
                heardStepCount = Mathf.Clamp(heardStepCount + 1, 1, 5);
                heardStepTimer = isWaterStep ? 0.80f : 1.25f;

                float awarenessGain = isWaterStep
                    ? (0.025f + signal * 0.07f)
                    : isWoodStep
                        ? (0.16f + signal * 0.22f)
                        : (0.12f + signal * 0.18f);
                if (isRunningStep)
                {
                    awarenessGain *= 1.30f;
                }

                awareness = Mathf.Clamp01(awareness + awarenessGain);

                bool closeEnoughToCommit = signal >= (isWaterStep ? 0.97f : hearingChaseSignal + (isRunningStep ? 0.08f : 0.18f));
                bool repeatedSteps = heardStepCount >= (isWaterStep ? 5 : (isRunningStep ? 1 : 2));
                if (closeEnoughToCommit && repeatedSteps && awareness >= 0.22f)
                {
                    BeginChase(lastKnownPosition);
                    return true;
                }

                if (state == EnemyState.Chase)
                {
                    stateTimer = Mathf.Max(stateTimer, chaseMemory * 0.78f);
                    return true;
                }

                BeginInvestigate(lastKnownPosition, investigatePause + Mathf.Lerp(0.45f, 1.25f, signal), 0.20f);
                return true;
            }

            if (isDoor)
            {
                awareness = Mathf.Clamp01(awareness + 0.34f + signal * 0.34f);
                if (signal >= 0.05f)
                {
                    BeginChase(lastKnownPosition);
                    return true;
                }
            }
            else if (isRepairOrEngine)
            {
                awareness = Mathf.Clamp01(awareness + 0.34f + signal * 0.30f);
                BeginChase(lastKnownPosition);
                return true;
            }

            if (state == EnemyState.Chase)
            {
                stateTimer = Mathf.Max(stateTimer, chaseMemory * 0.75f);
                return true;
            }

            awareness = Mathf.Clamp01(awareness + 0.14f + signal * 0.18f);
            BeginInvestigate(lastKnownPosition, investigatePause + Mathf.Lerp(0.45f, 1.15f, signal), 0.24f);
            return true;
        }

        private bool CanSeePlayer(PlayerMotor player, out float sightFactor)
        {
            sightFactor = 0f;
            if (player == null || player.IsHidden)
            {
                return false;
            }

            Vector3 origin = transform.position + Vector3.up * 1.55f;
            Vector3 target = player.EyeTransform.position;
            Vector3 toPlayer = target - origin;
            float distance = toPlayer.magnitude;
            float effectiveDistance = Mathf.Max(0.75f, viewDistance * player.VisibilityFactor);
            if (distance > effectiveDistance)
            {
                return false;
            }

            Vector3 flatForward = Flatten(transform.forward);
            Vector3 flatToPlayer = Flatten(toPlayer).normalized;
            float angle = Vector3.Angle(flatForward, flatToPlayer);
            if (angle > viewAngle * 0.5f)
            {
                return false;
            }

            if (Physics.Raycast(origin, toPlayer.normalized, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                bool visible = hit.collider.GetComponentInParent<PlayerMotor>() != null;
                if (visible)
                {
                    float angleFactor = 1f - Mathf.InverseLerp(0f, viewAngle * 0.5f, angle);
                    float distanceFactor = 1f - Mathf.Clamp01(distance / effectiveDistance);
                    sightFactor = Mathf.Clamp01(0.35f + angleFactor * 0.35f + distanceFactor * 0.30f);
                }
                return visible;
            }

            return false;
        }

        private bool CanCatchPlayer(PlayerMotor player)
        {
            if (player == null)
            {
                return false;
            }

            Vector3 origin = transform.position + Vector3.up * 1.15f;
            Vector3 target = player.EyeTransform.position;
            Vector3 toPlayer = target - origin;
            float distance = toPlayer.magnitude;
            if (distance <= 0.01f)
            {
                return true;
            }

            if (Physics.Raycast(origin, toPlayer.normalized, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.collider.GetComponentInParent<PlayerMotor>() != null;
            }

            return false;
        }

        private void UpdateCapture(PlayerMotor player, float distanceToPlayer, bool seesPlayer)
        {
            if (GameManager.Instance == null || player == null)
            {
                captureTimer = 0f;
                return;
            }

            if (player.IsHidden)
            {
                captureTimer = 0f;
                TryResolveHiddenPlayer(player, distanceToPlayer);
                return;
            }

            bool overlapCatch = ControllersOverlap(player);
            bool closeCatch = distanceToPlayer <= catchDistance;
            bool contactCatch = distanceToPlayer <= catchDistance * 0.78f;
            bool clearCatch = seesPlayer || CanCatchPlayer(player);
            bool dangerousState = state == EnemyState.Chase || awareness >= 0.82f || (state == EnemyState.Investigate && distanceToPlayer <= catchDistance * 0.9f);

            if (dangerousState && (overlapCatch || contactCatch || (closeCatch && clearCatch)))
            {
                captureTimer += Time.deltaTime;
                if (captureTimer >= captureHoldTime)
                {
                    TriggerCaptureAnimation();
                    GameManager.Instance.PlayerCaught(enemyName);
                }
            }
            else
            {
                captureTimer = 0f;
            }
        }

        private void TryResolveHiddenPlayer(PlayerMotor player, float distanceToPlayer)
        {
            HideSpotInteractable hideSpot = player.CurrentHideSpot;
            if (hideSpot == null)
            {
                return;
            }

            if (player.HideOutcomeSucceeded)
            {
                captureTimer = 0f;
                awareness = Mathf.MoveTowards(awareness, 0.08f, Time.deltaTime * 2.0f);
                hasLastKnownPosition = false;
                heardStepCount = 0;
                heardStepTimer = 0f;
                if (state != EnemyState.Patrol)
                {
                    AdvancePatrol();
                    stateTimer = 0f;
                    SetState(EnemyState.Patrol);
                }
                return;
            }

            float reactionDistance = Mathf.Max(catchDistance, hideSpot.captureCheckDistance);
            if (distanceToPlayer > reactionDistance)
            {
                captureTimer = 0f;
                return;
            }

            bool clearProbe = CanProbeHiddenSpot(hideSpot, player, distanceToPlayer);
            if (!clearProbe)
            {
                captureTimer = 0f;
                if (state != EnemyState.Chase)
                {
                    BeginInvestigate(hideSpot.ExitAnchor.position, investigatePause + 0.45f, 0.34f);
                }
                return;
            }

            if (!player.CanRollHideOutcome)
            {
                captureTimer = 0f;
                if (state != EnemyState.Chase)
                {
                    BeginInvestigate(hideSpot.ExitAnchor.position, investigatePause + 0.55f, 0.42f);
                }
                return;
            }

            if (Random.value <= hideSpot.safeEscapeChance)
            {
                player.ResolveHideOutcome(true);
                awareness = Mathf.MoveTowards(awareness, 0.08f, 0.65f);
                hasLastKnownPosition = false;
                heardStepCount = 0;
                heardStepTimer = 0f;
                captureTimer = 0f;
                chaseAfterglowTimer = 0f;
                AdvancePatrol();
                SetState(EnemyState.Patrol);
                stateTimer = 0f;
                return;
            }

            player.ResolveHideOutcome(false);
            GameManager.Instance.PlayerCaught(enemyName);
        }

        private bool CanProbeHiddenSpot(HideSpotInteractable hideSpot, PlayerMotor player, float distanceToPlayer)
        {
            if (hideSpot == null || player == null)
            {
                return false;
            }

            if (distanceToPlayer <= 0.75f)
            {
                return true;
            }

            Vector3 origin = transform.position + Vector3.up * 1.25f;
            Vector3 exitTarget = hideSpot.ExitAnchor.position + Vector3.up * 0.55f;
            if (!HasLineToTarget(origin, exitTarget, hideSpot, player, false))
            {
                return false;
            }

            if (distanceToPlayer <= 1.25f)
            {
                return true;
            }

            Vector3 hideTarget = hideSpot.HideAnchor.position + Vector3.up * 0.55f;
            return HasLineToTarget(origin, hideTarget, hideSpot, player, true);
        }

        private bool HasLineToTarget(Vector3 origin, Vector3 target, HideSpotInteractable hideSpot, PlayerMotor player, bool allowDirectPlayerHitOnly)
        {
            Vector3 toTarget = target - origin;
            float distance = toTarget.magnitude;
            if (distance <= 0.01f)
            {
                return true;
            }

            if (Physics.Raycast(origin, toTarget.normalized, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null)
                {
                    return false;
                }

                Transform hitTransform = hit.collider.transform;
                if (hitTransform.GetComponentInParent<PlayerMotor>() != null)
                {
                    return true;
                }

                if (!allowDirectPlayerHitOnly && hideSpot != null && (hitTransform == hideSpot.ExitAnchor || hitTransform.IsChildOf(hideSpot.ExitAnchor)))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        private bool ControllersOverlap(PlayerMotor player)
        {
            CharacterController playerController = player != null ? player.GetComponent<CharacterController>() : null;
            if (playerController == null || controller == null)
            {
                return false;
            }

            Bounds enemyBounds = controller.bounds;
            Bounds playerBounds = playerController.bounds;
            enemyBounds.Expand(0.08f);
            playerBounds.Expand(0.08f);
            return enemyBounds.Intersects(playerBounds);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (GameManager.Instance == null || GameManager.Instance.IsEnded || GameManager.Instance.EscapeSequenceActive || hit == null)
            {
                return;
            }

            PlayerMotor player = hit.collider != null ? hit.collider.GetComponentInParent<PlayerMotor>() : null;
            if (player == null || player.IsHidden)
            {
                return;
            }

            if (state == EnemyState.Chase || awareness >= 0.9f)
            {
                GameManager.Instance.PlayerCaught(enemyName);
            }
        }

        private void ReportThreat(PlayerMotor player, float distance, bool seesPlayer)
        {
            if (GameManager.Instance == null || player == null || player.IsHidden)
            {
                return;
            }

            float normalizedDistance = Mathf.Clamp01(1f - (distance / Mathf.Max(0.01f, viewDistance + 2f)));
            float threat = 0f;
            string label = "YÊN LẶNG";

            switch (state)
            {
                case EnemyState.Patrol:
                    if (distance < 5.4f || awareness > 0.18f)
                    {
                        threat = 0.12f + normalizedDistance * 0.24f + awareness * 0.18f;
                        label = awareness > 0.28f ? "NÓ ĐANG NGHE" : "GẦN ĐÂY";
                    }
                    break;
                case EnemyState.Investigate:
                    threat = 0.44f + normalizedDistance * 0.24f + awareness * 0.16f;
                    label = chaseAfterglowTimer > 0f ? "VẪN ĐANG BỊ DÍ" : "NÓ ĐANG LẦN THEO";
                    break;
                case EnemyState.Chase:
                    threat = 0.80f + normalizedDistance * 0.18f + (seesPlayer ? 0.04f : 0f);
                    label = "NÓ ĐANG DÍ SÁT";
                    break;
            }

            if (chaseAfterglowTimer > 0f)
            {
                threat = Mathf.Max(threat, 0.52f + normalizedDistance * 0.10f);
                if (label == "YÊN LẶNG")
                {
                    label = "VẪN ĐANG BỊ DÍ";
                }
            }

            if (threat > 0f)
            {
                GameManager.Instance.ReportThreat(threat, label);
            }
        }

        private void RefreshColor()
        {
            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (bodyRenderer != null)
            {
                switch (state)
                {
                    case EnemyState.Patrol:
                        UnityCompatibility.SetRendererColor(bodyRenderer, new Color(0.70f, 0.76f, 0.82f));
                        break;
                    case EnemyState.Investigate:
                        UnityCompatibility.SetRendererColor(bodyRenderer, new Color(0.82f, 0.78f, 0.68f));
                        break;
                    case EnemyState.Chase:
                        UnityCompatibility.SetRendererColor(bodyRenderer, new Color(0.92f, 0.30f, 0.36f));
                        break;
                }
            }

            if (eyeLight != null)
            {
                switch (state)
                {
                    case EnemyState.Patrol:
                        eyeLight.color = new Color(0.85f, 0.18f, 0.26f);
                        eyeLight.intensity = 0.50f;
                        break;
                    case EnemyState.Investigate:
                        eyeLight.color = new Color(1.0f, 0.54f, 0.20f);
                        eyeLight.intensity = 0.85f;
                        break;
                    case EnemyState.Chase:
                        eyeLight.color = new Color(1.0f, 0.18f, 0.20f);
                        eyeLight.intensity = 1.45f;
                        break;
                }
            }
        }

        private void ResolveAuthoringReferences()
        {
            if (authoring == null)
            {
                authoring = GetComponent<GhostAuthoring>();
            }

            if (authoring != null)
            {
                authoring.ResolveMissingReferences();
                if (bodyRenderer == null && authoring.PrimaryRenderer != null)
                {
                    bodyRenderer = authoring.PrimaryRenderer;
                }

                if (modelAnimator == null && authoring.Animator != null)
                {
                    modelAnimator = authoring.Animator;
                }
            }
        }

        private Vector3[] BuildFallbackPatrolRoute(Vector3 origin)
        {
            float radius = 6f;
            if (authoring != null)
            {
                radius = Mathf.Max(2.5f, authoring.DefaultRouteRadius);
            }

            return new[]
            {
                origin + new Vector3(radius, 0f, -radius * 0.35f),
                origin + new Vector3(radius * 0.35f, 0f, radius),
                origin + new Vector3(-radius, 0f, radius * 0.42f),
                origin + new Vector3(-radius * 0.28f, 0f, -radius)
            };
        }

        private void UpdateAnimator()
        {
            if (modelAnimator == null)
            {
                return;
            }

            float normalizedSpeed = 0f;
            float referenceSpeed = Mathf.Max(0.01f, chaseSpeed);
            if (animationMoveSpeed > 0.01f)
            {
                normalizedSpeed = Mathf.Clamp01(animationMoveSpeed / referenceSpeed);
            }

            if (!string.IsNullOrEmpty(animatorSpeedParameter))
            {
                modelAnimator.SetFloat(animatorSpeedParameter, normalizedSpeed);
            }

            if (!string.IsNullOrEmpty(animatorMovingParameter))
            {
                modelAnimator.SetBool(animatorMovingParameter, animationMoveSpeed > 0.05f);
            }

            if (!string.IsNullOrEmpty(animatorStateParameter))
            {
                modelAnimator.SetInteger(animatorStateParameter, (int)state);
            }

            if (!string.IsNullOrEmpty(animatorAlertnessParameter))
            {
                modelAnimator.SetFloat(animatorAlertnessParameter, awareness);
            }
        }

        private void TriggerCaptureAnimation()
        {
            if (modelAnimator == null || string.IsNullOrEmpty(animatorCaptureTrigger))
            {
                return;
            }

            modelAnimator.ResetTrigger(animatorCaptureTrigger);
            modelAnimator.SetTrigger(animatorCaptureTrigger);
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
