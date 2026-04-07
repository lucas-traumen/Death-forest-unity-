using UnityEngine;

namespace HollowManor
{
    public sealed class EndingSequencePoints : MonoBehaviour
    {
        [SerializeField] private Transform interactPoint;
        [SerializeField] private Transform doorApproachPoint;
        [SerializeField] private Transform seatPoint;
        [SerializeField] private Transform seatFacingPoint;
        [SerializeField] private Transform carAlignPoint;
        [SerializeField] private Transform vehicleRoot;
        [SerializeField] private Transform exitPathStart;
        [SerializeField] private Transform exitPathEnd;
        [SerializeField] private Transform endingCameraPoint;
        [SerializeField] private Transform ghostRevealPoint;
        [SerializeField] private bool moveVehicleDuringOutro = true;
        [SerializeField] private float driveAwayDuration = 2.4f;

        public Transform InteractPoint => interactPoint;
        public Transform DoorApproachPoint => doorApproachPoint != null ? doorApproachPoint : interactPoint;
        public Transform SeatPoint => seatPoint != null ? seatPoint : interactPoint;
        public Transform SeatFacingPoint => seatFacingPoint != null ? seatFacingPoint : seatPoint;
        public Transform CarAlignPoint => carAlignPoint != null ? carAlignPoint : interactPoint;
        public Transform VehicleRoot => ResolveVehicleRoot();
        public Transform ExitPathStart => exitPathStart != null ? exitPathStart : CarAlignPoint;
        public Transform ExitPathEnd => exitPathEnd != null ? exitPathEnd : ExitPathStart;
        public Transform EndingCameraPoint => endingCameraPoint;
        public Transform GhostRevealPoint => ghostRevealPoint;
        public bool MoveVehicleDuringOutro => moveVehicleDuringOutro;
        public float DriveAwayDuration => Mathf.Max(0.8f, driveAwayDuration);

        public Vector3 DoorApproachPosition => DoorApproachPoint != null ? DoorApproachPoint.position : transform.position;
        public Vector3 SeatPosition => SeatPoint != null ? SeatPoint.position : transform.position;
        public Quaternion SeatRotation
        {
            get
            {
                Transform facing = SeatFacingPoint;
                if (facing != null)
                {
                    Vector3 forward = facing.forward;
                    if (forward.sqrMagnitude > 0.0001f)
                    {
                        return Quaternion.LookRotation(forward, Vector3.up);
                    }
                }

                return transform.rotation;
            }
        }

        private void Awake()
        {
            ResolveMissingPoints();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveMissingPoints();
        }
#endif

        public Transform ResolveVehicleRoot()
        {
            if (vehicleRoot != null)
            {
                return vehicleRoot;
            }

            DeathForestSceneRoot sceneRoot = GetComponentInParent<DeathForestSceneRoot>();
            Transform searchRoot = sceneRoot != null && sceneRoot.PropRoot != null ? sceneRoot.PropRoot : transform.root;
            if (searchRoot == null)
            {
                return null;
            }

            Renderer[] renderers = searchRoot.GetComponentsInChildren<Renderer>(true);
            float bestDistance = float.MaxValue;
            Transform best = null;
            Vector3 origin = CarAlignPoint != null ? CarAlignPoint.position : transform.position;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                string lowered = renderer.transform.root.name.ToLowerInvariant();
                string selfLowered = renderer.transform.name.ToLowerInvariant();
                if (!lowered.Contains("car") && !selfLowered.Contains("car") && !lowered.Contains("xe") && !selfLowered.Contains("xe"))
                {
                    continue;
                }

                float distance = Vector3.Distance(origin, renderer.bounds.center);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = renderer.transform.root;
                }
            }

            vehicleRoot = best;
            return vehicleRoot;
        }

        public void ResolveMissingPoints()
        {
            if (interactPoint == null) interactPoint = transform.Find("InteractPoint");
            if (doorApproachPoint == null) doorApproachPoint = transform.Find("DoorApproachPoint");
            if (seatPoint == null) seatPoint = transform.Find("SeatPoint");
            if (seatFacingPoint == null) seatFacingPoint = transform.Find("SeatFacingPoint");
            if (carAlignPoint == null) carAlignPoint = transform.Find("CarAlignPoint");
            if (vehicleRoot == null) vehicleRoot = transform.Find("VehicleRoot");
            if (exitPathStart == null) exitPathStart = transform.Find("ExitPathStart");
            if (exitPathEnd == null) exitPathEnd = transform.Find("ExitPathEnd");
            if (endingCameraPoint == null) endingCameraPoint = transform.Find("EndingCameraPoint");
            if (ghostRevealPoint == null) ghostRevealPoint = transform.Find("GhostRevealPoint");
        }

        public static EndingSequencePoints FindForTransform(Transform current)
        {
            if (current == null)
            {
                return null;
            }

            EndingSequencePoints direct = current.GetComponent<EndingSequencePoints>();
            if (direct != null)
            {
                return direct;
            }

            return current.GetComponentInParent<EndingSequencePoints>();
        }
    }
}
