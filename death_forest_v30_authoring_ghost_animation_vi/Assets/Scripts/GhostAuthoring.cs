using System.Collections.Generic;
using UnityEngine;

namespace HollowManor
{
    [ExecuteAlways]
    public sealed class GhostAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer primaryRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform patrolRouteRoot;
        [SerializeField] private Transform[] patrolPoints = new Transform[0];
        [SerializeField] private float defaultRouteRadius = 7f;

        public Transform VisualRoot => visualRoot;
        public Renderer PrimaryRenderer => primaryRenderer;
        public Animator Animator => animator;
        public Transform PatrolRouteRoot => patrolRouteRoot;
        public Transform[] PatrolPoints => patrolPoints ?? new Transform[0];
        public float DefaultRouteRadius => defaultRouteRadius;

        private void Awake()
        {
            ResolveMissingReferences();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveMissingReferences();
        }
#endif

        public void ResolveMissingReferences()
        {
            if (visualRoot == null)
            {
                visualRoot = transform.Find("ModelRoot");
            }

            if (patrolRouteRoot == null)
            {
                patrolRouteRoot = transform.Find("PatrolRoute");
            }

            if (visualRoot != null)
            {
                if (primaryRenderer == null)
                {
                    primaryRenderer = visualRoot.GetComponentInChildren<Renderer>(true);
                }

                if (animator == null)
                {
                    animator = visualRoot.GetComponentInChildren<Animator>(true);
                }
            }

            RefreshPatrolPointArray();
        }

        public void RefreshPatrolPointArray()
        {
            if (patrolRouteRoot == null)
            {
                patrolPoints = new Transform[0];
                return;
            }

            List<Transform> points = new List<Transform>();
            for (int i = 0; i < patrolRouteRoot.childCount; i++)
            {
                Transform child = patrolRouteRoot.GetChild(i);
                if (child != null)
                {
                    points.Add(child);
                }
            }

            patrolPoints = points.ToArray();
        }

        public void EnsureRouteChildren(Vector3[] worldPoints)
        {
            if (patrolRouteRoot == null)
            {
                patrolRouteRoot = new GameObject("PatrolRoute").transform;
                patrolRouteRoot.SetParent(transform, false);
            }

            while (patrolRouteRoot.childCount > 0)
            {
                Transform child = patrolRouteRoot.GetChild(patrolRouteRoot.childCount - 1);
                if (child != null)
                {
                    UnityCompatibility.DestroyObject(child.gameObject);
                }
            }

            if (worldPoints == null || worldPoints.Length == 0)
            {
                worldPoints = BuildDefaultWorldRoute();
            }

            for (int i = 0; i < worldPoints.Length; i++)
            {
                Transform point = new GameObject("Point_" + (i + 1).ToString("00")).transform;
                point.SetParent(patrolRouteRoot, false);
                point.position = worldPoints[i];
            }

            RefreshPatrolPointArray();
        }

        public Vector3[] BuildWaypointPositions(Vector3 fallbackOrigin)
        {
            ResolveMissingReferences();

            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                List<Vector3> positions = new List<Vector3>();
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    Transform point = patrolPoints[i];
                    if (point != null)
                    {
                        positions.Add(point.position);
                    }
                }

                if (positions.Count > 0)
                {
                    return positions.ToArray();
                }
            }

            return BuildDefaultWorldRoute(fallbackOrigin);
        }

        public Vector3[] BuildDefaultWorldRoute()
        {
            return BuildDefaultWorldRoute(transform.position);
        }

        public Vector3[] BuildDefaultWorldRoute(Vector3 origin)
        {
            float radius = Mathf.Max(2.5f, defaultRouteRadius);
            return new[]
            {
                origin + new Vector3(radius, 0f, -radius * 0.35f),
                origin + new Vector3(radius * 0.45f, 0f, radius),
                origin + new Vector3(-radius, 0f, radius * 0.40f),
                origin + new Vector3(-radius * 0.30f, 0f, -radius)
            };
        }
    }
}
