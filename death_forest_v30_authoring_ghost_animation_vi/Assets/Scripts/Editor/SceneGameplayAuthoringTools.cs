#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HollowManor.EditorTools
{
    public static class SceneGameplayAuthoringTools
    {
        [MenuItem("Death Forest/Gameplay Authoring/Refresh All Pickup Spawn Points")]
        public static void RefreshAllPickupSpawnPointsMenu()
        {
            int total = RefreshAllPickupSpawnPoints();
            EditorUtility.DisplayDialog("Death Forest", "Da refresh " + total + " diem dat vat pham trong scene hien tai.", "OK");
        }

        [MenuItem("Death Forest/Gameplay Authoring/Refresh Selected Pickup Spawn Point")]
        public static void RefreshSelectedPickupSpawnPointMenu()
        {
            ItemSpawnPoint point = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<ItemSpawnPoint>() : null;
            if (point == null)
            {
                EditorUtility.DisplayDialog("Death Forest", "Hay chon mot ItemSpawnPoint trong Hierarchy truoc.", "OK");
                return;
            }

            point.RefreshSpawnedPickup(true);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Death Forest", "Da refresh pickup cho spawn point da chon.", "OK");
        }

        [MenuItem("Death Forest/Gameplay Authoring/Refresh All Ghost Authoring")]
        public static void RefreshAllGhostAuthoringMenu()
        {
            int total = RefreshAllGhostAuthoring();
            EditorUtility.DisplayDialog("Death Forest", "Da refresh " + total + " ma va patrol route trong scene hien tai.", "OK");
        }

        [MenuItem("Death Forest/Gameplay Authoring/Refresh Selected Ghost Authoring")]
        public static void RefreshSelectedGhostAuthoringMenu()
        {
            PatrolEnemy ghost = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponentInParent<PatrolEnemy>() : null;
            if (ghost == null)
            {
                EditorUtility.DisplayDialog("Death Forest", "Hay chon root cua ma trong Hierarchy truoc.", "OK");
                return;
            }

            GhostAuthoring authoring = ghost.GetComponent<GhostAuthoring>();
            if (authoring == null)
            {
                authoring = ghost.gameObject.AddComponent<GhostAuthoring>();
            }

            authoring.ResolveMissingReferences();
            authoring.EnsureRouteChildren(authoring.BuildWaypointPositions(ghost.transform.position));
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Death Forest", "Da refresh ghost authoring cho ma da chon.", "OK");
        }

        [MenuItem("Death Forest/Gameplay Authoring/Rebuild Ending Point Anchors")]
        public static void RebuildEndingPointAnchorsMenu()
        {
            int total = EnsureEndingAnchors();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Death Forest", "Da kiem tra va tao " + total + " cum ending point trong scene hien tai.", "OK");
        }

        public static int RefreshAllPickupSpawnPoints()
        {
            ItemSpawnPoint[] points = UnityEngine.Object.FindObjectsByType<ItemSpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int refreshed = 0;
            foreach (ItemSpawnPoint point in points)
            {
                if (point == null || point.GetComponentInParent<DeathForestSceneRoot>() == null)
                {
                    continue;
                }

                point.RefreshSpawnedPickup(true);
                refreshed++;
            }

            if (refreshed > 0)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            return refreshed;
        }

        public static int RefreshAllGhostAuthoring()
        {
            PatrolEnemy[] ghosts = UnityEngine.Object.FindObjectsByType<PatrolEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int refreshed = 0;
            foreach (PatrolEnemy ghost in ghosts)
            {
                if (ghost == null || ghost.GetComponentInParent<DeathForestSceneRoot>() == null)
                {
                    continue;
                }

                GhostAuthoring authoring = ghost.GetComponent<GhostAuthoring>();
                if (authoring == null)
                {
                    authoring = ghost.gameObject.AddComponent<GhostAuthoring>();
                }

                authoring.ResolveMissingReferences();
                authoring.EnsureRouteChildren(authoring.BuildWaypointPositions(ghost.transform.position));
                EditorUtility.SetDirty(ghost.gameObject);
                refreshed++;
            }

            if (refreshed > 0)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            return refreshed;
        }

        public static int EnsureEndingAnchors()
        {
            EndingSequencePoints[] allSets = UnityEngine.Object.FindObjectsByType<EndingSequencePoints>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = 0;
            foreach (EndingSequencePoints set in allSets)
            {
                if (set == null || set.GetComponentInParent<DeathForestSceneRoot>() == null)
                {
                    continue;
                }

                LevelFactory.EnsureEndingSequenceChildren(set.transform);
                EditorUtility.SetDirty(set.gameObject);
                count++;
            }

            return count;
        }
    }
}
#endif
